using System.Text.Json.Serialization;

namespace MintBlockchainWrapper.Models;
public class UserTree
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("treeId")]
    public int TreeId { get; set; }

    [JsonPropertyName("address")]
    public string Address { get; set; }

    [JsonPropertyName("ens")]
    public string Ens { get; set; }

    [JsonPropertyName("amount")]
    public ulong Amount { get; set; }

    [JsonPropertyName("role")]
    public string Role { get; set; }

    [JsonPropertyName("rank")]
    public int Rank { get; set; }
}

public class LeaderboardResponse
{
    [JsonPropertyName("code")]
    public int Code { get; set; }

    [JsonPropertyName("result")]
    public List<UserTree> Users { get; set; }

    [JsonPropertyName("msg")]
    public string Msg { get; set; }
}

