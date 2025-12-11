
namespace ChessApi.DTOs.AiPerformance
{
    public class AiPerformanceCreateDto
    {
        public int GameId { get; set; }
        public string AiLevel { get; set; }             // easy | medium | hard
        public string AlgorithmType { get; set; }       // minimax | alpha_beta
        public decimal AverageDepth { get; set; }
        public int AverageNodesEvaluated { get; set; }
        public int AverageMoveTimeMs { get; set; }
        public int TotalMoves { get; set; }
    }

    public class AiPerformanceUpdateDto
    {
        public int PerformanceId { get; set; }
        public int GameId { get; set; }
        public string AiLevel { get; set; }             // easy | medium | hard
        public string AlgorithmType { get; set; }       // minimax | alpha_beta
        public decimal AverageDepth { get; set; }
        public int AverageNodesEvaluated { get; set; }
        public int AverageMoveTimeMs { get; set; }
        public int TotalMoves { get; set; }
    }

    public class AiPerformanceDto
    {
        public int PerformanceId { get; set; }
        public int GameId { get; set; }
        public string AiLevel { get; set; }             // easy | medium | hard
        public string AlgorithmType { get; set; }       // minimax | alpha_beta
        public decimal AverageDepth { get; set; }
        public int AverageNodesEvaluated { get; set; }
        public int AverageMoveTimeMs { get; set; }
        public int TotalMoves { get; set; }
    }
}