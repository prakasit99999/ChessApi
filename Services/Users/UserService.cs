using ChessApi.DbContext;
using ChessApi.DTOs.User;
using ChessApi.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ChessApi.Services.Users
{
    public class UserService : IUserService
    {
        private readonly ChessDbContext _context;
        public UserService(ChessDbContext context)
        {
            _context = context;
        }
        public async Task<ProfileResponse> GetProfileAsync(ProfileRequest request)
        {
            var user = await _context.users.FirstOrDefaultAsync(u => u.user_id == request.UserId);
            if (user == null)
            {
                return new ProfileResponse
                {
                    Success = false,
                    Message = "User not found"
                };
            }
            return new ProfileResponse
            {
                Username = user.username,
                Email = user.email,
                Rating = (int)user.rating,
                GamesPlayed = (int)user.games_played,
                GamesWon = (int)user.games_won,
                GamesLost = (int)user.games_lost,
                GamesDrawn = (int)user.games_drawn,
                Status = user.status ?? "offline",
                Success = true,
                Message = "Profile retrieved successfully"
            };
        }

    }
}
