using BAL.interfaces;
using BLLProject.Specifications;
using DAL.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjectAPI.DTO;
using Utilities;

namespace ProjectAPI.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly IWebHostEnvironment _hostEnvironment;

        public ProductController(IUnitOfWork unitOfWork, IWebHostEnvironment hostEnvironment)
        {
            this.unitOfWork = unitOfWork;
            _hostEnvironment = hostEnvironment;
        }

        [HttpGet]
        public IActionResult Index()
        {
            var products = unitOfWork.Repository<Product>().GetAll();
            if (!products.Any())
                return NotFound("No categories found.");

            var productDTO = products.Select(c => c.ToProductDTO()).ToList();

            return Ok(productDTO);
        }

      
        [HttpGet("GetById/{id}")]
        public IActionResult GetById(int id)
        {
            var products = unitOfWork.Repository<Product>().Get(id);
            if (products == null)
            {
                return NotFound("Product not found");
            }
            
            var productDTO = products.ToProductDTO();

            return Ok(productDTO);

        }

        [HttpGet("{establishmentId}")]
        public IActionResult AllProductInEstablishment(int establishmentId)
        {
            var spec = new BaseSpecification<Product>(e => e.EstablishmentId == establishmentId);
            var products =  unitOfWork.Repository<Product>().GetAllWithSpec(spec);

            if (!products.Any())
                return NotFound("No products found.");

            // reviews بتاع المنتجات اللي لسه جايبنها، علشان نستخدمهم في البحث عن الـ id بنستخرج كل الـ 
            var productIds = products.Select(p => p.id).ToList();

            //الخاصة بالمنتجات دي Reviews هات كل الـ  
            var reviews =  unitOfWork.Repository<Review>()
                .GetAllWithSpec(new BaseSpecification<Review>(r => productIds.Contains(r.ProductId)));

            // احسب المتوسطات لكل منتج
            var ratings = reviews
                .GroupBy(r => r.ProductId)
                .ToDictionary(g => g.Key, g => g.Average(r => r.Rating));

            var productsDTO = products.Select(p =>
            {
                double rating = ratings.ContainsKey(p.id) ? ratings[p.id] : 0;
                return new ProductRateDTO
                {
                    productDTO = p.ToProductDTO(),
                    Rating = Math.Round(rating, 1)
                };
            }).ToList();

            return Ok(productsDTO);
        }


        [Authorize(Roles = SD.AdminRole)]
        [HttpPost]
        public IActionResult Add([FromForm] CreateProductDTO productDTO, IFormFile imageFile)
        {
            if (productDTO == null || imageFile == null)
                return BadRequest("Invalid data or missing image");

            var categoryExists = unitOfWork.Repository<Category>().Get(productDTO.CategoryId);
            if (categoryExists == null)
                return BadRequest($"Category with ID {productDTO.CategoryId} not found");

            var imageUrl = ImageHelper.SaveImage(imageFile, _hostEnvironment);
            

            var product = productDTO.ToCreateProductModel();
            product.ImageUrl = imageUrl;
            unitOfWork.Repository<Product>().Add(product);
            unitOfWork.Complete();

            return CreatedAtAction(nameof(GetById), new { id = product.id }, productDTO);
        }

        [Authorize(Roles = SD.AdminRole)]
        [HttpPut("{id}")]
        public IActionResult Update(int id, [FromForm] CreateProductDTO productDTO, IFormFile? imageFile)
        {
            if (productDTO == null)
                return BadRequest("Invalid data");

            var existingProduct = unitOfWork.Repository<Product>().Get(id);
            if (existingProduct == null)
                return NotFound("Product not found");

            if (imageFile != null)
            {
                ImageHelper.DeleteImage(existingProduct.ImageUrl, _hostEnvironment);
                existingProduct.ImageUrl = ImageHelper.SaveImage(imageFile, _hostEnvironment);
            }

            existingProduct = productDTO.ToCreateProductModel(existingProduct);
            unitOfWork.Repository<Product>().Update(existingProduct);
            unitOfWork.Complete();

            return NoContent();
        }


        [Authorize(Roles = SD.AdminRole)]
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var product = unitOfWork.Repository<Product>().Get(id);
            if (product == null)
                return NotFound($"Product with ID {id} not found");

            ImageHelper.DeleteImage(product.ImageUrl, _hostEnvironment);
            unitOfWork.Repository<Product>().Delete(product);
            unitOfWork.Complete();

            return NoContent();
        }
    }
}
