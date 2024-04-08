using System.Text.Json.Serialization;

namespace MintBlockchainWrapper.Models;

public class EnergyList
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
}

public class EnergyListResponse
{
    [JsonPropertyName("code")]
    public int Code { get; set; }

    [JsonPropertyName("result")]
    public List<EnergyList> Result { get; set; }

    [JsonPropertyName("msg")]
    public string? ErrorMessage { get; set; }
}