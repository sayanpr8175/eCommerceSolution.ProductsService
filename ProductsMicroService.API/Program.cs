using eCommerce.ProductsService.DataAccessLayer;

using eCommerce.ProductsService.BusinessLogicLayer;
using FluentValidation.AspNetCore;
using eCommerce.ProductsMicroService.API.Middleware;
using eCommerce.ProductsMicroService.API.APIEndpoints;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services from data access layer,
// remember to pass the configuration (for conn string reading)

builder.Services.AddDataAccessLayer(builder.Configuration);

// Add services from business logic layer

builder.Services.AddBusinessLogicLayer();
builder.Services.AddControllers();
//Fluent validations
builder.Services.AddFluentValidationAutoValidation();

// add model binder to bind enum type

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

// Add swagger

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add cors

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(builder =>
    {
        builder.WithOrigins("http://localhost:4200")
        .AllowAnyMethod().AllowAnyHeader();
    });
});

var app = builder.Build();

app.UseExceptionHandlingMiddleware();

app.UseRouting();

// Enable cors

app.UseCors();

// Enabling swagger UI

app.UseSwagger();
app.UseSwaggerUI();


// Auth
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapProductAPIEndpoints();

app.MapControllers();

app.Run();
