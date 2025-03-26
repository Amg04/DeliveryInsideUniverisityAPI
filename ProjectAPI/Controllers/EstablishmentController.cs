using BAL.interfaces;
using BLLProject.Specifications;
using DAL.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using ProjectAPI.DTO;
using Utilities;

namespace ProjectAPI.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class EstablishmentController : ControllerBase
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly IWebHostEnvironment hostEnvironment;

        public EstablishmentController(IUnitOfWork unitOfWork,IWebHostEnvironment hostEnvironment)
        {
            this.unitOfWork = unitOfWork;
            this.hostEnvironment = hostEnvironment;
        }

        [HttpGet("AllEstablishments")]
        public IActionResult AllEstablishments()
        {
           var establishments = unitOfWork.Repository<Establishment>().GetAll();
            if (!establishments.Any())
                return NotFound("No categories found.");
            return Ok(establishments);
        }


        
        [HttpGet("UniqueEstablishments")]
        public IActionResult GetUniqueEsbCategories()
        {
            var establishments = Enum.GetValues(typeof(EsbCategoryType))
                                 .Cast<EsbCategoryType>()
                                 .Select(e => e.ToString())
                                 .ToList();

            return Ok(establishments);
        }

        [HttpGet("{esbCategory}")]
        public IActionResult CategoriesByEsbCategory(string esbCategory)
        {
            // check if in enum or not 
            if (!Enum.TryParse<EsbCategoryType>(esbCategory, true, out var parsedCategory)
                || !Enum.IsDefined(typeof(EsbCategoryType), parsedCategory))
            {
                return BadRequest("Invalid EsbCategory. Allowed values: Restaurant, Cafe, GroceryStore, BookStore.");
            }

            // get First 5 Product
            var spec = new BaseSpecification<Establishment>(
                 e => e.EsbCategory == parsedCategory,
                query => query.Include(e => e.Products.OrderBy(p => p.id).Take(5)));

            var establishments = unitOfWork.Repository<Establishment>().GetAllWithSpec(spec);

            if (!establishments.Any())
            {
                return NotFound($"No establishments found for category: {esbCategory}");
            }

            var allProductIds = establishments.SelectMany(e => e.Products).Select(p => p.id).ToList();

            // الخاصة بهذه المنتجات reviews هات الـ
            var reviews = unitOfWork.Repository<Review>()
                .GetAllWithSpec(new BaseSpecification<Review>(r => allProductIds.Contains(r.ProductId)));

            // احسب المتوسطات لكل منتج
            var ratings = reviews
                .GroupBy(r => r.ProductId)
                .ToDictionary(g => g.Key, g => g.Average(r => r.Rating));


            var establishmentsDto = establishments.Select(e => new
            {
                e.id,
                e.Name,
                e.Location,
                e.OpeningHours,
                e.ContactNumber,
                e.ImageUrl,
                e.EsbCategory,
                e.CategoryId,
                Products = e.Products.Select(p => new
                {  
                    p.id,
                    p.Name,
                    p.Description,
                    p.ImageUrl,
                    p.IsAvailable,
                    p.price,
                    Rating = ratings.ContainsKey(p.id) ? Math.Round(ratings[p.id], 1) : 0
                }).ToList()
            }).ToList();
            return Ok(establishmentsDto);
        }

        [Authorize(Roles = SD.AdminRole)]
        [HttpPost]
        public IActionResult Add([FromForm] EstablishmentDTO establishmentDTO,IFormFile imageFile)
        {
            if (establishmentDTO == null || imageFile == null)
                return BadRequest("Invalid data or missing image");

            var categoryExists = unitOfWork.Repository<Category>().Get(establishmentDTO.CategoryId);
            if (categoryExists == null)
                return BadRequest($"Category with ID {establishmentDTO.CategoryId} not found");

            var imageUrl = ImageHelper.SaveImage(imageFile, hostEnvironment);

            var establishment = establishmentDTO.ToEstablishmentModel();
            establishment.ImageUrl = imageUrl;
            unitOfWork.Repository<Establishment>().Add(establishment);
            unitOfWork.Complete();

            return CreatedAtAction(nameof(GetUniqueEsbCategories), new { id = establishment.id }, establishmentDTO);
        }
        [Authorize(Roles = SD.AdminRole)]
        [HttpPut("{id}")]
        public IActionResult UpdateEstablishment(int id, [FromForm] EstablishmentDTO EstablishmentDTO, IFormFile? imageFile)
        {
            if (EstablishmentDTO == null)
                return BadRequest("Invalid data");

            var existingEstablishment = unitOfWork.Repository<Establishment>().Get(id);
            if (existingEstablishment == null)
                return NotFound("Establishment not found");

            if (imageFile != null)
            {
                ImageHelper.DeleteImage(existingEstablishment.ImageUrl, hostEnvironment);
                existingEstablishment.ImageUrl = ImageHelper.SaveImage(imageFile, hostEnvironment);
            }

            existingEstablishment = EstablishmentDTO.ToEstablishmentModel(existingEstablishment);

            unitOfWork.Repository<Establishment>().Update(existingEstablishment);
            unitOfWork.Complete();

            return NoContent();
        }
        [Authorize(Roles = SD.AdminRole)]
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var Establishment = unitOfWork.Repository<Establishment>().Get(id);

            if (Establishment == null)
                return NotFound($"Establishment with ID {id} not found");

            ImageHelper.DeleteImage(Establishment.ImageUrl, hostEnvironment);
            unitOfWork.Repository<Establishment>().Delete(Establishment);
            unitOfWork.Complete();

            return NoContent();
        }
    }
}
