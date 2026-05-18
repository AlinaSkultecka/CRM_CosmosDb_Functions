using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = FunctionsApplication.CreateBuilder(args);

// Configure Azure Functions isolated worker
builder.ConfigureFunctionsWebApplication();

// Build and run the Function app
builder.Build().Run();