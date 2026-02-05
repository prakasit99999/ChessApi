using ChessApi.DTOs.Auth;
using ChessApi.Models;
using ChessApi.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ChessApi.Validations.Auth;
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
            // 1. Validation
            var errors = LoginVaildation.Validate(request);
            if (errors.Any())
            {
                return new AuthResponse { Success = false, Message = string.Join(", ", errors) };
            }

            var user = await _context.users.FirstOrDefaultAsync(u => u.email == request.Email);
            if (user == null)
            {
                return new AuthResponse { Success = false, Message = "Email not found." };
            }

            var result = _passwordHasher.VerifyHashedPassword(user, user.password_hash, request.Password);

            if (result == PasswordVerificationResult.Failed)
            {
                return new AuthResponse { Success = false, Message = "Invalid password." }; // รหัสผิดต้องเด้งออก
            }

            user.status = "Online";
            _context.users.Update(user);
            await _context.SaveChangesAsync();

            // 5. Generate Token
            var token = _jwtService.GenerateToken(user.user_id, user.username);

            return new AuthResponse
            {
                Success = true,
                Message = "Login successful.",
                UserId = user.user_id,
                Username = user.username,
                Email = user.email,
                Token = token,
                Status = user.status.ToString()
            };
        }

        public async Task<AuthResponse> LogOutAsync(string userIdStr)
        {
            if (int.TryParse(userIdStr, out int userId))
            {
                var user = await _context.users.FindAsync(userId);
                if (user != null)
                {
                    user.status = "Offline";
                    _context.users.Update(user);
                    await _context.SaveChangesAsync();
                }
            }
            return new AuthResponse { Success = true, Message = "Logout successful." };
        }


        public async Task<RegisterResponse> RegisterAsync(RegisterRequest request)
        {
            // 1. Validation
            var errors = RegisteVaildation.Validate(request);
            if (errors.Any())
            {
                return new RegisterResponse { Success = false, Message = string.Join(", ", errors) };
            }


            if (await _context.users.AnyAsync(u => u.email == request.Email))
            {
                return new RegisterResponse { Success = false, Message = "Email already exists." };
            }

            if (await _context.users.AnyAsync(u => u.username == request.Username))
            {
                return new RegisterResponse { Success = false, Message = "Username already exists." };
            }

            var user = new user
            {
                username = request.Username,
                email = request.Email,
                rating = 1200,
                status = "offline",
                created_at = DateTime.UtcNow
            };
            user.password_hash = _passwordHasher.HashPassword(user, request.Password);
            try
            {
                _context.users.Add(user);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return new RegisterResponse { Success = false, Message = ex.Message };
            }

            return new RegisterResponse { Success = true, Message = "Registration successful." };
        }
    }
}