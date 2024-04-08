using System.Text.Json.Serialization;

namespace MintBlockchainWrapper.Models;
public class ClaimResponse
{
    [JsonPropertyName("code")]
    public int Code { get; set; }

    [JsonPropertyName("msg")]
    public string Msg { get; set; }
}
