

using eCommerce.DataAccessLayer.Entities;
using System.Linq.Expressions;

namespace eCommerce.DataAccessLayer.RepositoryContracts;

public interface IProductsRepository
{
    // Gets all products from the table
    Task<IEnumerable<Product>> GetProducts();

    // Gets all products based on specific conditions
    Task<IEnumerable<Product?>> GetProductsByCondition(Expression<Func<Product, bool>> conditionExpression);

    // Gets single product based on specific conditions
    Task<Product?> GetProductByCondition(Expression<Func<Product, bool>> conditionExpression);

    // Adds a product and returns the added object, or null if unsuccessful
    Task<Product?> AddProduct(Product product);

    Task<Product?> UpdateProduct(Product product);

    /// <summary>
    ///  Deletes product asynchronously
    /// </summary>
    /// <param name="productID">The productID to be deleted</param>
    /// <returns>Returns true if the deletion is successful, false</returns>
    Task<bool> DeleteProduct(Guid productID);
}
