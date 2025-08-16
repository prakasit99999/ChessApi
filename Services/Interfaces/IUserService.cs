using ChessApi.DTOs.User;
namespace ChessApi.Services.Interfaces
{
    public interface IUserService
    {
        Task<ProfileResponse> GetProfileAsync(ProfileRequest request);
    }
}
