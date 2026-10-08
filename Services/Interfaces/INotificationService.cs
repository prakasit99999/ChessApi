using System.Collections.Generic;
using System.Threading.Tasks;
using ChessApi.DTOs.Notifications;

namespace ChessApi.Services.Interfaces
{
    public interface INotificationService
    {
        Task<NotificationDto> SendNotificationAsync(NotificationDto notification);
        Task<NotificationDto> NotifyGameStartedAsync(int gameId, string gameType, string? whitePlayer, string? blackPlayer);
        Task<NotificationDto> NotifyGameEndedAsync(int gameId, string result, string? reason);
        Task<NotificationDto> NotifyUserRegisteredAsync(int userId, string username, string email);
        Task<List<NotificationDto>> GetRecentNotificationsAsync(int limit = 50);
        Task<int> GetUnreadCountAsync();
        Task<bool> MarkAsReadAsync(string notificationId);
        Task<bool> MarkAllAsReadAsync();
        Task ClearAllAsync();
    }
}
