using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace ChessApi.Hubs
{
    public class NotificationHub : Hub
    {
        public const string AdminGroupName = "AdminGroup";

        public override async Task OnConnectedAsync()
        {
            // เพิ่ม connection นี้เข้ากลุ่ม AdminGroup โดยอัตโนมัติ เพื่อรับ notification ฝั่ง Admin
            await Groups.AddToGroupAsync(Context.ConnectionId, AdminGroupName);
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(System.Exception? exception)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, AdminGroupName);
            await base.OnDisconnectedAsync(exception);
        }

        /// <summary>
        /// ให้ Client สามารถ Join Admin Group แบบเจาะจง
        /// </summary>
        public async Task JoinAdminGroup()
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, AdminGroupName);
            await Clients.Caller.SendAsync("JoinedAdminGroup", "Joined successfully");
        }

        /// <summary>
        /// ให้ Client สามารถ Leave Admin Group
        /// </summary>
        public async Task LeaveAdminGroup()
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, AdminGroupName);
            await Clients.Caller.SendAsync("LeftAdminGroup", "Left successfully");
        }
    }
}
