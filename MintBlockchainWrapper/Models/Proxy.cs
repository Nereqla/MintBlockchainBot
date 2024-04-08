namespace MintBlockchainWrapper.Models;
public class Proxy
{
    public string IP { get; set; }
    public int Port { get; set; }
    public string UserName { get; set; }
    public string Password { get; set; }
    public ProxyType Type { get; set; }
}

public enum ProxyType
{
    None,
    Http,
    Https,
    //Socks4,
    //Socks5,
}