namespace ChessApi.DTOs.Game
{
    public class MoveDto
    {
        public enum pieceType
        {
            Pawn,
            Rook,
            Knight,
            Bishop,
            Queen,
            King
        }
        public enum team
        {
            White,
            Black
        }

        public enum capturePieceType
        {
            None,
            Pawn,
            Rook,
            Knight,
            Bishop,
            Queen,
            King
        }

        public enum capturedPicecTeam
        {
            None,
            White,
            Black
        }

        public enum promotedTo
        {
            None,
            Rook,
            Knight,
            Bishop,
            Queen
        }

        public enum promotedFrom
        {
            None,
            Pawn
        }

        public enum algorithmType
        {
            None,
            Minimax,
            AlphaBeta,
        }

        public class MoveRequest
        {
            public int GameId { get; set; }
            public int MoveNumber { get; set; }

            public int StartX { get; set; }
            public int StartY { get; set; }
            public int EndX { get; set; }
            public int EndY { get; set; }

            public int CapturedX { get; set; }
            public int CapturedY { get; set; }

            public pieceType PieceType { get; set; }
            public team PlayerTurn { get; set; }

            public capturePieceType CapturedPieceType { get; set; }
            public capturedPicecTeam CapturedPieceTeam { get; set; }

            public promotedTo PromotedTo { get; set; }
            public promotedFrom PromotedFrom { get; set; }

            public algorithmType AlgorithmType { get; set; }

            public bool IsCastling { get; set; }
            public bool IsEnPassant { get; set; }
            public bool IsCapture { get; set; }
            public bool IsCheck { get; set; }
            public bool IsPawnTwoStep { get; set; }
            public bool PieceHasMovedBefore { get; set; }

            public int AiEvaluationScore { get; set; }
            public int AiDepthSearched { get; set; }
            public int AiNodesEvaluated { get; set; }
            public int MoveTimeMilliseconds { get; set; }
        }

        public class MoveBatchRequest
        {
            public List<MoveRequest> moves { get; set; }
        }

        public class LatestMoveResponseDto
        {
            public int MoveNumber { get; set; }

            // ส่งพิกัดแบบตัวเลข (0-7) เพื่อให้ Unity ใช้งานง่ายกับ Array
            public int StartX { get; set; }
            public int StartY { get; set; }
            public int EndX { get; set; }
            public int EndY { get; set; }

            // ส่งแบบ String ("e2", "e4") เผื่อใช้แสดงผลหรือ Log
            public string FromPosition { get; set; } = string.Empty;
            public string ToPosition { get; set; } = string.Empty;
            public string PlayerTurn { get; set; } = string.Empty; // "White" or "Black"
 
            // New fields for synchronization to match client expectations
            public bool IsCastling { get; set; }
            public bool IsEnPassant { get; set; }
 
            // Corresponds to the 'promotedTo' enum integer value. 0 for None.
            public int PromotedTo { get; set; }
 
            // Fields for game over status
            public string? Status { get; set; }
            public string? Winner { get; set; }
        }

    }
}
