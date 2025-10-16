using Microsoft.AspNetCore.Mvc;
using Repository.Interface;
using DTO;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace BBltZen.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OperationalNotificationServiceController : ControllerBase
    {
        private readonly IOperationalNotificationServiceRepository _notificationService;
        private readonly ILogger<OperationalNotificationServiceController> _logger;

        public OperationalNotificationServiceController(
            IOperationalNotificationServiceRepository notificationService,
            ILogger<OperationalNotificationServiceController> logger)
        {
            _notificationService = notificationService;
            _logger = logger;
        }

        [HttpPost("low-stock")]
        public async Task<ActionResult<List<LowStockNotificationDTO>>> NotifyLowStock()
        {
            try
            {
                _logger.LogInformation("Notifica ingredienti in esaurimento");
                var result = await _notificationService.NotifyLowStockAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore notifica ingredienti esauriti");
                return StatusCode(500, new { message = "Errore interno del server" });
            }
        }

        [HttpPost("order-status-change/{orderId}")]
        public async Task<ActionResult<OrderStatusNotificationDTO>> NotifyOrderStatusChange(int orderId, [FromBody] StatusChangeRequest request)
        {
            try
            {
                _logger.LogInformation($"Notifica cambio stato ordine: {orderId}");
                var result = await _notificationService.NotifyOrderStatusChangeAsync(orderId, request.NuovoStato);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, $"Ordine non trovato: {orderId}");
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Errore notifica cambio stato ordine: {orderId}");
                return StatusCode(500, new { message = "Errore interno del server" });
            }
        }

        [HttpPost("create")]
        public async Task<ActionResult<NotificationDTO>> CreateNotification([FromBody] CreateNotificationRequest request)
        {
            try
            {
                _logger.LogInformation($"Creazione notifica: {request.Titolo}");
                var result = await _notificationService.CreateNotificationAsync(
                    request.Tipo,
                    request.Titolo,
                    request.Messaggio,
                    request.Priorita
                );
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Errore creazione notifica: {request.Titolo}");
                return StatusCode(500, new { message = "Errore interno del server" });
            }
        }

        [HttpGet("unread")]
        public async Task<ActionResult<List<NotificationDTO>>> GetUnreadNotifications()
        {
            try
            {
                var result = await _notificationService.GetUnreadNotificationsAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore recupero notifiche non lette");
                return StatusCode(500, new { message = "Errore interno del server" });
            }
        }

        [HttpPost("mark-read/{notificationId}")]
        public async Task<ActionResult> MarkNotificationAsRead(int notificationId)
        {
            try
            {
                var result = await _notificationService.MarkNotificationAsReadAsync(notificationId);
                if (!result)
                    return NotFound(new { message = "Notifica non trovata" });

                return Ok(new { message = "Notifica segnata come letta" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Errore marcatura notifica come letta: {notificationId}");
                return StatusCode(500, new { message = "Errore interno del server" });
            }
        }

        [HttpGet("summary")]
        public async Task<ActionResult<NotificationSummaryDTO>> GetNotificationSummary()
        {
            try
            {
                var result = await _notificationService.GetNotificationSummaryAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore recupero summary notifiche");
                return StatusCode(500, new { message = "Errore interno del server" });
            }
        }

        [HttpPost("system-alert")]
        public async Task<ActionResult> NotifySystemAlert([FromBody] SystemAlertRequest request)
        {
            try
            {
                await _notificationService.NotifySystemAlertAsync(request.Messaggio, request.Priorita);
                return Ok(new { message = "Allarme sistema notificato" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Errore notifica allarme sistema: {request.Messaggio}");
                return StatusCode(500, new { message = "Errore interno del server" });
            }
        }

        [HttpPost("new-order/{orderId}")]
        public async Task<ActionResult> NotifyNewOrder(int orderId)
        {
            try
            {
                await _notificationService.NotifyNewOrderAsync(orderId);
                return Ok(new { message = "Nuovo ordine notificato" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Errore notifica nuovo ordine: {orderId}");
                return StatusCode(500, new { message = "Errore interno del server" });
            }
        }

        [HttpPost("payment-issue/{orderId}")]
        public async Task<ActionResult> NotifyPaymentIssue(int orderId)
        {
            try
            {
                await _notificationService.NotifyPaymentIssueAsync(orderId);
                return Ok(new { message = "Problema pagamento notificato" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Errore notifica problema pagamento: {orderId}");
                return StatusCode(500, new { message = "Errore interno del server" });
            }
        }

        [HttpPost("clean-old")]
        public async Task<ActionResult> CleanOldNotifications([FromQuery] int giorni = 30)
        {
            try
            {
                await _notificationService.CleanOldNotificationsAsync(giorni);
                return Ok(new { message = "Notifiche vecchie pulite" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore pulizia notifiche vecchie");
                return StatusCode(500, new { message = "Errore interno del server" });
            }
        }

        [HttpGet("pending-count")]
        public async Task<ActionResult<int>> GetPendingNotificationsCount()
        {
            try
            {
                var count = await _notificationService.GetPendingNotificationsCountAsync();
                return Ok(count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore conteggio notifiche pendenti");
                return StatusCode(500, new { message = "Errore interno del server" });
            }
        }
    }

    public class StatusChangeRequest
    {
        public string NuovoStato { get; set; } = null!;
    }

    public class CreateNotificationRequest
    {
        public string Tipo { get; set; } = null!;
        public string Titolo { get; set; } = null!;
        public string Messaggio { get; set; } = null!;
        public string Priorita { get; set; } = "Media";
    }

    public class SystemAlertRequest
    {
        public string Messaggio { get; set; } = null!;
        public string Priorita { get; set; } = "Media";
    }
}