using ChessApi.DbContext;
using ChessApi.DTOs.Game;
using ChessApi.Models;
using ChessApi.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ChessApi.Services.Move
{
    public class MoveService : IMoveService
    {
        private readonly ChessDbContext _context;

        public MoveService(ChessDbContext context)
        {
            _context = context;
        }

        public async Task<bool> MakeMoveAsync(MoveDto.MoveRequest dto)
        {
            // 1. ตรวจสอบว่า Game นี้มีอยู่จริง และกำลังเล่นอยู่
            var game = await _context.games.FirstOrDefaultAsync(g => g.game_id == dto.GameId);

            if (game == null) return false;
            // หมายเหตุ: ถ้าอยากให้บันทึกได้แม้จบเกมแล้ว ให้ลบเงื่อนไข game_status ออก
            // if (game.game_status != "in_progress" && game.game_status != null) return false;

            // 2. แปลง Algorithm Type ให้ตรงกับ Database Enum ('minimax', 'alpha_beta')
            string? algoString = null;// default
            if (dto.AlgorithmType == MoveDto.algorithmType.AlphaBeta) algoString = "alpha_beta";
            else if (dto.AlgorithmType == MoveDto.algorithmType.Minimax) algoString = "minimax";

            // 3. Map ข้อมูลจาก DTO -> Entity
            var newMove = new move
            {
                game_id = dto.GameId,
                move_number = dto.MoveNumber,

                // พิกัด
                start_x = (sbyte)dto.startX,
                start_y = (sbyte)dto.startY,
                end_x = (sbyte)dto.endX,
                end_y = (sbyte)dto.endY,

                // ตัวหมากและทีม (แปลง Enum เป็นตัวเล็ก)
                piece_type = dto.PieceType.ToString().ToLower(),
                team = dto.PlayerTurn.ToString().ToLower(),

                // การกินหมาก
                captured_piece_type = dto.CapturedPieceType == MoveDto.capturePieceType.None
                                      ? null : dto.CapturedPieceType.ToString().ToLower(),

                captured_piece_team = dto.CapturedPieceTeam == MoveDto.capturedPicecTeam.None
                                      ? null : dto.CapturedPieceTeam.ToString().ToLower(),

                captured_x = dto.IsCapture ? (sbyte?)dto.capturedX : null,
                captured_y = dto.IsCapture ? (sbyte?)dto.capturedY : null,

                // การเลื่อนยศ
                promoted_from = dto.PromotedFrom == MoveDto.promotedFrom.None
                                ? null : dto.PromotedFrom.ToString().ToLower(),
                promoted_to = dto.PromotedTo == MoveDto.promotedTo.None
                              ? null : dto.PromotedTo.ToString().ToLower(),

                // ถ้ามีการเลื่อนยศ พิกัดคือจุดสิ้นสุด
                promoted_x = dto.PromotedTo != MoveDto.promotedTo.None ? (sbyte?)dto.endX : null,
                promoted_y = dto.PromotedTo != MoveDto.promotedTo.None ? (sbyte?)dto.endY : null,

                // En Passant (สมมติว่าตำแหน่งที่กินคือ EnPassant target)
                enpassant_x = dto.IsEnPassant ? (sbyte?)dto.endX : null,
                enpassant_y = dto.IsEnPassant ? (sbyte?)dto.endY : null,

                // Flags (สังเกตชื่อตัวแปร IsCasting ตาม DTO ของคุณ)
                is_castling = dto.IsCasting,
                is_en_passant = dto.IsEnPassant,
                is_capture = dto.IsCapture,
                is_check = dto.IsCheck,
                is_pawn_two_step = dto.IsPawnTwoStep,
                piece_has_moved_before = dto.PieceHasMovedBefore,

                // AI Stats
                algorithm_type = algoString,
                ai_evaluation_score = dto.aiEvaluationScore,
                ai_depth_searched = dto.aiDepthSearched,
                ai_nodes_evaluated = dto.aiNodesEvaluated,
                move_time_ms = dto.moveTimeMilliseconds, // ชื่อตาม DTO ใหม่

                created_at = DateTime.UtcNow
            };

            // 4. บันทึกลงฐานข้อมูล
            _context.moves.Add(newMove);

            // 5. อัปเดต move_count ล่าสุดในตาราง Game
            game.move_count = dto.MoveNumber;

            await _context.SaveChangesAsync();
            return true;
        }
    }
}