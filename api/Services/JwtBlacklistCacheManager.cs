using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;

namespace api.Services
{
    public static class JwtBlacklistCacheManager
    {
        public static readonly ConcurrentDictionary<string, string> JwtBlacklistCache = new ConcurrentDictionary<string, string>();

        public static void AddToken(string token , string jwtHash)
        {
            if (string.IsNullOrEmpty(token))
            {
                return;
            }
            JwtBlacklistCache[token] = jwtHash;
        }
        

        public static string GetTokenHash(string token)
        {
            JwtBlacklistCache.TryGetValue(token, out var hash);
            return hash;
        }

        public static string GetTokenByHash(string hash)
        {
            foreach (var kvp in JwtBlacklistCache)
            {
                if (kvp.Value == hash)
                {
                    return kvp.Key;
                }
            }
            return null;
        }

    }
}