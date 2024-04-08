using MintBlockchainWrapper.Models;

namespace MintBlockChainBotConsoleUI.Models;
public class Credential
{
    public string AccountName { get; set; }
    public string WalletPrivateKey { get; set; }
}

public class ApplicationSettings
{
    public string WebHookURL { get; set; }
    public List<Credential> Credentials { get; set; }
}
