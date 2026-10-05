using ChessApi.DTOs.User;
namespace ChessApi.Services.Interfaces
{
    public interface IUserService
    {
        Task<ProfileResponse> GetProfileAsync(ProfileRequest request);
        Task<ProfileResponse> GetProfileByIdAsync(int userId);
        Task<UpdateProfileResponse> UpdateProfileAsync(int userId, UpdateProfileRequest request);
        Task<UpdateProfileResponse> AdminUpdateUserAsync(int userId, AdminUpdateUserRequest request);
        Task<bool> UpdateUserStatusAsync(int userId, string status);
        Task<List<UserListDto>> GetAllPlayersAsync();
        Task<List<UserListDto>> SearchPlayersAsync(string query);
        Task<PagedResult<UserListDto>> GetPagedUsersAsync(UserPagedRequest request);
        Task<UserCountsDto> GetUserCountsAsync();
        Task BroadcastUserCountsAsync();
    }
}
