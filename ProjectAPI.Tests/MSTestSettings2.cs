using BAL.interfaces;
using BLLProject.interfaces;
using DAL.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ProjectAPI.Controllers;
using ProjectAPI.DTO.ProductDTOs;

namespace ProjectAPI.Tests
{
    [TestClass]
    public class ProductControllerTests
    {
        private Mock<IUnitOfWork> mockUnitOfWork;
        private Mock<IGenericRepository<Product>> mockProductRepo;
        private ProductController controller;

        [TestInitialize]
        public void Setup()
        {
            mockUnitOfWork = new Mock<IUnitOfWork>();
            mockProductRepo = new Mock<IGenericRepository<Product>>();

            mockUnitOfWork.Setup(u => u.Repository<Product>()).Returns(mockProductRepo.Object);

            controller = new ProductController(mockUnitOfWork.Object);
        }

        [TestMethod]
        public void Index_ProductsExist_ReturnsOk()
        {
            // Arrange
            var products = new List<Product>
            {
                new Product { id = 1, Name = "Product 1" },
                new Product { id = 2, Name = "Product 2" }
            };

            mockProductRepo.Setup(r => r.GetAll()).Returns(products.AsQueryable());

            // Act
            var result = controller.Index() as OkObjectResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(200, result.StatusCode);

            var returned = result.Value as List<ProductDTO>;
            Assert.IsNotNull(returned);
            Assert.AreEqual(2, returned.Count);
        }

        [TestMethod]
        public void Index_NoProducts_ReturnsNotFound()
        {
            // Arrange
            mockProductRepo.Setup(r => r.GetAll()).Returns(new List<Product>().AsQueryable());

            // Act
            var result = controller.Index() as NotFoundObjectResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(404, result.StatusCode);
            Assert.AreEqual("No categories found.", result.Value);
        }
    }
}
