using MintBlockchainWrapper;
using MintBlockchainWrapper.Helpers;
using MintBlockchainWrapper.Models;
using System.Text;
using System.Text.Json;

namespace MintBlockchainBot;
public class MintForest
{
    private HttpHelper _httpHelper = new HttpHelper();
    private Authorization _authorization;
    private string _bearerToken = String.Empty;
    private string _publicKey = String.Empty;
    private bool _isAuthenticated = false;
    public MintForest(string privateKey)
    {
        _authorization = new Authorization(privateKey, _httpHelper);
        _publicKey = NethereumHelper.GetPublicKey(privateKey);
    }

    public async Task<bool> Login()
    {
        _bearerToken = await _authorization.GetBearerToken();

        _httpHelper.Client.DefaultRequestHeaders.Add("Authorization", $"Bearer {_bearerToken}");

        if (await CheckBearer())
        {
            _isAuthenticated = true;
            return true;
        }

        await _authorization.RemoveFromCacheUnauthorizedToken(_bearerToken);
        _bearerToken = await _authorization.GetBearerToken();

        _httpHelper.Client.DefaultRequestHeaders.Remove("Authorization");
        _httpHelper.Client.DefaultRequestHeaders.Add("Authorization", $"Bearer {_bearerToken}");

        if (await CheckBearer())
        {
            _isAuthenticated = true;
            return true;
        }
        else
        {
            _isAuthenticated = false;
            return true;
        }
    }
    private async Task<bool> CheckBearer()
    {
        var response = await _httpHelper.Client.GetAsync(Config.UserInfoEndpoint, HttpCompletionOption.ResponseHeadersRead);
        return response.IsSuccessStatusCode;
    }

    public async Task<UserInfoResponse> GetUserInfo()
    {
        if (!_isAuthenticated) return null;

        using (HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, Config.UserInfoEndpoint))
        {
            using (var response = await _httpHelper.Client.SendAsync(request))
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    throw new Exception("#401 Unauthorized GetUserInfo()");
                }
                if (response.IsSuccessStatusCode)
                {
                    return JsonSerializer.Deserialize<UserInfoResponse>(responseContent);
                }
                else
                {
                    throw new Exception($"{response.StatusCode} Ele alınmayan kritik hata!\nResponse: {response.Content}");
                }
            }
        }
    }

    public async Task<InjectMeResponse> InjectMe(int energyAmount)
    {
        if (!_isAuthenticated) return null;

        using (var content = new StringContent(JsonSerializer.Serialize(new { energy = energyAmount, _publicKey }), Encoding.UTF8, "application/json"))
        {
            using (HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, Config.InjectMeEndpoint))
            {
                using (var response = await _httpHelper.Client.SendAsync(request))
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    {
                        throw new Exception("#401 Unauthorized InjectMe()");
                    }
                    if (response.IsSuccessStatusCode)
                    {
                        return JsonSerializer.Deserialize<InjectMeResponse>(responseContent);
                    }
                    else
                    {
                        throw new Exception($"{response.StatusCode} Ele alınmayan kritik hata!\nResponse: {response.Content}");
                    }
                }
            }
        }
    }

    public async Task<EnergyListResponse> GetEnergyList()
    {
        if (!_isAuthenticated) return null;

        using (HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, Config.EnergyListEndpoint))
        {
            using (var response = await _httpHelper.Client.SendAsync(request))
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    throw new Exception("#401 Unauthorized GetEnergyList()");
                }
                if (response.IsSuccessStatusCode)
                {
                    return JsonSerializer.Deserialize<EnergyListResponse>(responseContent);
                }
                else
                {
                    throw new Exception($"{response.StatusCode} Ele alınmayan kritik hata!\nResponse: {response.Content}");
                }
            }
        }
    }

    public async Task<ClaimResponse> Claim(ClaimRequestBody requestBody)
    {
        if (!_isAuthenticated) return null;

        using (var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json"))
        {
            using (HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, Config.ClaimEndpoint))
            {
                using (var response = await _httpHelper.Client.SendAsync(request))
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    {
                        throw new Exception("#401 Unauthorized Claim()");
                    }
                    if (response.IsSuccessStatusCode)
                    {
                        return JsonSerializer.Deserialize<ClaimResponse>(responseContent);
                    }
                    else
                    {
                        throw new Exception($"{response.StatusCode} Ele alınmayan kritik hata!\nResponse: {response.Content}");
                    }
                }
            }
        }
    }

    public async Task<TimeSpan> GetNextDailyTime() => (DateTime.Today.AddDays(1).Date - DateTime.UtcNow);
}



