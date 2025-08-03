using ChessApi.DTOs.Room;

namespace ChessApi.Services.Interfaces
{
    public interface IRoomService
    {
        Task<RoomDto> CreateRoomAsync(CreateRoomRequest request);
        Task<RoomDto> GetRoomAsync(int roomId);
        Task<IEnumerable<RoomDto>> GetAllRoomsAsync();
        Task<RoomDto> UpdateRoomAsync(int roomId, string roomName);
        Task DeleteRoomAsync(int roomId);
        Task JoinRoomAsync(int roomId, int userId);
        Task LeaveRoomAsync(int roomId, int userId);
    }
}
