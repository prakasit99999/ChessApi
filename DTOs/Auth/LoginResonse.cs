namespace ChessApi.DTOs.Auth
{
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
