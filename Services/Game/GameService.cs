using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ChessApi.DbContext;
using ChessApi.DTOs.Game;
using ChessApi.Models;
using ChessApi.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ChessApi.Services.Game
{
    public class GameService : IGameService
    {
        private readonly ChessDbContext _context;
        private readonly IRatingService _ratingService;

        public GameService(ChessDbContext context, IRatingService ratingService)
        {
            _context = context;
            _ratingService = ratingService;
        }

        public async Task<int> CreateGameAsync(GameCreateDto dto)
        {
            int? whiteId = dto.WhitePlayerId;
            int? blackId = dto.BlackPlayerId;
            string status = "in_progress";

            // Normalize IDs: ถ้า ID <= 0 ให้เป็น null (Guest)
            if (whiteId <= 0) whiteId = null;
            if (blackId <= 0) blackId = null;

            switch (dto.GameType.ToLower())
            {
                case "online_multiplayer":
                    // Online: WhiteId มาจาก Controller (User), BlackId เป็น null (รอคน Join)
                    blackId = null;
                    status = "waiting_for_opponent";
                    break;

                case "single_player":
                case "ai_vs_ai":
                case "local_multiplayer":
                    // Offline: WhiteId (User/Guest), BlackId (AI/Guest -> null)
                    // เริ่มเกมได้เลย
                    blackId = null;
                    status = "in_progress";
                    break;

                default:
                    break;
            }

            var newGame = new Models.game
            {
                game_type = dto.GameType,
                match_mode = dto.MatchMode == 1 ? "ranked" : dto.MatchMode == 2 ? "normal" : null,
                white_player_type = dto.WhitePlayerType,
                black_player_type = dto.BlackPlayerType,
                game_status = status,
                move_count = 0,
                created_at = DateTime.UtcNow,
                started_at = DateTime.UtcNow,
                white_player_id = whiteId,
                black_player_id = blackId,
                result = null,
                result_reason = null
            };

            _context.games.Add(newGame);
            await _context.SaveChangesAsync();
            return newGame.game_id;
        }

        //  2. End Game
        public async Task<bool> FinalizeGameAsync(GameResultDto dto)
        {
            var game = await GetGameWithPlayers(dto.GameId);
            if (game == null) return false;

            //  กันยิงซ้ำ
            if (game.game_status == "finished")
                return false;

            string rawResult = dto.Result?.ToLower() ?? "draw";

            game.game_status = "finished";
            game.result = MapResultToEnum(rawResult);
            game.result_reason = dto.ResultReason;
            game.finished_at = DateTime.UtcNow;

            // อัปเดตสถิติ (Win/Loss/Draw) สำหรับ Online Games
            string winnerColor = rawResult switch
            {
                "white_wins" => "white",
                "black_wins" => "black",
                _ => "draw"
            };
            UpdatePlayerStats(game, winnerColor);


            await _context.SaveChangesAsync();

            //  ตัดคะแนนเฉพาะ Ranked
            bool isRankedOnline =
                game.game_type == "online_multiplayer" &&
                game.match_mode == "ranked" &&
                game.white_player_id.HasValue &&
                game.black_player_id.HasValue &&
                rawResult != "abandoned";

            if (isRankedOnline)
            {
                await _ratingService.ProcessGameResultAsync(
                    game.white_player_id.Value,
                    game.black_player_id.Value,
                    winnerColor
                );
            }

            return true;
        }


        //  3. Resign / Abort
        public async Task<bool> ResignGameAsync(int gameId, int playerId, string reason)
        {
            var game = await GetGameWithPlayers(gameId);
            if (game == null || game.game_status != "in_progress")
                return false;

            bool isWhite = game.white_player_id == playerId;
            bool isBlack = game.black_player_id == playerId;
            if (!isWhite && !isBlack) return false;

            // เดิน < 2 = โมฆะ
            if (game.move_count < 2)
            {
                game.game_status = "finished";
                game.result = "abandoned";
                game.result_reason = "game_aborted_early";
                game.finished_at = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                return true;
            }

            string winnerColor = isWhite ? "black" : "white";

            game.game_status = "finished";
            game.result = MapResultToEnum(winnerColor);
            game.result_reason = string.IsNullOrEmpty(reason) ? "resignation" : reason;
            game.finished_at = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            //  ตัดคะแนนเฉพาะ Ranked
            if (
                game.game_type == "online_multiplayer" &&
                game.match_mode == "ranked" &&
                game.white_player_id.HasValue &&
                game.black_player_id.HasValue
            )
            {
                await _ratingService.ProcessGameResultAsync(
                    game.white_player_id.Value,
                    game.black_player_id.Value,
                    winnerColor
                );
            }

            return true;
        }


        public async Task<GameResultDto?> GetGameResultAsync(int gameID)
        {
            var game = await _context.games.FirstOrDefaultAsync(g => g.game_id == gameID);
            if (game == null) return null;

            return new GameResultDto
            {
                GameId = game.game_id,
                GameType = game.game_type,
                MatchMode = game.match_mode,
                WhitePlayerType = game.white_player_type,
                BlackPlayerType = game.black_player_type,
                Result = game.result,
                ResultReason = game.result_reason,
                MoveCount = game.move_count,
                CreatedAt = game.created_at,
                StartedAt = game.started_at,
                FinishedAt = game.finished_at
            };
        }

        private async Task<game?> GetGameWithPlayers(int gameId)
        {
            return await _context.games
                .Include(g => g.white_player)
                .Include(g => g.black_player)
                .FirstOrDefaultAsync(g => g.game_id == gameId);
        }

        private string MapResultToEnum(string input)
        {
            return input switch
            {
                "white" => "white_wins",
                "black" => "black_wins",
                "draw" => "draw",
                "abandoned" => "abandoned",
                _ => "draw"
            };
        }

        private void UpdatePlayerStats(game game, string winner)
        {
            // กันเหนียวอีกชั้น
            if (game.game_type != "online_multiplayer") return;

            if (game.white_player != null)
            {
                game.white_player.games_played++;
                if (winner == "white") game.white_player.games_won++;
                else if (winner == "black") game.white_player.games_lost++;
                else game.white_player.games_drawn++;
            }

            if (game.black_player != null)
            {
                game.black_player.games_played++;
                if (winner == "black") game.black_player.games_won++;
                else if (winner == "white") game.black_player.games_lost++;
                else game.black_player.games_drawn++;
            }
        }
    }
}