namespace MintBlockchainWrapper;
public static class Config
{
    public static string UserAgent { get; } = "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:130.0) Gecko/20100101 Firefox/130.0";
    public static string Referer { get; } = "https://www.mintchain.io/mint-forest";
    public static string Origin { get; } = "https://www.mintchain.io";
    public static string LoginEndpoint { get; } = "https://www.mintchain.io/api/tree/login";
    public static string UserInfoEndpoint { get; } = "https://www.mintchain.io/api/tree/user-info";
    public static string InjectMeEndpoint { get; } = "https://www.mintchain.io/api/tree/inject";
    public static string ClaimEndpoint { get; } = "https://www.mintchain.io/api/tree/claim";
    public static string EnergyListEndpoint { get; } = "https://www.mintchain.io/api/tree/energy-list";

    public static string GetStealEnergyListEndpoint(int userID) => $"https://www.mintchain.io/api/tree/steal/energy-list?id={userID}";
    public static string GetTransactionDataEndpoint(TransactionType type, int userID) => $"https://www.mintchain.io/api/tree/get-forest-proof?type={type.ToString()}" + (userID == 0 ? "" : $"&id={userID}");
    public static string GetLeaderboardEndpoint(int pageNumber) => $"https://www.mintchain.io/api/tree/leaderboard?page={pageNumber}";
    public static string GetActivityEndPoint(int treeID) => $"https://www.mintchain.io/api/tree/activity?page=1&treeid={treeID}";
}

public enum TransactionType
{
    Steal,
    Signin
}
