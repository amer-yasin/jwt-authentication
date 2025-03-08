using System;
using System.IO;
using System.Threading.Tasks;

namespace api.Services
{
    public class LoggingService : ILoggingService
    {
        public async Task LogAsync(string sourceIp, string jwtHash, DateTime dateTime, string timeZone, string osVersion, string userAgent, string browserVersion, int failedAttempts, string status, string actions, string userName , string jwtToken , string currentUrl)
        {
            string logMessage = $"\"{dateTime}\" | \"{sourceIp}\" | \"{jwtHash}\" | \"{timeZone}\" | \"{osVersion}\" | \"{userAgent}\" | \"{browserVersion}\" | \"{failedAttempts}\" | \"{status}\" | \"{actions}\" | \"{userName}\" | \"{jwtToken}\" | \"{currentUrl}\"";
            string logFilePath = Path.Combine("log", "log.txt");

            try
            {
                // Ensure the directory exists
                string directory = Path.GetDirectoryName(logFilePath);
                Console.WriteLine($"Directory: {directory}");
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                    Console.WriteLine($"Directory created: {directory}");
                }

                // Append the log message to the file
                await File.AppendAllTextAsync(logFilePath, logMessage + Environment.NewLine);
                Console.WriteLine($"Log written to {logFilePath}");
            }
            catch (Exception ex)
            {
                // Handle or log the exception as needed
                Console.WriteLine($"An error occurred while writing to the log file: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
            }
        }
    }
}