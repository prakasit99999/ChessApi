namespace ChessApi.DTOs.Auth
{
    public class RegisterRequest
    {
        public string Username { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
    }
    public class RegisterResponse
    {
        public string Message { get; set; }
        public bool Success { get; set; }
    }

}
