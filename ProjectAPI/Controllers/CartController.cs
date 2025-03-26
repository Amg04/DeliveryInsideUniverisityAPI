using BAL.interfaces;
using BLLProject.Specifications;
using DAL;
using DAL.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using ProjectAPI.DTO;
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
        private readonly Stripedata stripeData;
        private readonly StripeSettings stripeSettings;

        public CartController(IUnitOfWork unitOfWork,
            IOptions<Stripedata> stripeOptions,
            IOptions<StripeSettings> stripeSettings)
        {

            this.unitOfWork = unitOfWork;
            this.stripeData = stripeOptions.Value;
            this.stripeSettings = stripeSettings.Value;
        }


        [HttpGet("Index")]
        public IActionResult Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var spec = new BaseSpecification<ShoppingCart>(u => u.UserId == userId);
            spec.Includes.Add(c => c.Product);
            var shoppingCart = unitOfWork.Repository<ShoppingCart>().GetAllWithSpec(spec).ToList();
            var shoppingCartDTOs = shoppingCart.Select(c => c.ToShoppingCartDTO()).ToList();
            var cartDTO = new CartDTO()
            {
                CartsList = shoppingCartDTOs,
            };

            // Total
            foreach (var item in cartDTO.CartsList)
            {
                cartDTO.TotalCarts += (item.Count * item.Product.price);
            }

            return Ok(cartDTO);
        }

        [HttpGet("GetSummary")]
        public IActionResult Summary()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var spec = new BaseSpecification<ShoppingCart>(u => u.UserId == userId);
            spec.Includes.Add(c => c.Product);
            var shoppingCart = unitOfWork.Repository<ShoppingCart>().GetAllWithSpec(spec).ToList();
            var shoppingCartDTOs = shoppingCart.Select(c => c.ToShoppingCartDTO()).ToList();

            var user = unitOfWork.Repository<User>().GetEntityWithSpec(new BaseSpecification<User>(x => x.Id == userId));


            var Order = new Order();
            Order.UserId = userId;
            Order.Name = user.Name;
            Order.Address = user.Address;
            Order.PhoneNumber = user.PhoneNumber;
            var OrderDTO = Order.ToOrderDTO();
            var cartDTO = new CartDTO()
            {
                CartsList = shoppingCartDTOs,
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
        public IActionResult SubmitPaymentMethod([FromBody] PaymentOrderDTO paymentDto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var spec = new BaseSpecification<ShoppingCart>(u => u.UserId == userId);
            spec.Includes.Add(c => c.Product);
            var CartsList = unitOfWork.Repository<ShoppingCart>().GetAllWithSpec(spec).ToList();
            if (!CartsList.Any())
                return BadRequest("Shopping Cart is Empty");

            var user = unitOfWork.Repository<User>().GetEntityWithSpec(new BaseSpecification<User>(x => x.Id == userId));

            // if order is exist 
            var spec2 = new BaseSpecification<Order>(x => x.UserId == userId && x.PaymentStatus == PaymentStatus.Pending);
            var existingOrder = unitOfWork.Repository<Order>().GetEntityWithSpec(spec2);

            user.Name = paymentDto.Name;
            user.Address = paymentDto.Address;
            user.PhoneNumber = paymentDto.PhoneNumber;

            Order order;
            if (existingOrder != null)
            {
                // تحديث الطلب الحالي
                existingOrder.Address = user.Address;
                existingOrder.Name = user.Name;
                existingOrder.PhoneNumber = user.PhoneNumber;
                existingOrder.TotalPrice = 0;
                foreach (var item in CartsList)
                {
                    existingOrder.TotalPrice += (item.Count * item.Product.price);
                }

                existingOrder.OrderDate = DateTime.Now;
                unitOfWork.Repository<Order>().Update(existingOrder);

                // حذف تفاصيل الطلب القديمة المرتبطة بالطلب (إن وجدت)
                var oldOrderspec = new BaseSpecification<OrderItem>(x => x.OrderId == existingOrder.id);
                var oldOrderDetails = unitOfWork.Repository<OrderItem>().GetAllWithSpec(oldOrderspec);
                foreach (var detail in oldOrderDetails)
                {
                    unitOfWork.Repository<OrderItem>().Delete(detail);
                }

                order = existingOrder;
            }
            else
            {
                var establishmentId = CartsList.FirstOrDefault()?.Product?.EstablishmentId;
                if (establishmentId == null)
                    return BadRequest("Establishment not found for items in cart.");
                order = new Order
                {
                    UserId = userId,
                    Name = user.Name,
                    EstablishmentId = (int)establishmentId,
                    Address = user.Address,
                    PhoneNumber = user.PhoneNumber,
                    OrderDate = DateTime.Now,
                    PaymentStatus = paymentDto.PaymentMethod.ToLower() == "online" ? PaymentStatus.Pending : PaymentStatus.CashOnDelivery,
                    OrderStatus = OrderStatus.Pending,
                };
                foreach (var item in CartsList)
                {
                    order.TotalPrice += (item.Count * item.Product.price);
                }
                unitOfWork.Repository<Order>().Add(order);
            }
            unitOfWork.Complete();

            foreach (var item in CartsList)
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
                foreach (var item in CartsList)
                {
                    unitOfWork.Repository<ShoppingCart>().Delete(item);
                }
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
            var CartsList = unitOfWork.Repository<ShoppingCart>().GetAllWithSpec(spec).ToList();
            var domain = stripeSettings.Domain;
            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string> { "card" },
                LineItems = new List<SessionLineItemOptions>(),

                Mode = "payment",
                SuccessUrl = domain + $"api/Cart/OrderConfirmation?id={order.id}",
                CancelUrl = domain + $"api/cart/Index",
            };

            foreach (var item in CartsList)
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

        [HttpPost("Plus/{CartId}")]
        public IActionResult Plus(int? CartId)
        {
            if (CartId == null)
            {
                return NotFound();
            }

            var shoppingCart = unitOfWork.Repository<ShoppingCart>()
                .GetEntityWithSpec(new BaseSpecification<ShoppingCart>(x => x.id == CartId));

            var increaseCount = unitOfWork.Repository<ShoppingCart>() as IShoppingCartRepository;
            if (increaseCount == null)
                return StatusCode(500, "failed to increaseCount.");
            increaseCount.IncreaseCount(shoppingCart, 1);

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
                var count = unitOfWork.Repository<ShoppingCart>().GetAllWithSpec(spec).ToList().Count() - 1;

                HttpContext.Session.SetInt32(SD.SessionKey, count);
            }
            else
            {
                var increaseCount = unitOfWork.Repository<ShoppingCart>() as IShoppingCartRepository;
                if (increaseCount == null)
                    return StatusCode(500, "failed to increaseCount.");
                increaseCount.DecreaseCount(shoppingCart, 1);
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
            var count = unitOfWork.Repository<ShoppingCart>().GetAllWithSpec(spec).ToList().Count() - 1;

            HttpContext.Session.SetInt32(SD.SessionKey, count);

            return Ok();
        }



    }
}
