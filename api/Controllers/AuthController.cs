using api.Data;
using api.Models;
using api.Models.Dtos;
using api.Models.Entities;
using api.Repositories;
using api.Services;
using api.Services.Passwords;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class AuthController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        private readonly ITokenService _tokenService;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IEmailService _emailService; // Assuming you have an email service


        public AuthController(ITokenService tokenService, IPasswordHasher passwordHasher, IUserRepository userRepository, IEmailService emailService)
        {
            _emailService = emailService;
            _userRepository = userRepository;
            _tokenService = tokenService;
            _passwordHasher = passwordHasher;
            _emailService = emailService;
        }


        [HttpPost("login")]
        [AllowAnonymous]
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
                return BadRequest(new { message = "Invalid password" });
            }

            Token token = _tokenService.CreateToken(user);
            JwtCacheManager.AddToken(token.AccessToken);

            user.RefreshToken = token.AccessToken;
            user.RefreshTokenEndDate = token.Expiration.AddDays(7);
            await _userRepository.CommitAsync();

            return Ok(new
            {
                accessToken = token.AccessToken,
                refreshToken = token.RefreshToken,
                username = user.Email, // Or user.Username if available
                role = user.Role,
                id = user.Id
            });
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            string token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

            if (string.IsNullOrEmpty(token))
            {
                return BadRequest(new { message = "Token is required" });
            }

            User user = await _userRepository.GetByRefreshTokenAsync(token);

            if (user == null)
            {
                return BadRequest(new { message = "Invalid token" });
            }

            // Remove the token from the cache
            //bool removedFromCache = JwtCacheManager.RemoveToken(token);

            //if (!removedFromCache)
            //{
            //    return BadRequest(new { message = "Failed to remove token from cache" });
            //}

            // Invalidate the user's authentication
            user.RefreshToken = null;
            user.RefreshTokenEndDate = null;

            await _userRepository.CommitAsync();

            return Ok(new { message = "User logged out successfully" });
        }

        [HttpGet("revokelogin")]
        [AllowAnonymous]
        public async Task<IActionResult> RevokeLogin(string tokenhash)
        {
            if (string.IsNullOrEmpty(tokenhash))
            {
                return BadRequest(new { message = "Token hash is required" });
            }

            var token = JwtCacheManager.GetTokenByHash(tokenhash);

            if (token == null)
            {
                return BadRequest(new { message = "Invalid token hash" });
            }

            JwtBlacklistCacheManager.AddToken(token, tokenhash);

            User user = await _userRepository.GetByRefreshTokenAsync(token);

            if (user == null)
            {
                return BadRequest(new { message = "Invalid token" });
            }

            // Remove the token from the cache
            bool removedFromCache = JwtCacheManager.RemoveToken(token);

            if (!removedFromCache)
            {
                return BadRequest(new { message = "Failed to remove token from cache" });
            }

            // Invalidate the user's authentication
            user.RefreshToken = null;
            user.RefreshTokenEndDate = null;

            await _userRepository.CommitAsync();

            return Ok(new { message = "User token is revoked successfully" });
        }

        [HttpGet("sendalert")]
        [AllowAnonymous]
        public async Task<IActionResult> SendAlert(string tokenhash,string email)
        {
            string subject = "Security Alert: New Device Login Detected";
            string body = $@"
                <html>
                <body>
                    <p>Dear User,</p>
                    <p>We detected a login to your account from a new device.</p>
                    <p>If this was you, no further action is required. You can safely ignore this message.</p>
                    <p>However, if you do not recognize this activity, we strongly recommend that you take immediate action to protect your account:</p>
                    <ul>
                        <li>🔒 <a href='http://localhost:5001/api/auth/revokelogin?tokenhash={tokenhash}'>Revoke Access</a></li>
                        <li>🔐 <a href='http://monday.lcl:3000'>Secure Your Account</a></li>
                    </ul>
                    <p>For your safety, never share your login credentials or token links with anyone.</p>
                    <p>This is an automated message. Please do not reply to this email.<br>
                    If you believe this email was sent in error or you have any concerns, contact our support team.</p>
                    <p>Stay safe,<br>Security Team</p>
                </body>
                </html>
            ";

            /*string recipientEmail = "user@email.com";
            var emailClaim = User.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub)?.Value;
            if (!string.IsNullOrEmpty(emailClaim))
            {
                recipientEmail = emailClaim;
            }*/

            // Assuming you have an email service to send emails
            await _emailService.SendEmailAsync(email, subject, body);

            return Ok(new { message = "Security email shared" });
        }

        [HttpPost("refreshToken")]
        public async Task<IActionResult> RefreshToken(RefreshToken refreshToken)
        {
            User user = await _userRepository.GetByRefreshTokenAsync(refreshToken.Token);

            if (user == null)
            {
                return BadRequest(new { message = "refresh token is invalid." });
            }
            if (user.RefreshTokenEndDate < DateTime.Now)
            {
                return BadRequest(new { message = "refresh token expired" });
            }

            Token token = _tokenService.CreateToken(user);
            JwtCacheManager.AddToken(token.AccessToken);

            user.RefreshToken = token.AccessToken;
            user.RefreshTokenEndDate = token.Expiration.AddDays(7);
            await _userRepository.CommitAsync();

            return Ok(token);
        }

        [HttpGet("currentUser")]
        public IActionResult GetCurrentUser()
        {
            // Retrieve the claims from the JWT token
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            var usernameClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;
            var emailClaim = User.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub)?.Value;
            var roleClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;

            // Ensure the user is authenticated and claims are present
            if (string.IsNullOrEmpty(userIdClaim) || string.IsNullOrEmpty(emailClaim))
            {
                return Unauthorized(new { message = "User is not authenticated." });
            }

            // Return the user details
            return Ok(new
            {
                id = userIdClaim,
                username = usernameClaim,
                email = emailClaim,
                role = roleClaim
            });
        }
    }
}
