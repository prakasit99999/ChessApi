using ChessApi.DTOs.Social;

namespace ChessApi.Services.Interfaces
{
    public interface ISocialService
    {
        Task<bool> SendFriendRequestAsync(int senderId, string targetUsername);
        Task<bool> AcceptFriendRequestAsync(int userId, int requesterId);
        Task<List<FriendDto>> GetFriendListAsync(int userId);
    }
}