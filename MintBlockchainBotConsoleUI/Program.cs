using MintBlockchainBot;
using MintBlockChainBotConsoleUI.Helpers;
using MintBlockChainBotConsoleUI.Models;

namespace MintBlockChainBotConsoleUI;

internal class Program
{
    private static ApplicationSettings _appSettings;
    static async Task Main(string[] args)
    {
        LoadApplicationSettings();

        MintForest mfAccount1 = new MintForest(_appSettings.Credentials.First().WalletPrivateKey);

        var checkLogin = await mfAccount1.Login();

        var info = await mfAccount1.GetUserInfo();
        Console.WriteLine(info);
        Console.ReadLine();
    }

    private static void LoadApplicationSettings()
    {
        try
        {
            _appSettings = JsonFileManager.ReadCredentials();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            Console.WriteLine("Programı kapatmak için lütfen enter'a basın...");
            Console.ReadLine();
            Environment.Exit(0);
        }
    }
}
