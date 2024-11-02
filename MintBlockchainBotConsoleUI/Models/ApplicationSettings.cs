using MintBlockchainWrapper.Models;

namespace MintBlockChainBotConsoleUI.Models;
public class Credential
{
    public string AccountName { get; set; }
    public string WalletPrivateKey { get; set; }
    public bool StealPointsOnThisAccount { get; set; } = true;
    public bool CollectDailyOnChain { get; set; } = true;
    public Proxy? Proxy { get; set; }
}

public class ApplicationSettings
{
    public string WebHookURL { get; set; }
    public string ErrorWebHookURL { get; set; }
    public List<Credential> Credentials { get; set; }
}
