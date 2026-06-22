using Application.DTOs;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.Abstraction
{
    public interface IAuthService
    {
        Task<AuthResponseDto> LoginAsync(LoginDto loginDto, HttpResponse response);
        Task<AuthResponseDto> RegisterAsync(RegisterDto registerDto, HttpResponse response);
        Task LogoutAsync(HttpRequest request, HttpResponse response);
        Task<UserDto> GetCurrentUserAsync(ClaimsPrincipal principal, HttpRequest request);
        Task<AuthResponseDto> RefreshTokenAsync(HttpRequest request, HttpResponse response);
        Task<bool> RevokeTokenAsync(HttpRequest request, HttpResponse response);
    }
}
