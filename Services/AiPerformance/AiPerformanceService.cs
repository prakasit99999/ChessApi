using ChessApi.DbContext;
using ChessApi.DTOs.AiPerformance;
using ChessApi.Models;
using ChessApi.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;
using System.Text.Json;
using System.Linq;
using System.Threading.Tasks;
using System.Text;


namespace ChessApi.Services.AiPerformance
{
    public class AiPerformanceService : IAiPerformance
    {
        private readonly ChessDbContext _context;

        public AiPerformanceService(ChessDbContext context)
        {
            _context = context;
        }

        public async Task<int> CreateAiPerformanceAsync(AiPerformanceCreateDto dto)
        {
            // 1. ตรวจสอบว่า game มีอยู่จริง และดึง game_type มาด้วย
            var game = await _context.games
                .FirstOrDefaultAsync(g => g.game_id == dto.GameId);

            if (game == null)
                throw new KeyNotFoundException($"Game with ID {dto.GameId} does not exist.");

            var aiColorNormalized = dto.AiColor?.ToLower();

            // 2. Validate ตาม game_type
            if (game.game_type == "single_player")
            {
                // Player vs AI → มีได้แค่ 1 record เฉพาะสี AI
                // ตรวจว่า ai_color ที่ส่งมาตรงกับฝั่ง AI จริง
                var whiteIsAi = game.white_player_type != "human";
                var blackIsAi = game.black_player_type != "human";

                var validAiColor = whiteIsAi ? "white" : blackIsAi ? "black" : null;

                if (aiColorNormalized != validAiColor)
                    throw new InvalidOperationException(
                        $"single_player mode: ai_color ต้องเป็น '{validAiColor}' ตามสีที่ AI เล่นจริง");
            }
            else if (game.game_type == "ai_vs_ai")
            {
                // AI vs AI → มีได้ 2 records (white + black) ไม่มากกว่านั้น
                if (aiColorNormalized != "white" && aiColorNormalized != "black")
                    throw new InvalidOperationException("ai_vs_ai mode: ai_color ต้องเป็น 'white' หรือ 'black' เท่านั้น");
            }
            else
            {
                // โหมดอื่น (online/local multiplayer) ไม่ควรมี ai_performance
                throw new InvalidOperationException(
                    $"game_type '{game.game_type}' ไม่รองรับการบันทึก AI performance");
            }

            // 3. เช็คซ้ำด้วย (game_id + ai_color) — key คู่นี้ต้อง unique
            var existingPerf = await _context.ai_performances
                .FirstOrDefaultAsync(p => p.game_id == dto.GameId
                                       && p.ai_color == aiColorNormalized);

            if (existingPerf != null)
            {
                // สี + เกมนี้มีอยู่แล้ว → Return ID เดิม ไม่ insert ซ้ำ
                return existingPerf.performance_id;
            }

            // 4. Insert record ใหม่
            var entity = new ai_performance
            {
                game_id = dto.GameId,
                ai_level = dto.AiLevel?.ToLower(),
                ai_color = aiColorNormalized,
                algorithm_type = dto.AlgorithmType?.ToLower(),
                average_depth = dto.AverageDepth,
                average_nodes_evaluated = dto.AverageNodesEvaluated,
                average_move_time_ms = dto.AverageMoveTimeMs,
                total_moves = dto.TotalMoves,
                created_at = DateTime.UtcNow
            };

            _context.ai_performances.Add(entity);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                throw new Exception($"Database Error: {ex.InnerException?.Message ?? ex.Message}");
            }

            return entity.performance_id;
        }

        public async Task<AiPerformanceDto?> GetAiPerformanceAsync(int id)
        {
            var e = await _context.ai_performances
                .FirstOrDefaultAsync(x => x.performance_id == id);

            if (e == null) return null;

            return new AiPerformanceDto
            {
                PerformanceId = e.performance_id,
                GameId = e.game_id,
                AiLevel = e.ai_level,
                AiColor = e.ai_color,
                AlgorithmType = e.algorithm_type,
                AverageDepth = (decimal)e.average_depth,
                AverageNodesEvaluated = (int)e.average_nodes_evaluated,
                AverageMoveTimeMs = (int)e.average_move_time_ms,
                TotalMoves = (int)e.total_moves
            };
        }

