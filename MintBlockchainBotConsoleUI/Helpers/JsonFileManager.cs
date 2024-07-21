using MintBlockchainBotConsoleUI.Helpers;
using MintBlockchainBotConsoleUI.Models;
using MintBlockChainBotConsoleUI.Models;
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
            Credentials = new List<Credential>()
            {
                new Credential()
                {
                    AccountName = "",
                    WalletPrivateKey = "",
                }
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



    private static string _todaysLeaderboardFileName = DateTime.Now.ToString("dd_MM_yyyy") + "_leaderboard.json";
    private static string _todaysStealableListFileName = DateTime.Now.ToString("dd_MM_yyyy") + "_checkedlist.json";
    public static List<NotClaimedUsers>? LoadNotClaimedLeaderboardUsersIfExists()
    {
        if (File.Exists(_todaysLeaderboardFileName))
        {
            return JsonSerializer.Deserialize<List<NotClaimedUsers>>(File.ReadAllText(_todaysLeaderboardFileName));
        }
        else
        {
            return null;
        }
    }

    public static List<StealableUser> LoadStealableUsersIfExists()
    {
        if (File.Exists(_todaysStealableListFileName))
        {
            return JsonSerializer.Deserialize<List<StealableUser>>(File.ReadAllText(_todaysStealableListFileName));
        }
        else
        {
            return null;
        }
    }

    public static void SaveNotClaimedLeaderboardUsersToFile(List<NotClaimedUsers> leaderboardDailyList)
    {
        try
        {
            File.WriteAllText(_todaysLeaderboardFileName, JsonSerializer.Serialize(leaderboardDailyList));
        }
        catch(Exception ex)
        {
            Console.WriteLine(DateTime.Now + " - SaveNotClaimedUsersToFile methodunda bir hata oluştu, hata loglandı.");
            ExceptionLogger.Log(ex);
        }
    }

    public static void SaveSteableUsersToFile(List<StealableUser> stealableUsers)
    {
        try
        {
            File.WriteAllText(_todaysStealableListFileName, JsonSerializer.Serialize(stealableUsers));
        }
        catch(Exception ex)
        {
            Console.WriteLine(DateTime.Now + " - SaveSteableUsersToFile methodunda bir hata oluştu, hata loglandı.");
            ExceptionLogger.Log(ex);
        }
    }
}
