using System;
using System.Collections.Generic;

namespace ChessApi.DTOs.Rating
{
    public class LeaderboardDto
    {
        public int Rank { get; set; }
        public string Username { get; set; }
        public int Rating { get; set; }
        public int Wins { get; set; }
        public int Losses { get; set; }
        public int Draws { get; set; }
    }
}
