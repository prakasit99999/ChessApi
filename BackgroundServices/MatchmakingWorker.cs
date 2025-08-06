using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ChessApi.DbContext;
using Microsoft.EntityFrameworkCore;
using ChessApi.DTOs.Room;
using ChessApi.Models;
using ChessApi.Services.Interfaces;

namespace ChessApi.BackgroundServices
{
    public class MatchmakingWorker : BackgroundService
    {
        private readonly ILogger<MatchmakingWorker> _logger;
        private readonly IServiceProvider _services;

        public MatchmakingWorker(ILogger<MatchmakingWorker> logger, IServiceProvider services)
        {
            _logger = logger;
            _services = services;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Matchmaking Worker started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using (var scope = _services.CreateScope())
                    {
                        var db = scope.ServiceProvider.GetRequiredService<ChessDbContext>();
                        var roomService = scope.ServiceProvider.GetRequiredService<IRoomService>();

                        var queue = await db.matchmaking_queues
                            .Include(q => q.user)
                            .OrderBy(q => q.user.rating)
                            .ToListAsync(stoppingToken);

                        for (int i = 0; i < queue.Count - 1; i++)
                        {
                            var player1 = queue[i];
                            var player2 = queue[i + 1];

                            // ตรวจสอบว่าคะแนนห่างกันไม่เกิน 200
                            int rating1 = player1.user.rating ?? 1000;
                            int rating2 = player2.user.rating ?? 1000;

                            if (Math.Abs(rating1 - rating2) <= 200)
                            {
                                // สร้างห้อง
                                var roomDto = await roomService.CreateRoomAsync(new CreateRoomRequest
                                {
                                    HostUsername = player1.user.username,
                                    RoomName = $"{player1.user.username} vs {player2.user.username}",
                                    MaxParticipants = 2,
                                    TimeControlMinutes = player1.preferred_time_control ?? 5,
                                    IsRated = true
                                });

                                // สร้างเกม
                                var game = new game
                                {
                                    room_id = roomDto.RoomId,
                                    game_type = "ranked",
                                    white_player_id = player1.user_id,
                                    black_player_id = player2.user_id,
                                    white_player_type = "human",
                                    black_player_type = "human",
                                    game_status = "waiting",
                                    current_turn = "white",
                                    is_rated = true,
                                    time_control_minutes = player1.preferred_time_control,
                                    created_at = DateTime.UtcNow
                                };
                                db.games.Add(game);

                                // ลบออกจาก queue
                                db.matchmaking_queues.Remove(player1);
                                db.matchmaking_queues.Remove(player2);
                                await db.SaveChangesAsync(stoppingToken);

                                _logger.LogInformation($"Matched: {player1.user.username} vs {player2.user.username}");
                                break; // ป้องกัน concurrent modification
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred in matchmaking loop.");
                }

                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken); // รอ 5 วิ
            }

            _logger.LogInformation("Matchmaking Worker stopped.");
        }
    }
}
