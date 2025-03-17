using System;
using System.Threading.Tasks;

namespace api.Services
{
    public interface ILoggingService
    {
        Task LogAsync(string date, string time, string method, string userName, string sourceIp, string userAgent, string host, string status, string timeZone, string geoLocation, string osVersion, string browserVersion, string deviceType, int failedAttempts, string jwtHash, string actions, string jwtToken , string logFileName);
    }
}