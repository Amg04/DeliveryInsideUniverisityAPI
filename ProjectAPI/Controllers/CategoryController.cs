using BAL.interfaces;
using DAL.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjectAPI.DTO.CategoryDTOs;
using Utilities;

namespace ProjectAPI.Controllers
{
    [Authorize(Roles = SD.AdminRole)]
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private readonly IUnitOfWork unitOfWork;

        public CategoryController(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }

        [HttpGet("AllCategories")]
        public IActionResult AllCategories()
        {
            var categories = unitOfWork.Repository<Category>().GetAll();
            if (!categories.Any())
                return NotFound("No categories found.");

            return Ok(categories);
        }

        [HttpGet("CategoryById/{id}")]
        public IActionResult GetById(int id)
        {

            var category = unitOfWork.Repository<Category>().Get(id);

            if (category == null)
                return NotFound($"Category with ID {id} not found");

            return Ok(category);
        }


        [HttpPost]
        public IActionResult Add(CreateCategoryDTO categoryDto)
        {
            if (categoryDto == null)
                return BadRequest("Invalid data");

            var category = new Category()
            {
                Name = categoryDto.Name,
                Description = categoryDto.Description,
            };

            unitOfWork.Repository<Category>().Add(category);
            unitOfWork.Complete();

            return CreatedAtAction(nameof(GetById), new { id = category.id }, categoryDto);
        }

        [HttpPut]
        public IActionResult UpdateCategory(Category category)
        {
            if (category == null)
                return BadRequest("Invalid data");

            var existingCategory = unitOfWork.Repository<Category>().Get(category.id);
            if (existingCategory == null)
                return NotFound("Category not found");

            existingCategory.Name = category.Name;
            existingCategory.Description = category.Description;
            unitOfWork.Complete();

            return NoContent();
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var category = unitOfWork.Repository<Category>().Get(id);

            if (category == null)
                return NotFound($"Category with ID {id} not found");

            unitOfWork.Repository<Category>().Delete(category);
            unitOfWork.Complete();

            return NoContent(); 
        }
    }
}
