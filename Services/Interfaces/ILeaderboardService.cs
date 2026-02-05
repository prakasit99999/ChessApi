using ChessApi.DTOs.Leaderboard;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ChessApi.Services.Interfaces
{
    public interface ILeaderboardService
    {
        Task<IEnumerable<LeaderboardDto>> GetLeaderboardAsync();
    }
}
