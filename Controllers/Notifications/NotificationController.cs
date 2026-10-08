using System.Threading.Tasks;
using ChessApi.DTOs.Notifications;
using ChessApi.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChessApi.Controllers.Notifications
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationService _notificationService;

        public NotificationController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        /// <summary>
        /// ดึงรายการแจ้งเตือนล่าสุดทั้งหมด (GET /api/notification)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetRecentNotifications([FromQuery] int limit = 50)
        {
            var notifications = await _notificationService.GetRecentNotificationsAsync(limit);
            var unreadCount = await _notificationService.GetUnreadCountAsync();

            return Ok(new
            {
                success = true,
                unreadCount,
                total = notifications.Count,
                data = notifications
            });
        }

        /// <summary>
        /// ดึงจำนวนการแจ้งเตือนที่ยังไม่ได้อ่าน (GET /api/notification/unread-count)
        /// </summary>
        [HttpGet("unread-count")]
        public async Task<IActionResult> GetUnreadCount()
        {
            var count = await _notificationService.GetUnreadCountAsync();
            return Ok(new { success = true, unreadCount = count });
        }

        /// <summary>
        /// ทำเครื่องหมายรายการแจ้งเตือนว่าอ่านแล้ว (PUT /api/notification/mark-read/{id})
        /// </summary>
        [HttpPut("mark-read/{id}")]
        public async Task<IActionResult> MarkAsRead([FromRoute] string id)
        {
            var result = await _notificationService.MarkAsReadAsync(id);
            if (!result)
                return NotFound(new { success = false, message = "Notification not found" });

            return Ok(new { success = true, message = "Marked as read" });
        }

        /// <summary>
        /// ทำเครื่องหมายรายการแจ้งเตือนทั้งหมดว่าอ่านแล้ว (PUT /api/notification/mark-all-read)
        /// </summary>
        [HttpPut("mark-all-read")]
        public async Task<IActionResult> MarkAllAsRead()
        {
            await _notificationService.MarkAllAsReadAsync();
            return Ok(new { success = true, message = "All notifications marked as read" });
        }

        /// <summary>
        /// ล้างรายการแจ้งเตือนทั้งหมด (DELETE /api/notification/clear)
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpDelete("clear")]
        public async Task<IActionResult> ClearNotifications()
        {
            await _notificationService.ClearAllAsync();
            return Ok(new { success = true, message = "All notifications cleared" });
        }

        /// <summary>
        /// Admin ส่ง Notification ทดสอบหรือ Broadcast แบบกำหนดเอง (POST /api/notification/broadcast)
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpPost("broadcast")]
        public async Task<IActionResult> BroadcastCustom([FromBody] CreateCustomNotificationRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Title) || string.IsNullOrWhiteSpace(request.Message))
            {
                return BadRequest(new { success = false, message = "Title and Message are required" });
            }

            var notification = new NotificationDto
            {
                Title = request.Title,
                Message = request.Message,
                Type = string.IsNullOrWhiteSpace(request.Type) ? "system" : request.Type,
                Severity = string.IsNullOrWhiteSpace(request.Severity) ? "info" : request.Severity,
                Metadata = request.Metadata
            };

            var sent = await _notificationService.SendNotificationAsync(notification);
            return Ok(new { success = true, message = "Notification broadcasted successfully", data = sent });
        }
    }
}
