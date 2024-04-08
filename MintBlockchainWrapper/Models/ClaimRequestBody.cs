using System.Text.Json.Serialization;

namespace MintBlockchainWrapper.Models;

public class ClaimRequestBody
{
    [JsonPropertyName("uid")]
    public List<object> Uid { get; set; }

    [JsonPropertyName("amount")]
    public int Amount { get; set; }

    [JsonPropertyName("includes")]
    public List<object> Includes { get; set; }

    [JsonPropertyName("type")]
    public string Type { get; set; }

    [JsonPropertyName("freeze")]
    public bool Freeze { get; set; }

    [JsonPropertyName("id")]
    public string Id { get; set; }
}