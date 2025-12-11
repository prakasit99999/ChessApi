//using ChessApi.Services.Interfaces;
////using ChessApi.Models;
//using ChessApi.DTOs.Game;
//using System.Collections.Generic;
//using System.Threading.Tasks;
//using Microsoft.EntityFrameworkCore;
////using ChessApi.DbContext;

//namespace ChessApi.Services.Game
//{
//    public class GameService : IGameService
//    {
//        private readonly ChessDbContext _context;
//        public GameService(ChessDbContext context)
//        {
//            _context = context;
//        }


//        public async Task<GameResultDto?> GetGameResultAsync(int gameID)
//        {
//            var game = await _context.games.FirstOrDefaultAsync(g => g.game_id == gameID);
//            if (game == null) return null;

//            return new GameResultDto
//            {
//                GameId = game.game_id,
//                GameType = game.game_type,
//                WhitePlayerType = game.white_player_type,
//                BlackPlayerType = game.black_player_type,
//                Result = game.result,
//                ResultReason = game.result_reason,
//                MoveCount = game.move_count,
//                WhiteRatingBefore = game.white_rating_before,
//                WhiteRatingAfter = game.white_rating_after,
//                BlackRatingBefore = game.black_rating_before,
//                BlackRatingAfter = game.black_rating_after,
//                IsRated = game.is_rated,
//                TimeControlMinutes = game.time_control_minutes,
//                CreatedAt = game.created_at,
//                StartedAt = game.started_at,
//                FinishedAt = game.finished_at
//            };
//        }

//        public async Task<bool> FinalizeGameAsync(GameResultDto dto)
//        {
//            var game = await _context.games
//                .Include(g => g.white_player)
//                .Include(g => g.black_player)
//                .FirstOrDefaultAsync(g => g.game_id == dto.GameId);

//            if (game == null) return false;

//            // อัปเดตสถานะเกม
//            game.result = dto.Result;
//            game.result_reason = dto.ResultReason;
//            game.game_status = "Finished";
//            game.finished_at = DateTime.UtcNow;

//            // อัปเดตสถิติผู้เล่น
//            if (game.white_player != null && game.black_player != null)
//            {
//                game.white_player.games_played++;
//                game.black_player.games_played++;

//                switch (dto.Result)
//                {
//                    case "white":
//                        game.white_player.games_won++;
//                        game.black_player.games_lost++;
//                        break;
//                    case "black":
//                        game.black_player.games_won++;
//                        game.white_player.games_lost++;
//                        break;
//                    case "draw":
//                        game.white_player.games_drawn++;
//                        game.black_player.games_drawn++;
//                        break;
//                }
//            }

//            await _context.SaveChangesAsync();
//            return true;
//        }

//    }
//}
