using System.Text.Json.Serialization;

namespace MintBlockchainWrapper.Models;
public class LoginRequestBody
{
    [JsonPropertyName("address")]
    public string Address { get; set; }

    [JsonPropertyName("signature")]
    public string Signature { get; set; }

    [JsonPropertyName("message")]
    public string Message { get; set; }
}
