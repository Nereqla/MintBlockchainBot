namespace MintBlockchainBotConsoleUI.Models;
internal class DiscordMessage
{
    public string AccountName { get; set; }
    public List<string> Messages { get; set; }
    public bool IsError { get; set; }
}