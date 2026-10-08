using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ChessApi.DTOs.Notifications;
using ChessApi.Hubs;
using ChessApi.Services.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace ChessApi.Services.Notifications
{
    public class NotificationService : INotificationService
    {
        private readonly IHubContext<NotificationHub> _hubContext;
        // เก็บประวัติการแจ้งเตือนล่าสุดใน Memory (จำกัดขนาดเพื่อไม่ให้เปลืองหน่วยความจำ)
        private readonly ConcurrentQueue<NotificationDto> _notifications = new();
        private const int MaxStoredNotifications = 200;

        public NotificationService(IHubContext<NotificationHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public async Task<NotificationDto> SendNotificationAsync(NotificationDto notification)
        {
            // บันทึกลงหน่วยความจำ
            _notifications.Enqueue(notification);

            // ควบคุมขนาดไม่ให้เกิน MaxStoredNotifications
            while (_notifications.Count > MaxStoredNotifications)
            {
                _notifications.TryDequeue(out _);
            }

            // ส่ง Broadcast ผ่าน SignalR ไปยัง Admin Dashboard
            try
            {
                // ส่งไปยัง Admin Group
                await _hubContext.Clients.Group(NotificationHub.AdminGroupName)
                    .SendAsync("ReceiveNotification", notification);

                // ส่งไปยัง All Clients ของ NotificationHub เผื่อ client ยังไม่ได้เข้า group
                await _hubContext.Clients.All
                    .SendAsync("ReceiveNotification", notification);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[NotificationService] SignalR broadcast error: {ex.Message}");
            }

            return notification;
        }

        public async Task<NotificationDto> NotifyGameStartedAsync(int gameId, string gameType, string? whitePlayer, string? blackPlayer)
        {
            var white = string.IsNullOrWhiteSpace(whitePlayer) ? "Unknown" : whitePlayer;
            var black = string.IsNullOrWhiteSpace(blackPlayer) ? "Unknown" : blackPlayer;

            var notification = new NotificationDto
            {
                Type = "game_started",
                Title = "เริ่มเล่นเกมใหม่!",
                Message = $"เกม #{gameId} ({gameType}) เริ่มต้นแล้วระหว่าง {white} (ขาว) VS {black} (ดำ)",
                Severity = "info",
                CreatedAt = DateTime.UtcNow,
                Metadata = new
                {
                    GameId = gameId,
                    GameType = gameType,
                    WhitePlayer = white,
                    BlackPlayer = black,
                    StartedAt = DateTime.UtcNow
                }
            };

            return await SendNotificationAsync(notification);
        }

        public async Task<NotificationDto> NotifyGameEndedAsync(int gameId, string result, string? reason)
        {
            var notification = new NotificationDto
            {
                Type = "game_ended",
                Title = "จบการแข่งขัน!",
                Message = $"เกม #{gameId} จบลงแล้ว ผล: {result}" + (string.IsNullOrWhiteSpace(reason) ? "" : $" ({reason})"),
                Severity = "success",
                CreatedAt = DateTime.UtcNow,
                Metadata = new
                {
                    GameId = gameId,
                    Result = result,
                    Reason = reason,
                    EndedAt = DateTime.UtcNow
                }
            };

            return await SendNotificationAsync(notification);
        }

        public async Task<NotificationDto> NotifyUserRegisteredAsync(int userId, string username, string email)
        {
            var notification = new NotificationDto
            {
                Type = "user_registered",
                Title = "ผู้ใช้งานใหม่ลงทะเบียน",
                Message = $"ผู้ใช้ {username} (ID: {userId}) ได้ทำการลงทะเบียนเข้าสู่ระบบ",
                Severity = "info",
                CreatedAt = DateTime.UtcNow,
                Metadata = new
                {
                    UserId = userId,
                    Username = username,
                    Email = email
                }
            };

            return await SendNotificationAsync(notification);
        }

        public Task<List<NotificationDto>> GetRecentNotificationsAsync(int limit = 50)
        {
            var list = _notifications
                .OrderByDescending(n => n.CreatedAt)
                .Take(limit)
                .ToList();

            return Task.FromResult(list);
        }

        public Task<int> GetUnreadCountAsync()
        {
            var count = _notifications.Count(n => !n.IsRead);
            return Task.FromResult(count);
        }

        public Task<bool> MarkAsReadAsync(string notificationId)
        {
            var item = _notifications.FirstOrDefault(n => n.Id == notificationId);
            if (item != null)
            {
                item.IsRead = true;
                return Task.FromResult(true);
            }
            return Task.FromResult(false);
        }

        public Task<bool> MarkAllAsReadAsync()
        {
            foreach (var n in _notifications)
            {
                n.IsRead = true;
            }
            return Task.FromResult(true);
        }

        public Task ClearAllAsync()
        {
            while (_notifications.TryDequeue(out _)) { }
            return Task.CompletedTask;
        }
    }
}
