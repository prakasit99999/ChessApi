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
        public string Username { get; set; }
        public string Email { get; set; }
        public string Token { get; set; }
        public string Status { get; set; }
        public string Message { get; set; }
        public bool Success { get; set; }

    }
}
