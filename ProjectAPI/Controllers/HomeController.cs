using BAL.interfaces;
using BLLProject.Repositories;
using BLLProject.Specifications;
using DAL.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjectAPI.DTO.HomeDTOs;
using System.Security.Claims;
using Utilities;

namespace ProjectAPI.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class HomeController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;

        public HomeController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [HttpGet("AllProducts")]
        public IActionResult GetAll()
        {
            var spec = new BaseSpecification<Product>();
            spec.Includes.Add(c => c.Category);
            var products = _unitOfWork.Repository<Product>().GetAllWithSpec(spec);

            return Ok(products);
        }

        [HttpGet("ProductDetails/{ProductId}")]
        public IActionResult ShowProductDetails(int ProductId)
        {

            var spec = new BaseSpecification<Product>(e => e.id == ProductId);
            spec.Includes.Add(c => c.Category);
            var product = _unitOfWork.Repository<Product>().GetEntityWithSpec(spec);
            if (product == null)
                return NotFound();

            ShoppingCart obj = new ShoppingCart()
            {
                ProductId = ProductId,
                Product = product,
                Count = 1
            };

            var ShoppingCartDto = obj.ToShoppingCartDTO();

           return Ok(ShoppingCartDto);
        }

        [HttpPost("AddToCart")]
        public IActionResult SaveInShoppingCart([FromBody] ProductCountDTO shoppingCartDTO)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User must be logged in.");
            }
            if (shoppingCartDTO.Count <= 0)
            {
                return BadRequest("Count must be at least 1.");
            }

            var cartObj = _unitOfWork.Repository<ShoppingCart>()
                .GetEntityWithSpec(new BaseSpecification<ShoppingCart>
                (u => u.UserId == userId && u.ProductId == shoppingCartDTO.ProductId));

            if (cartObj == null) 
            {
                cartObj = new ShoppingCart
                {
                    ProductId = shoppingCartDTO.ProductId,
                    UserId = userId,
                    Count = shoppingCartDTO.Count
                };

                _unitOfWork.Repository<ShoppingCart>().Add(cartObj);
            }
            else 
            {
                _unitOfWork.ShoppingCartRepository.IncreaseCount(cartObj, shoppingCartDTO.Count);

            }
            _unitOfWork.Complete();

            return Ok(new { Message = "Product added to cart successfully" });
        }
    }
}


