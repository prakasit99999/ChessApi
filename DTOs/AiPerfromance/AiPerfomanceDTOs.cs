
namespace ChessApi.DTOs.AiPerformance
{
    public class AiPerformanceCreateDto
    {
        public int GameId { get; set; }
        public string AiLevel { get; set; }             // easy | medium | hard
        public string? AiColor { get; set; }            // white | black
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
        public string? AiColor { get; set; }            // white | black
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
        public string? AiColor { get; set; }            // white | black
        public string AlgorithmType { get; set; }       // minimax | alpha_beta
        public decimal AverageDepth { get; set; }
        public int AverageNodesEvaluated { get; set; }
        public int AverageMoveTimeMs { get; set; }
        public int TotalMoves { get; set; }
    }

    public class AiPerformancePageRequest
    {
        private const int MaxPageSize = 1000;
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

        public int? GameId { get; set; }
        public string? AiLevel { get; set; }
        public string? AiColor { get; set; }
        public string? AlgorithmType { get; set; }
        public string? Search { get; set; }
        public string? Sort { get; set; }
    }

    public class AiPerformancePageResponse
    {
        public List<AiPerformanceListDto> Items { get; set; } = new();
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public int TotalPages => PageSize > 0 ? (int)Math.Ceiling((double)TotalCount / PageSize) : 0;
        public bool HasNext => PageNumber < TotalPages;
        public bool HasPrevious => PageNumber > 1;
    }

    public class AiPerformanceListDto
    {
        public int PerformanceId { get; set; }
        public int GameId { get; set; }
        public string AiLevel { get; set; }             // easy | medium | hard
        public string? AiColor { get; set; }            // white | black
        public string AlgorithmType { get; set; }       // minimax | alpha_beta
        public decimal AverageDepth { get; set; }
        public int AverageNodesEvaluated { get; set; }
        public int AverageMoveTimeMs { get; set; }
        public int TotalMoves { get; set; }
    }

    public class AiPerformanceAllResponse
    {
        public string Message { get; set; }
        public bool Success { get; set; }
        public List<AiPerformanceListDto> data { get; set; } = new();
    }


    public class AiMetricSummaryDto
    {
        public long MinimaxNodes { get; set; }
        public long AlphaBetaNodes { get; set; }
        public int MinimaxTimeMs { get; set; }
        public int AlphaBetaTimeMs { get; set; }
        public double NodeReductionRate { get; set; } // % การลด Node
        public string SpeedupFactor { get; set; } = "1.0x"; // อัตราความเร็วที่เพิ่มขึ้น
        public double PruningEfficiency { get; set; } // ประสิทธิภาพการ Pruning (%)
    }

    // 2. DTO ผลลัพธ์เปรียบเทียบตาม Depth (สำหรับ ApexCharts และ Log Table)
    public class AiBenchmarkDepthDto
    {
        public int Depth { get; set; }
        public long MinimaxNodes { get; set; }
        public long AlphaBetaNodes { get; set; }
        public int MinimaxTimeMs { get; set; }
        public int AlphaBetaTimeMs { get; set; }
        public long PrunedNodes => Math.Max(0, MinimaxNodes - AlphaBetaNodes);
    }

    // 3. DTO หลักที่ API จะ Return ออกไปทั้งหมด (Response Main DTO)
    public class AiPerformanceResponseDto
    {
        public int SelectedDepth { get; set; }
        public AiMetricSummaryDto CurrentMetrics { get; set; } = new();
        public List<AiBenchmarkDepthDto> BenchmarkHistory { get; set; } = new();
    }


}