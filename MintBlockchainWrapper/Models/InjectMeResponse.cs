using System.Text.Json.Serialization;

namespace MintBlockchainWrapper.Models;
public class InjectMeResponse
{
    [JsonPropertyName("code")]
    public int? Code { get; set; }

    [JsonPropertyName("msg")]
    public string? Msg { get; set; }

    [JsonPropertyName("result")]
    public bool? Result { get; set; }
}
