using ChessApi.DbContext;
using ChessApi.DTOs.AiPerformance;
using ChessApi.Models;
using ChessApi.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

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
            var entity = new ai_performance
            {
                game_id = dto.GameId,
                ai_level = dto.AiLevel,
                algorithm_type = dto.AlgorithmType,
                average_depth = dto.AverageDepth,
                average_nodes_evaluated = dto.AverageNodesEvaluated,
                average_move_time_ms = dto.AverageMoveTimeMs,
                total_moves = dto.TotalMoves
            };

            _context.ai_performances.Add(entity);
            await _context.SaveChangesAsync();

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

            e.game_id = dto.GameId;
            e.ai_level = dto.AiLevel;
            e.algorithm_type = dto.AlgorithmType;
            e.average_depth = dto.AverageDepth;
            e.average_nodes_evaluated = dto.AverageNodesEvaluated;
            e.average_move_time_ms = dto.AverageMoveTimeMs;
            e.total_moves = dto.TotalMoves;

            await _context.SaveChangesAsync();
        }
    }
}
