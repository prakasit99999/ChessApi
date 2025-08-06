using ChessApi.DbContext;
using ChessApi.DTOs.Room;
using ChessApi.Models;
using ChessApi.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
namespace ChessApi.Services.Room
{
    public class RoomService : IRoomService
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
            var host = await _context.users.FirstOrDefaultAsync(u => u.username == request.HostUsername);
            if (host == null) throw new Exception("Host user not found");

            var room = new game_room
            {
                room_name = request.RoomName,
                max_participants = request.MaxParticipants,
                room_code = GenerateRoomCode(),
                room_status = "waiting",
                time_control_minutes = request.TimeControlMinutes,
                is_rated = request.IsRated,
                created_by_user_id = host.user_id, 
                created_at = DateTime.UtcNow
            };

            _context.game_rooms.Add(room);
            await _context.SaveChangesAsync();
            await _context.Entry(room)
            .Reference(r => r.created_by_user)
            .LoadAsync();

            return new RoomDto
            {
                RoomId = room.room_id,
                RoomCode = room.room_code,
                RoomName = room.room_name,
                MaxParticipants = (int)room.max_participants,
                CurrentParticipants = room.current_participants ?? 0, // เริ่มต้น 1 คนคือ Host
                RoomStatus = room.room_status,
                IsRated = (bool)room.is_rated,
                TimeControlMinutes = (int)room.time_control_minutes,
                HostUsername = room.created_by_user?.username ?? "Unknown"
            };
        }

        public async Task<RoomDto> GetRoomAsync(int roomId)
        {
            var rooms = await _context.game_rooms
              .Include(r => r.created_by_user)
              .Include(r => r.room_participants)
              .ToListAsync();
            return (RoomDto)rooms.Select(room => new RoomDto
            {
                RoomId = room.room_id,
                RoomCode = room.room_code,
                RoomName = room.room_name,
                MaxParticipants = (int)room.max_participants,
                CurrentParticipants = room.room_participants.Count(p => p.status == "active"), // ✅ ใช้จริง
                RoomStatus = room.room_status,
                IsRated = (bool)room.is_rated,
                TimeControlMinutes = (int)room.time_control_minutes,
                HostUsername = room.created_by_user?.username ?? "Unknown"
            });
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
                HostUsername = _context.users.Where(u => u.user_id == room.created_by_user_id)
                                             .Select(u => u.username)
                                             .FirstOrDefault() ?? "Unknown"
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
                CurrentParticipants = room.current_participants ?? 0, // ควร���ะมีการนับจำนวนผู้เข้าร่วมจริง
                RoomStatus = room.room_status,
                IsRated = (bool)room.is_rated,
                TimeControlMinutes = (int)room.time_control_minutes,
                HostUsername = room.created_by_user?.username ?? "Unknown"
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
            // Validate room
            var room = await _context.game_rooms.FindAsync(roomId)
                       ?? throw new Exception("Room not found");

            if (room.room_status != "waiting")
                throw new Exception("Room is not joinable.");

            if (room.current_participants >= room.max_participants)
                throw new Exception("Room is full.");

            // Validate user
            var alreadyJoined = await _context.room_participants
                .AnyAsync(p => p.room_id == roomId && p.user_id == userId && p.status == "active");

            if (alreadyJoined)
                throw new Exception("User already in room.");

            // Passed validation → Proceed
            _context.room_participants.Add(new room_participant
            {
                room_id = roomId,
                user_id = userId,
                role = "player",
                player_color = "random",
                is_ready = false,
                status = "active",
                joined_at = DateTime.UtcNow
            });

            room.current_participants++;
            await _context.SaveChangesAsync();
        }

        public async Task LeaveRoomAsync(int roomId, int userId)
        {
            var room = await _context.game_rooms.FindAsync(roomId);
            if (room == null) throw new Exception("Room not found");

            var participant = await _context.room_participants
                .FirstOrDefaultAsync(p => p.room_id == roomId && p.user_id == userId && p.status == "active");

            if (participant == null)
                throw new Exception("User is not in the room.");

            if (participant.role == "host")
                throw new Exception("Host cannot leave the room directly."); // หรือเปลี่ยน host ก็ได้

            participant.status = "left";
            participant.left_at = DateTime.UtcNow;

            room.current_participants = Math.Max((room.current_participants ?? 1) - 1, 0);

            await _context.SaveChangesAsync();

        }
    }
}
