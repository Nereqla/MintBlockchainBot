using MintBlockchainWrapper;
using MintBlockchainWrapper.Helpers;
using MintBlockchainWrapper.Models;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.Web3;
using System.Text;
using System.Text.Json;

namespace MintBlockchainBot;
public class MintForest
{
    private HttpHelper _httpHelper;
    private Authorization _authorization;
        private string _bearerToken = String.Empty;
    private string _publicKey = String.Empty;
    private bool _isAuthenticated = false;
    public MintForest(string privateKey, Proxy proxy = null)
    {
        _httpHelper = new HttpHelper(proxy);
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

        using (var content = new StringContent(JsonSerializer.Serialize(new { energy = energyAmount, address = _publicKey }), Encoding.UTF8, "application/json"))
        {
            using (HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, Config.InjectMeEndpoint))
            {
                request.Content = content;
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

    public async Task<NormalClaim> Claim(ClaimRequestBody requestBody)
    {
        if (!_isAuthenticated) return null;

        using (var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json"))
        {
            using (HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, Config.ClaimEndpoint))
            {
                request.Content = content;
                using (var response = await _httpHelper.Client.SendAsync(request))
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    {
                        throw new Exception("#401 Unauthorized Claim()");
                    }
                    if (response.IsSuccessStatusCode)
                    {
                        return JsonSerializer.Deserialize<NormalClaim>(responseContent);
                    }
                    else
                    {
                        throw new Exception($"{response.StatusCode} Ele alınmayan kritik hata!\nResponse: {response.Content}");
                    }
                }
            }
        }
    }

    public async Task<LeaderboardResponse> GetLeaderboard(int pageID)
    {
        if (!_isAuthenticated) return null;

        using (HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, Config.GetLeaderboardEndpoint(pageID)))
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
                    return JsonSerializer.Deserialize<LeaderboardResponse>(responseContent);
                }
                else
                {
                    throw new Exception($"{response.StatusCode} Ele alınmayan kritik hata!\nResponse: {response.Content}");
                }
            }
        }
    }

    public async Task<EnergyListResponse> GetStealEnergyList(int userID)
    {
        if (!_isAuthenticated) return null;

        using (HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, Config.GetStealEnergyListEndpoint(userID)))
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
                    return JsonSerializer.Deserialize<EnergyListResponse>(responseContent);
                }
                else
                {
                    throw new Exception($"{response.StatusCode} Ele alınmayan kritik hata!\nResponse: {response.Content}");
                }
            }
        }
    }

    public async Task<ChainResponse<T>> GetTransactionData<T>(TransactionType type, int userID = 0)
    {
        if (!_isAuthenticated) return null;

        using (HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, Config.GetTransactionDataEndpoint(type,userID)))
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
                   return JsonSerializer.Deserialize<ChainResponse<T>>(responseContent);
                }
                else
                {
                    throw new Exception($"{response.StatusCode} Ele alınmayan kritik hata!\nResponse: {response.Content}");
                }
            }
        }
    }

    public async Task<ActivityResponse> GetActivity(int treeID)
    {
        if (!_isAuthenticated) return null;

        using (HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, Config.GetActivityEndPoint(treeID)))
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
                    return JsonSerializer.Deserialize<ActivityResponse>(responseContent);
                }
                else
                {
                    throw new Exception($"{response.StatusCode} Ele alınmayan kritik hata!\nResponse: {response.Content}");
                }
            }
        }
    }


    public async Task<bool> PerformContractAction(string txData)
    {
        try
        {
            var rpcUrl = "https://rpc.mintchain.io";
            var account = new Nethereum.Web3.Accounts.Account(_authorization.PrivateKey);
            var web3 = new Web3(account, rpcUrl);
            var contractAddress = "0x12906892AaA384ad59F2c431867af6632c68100a";

            string transactionHash = await web3.TransactionManager.SendTransactionAsync(new TransactionInput
            {
                From = account.Address,
                To = contractAddress,
                Data = txData,
                Gas = new Nethereum.Hex.HexTypes.HexBigInteger(100000),
                GasPrice = new Nethereum.Hex.HexTypes.HexBigInteger(Web3.Convert.ToWei(0.0001, Nethereum.Util.UnitConversion.EthUnit.Gwei)),
                Value = new Nethereum.Hex.HexTypes.HexBigInteger(0)
            });

            var receipt = await web3.TransactionManager.TransactionReceiptService.PollForReceiptAsync(transactionHash);
            if (receipt != null)
            {
                return receipt.Status.Value == 0 ? false : true;
            }
            else    
            {
                Console.WriteLine($"## Başarısız işlem hash: {receipt}");
                return false;
            }

        }
        catch (Exception ex)
        {
            Console.WriteLine("public async Task<bool> PerformContractSteal(string txData) İMZALI METHODDA BİR HATA MEYDANA GELDİ \n");
            Console.WriteLine(ex.ToString() + " \n");
            return false;
        }

    }

    public async Task<bool> SimulateContractAction(string txData)
    {
        try
        {
            var rpcUrl = "https://rpc.mintchain.io";
            var account = new Nethereum.Web3.Accounts.Account(_authorization.PrivateKey);
            var web3 = new Web3(account, rpcUrl);
            var contractAddress = "0x12906892AaA384ad59F2c431867af6632c68100a";

            var transactionInput = new TransactionInput
            {
                From = account.Address,
                To = contractAddress,
                Data = txData,
                Gas = new Nethereum.Hex.HexTypes.HexBigInteger(100000),
                GasPrice = new Nethereum.Hex.HexTypes.HexBigInteger(Web3.Convert.ToWei(0.0001, Nethereum.Util.UnitConversion.EthUnit.Gwei)),
                Value = new Nethereum.Hex.HexTypes.HexBigInteger(0)
            };

            var result = await web3.Eth.Transactions.Call.SendRequestAsync(transactionInput);

            return !string.IsNullOrEmpty(result);
        }
        catch (Exception)
        {
            return false;
        }
    }


    public TimeSpan GetNextDailyTime() => (DateTime.UtcNow.Date.AddDays(1).Date - DateTime.UtcNow);
}

