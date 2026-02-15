using System;
using System.Threading.Tasks;
using ChessApi.DbContext;
using ChessApi.DTOs.Game;
using ChessApi.Models;
using ChessApi.Services.Interfaces;
using ChessApi.Utilities.Helpers;
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

        // 1️     CREATE GAME
        public async Task<int> CreateGameAsync(GameCreateDto dto)
        {
            int? whiteId = dto.WhitePlayerId;
            int? blackId = dto.BlackPlayerId;

            if (whiteId <= 0) whiteId = null;
            if (blackId <= 0) blackId = null;

            string status = "in_progress";

            switch (dto.GameType.ToLower())
            {
                case "online_multiplayer":
                    status = (whiteId.HasValue && blackId.HasValue)
                        ? "in_progress"
                        : "waiting_for_opponent";
                    break;

                case "single_player":
                case "ai_vs_ai":
                    status = "in_progress";
                    break;

                default:
                    status = "in_progress";
                    break;
            }

            var newGame = new game
            {
                game_type = dto.GameType,
                match_mode = dto.MatchMode == 1
                                ? "ranked"
                                : dto.MatchMode == 2
                                    ? "normal"
                                    : null,

                white_player_type = dto.WhitePlayerType,
                black_player_type = dto.BlackPlayerType,

                white_player_id = whiteId,
                black_player_id = blackId,

                game_status = status,
                move_count = 0,

                created_at = DateTime.UtcNow,
                started_at = status == "in_progress" ? DateTime.UtcNow : null,

                result = null,
                result_reason = null
            };

            _context.games.Add(newGame);
            await _context.SaveChangesAsync();

            return newGame.game_id;
        }

        // 2️ FINALIZE GAME
        public async Task<(bool Success, int? WhiteRating, int? BlackRating)>
            FinalizeGameAsync(GameResultDto dto)
        {
            var game = await GetGameWithPlayers(dto.GameId);
            if (game == null) return (false, null, null);

            // กันยิงซ้ำ (Concurrency Guard)
            if (game.game_status != "in_progress")
                return (false, null, null);

            using var tx = await _context.Database.BeginTransactionAsync();

            try
            {
                string rawResult = dto.Result?.ToLower() ?? "draw";

                game.game_status = "finished";
                game.result = MapResultToEnum.MapResultToString(rawResult);
                game.result_reason = dto.ResultReason;
                game.finished_at = DateTime.UtcNow;

                string winnerColor = rawResult switch
                {
                    "white_wins" => "white",
                    "black_wins" => "black",
                    "white" => "white",
                    "black" => "black",
                    _ => "draw"
                };

                UpdatePlayerStats(game, winnerColor);

                await _context.SaveChangesAsync();

                // Rating เฉพาะ Ranked Online
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

                await tx.CommitAsync();

                if (game.white_player != null)
                    await _context.Entry(game.white_player).ReloadAsync();

                if (game.black_player != null)
                    await _context.Entry(game.black_player).ReloadAsync();

                return (true,
                    game.white_player?.rating,
                    game.black_player?.rating);
            }
            catch
            {
                await tx.RollbackAsync();
                return (false, null, null);
            }
        }

        // 3️ RESIGN / ABORT
        public async Task<(bool Success, int? WhiteRating, int? BlackRating)>
            ResignGameAsync(int gameId, int playerId, string reason)
        {
            var game = await GetGameWithPlayers(gameId);
            if (game == null || game.game_status != "in_progress")
                return (false, null, null);

            bool isWhite = game.white_player_id == playerId;
            bool isBlack = game.black_player_id == playerId;

            if (!isWhite && !isBlack)
                return (false, null, null);

            using var tx = await _context.Database.BeginTransactionAsync();

            try
            {
                // ถ้าเดินไม่ถึง 2 ตา → abandoned
                if (game.move_count < 2)
                {
                    game.game_status = "finished";
                    game.result = "abandoned";
                    game.result_reason = "game_aborted_early";
                    game.finished_at = DateTime.UtcNow;

                    await _context.SaveChangesAsync();
                    await tx.CommitAsync();

                    return (true,
                        game.white_player?.rating,
                        game.black_player?.rating);
                }

                string winnerColor = isWhite ? "black" : "white";

                game.game_status = "finished";
                game.result = MapResultToEnum.MapResultToString(winnerColor);
                game.result_reason = string.IsNullOrEmpty(reason)
                                        ? "resignation"
                                        : reason;
                game.finished_at = DateTime.UtcNow;

                UpdatePlayerStats(game, winnerColor);

                await _context.SaveChangesAsync();

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

                await tx.CommitAsync();

                if (game.white_player != null)
                    await _context.Entry(game.white_player).ReloadAsync();

                if (game.black_player != null)
                    await _context.Entry(game.black_player).ReloadAsync();

                return (true,
                    game.white_player?.rating,
                    game.black_player?.rating);
            }
            catch
            {
                await tx.RollbackAsync();
                return (false, null, null);
            }
        }
        // 4️ GET RESULT
        public async Task<GameResultDto?> GetGameResultAsync(int gameID)
        {
            var game = await _context.games
                .FirstOrDefaultAsync(g => g.game_id == gameID);

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

        // 5️ GET END GAME INFO (for permission checks)
        public async Task<(bool Found, string? GameType, string? GameStatus, int? WhitePlayerId, int? BlackPlayerId)>
            GetEndGameInfoAsync(int gameId)
        {
            var game = await _context.games
                .AsNoTracking()
                .Where(g => g.game_id == gameId)
                .Select(g => new
                {
                    g.game_type,
                    g.game_status,
                    g.white_player_id,
                    g.black_player_id
                })
                .FirstOrDefaultAsync();

            if (game == null)
                return (false, null, null, null, null);

            return (true,
                game.game_type,
                game.game_status,
                game.white_player_id,
                game.black_player_id);
        }

        // PRIVATE METHODS

        private async Task<game?> GetGameWithPlayers(int gameId)
        {
            return await _context.games
                .Include(g => g.white_player)
                .Include(g => g.black_player)
                .FirstOrDefaultAsync(g => g.game_id == gameId);
        }

     

        private void UpdatePlayerStats(game game, string winner)
        {
            if (game.game_type != "online_multiplayer") return;

            if (game.white_player != null)
            {
                game.white_player.games_played++;

                if (winner == "white")
                    game.white_player.games_won++;
                else if (winner == "black")
                    game.white_player.games_lost++;
                else
                    game.white_player.games_drawn++;
            }

            if (game.black_player != null)
            {
                game.black_player.games_played++;

                if (winner == "black")
                    game.black_player.games_won++;
                else if (winner == "white")
                    game.black_player.games_lost++;
                else
                    game.black_player.games_drawn++;
            }
        }
    }
}
