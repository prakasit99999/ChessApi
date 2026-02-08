using System;

namespace ChessApi.DTOs.Invites
{
    public class InviteRequestDto
    {
        public int FromUserId { get; set; }
        public int ToUserId { get; set; }
        public string GameType { get; set; } = "online_multiplayer";
        public int? MatchMode { get; set; } = 0; // 0 = null, 1 = ranked, 2 = normal
        public int? ExpiresInSeconds { get; set; } = 300;
    }

    public class InviteResponseDto
    {
        public string InviteId { get; set; }
        public int FromUserId { get; set; }
        public int ToUserId { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ExpiresAt { get; set; }
        public int? GameId { get; set; }
    }

    public class InviteActionDto
    {
        public int UserId { get; set; }
    }
}
