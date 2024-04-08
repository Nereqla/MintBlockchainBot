using MintBlockchainBot;
using MintBlockchainBotConsoleUI.Helpers;
using MintBlockchainBotConsoleUI.Models;
using MintBlockchainWrapper.Models;

namespace MintBlockchainBotConsoleUI;
internal class Bot
{
    private string _accountName;
    public MintForest _mfWrapper { get; set; }
    public Bot(MintForest wrapper, string accountName)
    {
        _mfWrapper = wrapper;
        _accountName = accountName;
    }

    private Random _rnd = new Random();
    public async Task Start()
    {
        bool tryReLoginOnce = true;
        while (true)
        {
            try
            {
                var getEnergyResponse = await GetEnergyListAndClaim();

                var userInfo = await GetUserInfoAndInject();

                var nextCheck = _mfWrapper.GetNextDailyTime().
                    Add(TimeSpan.FromHours(_rnd.Next(8,15))).
                    Add(TimeSpan.FromMinutes(_rnd.Next(5,45)));


                int totalTreePoint = userInfo.Result.Tree + userInfo.Result.Energy;
                await Log($"Tree: {totalTreePoint}");
                await Log($"Tüm iş bitti, sıradaki işlem ertesi gün {DateTime.Now.Add(nextCheck).ToString("HH:mm:ss")} saatinde!");

                if (DiscordWebHookManager.State)
                {
                    DiscordWebHookManager.MessageQueue.Enqueue(new DiscordMessage()
                    {
                        AccountName = _accountName,
                        IsError = false,
                        Messages = new List<string>()
                        {
                            $"__Tree:__|{totalTreePoint}",
                            $"__Sonraki İşlem:__| Yarın {DateTime.Now.Add(nextCheck).ToString("HH:mm:ss")} saatinde!"
                        },
                    });
                }

                await Task.Delay(nextCheck);
            }
            catch (Exception ex)
            {
                await Log("Beklenmedik hata!");
                await Log(ex.Message);
                if (ex.Message.ToLower().Contains("unauthorized") && tryReLoginOnce)
                {
                    await Log("Tek defa olmak üzere, yeni token alınıyor.");
                    await _mfWrapper.Login();
                    tryReLoginOnce = false;
                    continue;
                }
                
                break;
            }
        }
    }
    private async Task Log(string msg)
    {
        Console.WriteLine($"{DateTime.Now} - {_accountName}: {msg}");
    }

    private async Task<EnergyListResponse> GetEnergyListAndClaim()
    {
        await Log("Ödüller kontrol ediliyor..");

        var getEnergyResponse = await _mfWrapper.GetEnergyList();
        if (getEnergyResponse is null || getEnergyResponse.ErrorMessage is not null)
        {
            throw new Exception($"(getEnergyResponse) Kritik hata: {getEnergyResponse?.ErrorMessage}");
        }

        var daily = getEnergyResponse.Result.Find(x => x.Type.ToLower() == "daily");

        // TODO: Döngü kurulabilirdi ama EnergyList'in henüz 1'den fazla toplanabilir değer döndüğünü görmedim. İleride gerekirse eklenir.
        await Task.Delay(_rnd.Next(500, 2500));
        if (!daily.Freeze)
        {
            var claimResponse = await _mfWrapper.Claim(new ClaimRequestBody()
            {
                Uid = daily.Uid,
                Amount = daily.Amount,
                Includes = daily.Includes,
                Type = daily.Type,
                Freeze = daily.Freeze,
                Id = "500_",
            });
            if (claimResponse is null)
            {
                throw new Exception($"(injectMeResponse) Kritik hata");
            }
            await Log("Günlük ödül alındı!");
        }
        else await Log("Günlük ödül zaten alınmış!");

        return getEnergyResponse;
    }

    private async Task<UserInfoResponse> GetUserInfoAndInject()
    {
        var userInfo = await _mfWrapper.GetUserInfo();
        if (userInfo is null)
        { 
            throw new Exception("userInfo null olamaz!");
        }

        await Task.Delay(_rnd.Next(500, 2500));
        if (userInfo.Result.Energy > 0)
        {
            var injectMeResponse = await _mfWrapper.InjectMe(userInfo.Result.Energy);
            if (injectMeResponse is not null && injectMeResponse.Msg.Contains("ok"))
                Log($"{userInfo.Result.Energy} ME puanının tamamı aşılandı!");
            else
            {
                await Log($"(injectMeResponse) Kritik hata: {injectMeResponse.Msg}");
                throw new Exception(injectMeResponse.Msg);
            }
        }
        else Log("Aşılanacak hiç ME yok!");

        return userInfo;
    }
}
