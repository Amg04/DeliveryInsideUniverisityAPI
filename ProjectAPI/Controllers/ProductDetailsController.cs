using BAL.interfaces;
using BLLProject.Repositories;
using BLLProject.Specifications;
using DAL.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjectAPI.DTO;
using System.Security.Claims;
using Utilities;

namespace ProjectAPI.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ProductDetailsController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;

        public ProductDetailsController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [HttpGet("{id}")]
        public IActionResult ShowProductDetails(int id)
        {

            var spec = new BaseSpecification<Product>(e => e.id == id);
            spec.Includes.Add(c => c.Category);
            var product = _unitOfWork.Repository<Product>().GetEntityWithSpec(spec);
            if (product == null)
                return NotFound();

            ShoppingCart obj = new ShoppingCart()
            {
                ProductId = id,
                Product = product,
                Count = 1
            };

            var CategoryName = obj.Product.Category.Name;

            return Ok(new
            {
                obj.ProductId,
                Product = new
                {
                    product.Name,
                    product.Description,
                    product.ImageUrl,
                    product.price,
                    product.IsAvailable,
                    product.CategoryId,
                    product.EstablishmentId,
                    CategoryName
                }
            ,
                obj.Count
            });
        }

        [HttpPost]
        public IActionResult SaveInShoppingCart([FromBody] ShoppingCart2DTO shoppingCartDTO)
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
            var spec = new BaseSpecification<ShoppingCart>(u => u.UserId == userId && u.ProductId == shoppingCartDTO.ProductId);
            var cartObj = _unitOfWork.Repository<ShoppingCart>().GetEntityWithSpec(spec);

            if (cartObj == null) // المنتج غير موجود في السلة، يتم إضافته
            {
                cartObj = new ShoppingCart
                {
                    ProductId = shoppingCartDTO.ProductId,
                    UserId = userId,
                    Count = shoppingCartDTO.Count
                };

                _unitOfWork.Repository<ShoppingCart>().Add(cartObj);
            }
            else // المنتج موجود مسبقًا، زيادة العدد
            {
                _unitOfWork.ShoppingCartRepository.IncreaseCount(cartObj, shoppingCartDTO.Count);

            }
            _unitOfWork.Complete();

            // تحديث عدد المنتجات في الجلسة
            var spec2 = new BaseSpecification<ShoppingCart>(x => x.UserId == userId);
            int count = _unitOfWork.Repository<ShoppingCart>().GetAllWithSpec(spec2).Count();
            HttpContext.Session?.SetInt32(SD.SessionKey, count);
            // or 
            //int count = _unitOfWork.Repository<ShoppingCart>().Count(x => x.UserId == userId);
            //HttpContext.Session?.SetInt32(SD.SessionKey, count);

            return Ok(new { Message = "Product added to cart successfully", CartCount = count });
        }
    }
}


