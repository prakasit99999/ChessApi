using ChessApi.Services.Interfaces;
using ChessApi.Models;
using ChessApi.DTOs.Game;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ChessApi.DbContext;

namespace ChessApi.Services.Game
{
    public class GameService : IGameService
    {
        private readonly ChessDbContext _context;
        public GameService(ChessDbContext context)
        {
            _context = context;
        }

        public void StartGame()
        {
            // Implementation for starting a game
        }

        public void EndGame(int gameId, string result, string? reason = null)
        {
            var game = _context.games.Find(gameId);
            if (game != null)
            {
                game.result = result;
                game.result_reason = reason;
                game.game_status = "Finished";
                _context.SaveChanges();
            }
        }

        public async Task<GameResultDto?> GetGameResultAsync(int gameId)
        {
            var game = await _context.games.FirstOrDefaultAsync(g => g.game_id == gameId);
            if (game == null) return null;

            return new GameResultDto
            {
                GameId = game.game_id,
                GameType = game.game_type,
                WhitePlayerType = game.white_player_type,
                BlackPlayerType = game.black_player_type,
                Result = game.result,
                ResultReason = game.result_reason,
                MoveCount = game.move_count,
                WhiteRatingBefore = game.white_rating_before,
                WhiteRatingAfter = game.white_rating_after,
                BlackRatingBefore = game.black_rating_before,
                BlackRatingAfter = game.black_rating_after,
                IsRated = game.is_rated,
                TimeControlMinutes = game.time_control_minutes,
                CreatedAt = game.created_at,
                StartedAt = game.started_at,
                FinishedAt = game.finished_at
            };
        }
    }
}
