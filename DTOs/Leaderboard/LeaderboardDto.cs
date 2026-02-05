namespace ChessApi.DTOs.Leaderboard
{
    public class LeaderboardDto
    {
        public int Rank { get; set; }
        public string Username { get; set; }
        public int Rating { get; set; }
        public int W { get; set; }
        public int L { get; set; }
        public int D { get; set; }
    }
}

