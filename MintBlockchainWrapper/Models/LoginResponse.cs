using System.Text.Json.Serialization;

namespace MintBlockchainWrapper.Models;
public class Result
{
    [JsonPropertyName("access_token")]
    public string AccessToken { get; set; }

    [JsonPropertyName("user")]
    public User User { get; set; }
}

public class LoginResponse
{
    [JsonPropertyName("code")]
    public int Code { get; set; }

    [JsonPropertyName("result")]
    public Result Result { get; set; }

    [JsonPropertyName("msg")]
    public string Msg { get; set; }
}

public class User
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("treeId")]
    public int? TreeId { get; set; }

    [JsonPropertyName("address")]
    public string Address { get; set; }

    [JsonPropertyName("ens")]
    public object Ens { get; set; }

    [JsonPropertyName("energy")]
    public int Energy { get; set; }

    [JsonPropertyName("tree")]
    public int Tree { get; set; }

    [JsonPropertyName("inviteId")]
    public int? InviteId { get; set; }

    [JsonPropertyName("type")]
    public string Type { get; set; }

    [JsonPropertyName("stake_id")]
    public int StakeId { get; set; }

    [JsonPropertyName("nft_id")]
    public int NftId { get; set; }

    [JsonPropertyName("nft_pass")]
    public int NftPass { get; set; }

    [JsonPropertyName("signin")]
    public int Signin { get; set; }

    [JsonPropertyName("code")]
    public object Code { get; set; }

    [JsonPropertyName("twitter")]
    public string Twitter { get; set; }

    [JsonPropertyName("discord")]
    public object Discord { get; set; }

    [JsonPropertyName("status")]
    public string Status { get; set; }

    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; }
}

