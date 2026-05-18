using Newtonsoft.Json;

namespace CrmApi.Data.Entities;

// This class represents one customer document in Cosmos DB
public class Customer
{
    // Cosmos DB requires lowercase "id"
    [JsonProperty(PropertyName = "id")]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    public string Name { get; set; } = "";
    public string Title { get; set; } = "";
    public string Phone { get; set; } = "";
    public string Email { get; set; } = "";
    public string Address { get; set; } = "";

    // Every customer must have one responsible seller
    public Seller ResponsibleSeller { get; set; } = new Seller();
}
