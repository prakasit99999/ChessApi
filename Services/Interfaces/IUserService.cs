using ChessApi.DTOs.User;
namespace ChessApi.Services.Interfaces
{
    public interface IUserService
    {
        Task<ProfileResponse> GetProfileAsync(ProfileRequest request);
        Task<bool> UpdateUserStatusAsync(int userId, string status);
        Task<List<UserListDto>> GetAllPlayersAsync();
        Task<List<UserListDto>> SearchPlayersAsync(string query);
    }
}
