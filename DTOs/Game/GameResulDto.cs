namespace ChessApi.DTOs.Game
{
    public class GameResultDto
    {
        public int GameId { get; set; }
        public string GameType { get; set; }           // จาก game_type
        public string WhitePlayerType { get; set; }    // human / ai
        public string BlackPlayerType { get; set; }    // human / ai
        public string Result { get; set; }
        public string? ResultReason { get; set; }
        public int? MoveCount { get; set; }
        public int? WhiteRatingBefore { get; set; }
        public int? WhiteRatingAfter { get; set; }
        public int? BlackRatingBefore { get; set; }
        public int? BlackRatingAfter { get; set; }
        public bool? IsRated { get; set; }
        public int? TimeControlMinutes { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? StartedAt { get; set; }
        public DateTime? FinishedAt { get; set; }
    }
}
