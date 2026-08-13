

using eCommerce.BusinessLogicLayer.DTO;
using eCommerce.BusinessLogicLayer.ServiceContracts;
using FluentValidation;
using FluentValidation.Results;


namespace eCommerce.ProductsMicroService.API.APIEndpoints
{
    public static class ProductAPIEndpoints
    {
        public static IEndpointRouteBuilder MapProductAPIEndpoints(this IEndpointRouteBuilder app)
        {

            // Get /api/products

            app.MapGet("/api/products", async (IProductsService productsService) =>
            {
                List<ProductResponse?> products = await productsService.GetProducts();

                return Results.Ok(products);
            });

            // api/products/search/productid/{ProductID}

            app.MapGet("/api/products/search/product-id/{ProductID:guid}", async (IProductsService 
                productsService, Guid ProductID ) =>
            {
                ProductResponse? product = await 
                productsService.GetProductByCondition(t => t.ProductID == ProductID);

                return Results.Ok(product);
            });

            app.MapGet("/api/products/search/{SearchString}", async (IProductsService
               productsService, string SearchString) =>
            {
                List<ProductResponse?> productsByProductName = await
                productsService.GetProductsByCondition(t => t.ProductName.Contains(SearchString,
                StringComparison.OrdinalIgnoreCase));

                List<ProductResponse?> productsByCategory = await
                productsService.GetProductsByCondition(t => t.Category.Contains(SearchString,
                StringComparison.OrdinalIgnoreCase));

                var products = productsByProductName.Union(productsByCategory);

                return Results.Ok(products);
            });


            // POST /api/products
            app.MapPost("/api/products", async (IProductsService
               productsService, IValidator<ProductAddRequest> productAddRequestValidator , ProductAddRequest productAddRequest) =>
            {
                // Validate the product add req

                ValidationResult validResult  = await productAddRequestValidator.ValidateAsync(productAddRequest);

                if(!validResult.IsValid)
                {
                  Dictionary<string, string[]> errors = validResult.Errors.GroupBy(temp => temp.PropertyName)
                                .ToDictionary(grp => grp.Key,
                                grp => grp.Select(err => err.ErrorMessage).ToArray());

                    return Results.ValidationProblem(errors);
                }

                ProductResponse? addedProduct = await productsService.AddProduct(productAddRequest);
                if(addedProduct!=null)
                {
                    return Results.Created($"/api/products/search/product-id/{addedProduct.ProductID}",
                        addedProduct);
                }
                else
                {
                    return Results.Problem("Error in adding product");
                }
            });


            // Put /api/products
            app.MapPut("/api/products", async (IProductsService
               productsService, IValidator<ProductUpdateRequest> productAddRequestValidator,
               ProductUpdateRequest productUpdateRequest) =>
            {
                // Validate the product add req

                ValidationResult validResult = await 
                productAddRequestValidator.ValidateAsync(productUpdateRequest);

                if (!validResult.IsValid)
                {
                    Dictionary<string, string[]> errors = validResult.Errors.GroupBy(temp => temp.PropertyName)
                                  .ToDictionary(grp => grp.Key,
                                  grp => grp.Select(err => err.ErrorMessage).ToArray());

                    return Results.ValidationProblem(errors);
                }

                ProductResponse? updatedProduct = await productsService.UpdateProduct(productUpdateRequest);
                if (updatedProduct != null)
                {
                    return Results.Ok(updatedProduct);
                }
                else
                {
                    return Results.Problem("Error in Updating product");
                }
            });

            // Delete /api/products/id
            app.MapDelete("/api/products/{productID:guid}", async (IProductsService
               productsService, Guid productID ) =>
            {

                bool deleteResponse = await productsService.DeleteProduct(productID);

               if(deleteResponse)
                {
                    return Results.Ok(true);
                }
                else
                {
                    return Results.Problem("Error in deleting product");
                }
            });


            return app;
        }
    }
}
