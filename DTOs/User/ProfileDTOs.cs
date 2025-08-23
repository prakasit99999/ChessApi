namespace ChessApi.DTOs.User
{
    public class ProfileRequest
    {
        public int UserId { get; set; }
    }
    public class ProfileResponse
    {
        public string Username { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public int Rating { get; set; }
        public int GamesPlayed { get; set; }
        public int GamesWon { get; set; }
        public int GamesLost { get; set; }
        public int GamesDrawn { get; set; }
        public string Status { get; set; } // online, offline, playing
        public bool Success { get; set; } = false;
        public string? Message { get; set; } = null;
    }
}
