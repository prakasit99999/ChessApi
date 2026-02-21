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

        // เพิ่ม userId เข้ามา (nullable)
        public async Task<bool> MakeMoveAsync(MoveDto.MoveRequest dto, int? userId)
        {
            // 1️ ตรวจสอบ Game
            var game = await _context.games
                .FirstOrDefaultAsync(g => g.game_id == dto.GameId);

            if (game == null)
                return false;

            // 2️ ต้องกำลังเล่นอยู่
            if (game.game_status != "in_progress")
                return false;

            // 3️ (Optional) Allow Local Multiplayer if needed
            if (game.game_type == "local_multiplayer")
                return false;

            // Security Check สำหรับ Online เท่านั้น
            if (game.game_type == "online_multiplayer")
            {
                // ต้อง Login
                if (!userId.HasValue)
                    return false;

                // ต้องเป็นผู้เล่นในเกมนี้
                if (game.white_player_id != userId &&
                    game.black_player_id != userId)
                    return false;

                // ต้องเป็น Turn ของเขา
                bool isWhiteTurn = dto.PlayerTurn.ToString().ToLower() == "white";

                if (isWhiteTurn && game.white_player_id != userId)
                    return false;

                if (!isWhiteTurn && game.black_player_id != userId)
                    return false;
            }

            // 5️ ป้องกัน MoveNumber มั่ว
            if (dto.MoveNumber != game.move_count + 1)
                return false;

            // 6️ Map Algorithm
            string? algoString = null;
            if (dto.AlgorithmType == MoveDto.algorithmType.AlphaBeta)
                algoString = "alpha_beta";
            else if (dto.AlgorithmType == MoveDto.algorithmType.Minimax)
                algoString = "minimax";

            // 7️ สร้าง Move Entity
            var newMove = new move
            {
                game_id = dto.GameId,
                move_number = dto.MoveNumber,

                start_x = (sbyte)dto.StartX,
                start_y = (sbyte)dto.StartY,
                end_x = (sbyte)dto.EndX,
                end_y = (sbyte)dto.EndY,

                piece_type = dto.PieceType.ToString().ToLower(),
                team = dto.PlayerTurn.ToString().ToLower(),

                captured_piece_type =
                    dto.CapturedPieceType == MoveDto.capturePieceType.None
                    ? null
                    : dto.CapturedPieceType.ToString().ToLower(),

                captured_piece_team =
                    dto.CapturedPieceTeam == MoveDto.capturedPicecTeam.None
                    ? null
                    : dto.CapturedPieceTeam.ToString().ToLower(),

                captured_x = dto.IsCapture ? (sbyte?)dto.CapturedX : null,
                captured_y = dto.IsCapture ? (sbyte?)dto.CapturedY : null,

                promoted_from =
                    dto.PromotedFrom == MoveDto.promotedFrom.None
                    ? null
                    : dto.PromotedFrom.ToString().ToLower(),

                promoted_to =
                    dto.PromotedTo == MoveDto.promotedTo.None
                    ? null
                    : dto.PromotedTo.ToString().ToLower(),

                promoted_x =
                    dto.PromotedTo != MoveDto.promotedTo.None
                    ? (sbyte?)dto.EndX : null,

                promoted_y =
                    dto.PromotedTo != MoveDto.promotedTo.None
                    ? (sbyte?)dto.EndY : null,

                enpassant_x =
                    dto.IsEnPassant ? (sbyte?)dto.EndX : null,

                enpassant_y =
                    dto.IsEnPassant ? (sbyte?)dto.EndY : null,

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
                move_time_ms = dto.MoveTimeMilliseconds,

                created_at = DateTime.UtcNow
            };

            _context.moves.Add(newMove);

            // 8️⃣ Update move_count
            game.move_count = dto.MoveNumber;

            await _context.SaveChangesAsync();
            return true;
        }

        // =========================================================
        // Latest Move
        // =========================================================
        public async Task<MoveDto.LatestMoveResponseDto?> GetLatestMoveAsync(int gameId)
        {
            var lastMove = await _context.moves
                .Where(m => m.game_id == gameId)
                .OrderByDescending(m => m.move_number)
                .FirstOrDefaultAsync();

            if (lastMove == null)
                return null;

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
                    _ => 0
                },

                Status = null,
                Winner = null
            };
        }
    }
}
