
using DataAccessLayer.Entities;
using eCommerce.BusinessLogicLayer.DTO;
using System.Linq.Expressions;

namespace eCommerce.BusinessLogicLayer.ServiceContracts;

public interface IProductsService
{
    /// <summary>
    /// Retrieves the list of products from the products repository
    /// </summary>
    /// <returns>List of ProductResponse</returns>
    Task<List<ProductResponse?>> GetProducts();

    Task<List<ProductResponse?>> GetProductsByCondition
        (Expression<Func<Product, bool>> conditionExpression);

    /// <summary>
    /// Returns single product
    /// </summary>
    /// <param name="conditionExpression"></param>
    /// <returns>Product</returns>

    Task<Product?> GetProductByCondition
    (Expression<Func<Product, bool>> conditionExpression);

    /// <summary>
    /// Adds product into the table
    /// </summary>
    /// <param name="productAddRequest"></param>
    /// <returns></returns>
    Task<ProductResponse?> AddProduct (ProductAddRequest productAddRequest);

    Task<ProductResponse?> UpdateProduct (ProductAddRequest productAddRequest);

    /// <summary>
    /// Deletes the product based on product id
    /// </summary>
    /// <param name="productID"></param>
    /// <returns></returns>
    Task<bool> DeleteProduct(Guid productID);
}
