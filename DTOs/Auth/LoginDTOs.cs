using NuGet.Common;

namespace ChessApi.DTOs.Auth
{
    public class LoginRequest
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }

    public class LoginResonse
    {
        public string Token { get; set; }
        public int UserId { get; set; }
        public string Username { get; set; }
        public string Message { get; set; }
        public bool Success { get; set; }
        public LoginResonse(string token, int userId, string username)
        {
            Token = token;
            UserId = userId;
            Username = username;
        }
    }
    public class LogoutRequest
    {
        public int UserId { get; set; }
    }

    public class AdminLoginRequest
    {
        public string? UsernameOrEmail { get; set; }
        public string? Username { get; set; }
        public string? Email { get; set; }
        public string Password { get; set; } = string.Empty;

        public string GetIdentifier()
        {
            if (!string.IsNullOrWhiteSpace(UsernameOrEmail)) return UsernameOrEmail.Trim();
            if (!string.IsNullOrWhiteSpace(Username)) return Username.Trim();
            if (!string.IsNullOrWhiteSpace(Email)) return Email.Trim();
            return string.Empty;
        }
    }
}
