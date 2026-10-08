using System;

namespace ChessApi.DTOs.Notifications
{
    public class NotificationDto
    {
        public string Id { get; set; } = Guid.NewGuid().ToString("N");

        /// ประเภทการแจ้งเตือน เช่น game_started, game_ended, user_registered, system
        public string Type { get; set; } = "general";

        /// หัวข้อการแจ้งเตือน
        public string Title { get; set; } = string.Empty;

        /// รายละเอียดข้อความ
        public string Message { get; set; } = string.Empty;

        /// ระดับความสำคัญ: "info", "success", "warning", "error"
        public string Severity { get; set; } = "info";

        /// เวลาที่สร้าง (UTC)
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// สถานะอ่านแล้วหรือไม่
        public bool IsRead { get; set; } = false;

        /// ข้อมูลเสริม (เช่น GameId, PlayerNames, Scores)
        public object? Metadata { get; set; }
    }

    public class CreateCustomNotificationRequest
    {
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Type { get; set; } = "system";
        public string Severity { get; set; } = "info";
        public object? Metadata { get; set; }
    }

    public class NotificationSummaryDto
    {
        public int TotalCount { get; set; }
        public int UnreadCount { get; set; }
        public List<NotificationDto> RecentNotifications { get; set; } = new();
    }
}
