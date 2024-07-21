using System.Text.Json.Serialization;

namespace MintBlockchainWrapper.Models;
public class NormalClaim
{
    [JsonPropertyName("code")]
    public int? Code { get; set; }

    [JsonPropertyName("result")]
    public int? Result { get; set; }

    [JsonPropertyName("msg")]
    public string Msg { get; set; }
}
