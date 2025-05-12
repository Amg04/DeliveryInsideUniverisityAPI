using BAL.interfaces;
using BLLProject.Specifications;
using DAL.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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

        public EstablishmentController(IUnitOfWork unitOfWork, IWebHostEnvironment hostEnvironment)
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

        //[HttpGet("{esbCategory}")]
        //public IActionResult CategoriesByEsbCategory(string esbCategory)
        //{
        //    // check if in enum or not 
        //    if (!Enum.TryParse<EsbCategoryType>(esbCategory, true, out var parsedCategory)
        //        || !Enum.IsDefined(typeof(EsbCategoryType), parsedCategory))
        //    {
        //        return BadRequest("Invalid EsbCategory. Allowed values: Restaurant, Cafe, GroceryStore, BookStore.");
        //    }

        //    // get First 5 Product
        //    var spec = new BaseSpecification<Establishment>(
        //         e => e.EsbCategory == parsedCategory,
        //        query => query.Include(e => e.Products.OrderBy(p => p.id).Take(5)));

        //    var establishments = unitOfWork.Repository<Establishment>().GetAllWithSpec(spec);

        //    if (!establishments.Any())
        //    {
        //        return NotFound($"No establishments found for category: {esbCategory}");
        //    }

        //    var allProductIds = establishments.SelectMany(e => e.Products).Select(p => p.id).ToList();

        //    var reviews = unitOfWork.Repository<Review>()
        //        .GetAllWithSpec(new BaseSpecification<Review>(r => allProductIds.Contains(r.ProductId)));

        //    var ratings = reviews
        //        .GroupBy(r => r.ProductId)
        //        .ToDictionary(g => g.Key, g => g.Average(r => r.Rating));

        //    var establishmentRatings = establishments.ToDictionary(
        //    e => e.id,
        //    e =>
        //    {
        //        var productIds = e.Products.Select(p => p.id);
        //        var ratedProducts = productIds.Where(id => ratings.ContainsKey(id)).ToList();
        //        if (!ratedProducts.Any()) return 0.0;
        //        return Math.Round(ratedProducts.Average(id => ratings[id]), 1);
        //    });

        //    var establishmentsDto = establishments.Select(e => new
        //    {
        //        e.id,
        //        e.Name,
        //        e.Location,
        //        e.OpeningHours,
        //        e.ContactNumber,
        //        e.ImageUrl,
        //        e.EsbCategory,
        //        e.CategoryId,
        //        EstablishmentRating = establishmentRatings[e.id],
        //        Products = e.Products.Select(p => new
        //        {
        //            p.id,
        //            p.Name,
        //            p.Description,
        //            p.ImageUrl,
        //            p.IsAvailable,
        //            p.price,
        //            Rating = ratings.ContainsKey(p.id) ? Math.Round(ratings[p.id], 1) : 0
        //        }).ToList()
        //    }).ToList();
        //    return Ok(establishmentsDto);
        //}

        [HttpGet("{esbCategory}")]
        public IActionResult CategoriesByEsbCategory(string esbCategory)
        {
            // check if in enum or not 
            if (!Enum.TryParse<EsbCategoryType>(esbCategory, true, out var parsedCategory)
                || !Enum.IsDefined(typeof(EsbCategoryType), parsedCategory))
            {
                return BadRequest("Invalid EsbCategory. Allowed values: Restaurant, Cafe, GroceryStore, BookStore.");
            }

            var establishments = GetEstablishmentsWithTopProducts(parsedCategory);
            if (!establishments.Any())
            {
                return NotFound($"No establishments found for category: {esbCategory}");
            }

            var allProductIds = establishments.SelectMany(e => e.Products).Select(p => p.id).ToList();

            var reviews = unitOfWork.Repository<Review>()
                .GetAllWithSpec(new BaseSpecification<Review>(r => allProductIds.Contains(r.ProductId)));

            var productRatings = reviews
                .GroupBy(r => r.ProductId)
                .ToDictionary(g => g.Key, g => g.Average(r => r.Rating));
            var establishmentRatings = CalculateEstablishmentRatings(establishments, productRatings);

            var dto = MapToDto(establishments, productRatings, establishmentRatings);
            return Ok(dto);
        }

        private List<Establishment> GetEstablishmentsWithTopProducts(EsbCategoryType category)
        {
            var spec = new BaseSpecification<Establishment>(
                e => e.EsbCategory == category,
                query => query.Include(e => e.Products.OrderBy(p => p.id).Take(5)));

            return unitOfWork.Repository<Establishment>().GetAllWithSpec(spec).ToList();
        }
        private Dictionary<int, double> CalculateEstablishmentRatings(IEnumerable<Establishment> establishments, Dictionary<int, double> productRatings)
        {
            return establishments.ToDictionary(
                e => e.id,
                e =>
                {
                    var ratedProductIds = e.Products.Select(p => p.id).Where(id => productRatings.ContainsKey(id)).ToList();
                    return ratedProductIds.Any() ? Math.Round(ratedProductIds.Average(id => productRatings[id]), 1) : 0.0;
                });
        }
        private List<object> MapToDto(IEnumerable<Establishment> establishments, Dictionary<int, double> productRatings, Dictionary<int, double> establishmentRatings)
        {
            return establishments.Select(e => new
            {
                e.id,
                e.Name,
                e.Location,
                e.OpeningHours,
                e.ContactNumber,
                e.ImageUrl,
                e.EsbCategory,
                e.CategoryId,
                EstablishmentRating = establishmentRatings[e.id],
                Products = e.Products.Select(p => new
                {
                    p.id,
                    p.Name,
                    p.Description,
                    p.ImageUrl,
                    p.IsAvailable,
                    p.price,
                    productRatings = productRatings.ContainsKey(p.id) ? Math.Round(productRatings[p.id], 1) : 0
                }).ToList()
            }).ToList<object>();
        }

        [Authorize(Roles = SD.AdminRole)]
        [HttpPost]
        public IActionResult Add([FromForm] EstablishmentDTO establishmentDTO, IFormFile imageFile)
        {
            if (establishmentDTO == null || imageFile == null)
                return BadRequest("Invalid data or missing image");

            var categoryExists = unitOfWork.Repository<Category>().Get(establishmentDTO.CategoryId);
            if (categoryExists == null)
                return BadRequest($"Category with ID {establishmentDTO.CategoryId} not found");

            //var webRootPath = hostEnvironment.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            //var uploadPath = Path.Combine(webRootPath, "api", "uploads");
            //Directory.CreateDirectory(uploadPath);

            //var fileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
            //var filePath = Path.Combine(uploadPath, fileName);

            //using (var fileStream = new FileStream(filePath, FileMode.Create))
            //{
            //    imageFile.CopyTo(fileStream);
            //}

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

                //        var webRootPath = hostEnvironment.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                //        var filePath = Path.Combine(webRootPath, "api", "uploads", Path.GetFileName(imageUrl));

                //        if (System.IO.File.Exists(filePath))
                //        {
                //            System.IO.File.Delete(filePath);
                //        }
                ImageHelper.DeleteImage(existingEstablishment.ImageUrl, hostEnvironment);
                //var webRootPath = hostEnvironment.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                //var uploadPath = Path.Combine(webRootPath, "api", "uploads");
                //Directory.CreateDirectory(uploadPath);

                //var fileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
                //var filePath = Path.Combine(uploadPath, fileName);

                //using (var fileStream = new FileStream(filePath, FileMode.Create))
                //{
                //    imageFile.CopyTo(fileStream);
                //}
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
            //        var webRootPath = hostEnvironment.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            //        var filePath = Path.Combine(webRootPath, "api", "uploads", Path.GetFileName(imageUrl));

            //        if (System.IO.File.Exists(filePath))
            //        {
            //            System.IO.File.Delete(filePath);
            //        }
            ImageHelper.DeleteImage(Establishment.ImageUrl, hostEnvironment);
            unitOfWork.Repository<Establishment>().Delete(Establishment);
            unitOfWork.Complete();

            return NoContent();
        }

     

    }
}
