namespace ChessApi.DTOs.Game
{
    public class MoveDto
    {
        public int GameId { get; set; }
        public int MoveNumber { get; set; }
        public string PlayerColor { get; set; } // "white" or "black"
        public string PieceType { get; set; }   // "pawn", "rook", etc.
        public string From { get; set; }        // เช่น "e2"
        public string To { get; set; }          // เช่น "e4"
        public string MoveNotation { get; set; } // เช่น "e4", "Nf3", "O-O"
        public string? CapturedPiece { get; set; }
        public string? PromotionPiece { get; set; }
        public bool IsCastle { get; set; }
        public string? CastleType { get; set; } // "kingside" or "queenside"
        public bool IsEnPassant { get; set; }
        public bool IsCheck { get; set; }
        public bool IsCheckmate { get; set; }
        public int MoveTimeSeconds { get; set; }

        // Optional: AI data
        public decimal? AiEvaluationScore { get; set; }
        public int? AiDepthSearched { get; set; }
        public int? AiNodesEvaluated { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    }
}
