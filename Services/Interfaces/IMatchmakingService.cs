using ChessApi.DTOs.Matchmaking;

namespace ChessApi.Services.Interfaces
{
    public interface IMatchmakingService
    {
        Task JoinQueueAsync(JoinQueueDTOs request);
        Task CancelQueueAsync(CancelQueueDTOs request);
        Task<MatchFoundDTOs?> CheckForMatchAsync(string username);
    }
}
