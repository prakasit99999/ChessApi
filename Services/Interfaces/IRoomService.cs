using ChessApi.DTOs.Room;
using ChessApi.Services.Room;
namespace ChessApi.Services.Interfaces
{
    public interface IRoomService
    {
        Task<RoomService> CreateRoomAsync(CreateRoomRequest request);
        Task<RoomService> GetRoomAsync(string roomId);
        Task<IEnumerable<RoomService>> GetAllRoomsAsync();
        Task<RoomService> UpdateRoomAsync(string roomId, string RoomName );
        Task DeleteRoomAsync(string roomId);
        Task JoinRoomAsync(string roomId, string userId);
        Task LeaveRoomAsync(string roomId, string userId);
    }
}
