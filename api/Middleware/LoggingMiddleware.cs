using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using api.Services;
using UAParser;
public class LoggingMiddleware
{
    private readonly RequestDelegate _next;

    public LoggingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        using (var scope = context.RequestServices.CreateScope())
        {
            var _loggingService = scope.ServiceProvider.GetRequiredService<ILoggingService>();

            var request = context.Request;
            var response = context.Response;
            var sourceIp = context.Connection.RemoteIpAddress?.MapToIPv4().ToString();
            var jwtToken = request.Headers["Authorization"].ToString().Replace("Bearer ", "");

            // Compute the hash of the JWT token
            var jwtHash = string.Empty;
            jwtHash = JwtCacheManager.GetTokenHash(jwtToken);
            if (string.IsNullOrEmpty(jwtHash))
            {
                JwtCacheManager.AddToken(jwtToken);
                jwtHash = JwtCacheManager.GetTokenHash(jwtToken);
            }

            var dateTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.Local);
            var date = dateTime.ToString("yyyy-MM-dd");
            var time = dateTime.ToString("HH:mm:ss");
            var timeZone = request.Headers["X-TimeZone"].ToString();
            if (string.IsNullOrEmpty(timeZone))
            {
                timeZone = "Unknown"; // Fallback if the client does not provide the time zone
            }
    
            var userAgent = request.Headers["User-Agent"].ToString();

            // Parse the user agent string
            var uaParser = Parser.GetDefault();
            ClientInfo clientInfo = uaParser.Parse(userAgent);
            var osVersion = clientInfo.OS.ToString();
            var browserVersion = clientInfo.UA.ToString();
            var deviceType = clientInfo.Device.ToString();

            // Placeholder for geolocation service
            var geoLocation = "Duabi";

            var failedAttempts = 0; // You can customize this as needed
            var status = response.StatusCode.ToString();
            var actions = request.Path;
            var userName = context.User.Identity?.Name;
            var currentUrl = request.Path;
            var method = request.Method;
            var host = request.Host.ToString();

            await _loggingService.LogAsync(date, time, method, userName, sourceIp, userAgent, host, status, timeZone, geoLocation, osVersion, browserVersion, deviceType, failedAttempts, jwtHash, actions, jwtToken , "log.txt");
        }

        await _next(context);
    }

}