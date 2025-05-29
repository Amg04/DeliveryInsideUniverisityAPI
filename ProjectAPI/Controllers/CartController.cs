using BAL.interfaces;
using BLLProject.Specifications;
using DAL;
using DAL.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using ProjectAPI.DTO.CartDTOs;
using Stripe.Checkout;
using System.Security.Claims;
using Utilities;

namespace ProjectAPI.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class CartController : ControllerBase
    {

        private readonly IUnitOfWork unitOfWork;
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly UserManager<User> userManager;
        private readonly Stripedata stripeData;
        private readonly StripeSettings stripeSettings;

        public CartController(IUnitOfWork unitOfWork,
            IOptions<Stripedata> stripeOptions,
            IOptions<StripeSettings> stripeSettings,
             RoleManager<IdentityRole> roleManager,
            UserManager<User> userManager)
        {

            this.unitOfWork = unitOfWork;
            this.roleManager = roleManager;
            this.userManager = userManager;
            this.stripeData = stripeOptions.Value;
            this.stripeSettings = stripeSettings.Value;
        }


        [HttpGet("Cart")]
        public IActionResult Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var spec = new BaseSpecification<ShoppingCart>(u => u.UserId == userId);
            spec.Includes.Add(c => c.Product);
            var shoppingCart = unitOfWork.Repository<ShoppingCart>().GetAllWithSpec(spec).ToList();

            decimal totalCarts = shoppingCart.Sum(item => item.Count * item.Product.price);

            return Ok(new
            {
                CartsList = shoppingCart,
                TotalCarts = totalCarts
            });
        }

        [HttpGet("GetSummary")]
        public async Task<IActionResult> SummaryAsync()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var spec = new BaseSpecification<ShoppingCart>(u => u.UserId == userId);
            spec.Includes.Add(c => c.Product);
            var shoppingCarts = unitOfWork.Repository<ShoppingCart>().GetAllWithSpec(spec);

            var user = await userManager.FindByIdAsync(userId);

            var Order = new Order();
            Order.UserId = userId;
            Order.Name = user.Name;
            Order.Address = user.Address;
            Order.PhoneNumber = user.PhoneNumber;
            var OrderDTO = Order.ToOrderDTO();
            var cartDTO = new CartDTO()
            {
                CartsList = shoppingCarts,
                order = OrderDTO
            };

            foreach (var item in cartDTO.CartsList)
            {
                cartDTO.order.TotalPrice += (item.Count * item.Product.price);
                cartDTO.TotalCarts += (item.Count * item.Product.price);
            }
            return Ok(cartDTO);
        }

        [HttpGet("GetPaymentOptions")]
        public IActionResult GetPaymentOptions()
        {
            var paymentOptions = new List<string> { "online", "cash" };
            return Ok(paymentOptions);
        }

        [HttpPost("SubmitPaymentMethod")]
        public async Task<IActionResult> SubmitPaymentMethodAsync([FromBody] PaymentOrderDTO paymentDto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var spec = new BaseSpecification<ShoppingCart>(u => u.UserId == userId);
            spec.Includes.Add(c => c.Product);
            var cartItems = unitOfWork.Repository<ShoppingCart>().GetAllWithSpec(spec).ToList();
            if (!cartItems.Any())
                return BadRequest("Shopping Cart is Empty");

            var user = await userManager.FindByIdAsync(userId);

            // if order is exist 
            var existingOrder = unitOfWork.Repository<Order>()
                .GetEntityWithSpec(new BaseSpecification<Order>(x => x.UserId == userId && x.PaymentStatus == PaymentStatus.Pending));

            Order order;
            if (existingOrder != null)
            {
                existingOrder.Address = paymentDto.Address;
                existingOrder.Name = paymentDto.Name;
                existingOrder.PhoneNumber = paymentDto.PhoneNumber;
                existingOrder.OrderDate = DateTime.Now;
                existingOrder.TotalPrice = cartItems.Sum(c => c.Count * c.Product.price);

                unitOfWork.Repository<Order>().Update(existingOrder);

                // delete orderItem associated with the order (if exist)
                var oldOrderspec = new BaseSpecification<OrderItem>(x => x.OrderId == existingOrder.id);
                var oldOrderDetails = unitOfWork.Repository<OrderItem>().GetAllWithSpec(oldOrderspec);
              
                unitOfWork.OrderItemRepository.RemoveRange(oldOrderDetails);
                order = existingOrder;
            }
            else
            {
                var establishmentId = cartItems.FirstOrDefault()?.Product?.EstablishmentId;
                if (establishmentId == null)
                    return BadRequest("Establishment not found for cart items.");
                order = new Order
                {
                    UserId = userId,
                    Name = paymentDto.Name,
                    EstablishmentId = (int)establishmentId,
                    Address = paymentDto.Address,
                    PhoneNumber = paymentDto.PhoneNumber,
                    OrderDate = DateTime.Now,
                    PaymentStatus = paymentDto.PaymentMethod.ToLower() == "online" ? PaymentStatus.Pending : PaymentStatus.CashOnDelivery,
                    OrderStatus = OrderStatus.Pending,
                    TotalPrice = cartItems.Sum(c => c.Count * c.Product.price)
                };
                
                unitOfWork.Repository<Order>().Add(order);
            }
            unitOfWork.Complete();
            foreach (var item in cartItems)
            {
                OrderItem orderItem = new OrderItem()
                {
                    ProductId = item.ProductId,
                    OrderId = order.id,
                    SubTotal = Math.Max(0, item.Product.price * item.Count),
                    Quantity = Math.Max(1, item.Count)
                };

                unitOfWork.Repository<OrderItem>().Add(orderItem);
            }
            unitOfWork.Complete();

            if (paymentDto.PaymentMethod.ToLower() == "online")
            {
                var stripeUrl = CreateStripeCheckout(order);  // go to stripe checkout
                return Ok(new
                {
                    message = "Payment Follow-Up via Stripe",
                    paymentType = "online",
                    redirectUrl = stripeUrl
                });
            }
            else if (paymentDto.PaymentMethod.ToLower() == "cash")
            {
                unitOfWork.ShoppingCartRepository.RemoveRange(cartItems);
                unitOfWork.Complete();

                return Ok(new
                {
                    message = "Order confirmed and payment upon receipt",
                    paymentType = "cash",
                    orderId = order.id
                });
            }

            return BadRequest("Incorrect Payment method or not supported yet");
        }

        private string CreateStripeCheckout(Order order)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var spec = new BaseSpecification<ShoppingCart>(u => u.UserId == userId);
            spec.Includes.Add(c => c.Product);
            var cartItems = unitOfWork.Repository<ShoppingCart>().GetAllWithSpec(spec).ToList();
            var domain = stripeSettings.Domain;
            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string> { "card" },
                LineItems = new List<SessionLineItemOptions>(),

                Mode = "payment",
                SuccessUrl = domain + $"api/Cart/OrderConfirmation?id={order.id}",
                CancelUrl = domain + $"api/Cart/PaymentCancelled",
            };

            foreach (var item in cartItems)
            {
                var sessionLineOption = new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        UnitAmount = (long)(item.Product.price * 100),
                        Currency = "usd",
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = item.Product.Name,
                        }
                    },
                    Quantity = item.Count,
                };
                options.LineItems.Add(sessionLineOption);
            }

            var payment = new Payment()
            {
                OrderId = order.id,
                PaymentTime = DateTime.Now,
            };

            var client = new Stripe.StripeClient(stripeData.SecretKey);
            var service = new SessionService(client);
            Session session = service.Create(options);
            payment.SessionId = session.Id;

            unitOfWork.Repository<Payment>().Add(payment);
            unitOfWork.Complete();

            return session.Url;
        }

        [HttpGet("OrderConfirmation")]
        [AllowAnonymous]
        public IActionResult OrderConfirmation(int id)
        {
            var order = unitOfWork.Repository<Order>().Get(id);
            var spec = new BaseSpecification<Payment>(p => p.OrderId == id);
            var paymentMethod = unitOfWork.Repository<Payment>().GetEntityWithSpec(spec);
           
            var client = new Stripe.StripeClient(stripeData.SecretKey);
            var service = new SessionService(client);
            Session session = service.Get(paymentMethod.SessionId);

            if (session.PaymentStatus.ToLower() == "paid")
            {
                unitOfWork.OrderRepository.UpdatePaymentStatus(id, PaymentStatus.Approved);
                paymentMethod.PaymentIntentId = session.PaymentIntentId;
                unitOfWork.Complete();

                var ShoppingCartSpec = new BaseSpecification<ShoppingCart>(u => u.UserId == order.UserId);
                var shoppingCarts = unitOfWork.Repository<ShoppingCart>().GetAllWithSpec(ShoppingCartSpec);

                unitOfWork.ShoppingCartRepository.RemoveRange(shoppingCarts);
                unitOfWork.Complete();

                return Ok(new
                {
                    message = "Payment successful and order confirmed.",
                    orderId = order.id,
                    orderStatus = order.OrderStatus.ToString(),
                    paymentStatus = order.PaymentStatus.ToString()
                });
            }
            return BadRequest(new
            {
                message = "Payment not completed.",
                orderId = order.id,
                orderStatus = order.OrderStatus.ToString(),
                paymentStatus = "Pending"
            });

        }

        [HttpGet("PaymentCancelled")]
        [AllowAnonymous]
        public IActionResult PaymentCancelled()
        {
            return Ok(new { message = "Payment cancelled by the user." });
        }


        [HttpPost("Plus/{CartId}")]
        public IActionResult Plus(int? CartId)
        {
            if (CartId == null)
            {
                return NotFound();
            }

            var shoppingCart = unitOfWork.Repository<ShoppingCart>()
                .GetEntityWithSpec(new BaseSpecification<ShoppingCart>(x => x.id == CartId));

            var increaseCount = unitOfWork.ShoppingCartRepository.IncreaseCount(shoppingCart, 1);

            unitOfWork.Complete();
            return Ok();
        }

        [HttpPost("Minus/{CartId}")]
        public IActionResult Minus(int? CartId)
        {
            if (CartId == null)
            {
                return NotFound();
            }
            var shoppingCart = unitOfWork.Repository<ShoppingCart>()
              .GetEntityWithSpec(new BaseSpecification<ShoppingCart>(x => x.id == CartId));

            if (shoppingCart.Count <= 1)
            {
                unitOfWork.Repository<ShoppingCart>().Delete(shoppingCart);

                var spec = new BaseSpecification<ShoppingCart>(x => x.UserId == shoppingCart.UserId);
            }
            else
            {
                var increaseCount = unitOfWork.ShoppingCartRepository.DecreaseCount(shoppingCart, 1);
            }
            unitOfWork.Complete();

            return Ok();
        }

        [HttpPost("Remove/{CartId}")]
        public IActionResult Remove(int? CartId)
        {
            var ShoppingCart = unitOfWork.Repository<ShoppingCart>()
                .GetEntityWithSpec(new BaseSpecification<ShoppingCart>(x => x.id == CartId));

            unitOfWork.Repository<ShoppingCart>().Delete(ShoppingCart);
            unitOfWork.Complete();

            var spec = new BaseSpecification<ShoppingCart>(x => x.UserId == ShoppingCart.UserId);
            return Ok();
        }



    }
}
