using ChessApi.DbContext;
using ChessApi.DTOs.Leaderboard;
using ChessApi.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace ChessApi.Services.Leaderboard
{
    public class LeaderboardService : ILeaderboardService
    {
        private readonly ChessDbContext _context;

        public LeaderboardService(ChessDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<LeaderboardDto>> GetLeaderboardAsync()
        {
            var users = await _context.users
                .OrderByDescending(u => u.rating ?? 1200)
                .ThenBy(u => u.username)
                .Take(20)
                .Select(u => new LeaderboardDto
                {
                    Username = u.username,
                    Rating = u.rating ?? 1200,
                    W = u.games_won ?? 0,
                    L = u.games_lost ?? 0,
                    D = u.games_drawn ?? 0
                })
                .ToListAsync();

            return users.Select((dto, index) =>
            {
                dto.Rank = index + 1;
                return dto;
            });
        }


        public async Task<DashboardOverviewDto> GetDashboardOverviewAsync()
        {
            //
            // 1. จำนวน Users และ Games ทั้งหมด
            int totalUsers = await _context.users.CountAsync();
            int totalGames = await _context.games.CountAsync();

            // 2. คำนวณเวลาประมวลผลเฉลี่ย Alpha-Beta 100 ตาล่าสุด
            var abMoves = await _context.moves
                .Where(m => m.algorithm_type == "alpha_beta" && m.move_time_ms.HasValue)
                .OrderByDescending(m => m.created_at)
                .Take(100)
                .Select(m => m.move_time_ms!.Value)
                .ToListAsync();

            int avgMoveTime = abMoves.Any() ? (int)abMoves.Average() : 0;

            // 3. ดึงผู้เล่นล่าสุด 5 คน (ใช้ new RecentUserDto { ... } แบบถูกไวยากรณ์)
            var recentUsers = await _context.users
                .OrderByDescending(u => u.created_at)
                .Take(5)
                .Select(u => new RecentUserDto
                {
                    Username = u.username,
                    Email = u.email ?? "",
                    Role = "Player",
                    GamesPlayed = u.games_played ?? 0,
                    Status = u.status ?? "offline"
                })
                .ToListAsync();

            return new DashboardOverviewDto
            {
                TotalUsers = totalUsers,
                TotalGames = totalGames,
                AvgMoveTimeMs = avgMoveTime,
                PruningEfficiency = 92.4,
                SystemIssues = 0,
                RecentUsers = recentUsers
            };
        }
    }
}