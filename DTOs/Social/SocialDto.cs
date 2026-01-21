namespace ChessApi.DTOs.Social
{
    public class FriendRequestDto
    {
        public int SenderId { get; set; }
        public string TargetUsername { get; set; } = string.Empty;
    }

    public class AcceptFriendRequestDto
    {
        public int UserId { get; set; } // My ID
        public int RequesterId { get; set; } // The person who sent the request
    }

    public class FriendDto
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Status { get; set; } = "offline";
        public int Rating { get; set; }
    }
}
