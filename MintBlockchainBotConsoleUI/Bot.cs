using MintBlockchainBot;
using MintBlockchainBotConsoleUI.Helpers;
using MintBlockchainBotConsoleUI.Models;
using MintBlockChainBotConsoleUI.Helpers;
using MintBlockchainWrapper.Models;

namespace MintBlockchainBotConsoleUI;
internal class Bot
{
    private string _accountName;
    private List<StealableUser> _steableUsers;

    public MintForest _mfWrapper { get; set; }
    public Bot(MintForest wrapper, string accountName)
    {
        _mfWrapper = wrapper;
        _accountName = accountName;


        //TODO: Stealableusers json dosyasından yüklenecek, json dosyasına kaydedilecek.
        _steableUsers = new List<StealableUser>();
    }

    private Random _rnd = new Random();
    public async Task Start()
    {
        bool tryReLoginOnce = true;
        while (true)
        {
            try
            {
                StealPointsFromUsersLogic();

                var getEnergyResponse = await GetEnergyListAndClaim();

                var userInfo = await GetUserInfoAndInject();

                var nextCheck = _mfWrapper.GetNextDailyTime().
                    Add(TimeSpan.FromHours(_rnd.Next(3, 9))).
                    Add(TimeSpan.FromMinutes(_rnd.Next(5, 45)));


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
                ExceptionLogger.Log(ex);
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
                Id = $"{daily.Amount}_",
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
        if (CurrentStealCount >= 5)
        {
            Log("Hesap tüm araklama işlemlerini bitirmiş.");
            return;
        }

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


        List<NotClaimedUsers> usersNotClaimedDaily = JsonFileManager.LoadNotClaimedLeaderboardUsersIfExists();

        if (usersNotClaimedDaily is null)
        {
            Log("Kayıtlı leaderboard listeli bulunamadı, kontrol edilecek.");
            usersNotClaimedDaily = await FindUsersNotClaimedDaily();
            JsonFileManager.SaveNotClaimedLeaderboardUsersToFile(usersNotClaimedDaily);
            Log("Kayıt başarılı.");
        }
        else Log("Kayıtlı liste içeri aktarıldı!");

        Log($"Henüz günlüğünü toplamamış {usersNotClaimedDaily.Count} kullanıcı bulundu.");

        if (DateTime.Now < stealTime)
        {
            TimeSpan delay = stealTime - DateTime.Now;
            Log($"Çalmak çırpmak için Saat 15:00'a kadar bekleniyor: {delay}");
            await Task.Delay(delay);
            Log("Çalma çırpma vakti geldi, haydi kolay gele!");
        }
        else Log("Çalma çırpma saati geçmiş bile! Elimizi çabuk tutalım!");


        List<StealableUser> stealableUsers = JsonFileManager.LoadStealableUsersIfExists();
        if (stealableUsers is null)
        {
            stealableUsers = await ScanUsersAmount(usersNotClaimedDaily);
        }
        stealableUsers.Sort((x, y) => y.Amount.CompareTo(x.Amount));

        //start collecting

        foreach (var steableUser in stealableUsers)
        {
            if (CurrentStealCount < 5)
            {
                await StealME(steableUser);
            }
            else break;
        }

        if (CurrentStealCount < 5)
        {
            Log("Sıralama tablosundaki tüm kullanıcılar bitti ama hala yeterince toplayamadık. Üzgünüm.");
        }
        else Log("5 de 5 çalma çırpma işlemi tamamlandı. Yarın tekrar görüşmek üzere.");


    }

    /// <summary>
    /// Bu method aynı zamanda eğer başarılı olursa StealCount'ı arttırmaktan da sorumlu.
    /// </summary>
    /// <param name="user"></param>
    /// <returns></returns>
    private async Task StealME(StealableUser user)
    {
        int maxRetries = 30;
        int counter = 1;

        // Sonsuz döngü olmasın diye.
        while (counter <= maxRetries)
        {
            // collect higher
            ClaimResponse tempResponse;
            try
            {
                tempResponse = await _mfWrapper.ClaimSteal(user.UserId);
            }
            catch (Exception ex)
            {
                ExceptionLogger.Log(ex);
                Log("[Hata] - Kaynak: StealME() - Mesajı" + ex.Message);
                Log("Tekrar DENENİYOR!!!");
                await Task.Delay(TimeSpan.FromSeconds(8));
                continue;
            }

            if (tempResponse.Msg.Contains("5 times per day."))
            {
                Log("5 times per day. Hakkın bitmiş!");
                CurrentStealCount = 5;
                break;
            }
            else if (tempResponse.Msg.Contains("Frequent operations"))
            {
                //try again after delay
                Log("Frequent operations. Tekrar denenecek!");
                await Task.Delay(TimeSpan.FromSeconds(12));
                continue;
            }
            else if (tempResponse.Msg.Contains("collected by someone else"))
            {
                Log("Bunu çoktan kapmışlar!");
                break;
            }
            else if (tempResponse.Msg.Contains("ok"))
            {
                Log($"User ID: {user.UserId} - Tree ID: {user.TreeId} - Amount: {user.Amount} | Başarıyla toplandı!");
                CurrentStealCount++;
                break;
            }
            else if (tempResponse.Msg.Contains("Invalid User"))
            {
                Log($"Bu kullanıcı bulunamadı! || User ID: {user.UserId} - Tree ID: {user.TreeId} - Amount: {user.Amount} | Bu kaçtı!");
                await Task.Delay(TimeSpan.FromSeconds(3));

                break;
            }
            else
            {
                Log("StealME() - TANIMLANAMAYAN HATA");
                Log($"\nMalum response:\n\n Msg:{tempResponse.Msg}\nStatus:{tempResponse.Code}\n\n");
                Log($"User ID: {user.UserId} - Tree ID: {user.TreeId} - Amount: {user.Amount} | Bu kaçtı!\n");
                break;
            }
        }
        if (counter > maxRetries) Log($"FindUsersNotClaimedDaily() Methodu genel tekrar sayısını aştı. ({maxRetries}) | Kaçırılan Amount: {user.Amount}");
        await Task.Delay(TimeSpan.FromSeconds(12));
    }

    private async Task<List<StealableUser>> ScanUsersAmount(List<NotClaimedUsers> usersNotClaimedDaily)
    {
        int retry = 0;
        int counter = 1;
        int maxRetryInARow = 15;
        int minAmountToCollect = 2000;
        bool collectIfAmmountHigher = true;

        List<StealableUser> tempStealableUsers = new List<StealableUser>();
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
                            Log($"Wow! {minAmountToCollect} üstü puan bulundu! => Miktar: {amount}");
                            await StealME(new StealableUser()
                            {
                                Amount = amount,
                                TreeId = notClaimedUser.TreeId,
                                UserId = notClaimedUser.UserId,
                            });
                            break;
                        }

                        tempStealableUsers.Add(new StealableUser()
                        {
                            Amount = amount,
                            TreeId = notClaimedUser.TreeId,
                            UserId = notClaimedUser.UserId,
                        });

                        JsonFileManager.SaveSteableUsersToFile(tempStealableUsers);
                        Log($"Listeye eklendi {amount} | => TreeId: {notClaimedUser.TreeId} - UserID: {notClaimedUser.UserId}");
                    }
                    counter++;
                    await Task.Delay(600);
                    tryAgain = false;
                    retry = 0;
                }
                catch (Exception ex)
                {
                    ExceptionLogger.Log(ex);
                    if (retry <= maxRetryInARow)
                    {
                        var dflkgjdfg = ex;
                        tryAgain = true;
                        retry++;
                        Log($"!deneme sayısı: {retry} ");
                        Log($"[ScanUsersAmount] - HATA! - Hata Mesajı: {ex.Message}");
                        await Task.Delay(TimeSpan.FromSeconds(8));
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
            int retryCount = 0;
            int maxRetryInARow = 20;
            while (retryCount <= maxRetryInARow)
            {
                try
                {
                    foreach (var user in leaderboard.Users)
                    {
                        var userActivities = await _mfWrapper.GetActivity(user.TreeId);
                        var usersActivity = userActivities.Activities.First(x => x.Type.Contains("daily"));
                        if (usersActivity.ClaimAt < DateTime.Today)
                        {
                            tempUsersNotClaimedDaily.Add(new NotClaimedUsers()
                            {
                                TreeId = user.TreeId,
                                UserId = user.Id
                            });
                        }
                        await Task.Delay(100);
                        
                    }
                    Log($"Leaderboard Sayfa {pageCounter} bitti.");
                    pageCounter += 1;
                    break;
                }
                catch (Exception ex)
                {
                    retryCount++;
                    ExceptionLogger.Log(ex);
                    Log("[HATA] - Kaynak: FindUsersNotClaimedDaily() - Mesajı:" + ex.Message);
                }
                
            }
        }

        return tempUsersNotClaimedDaily;
    }

    
    private async Task Log(string msg) => await Console.Out.WriteLineAsync($"{DateTime.Now} - {_accountName}: {msg}");
}
