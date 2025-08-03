namespace ChessApi.DTOs.Room
{
    public class CreateRoomRequest
    {
        public string RoomName { get; set; }
        public string HostUsername { get; set; }
        public int MaxParticipants { get; set; }
        public bool IsRated { get; set; }
        public int TimeControlMinutes { get; set; }
    }
}
