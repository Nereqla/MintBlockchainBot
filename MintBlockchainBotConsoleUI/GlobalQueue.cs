using MintBlockchainBotConsoleUI.Helpers;
using MintBlockchainBotConsoleUI.Models;
using MintBlockChainBotConsoleUI.Helpers;
using System.Collections.Concurrent;

namespace MintBlockchainBotConsoleUI;
internal static class GlobalQueue
{
    public static ConcurrentQueue<RandomUser> RandomUsers { get; private set; }
    public static ConcurrentQueue<int> LeaderBoardPages { get; private set; }
    public static ConcurrentQueue<RandomUser> UsersUnclaimedDaily { get; private set; }
    public static ConcurrentQueue<RandomUser> LeaderboardUsers { get; private set; }
    public static ConcurrentQueue<StealableUser> StealableUsers { get; private set; }

    public static Barrier ScanCompletedBarrier;
    public static Barrier StealingIsCompletedOnAccountsBarrier;

    static GlobalQueue()
    {
        LoadAll();
        LoadRandomUsers();
        StealableUsers = new ConcurrentQueue<StealableUser>();
        UsersUnclaimedDaily = new ConcurrentQueue<RandomUser>();
    }
    
    public static void LoadAll()
    {
        LoadLeaderboardPages();
        LoadLeaderBoardUsers();
    }

    public static void SetBarrier(int botCount)
    {
        ScanCompletedBarrier = new Barrier(botCount, (b) =>
        {
            JsonFileManager.SaveLeaderboardUsers(LeaderboardUsers.ToList());
        });

        StealingIsCompletedOnAccountsBarrier = new Barrier(botCount, x =>
        {
            LoadAll();
        });
    }

    private static void LoadRandomUsers()
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

    private static void LoadLeaderboardPages()
    {
        LeaderBoardPages = new ConcurrentQueue<int>();
        for (int i = 1; i <= 20; i++)
        {
            LeaderBoardPages.Enqueue(i);
        }
    }

    public static void LoadLeaderBoardUsers()
    {
        List<RandomUser>? leaderboardUsersList = JsonFileManager.LoadLeaderBoardUsers();
        leaderboardUsersList?.Shuffle();
        
        LeaderboardUsers = new ConcurrentQueue<RandomUser>(leaderboardUsersList ?? new List<RandomUser>());
    }
}
