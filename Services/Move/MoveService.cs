﻿using ChessApi.DbContext;
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
                start_x = (sbyte)dto.StartX,
                start_y = (sbyte)dto.StartY,
                end_x = (sbyte)dto.EndX,
                end_y = (sbyte)dto.EndY,


                // ตัวหมากและทีม (แปลง Enum เป็นตัวเล็ก)
                piece_type = dto.PieceType.ToString().ToLower(),
                team = dto.PlayerTurn.ToString().ToLower(),

                // การกินหมาก
                captured_piece_type = dto.CapturedPieceType == MoveDto.capturePieceType.None
                                      ? null : dto.CapturedPieceType.ToString().ToLower(),

                captured_piece_team = dto.CapturedPieceTeam == MoveDto.capturedPicecTeam.None
                                      ? null : dto.CapturedPieceTeam.ToString().ToLower(),

                captured_x = dto.IsCapture ? (sbyte?)dto.CapturedX : null,
                captured_y = dto.IsCapture ? (sbyte?)dto.CapturedY : null,

                // การเลื่อนยศ
                promoted_from = dto.PromotedFrom == MoveDto.promotedFrom.None
                                ? null : dto.PromotedFrom.ToString().ToLower(),
                promoted_to = dto.PromotedTo == MoveDto.promotedTo.None
                              ? null : dto.PromotedTo.ToString().ToLower(),

                // ถ้ามีการเลื่อนยศ พิกัดคือจุดสิ้นสุด
                promoted_x = dto.PromotedTo != MoveDto.promotedTo.None ? (sbyte?)dto.EndX : null,
                promoted_y = dto.PromotedTo != MoveDto.promotedTo.None ? (sbyte?)dto.EndY : null,

                // En Passant (สมมติว่าตำแหน่งที่กินคือ EnPassant target)
                enpassant_x = dto.IsEnPassant ? (sbyte?)dto.EndX : null,
                enpassant_y = dto.IsEnPassant ? (sbyte?)dto.EndY : null,

                // Flags (สังเกตชื่อตัวแปร IsCasting ตาม DTO ของคุณ)
                is_castling = dto.IsCastling,
                is_en_passant = dto.IsEnPassant,
                is_capture = dto.IsCapture,
                is_check = dto.IsCheck,
                is_pawn_two_step = dto.IsPawnTwoStep,
                piece_has_moved_before = dto.PieceHasMovedBefore,

                // AI Stats
                algorithm_type = algoString,
                ai_evaluation_score = dto.AiEvaluationScore,
                ai_depth_searched = dto.AiDepthSearched,
                ai_nodes_evaluated = dto.AiNodesEvaluated,
                move_time_ms = dto.MoveTimeMilliseconds, // ชื่อตาม DTO ใหม่

                created_at = DateTime.UtcNow
            };

            // 4. บันทึกลงฐานข้อมูล
            _context.moves.Add(newMove);

            // 5. อัปเดต move_count ล่าสุดในตาราง Game
            game.move_count = dto.MoveNumber;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<MoveDto.LatestMoveResponseDto?> GetLatestMoveAsync(int gameId)
        {
            var lastMove = await _context.moves
                .Where(m => m.game_id == gameId)
                .OrderByDescending(m => m.move_number)
                .FirstOrDefaultAsync();

            if (lastMove == null) return null;

            return new MoveDto.LatestMoveResponseDto
            {
                MoveNumber = lastMove.move_number,
                StartX = lastMove.start_x,
                StartY = lastMove.start_y,
                EndX = lastMove.end_x,
                EndY = lastMove.end_y,
                FromPosition = $"{(char)('a' + lastMove.start_x)}{lastMove.start_y + 1}",
                ToPosition = $"{(char)('a' + lastMove.end_x)}{lastMove.end_y + 1}",
                PlayerTurn = lastMove.team,
                IsCastling = lastMove.is_castling ?? false,
                IsEnPassant = lastMove.is_en_passant ?? false,
                PromotedTo = lastMove.promoted_to switch
                {
                    "queen" => 5,
                    "rook" => 4,
                    "bishop" => 3,
                    "knight" => 2,
                    "pawn" => 1,
                    _ => 0 // None
                },

                // optional (เผื่ออนาคต)
                Status = null,
                Winner = null

            };
        }
    }
}