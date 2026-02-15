using Microsoft.AspNetCore.Mvc;

namespace ChessApi.DTOs.Matchmaking
{
    public class JoinQueueDTOs
    {
        public string Username { get; set; } = string.Empty; // ต้องระบุเพื่อหา user_id
        public int MinRating { get; set; } = 0;
        public int MaxRating { get; set; } = 3000;
        public int MatchMode { get; set; } = 0; // 0 =null 2= normal, 1 = ranked
    }
}
