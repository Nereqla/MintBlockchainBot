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
}
