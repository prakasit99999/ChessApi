using ChessApi.DbContext;
using ChessApi.DTOs.Leaderboard;
using ChessApi.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
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

            return users.Select((dto, index) => {
                dto.Rank = index + 1;
                return dto;
            });
        }
    }
}