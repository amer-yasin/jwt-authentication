using System;
using System.IO;
using System.Threading.Tasks;

namespace api.Services
{
    public class LoggingService : ILoggingService
    {
        public async Task LogAsync(
            string date, 
            string time, 
            string method, 
            string userName, 
            string sourceIp, 
            string userAgent, 
            string host, 
            string status, 
            string timeZone, 
            string geoLocation, 
            string osVersion, 
            string browserVersion, 
            string deviceType, 
            int failedAttempts, 
            string jwtHash, 
            string actions, 
            string jwtToken, 
            string screenResolution, // Add screen resolution parameter
            string browserLanguage,  // Add browser language parameter
            string logFileName       // Keep log file name as the last parameter
        )
        {
            string logMessage = $"\"{date}\" | \"{time}\" | \"{method}\" | \"{userName}\" | \"{sourceIp}\" | \"{userAgent}\" | \"{host}\" | \"{status}\" | \"{timeZone}\" | \"{geoLocation}\" | \"{osVersion}\" | \"{browserVersion}\" | \"{deviceType}\" | \"{failedAttempts}\" | \"{jwtHash}\" | \"{actions}\" | \"{jwtToken}\" | \"{screenResolution}\" | \"{browserLanguage}\"";
            string logFilePath = Path.Combine("log", logFileName);

            try
            {
                // Ensure the directory exists
                string directory = Path.GetDirectoryName(logFilePath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                // Append the log message to the file
                await File.AppendAllTextAsync(logFilePath, logMessage + Environment.NewLine);
            }
            catch (Exception ex)
            {
                // Handle or log the exception as needed
                Console.WriteLine($"An error occurred while writing to the log file: {ex.Message}");
            }
        }
    }
}