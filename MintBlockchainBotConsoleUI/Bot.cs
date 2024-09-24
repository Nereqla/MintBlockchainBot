using MintBlockchainBot;
using MintBlockchainBotConsoleUI.Helpers;
using MintBlockchainBotConsoleUI.Models;
using MintBlockChainBotConsoleUI.Helpers;
using MintBlockchainWrapper;
using MintBlockchainWrapper.Models;
using System.Net;
using System.Text.RegularExpressions;

namespace MintBlockchainBotConsoleUI;
internal class Bot
{
    private string _accountName;
    private List<StealableUser> _steableUsers;
    private bool _collectDailyOnChain = true;
    private int _minEnergyAmountToCollect = 5000;
    private int _maxDailyStealLimit = 8;
    private int _currentStealCount = 0;
    private Random _rnd = new Random();

    public MintForest _mfWrapper { get; set; }
    public Bot(MintForest wrapper, string accountName)
    {
        _mfWrapper = wrapper;
        _accountName = accountName;


        _steableUsers = new List<StealableUser>();
    }

    public async Task Start()
    {
        bool tryReLoginOnce = true;
        while (true)
        {
            try
            {
                ExceptionLogger.DeleteOldLogFiles();
                JsonFileManager.RemoveOldCacheFiles();

                var stealpoints = StealPointsFromUsersLogic();
                await GetEnergyListAndClaimDaily();

                var userInfo = await GetUserInfoAndInject();

                var nextCheck = _mfWrapper.GetNextDailyTime().
                    Add(TimeSpan.FromHours(_rnd.Next(3, 8))).
                    Add(TimeSpan.FromMinutes(_rnd.Next(5, 45)));


                int totalTreePoint = userInfo.Result.Tree + userInfo.Result.Energy;
                await Log($"Tree: {totalTreePoint}");
                await Log($"Toplama işlemi bitti");

                InformDiscord(new List<string>()
                {
                    $"Toplama işlemi bitti.",
                    $"__Tree:__|{totalTreePoint}",
                    $"__Sonraki İşlem:__| Yarın {DateTime.Now.Add(nextCheck).ToString("HH:mm:ss")} saatinde!"
                });

                await stealpoints;
                await Log("Çalma çırpma işlemi de bitti...");

                
                await Log($"Sıradaki toplama ertesi gün {DateTime.Now.Add(nextCheck).ToString("HH:mm:ss")} saatinde!");
                await Task.Delay(nextCheck);
            }
            catch (Exception ex)
            {
                InformDiscord(new List<string>()
                {
                    $"Kritik bir hata gerçekleşti!!",
                    $"{ex.Message.ToLower()}",
                    $"__ReLoginState:__ {tryReLoginOnce}",
                }, true);

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

    private async Task ClaimDailyOnChain()
    {
        await Log("Ödüller kontrol ediliyor..");

        var dailySignTX = await _mfWrapper.GetTransactionData<DailyLoginResult>(TransactionType.Signin, 0);

        if (!CheckTxDataFormat(dailySignTX.Result.Tx))
        {
            await Log("Günlük toplanırken bir hata oluştu, TxData isteği başarısız. ClaimDailyOnChain()");
            InformDiscord(new List<string>()
                {
                    $"Kritik bir hata gerçekleşti!!",
                    $"Yer: ClaimDailyOnChain()",
                    $"Hata: TxData kontrolü geçemedi... Günlük toplanamadı!",
                }, true);
            return;
        }

        if (await _mfWrapper.SimulateContractAction(dailySignTX.Result.Tx))
        {
            var isSuccess = await _mfWrapper.PerformContractAction(dailySignTX.Result.Tx);
            if (isSuccess)
            {
                Log($"Günlük Enerji: {dailySignTX.Result.Energy} | Başarıyla toplandı!");
            }
            else
            {
                await Log($"## Kontrat etkileşimi başarısız! Contract etkileşimi başarısız oldu  #!#@??? - ClaimDailyOnChain()");
                InformDiscord(new List<string>()
                {
                    $"Kritik bir hata gerçekleşti!!",
                    $"Yer: ClaimDailyOnChain()",
                    $"Hata: Kontrat etkileşimi başarısız! Contract etkileşimi başarısız oldu  #!#@???... Günlük toplanamadı!",
                }, true);
                return;
            }
        }
        else
        {
            await Log("Günlük toplanırken bir hata oluştu, Simule işlemi başarısız oldu!!! ClaimDailyOnChain()");
            InformDiscord(new List<string>()
                {
                    $"Kritik bir hata gerçekleşti!!",
                    $"Yer: ClaimDailyOnChain()",
                    $"Hata: Simulasyon başarısız oldu... Günlük toplanamadı!",
                }, true);
            return;
        }
    }

    private async Task<EnergyListResponse> GetEnergyListAndClaimDaily()
    {
        await Log("Ödüller kontrol ediliyor..");

        var getEnergyResponse = await _mfWrapper.GetEnergyList();
        if (getEnergyResponse is null || getEnergyResponse.ErrorMessage is not null)
        {
            throw new Exception($"(getEnergyResponse) Kritik hata: {getEnergyResponse?.ErrorMessage}");
        }

        var collectables = getEnergyResponse.Result;

        await Task.Delay(_rnd.Next(750, 1750));
        if (collectables.Count <= 0)
        {
            await Log("Toplanabilir her şey zaten toplanmış.");
            return getEnergyResponse;
        }

        if (_collectDailyOnChain)
        {
            var daily = collectables.FirstOrDefault(x => x.Type.ToLower() == "daily");

            if (daily is null)
            {
                await Log("Günlük bulunamadı WTF #1!!??");
            }
            else if (daily.Freeze)
            {
                await Log("Daily zaten toplanmış...");
            }
            else
            {
                collectables.Remove(daily);
                await ClaimDailyOnChain();
            }
        }

        foreach (var collectable in collectables)
        {
            if (!collectable.Freeze)
            {
                var claimResponse = await _mfWrapper.Claim(new ClaimRequestBody()
                {
                    Uid = collectable.Uid,
                    Amount = collectable.Amount,
                    Includes = collectable.Includes,
                    Type = collectable.Type,
                    Freeze = collectable.Freeze,
                    Id = $"{collectable.Amount}_",
                });
                if (claimResponse is null)
                {
                    throw new Exception($"(Claim) Kritik hata - 567889234");
                }
                else if (claimResponse.Msg.ToLower().Contains("ok"))
                {
                    await Log($"Toplandı: {collectable.Amount} | Type: {collectable.Type}");
                }
            }
        }

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
    
    private async Task StealPointsFromUsersLogic()
    {
        var userInfo = await _mfWrapper.GetUserInfo();
        if (userInfo is null)
        {
            throw new Exception("userInfo null olamaz!");
        }

        _currentStealCount = userInfo.Result.StealCount;
        if (_currentStealCount >= _maxDailyStealLimit)
        {
            Log("Hesap tüm araklama işlemlerini bitirmiş.");
            return;
        }

        DateTime leaderBoardCheckTime = DateTime.UtcNow.Date.AddHours(11).AddMinutes(30).AddSeconds(1); // 14:30
        DateTime stealTime = DateTime.UtcNow.Date.AddHours(12).AddSeconds(1); // 15:00

        if (DateTime.UtcNow < leaderBoardCheckTime)
        {
            TimeSpan delay = leaderBoardCheckTime - DateTime.UtcNow;
            Log($"Saat 14:30'a kadar bekleniyor: {delay}");
            await Task.Delay(delay);
            Log("Vakit geldi! Hadi henüz günlüklerini toplamamış oyuncuları bulalım.");
        }
        else Log("Saat 14:30'u geçmiş. Sıralamadaki oyuncular kontrol ediliyor.");

        List<RandomUser> usersNotClaimedDaily = JsonFileManager.LoadNotClaimedLeaderboardUsersIfExists(_accountName);

        if (usersNotClaimedDaily is null)
        {
            Log("Kayıtlı leaderboard listeli bulunamadı, kontrol edilecek.");
            usersNotClaimedDaily = await FindUsersNotClaimedDaily();
            JsonFileManager.SaveNotClaimedLeaderboardUsersToFile(usersNotClaimedDaily, _accountName);
            Log("Kayıt başarılı.");
        }
        else Log("Kayıtlı liste içeri aktarıldı!");

        Log($"Henüz günlüğünü toplamamış {usersNotClaimedDaily.Count} kullanıcı bulundu.");

        if (DateTime.UtcNow < stealTime)
        {
            TimeSpan delay = stealTime - DateTime.UtcNow;
            Log($"Çalmak çırpmak için Saat 15:00'a kadar bekleniyor: {delay}");
            await Task.Delay(delay);
            Log("Çalma çırpma vakti geldi, haydi kolay gele!");
        }
        else Log("Çalma çırpma saati geçmiş bile! Elimizi çabuk tutalım!");

        await ScanUsersAmount(usersNotClaimedDaily);

        if (_currentStealCount < _maxDailyStealLimit)
        {
            Log($"Sıralama tablosundaki tüm kullanıcılar bitti ama hala yeterince toplayamadık. Üzgünüm. Çalma Hakkı: {_currentStealCount}/{_maxDailyStealLimit}");
            InformDiscord(new List<string>()
            {
                $"Sıralama tablosundaki tüm kullanıcılar bitti ama hala yeterince toplayamadık. StealCount: {_currentStealCount}/{_maxDailyStealLimit}",
                $"BruteForce denenecek!",
            });

            Log("BruteForce zamanı! 1'den 10bine!");
            await TimeToBruteForce();

            if (_currentStealCount < _maxDailyStealLimit)
            {
                InformDiscord(new List<string>()
                {
                    $"Brute Force Bitti. Günlük çalma miktarını yine tamamlayamadık! HOW THE FUCK?",
                    $"StealCount: {_currentStealCount}/{_maxDailyStealLimit}"
                },true);
            }
            else InformDiscord(new List<string>()
                {
                    $"Brute Force Bitti. Başarıyla günlüğü tamamladık! StealCount: {_currentStealCount}/{_maxDailyStealLimit}",
                });
        }
        else
        {
            InformDiscord(new List<string>()
            {
                $"Başarıyla 8'de 8 çalma işlemi yapıldı.",
            });
        }
    }

    private async Task StealME(StealableUser user)
    {
        int maxRetries = 10;
        int counter = 1;

        while (counter++ <= maxRetries && _currentStealCount < 8)
        {
            ChainResponse<StealResult> stealResponse;
            try
            {
                stealResponse = await _mfWrapper.GetTransactionData<StealResult>(TransactionType.Steal, user.UserId);
            }
            catch (Exception ex)
            {
                ExceptionLogger.Log(ex);
                Log("[Hata] - Kaynak: StealME() - Mesajı" + ex.Message);
                Log("Tekrar DENENİYOR!!!");
                await Task.Delay(TimeSpan.FromSeconds(8));
                continue;
            }
            if (stealResponse.Msg is null) break;

            if (stealResponse.Result is not null && CheckTxDataFormat(stealResponse.Result.Tx) && stealResponse.Msg.Contains("ok"))
            {
                if (stealResponse.Result.Collected > _currentStealCount)
                {
                    Log($"Ramdeki StealCount, endpointten dönen ile eşitlendi. {_currentStealCount} => {stealResponse.Result.Collected}");
                    _currentStealCount = stealResponse.Result.Collected;
                    continue;
                }

                if (await _mfWrapper.SimulateContractAction(stealResponse.Result.Tx))
                {
                    var isSuccess = await _mfWrapper.PerformContractAction(stealResponse.Result.Tx);
                    if (isSuccess)
                    {
                        _currentStealCount++;
                        Log($"User ID: {user.UserId} - Tree ID: {user.TreeId} - Amount: {user.Amount} | Başarıyla toplandı! - Kalan Hak: {_currentStealCount}/8");
                        break;
                    }
                    else
                    {
                        await Log($"1 ## Kontrat etkileşimi başarısız! #!#@???");
                        await Log($"2 ## Kontrat etkileşimi başarısız! #!#@???");
                        break;
                    }
                }
                else
                {
                    await Log($"Simule edilen işlem hata döndü! - Kalan çalma hakkı: {_currentStealCount}/{_maxDailyStealLimit}");
                    counter += 4;
                    await Task.Delay(1500);
                    continue;
                }
            }

            if (stealResponse.Msg.Contains("8 times per day."))
            {
                Log("8 times per day. Hakkın bitmiş!");
                _currentStealCount = 8;
                break;
            }
            else if (stealResponse.Msg.Contains("Frequent operations"))
            {
                //try again after delay
                Log("Frequent operations. Tekrar denenecek!");
                await Task.Delay(TimeSpan.FromSeconds(15));
                continue;
            }
            else if (stealResponse.Msg.Contains("collected by someone else"))
            {
                Log("Bunu çoktan kapmışlar!");
                break;
            }
            else if (stealResponse.Msg.Contains("Invalid User"))
            {
                Log($"Bu kullanıcı bulunamadı! || User ID: {user.UserId} - Tree ID: {user.TreeId} - Amount: {user.Amount} | Bu kaçtı!");
                await Task.Delay(TimeSpan.FromSeconds(3));

                break;
            }
            else if (stealResponse.Msg.Contains("Temporarily unavailable"))
            {
                Log("Temporarily unavailable");
                await Task.Delay(TimeSpan.FromSeconds(8));
                continue;
            }
            else
            {
                Log("StealME() - TANIMLANAMAYAN HATA");
                Log($"\nMalum response:\n\n Msg:{stealResponse.Msg}\nStatus:{stealResponse.Code}\n\n");
                Log($"User ID: {user.UserId} - Tree ID: {user.TreeId} - Amount: {user.Amount} | Bu kaçtı!\n");
                break;
            }
        }
        if (counter > maxRetries) Log($"FindUsersNotClaimedDaily() Methodu genel tekrar sayısını aştı. ({maxRetries}) | Kaçırılan Amount: {user.Amount}");
        await Task.Delay(TimeSpan.FromSeconds(12));
    }

    private async Task TryStealFromUser(RandomUser user)
    {
        int retry = 0;
        int counter = 1;
        int maxRetryInARow = 15;
        bool tryAgain = false;

        if (user is null) return;

        do
        {
            try
            {
                var checkenergyList = await _mfWrapper.GetStealEnergyList(user.UserId);
                if (checkenergyList.Result is not null && checkenergyList.Result.Count > 0)
                {
                    int amount = checkenergyList.Result.First().Amount;
                    await Log($"UserID: {user.UserId} | Energy: {amount}");
                    if (amount >= _minEnergyAmountToCollect && _currentStealCount < 8)
                    {
                        Log($"Wow! {_minEnergyAmountToCollect} üstü puan bulundu! => Miktar: {amount}");
                        await StealME(new StealableUser()
                        {
                            Amount = amount,
                            TreeId = user.TreeId,
                            UserId = user.UserId,
                        });
                        break;
                    }
                }
                counter++;
                await Task.Delay(250);
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
                    Log($"[TryStealFromUser] - HATA! - Hata Mesajı: {ex.Message}");
                    await Task.Delay(TimeSpan.FromSeconds(8));
                }
                else
                {
                    Log($"[Eyvah!] - (TryStealFromUser) isimli method tek seferde {maxRetryInARow} hata sayısını aştı.");
                    tryAgain = false;
                    return;
                }
            }
        } while (tryAgain);
    }

    private async Task ScanUsersAmount(List<RandomUser> usersNotClaimedDaily)
    {
        foreach (var notClaimedUser in usersNotClaimedDaily)
        {
            if (_currentStealCount >= _maxDailyStealLimit)
            {
                await Log("Toplama hakkı bittiğinden tarama sonlandırılıyor... ScanUsersAmount()");
                break;
            }

            await TryStealFromUser(notClaimedUser);
        }
    }

    private async Task TimeToBruteForce()
    {
        while (GlobalQueue.RandomUsers.TryDequeue(out RandomUser user))
        {
            if (_currentStealCount >= _maxDailyStealLimit)
            {
                await Log("Toplama hakkı bittiğinden BruteForce sonlandırılıyor... TimeToBruteForce()");
                break;
            }

            await TryStealFromUser(user);
        }
    }

    private async Task<List<RandomUser>> FindUsersNotClaimedDaily()
    {
        List<Models.RandomUser> tempUsersNotClaimedDaily = new List<Models.RandomUser>();
        int pageCounter = 1;
        while (pageCounter <= 20)
        {
            int retryCount = 0;
            int maxRetryInARow = 20;
            while (retryCount <= maxRetryInARow)
            {
                try
                {
                    var leaderboard = await _mfWrapper.GetLeaderboard(pageCounter);
                    foreach (var user in leaderboard.Users)
                    {
                        var userActivities = await _mfWrapper.GetActivity(user.TreeId);
                        var usersActivity = userActivities.Activities.FirstOrDefault(x => x.Type.Contains("daily"));
                        if (usersActivity is null || usersActivity.ClaimAt < DateTime.UtcNow.Date)
                        {
                            tempUsersNotClaimedDaily.Add(new Models.RandomUser()
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
                catch (HttpRequestException exception)
                {
                    if (exception.StatusCode == HttpStatusCode.BadGateway)
                    {
                        await Log("FindUsersNotClaimedDaily() BadGateWay : " + exception.Message);
                        await Task.Delay(1000);
                        continue;
                    }
                    else
                    {
                        retryCount++;
                        await Log("FindUsersNotClaimedDaily() İstek Hatası!? : " + exception.Message + $" - StatusCode: {exception.StatusCode}");
                        await Task.Delay(1000);
                        continue;
                    }
                }
                catch (Exception ex)
                {
                    retryCount++;
                    ExceptionLogger.Log(ex);
                    Log("[HATA] - Kaynak: FindUsersNotClaimedDaily() - Mesaj: " + ex.Message);
                }

            }
        }

        return tempUsersNotClaimedDaily;
    }

    private async Task Log(string msg) => await Console.Out.WriteLineAsync($"{DateTime.Now} - {_accountName}: {msg}");

    private async Task InformDiscord(List<string> messages, bool isError = false)
    {
        if (DiscordWebHookManager.State)
        {
            DiscordWebHookManager.MessageQueue.Enqueue(new DiscordMessage()
            {
                AccountName = _accountName,
                IsError = isError,
                Messages = messages
            });
        }
        else await Log($"Discord web hook bağlantısı girilmediği için discord bilgilendirmesi yapılmadı.");
    }

    private bool IsHexadecimal(string input)
    {
        Regex hexPattern = new Regex("^[0-9A-Fa-f]+$");
        return hexPattern.IsMatch(input.Trim(['0', 'x']));
    }

    private bool CheckTxDataFormat(string data)
    {
        if (String.IsNullOrEmpty(data)) return false;

        if (!data.StartsWith("0x")) return false;

        if (data.Length < 50) return false;

        if (!IsHexadecimal(data)) return false;


        return true;
    }

}
