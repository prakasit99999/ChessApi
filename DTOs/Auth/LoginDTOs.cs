using NuGet.Common;

namespace ChessApi.DTOs.Auth
{
    public class LoginRequest
    {
        public string Email { get; set; }
        public string PasswordHash { get; set; }
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
}
