namespace ChessApi.DTOs.Game
{

    public class GameCreateDto
    {
        // ประเภทเกม: single_player, ai_vs_ai, etc.
        public string GameType { get; set; } = "single_player";

        // โหมดการเล่น: ranked (จัดอันดับ), LogOutAsync (กระชับมิตร)
        public int? MatchMode { get; set; } = 0; // 0 = null, 1 = ranked , 2 = normal

        // ระบุว่าเป็น human หรือ ai_easy, etc.
        public string WhitePlayerType { get; set; } = "human";
        public string BlackPlayerType { get; set; } = "ai_easy";

        // ถ้าล็อกอินแล้ว ให้ส่ง ID มา (ถ้าไม่ส่งมาจะเป็น null)
        public int? WhitePlayerId { get; set; }
        public int? BlackPlayerId { get; set; }
    }


    public class GameResultDto
    {
        public int GameId { get; set; }
        public string? GameType { get; set; }
        public string? MatchMode { get; set; }
        public string? WhitePlayerType { get; set; }
        public string? BlackPlayerType { get; set; }   // human / ai
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

    public class GameResignDto
    {
        public int GameId { get; set; }
        public int PlayerId { get; set; }
        public string Reason { get; set; }
    }


    public class GameStateDto
    {
        // ข้อมูลอ้างอิง
        public int StateId { get; set; }
        public int GameId { get; set; }
        public int MoveNumber { get; set; }

        // ข้อมูลตำแหน่งหมาก (FEN) - แนะนำให้ใช้ตัวนี้เป็นหลักในการวาดกระดาน
        public string FenPosition { get; set; }

        // สถานะของเกมในตานั้นๆ
        public bool IsCheck { get; set; }
        public bool IsCheckmate { get; set; }
        public bool IsStalemate { get; set; }

        // ข้อมูลสำหรับการวิเคราะห์ (จาก Model ของคุณ)
        public float? PositionScore { get; set; } // คะแนนความได้เปรียบ (เช่น +1.5, -0.8)
        public string PvLine { get; set; }        // Principal Variation (สายตาเดินที่ดีที่สุดที่ AI แนะนำ)

        public DateTime CreatedAt { get; set; }

        // เพิ่มเติม: ข้อมูลตาเดินล่าสุดเพื่อให้ Unity ทำ Highlight
        public MoveDto LastMove { get; set; }
    }
}
