using ChessApi.DTOs.Matchmaking;

namespace ChessApi.Services.Interfaces
{
    public interface IMatchmakingService
    {
        Task JoinQueueAsync(JoinQueueRequest request);
        Task CancelQueueAsync(CancelQueueRequest request);
        Task<MatchFoundResponse?> CheckForMatchAsync(string username);
    }
}
