using BAL.interfaces;
using BLLProject.Specifications;
using DAL;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Utilities;

namespace ProjectAPI.Controllers
{
    [Authorize(Roles = SD.DeliveryRole)]
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentStatusController : ControllerBase
    {
        private readonly IUnitOfWork unitOfWork;

        public PaymentStatusController(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }

        [HttpPut("update-payment-status")]
        public IActionResult UpdatePaymentStatus([FromBody] PaymentStatusUpdateRequest request)
        {
            var order = unitOfWork.Repository<Order>()
                .GetEntityWithSpec(new BaseSpecification<Order>(o => o.id == request.OrderId));

            if (order == null)
                return NotFound($"Order with Id {request.OrderId} not found.");

            if (order.PaymentStatus != PaymentStatus.CashOnDelivery)
                return BadRequest("The order has already been paid or updated before.");

            if (!(request.NewPaymentStatus == PaymentStatus.Failed || request.NewPaymentStatus == PaymentStatus.Completed))
                return BadRequest($"Invalid payment status: {request.NewPaymentStatus}. Must be Completed or Failed.");

            unitOfWork.OrderRepository.UpdatePaymentStatus(request.OrderId, request.NewPaymentStatus);
            unitOfWork.Complete();
            
            return Ok(new
            {
                message = $"Payment status updated to {request.NewPaymentStatus}.",
                orderId = request.OrderId
            });
        }
    }
}
