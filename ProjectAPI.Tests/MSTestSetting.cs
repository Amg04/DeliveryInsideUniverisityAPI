using BAL.interfaces;
using BLLProject.interfaces;
using DAL.Models;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Nest;
using ProjectAPI.Controllers;

namespace ProjectAPI.Tests
{
    [TestClass]
    public class CategoryControllerTests
    {
        private Mock<IUnitOfWork> mockUnitOfWork;
        private Mock<IGenericRepository<Category>> mockCategoryRepo;
        private CategoryController controller;

        [TestInitialize]
        public void Setup()
        {
            mockUnitOfWork = new Mock<IUnitOfWork>();
            mockCategoryRepo = new Mock<IGenericRepository<Category>>();
            mockUnitOfWork.Setup(u => u.Repository<Category>()).Returns(mockCategoryRepo.Object);
            controller = new CategoryController(mockUnitOfWork.Object);
        }

        [TestMethod]
        public void GetById_CategoryExists_ReturnsOk()
        {
            var category = new Category { id = 1, Name = "Test", Description = "Desc" };
            mockCategoryRepo.Setup(r => r.Get(1)).Returns(category);

            var result = controller.GetById(1) as OkObjectResult;

            Assert.IsNotNull(result);
            Assert.AreEqual(200, result.StatusCode);
            Assert.AreEqual(category, result.Value);
        }

        [TestMethod]
        public void GetById_CategoryDoesNotExist_ReturnsNotFound()
        {
            mockCategoryRepo.Setup(r => r.Get(1)).Returns((Category)null);

            var result = controller.GetById(1) as NotFoundObjectResult;

            Assert.IsNotNull(result);
            Assert.AreEqual(404, result.StatusCode);
            Assert.AreEqual("Category with ID 1 not found", result.Value);
        }
    }
}
