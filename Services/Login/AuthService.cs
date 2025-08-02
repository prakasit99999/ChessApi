using ChessApi.DTOs.Auth;
using ChessApi.Models;
using ChessApi.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ChessApi.DbContext;

namespace ChessApi.Services.Login
{
    public class AuthService : IAuthService
    {
        private readonly IPasswordHasher<user> _passwordHasher;
        private readonly JwtService _jwtService;
        private readonly ChessDbContext _context;


        public AuthService(IPasswordHasher<user> passwordHasher, JwtService jwtService, ChessDbContext context)
        {
            _passwordHasher = passwordHasher;
            _jwtService = jwtService;
            _context = context;
        }

        public async Task<AuthResponse> LoginAsync(LoginRequest request)
        {
            // TODO: ดึง user จริงจากฐานข้อมูล
            var user = new user
            {
                username = request.Username,
                email = request.PasswordHash
            };

            var result = _passwordHasher.VerifyHashedPassword(user, user.username, request.PasswordHash);

            if (result == PasswordVerificationResult.Success)
            {
                var token = _jwtService.GenerateToken(user.user_id, user.username);

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

        public async Task<RegisterResponse> RegisterAsync(RegisterRequest request)
        {
            var existingUser = await _context.users.FirstOrDefaultAsync(u => u.username == request.Username || u.email == request.Email);

            if (existingUser != null)
            {
                return new RegisterResponse
                {
                    Success = false,
                    Message = "Username or email already exists."
                };
            }

            var user = new user
            {
                username = request.Username,
                email = request.Email,

            };

            user.password_hash = _passwordHasher.HashPassword(user, request.Password);

            // TODO: บันทึก user ลงฐานข้อมูลจริง
            _context.users.Add(user);
            await _context.SaveChangesAsync();

            var token = _jwtService.GenerateToken(user.user_id, user.username);

            return new RegisterResponse 
            {
                Success = true,
                Message = "Registration successful."
            };
        }
    }
}