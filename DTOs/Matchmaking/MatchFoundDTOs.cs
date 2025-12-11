namespace ChessApi.DTOs.Matchmaking
{
    public class MatchFoundDTOs
    {
        public int GameId { get; set; }
        public string OpponentUsername { get; set; } = string.Empty;
        public int TimeControlMinutes { get; set; }
        public string RoomCode { get; set; } = string.Empty;
        public string GameType { get; set; } = "standard";
        public bool IsRated { get; set; }
        public string Color { get; set; } = "random";
    }
}