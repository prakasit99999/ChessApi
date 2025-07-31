using ChessApi.DTOs.Auth;
using ChessApi.Models;
using ChessApi.Services.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace ChessApi.Services
{
    public class AuthService : IAuthService
    {
        private readonly IPasswordHasher<user> _passwordHasher;
        private readonly JwtService _jwtService;

        public AuthService(IPasswordHasher<user> passwordHasher, JwtService jwtService)
        {
            _passwordHasher = passwordHasher;
            _jwtService = jwtService;
        }

        public async Task<AuthResponse> LoginAsync(LoginRequest request)
        {
            // TODO: ดึง user จริงจากฐานข้อมูล
            var user = new user
            {
                user_id = 1,
                username = request.Username,
                password_hash = _passwordHasher.HashPassword(null!, "1234") // จำลอง hash จาก DB
            };

            var result = _passwordHasher.VerifyHashedPassword(user, user.username, request.PasswordHash);

            if (result == PasswordVerificationResult.Success)
            {
                var token = _jwtService.GenerateToken(user.user_id,user.username);

                return new AuthResponse
                {
                    UserId = user.user_id,
                    Username = user.username,
                    Email = user.email,
                    Token = token
                };
            }

            return null;
        }

        public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
        {
            var user = new user
            {
                username = request.Username,
                email = request.Email
            };

            user.password_hash = _passwordHasher.HashPassword(user, request.Password);

            // TODO: บันทึก user ลงฐานข้อมูลจริง
            user.user_id = 2;

            var token = _jwtService.GenerateToken(user.user_id,user.username);

            return new AuthResponse
            {
                UserId = user.user_id,
                Username = user.username,
                Email = user.email,
                Token = token
            };
        }
    }
}
