using AutoMapper;
using eCommerce.BusinessLogicLayer.DTO;
using eCommerce.BusinessLogicLayer.ServiceContracts;
using eCommerce.DataAccessLayer.Entities;
using eCommerce.DataAccessLayer.RepositoryContracts;
using eCommerce.ProductsService.BusinessLogicLayer.RabbitMQ;
using FluentValidation;
using FluentValidation.Results;
using System.Linq.Expressions;

namespace eCommerce.BusinessLogicLayer.Services;

public class ProductsService : IProductsService
{
    private readonly IValidator<ProductAddRequest> _productAddRequestValidator;
    private readonly IValidator<ProductUpdateRequest> _productUpdateRequestValidator;
    private readonly IMapper _mapper;
    private readonly IProductsRepository _productsRepository;
    private readonly IRabbitMQPublisher _rabbitMQPublisher;

    public ProductsService(IValidator<ProductAddRequest> productAddRequestValidator,
        IValidator<ProductUpdateRequest> productUpdateRequestValidator,
        IMapper mapper, IProductsRepository productsRepository,
        IRabbitMQPublisher rabbitMQPublisher)
    {
        _productAddRequestValidator = productAddRequestValidator;
        _productUpdateRequestValidator = productUpdateRequestValidator;
        _mapper = mapper;
        _productsRepository = productsRepository;
        _rabbitMQPublisher = rabbitMQPublisher;

    }

    public async Task<ProductResponse?> AddProduct(ProductAddRequest productAddRequest)
    {
        if(productAddRequest==null)
        {
            throw new ArgumentException(nameof(productAddRequest));
        }

        // Validate using fluent validation.

        ValidationResult validResult =  await _productAddRequestValidator.ValidateAsync(productAddRequest);

        if(!validResult.IsValid)
        {
            string errors = string.Join(", ", validResult.Errors.Select(temp => temp.ErrorMessage));

            throw new ArgumentException(errors);
        }

        Product obj = _mapper.Map<Product>(productAddRequest);

        Product? addedProduct = await _productsRepository.AddProduct(obj);

        if(addedProduct==null)
        {
            return null;
        }

        ProductResponse productResponseObj = _mapper.Map<ProductResponse>(addedProduct);

        return productResponseObj;
    }

    public async Task<bool> DeleteProduct(Guid productID)
    {
        Product? productObj = await _productsRepository.GetProductByCondition(temp => temp.ProductID == productID);
        if(productObj==null)
        {
            return false;
        }

        bool success = await _productsRepository.DeleteProduct(productID);

        // We are publishing deletion message in case of product delete.
        if(success)
        {
            ProductDeleteMessage message = new ProductDeleteMessage(
                productObj.ProductID,
                productObj.ProductName);

            // string routingKey = "product.delete";

            // This was for old direct exchange method
            // _rabbitMQPublisher.Publish<ProductDeleteMessage>(routingKey, message);

            // implementing headers exchange as it is widely used.

            var headers = new Dictionary<string, object>()
            {
                {"event", "product.delete" },
                {"RowCount", 1 }
            };

            _rabbitMQPublisher.Publish<ProductDeleteMessage>(headers, message);
        }

        return success;
    }

    public async Task<ProductResponse?> GetProductByCondition(Expression<Func<Product, bool>> conditionExpression)
    {
        Product? productObj = await _productsRepository.GetProductByCondition(conditionExpression);

        if(productObj==null)
        {
            return null;
        }

        ProductResponse productResponseObj =  _mapper.Map<ProductResponse>(productObj);

        return productResponseObj;

    }

    public async Task<List<ProductResponse?>> GetProducts()
    {
        IEnumerable<Product?> ProductsList = await _productsRepository.GetProducts();

        IEnumerable<ProductResponse?> productResponsesObj = _mapper.Map<IEnumerable<ProductResponse>>(ProductsList);

        return productResponsesObj.ToList();
    }

    public async Task<List<ProductResponse?>> GetProductsByCondition(Expression<Func<Product, bool>> conditionExpression)
    {
        IEnumerable<Product?> ProductsList = await _productsRepository.GetProductsByCondition(conditionExpression);

        IEnumerable<ProductResponse?> productResponsesObj = _mapper.Map<IEnumerable<ProductResponse>>(ProductsList);

        return productResponsesObj.ToList();
    }

    public async Task<ProductResponse?> UpdateProduct(ProductUpdateRequest productUpdateRequest)
    {
        Product? productObj = await _productsRepository.GetProductByCondition(temp => temp.ProductID == productUpdateRequest.ProductID);
        if(productObj==null)
        {
            throw new ArgumentException("Invalid product id");
        }

        ValidationResult validProduct = await _productUpdateRequestValidator.ValidateAsync(productUpdateRequest);

        if (!validProduct.IsValid)
        {
            string errors = string.Join(", ", validProduct.Errors.Select(t => t.ErrorMessage));

            throw new ArgumentException(errors);
        }

        Product product = _mapper.Map<Product>(productUpdateRequest);

        bool isProductNameChanged = (productUpdateRequest.ProductName != productObj.ProductName);

        Product? updatedProduct =  await _productsRepository.UpdateProduct(product);

        if (isProductNameChanged)
        {
            //string routingKey = "product.update.name";
            var message = new ProductNameUpdateMessage(product.ProductID, product.ProductName);

            //this was for direct exchange
            // _rabbitMQPublisher.Publish<ProductNameUpdateMessage>(routingKey, message); 

            // this new invoking is for headers exchange
          
            var headers = new Dictionary<string, object>()
            {
                {"event", "product.update" },
                {"field", "name"},
                {"RowCount", 1 }
            };

            _rabbitMQPublisher.Publish<ProductNameUpdateMessage>(headers, message);

        }

        ProductResponse prodResp = _mapper.Map<ProductResponse>(updatedProduct);

        return prodResp;
        
    }

   
    
}
