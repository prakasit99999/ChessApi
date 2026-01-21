using ChessApi.DbContext;
using ChessApi.DTOs.Social;
using ChessApi.Models;
using ChessApi.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ChessApi.Services.Social
{
    public class SocialService : ISocialService
    {
        private readonly ChessDbContext _context;

        public SocialService(ChessDbContext context)
        {
            _context = context;
        }

        public async Task<bool> SendFriendRequestAsync(int senderId, string targetUsername)
        {
            // 1. Find target user by name
            var targetUser = await _context.users.FirstOrDefaultAsync(u => u.username == targetUsername);
            if (targetUser == null) return false; // User not found
            if (targetUser.user_id == senderId) return false; // Cannot add yourself

            // 2. Check if friendship already exists (pending or accepted)
            var existing = await _context.friendships.AnyAsync(f =>
                (f.user1_id == senderId && f.user2_id == targetUser.user_id) ||
                (f.user1_id == targetUser.user_id && f.user2_id == senderId));

            if (existing) return false;

            // 3. Insert into friendships table
            var friendship = new friendship
            {
                user1_id = senderId,
                user2_id = targetUser.user_id,
                status = "pending",
                created_at = DateTime.UtcNow
            };

            _context.friendships.Add(friendship);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> AcceptFriendRequestAsync(int userId, int requesterId)
        {
            // Find the pending request where user1 is the requester and user2 is me
            var friendship = await _context.friendships.FirstOrDefaultAsync(f =>
                f.user1_id == requesterId &&
                f.user2_id == userId &&
                f.status == "pending");

            if (friendship == null) return false;

            friendship.status = "accepted";
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<FriendDto>> GetFriendListAsync(int userId)
        {
            var friends = await _context.friendships
                .Where(f => (f.user1_id == userId || f.user2_id == userId) && f.status == "accepted")
                .Include(f => f.user1)
                .Include(f => f.user2)
                .Select(f => f.user1_id == userId ? f.user2 : f.user1) // Select the *other* user
                .Select(u => new FriendDto
                {
                    Id = u.user_id,
                    Username = u.username,
                    Status = u.status ?? "offline",
                    Rating = u.rating ?? 1200
                })
                .ToListAsync();

            return friends;
        }
    }
}