        public async Task UpdateAiPerformanceAsync(AiPerformanceUpdateDto dto)
        {
            var e = await _context.ai_performances
                .FirstOrDefaultAsync(x => x.performance_id == dto.PerformanceId);

            if (e == null) throw new Exception("record not found");

            if (e.game_id != dto.GameId)
            {
                var gameExists = await _context.games.AnyAsync(g => g.game_id == dto.GameId);
                if (!gameExists) throw new KeyNotFoundException($"Game ID {dto.GameId} not found");
            }

            e.game_id = dto.GameId;
            e.ai_level = dto.AiLevel;
            e.ai_color = dto.AiColor?.ToLower();
            e.algorithm_type = dto.AlgorithmType;
            e.average_depth = dto.AverageDepth;
            e.average_nodes_evaluated = dto.AverageNodesEvaluated;
            e.average_move_time_ms = dto.AverageMoveTimeMs;
            e.total_moves = dto.TotalMoves;

            await _context.SaveChangesAsync();
        }



        // ดึงข้อมูล Analytic ภาพรวมระบบ (สำหรับ Analytics Dashboard)
        public async Task<AiPerformanceResponseDto> GetAiPerformanceAnalyticAsync(int targetDepth = 4)
        {
            // 1. กำหนด Max Depth สูงสุดที่ระบบออกแบบไว้รองรับ (เช่น Max = 5)
            const int MAX_SUPPORTED_DEPTH = 5;
            const int MIN_SUPPORTED_DEPTH = 1;

            // Validate/Clamp ค่า targetDepth ไม่ให้เกินขอบเขต
            int validTargetDepth = Math.Clamp(targetDepth, MIN_SUPPORTED_DEPTH, MAX_SUPPORTED_DEPTH);

            // 2. ดึงเฉพาะ moves ที่ ai_depth_searched ไม่เกิน MAX_SUPPORTED_DEPTH
            var aiMoves = await _context.moves
                .Where(m => m.algorithm_type != null
                         && m.ai_depth_searched.HasValue
                         && m.ai_depth_searched.Value <= MAX_SUPPORTED_DEPTH)
                .ToListAsync();

            if (!aiMoves.Any())
            {
                return new AiPerformanceResponseDto { SelectedDepth = validTargetDepth };
            }

            // 3. Group ข้อมูลตาม Depth (จะถูกจำกัดไม่เกิน MAX_SUPPORTED_DEPTH)
            var benchmarkHistory = aiMoves
                .GroupBy(m => m.ai_depth_searched!.Value)
                .Where(g => g.Key <= MAX_SUPPORTED_DEPTH) // ป้องกันกรณีหลุดกรอบ
                .OrderBy(g => g.Key)
                .Select(g =>
                {
                    int depth = g.Key;
                    var minimaxMoves = g.Where(m => m.algorithm_type == "minimax").ToList();
                    var alphaBetaMoves = g.Where(m => m.algorithm_type == "alpha_beta").ToList();

                    // คำนวณ Nodes: ถ้าไม่มี Minimax ให้ใช้สูตรทฤษฎี (35 ^ depth)
                    long minimaxNodes = minimaxMoves.Any()
                        ? (long)minimaxMoves.Average(m => m.ai_nodes_evaluated ?? 0)
                        : (long)Math.Pow(35, depth);

                    long abNodes = alphaBetaMoves.Any()
                        ? (long)alphaBetaMoves.Average(m => m.ai_nodes_evaluated ?? 0)
                        : 0;

                    int abTime = alphaBetaMoves.Any()
                        ? (int)alphaBetaMoves.Average(m => m.move_time_ms ?? 0)
                        : 0;

                    int minimaxTime = minimaxMoves.Any()
                        ? (int)minimaxMoves.Average(m => m.move_time_ms ?? 0)
                        : (int)Math.Max(abTime, abTime * ((double)minimaxNodes / Math.Max(1, abNodes)));

                    return new AiBenchmarkDepthDto
                    {
                        Depth = depth,
                        MinimaxNodes = minimaxNodes,
                        AlphaBetaNodes = abNodes,
                        MinimaxTimeMs = minimaxTime,
                        AlphaBetaTimeMs = abTime
                    };
                }).ToList();

            // 4. ดึงข้อมูล DTO ตาม Depth ที่รับมา
            var targetData = benchmarkHistory.FirstOrDefault(b => b.Depth == validTargetDepth)
                             ?? benchmarkHistory.LastOrDefault()
                             ?? new AiBenchmarkDepthDto();

            return BuildResponseDto(validTargetDepth, targetData, benchmarkHistory);
        }

