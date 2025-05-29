using BAL.interfaces;
using BLLProject.Specifications;
using DAL;
using DAL.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjectAPI.DTO.ReviewDTOs;
using System.Security.Claims;

namespace ProjectAPI.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ReviewController : ControllerBase
    {
        private readonly IUnitOfWork unitOfWork;

        public ReviewController(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }

        [HttpGet]
        public IActionResult Index()
        {
            var Reviews = unitOfWork.Repository<Review>().GetAll();
            if (!Reviews.Any())
                return NotFound("No Reviews found.");

            var ReviewDTO = Reviews.Select(c => c.ToReviewDTO()).ToList();
            return Ok(ReviewDTO);
        }

        [HttpPost]
        public IActionResult Add(ReviewDTO reviewDTO)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("User not authorized or token invalid.");

            var productExists = unitOfWork.Repository<Product>()
                .GetEntityWithSpec(new BaseSpecification<Product>(p => p.id == reviewDTO.ProductId));
            if (productExists == null)
                return NotFound("Product not found.");

            var orderExists = unitOfWork.Repository<Order>()
                .GetEntityWithSpec(new BaseSpecification<Order>(p => p.id == reviewDTO.OrderId));
            if (orderExists == null)
                return NotFound("Order not found.");

            var review = new Review()
            {
                UserId = userId,
                ProductId = reviewDTO.ProductId,
                OrderId = reviewDTO.OrderId,
                Rating = reviewDTO.Rating,
                Comment = reviewDTO.Comment,
            };

            unitOfWork.Repository<Review>().Add(review);
            unitOfWork.Complete();

            return CreatedAtAction(nameof(Index), new { id = review.id }, reviewDTO);
        }
    }
}
