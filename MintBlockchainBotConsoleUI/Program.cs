﻿using MintBlockchainBot;
using MintBlockchainBotConsoleUI;
using MintBlockchainBotConsoleUI.Helpers;
using MintBlockChainBotConsoleUI.Helpers;
using MintBlockChainBotConsoleUI.Models;

namespace MintBlockChainBotConsoleUI;

internal class Program
{
    private static ApplicationSettings _appSettings;
    static async Task Main(string[] args)
    {
        Console.Title = "MintForest";

        LoadApplicationSettings();

        foreach (var account in _appSettings.Credentials) 
        {
            try
            {
                var wrapper = new MintForest(account.WalletPrivateKey, account.Proxy);
                if (await wrapper.Login())
                {
                    Console.WriteLine($"{account.AccountName} isimli hesaba başarı ile girildi!");
                    _ = Task.Factory.StartNew(async() => {
                        Bot bot = new Bot(wrapper,account.AccountName, account.StealPointsOnThisAccount, account.CollectDailyOnChain);
                        await bot.Start();
                    });
                }
            }
            catch (Exception ex) 
            {
                Console.WriteLine($"{DateTime.Now} - {account.AccountName}: Hesaba giriş başarısız!");
                Console.WriteLine(ex.Message);
            }
        }

        await Task.Delay(-1);
    }

    private static void LoadApplicationSettings()
    {
        try
        {
            _appSettings = JsonFileManager.ReadCredentials();
            GlobalQueue.SetBarrier(_appSettings.Credentials.Count);
            DiscordWebHookManager.DiscordInformWebHook = _appSettings.WebHookURL;
            DiscordWebHookManager.DiscordErrorWebHook = _appSettings.ErrorWebHookURL;
            DiscordWebHookManager.DiscordLogic();
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
