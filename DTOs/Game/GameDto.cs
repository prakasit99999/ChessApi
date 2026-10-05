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

    public class GameListDto
    {
        public int GameId { get; set; }
        public string GameType { get; set; } = string.Empty;
        public string? MatchMode { get; set; }
        public string GameStatus { get; set; } = string.Empty;
        public string? Result { get; set; }
        public string? ResultReason { get; set; }
        public int MoveCount { get; set; }

        // ผู้เล่นฝ่ายขาว (White Player)
        public int? WhitePlayerId { get; set; }
        public string? WhitePlayerUsername { get; set; }
        public int? WhitePlayerRating { get; set; }
        public string WhitePlayerType { get; set; } = string.Empty;

        // ผู้เล่นฝ่ายดำ (Black Player)
        public int? BlackPlayerId { get; set; }
        public string? BlackPlayerUsername { get; set; }
        public int? BlackPlayerRating { get; set; }
        public string BlackPlayerType { get; set; } = string.Empty;

        // ผู้ชนะ
        public string? Winner { get; set; }

        public DateTime? CreatedAt { get; set; }
        public DateTime? StartedAt { get; set; }
        public DateTime? FinishedAt { get; set; }
    }

    public class GamePagedRequest
    {
        private const int MaxPageSize = 100;
        private int _pageSize = 10;
        private int _pageNumber = 1;

        public int PageNumber
        {
            get => _pageNumber;
            set => _pageNumber = value < 1 ? 1 : value;
        }

        public int PageSize
        {
            get => _pageSize;
            set => _pageSize = value > MaxPageSize ? MaxPageSize : (value < 1 ? 10 : value);
        }

        public int? UserId { get; set; }
        public string? GameType { get; set; }
        public string? MatchMode { get; set; }
        public string? GameStatus { get; set; }
        public string? Search { get; set; }
    }

    public class GameDetailDto : GameListDto
    {
        public List<MoveDto.MoveDetailDto> Moves { get; set; } = new();
    }

    public class GameCountsDto
    {
        public int TotalGames { get; set; }
        public int ActiveMatches { get; set; }
        public int AiMatches { get; set; }
        public int RankedGames { get; set; }
    }

    public class GameChartRequest
    {
        /// <summary>
        /// จำนวนวันย้อนหลังที่ต้องการดูข้อมูล เช่น 30 = ย้อนหลัง 30 วัน
        /// ค่าเริ่มต้น: 30 วัน, ค่าสูงสุด: 365 วัน
        /// </summary>
        private int _days = 30;
        public int Days
        {
            get => _days;
            set => _days = value < 1 ? 30 : (value > 365 ? 365 : value);
        }

        /// <summary>
        /// วันเริ่มต้น (ถ้าไม่ระบุจะใช้ DateTime.UtcNow - Days)
        /// </summary>
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// วันสิ้นสุด (ถ้าไม่ระบุจะใช้ DateTime.UtcNow)
        /// </summary>
        public DateTime? EndDate { get; set; }
    }

    public class GameChartDataDto
    {
        /// วันที่ในรูปแบบ "dd/MM/yyyy" เช่น "04/10/2026"
        public string DateLabel { get; set; } = string.Empty;
        /// วันที่แบบ ISO สำหรับใช้ Sort หรือเปรียบเทียบใน Frontend
        public string DateIso { get; set; } = string.Empty;
        /// จำนวนเกมที่มี AI ร่วมเล่นในวันนี้ (single_player, ai_vs_ai หรือ player_type ขึ้นต้นด้วย "ai")
        public int AiGames { get; set; }
        /// จำนวนเกม Multiplayer ระหว่างผู้เล่น (online_multiplayer, local_multiplayer)
        public int MultiplayerGames { get; set; }
        /// จำนวนเกมทั้งหมดในวันนี้ (AiGames + MultiplayerGames)
        /// </summary>
        public int TotalGames { get; set; }
    }

    public class GameDetails
    {
        public int GameId { get; set; }
        public string? GameType { get; set; }
        public string? MatchMode { get; set; }
        public string? GameStatus { get; set; }
        public string? Result { get; set; }
        public string? ResultReason { get; set; }

        // ประกาศเป็น Property เพื่อรองรับ JSON Serialization
        public PlayerDto WhitePlayer { get; set; } = new();
        public PlayerDto BlackPlayer { get; set; } = new();
        public AiPerformanceDto? AiPerformance { get; set; }

        // รายการ ตาเดิน (Moves)
        public List<MoveDto.MoveDetailDto> Moves { get; set; } = new();

        public string? Message { get; set; }
        public bool Success { get; set; }
    }

    public class PlayerDto
    {
        public int UserId { get; set; }
        public string? Username { get; set; }
        public int Rating { get; set; }
        public string? PlayerType { get; set; }
    }

    public class AiPerformanceDto
    {
        public string? AlgorithmType { get; set; }
        public float? EvaluationScore { get; set; }
        public int? AiDepthSearched { get; set; }
        public int? AiNodesEvaluated { get; set; }
        public int? AiMoveTimeMs { get; set; }
    }


}
