using System.Collections.Generic;
using System.Threading.Tasks;
using ChessApi.DTOs.Invites;
using ChessApi.Services.Interfaces;

namespace ChessApi.Services.Interfaces
{
    public interface IInviteService
    {
        Task<InviteResponseDto> CreateAsync(InviteRequestDto dto);
        Task<InviteResponseDto?> AcceptAsync(string inviteId, int userId);
        Task<InviteResponseDto?> DeclineAsync(string inviteId, int userId);
        Task<InviteResponseDto?> CancelAsync(string inviteId, int userId);
        Task<IReadOnlyList<InviteResponseDto>> GetInboxAsync(int userId);
        Task<IReadOnlyList<InviteResponseDto>> GetSentAsync(int userId);
    }
}
