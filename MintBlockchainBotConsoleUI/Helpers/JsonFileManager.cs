using MintBlockchainBotConsoleUI.Helpers;
using MintBlockchainBotConsoleUI.Models;
using MintBlockChainBotConsoleUI.Models;
using MintBlockchainWrapper.Models;
using System.Diagnostics;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;

namespace MintBlockChainBotConsoleUI.Helpers;
internal class JsonFileManager
{
    private static string _fileName = "Settings.json";
    private static string _appPath = AppDomain.CurrentDomain.BaseDirectory;

    public static ApplicationSettings ReadCredentials()
    {
        ApplicationSettings accounts = null;
        if (File.Exists(_fileName))
        {
            string currentSettings = File.ReadAllText(_fileName, Encoding.UTF8);
            try
            {
                accounts = JsonSerializer.Deserialize<ApplicationSettings>(currentSettings, new JsonSerializerOptions
                {
                    Encoder = JavaScriptEncoder.Create(UnicodeRanges.All)
                });
            }
            catch
            {
                CreateCredentialsFile();
                throw new Exception($"{_fileName} dosyası okunurken hata oluştu, dosya yeniden oluşturuldu lütfen içini doldurunuz.");
            }
        }
        else
        {
            CreateCredentialsFile();
            throw new Exception($"{_fileName} Dosyası bulunamadığı için yenisi oluşturuluyor, lütfen içini doldurunuz.");
        }
        return accounts;
    }

    static JsonFileManager()
    {
        RemoveOldCacheFiles();
    }

    private static void CreateCredentialsFile()
    {
        // Önceki dosyayı yedekle.
        if (File.Exists(_fileName))
        {
            var folderInfo = Directory.CreateDirectory(Path.Combine(_appPath, "saves"));
            var newFileName = $"Settings_{Stopwatch.GetTimestamp()}.json";
            File.Move(_fileName, Path.Combine(folderInfo.FullName, newFileName));
            Console.WriteLine($"Settings.json içindekilerle birlikte saves klasöründe {newFileName} isminde yedeklendi.");
        }

        string defaultSettings = JsonSerializer.Serialize(new ApplicationSettings()
        {
            WebHookURL = null,
            ErrorWebHookURL = null,
            Credentials = new List<Credential>()
            {
                new Credential()
                {
                    AccountName = "",
                    WalletPrivateKey = "",
                    Proxy = new Proxy()
                    {
                        IP = "ip",
                        Password = "password",
                        Port = 2423,
                        Type = ProxyType.Https,
                        UserName = "username",
                    }
                },
            }

        }, new JsonSerializerOptions()
        {
            Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
            WriteIndented = true,
        }
        );

        try
        {
            File.WriteAllText(_fileName, defaultSettings, Encoding.UTF8);
        }
        catch
        {
            throw new Exception($"{_fileName} dosyası yazılamadı, izin sorunu olabilir!");
        }
    }



    private static string _todaysLeaderboardListFileName => DateTime.Now.ToString("dd_MM_yyyy") + "_leaderboard";
    private static string _todaysStealableListFileName => DateTime.Now.ToString("dd_MM_yyyy") + "_checkedlist";

    public static List<NotClaimedUsers>? LoadNotClaimedLeaderboardUsersIfExists(string accountName)
    {
        var name = _todaysLeaderboardListFileName + $"_{accountName}.json";
        if (File.Exists(name))
        {
            return JsonSerializer.Deserialize<List<NotClaimedUsers>>(File.ReadAllText(name));
        }
        else
        {
            return null;
        }
    }

    public static List<StealableUser> LoadStealableUsersIfExists(string accountName)
    {
        var name = _todaysStealableListFileName + $"_{accountName}.json";
        if (File.Exists(name))
        {
            return JsonSerializer.Deserialize<List<StealableUser>>(File.ReadAllText(name));
        }
        else
        {
            return null;
        }
    }

    public static void SaveNotClaimedLeaderboardUsersToFile(List<NotClaimedUsers> leaderboardDailyList, string accountName)
    {
        var name = _todaysLeaderboardListFileName + $"_{accountName}.json";
        try
        {
            File.WriteAllText(name, JsonSerializer.Serialize(leaderboardDailyList));
        }
        catch(Exception ex)
        {
            Console.WriteLine(DateTime.Now + " - SaveNotClaimedUsersToFile methodunda bir hata oluştu, hata loglandı.");
            ExceptionLogger.Log(ex);
        }
    }

    public static void SaveSteableUsersToFile(List<StealableUser> stealableUsers, string accountName)
    {
        var name = _todaysStealableListFileName + $"_{accountName}.json";
        try
        {
            File.WriteAllText(name, JsonSerializer.Serialize(stealableUsers));
        }
        catch(Exception ex)
        {
            Console.WriteLine(DateTime.Now + " - SaveSteableUsersToFile methodunda bir hata oluştu, hata loglandı.");
            ExceptionLogger.Log(ex);
        }
    }

    /// <summary>
    /// Bu günden daha eski tüm kayıtlı leaderboard ve checkedlist dosyalarını siler.
    /// </summary>
    public static void RemoveOldCacheFiles()
    {
        string todaysDate = DateTime.Now.ToString("dd_MM_yyyy");

        var files = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory);

        foreach (var file in files)
        {
            var info = new FileInfo(file);
            if (info.Name.Contains("_leaderboard"))
            {
                var namesDate = info.Name.Split("_lead").First();
                if (namesDate != todaysDate) info.Delete();
            }
            else if (info.Name.Contains("_checkedlist"))
            {
                var namesDate = info.Name.Split("_check").First();
                if (namesDate != todaysDate) info.Delete();
            }
        }
    }
}
