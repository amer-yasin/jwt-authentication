using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;

namespace api.Services
{
    public static class JwtCacheManager
    {
        public static readonly ConcurrentDictionary<string, string> JwtCache = new ConcurrentDictionary<string, string>();

        public static void AddToken(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                return;
            }

            if (JwtCache.ContainsKey(token))
            {
                return;
            }
            
            // Compute the hash of the JWT token
            byte[] inputBytes = Encoding.UTF8.GetBytes(token);
            byte[] hashBytes = SHA256.Create().ComputeHash(inputBytes);

            // Convert byte array to hex string
            StringBuilder sb = new StringBuilder();
            foreach (byte b in hashBytes)
            {
                sb.Append(b.ToString("x2")); // Converts to a lowercase hexadecimal string
            }
            string jwtHash = sb.ToString();
            JwtCache[token] = jwtHash;
        }

        public static string GetTokenHash(string token)
        {
            JwtCache.TryGetValue(token, out var hash);
            return hash;
        }

        public static string GetTokenByHash(string hash)
        {
            foreach (var kvp in JwtCache)
            {
                if (kvp.Value == hash)
                {
                    return kvp.Key;
                }
            }
            return null;
        }

        public static bool RemoveToken(string token)
        {
            return JwtCache.TryRemove(token, out _);
        }

        public static bool RemoveTokenByHash(string hash)
        {
            var token = GetTokenByHash(hash);
            if (token != null)
            {
                return RemoveToken(token);
            }
            return false;
        }
    }
}