using MintBlockchainBot;
using MintBlockchainBotConsoleUI.Helpers;
using MintBlockchainBotConsoleUI.Models;
using MintBlockchainWrapper.Models;

namespace MintBlockchainBotConsoleUI;
internal class Bot
{
    private string _accountName;
    private List<StealableUsers> _steableUsers;


    public MintForest _mfWrapper { get; set; }
    public Bot(MintForest wrapper, string accountName)
    {
        _mfWrapper = wrapper;
        _accountName = accountName;


        //TODO: steableusers json dosyasından yüklenecek, json dosyasına kaydedilecek.
        _steableUsers = new List<StealableUsers>();
    }

    private Random _rnd = new Random();
    public async Task Start()
    {

        bool tryReLoginOnce = true;
        while (true)
        {
            try
            {

                await StealPointsFromUsersLogic();

                var getEnergyResponse = await GetEnergyListAndClaim();

                var userInfo = await GetUserInfoAndInject();

                var nextCheck = _mfWrapper.GetNextDailyTime().
                    Add(TimeSpan.FromHours(_rnd.Next(3,9))).
                    Add(TimeSpan.FromMinutes(_rnd.Next(5,45)));


                int totalTreePoint = userInfo.Result.Tree + userInfo.Result.Energy;
                await Log($"Tree: {totalTreePoint}");
                await Log($"Toplama işlemi bitti, sıradaki toplama ertesi gün {DateTime.Now.Add(nextCheck).ToString("HH:mm:ss")} saatinde!");

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

                if (ex.Message.ToLower().Contains("unauthorized") && tryReLoginOnce)
                {
                    await Log("[Unauthorized!] - Tek defa olmak üzere, yeni token alınıyor.");
                    await _mfWrapper.Login();
                    tryReLoginOnce = false;
                    continue;
                }
                else
                {
                    await Log("[Ne oldu şimdi?] - Ele alınmamış, beklenmedik bir hata!");
                    await Log(ex.Message);
                    break;
                }
            }
        }
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


    private int CurrentStealCount = 0;
    private async Task StealPointsFromUsersLogic()
    {
        var userInfo = await _mfWrapper.GetUserInfo();
        if (userInfo is null)
        {
            throw new Exception("userInfo null olamaz!");
        }

        CurrentStealCount = userInfo.Result.StealCount;
        //if (CurrentStealCount >= 5)
        //{
        //    Log("Hesap tüm araklama işlemlerini bitirmiş.");
        //    return;
        //}

        DateTime leaderBoardCheckTime = DateTime.Today.AddHours(14).AddMinutes(30).AddSeconds(1); // 14:30
        DateTime stealTime = DateTime.Today.AddHours(15).AddSeconds(1); // 15:00

        if (DateTime.Now < leaderBoardCheckTime)
        {
            TimeSpan delay = leaderBoardCheckTime - DateTime.Now;
            Log($"Saat 14:30'a kadar bekleniyor: {delay}");
            await Task.Delay(delay);
            Log("Vakit geldi! Hadi henüz günlüklerini toplamamış oyuncuları bulalım.");
        }
        else Log("Saat 14:30'u geçmiş. Sıralamadaki oyuncular kontrol ediliyor.");

        var usersNotClaimedDaily = await FindUsersNotClaimedDaily();

        Log($"Henüz günlüğünü toplamamış {usersNotClaimedDaily.Count} kullanıcı bulundu.");

        if (DateTime.Now < stealTime)
        {
            TimeSpan delay = stealTime - DateTime.Now;
            Log($"Çalmak çırpmak için Saat 15:00'a kadar bekleniyor: {delay}");
            await Task.Delay(delay);
            Log("Çalma çırpma vakti geldi, haydi kolay gele!");
        }
        else Log("Çalma çırpma saati geçmiş bile! Elimizi çabuk tutalım!");


        var stealableUsers = await ScanUsersAmount(usersNotClaimedDaily);
        stealableUsers.Sort((x, y) => y.Amount.CompareTo(x.Amount));

        //start collecting

        foreach (var user in stealableUsers)
        {
            if (CurrentStealCount < 5)
            {
                while (true)
                {
                    // collect higher
                    var response = await _mfWrapper.ClaimSteal(user.UserId); // 503 gateway hatası alabilir, ele alınmalı.

                    if (response.Msg.Contains("5 times per day."))
                    {
                        CurrentStealCount = 5;
                        break;
                    }
                    else if (response.Msg.Contains("Frequent operations"))
                    {
                        //try again after delay
                        await Task.Delay(TimeSpan.FromSeconds(2));
                        continue;
                    }
                    else if (response.Msg.Contains("collected by someone else"))
                    {
                        continue;
                    }
                    else if (response.Msg.Contains("ok"))
                    {
                        CurrentStealCount++;
                        break;
                    }
                }
            }
            else break;
        }

        if (CurrentStealCount < 5)
        {
            Log("Sıralama tablosundaki tüm kullanıcılar bitti ama hala yeterince toplayamadık. Üzgünüm.");
        }
        else Log("5 de 5 çalma çırpma işlemi tamamlandı. Yarın tekrar görüşmek üzere.");
    }

    private bool collectIfAmmountHigher = true;
    private int minAmountToCollect = 3000;
    private async Task<List<StealableUsers>> ScanUsersAmount(List<NotClaimedUsers> usersNotClaimedDaily)
    {
        int counter = 1;
        int maxRetryInARow = 5;
        int retry = 0;
        List<StealableUsers> tempStealableUsers = new List<StealableUsers>();
        foreach (var notClaimedUser in usersNotClaimedDaily)
        {
            bool tryAgain = false;
            do
            {
                try
                {
                    var checkenergyList = await _mfWrapper.GetStealEnergyList(notClaimedUser.UserId);
                    if (checkenergyList.Result is not null && checkenergyList.Result.Count > 0)
                    {

                        int amount = checkenergyList.Result.First().Amount;
                        if (amount >= minAmountToCollect && collectIfAmmountHigher && CurrentStealCount < 5)
                        {
                            Log($"Wow! 3000 üstü puan toplandı! => Miktar: {amount}");
                            _mfWrapper.ClaimSteal(notClaimedUser.UserId); // ya toplayamazsa? Hata ele alınmalı.
                            CurrentStealCount++;
                            break;
                        }

                        tempStealableUsers.Add(new StealableUsers()
                        {
                            Amount = amount,
                            TreeId = notClaimedUser.TreeId,
                            UserId = notClaimedUser.UserId,
                        });
                    }
                    counter++;
                    await Task.Delay(600);
                    tryAgain = false;
                    retry = 0;
                }
                catch (Exception ex)
                {
                    if (retry <= maxRetryInARow)
                    {
                        var dflkgjdfg = ex;
                        tryAgain = true;
                        retry++;
                    }
                    else
                    {
                        Log($"[Eyvah!] - (SearchAmountInUsers) isimli method tek seferde {maxRetryInARow} hata sayısını aştı.");
                        tryAgain = false;
                        return null;
                    }
                }
            } while (tryAgain);
        }

        return tempStealableUsers;
    }

    private async Task<List<NotClaimedUsers>> FindUsersNotClaimedDaily()
    {
        List<NotClaimedUsers> tempUsersNotClaimedDaily = new List<NotClaimedUsers>();
        int pageCounter = 1;
        while (pageCounter <= 20)
        {
            var leaderboard = await _mfWrapper.GetLeaderboard(pageCounter);

            foreach (var user in leaderboard.Users)
            {
                var userActivities = await _mfWrapper.GetActivity(user.TreeId);
                var usersActivity = userActivities.Activities.First(x => x.Type.Contains("daily"));
                if (usersActivity.ClaimAt < DateTime.Today)
                {
                    tempUsersNotClaimedDaily.Add(new NotClaimedUsers() { 
                        TreeId = user.TreeId,
                        UserId = user.Id
                    });
                }
            }
            pageCounter += 1;
        }

        return tempUsersNotClaimedDaily;
    }

    private async Task Log(string msg) => await Console.Out.WriteLineAsync($"{DateTime.Now} - {_accountName}: {msg}");
}