        // ดึงข้อมูล Analytics เฉพาะเกมใดเกมหนึ่ง
        public async Task<AiPerformanceResponseDto> GetAiPerformanceByIdAsync(int id)
        {
            var aiPerformance = await _context.ai_performances.FindAsync(id);
            if (aiPerformance == null)
                throw new KeyNotFoundException($"AI performance record not found for ID: {id}");

            // ดึง moves เฉพาะของ game_id นี้
            var aiMovesForBenchmark = await _context.moves
                .Where(m => m.game_id == aiPerformance.game_id && m.algorithm_type != null && m.ai_depth_searched.HasValue)
                .ToListAsync();

            var benchmarkHistory = aiMovesForBenchmark
                .GroupBy(m => m.ai_depth_searched!.Value)
                .OrderBy(g => g.Key)
                .Select(g =>
                {
                    int depth = g.Key;
                    var minimaxMoves = g.Where(m => m.algorithm_type == "minimax").ToList();
                    var alphaBetaMoves = g.Where(m => m.algorithm_type == "alpha_beta").ToList();

                    long minimaxNodes = minimaxMoves.Any()
                        ? (long)minimaxMoves.Average(m => m.ai_nodes_evaluated ?? 0)
                        : (long)Math.Pow(35, depth);

                    long abNodes = alphaBetaMoves.Any()
                        ? (long)alphaBetaMoves.Average(m => m.ai_nodes_evaluated ?? 0)
                        : 0;

                    int abTime = alphaBetaMoves.Any()
                        ? (int)alphaBetaMoves.Average(m => m.move_time_ms ?? 0)
                        : 0;

                    int minimaxTime = minimaxMoves.Any()
                        ? (int)minimaxMoves.Average(m => m.move_time_ms ?? 0)
                        : (int)Math.Max(abTime, abTime * ((double)minimaxNodes / Math.Max(1, abNodes)));

                    return new AiBenchmarkDepthDto
                    {
                        Depth = depth,
                        MinimaxNodes = minimaxNodes,
                        AlphaBetaNodes = abNodes,
                        MinimaxTimeMs = minimaxTime,
                        AlphaBetaTimeMs = abTime
                    };
                }).ToList();

            int targetDepth = Convert.ToInt32(aiPerformance.average_depth ?? 0);
            var targetData = benchmarkHistory.FirstOrDefault(b => b.Depth == targetDepth)
                             ?? benchmarkHistory.LastOrDefault()
                             ?? new AiBenchmarkDepthDto();

            return BuildResponseDto(targetDepth, targetData, benchmarkHistory);
        }

        // Helper Method สำหรับสร้าง Response DTO
        private static AiPerformanceResponseDto BuildResponseDto(int targetDepth, AiBenchmarkDepthDto targetData, List<AiBenchmarkDepthDto> benchmarkHistory)
        {
            double reductionRate = targetData.MinimaxNodes > 0
                ? (1.0 - ((double)targetData.AlphaBetaNodes / targetData.MinimaxNodes)) * 100
                : 0;

            double speedup = targetData.AlphaBetaTimeMs > 0
                ? (double)targetData.MinimaxTimeMs / targetData.AlphaBetaTimeMs
                : 1.0;

            return new AiPerformanceResponseDto
            {
                SelectedDepth = targetDepth,
                CurrentMetrics = new AiMetricSummaryDto
                {
                    MinimaxNodes = targetData.MinimaxNodes,
                    AlphaBetaNodes = targetData.AlphaBetaNodes,
                    MinimaxTimeMs = targetData.MinimaxTimeMs,
                    AlphaBetaTimeMs = targetData.AlphaBetaTimeMs,
                    NodeReductionRate = Math.Round(Math.Max(0, reductionRate), 1),
                    PruningEfficiency = Math.Round(Math.Max(0, reductionRate), 1),
                    SpeedupFactor = $"{speedup:F1}x"
                },
                BenchmarkHistory = benchmarkHistory
            };
        }

        public async Task<AiPerformanceAllResponse> GetAllAiPerformanceAsync()
        {
            var aiPerformances = await _context.ai_performances.ToListAsync();

            // Bug 4 fix: Select ต้องสร้าง AiPerformanceListDto ไม่ใช่ AiPerformanceAllResponse
            // Bug 5 fix: AverageNodesEvaluated และ AverageMoveTimeMs เป็น int ใน DTO ไม่ใช่ long
            var items = aiPerformances.Select(x => new AiPerformanceListDto
            {
                PerformanceId = (int)x.performance_id,
                GameId = x.game_id,           // game_id เป็น int ไม่ใช่ nullable
                AiLevel = x.ai_level ?? "",
                AiColor = x.ai_color ?? "",
                AlgorithmType = x.algorithm_type ?? "",
                AverageDepth = x.average_depth ?? 0m,
                AverageNodesEvaluated = x.average_nodes_evaluated ?? 0,
                AverageMoveTimeMs = x.average_move_time_ms ?? 0,
                TotalMoves = x.total_moves ?? 0
            }).ToList();

            if (items.Count > 0)
            {
                return new AiPerformanceAllResponse
                {
                    Message = "Data retrieved successfully",
                    Success = true,
                    data = items
                };
            }
            else
            {
                return new AiPerformanceAllResponse
                {
                    Message = "No data found",
                    Success = false,
                    data = new List<AiPerformanceListDto>()
                };
            }
        }
    }
}