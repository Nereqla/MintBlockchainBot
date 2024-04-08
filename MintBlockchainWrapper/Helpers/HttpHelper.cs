using MintBlockchainWrapper.Models;
using System.Net;

namespace MintBlockchainWrapper.Helpers;
internal class HttpHelper
{
    public HttpClient Client = new HttpClient();

    private Proxy? Proxy { get; set; }
    public HttpHelper()
    {
        SetHttpClient();
    }


    private void SetHttpClient()
    {
        HttpClientHandler handler = new HttpClientHandler() { AutomaticDecompression = DecompressionMethods.All };

        if (Proxy is not null && Proxy.Type != ProxyType.None && !string.IsNullOrWhiteSpace(Proxy.IP) && Proxy.Port > 0)
        {
            if (!string.IsNullOrWhiteSpace(Proxy.UserName) && !string.IsNullOrWhiteSpace(Proxy.Password))
            {
                var credentials = new NetworkCredential(Proxy.UserName, Proxy.Password);
                handler.Proxy = new WebProxy($"{Proxy.IP}:{Proxy.Port}", true, null, credentials);
            }
            else
            {
                handler.Proxy = new WebProxy(Proxy.IP, Proxy.Port);
            }
        }

        Client = new HttpClient(handler);
        Client.DefaultRequestHeaders.Add("User-Agent", Config.UserAgent);
        Client.DefaultRequestHeaders.Add("Accept", "application/json, text/plain, */*");
        Client.DefaultRequestHeaders.Add("Accept-Language", "en-US,en;q=0.5");
        Client.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate, br");
        Client.DefaultRequestHeaders.Add("Referer", Config.Referer);
        Client.DefaultRequestHeaders.Add("Origin", Config.Origin);
    }
}
