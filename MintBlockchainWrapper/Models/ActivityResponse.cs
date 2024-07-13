using System.Text.Json.Serialization;

namespace MintBlockchainWrapper.Models;

public partial class ActivityResponse
{
    [JsonPropertyName("code")]
    public int Code { get; set; }

    [JsonPropertyName("result")]
    public List<Activity> Activities { get; set; }

    [JsonPropertyName("msg")]
    public string Msg { get; set; }
}

public partial class Activity
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("uid")]
    public int Uid { get; set; }

    [JsonPropertyName("type")]
    public string Type { get; set; }

    [JsonPropertyName("amount")]
    public int Amount { get; set; }

    [JsonPropertyName("taskId")]
    public object TaskId { get; set; }

    [JsonPropertyName("info")]
    public string Info { get; set; }

    [JsonPropertyName("claimAt")]
    public DateTime ClaimAt { get; set; }

    [JsonPropertyName("children")]
    public List<object> Children { get; set; }
}
