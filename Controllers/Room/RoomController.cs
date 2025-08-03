using ChessApi.DTOs.Room;
using ChessApi.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ChessApi.Controllers.Room
{
    [Route("api/[controller]")]
    [ApiController]
    public class RoomController : ControllerBase
    {
        private readonly IRoomService _roomService;
        public RoomController(IRoomService roomService)
        {
            _roomService = roomService;
        }
        [HttpPost("create")]
        public async Task<IActionResult> CreateRoom([FromBody] CreateRoomRequest request)
        {
            var result = await _roomService.CreateRoomAsync(request);
            return Ok(result);
        }
        // เพิ่ม GetRoom, GetAll, Update, Delete, Join, Leave
        [HttpGet("{roomId}")]
        public async Task<IActionResult> GetRoom(int roomId)
        {
            var room = await _roomService.GetRoomAsync(roomId);
            if (room == null) return NotFound();
            return Ok(room);
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAllRooms()
        {
            var rooms = await _roomService.GetAllRoomsAsync();
            return Ok(rooms);
        }
        
        [HttpPut("{roomId}")]
        public async Task<IActionResult> UpdateRoom(int roomId, [FromBody] string roomName)
        {
            var updatedRoom = await _roomService.UpdateRoomAsync(roomId, roomName);
            if (updatedRoom == null) return NotFound();
            return Ok(updatedRoom);
        }

        [HttpDelete("{roomId}")]
        public async Task<IActionResult> DeleteRoom(int roomId)
        {
            await _roomService.DeleteRoomAsync(roomId);
            return NoContent();
        }

        [HttpPost("{roomId}/join")]
        public async Task<IActionResult> JoinRoom(int roomId, [FromQuery] int userId)
        {
            await _roomService.JoinRoomAsync(roomId, userId);
            return Ok();
        }

        [HttpPost("{roomId}/leave")]
        public async Task<IActionResult> LeaveRoom(int roomId, [FromQuery] int userId)
        {
            await _roomService.LeaveRoomAsync(roomId, userId);
            return Ok();
        }
    }
}
