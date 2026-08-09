using eCommerce.ProductsService.DataAccessLayer;

using eCommerce.ProductsService.BusinessLogicLayer;
using FluentValidation.AspNetCore;
using eCommerce.ProductsMicroService.API.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Add services from data access layer,
// remember to pass the configuration (for conn string reading)

builder.Services.AddDataAccessLayer(builder.Configuration);

// Add services from business logic layer

builder.Services.AddBusinessLogicLayer();
builder.Services.AddControllers();
//Fluent validations
builder.Services.AddFluentValidationAutoValidation();

var app = builder.Build();

app.UseExceptionHandlingMiddleware();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
