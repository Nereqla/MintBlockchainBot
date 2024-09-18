using System.Text.Json.Serialization;

namespace MintBlockchainWrapper.Models;
public class ChainResponse<T>
{
    [JsonPropertyName("code")]
    public int? Code { get; set; }

    [JsonPropertyName("result")]
    public T? Result { get; set; }

    [JsonPropertyName("msg")]
    public string? Msg { get; set; }
}

public class StealResult
{
    [JsonPropertyName("tx")]
    public string Tx { get; set; }

    [JsonPropertyName("amount")]
    public int Amount { get; set; }

    [JsonPropertyName("collected")]
    public int Collected { get; set; }
}

public class DailyLoginResult
{
    [JsonPropertyName("energy")]
    public int Energy { get; set; }

    [JsonPropertyName("tx")]
    public string Tx { get; set; }
}
