namespace ChessApi.DTOs.Matchmaking
{
    public class MatchFoundResponse
    {
        public int GameId { get; set; }
        public string OpponentUsername { get; set; } = string.Empty;
        public string RoomCode { get; set; }
        public int TimeControlMinutes { get; set; }
        public string GameType { get; set; } = "standard"; // standard / blitz / bullet
        public bool IsRated { get; set; }
        public string Color { get; set; } = "random"; // white / black
    }
}
