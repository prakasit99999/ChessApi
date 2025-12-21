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
            var gameExists = await _context.games.AnyAsync(g => g.game_id == dto.GameId);

            if (!gameExists)
            {
                throw new KeyNotFoundException($"Game with ID {dto.GameId} does not exist.");
            }

            // -------------------------------------------------------------
            // ✅ เพิ่มการเช็คซ้ำ (Optional): ป้องกันการบันทึกข้อมูลซ้ำสำหรับเกมเดิม
            // -------------------------------------------------------------
            var existingPerf = await _context.ai_performances
                .FirstOrDefaultAsync(p => p.game_id == dto.GameId);

            if (existingPerf != null)
            {
                // ถ้ามีอยู่แล้ว ให้ Update แทน หรือ Return ID เดิมกลับไปเลย
                return existingPerf.performance_id;
            }

            var entity = new ai_performance
            {
                game_id = dto.GameId,

                // 🛠️ แก้ไข: แปลงเป็นตัวเล็กทั้งหมด (ToLower) เพื่อให้ตรงกับ Enum ใน Database
                ai_level = dto.AiLevel?.ToLower(),
                algorithm_type = dto.AlgorithmType?.ToLower(),

                average_depth = dto.AverageDepth,
                average_nodes_evaluated = dto.AverageNodesEvaluated,
                average_move_time_ms = dto.AverageMoveTimeMs,
                total_moves = dto.TotalMoves,

                // ✅ เพิ่ม: บันทึกเวลาปัจจุบัน
                created_at = DateTime.UtcNow
            };

            _context.ai_performances.Add(entity);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                // Handle Error กรณีข้อมูลไม่ถูกต้อง
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
            e.algorithm_type = dto.AlgorithmType;
            e.average_depth = dto.AverageDepth;
            e.average_nodes_evaluated = dto.AverageNodesEvaluated;
            e.average_move_time_ms = dto.AverageMoveTimeMs;
            e.total_moves = dto.TotalMoves;

            await _context.SaveChangesAsync();
        }
    }
}