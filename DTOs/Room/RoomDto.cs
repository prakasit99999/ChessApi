using ChessApi.Models;

namespace ChessApi.DTOs.Room
{
    public class RoomDto
    {
        public int RoomId { get; set; }
        public string RoomCode { get; set; }
        public string RoomName { get; set; }
        public string HostUsername { get; set; }
        public int CurrentParticipants { get; set; }
        public int MaxParticipants { get; set; }
        public string RoomStatus { get; set; }
        public bool IsRated { get; set; }
        public int TimeControlMinutes { get; set; }
    }
}
