using System.Text.Json.Serialization;

namespace MintBlockchainBotConsoleUI.Models;
public class Embed
{
    [JsonPropertyName("title")]
    public string Title { get; set; }

    [JsonPropertyName("description")]
    public string Description { get; set; }

    [JsonPropertyName("color")]
    public int Color { get; set; }

    [JsonPropertyName("fields")]
    public List<Field> Fields { get; set; }

    [JsonPropertyName("footer")]
    public Footer Footer { get; set; }

    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; set; } = DateTime.Now;
}

public class Field
{
    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("value")]
    public string Value { get; set; }

    [JsonPropertyName("inline")]
    public bool Inline { get; set; } = false;
}

public class Footer
{
    [JsonPropertyName("text")]
    public string Text { get; set; } = "Tarih";
}

public class DiscordWebHookMessageContent
{
    [JsonPropertyName("content")]
    public object Content { get; set; }

    [JsonPropertyName("embeds")]
    public List<Embed> Embeds { get; set; }

    [JsonPropertyName("username")]
    public string Username { get; set; } = "MintForest Bot";

    [JsonPropertyName("attachments")]
    public List<object> Attachments { get; set; }
}