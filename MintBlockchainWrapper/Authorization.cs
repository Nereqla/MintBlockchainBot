using MintBlockchainWrapper.Helpers;
using MintBlockchainWrapper.Models;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace MintBlockchainWrapper;
internal class Authorization
{
    private string _privateKey { get; set; }
    private string _publicKey { get; set; }
    private LoginResponse _loginResponse { get; set; }
    private HttpHelper _httpHelper;
    public Authorization(string privateKey, HttpHelper httpHelper)
    {
        _privateKey = privateKey;
        _publicKey = NethereumHelper.GetPublicKey(_privateKey);
        _httpHelper = httpHelper;

    }

    public async Task<string>? GetBearerToken()
    {
        var token = CheckCacheFile();
        if(!String.IsNullOrEmpty(token)) return token;

        try
        {
            token = await GetTokenFromSite();
        }
        catch(Exception ex)
        {
            Console.WriteLine($"Hata: GetTokenFromSite() => {ex.Message}");
        }

        if(!String.IsNullOrEmpty(token)) return token;

        return null;
    }

    private async Task<string> GetTokenFromSite()
    {
        var nonce = GenerateNonce();
        var msg = GetMessage(_publicKey,nonce);
        var requestbody = new LoginRequestBody()
        {
            Address = _publicKey,
            Message = msg,
            Signature = NethereumHelper.GetSignature(msg, _privateKey)
        };

        using (var content = new StringContent(JsonSerializer.Serialize(requestbody)))
        {
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            using (HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, Config.LoginEndpoint))
            {
                request.Content = content;
                using (var response = await _httpHelper.Client.SendAsync(request))
                {
                    if (response.IsSuccessStatusCode)
                    {
                        var responseContent = await response.Content.ReadAsStringAsync();
                        var LoginResponse = JsonSerializer.Deserialize<LoginResponse>(responseContent);
                        if (LoginResponse.Result.User.TreeId == null)
                        {
                            throw new Exception("Bu cüzdana ait oluşturulmuş bir hesap yok!");
                        }
                        if (LoginResponse is not null && LoginResponse.Msg.ToLower().Contains("ok"))
                        {
                            _loginResponse = LoginResponse;
                            CacheManager.SaveCache(new Cache() { 
                                WalletPublic=_publicKey,
                                LoginResponse = LoginResponse,
                                CreateTime = DateTime.Now.AddDays(6)
                            });
                            return _loginResponse.Result.AccessToken;
                        }
                        else return null;
                    }
                    else return null;
                }
            }
        }
    }

    public async Task RemoveFromCacheUnauthorizedToken(string token)
    {
        var caches = CacheManager.GetCache();
        if (caches != null)
        {
            var cache = caches.Find(cache => cache.LoginResponse.Result.AccessToken == token);
            if (cache is not null)
            {
                CacheManager.RemoveCache(cache);
            }
        }
    }

    private string? CheckCacheFile()
    {
        var caches = CacheManager.GetCache();
        if (caches != null) 
        {
            var cache = caches.Find(cache => cache.WalletPublic == _publicKey);
            if (cache is not null)
            {
                if (cache.CreateTime < DateTime.Now)
                {
                    CacheManager.RemoveCache(cache);
                    return null;
                }

                _loginResponse = cache.LoginResponse;
                return cache.LoginResponse.Result.AccessToken;
            }
        }
        return null;
    }
    string GenerateNonce()
    {
        using (RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider())
        {
            byte[] nonceBytes = new byte[7];
            rng.GetBytes(nonceBytes);

            StringBuilder nonceBuilder = new StringBuilder();
            foreach (byte b in nonceBytes)
            {
                nonceBuilder.Append((b % 10).ToString());
            }

            return nonceBuilder.ToString();
        }
    }
    string GetMessage(string publicKey, string nonce)
    {
        return String.Format($"You are participating in the Mint Forest event: \n {publicKey}\n\nNonce: {nonce}");
    }
}
