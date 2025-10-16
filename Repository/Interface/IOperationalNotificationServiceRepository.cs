using DTO;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Repository.Interface
{
    public interface IOperationalNotificationServiceRepository
    {
        // Notifiche automatiche
        Task<List<LowStockNotificationDTO>> NotifyLowStockAsync();
        Task<OrderStatusNotificationDTO> NotifyOrderStatusChangeAsync(int orderId, string nuovoStato);

        // Gestione notifiche
        Task<NotificationDTO> CreateNotificationAsync(string tipo, string titolo, string messaggio, string priorita);
        Task<List<NotificationDTO>> GetUnreadNotificationsAsync();
        Task<bool> MarkNotificationAsReadAsync(int notificationId);
        Task<NotificationSummaryDTO> GetNotificationSummaryAsync();

        // Notifiche specifiche
        Task NotifySystemAlertAsync(string messaggio, string priorita = "Media");
        Task NotifyNewOrderAsync(int orderId);
        Task NotifyPaymentIssueAsync(int orderId);

        // Utility
        Task CleanOldNotificationsAsync(int giorni = 30);
        Task<int> GetPendingNotificationsCountAsync();
    }
}