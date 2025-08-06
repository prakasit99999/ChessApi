using Microsoft.AspNetCore.Mvc;

namespace ChessApi.DTOs.Matchmaking
{
    public class JoinQueueRequest 
    {
        public string Username { get; set; } = string.Empty; // ต้องระบุเพื่อหา user_id
        public int PreferredTimeControl { get; set; } = 10; // นาที
        public int MinRating { get; set; } = 0;
        public int MaxRating { get; set; } = 3000;
    }
}
