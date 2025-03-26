using BAL.interfaces;
using DAL.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjectAPI.DTO;
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

            var categoryDTO = categories.Select(c => c.ToCategoryDTO()).ToList();
            return Ok(categoryDTO);
        }

        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {

            var category = unitOfWork.Repository<Category>().Get(id);

            if (category == null)
                return NotFound($"Category with ID {id} not found");

            var categoryDTO = category.ToCategoryDTO(); 

            return Ok(categoryDTO);
        }


        [HttpPost]
        public IActionResult Add(CategoryDTO categoryDto)
        {
            if (categoryDto == null)
                return BadRequest("Invalid data");

            var category = categoryDto.ToCategoryModel(); 

            unitOfWork.Repository<Category>().Add(category);
            unitOfWork.Complete();

            return CreatedAtAction(nameof(GetById), new { id = category.id }, categoryDto);
        }

        [HttpPut("{id}")]
        public IActionResult UpdateCategory(int id, CategoryDTO categoryDto)
        {
            if (categoryDto == null)
                return BadRequest("Invalid data");

            var existingCategory = unitOfWork.Repository<Category>().Get(id);
            if (existingCategory == null)
                return NotFound("Category not found");

            existingCategory = categoryDto.ToCategoryModel(existingCategory); 

            unitOfWork.Repository<Category>().Update(existingCategory);
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
