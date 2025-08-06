using System;
using System.Threading.Tasks;
using ChessApi.DbContext;
using ChessApi.DTOs.Matchmaking;
using ChessApi.DTOs.Room;
using ChessApi.Models;
using ChessApi.Services.Interfaces;
using ChessApi.Validations.Matchmaking;
using Microsoft.EntityFrameworkCore;
namespace ChessApi.Services.Matchmaking
{
    public class MatchmakingService : IMatchmakingService
    {
        private readonly ChessDbContext _context;
        private readonly ILogger<MatchmakingService> _logger;
        private readonly IRoomService _roomService;

        public MatchmakingService(
                 ChessDbContext context,
                 ILogger<MatchmakingService> logger,
                 IRoomService roomService)
        {
            _context = context;
            _logger = logger;
            _roomService = roomService;
        }

        public async Task JoinQueueAsync(JoinQueueRequest request)
        {
            //validation
            JoinQueueValidator.Validate(request);
            // Implementation for joining the matchmaking queue
            // This would typically involve adding the user to a queue in the database
            var user = await _context.users.FindAsync(request.Username);
            if (user == null)
            {
                throw new Exception("User not found");
            }
            // ตรวจสอบว่ามีคู่รออยู่ไหม
            var existingMatch = await _context.matchmaking_queues
                .Include(m => m.user)
                .Where(m => m.user.rating >= request.MinRating &&
                            m.user.rating <= request.MaxRating &&
                            m.user.username != request.Username)
                .OrderBy(m => Math.Abs(m.user.rating - user.rating ?? 1000))
                .FirstOrDefaultAsync();
            if (existingMatch != null)
            {
                //สร้าง room
                var roomDto = await _roomService.CreateRoomAsync(new CreateRoomRequest
                {
                    HostUsername = request.Username,
                    RoomName = $"{request.Username} vs {existingMatch.user.username}",
                    MaxParticipants = 2,
                    TimeControlMinutes = request.PreferredTimeControl,
                    IsRated = true // Assuming rated by default
                });

                //สร้าง Game 
                var game = new game
                {
                    room_id = roomDto.RoomId,
                    game_type = "ranked",
                    white_player_id = user.user_id,
                    black_player_id = existingMatch.user_id,
                    white_player_type = "human",
                    black_player_type = "human",
                    game_status = "waiting",
                    current_turn = "white",
                    is_rated = true,
                    time_control_minutes = request.PreferredTimeControl,
                    created_at = DateTime.UtcNow
                };
                _context.games.Add(game);
                // ลบทั้ง 2 ออกจาก queue
                _context.matchmaking_queues.Remove(existingMatch);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Match created: {user.username} vs {existingMatch.user.username}");
            }
            else
            {
                // ใส่ตัวเองเข้า queue
                _context.matchmaking_queues.Add(new matchmaking_queue
                {
                    user_id = user.user_id,
                    max_rating = user.rating ?? 1000,
                    preferred_time_control = request.PreferredTimeControl
                });

                await _context.SaveChangesAsync();
                _logger.LogInformation($"{user.username} joined matchmaking queue");

            }

        }
    

        public async Task CancelQueueAsync(CancelQueueRequest request)
        {
            //validation
             CancelQueueValidator.Validate(request);
            // Implementation for canceling the matchmaking queue
            var user = await _context.users.FindAsync(request.Username);
            if (user == null)
            {
                throw new Exception("User not found");
            }
            //ลบจาก matchmaking_queue โดยใช้ username หรือ user_id
            var queueEntry = await _context.matchmaking_queues
                .FirstOrDefaultAsync(m => m.user_id == user.user_id);
            if (queueEntry != null)
                {
                _context.matchmaking_queues.Remove(queueEntry);
                await _context.SaveChangesAsync();
                _logger.LogInformation($"{user.username} canceled matchmaking queue");
            }
            else
            {
                _logger.LogWarning($"{user.username} was not in the matchmaking queue");
            }
           

        }

        public async Task<MatchFoundResponse?> CheckForMatchAsync(string username)
        {
            //validation
            if (string.IsNullOrEmpty(username))
            {
                throw new ArgumentException("Username cannot be null or empty");
            }
            var user = await _context.users
                .Include(u => u.matchmaking_queues)
                .FirstOrDefaultAsync(u => u.username == username);
            
            if (user == null)
            {
                return null; // User n
            }
            // ตรวจสอบว่าผู้ใช้มีการเข้าคิวอยู่หรือไม่
            var game  = await _context.games
                .Include(g => g.white_player)
                .Include(g => g.black_player)
                .FirstOrDefaultAsync(g => (g.white_player.username == username ||
                g.black_player.username == username) && 
                g.game_status == "waiting");
            if (game != null) return null;
            // ตรวจสอบว่ามีคู่รออยู่ไหม
            var opponentId = game.white_player_id == user.user_id 
                ? game.black_player_id 
                : game.white_player_id;
            var opponent = await _context.users.FindAsync(opponentId);
            return new MatchFoundResponse
            {
                OpponentUsername = opponent?.username,
                GameId = game.game_id,
                RoomCode = game.room?.room_code ?? "",
                GameType = game?.game_type ?? "unknown",
                TimeControlMinutes = game?.time_control_minutes ?? 0,
                IsRated = game?.is_rated ?? false
            };
        }
    }
}
