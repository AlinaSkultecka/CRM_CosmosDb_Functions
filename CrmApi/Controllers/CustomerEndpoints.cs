using CrmApi.Core.Entities;
using CrmApi.Data;

namespace CrmApi.Controllers;

public static class CustomerEndpoints
{
    // This method defines all the API endpoints related to customers
    public static void MapCustomerEndpoints(this WebApplication app)
    {
        // ---------------------- GET CUSTOMERS ----------------------
        // Get all customers
        app.MapGet("/customers", async (CustomerRepository repository) =>
        {
            return Results.Ok(await repository.GetAllAsync());
        });

        // Get one customer by id
        app.MapGet("/customers/{id}", async (string id, CustomerRepository repository) =>
        {
            Customer? customer = await repository.GetByIdAsync(id);

            if (customer is null)
            {
                return Results.NotFound("Customer not found");
            }

            return Results.Ok(customer);
        });

        // ----------------------- CREATE CUSTOMER -------------------------
        app.MapPost("/customers", async (Customer customer, CustomerRepository repository) =>
        {
            if (string.IsNullOrWhiteSpace(customer.ResponsibleSeller.Email))
            {
                return Results.BadRequest("Customer must have a responsible seller with email.");
            }

            Customer createdCustomer = await repository.AddAsync(customer);

            return Results.Created($"/customers/{createdCustomer.Id}", createdCustomer);
        });

        // ----------------------- UPDATE CUSTOMER -------------------------
        app.MapPut("/customers/{id}", async (string id, Customer updatedCustomer, CustomerRepository repository) =>
        {
            Customer? customer = await repository.UpdateAsync(id, updatedCustomer);

            if (customer is null)
            {
                return Results.NotFound("Customer not found");
            }

            return Results.Ok(customer);
        });

        // ----------------------- DELETE CUSTOMER -------------------------
        app.MapDelete("/customers/{id}", async (string id, CustomerRepository repository) =>
        {
            bool deleted = await repository.DeleteAsync(id);

            if (!deleted)
            {
                return Results.NotFound("Customer not found");
            }

            return Results.Ok("Customer deleted");
        });

        // ----------------------- SEARCH CUSTOMERS -------------------------
        // Search by customer name
        app.MapGet("/customers/search/name/{name}", async (string name, CustomerRepository repository) =>
        {
            var result = await repository.SearchByCustomerNameAsync(name);

            return Results.Ok(result);
        });

        // Search by responsible seller name
        app.MapGet("/customers/search/seller/{sellerName}", async (string sellerName, CustomerRepository repository) =>
        {
            var result = await repository.SearchBySellerNameAsync(sellerName);

            return Results.Ok(result);
        });
    }
}
