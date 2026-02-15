using System;
using System.Linq;
using System.Threading.Tasks;
using ChessApi.DbContext;
using ChessApi.DTOs.Matchmaking;
using ChessApi.Models;
using ChessApi.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ChessApi.Services.Matchmaking
{
    public class MatchmakingService : IMatchmakingService
    {
        private readonly ChessDbContext _context;

        public MatchmakingService(ChessDbContext context)
        {
            _context = context;
        }

        // JOIN QUEUE
        public async Task<MatchFoundDTOs?> JoinQueueAsync(JoinQueueDTOs request)
        {
            var user = await _context.users
                .FirstOrDefaultAsync(u => u.username == request.Username);

            if (user == null)
                throw new Exception("User not found");

            string mode = request.MatchMode == 1 ? "ranked" : "normal";

            // ลบคิวเก่าของตัวเอง (กันเข้าซ้ำ)
            var oldQueue = await _context.matchmaking_queues
                .FirstOrDefaultAsync(q => q.user_id == user.user_id);

            if (oldQueue != null)
                _context.matchmaking_queues.Remove(oldQueue);

            await _context.SaveChangesAsync();

            // หา opponent ที่ match mode เดียวกัน
            var opponentQueue = await _context.matchmaking_queues
                .FirstOrDefaultAsync(q =>
                    q.user_id != user.user_id &&
                    q.status == "waiting" &&
                    q.match_mode == mode &&
                    q.min_rating <= user.rating &&
                    q.max_rating >= user.rating);

            if (opponentQueue != null)
            {
                var opponentUser = await _context.users
                    .FindAsync(opponentQueue.user_id);

                // สุ่มสี
                bool isWhite = new Random().Next(0, 2) == 0;

                var newGame = new game
                {
                    game_type = "online_multiplayer",
                    match_mode = mode,
                    white_player_id = isWhite ? user.user_id : opponentQueue.user_id,
                    black_player_id = isWhite ? opponentQueue.user_id : user.user_id,
                    white_player_type = "human",
                    black_player_type = "human",
                    game_status = "in_progress",
                    move_count = 0,
                    created_at = DateTime.UtcNow,
                    started_at = DateTime.UtcNow
                };

                _context.games.Add(newGame);

                // ลบคิว opponent
                _context.matchmaking_queues.Remove(opponentQueue);

                await _context.SaveChangesAsync();

                return new MatchFoundDTOs
                {
                    GameId = newGame.game_id,
                    RoomCode = newGame.game_id.ToString(),
                    OpponentUsername = opponentUser?.username ?? "Unknown",
                    Color = (newGame.white_player_id == user.user_id) ? "white" : "black",
                    GameType = "online_multiplayer"
                };
            }

            // ถ้าไม่เจอคู่ → เข้าคิว
            var queueItem = new matchmaking_queue
            {
                user_id = user.user_id,
                min_rating = request.MinRating,
                max_rating = request.MaxRating,
                status = "waiting",
                match_mode = mode,
                joined_at = DateTime.UtcNow
            };

            _context.matchmaking_queues.Add(queueItem);
            await _context.SaveChangesAsync();

            return null;
        }

        // CHECK MATCH (Polling)
        public async Task<MatchFoundDTOs?> CheckForMatchAsync(string username)
        {
            var user = await _context.users
                .FirstOrDefaultAsync(u => u.username == username);

            if (user == null) return null;

            var activeGame = await _context.games
                .Where(g =>
                    (g.white_player_id == user.user_id ||
                     g.black_player_id == user.user_id)
                    && g.game_status == "in_progress"
                    && g.result == null)
                .OrderByDescending(g => g.created_at)
                .FirstOrDefaultAsync();

            if (activeGame == null)
                return null;

            int opponentId = (activeGame.white_player_id == user.user_id)
                ? activeGame.black_player_id.Value
                : activeGame.white_player_id.Value;

            var opponentUser = await _context.users.FindAsync(opponentId);

            // Cleanup queue ตัวเอง
            var myQueue = await _context.matchmaking_queues
                .FirstOrDefaultAsync(q => q.user_id == user.user_id);

            if (myQueue != null)
            {
                _context.matchmaking_queues.Remove(myQueue);
                await _context.SaveChangesAsync();
            }

            return new MatchFoundDTOs
            {
                GameId = activeGame.game_id,
                RoomCode = activeGame.game_id.ToString(),
                OpponentUsername = opponentUser?.username ?? "Unknown",
                Color = (activeGame.white_player_id == user.user_id) ? "white" : "black",
                GameType = "online_multiplayer"
            };
        }

        // CANCEL QUEUE
        public async Task CancelQueueAsync(CancelQueueDTOs request)
        {
            var user = await _context.users
                .FirstOrDefaultAsync(u => u.username == request.Username);

            if (user == null) return;

            var queueItem = await _context.matchmaking_queues
                .FirstOrDefaultAsync(q => q.user_id == user.user_id);

            if (queueItem != null)
            {
                _context.matchmaking_queues.Remove(queueItem);
                await _context.SaveChangesAsync();
            }
        }
    }
}
