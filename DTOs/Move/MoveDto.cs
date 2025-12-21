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
            public int startX { get; set; }
            public int startY { get; set; }
            public int endX { get; set; }
            public int endY { get; set; }
            public int capturedX { get; set; }
            public int capturedY { get; set; }
            public pieceType PieceType { get; set; }
            public team PlayerTurn { get; set; }
            public capturePieceType CapturedPieceType { get; set; }
            public capturedPicecTeam CapturedPieceTeam { get; set; }
            public promotedTo PromotedTo { get; set; }
            public promotedFrom PromotedFrom { get; set; }
            public algorithmType AlgorithmType { get; set; }
            public bool IsCasting { get; set; }
            public bool IsEnPassant { get; set; }
            public bool IsCapture { get; set; }
            public bool IsCheck { get; set; }
            public bool IsPawnTwoStep { get; set; }
            public bool PieceHasMovedBefore { get; set; }

            public decimal aiEvaluationScore { get; set; }
            public int aiDepthSearched { get; set; }
            public int aiNodesEvaluated { get; set; }
            public int moveTimeMilliseconds { get; set; }

        }
        public class MoveBatchRequest
        {
            public List<MoveRequest> moves { get; set; }
        }

    }
}
