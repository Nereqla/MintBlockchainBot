using MintBlockchainBotConsoleUI.Models;
using System.Net.Http.Headers;
using System.Text.Json;

namespace MintBlockchainBotConsoleUI.Helpers;
internal class DiscordWebHookManager
{
    public static bool State = false;

    private static string _discordInformWebHook;
    private static string _discordErrorWebHook;
    private static TimeSpan _sendDelay = TimeSpan.FromSeconds(5);

    private static HttpClient _client = new HttpClient();
    public static string DiscordInformWebHook
    {
        get => _discordInformWebHook;

        set
        {
            if (!String.IsNullOrEmpty(value))
            {
                _discordInformWebHook = value;
                State = true;
            }
            else
            {
                _discordInformWebHook = value;
                State = false;
            }

        }
    }

    public static string DiscordErrorWebHook
    {
        get
        {
            if (String.IsNullOrEmpty(_discordErrorWebHook)) return _discordInformWebHook;
            else return _discordErrorWebHook;
        }

        set
        {
            if (!String.IsNullOrEmpty(value))
            {
                _discordErrorWebHook = value;
                State = true;
            }
            else
            {
                _discordErrorWebHook = value;
                State = false;
            }

        }
    }

    public static Queue<DiscordMessage> MessageQueue = new Queue<DiscordMessage>();

    public static async Task DiscordLogic()
    {
        while (true)
        {
            await Task.Delay(TimeSpan.FromSeconds(5));
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
            await Task.Delay(TimeSpan.FromSeconds(20));
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
                messageContent.Embeds.Add(embed);
                await SendMessage(messageContent,true);

                messageContent = new DiscordWebHookMessageContent();
                messageContent.Embeds = new List<Embed>();
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

        messageContent.Embeds.Add(embed);
        if (messageContent.Embeds.Count > 0)
        {
            await SendMessage(messageContent, true);
        }
    }

    private static string ErrorMessagesStringBuilder(List<string> messages)
    {
        string oneLineMessage = "";
        foreach (var message in messages)
        {
            oneLineMessage += $"{message}\n";
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

    private static async Task SendMessage(DiscordWebHookMessageContent msgContent, bool isErrorMessage = false)
    {
        string tempWebHook;
        if (isErrorMessage) tempWebHook = DiscordErrorWebHook;
        else tempWebHook = DiscordInformWebHook;

        try
        {
            using (var content = new StringContent(JsonSerializer.Serialize(msgContent)))
            {
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                using (HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, tempWebHook))
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
