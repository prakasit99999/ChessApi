using ChessApi.DTOs.Matchmaking;
using System.Threading.Tasks;

namespace ChessApi.Services.Interfaces
{
    public interface IMatchmakingService
    {
        // เปลี่ยนจาก Task เฉยๆ เป็น Task<MatchFoundDTOs?> เพื่อรับผลการจับคู่ทันที (ถ้ามี)
        Task<MatchFoundDTOs?> JoinQueueAsync(JoinQueueDTOs request);
        Task CancelQueueAsync(CancelQueueDTOs request);
        Task<MatchFoundDTOs?> CheckForMatchAsync(string username);
    }
}