namespace Application.DTOs
{
    public class AuthResponseDto
    {
        public string Email { get; set; } = null!;
        public string UserName { get; set; } = null!;
        public string? PhoneNumber { get; set; }
        public bool IsActive { get; set; }
        public DateTimeOffset? LastLoginAt { get; set; }
        public string AccessToken { get; set; } = null!;
    }
}
