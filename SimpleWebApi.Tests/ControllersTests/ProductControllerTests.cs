using Microsoft.AspNetCore.Mvc;
using Moq;
using SimpleWebApi.Controllers;
using SimpleWebApi.Models;
using SimpleWebApi.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleWebApi.Tests.ControllersTests
{
    public class ProductControllerTests
    {
        private readonly Mock<IProductRepository> _productRepositoryMock;
        private readonly ProductsController _productsController;
        public ProductControllerTests()
        {
            _productRepositoryMock = new Mock<IProductRepository>();

            //SUT
            _productsController = new ProductsController(_productRepositoryMock.Object);
        }
        [Fact]
        public void GetAllProducts_ReturnsOkResult_WithListOfProducts()
        {
            //Arrange 
            _productRepositoryMock.Setup(repo => repo.GetAllProducts()).Returns(GetTestProducts());

            //Act
            var result = _productsController.GetAllProducts();

            //Assert
            var OkResult = Assert.IsType<OkObjectResult>(result.Result);
            var products = Assert.IsType<List<Product>>(OkResult.Value);
            Assert.Equal(2, products.Count);
        }
        [Fact]
        public void GetProductsById_ReturnsNotFound_WhenProductNotFound()
        {
            //Arrange
            _productRepositoryMock.Setup(repo => repo.GetProductById(1)).Returns((Product)null);

            //Act
            var result = _productsController.GetProductById(1);
            //Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public void GetProductById_ReturnsOkResult_WithProduct()
        {
            //Arrange
            var product = GetTestProducts().FirstOrDefault(x => x.Id == 1);
            _productRepositoryMock.Setup(repo => repo.GetProductById(1)).Returns(product);
            //Act
            var Actual = _productsController.GetProductById(product.Id);
            //Assert
            var OkResult = Assert.IsType<OkObjectResult>(Actual.Result);
            var productResult = Assert.IsType<Product>(OkResult.Value);
            Assert.Equal("Product1", productResult.Name);
        }

        [Fact]
        public void AddProduct_ReturnsCreatedAtActionResult_WhenProductIsValid()
        {
            //Arrange
            var product = new Product { Id = 3, Name = "Product3" };
            _productRepositoryMock.Setup(repo => repo.AddProduct(product));

            //Act
            var result = _productsController.AddProduct(product);

            //Assert
            var CreatedAtResult = Assert.IsType<CreatedAtActionResult>(result);
            var Actual = Assert.IsType<Product>(CreatedAtResult.Value);
            Assert.Equal(product.Id, Actual.Id);
            Assert.Equal(product.Name, Actual.Name);
            _productRepositoryMock.Verify(repo=> repo.AddProduct(product), Times.Once);

        }

        [Fact] 
        public void UpdateProduct_ReturnsNotFound_WhenTheProductNotExists()
        {
            //Arrange
            var product = new Product { Id = 4, Name = "test" };
            _productRepositoryMock.Setup(repo => repo.GetProductById(product.Id)).Returns((Product)null);

            //Act
            var result = _productsController.UpdateProduct(product.Id, product);

            //Assert
            var NotFoundResponse = Assert.IsType<NotFoundResult>(result);
            _productRepositoryMock.Verify(repo => repo.UpdateProduct(It.IsAny<Product>()), Times.Never);
        }

        [Fact]
        public void UpdateProduct_ReturnsNoContent_WhenProductExists()
        {
            //Arrange
            var product = new Product { Id = 1, Name = "UpdatedProduct", Price = 20.0M };
            _productRepositoryMock.Setup(repo => repo.GetProductById(product.Id)).Returns(product);

            //Act
            var result = _productsController.UpdateProduct(product.Id, product);
            //Assert
            var NoContentResult = Assert.IsType<NoContentResult>(result);
            _productRepositoryMock.Verify(repo => repo.UpdateProduct(product), Times.Once);
        }

        [Fact]
        public void DeleteProduct_ReturnsNotFound_WhenProductNotExists()
        {
            //Arrange
            var productToBeDeleted = new Product { Id = 999, Name = "Product Doesn't Exist", Price = 100.0M };
            _productRepositoryMock.Setup(repo => repo.GetProductById(productToBeDeleted.Id)).Returns((Product)null);

            //Act
            var Actual = _productsController.DeleteProduct(productToBeDeleted.Id);

            //Assert
            var result = Assert.IsType<NotFoundResult>(Actual);
            _productRepositoryMock.Verify(repo => repo.DeleteProduct(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public void DeleteProduct_ReturnsNoContent_WhenProductIsDeleted()
        {
            //Arrange:
            var existingProduct = GetTestProducts().FirstOrDefault(x => x.Id == 1);
            _productRepositoryMock.Setup(repo => repo.GetProductById(1)).Returns(existingProduct);

            //Act:
            var Actual = _productsController.DeleteProduct(existingProduct.Id);

            //Assert:
            var NoContentResult = Assert.IsType<NoContentResult>(Actual);
            _productRepositoryMock.Verify(repo => repo.DeleteProduct(existingProduct.Id), Times.Once);
        }
        private List<Product> GetTestProducts()
        {
            return new List<Product>
            {
                new Product { Id = 1, Name = "Product1", Price = 10.5M },
                new Product { Id = 2, Name = "Product2", Price = 20.0M }
            };
        }
    }
}
