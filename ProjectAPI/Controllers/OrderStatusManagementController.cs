using BAL.interfaces;
using BLLProject.Repositories;
using BLLProject.Specifications;
using DAL;
using DAL.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjectAPI.DTO;
using Stripe;
using Utilities;

namespace ProjectAPI.Controllers
{
    [Authorize(Roles = SD.ExecutedOrderRole + "," + SD.DeliveryRole)]
    [Route("api/[controller]")]
    [ApiController]
    public class OrderStatusManagementController : ControllerBase
    {
        private readonly IUnitOfWork unitOfWork;

        public OrderStatusManagementController(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }

        [HttpGet("orders-by-status")]
        public IActionResult GetOrdersByStatus([FromQuery] OrderStatus status)
        {
            var orders = unitOfWork.Repository<Order>()
                .GetAllWithSpec(new BaseSpecification<Order>(o => o.OrderStatus == status));

            if (!orders.Any())
                return NotFound($"No orders found with status {status}.");

            var ordersDto = orders.Select(o => o.ToOrderDTO()).ToList();
            return Ok(ordersDto);
        }


        [HttpPut("update-status")]
        public IActionResult UpdateOrderStatus([FromBody] OrderStatusUpdateRequest request)
        {
            var orderFromDb = unitOfWork.Repository<Order>()
                .GetEntityWithSpec(new BaseSpecification<Order>(o => o.id == request.OrderId));
            var payment = unitOfWork.Repository<Payment>()
                .GetEntityWithSpec(new BaseSpecification<Payment>(o => o.OrderId == request.OrderId));

            if (orderFromDb == null)
                return NotFound($"Order with Id {request.OrderId} not found.");
            if (request.NewStatus == OrderStatus.Canceled && orderFromDb.PaymentStatus == PaymentStatus.Approved && payment == null)
                return BadRequest("Payment record not found for approved order.");

            if (!IsValidTransition(orderFromDb.OrderStatus, request.NewStatus))
                return BadRequest($"Cannot transition from {orderFromDb.OrderStatus} to {request.NewStatus}.");

            if(request.NewStatus == OrderStatus.Canceled)
            {
                if (orderFromDb.PaymentStatus == PaymentStatus.Approved)
                {
                    var option = new RefundCreateOptions
                    {
                        Reason = RefundReasons.RequestedByCustomer,
                        PaymentIntent = payment.PaymentIntentId
                    };
                    var service = new RefundService();
                    Refund refund = service.Create(option);

                    unitOfWork.OrderRepository.UpdateOrderStatus(orderFromDb.id, OrderStatus.Canceled);
                    unitOfWork.OrderRepository.UpdatePaymentStatus(orderFromDb.id, PaymentStatus.Refund);
                }
                else
                {
                    unitOfWork.OrderRepository.UpdateOrderStatus(orderFromDb.id, OrderStatus.Canceled);
                    unitOfWork.OrderRepository.UpdatePaymentStatus(orderFromDb.id, PaymentStatus.Failed);
                }
                unitOfWork.Complete();

                return Ok(new
                {
                    message = "Order has been cancelled successfully.",
                    orderId = orderFromDb.id,
                });
            }

            unitOfWork.OrderRepository.UpdateOrderStatus(request.OrderId, request.NewStatus);
            
            if (orderFromDb.OrderStatus == OrderStatus.OnTheWay)
            {
                orderFromDb.ShippingDate = DateTime.UtcNow;
                unitOfWork.Repository<Order>().Update(orderFromDb);
            }
            unitOfWork.Complete();
            var updatedOrder = unitOfWork.Repository<Order>()
                .GetEntityWithSpec(new BaseSpecification<Order>(o => o.id == request.OrderId));

           

            var orderDto = updatedOrder.ToOrderDTO();

            return Ok(new
            {
                message = "Order status updated successfully.",
                order = orderDto
            });
        }


        private bool IsValidTransition(OrderStatus currentStatus, OrderStatus newStatus)
        {
            return (currentStatus == OrderStatus.Pending && newStatus == OrderStatus.Preparing)
                || (currentStatus == OrderStatus.Pending && newStatus == OrderStatus.Canceled)
                || (currentStatus == OrderStatus.Preparing && newStatus == OrderStatus.Prepared)
                || (currentStatus == OrderStatus.Prepared && newStatus == OrderStatus.OnTheWay)
                || (currentStatus == OrderStatus.OnTheWay && newStatus == OrderStatus.Delivered);
        }

    }
}
