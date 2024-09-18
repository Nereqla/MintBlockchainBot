using MintBlockchainBotConsoleUI.Models;
using Org.BouncyCastle.Crypto.Generators;
using System.Net.Http.Headers;
using System.Text.Json;

namespace MintBlockchainBotConsoleUI.Helpers;
internal class DiscordWebHookManager
{
    public static bool State = false;

    private static string _discordWebHook;
    private static TimeSpan _sendDelay = TimeSpan.FromSeconds(5);

    private static HttpClient _client = new HttpClient();
    public static string DiscordWebHook
    {
        get => _discordWebHook;

        set
        {
            if (!String.IsNullOrEmpty(value))
            {
                _discordWebHook = value;
                State = true;
            }
            else
            {
                _discordWebHook = value;
                State = false;
            }

        }
    }

    public static Queue<DiscordMessage> MessageQueue = new Queue<DiscordMessage>();

    public static async Task DiscordLogic()
    {
        while (true)
        {
            if (State)
            {
                List<DiscordMessage> errorqueue = new List<DiscordMessage>();
                while (MessageQueue.TryDequeue(out DiscordMessage message))
                {
                    if (message.IsError)
                    {
                        errorqueue.Add(message);
                    }
                    else
                    {
                        await SendInformationMessage(message);
                        await Task.Delay(_sendDelay);
                    }
                }
                if (errorqueue.Count > 0)
                    await SendErrorMessageQueue(errorqueue);
            }
            await Task.Delay(TimeSpan.FromSeconds(10));
        }
    }

    private static async Task SendInformationMessage(DiscordMessage message)
    {
        var messageContent = new DiscordWebHookMessageContent();
        messageContent.Embeds = new List<Embed>();
        var embed = new Embed();
        embed.Title = "Bilgilendirme";
        embed.Description = message.AccountName;
        embed.Color = 65280; // green
        embed.Fields = new List<Field>();

        for (int i = 0; i < message.Messages.Count; i++)
        {
            embed.Fields.Add(new Field()
            {
                Name = $"{message.Messages[i]}",
                Value = ".",
            });
        }

        messageContent.Embeds.Add(embed);
        await SendMessage(messageContent);
    }

    // Discord Webhook maximum field count 25!
    private static int DiscordMaxFieldCount = 25;
    private static async Task SendErrorMessageQueue(List<DiscordMessage> errorsQueue)
    {
        var messageContent = new DiscordWebHookMessageContent();
        messageContent.Embeds = new List<Embed>();
        Embed embed = ErrorEmbedBuilder();

        int counter = 0;

        for (int i = 0; i < errorsQueue.Count; i++)
        {
            if (counter == DiscordMaxFieldCount)
            {
                await SendMessage(messageContent);
                embed = ErrorEmbedBuilder();

                await Task.Delay(_sendDelay);
                counter = 0;
            }

            embed.Fields.Add(new Field()
            {
                Name = errorsQueue[i].AccountName,
                Value = ErrorMessagesStringBuilder(errorsQueue[i].Messages),
            });
            counter++;
        }
        if (messageContent.Embeds.Count > 0)
        {
            await SendMessage(messageContent);
        }
    }

    private static string ErrorMessagesStringBuilder(List<string> messages)
    {
        string oneLineMessage = "";
        foreach (var message in messages)
        {
            oneLineMessage = $"{message}\n";
        }
        return oneLineMessage.TrimEnd('\n');
    }

    private static Embed ErrorEmbedBuilder()
    {
        var embed =  new Embed();
        embed.Title = "KRİTİK BİR HATA OLUŞTU";
        embed.Description = "Mint Forest Bot";
        embed.Color = 16711680; // red
        embed.Fields = new List<Field>();
        return embed;
    }

    private static async Task SendMessage(DiscordWebHookMessageContent msgContent)
    {
        try
        {
            using (var content = new StringContent(JsonSerializer.Serialize(msgContent)))
            {
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                using (HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, _discordWebHook))
                {
                    request.Content = content;
                    var response = await _client.SendAsync(request);
                    response.EnsureSuccessStatusCode();
                }
            }
        }
        catch (Exception ex)
        {
            await Console.Out.WriteLineAsync("Discord'a mesaj gönderilirken hata oluştu!");
            await Console.Out.WriteLineAsync(ex.Message);
            State = false;
        }
    }
}
