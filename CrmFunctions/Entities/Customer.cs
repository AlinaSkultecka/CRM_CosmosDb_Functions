using CrmFunctions.Entities;
using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace CrmFunctions.Entities;

public class Customer
{
    // Cosmos DB document id must be lowercase "id"
    [JsonProperty(PropertyName = "id")]
    [JsonPropertyName("id")]
    public string Id { get; set; } = "";

    public string Name { get; set; } = "";
    public string Title { get; set; } = "";
    public string Phone { get; set; } = "";
    public string Email { get; set; } = "";
    public string Address { get; set; } = "";

    public Seller ResponsibleSeller { get; set; } = new Seller();
}