using System.Text.Json.Serialization;

namespace MintBlockchainWrapper.Models;
public class ClaimResponse
{
    [JsonPropertyName("code")]
    public int? Code { get; set; }

    [JsonPropertyName("result")]
    public ClaimResult? Result { get; set; }

    [JsonPropertyName("msg")]
    public string? Msg { get; set; }
}

public class ClaimResult
{
    [JsonPropertyName("amount")]
    public int? Amount { get; set; }

    [JsonPropertyName("collected")]
    public int? Collected { get; set; }
}
