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
    }
}