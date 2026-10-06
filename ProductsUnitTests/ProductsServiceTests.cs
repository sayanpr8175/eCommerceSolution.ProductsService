using AutoFixture;
using AutoMapper;
using eCommerce.BusinessLogicLayer.DTO;
using eCommerce.DataAccessLayer.Entities;
using eCommerce.DataAccessLayer.RepositoryContracts;
using eCommerce.ProductsService.BusinessLogicLayer.RabbitMQ;
using eCommerce.ProductsService.BusinessLogicLayer.ServiceBus;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Moq;
using System.Linq.Expressions;

namespace eCommerce.ProductsMicroService.UnitTests;

public class ProductsServiceTests
{
    private readonly Mock<IProductsRepository> _repositoryMock = new();
    private readonly Mock<IMapper> _mapperMock = new();
    private readonly Mock<IValidator<ProductAddRequest>> _addValidatorMock = new();
    private readonly Mock<IValidator<ProductUpdateRequest>> _updateValidatorMock = new();
    private readonly Mock<IserviceBusPublisher> _serviceBusPublisherMock = new();
    private readonly Fixture _fixture = new();

    
    private readonly eCommerce.BusinessLogicLayer.Services.ProductsService _service;

    public ProductsServiceTests()
    {
        _service = new eCommerce.BusinessLogicLayer.Services.ProductsService(
            _addValidatorMock.Object,
            _updateValidatorMock.Object,
            _mapperMock.Object,
            _repositoryMock.Object,
            Mock.Of<IRabbitMQPublisher>(),
            _serviceBusPublisherMock.Object);
    }

    // --- AddProduct ---

    [Fact]
    public async Task AddProduct_ValidRequest_ReturnsAddedProduct()
    {
        // Arrange
        var request = _fixture.Create<ProductAddRequest>();
        var product = _fixture.Create<Product>();
        var expectedResponse = _fixture.Create<ProductResponse>();

        SetupValidation(_addValidatorMock);
        _mapperMock.Setup(mapper => mapper.Map<Product>(request)).Returns(product);
        _repositoryMock.Setup(repo => repo.AddProduct(product)).ReturnsAsync(product);
        _mapperMock.Setup(mapper => mapper.Map<ProductResponse>(product)).Returns(expectedResponse);

        // Act
        var result = await _service.AddProduct(request);

        // Assert
        result.Should().Be(expectedResponse);
        _repositoryMock.Verify(repo => repo.AddProduct(product), Times.Once);
    }

    [Fact]
    public async Task AddProduct_NullRequest_ThrowsArgumentNullException()
    {
        // Act
        Func<Task> act = () => _service.AddProduct(null!);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>();
        _repositoryMock.Verify(repo => repo.AddProduct(It.IsAny<Product>()), Times.Never);
    }

    [Fact]
    public async Task AddProduct_InvalidRequest_ThrowsArgumentExceptionWithAllErrors()
    {
        // Arrange
        var request = _fixture.Create<ProductAddRequest>();
        SetupValidation(_addValidatorMock, "Product name is required", "Unit price must be greater than zero");

        // Act
        Func<Task> act = () => _service.AddProduct(request);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("Product name is required, Unit price must be greater than zero");
        _repositoryMock.Verify(repo => repo.AddProduct(It.IsAny<Product>()), Times.Never);
    }

    // --- GetProducts ---

    [Fact]
    public async Task GetProducts_ProductsExist_ReturnsAllProducts()
    {
        // Arrange
        var products = _fixture.CreateMany<Product>().ToList();
        var expectedResponses = _fixture.CreateMany<ProductResponse>().ToList();

        _repositoryMock.Setup(repo => repo.GetProducts()).ReturnsAsync(products);
        _mapperMock.Setup(mapper => mapper.Map<IEnumerable<ProductResponse>>(products)).Returns(expectedResponses);

        // Act
        var result = await _service.GetProducts();

        // Assert
        result.Should().Equal(expectedResponses);
    }

    // --- GetProductByCondition ---

