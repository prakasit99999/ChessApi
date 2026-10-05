using Microsoft.AspNetCore.SignalR;
using ChessApi.DTOs.User;
using ChessApi.Services.Interfaces;
using ChessApi.Utilities.Helpers;
using System.Collections.Concurrent;

namespace ChessApi.Hubs
{
    public class UserHub : Hub
    {
        private readonly IUserService _userService;
        // เก็บ mapping connectionId -> userId
        private static readonly ConcurrentDictionary<string, int> _connectedUsers = new();

        public UserHub(IUserService userService)
        {
            _userService = userService;
        }

        public override async Task OnConnectedAsync()
        {
            var userId = Context.User != null ? UserClaimHelper.GetUserIdFromToken(Context.User) : null;
            if (userId.HasValue && userId.Value > 0)
            {
                _connectedUsers[Context.ConnectionId] = userId.Value;
                // อัปเดตสถานะเป็น online และกระจายข้อมูลไปยังทุกคน
                await _userService.UpdateUserStatusAsync(userId.Value, "online");
            }
            else
            {
                // ส่งตัวเลขล่าสุดให้เฉพาะผู้ที่เพิ่งต่อเข้ามา
                var counts = await _userService.GetUserCountsAsync();
                await Clients.Caller.SendAsync("ReceiveUserCounts", counts);
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            if (_connectedUsers.TryRemove(Context.ConnectionId, out var userId))
            {
                // ตรวจสอบว่ายังมี connection อื่นของ user นี้เปิดค้างอยู่หรือไม่
                var hasOtherConnections = _connectedUsers.Values.Any(id => id == userId);
                if (!hasOtherConnections)
                {
                    await _userService.UpdateUserStatusAsync(userId, "offline");
                }
            }

            await base.OnDisconnectedAsync(exception);
        }

        /// <summary>
        /// ขอตัวเลข Users ปัจจุบัน (ส่งกลับไปยัง Client ที่เรียก)
        /// </summary>
        public async Task<UserCountsDto> GetUserCounts()
        {
            var counts = await _userService.GetUserCountsAsync();
            await Clients.Caller.SendAsync("ReceiveUserCounts", counts);
            return counts;
        }

        /// <summary>
        /// กระจายตัวเลข Users ล่าสุดไปยังทุก Client ที่เชื่อมต่ออยู่
        /// </summary>
        public async Task BroadcastUserCounts()
        {
            await _userService.BroadcastUserCountsAsync();
        }

        /// <summary>
        /// อัปเดตสถานะของตนเองผ่าน SignalR (online, offline, pending, playing)
        /// </summary>
        public async Task UpdateStatus(string status)
        {
            var userId = Context.User != null ? UserClaimHelper.GetUserIdFromToken(Context.User) : null;
            if (userId.HasValue && userId.Value > 0)
            {
                await _userService.UpdateUserStatusAsync(userId.Value, status);
            }
        }
    }
}
