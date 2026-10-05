using ChessApi.DTOs.Auth;
namespace ChessApi.Services.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponse> LoginAsync(LoginRequest request);
        Task<AdminAuthResponse> AdminLoginAsync(AdminLoginRequest request);
        Task<AuthResponse> LogOutAsync(string userId);
        Task<RegisterResponse> RegisterAsync(RegisterRequest request);

    }
}
