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
            // Validate the request
            var errors = LoginVaildation.Validate(request);
            if (errors.Any())
            {
                return new AuthResponse
                {
                    Success = false,
                    Message = string.Join(", ", errors)
                };
            }
           // ค้นหาผู้ใช้จากฐานข้อมูลจริง
            var user = await _context.users.FirstOrDefaultAsync(u => u.email == request.Email);
            if (user == null)
            {
                return new  AuthResponse
                {
                    Success = false,
                    Message = "Email not found."
                };
            }

            // TODO: ตรวจสอบรหัสผ่าน
            var result = _passwordHasher.VerifyHashedPassword(user, user.password_hash, request.PasswordHash);
            if (result != PasswordVerificationResult.Success)
            {
                return new AuthResponse
                {
                    Success = false,
                    Message = "Invalid password."
                };
            }
            // Update last login time
            user.last_login = DateTime.UtcNow;
            user.status = "Online";
            _context.users.Update(user);
            await _context.SaveChangesAsync();

            var token = _jwtService.GenerateToken(user.user_id, user.username);
            return new AuthResponse
            {
                UserId = user.user_id,
                Username = user.username,
                Email = user.email,
                Status = user.status,
                Token = token,
                Success = true,
                Message = "Login successful"
            };

        }

        public async Task<RegisterResponse> RegisterAsync(RegisterRequest request)
        {
            // Validate the request
            var errors = RegisteVaildation.Validate(request); 
            if (errors.Any())
            {
                return new RegisterResponse
                {
                    Success = false,
                    Message = string.Join(", ", errors)
                };
            }
            var existingEmail = await _context.users.FirstOrDefaultAsync(u => u.email == request.Email);
            if (existingEmail != null)
            {
                return new RegisterResponse
                {
                    Success = false,
                    Message = "Email already exists."
                };
            }
            var existingUsername = await _context.users.FirstOrDefaultAsync(u => u.username == request.Username);
            if (existingUsername != null)
            {
                return new RegisterResponse
                {
                    Success = false,
                    Message = "Username already exists."
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