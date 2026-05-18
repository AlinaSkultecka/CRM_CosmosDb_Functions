using CrmApi.Controllers;
using CrmApi.Data.Interface;
using CrmApi.Data.Repo;

var builder = WebApplication.CreateBuilder(args);

// Register CustomerRepo so endpoints can use it
builder.Services.AddSingleton<ICustomerRepo, CustomerRepo>();

var app = builder.Build();

// Start page to check that API is running
app.MapGet("/", () => "CRM API is running with Cosmos DB");

// Register all customer endpoints
app.MapCustomerEndpoints();

app.Run();