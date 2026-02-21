using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using ChessApi.DbContext;
using ChessApi.DTOs.Matchmaking;
using ChessApi.Models;
using ChessApi.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using NuGet.Common;

namespace ChessApi.Services.Matchmaking
{
    public class MatchmakingService : IMatchmakingService
    {
        private readonly ChessDbContext _context;
        private const int QueueTtlSeconds = 300;

        public MatchmakingService(ChessDbContext context)
        {
            _context = context;
        }

        // JOIN QUEUE
        public async Task<MatchFoundDTOs?> JoinQueueAsync(JoinQueueDTOs request)
        {
            await using var tx = await _context.Database.BeginTransactionAsync(IsolationLevel.Serializable);

            var user = await _context.users
                .FirstOrDefaultAsync(u => u.username == request.Username);

            if (user == null)
                throw new Exception("User not found");

            var existingGame = await _context.games
                .Where(g =>
                    (g.white_player_id == user.user_id || g.black_player_id == user.user_id) &&
                    g.game_status == "in_progress" &&
                    g.result == null)
                .OrderByDescending(g => g.created_at)
                .FirstOrDefaultAsync();

            if (existingGame != null)
            {
                var existingMatch = await BuildMatchFoundAsync(user.user_id, existingGame);
                await tx.CommitAsync();
                return existingMatch;
            }

            if (string.Equals(user.status, "playing", StringComparison.OrdinalIgnoreCase))
                user.status = "online";

            string mode = request.MatchMode == 1 ? "ranked" : "normal";

            int minRating = request.MinRating;
            int maxRating = request.MaxRating;
            if (minRating > maxRating)
                (minRating, maxRating) = (maxRating, minRating);

            var myQueues = await _context.matchmaking_queues
                .Where(q => q.user_id == user.user_id)
                .ToListAsync();

            var myWaitingQueue = myQueues.FirstOrDefault(q => q.status == "waiting");
            var staleQueues = myQueues.Where(q => q.status != "waiting").ToList();
            if (staleQueues.Count > 0)
                _context.matchmaking_queues.RemoveRange(staleQueues);

            if (myWaitingQueue == null)
            {
                myWaitingQueue = new matchmaking_queue
                {
                    user_id = user.user_id,
                    status = "waiting"
                };
                _context.matchmaking_queues.Add(myWaitingQueue);
            }

            myWaitingQueue.min_rating = minRating;
            myWaitingQueue.max_rating = maxRating;
            myWaitingQueue.match_mode = mode;
            myWaitingQueue.joined_at = DateTime.UtcNow;

            int myRating = user.rating ?? 1200;

            var opponentCandidates = await _context.matchmaking_queues
                .Include(q => q.user)
                .Where(q =>
                    q.user_id != user.user_id &&
                    q.status == "waiting" &&
                    q.match_mode == mode &&
                    q.min_rating <= myRating &&
                    q.max_rating >= myRating)
                .OrderBy(q => q.joined_at)
                .ToListAsync();

            var opponentQueue = opponentCandidates.FirstOrDefault(q =>
            {
                int opponentRating = q.user.rating ?? 1200;
                return opponentRating >= minRating &&
                       opponentRating <= maxRating &&
                       !string.Equals(q.user.status, "playing", StringComparison.OrdinalIgnoreCase);
            });

            if (opponentQueue == null)
            {
                await _context.SaveChangesAsync();
                await tx.CommitAsync();
                return null;
            }

            bool isWhite = Random.Shared.Next(0, 2) == 0;
            var opponentUser = opponentQueue.user;

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

            user.status = "playing";
            opponentUser.status = "playing";

            opponentQueue.status = "matched";
            myWaitingQueue.status = "matched";

            await _context.SaveChangesAsync();
            await tx.CommitAsync();

            return new MatchFoundDTOs
            {
                GameId = newGame.game_id,
                RoomCode = newGame.game_id.ToString(),
                OpponentUsername = opponentUser.username,
                Color = (newGame.white_player_id == user.user_id) ? "white" : "black",
                GameType = newGame.game_type,
                MinRating = minRating,
                IsRated = string.Equals(mode, "ranked", StringComparison.OrdinalIgnoreCase),
                TimeControlMinutes = 0
            };
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
            {
                var waitingQueue = await _context.matchmaking_queues
                    .Where(q => q.user_id == user.user_id && q.status == "waiting")
                    .OrderByDescending(q => q.joined_at)
                    .FirstOrDefaultAsync();

                if (waitingQueue != null && waitingQueue.joined_at.HasValue)
                {
                    var ageSeconds = (DateTime.UtcNow - waitingQueue.joined_at.Value).TotalSeconds;
                    if (ageSeconds > QueueTtlSeconds)
                    {
                        waitingQueue.status = "expired";
                        await _context.SaveChangesAsync();
                    }
                }

                return null;
            }

            if (!string.Equals(user.status, "playing", StringComparison.OrdinalIgnoreCase))
                user.status = "playing";

            // Cleanup queue ตัวเอง
            var myQueues = await _context.matchmaking_queues
                .Where(q => q.user_id == user.user_id)
                .ToListAsync();

            if (myQueues.Count > 0)
            {
                foreach (var q in myQueues)
                    q.status = "matched";
            }

            await _context.SaveChangesAsync();

            return await BuildMatchFoundAsync(user.user_id, activeGame);
        }

        // CANCEL QUEUE
        public async Task CancelQueueAsync(CancelQueueDTOs request)
        {
            var user = await _context.users
                .FirstOrDefaultAsync(u => u.username == request.Username);

            if (user == null) return;

            var queueItems = await _context.matchmaking_queues
                .Where(q => q.user_id == user.user_id)
                .ToListAsync();

            if (queueItems.Count > 0)
            {
                foreach (var q in queueItems)
                    q.status = "cancelled";
                await _context.SaveChangesAsync();
            }
        }

        public async Task<string?> GetQueueStatusAsync(string username)
        {
            var user = await _context.users
                .FirstOrDefaultAsync(u => u.username == username);

            if (user == null) return null;

            var latestQueue = await _context.matchmaking_queues
                .Where(q => q.user_id == user.user_id)
                .OrderByDescending(q => q.joined_at)
                .FirstOrDefaultAsync();

            return latestQueue?.status;
        }

        private async Task<MatchFoundDTOs> BuildMatchFoundAsync(int userId, game activeGame)
        {
            int? opponentId = (activeGame.white_player_id == userId)
                ? activeGame.black_player_id
                : activeGame.white_player_id;

            string opponentUsername = "Unknown";
            if (opponentId.HasValue)
            {
                var opponent = await _context.users.FindAsync(opponentId.Value);
                if (opponent != null)
                    opponentUsername = opponent.username;
            }

            var latestQueue = await _context.matchmaking_queues
                .Where(q => q.user_id == userId && q.status == "matched")
                .OrderByDescending(q => q.joined_at)
                .FirstOrDefaultAsync();

            return new MatchFoundDTOs
            {
                GameId = activeGame.game_id,
                RoomCode = activeGame.game_id.ToString(),
                OpponentUsername = opponentUsername,
                Color = (activeGame.white_player_id == userId) ? "white" : "black",
                GameType = activeGame.game_type,
                MinRating = latestQueue?.min_rating ?? 0,
                IsRated = string.Equals(activeGame.match_mode, "ranked", StringComparison.OrdinalIgnoreCase),
                TimeControlMinutes = 0
            };
        }
    }
}
