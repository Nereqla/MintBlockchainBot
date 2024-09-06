using MintBlockchainWrapper.Models;
using System.Net;

namespace MintBlockchainWrapper.Helpers;
internal class HttpHelper
{
    public HttpClient Client = new HttpClient();

    private Proxy? _proxy { get; set; }
    public HttpHelper(Proxy proxy)
    {
        _proxy = proxy;
        SetHttpClient();
    }


    private void SetHttpClient()
    {
        HttpClientHandler handler = new HttpClientHandler() { AutomaticDecompression = DecompressionMethods.All };

        if (_proxy is not null && _proxy.Type != ProxyType.None && !string.IsNullOrWhiteSpace(_proxy.IP) && _proxy.Port > 0)
        {
            if (!string.IsNullOrWhiteSpace(_proxy.UserName) && !string.IsNullOrWhiteSpace(_proxy.Password))
            {
                var credentials = new NetworkCredential(_proxy.UserName, _proxy.Password);
                handler.Proxy = new WebProxy($"{_proxy.IP}:{_proxy.Port}", true, null, credentials);
            }
            else
            {
                handler.Proxy = new WebProxy(_proxy.IP, _proxy.Port);
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
