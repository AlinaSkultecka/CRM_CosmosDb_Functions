using CrmApi.Data.Entities;
using CrmApi.Data.Interface;
using Microsoft.Azure.Cosmos;

namespace CrmApi.Data.Repo;

// This class is responsible for storing and managing customers in Cosmos DB
public class CustomerRepo : ICustomerRepo
{
    // Cosmos client instance
    private readonly CosmosClient _cosmosClient;
    private readonly Database _database;
    private readonly Container _container;

    public CustomerRepo(IConfiguration configuration)
    {
        // Read Cosmos DB settings from appsettings.json
        string endpointUri = configuration["CosmosDb:EndpointUri"]!;
        string primaryKey = configuration["CosmosDb:PrimaryKey"]!;
        string databaseName = configuration["CosmosDb:DatabaseName"]!;
        string containerName = configuration["CosmosDb:ContainerName"]!;

        // Create connection to Cosmos DB Emulator
        _cosmosClient = new CosmosClient(endpointUri, primaryKey);

        // Get the database
        _database = _cosmosClient.GetDatabase(databaseName);

        // Get the container
        _container = _database.GetContainer(containerName);
    }

    // -------------------- GET CUSTOMERS --------------------

    // Get all customers
    public async Task<List<Customer>> GetAllAsync()
    {
        var query = new QueryDefinition("SELECT * FROM c");

        var customers = new List<Customer>();

        using FeedIterator<Customer> resultSet = _container.GetItemQueryIterator<Customer>(query);

        while (resultSet.HasMoreResults)
        {
            FeedResponse<Customer> response = await resultSet.ReadNextAsync();
            customers.AddRange(response);
        }

        return customers;
    }

    // Get a customer by ID
    public async Task<Customer?> GetByIdAsync(string id)
    {
        try
        {
            ItemResponse<Customer> response = await _container.ReadItemAsync<Customer>(
                id: id,
                partitionKey: new PartitionKey(id)
            );

            return response.Resource;
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    // -------------------- CREATE CUSTOMER --------------------

    public async Task<Customer> AddAsync(Customer customer)
    {
        // Create a new unique ID for the customer
        customer.Id = Guid.NewGuid().ToString();

        try
        {
            ItemResponse<Customer> response = await _container.CreateItemAsync(
                item: customer,
                partitionKey: new PartitionKey(customer.Id)
            );

            return response.Resource;
        }
        catch (CosmosException ex)
        {
            Console.WriteLine("COSMOS ERROR");
            Console.WriteLine($"Status code: {ex.StatusCode}");
            Console.WriteLine($"Sub status code: {ex.SubStatusCode}");
            Console.WriteLine($"Message: {ex.Message}");

            throw;
        }
    }

    // -------------------- UPDATE CUSTOMER --------------------

    public async Task<Customer?> UpdateAsync(string id, Customer updatedCustomer)
    {
        Customer? existingCustomer = await GetByIdAsync(id);

        if (existingCustomer is null)
        {
            return null;
        }

        // Keep the same ID when updating
        updatedCustomer.Id = id;

        ItemResponse<Customer> response = await _container.ReplaceItemAsync(
            item: updatedCustomer,
            id: id,
            partitionKey: new PartitionKey(id)
        );

        return response.Resource;
    }

    // -------------------- DELETE CUSTOMER --------------------

    public async Task<bool> DeleteAsync(string id)
    {
        Customer? existingCustomer = await GetByIdAsync(id);

        if (existingCustomer is null)
        {
            return false;
        }

        await _container.DeleteItemAsync<Customer>(
            id: id,
            partitionKey: new PartitionKey(id)
        );

        return true;
    }

    // -------------------- SEARCH CUSTOMERS --------------------

    // Search customers by name
    public async Task<List<Customer>> SearchByCustomerNameAsync(string name)
    {
        var query = new QueryDefinition(
            "SELECT * FROM c WHERE CONTAINS(LOWER(c.Name), LOWER(@name))"
        )
        .WithParameter("@name", name);

        return await RunQueryAsync(query);
    }

    // Search customers by responsible seller's name
    public async Task<List<Customer>> SearchBySellerNameAsync(string sellerName)
    {
        var query = new QueryDefinition(
            "SELECT * FROM c WHERE CONTAINS(LOWER(c.ResponsibleSeller.Name), LOWER(@sellerName))"
        )
        .WithParameter("@sellerName", sellerName);

        return await RunQueryAsync(query);
    }

    // Helper method to run Cosmos DB queries
    private async Task<List<Customer>> RunQueryAsync(QueryDefinition query)
    {
        var customers = new List<Customer>();

        using FeedIterator<Customer> resultSet = _container.GetItemQueryIterator<Customer>(query);

        while (resultSet.HasMoreResults)
        {
            FeedResponse<Customer> response = await resultSet.ReadNextAsync();
            customers.AddRange(response);
        }

        return customers;
    }
}