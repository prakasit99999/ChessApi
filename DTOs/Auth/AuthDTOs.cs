namespace ChessApi.DTOs.Auth
{
    public class AuthRequest
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }

    public class AuthResponse
    {
        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string Token { get; set; } = string.Empty;
        public string Status { get; set; } = "offline";
        public string Role { get; set; } = "User";
        public bool IsAdmin { get; set; } = false;
        public string Message { get; set; } = string.Empty;
        public bool Success { get; set; }
    }

    public class AdminAuthResponse
    {
        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string Token { get; set; } = string.Empty;
        public string Role { get; set; } = "Admin";
        public bool IsAdmin { get; set; } = true;
        public string Status { get; set; } = "Online";
        public string Message { get; set; } = string.Empty;
        public bool Success { get; set; } = false;
    }
}