    [Fact]
    public async Task GetProductByCondition_MatchFound_ReturnsProduct()
    {
        // Arrange
        var product = _fixture.Create<Product>();
        var expectedResponse = _fixture.Create<ProductResponse>();

        SetupProductLookup(product);
        _mapperMock.Setup(mapper => mapper.Map<ProductResponse>(product)).Returns(expectedResponse);

        // Act
        var result = await _service.GetProductByCondition(p => p.ProductID == product.ProductID);

        // Assert
        result.Should().Be(expectedResponse);
    }

    [Fact]
    public async Task GetProductByCondition_NoMatch_ReturnsNull()
    {
        // Arrange
        SetupProductLookup(null);

        // Act
        var result = await _service.GetProductByCondition(p => p.ProductID == Guid.NewGuid());

        // Assert
        result.Should().BeNull();
    }

    // --- UpdateProduct ---

    [Fact]
    public async Task UpdateProduct_ValidRequest_ReturnsUpdatedProduct()
    {
        // Arrange
        var request = _fixture.Create<ProductUpdateRequest>();
        var existingProduct = _fixture.Create<Product>();
        var product = _fixture.Create<Product>();
        var expectedResponse = _fixture.Create<ProductResponse>();

        SetupProductLookup(existingProduct);
        SetupValidation(_updateValidatorMock);
        _mapperMock.Setup(mapper => mapper.Map<Product>(request)).Returns(product);
        _repositoryMock.Setup(repo => repo.UpdateProduct(product)).ReturnsAsync(product);
        _mapperMock.Setup(mapper => mapper.Map<ProductResponse>(product)).Returns(expectedResponse);

        // Act
        var result = await _service.UpdateProduct(request);

        // Assert
        result.Should().Be(expectedResponse);
        _repositoryMock.Verify(repo => repo.UpdateProduct(product), Times.Once);
    }

    [Fact]
    public async Task UpdateProduct_ProductDoesNotExist_ThrowsArgumentException()
    {
        // Arrange
        var request = _fixture.Create<ProductUpdateRequest>();
        SetupProductLookup(null);

        // Act
        Func<Task> act = () => _service.UpdateProduct(request);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>().WithMessage("Invalid Product ID");
        _repositoryMock.Verify(repo => repo.UpdateProduct(It.IsAny<Product>()), Times.Never);
    }

    [Fact]
    public async Task UpdateProduct_InvalidRequest_ThrowsArgumentExceptionWithAllErrors()
    {
        // Arrange
        var request = _fixture.Create<ProductUpdateRequest>();
        var existingProduct = _fixture.Create<Product>();

        SetupProductLookup(existingProduct);
        SetupValidation(_updateValidatorMock, "Product name is required", "Unit price must be greater than zero");

        // Act
        Func<Task> act = () => _service.UpdateProduct(request);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("Product name is required, Unit price must be greater than zero");
        _repositoryMock.Verify(repo => repo.UpdateProduct(It.IsAny<Product>()), Times.Never);
    }

    // --- DeleteProduct ---

    [Fact]
    public async Task DeleteProduct_ProductExists_ReturnsTrue()
    {
        // Arrange
        var existingProduct = _fixture.Create<Product>();

        SetupProductLookup(existingProduct);
        _repositoryMock.Setup(repo => repo.DeleteProduct(existingProduct.ProductID)).ReturnsAsync(true);

        // Act
        var result = await _service.DeleteProduct(existingProduct.ProductID);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteProduct_ProductDoesNotExist_ReturnsFalse()
    {
        // Arrange
        SetupProductLookup(null);

        // Act
        var result = await _service.DeleteProduct(Guid.NewGuid());

        // Assert
        result.Should().BeFalse();
        _repositoryMock.Verify(repo => repo.DeleteProduct(It.IsAny<Guid>()), Times.Never);
    }

    // --- Helpers ---

    
    private void SetupProductLookup(Product? product) =>
        _repositoryMock
            .Setup(repo => repo.GetProductByCondition(It.IsAny<Expression<Func<Product, bool>>>()))
            .ReturnsAsync(product);

    
    private static void SetupValidation<T>(Mock<IValidator<T>> validatorMock, params string[] errors) =>
        validatorMock
            .Setup(validator => validator.ValidateAsync(It.IsAny<T>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(errors.Select(error => new ValidationFailure(string.Empty, error))));
}