using Application.DTOs;
using Application.Services.Abstraction;
using BCrypt.Net;
using Domain.Entities;
using Domain.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.Implementation
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly JwtSettings _jwtSettings;

        public AuthService(IUserRepository userRepository, IRefreshTokenRepository refreshTokenRepository, IUnitOfWork unitOfWork, IOptions<JwtSettings> jwtSetting)
        {
            _userRepository = userRepository;
            _refreshTokenRepository = refreshTokenRepository;
            _unitOfWork = unitOfWork;
            _jwtSettings = jwtSetting.Value;
        }

        public async Task<UserDto> GetCurrentUserAsync(ClaimsPrincipal principal, HttpRequest request)
        {
            var email = principal.FindFirst(JwtRegisteredClaimNames.Email)?.Value 
                        ?? principal.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
            if (email == null) {
                throw new InvalidOperationException("Not authorized");
            }
            
            var user = await _userRepository.GetByEmailAsync(email);
            if (user == null) { 
                throw new InvalidOperationException($"Invalid email: {email}");
            }

            var token = request.Headers["Authorization"].ToString().Replace("Bearer ", "");

            return new UserDto
            {
                Email=user.Email,
                UserName=user.UserName,
                PhoneNumber=user.PhoneNumber,
                IsActive=user.IsActive
            };

        }

        public async Task<AuthResponseDto> LoginAsync(LoginDto loginDto, HttpResponse response)
        {
            var user = await _userRepository.GetByEmailWithRefreshTokensAsync(loginDto.Email);

            if (user == null || !BCrypt.Net.BCrypt.Verify(loginDto.Password, user.HashedPassword))
            {
                throw new InvalidOperationException("Invalid email or password.");
            }

            user.LastLoginAt = DateTimeOffset.UtcNow;
            _userRepository.Update(user);
            var accessToken = GenerateAccessToken(user);
            var refreshTokenValue = GenerateRefreshToken();

            var refreshToken = new RefreshToken
            {
                HashedToken = HashToken(refreshTokenValue),
                UserId = user.Id,
                ExpiresAt = DateTimeOffset.UtcNow.AddDays(_jwtSettings.RefreshTokenExpiryDays),
                CreatedAt = DateTimeOffset.UtcNow,
                User = user,
            };
            user.RefreshTokens.Add(refreshToken);
            await _refreshTokenRepository.AddAsync(refreshToken);
            await _unitOfWork.SaveChangesAsync();

            SetRefreshTokenCookie(response, refreshTokenValue);

            return new AuthResponseDto
            {
                Email = user.Email,
                UserName = user.UserName,
                PhoneNumber = user.PhoneNumber,
                IsActive = user.IsActive,
                LastLoginAt = user.LastLoginAt,
                AccessToken = accessToken,
            };
        }

        public async Task LogoutAsync(HttpRequest request, HttpResponse response)
        {
            var token = request.Cookies["refreshToken"];

            if (!string.IsNullOrEmpty(token)) await RevokeTokenAsync(request, response);

            response.Cookies.Delete("refreshToken");
        }

        public async Task<AuthResponseDto> RefreshTokenAsync(HttpRequest request, HttpResponse response)
        {
            var token = request.Cookies["refreshToken"];
            if (string.IsNullOrEmpty(token))
                throw new UnauthorizedAccessException("No refresh token provided.");

            var hashedToken = HashToken(token);
            var refreshToken = await _refreshTokenRepository.GetByHashedTokenAsync(hashedToken);

            if (refreshToken == null || !refreshToken.IsActive)
                throw new UnauthorizedAccessException("Invalid or expired refresh token.");

            var user = refreshToken.User;

            _refreshTokenRepository.Revoke(refreshToken);

            var accessToken = GenerateAccessToken(user);
            var newRefreshTokenValue = GenerateRefreshToken();

            var newRefreshToken = new RefreshToken
            {
                HashedToken = HashToken(newRefreshTokenValue),
                UserId = user.Id,
                ExpiresAt = DateTimeOffset.UtcNow.AddDays(_jwtSettings.RefreshTokenExpiryDays),
                CreatedAt = DateTimeOffset.UtcNow,
                User = user,
            };

            await _refreshTokenRepository.AddAsync(newRefreshToken);
            await _unitOfWork.SaveChangesAsync();

            SetRefreshTokenCookie(response, newRefreshTokenValue);

            return new AuthResponseDto
            {
                Email = user.Email,
                UserName = user.UserName,
                PhoneNumber = user.PhoneNumber,
                IsActive = user.IsActive,
                LastLoginAt = user.LastLoginAt,
                AccessToken = accessToken,
            };
        }

        public async Task<AuthResponseDto> RegisterAsync(RegisterDto registerDto, HttpResponse response)
        {
            var existingEmail = await _userRepository.GetByEmailAsync(registerDto.Email);
            if (existingEmail != null)
            {
                throw new InvalidOperationException("Email is already registered.");
            }

            var existingUserName = await _userRepository.GetByUserNameAsync(registerDto.UserName);
            if (existingUserName != null)
            {
                throw new InvalidOperationException("Username is already taken.");
            }

            var user = new User
            {
                Email = registerDto.Email,
                UserName = registerDto.UserName,
                PhoneNumber = registerDto.PhoneNumber,
                HashedPassword = BCrypt.Net.BCrypt.HashPassword(registerDto.Password, workFactor: 12),
                IsActive = true,
                LastLoginAt = DateTimeOffset.UtcNow,
            };

            await _userRepository.AddAsync(user);

            var accessToken = GenerateAccessToken(user);
            var refreshTokenValue = GenerateRefreshToken();

            var refreshToken = new RefreshToken
            {
                HashedToken = HashToken(refreshTokenValue),
                UserId = user.Id,
                ExpiresAt = DateTimeOffset.UtcNow.AddDays(_jwtSettings.RefreshTokenExpiryDays),
                CreatedAt = DateTimeOffset.UtcNow,
                User = user,
            };

            await _refreshTokenRepository.AddAsync(refreshToken);
            await _unitOfWork.SaveChangesAsync();

            SetRefreshTokenCookie(response, refreshTokenValue);

            return new AuthResponseDto
            {
                Email = user.Email,
                UserName = user.UserName,
                PhoneNumber = user.PhoneNumber,
                IsActive = user.IsActive,
                LastLoginAt = user.LastLoginAt,
                AccessToken = accessToken,
            };
        }

        public async Task<bool> RevokeTokenAsync(HttpRequest request, HttpResponse response)
        {
            var token = request.Cookies["refreshToken"];
            if (string.IsNullOrEmpty(token)) return false;

            var hashedToken = HashToken(token);
            var refreshToken = await _refreshTokenRepository.GetByHashedTokenAsync(hashedToken);

            if (refreshToken == null || !refreshToken.IsActive) return false;

            _refreshTokenRepository.Revoke(refreshToken);
            await _unitOfWork.SaveChangesAsync();

            response.Cookies.Delete("refreshToken");
            return true;
        }



        private string GenerateAccessToken(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.UniqueName, user.UserName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpiryMinutes),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private string GenerateRefreshToken()
        {
            var randomBytes = new byte[64];
            using var rng = System.Security.Cryptography.RandomNumberGenerator.Create();
            rng.GetBytes(randomBytes);
            return Convert.ToBase64String(randomBytes);
        }

        private string HashToken(string token)
        {
            using var sha256 = System.Security.Cryptography.SHA256.Create();
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(token));
            return Convert.ToBase64String(bytes);
        }

        private void SetRefreshTokenCookie(HttpResponse response, string refreshToken)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Expires = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpiryDays),
                Secure = true,
                SameSite = SameSiteMode.Strict,
            };

            response.Cookies.Append("refreshToken", refreshToken, cookieOptions);
        }
    }
}
