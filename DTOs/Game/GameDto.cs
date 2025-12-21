namespace ChessApi.DTOs.Game
{

    public class GameCreateDto
    {
        // ประเภทเกม: single_player, ai_vs_ai, etc.
        public string GameType { get; set; } = "single_player";

        // ระบุว่าเป็น human หรือ ai_easy, etc.
        public string WhitePlayerType { get; set; } = "human";
        public string BlackPlayerType { get; set; } = "ai_easy";

        // ถ้าล็อกอินแล้ว ให้ส่ง ID มา (ถ้าไม่ส่งมาจะเป็น null)
        public int? WhitePlayerId { get; set; }
        public int? BlackPlayerId { get; set; }
    }
}

