using BAL.interfaces;
using BLLProject.Specifications;
using DAL;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Linq;

namespace ProjectAPI.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly IUnitOfWork unitOfWork;

        public OrderController(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }

        [HttpGet("PastOrders")]
        public IActionResult PastOrders()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("User not found.");

            var spec = new BaseSpecification<Order>(o => o.UserId == userId &&
            (o.OrderStatus == OrderStatus.Delivered|| o.OrderStatus == OrderStatus.Canceled));
            spec.ComplexIncludes.Add(q => q.Include(o => o.OrderItems)
                              .ThenInclude(i => i.Product));

            var pastOrders = unitOfWork.Repository<Order>().GetAllWithSpec(spec).ToList();
            if (!pastOrders.Any())
                return NotFound("No past orders found.");

            return Ok(pastOrders);
        }

        [HttpGet("CurrentOrders")]
        public  IActionResult CurrentOrder()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("User not found.");

            var spec = new BaseSpecification<Order>(o => o.UserId == userId 
            && (o.OrderStatus != OrderStatus.Delivered || o.OrderStatus != OrderStatus.Canceled));
            spec.ComplexIncludes.Add(q => q.Include(o => o.OrderItems)
                             .ThenInclude(i => i.Product));

            var currentOrder = unitOfWork.Repository<Order>().GetAllWithSpec(spec).ToList();
            if (currentOrder == null || !currentOrder.Any())
                return NotFound("No current order found.");

            return Ok(currentOrder);
        }
    }
}
