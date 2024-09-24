using MintBlockchainBotConsoleUI.Helpers;
using MintBlockchainBotConsoleUI.Models;
using System.Collections.Concurrent;

namespace MintBlockchainBotConsoleUI;
internal static class GlobalQueue
{
    public static ConcurrentQueue<RandomUser> RandomUsers { get; set; }

    static GlobalQueue()
    {
        List<RandomUser> randoms = new List<RandomUser>();
        for (int i = 1; i < 40000; i++)
        {
            randoms.Add(new RandomUser()
            {
                UserId = i,
                TreeId = 0,
            });
        }
        randoms.Shuffle();
        RandomUsers = new ConcurrentQueue<RandomUser>(randoms);
    }
}
