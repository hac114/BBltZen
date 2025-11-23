using DTO;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Repository.Interface
{
    public interface IOperationalNotificationServiceRepository
    {
        // ✅ NOTIFICHE AUTOMATICHE - ALLINEATO
        Task<IEnumerable<LowStockNotificationDTO>> NotifyLowStockAsync();
        Task<OrderStatusNotificationDTO> NotifyOrderStatusChangeAsync(int orderId, string nuovoStato);

        // ✅ GESTIONE NOTIFICHE - ALLINEATO
        Task<NotificationDTO> CreateNotificationAsync(string tipo, string titolo, string messaggio, string priorita);
        Task<IEnumerable<NotificationDTO>> GetUnreadNotificationsAsync();
        Task<bool> MarkNotificationAsReadAsync(int notificationId);
        Task<NotificationSummaryDTO> GetNotificationSummaryAsync();

        // ✅ NOTIFICHE SPECIFICHE - ALLINEATO
        Task NotifySystemAlertAsync(string messaggio, string priorita = "Media");
        Task NotifyNewOrderAsync(int orderId);
        Task NotifyPaymentIssueAsync(int orderId);

        // ✅ UTILITY - ALLINEATO
        Task CleanOldNotificationsAsync(int giorni = 30);
        Task<int> GetPendingNotificationsCountAsync();

        // ✅ AGGIUNTO PER COMPLETEZZA PATTERN
        Task<bool> ExistsAsync(int notificationId);
    }
}