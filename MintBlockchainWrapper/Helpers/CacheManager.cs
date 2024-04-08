using MintBlockchainWrapper.Models;
using System.Text.Json;

namespace MintBlockchainWrapper.Helpers;
internal static class CacheManager
{
    private static string _cacheFileName = "cache.json";
    private static string _fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,_cacheFileName);
    internal static List<Cache>? GetCache()
    {
        List<Cache> tempCache = null;
        

        if (File.Exists(_fullPath))
        {
            string jsonString = File.ReadAllText(_fullPath);
            tempCache = JsonSerializer.Deserialize<List<Cache>>(jsonString);
        }
        return tempCache;
    }

    internal static void SaveCache(Cache cache)
    {
        var caches = GetCache();

        if(caches == null)
            caches = new List<Cache>();

        caches.Add(cache);


        string json = JsonSerializer.Serialize(caches);
        File.WriteAllText(_fullPath, json);
    }

    internal static void RemoveCache(Cache cache)
    {
        var caches = GetCache();

        if (caches == null)
            return;

        var willRemove = caches.Find(x => x.LoginResponse.Result.AccessToken == cache.LoginResponse.Result.AccessToken);
        caches.Remove(willRemove);

        string json = JsonSerializer.Serialize(caches);
        File.WriteAllText(_fullPath, json);
    }

    //internal static void SaveCaches(List<Cache> caches)
    //{
    //    string json = JsonSerializer.Serialize(caches);
    //    File.WriteAllText(_fullPath, json);
    //}
}
