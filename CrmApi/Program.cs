using CrmApi.Controllers;
using CrmApi.Data;
using Microsoft.Azure.Cosmos;

var builder = WebApplication.CreateBuilder(args);

// Read Cosmos DB settings from appsettings.json
string endpoint = builder.Configuration["CosmosDb:Endpoint"]!;
string key = builder.Configuration["CosmosDb:Key"]!;
string databaseName = builder.Configuration["CosmosDb:DatabaseName"]!;
string containerName = builder.Configuration["CosmosDb:ContainerName"]!;

// Create CosmosClient.
// This client is used to communicate with Cosmos DB Emulator.
CosmosClient cosmosClient = new CosmosClient(endpoint, key);

// Create database if it does not already exist
Database database = await cosmosClient.CreateDatabaseIfNotExistsAsync(databaseName);

// Create container if it does not already exist.
// We use "/id" as partition key for beginner simplicity.
Container container = await database.CreateContainerIfNotExistsAsync(
    id: containerName,
    partitionKeyPath: "/id"
);

// Register the Cosmos container for dependency injection
builder.Services.AddSingleton(container);

// Register CustomerRepository so endpoints can use it
builder.Services.AddSingleton<CustomerRepository>();

var app = builder.Build();

// Start page to check that API is running
app.MapGet("/", () => "CRM API is running with Cosmos DB");

// Register all customer endpoints
app.MapCustomerEndpoints();

app.Run();