using System;
using System.Linq;
using System.Threading.Tasks;
using api.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using UAParser;

public class BlacklistTokenMiddleware
{
    private readonly RequestDelegate _next;
   

    public BlacklistTokenMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
        if (token != null && !string.IsNullOrEmpty (JwtBlacklistCacheManager.GetTokenHash(token)))
        {
            context.Response.StatusCode = 401; // Unauthorized
            await context.Response.WriteAsync("Token has been blacklisted.");
            
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

                var dateTime = DateTime.UtcNow;
                var date = dateTime.ToString("yyyy-MM-dd");
                var time = dateTime.ToString("HH:mm:ss");
                var timeZone = TimeZoneInfo.Local.StandardName;
                var osVersion = Environment.OSVersion.ToString();
                var userAgent = request.Headers["User-Agent"].ToString();

                // Parse the user agent string
                var uaParser = Parser.GetDefault();
                ClientInfo clientInfo = uaParser.Parse(userAgent);
                var browserVersion = clientInfo.UA.ToString();
                var deviceType = clientInfo.Device.ToString();

                // Placeholder for geolocation service
                var geoLocation = "Dubai";

                var failedAttempts = 0; // You can customize this as needed
                var status = response.StatusCode.ToString();
                var actions = request.Path;
                var userName = context.User.Identity?.Name;
                var currentUrl = request.Path;
                var method = request.Method;
                var host = request.Host.ToString();

                await _loggingService.LogAsync(date, time, method, userName, sourceIp, userAgent, host, status, timeZone, geoLocation, osVersion, browserVersion, deviceType, failedAttempts, jwtHash, actions, jwtToken , "BlackListLog.txt");
            }

        }

        await _next(context);
    }
}