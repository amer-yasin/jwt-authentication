using api.Data;
using api.Models;
using api.Models.Dtos;
using api.Models.Entities;
using api.Repositories;
using api.Services;
using api.Services.Passwords;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Text;
using System.IO; // Add this directive
using UAParser;

namespace api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        private readonly ITokenService _tokenService;
        private readonly IPasswordHasher _passwordHasher;
        private readonly ILoggingService _loggingService; // Add this line

        
        public AuthController(ITokenService tokenService, IPasswordHasher passwordHasher, IUserRepository userRepository, ILoggingService loggingService) // Modify constructor
        {
            _userRepository = userRepository;
            _tokenService = tokenService;
            _passwordHasher = passwordHasher; 
            _loggingService = loggingService; //Initialize the logging service
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(UserLogin userLogin)
        {
            User user = await _userRepository.GetByEmailAsync(userLogin.Email);

            if (user == null)
            {
                return BadRequest(new { message = "No registered user for this email." });
            }

            var passwordValid = _passwordHasher.VerifyPassword(userLogin.Password, user.Password);

            if (!passwordValid)
            {
                return BadRequest(new { message = "İnvalid password" });
            }

            Token token = _tokenService.CreateToken(user);

            user.RefreshToken = token.RefreshToken;
            user.RefreshTokenEndDate = token.Expiration.AddMinutes(5);
            await _userRepository.CommitAsync();

            // Log the successful login
                byte[] inputBytes = Encoding.UTF8.GetBytes(token.AccessToken);
                byte[] hashBytes = SHA256.Create().ComputeHash(inputBytes);

                // Convert byte array to hex string
                StringBuilder sb = new StringBuilder();
                foreach (byte b in hashBytes)
                {
                    sb.Append(b.ToString("x2")); // Converts to a lowercase hexadecimal string
                }
            string jwtHash = sb.ToString();

            string sourceIp = HttpContext.Connection.RemoteIpAddress.ToString();

            if (Request.Headers.ContainsKey("X-Forwarded-For"))
            {
                sourceIp = Request.Headers["X-Forwarded-For"].ToString().Split(',').FirstOrDefault();
            }

            string jwtToken = token.AccessToken;
            DateTime dateTime = DateTime.UtcNow;
            string timeZone = TimeZoneInfo.Local.StandardName;

            var uaParser = Parser.GetDefault();
            ClientInfo clientInfo = uaParser.Parse(Request.Headers["User-Agent"].ToString());
            string osVersion = clientInfo.OS.ToString();
            string userAgent = Request.Headers["User-Agent"].ToString();
            string browserVersion = clientInfo.UA.ToString();

            int failedAttempts = 0; // This should be tracked separately
            string status = "User logged in";
            string actions = "Login";
            string userName = user.Email;

            await _loggingService.LogAsync(sourceIp, jwtHash, dateTime, timeZone, osVersion, userAgent, browserVersion, failedAttempts, status, actions, userName, jwtToken, Request.Path);
            
            return Ok(token);
        }
        [HttpPost("refreshToken")]
        public async Task<IActionResult> Login(RefreshToken refreshToken)
        {
            User user = await _userRepository.GetByRefreshTokenAsync(refreshToken.Token);

            if (user == null)
            {
                return BadRequest(new { message = "refresh token is invalid." });
            }
            if(user.RefreshTokenEndDate < DateTime.Now)
            {
                return BadRequest(new { message = "refresh token expired" });
            }


            Token token = _tokenService.CreateToken(user);

            user.RefreshToken = token.RefreshToken;
            user.RefreshTokenEndDate = token.Expiration.AddMinutes(5);
            await _userRepository.CommitAsync();

            return Ok(token);
        }
    }
}
