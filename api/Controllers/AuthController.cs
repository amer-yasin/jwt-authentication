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

        public AuthController(ITokenService tokenService, IPasswordHasher passwordHasher, IUserRepository userRepository)
        {
            _userRepository = userRepository;
            _tokenService = tokenService;
            _passwordHasher = passwordHasher;
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
                role = user.Role
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
    }
}
