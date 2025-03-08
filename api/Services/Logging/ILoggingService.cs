using System;
using System.Threading.Tasks;

namespace api.Services
{
    public interface ILoggingService
    {
        Task LogAsync(string sourceIp, string jwtHash, DateTime dateTime, string timeZone, string osVersion, string userAgent, string browserVersion, int failedAttempts, string status, string actions, string userName , string jwtToken , string currentUrl);
    }
}