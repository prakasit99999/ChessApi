using ChessApi.DbContext;
using ChessApi.DTOs.Room;
using ChessApi.Models;
using ChessApi.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
namespace ChessApi.Services.Room
{
    public class RoomService:IRoomService
    {
        private readonly ChessDbContext _context;
        public RoomService(ChessDbContext context)
        {
            _context = context;
        }

        private string GenerateRoomCode()
        {
            return Guid.NewGuid().ToString().Substring(0, 6).ToUpper();
        }

        public async Task<RoomDto> CreateRoomAsync(CreateRoomRequest request)
        {
            var room = new game_room
            {
                room_name = request.RoomName,
                max_participants = request.MaxParticipants,
                room_code = GenerateRoomCode(),
                room_status = "waiting",
                time_control_minutes = request.TimeControlMinutes,
                is_rated = request.IsRated,
                created_at = DateTime.UtcNow
            };

            _context.game_rooms.Add(room);
            await _context.SaveChangesAsync();

            return new RoomDto
            {
                RoomId = room.room_id,
                RoomCode = room.room_code,
                RoomName = room.room_name,
                MaxParticipants = (int)room.max_participants,
                CurrentParticipants = 1, // เริ่มต้น 1 คนคือ Host
                RoomStatus = room.room_status,
                IsRated = (bool)room.is_rated,
                TimeControlMinutes = (int)room.time_control_minutes,
                HostUsername = request.HostUsername
            };
        }
        
        public async Task<RoomDto> GetRoomAsync(int roomId)
        {
            var room = await _context.game_rooms.FindAsync(roomId);
            if (room == null) return null;
            return new RoomDto
            {
                RoomId = room.room_id,
                RoomCode = room.room_code,
                RoomName = room.room_name,
                MaxParticipants = (int)room.max_participants,
                CurrentParticipants = 1, // ควรจะมีการนับจำนวนผู้เข้าร่วมจริง
                RoomStatus = room.room_status,
                IsRated = (bool)room.is_rated,
                TimeControlMinutes = (int)room.time_control_minutes,
                HostUsername = "HostUsername" // ควรจะดึงจากข้อมูลผู้ใช้ที่เป็น Host
            };
        }
        public async Task<IEnumerable<RoomDto>> GetAllRoomsAsync()
        {
            var rooms = await _context.game_rooms.ToListAsync();
            return rooms.Select(room => new RoomDto
            {
                RoomId = room.room_id,
                RoomCode = room.room_code,
                RoomName = room.room_name,
                MaxParticipants = (int)room.max_participants,
                CurrentParticipants = 1, // ควรจะมีการนับจำนวนผู้เข้าร่วมจริง
                RoomStatus = room.room_status,
                IsRated = (bool)room.is_rated,
                TimeControlMinutes = (int)room.time_control_minutes,
                HostUsername = "HostUsername" // ควรจะดึงจากข้อมูลผู้ใช้ที่เป็น Host
            });
        }
        public async Task<RoomDto> UpdateRoomAsync(int roomId, string roomName)
        {
            var room = await _context.game_rooms.FindAsync(roomId);
            if (room == null) return null;
            room.room_name = roomName;
            await _context.SaveChangesAsync();
            return new RoomDto
            {
                RoomId = room.room_id,
                RoomCode = room.room_code,
                RoomName = room.room_name,
                MaxParticipants = (int)room.max_participants,
                CurrentParticipants = 1, // ควร���ะมีการนับจำนวนผู้เข้าร่วมจริง
                RoomStatus = room.room_status,
                IsRated = (bool)room.is_rated,
                TimeControlMinutes = (int)room.time_control_minutes,
                HostUsername = "HostUsername" // ควรจะดึงจากข้อมูลผู้ใช้ที่เป็น Host
            };
        }
        public async Task DeleteRoomAsync(int roomId)
        {
            var room = await _context.game_rooms.FindAsync(roomId);
            if (room != null)
            {
                _context.game_rooms.Remove(room);
                await _context.SaveChangesAsync();
            }
        }
        public async Task JoinRoomAsync(int roomId, int userId)
        {
            var room = await _context.game_rooms.FindAsync(roomId);
            if (room != null && room.current_participants < room.max_participants)
            {
                room.current_participants++;
                // เพิ่ม logic สำหรับการเพิ่มผู้ใช้เข้าร่วมห้อง
                await _context.SaveChangesAsync();
            }
        }
        public async Task LeaveRoomAsync(int roomId, int userId)
        {
            var room = await _context.game_rooms.FindAsync(roomId);
            if (room != null && room.current_participants > 0)
            {
                room.current_participants--;
                // เพิ่ม logic สำหรับการลบผู้ใช้ที่ออกจากห้อง
                await _context.SaveChangesAsync();
            }
        }
     }
}
