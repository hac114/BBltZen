using Microsoft.AspNetCore.Mvc;
using DTO;
using Repository.Interface;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using Database.Models;

namespace BBltZen.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    //[Authorize] // ✅ COMMENTATO PER TEST CON SWAGGER
    public class OperationalNotificationServiceController : SecureBaseController
    {
        private readonly IOperationalNotificationServiceRepository _notificationService;
        private readonly BubbleTeaContext _context;

        public OperationalNotificationServiceController(
            IOperationalNotificationServiceRepository notificationService,
            BubbleTeaContext context,
            IWebHostEnvironment environment,
            ILogger<OperationalNotificationServiceController> logger)
            : base(environment, logger)
        {
            _notificationService = notificationService;
            _context = context;
        }

        [HttpPost("low-stock")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<LowStockNotificationDTO>>> NotifyLowStock()
        {
            try
            {
                _logger.LogInformation("Notifica ingredienti in esaurimento");
                var result = await _notificationService.NotifyLowStockAsync();

                LogAuditTrail("NOTIFY_LOW_STOCK", "NotificationService", $"Found: {result.Count()} items");
                LogSecurityEvent("LowStockNotificationTriggered", new
                {
                    Count = result.Count(),
                    User = User.Identity?.Name ?? "System",
                    Timestamp = DateTime.UtcNow
                });

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore notifica ingredienti esauriti");
                return SafeInternalError<IEnumerable<LowStockNotificationDTO>>(
                    _environment.IsDevelopment()
                        ? $"Errore notifica ingredienti esauriti: {ex.Message}"
                        : "Errore interno nel controllo ingredienti"
                );
            }
        }

        [HttpPost("order-status-change/{orderId}")]
        //[Authorize(Roles = "admin,manager,system")] // ✅ COMMENTATO PER TEST
        public async Task<ActionResult<OrderStatusNotificationDTO>> NotifyOrderStatusChange(int orderId, [FromBody] StatusChangeRequestDTO request)
        {
            try
            {
                if (orderId <= 0)
                    return SafeBadRequest<OrderStatusNotificationDTO>("ID ordine non valido");

                // ✅ Validazione del DTO della richiesta
                if (!IsModelValid(request))
                    return SafeBadRequest<OrderStatusNotificationDTO>("Dati richiesta non validi");

                // ✅ Controllo esistenza ordine con BubbleTeaContext
                var orderExists = await _context.Ordine.AnyAsync(o => o.OrdineId == orderId);
                if (!orderExists)
                    return SafeNotFound<OrderStatusNotificationDTO>("Ordine non trovato");

                _logger.LogInformation($"Notifica cambio stato ordine: {orderId}");
                var result = await _notificationService.NotifyOrderStatusChangeAsync(orderId, request.NuovoStato);

                // ✅ Log per audit
                LogAuditTrail("NOTIFY_ORDER_STATUS_CHANGE", "NotificationService", orderId.ToString());
                LogSecurityEvent("OrderStatusNotificationTriggered", new
                {
                    OrderId = orderId,
                    NewStatus = request.NuovoStato,
                    User = User.Identity?.Name ?? "System",
                    Timestamp = DateTime.UtcNow,
                    IPAddress = HttpContext.Connection.RemoteIpAddress?.ToString()
                });

                return Ok(result);
            }
            catch (ArgumentException argEx)
            {
                _logger.LogWarning(argEx, $"Ordine non trovato: {orderId}");
                return SafeNotFound<OrderStatusNotificationDTO>(
                    _environment.IsDevelopment()
                        ? argEx.Message
                        : "Ordine non trovato"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Errore notifica cambio stato ordine: {orderId}");
                return SafeInternalError<OrderStatusNotificationDTO>(
                    _environment.IsDevelopment()
                        ? $"Errore notifica cambio stato ordine {orderId}: {ex.Message}"
                        : "Errore interno nella notifica cambio stato"
                );
            }
        }

        [HttpPost("create")]
        [AllowAnonymous]
        public async Task<ActionResult<NotificationDTO>> CreateNotification([FromBody] CreateNotificationRequestDTO request)
        {
            try
            {
                if (!IsModelValid(request))
                    return SafeBadRequest<NotificationDTO>("Dati notifica non validi");

                _logger.LogInformation("Creazione notifica: {Titolo}", request.Titolo);
                var result = await _notificationService.CreateNotificationAsync(
                    request.Tipo,
                    request.Titolo,
                    request.Messaggio,
                    request.Priorita
                );

                LogAuditTrail("CREATE_NOTIFICATION", "NotificationService", result.NotificationId.ToString());
                LogSecurityEvent("NotificationCreated", new
                {
                    result.NotificationId, // ✅ SEMPLIFICATO
                    request.Tipo, // ✅ SEMPLIFICATO
                    request.Priorita, // ✅ SEMPLIFICATO
                    User = User.Identity?.Name ?? "System",
                    Timestamp = DateTime.UtcNow,
                    IPAddress = HttpContext.Connection.RemoteIpAddress?.ToString()
                });

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore creazione notifica: {Titolo}", request.Titolo);
                return SafeInternalError<NotificationDTO>(
                    _environment.IsDevelopment()
                        ? $"Errore creazione notifica: {ex.Message}"
                        : "Errore interno nella creazione notifica"
                );
            }
        }

        [HttpGet("unread")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<NotificationDTO>>> GetUnreadNotifications()
        {
            try
            {
                var result = await _notificationService.GetUnreadNotificationsAsync();

                LogAuditTrail("GET_UNREAD_NOTIFICATIONS", "NotificationService", $"Count: {result.Count()}");

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore recupero notifiche non lette");
                return SafeInternalError<IEnumerable<NotificationDTO>>(
                    _environment.IsDevelopment()
                        ? $"Errore recupero notifiche non lette: {ex.Message}"
                        : "Errore interno nel recupero notifiche"
                );
            }
        }

        [HttpPost("mark-read/{notificationId}")]
        //[Authorize(Roles = "admin,manager,operator")] // ✅ COMMENTATO PER TEST
        public async Task<ActionResult> MarkNotificationAsRead(int notificationId)
        {
            try
            {
                if (notificationId <= 0)
                    return SafeBadRequest("ID notifica non valido");

                var result = await _notificationService.MarkNotificationAsReadAsync(notificationId);
                if (!result)
                    return SafeNotFound("Notifica non trovata");

                // ✅ Log per audit
                LogAuditTrail("MARK_NOTIFICATION_READ", "NotificationService", notificationId.ToString());
                LogSecurityEvent("NotificationMarkedAsRead", new
                {
                    NotificationId = notificationId,
                    User = User.Identity?.Name ?? "Unknown",
                    Timestamp = DateTime.UtcNow,
                    IPAddress = HttpContext.Connection.RemoteIpAddress?.ToString()
                });

                return Ok(new { message = "Notifica segnata come letta" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Errore marcatura notifica come letta: {notificationId}");
                return SafeInternalError(
                    _environment.IsDevelopment()
                        ? $"Errore marcatura notifica {notificationId}: {ex.Message}"
                        : "Errore interno nella marcatura notifica"
                );
            }
        }

        [HttpGet("summary")]
        [AllowAnonymous] // ✅ AGGIUNTO ESPLICITAMENTE
        public async Task<ActionResult<NotificationSummaryDTO>> GetNotificationSummary()
        {
            try
            {
                var result = await _notificationService.GetNotificationSummaryAsync();

                // ✅ Log per audit
                LogAuditTrail("GET_NOTIFICATION_SUMMARY", "NotificationService",
                    $"Total: {result.TotalNotifiche}, Unread: {result.NotificheNonLette}");

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore recupero summary notifiche");
                return SafeInternalError<NotificationSummaryDTO>(
                    _environment.IsDevelopment()
                        ? $"Errore recupero summary notifiche: {ex.Message}"
                        : "Errore interno nel recupero summary"
                );
            }
        }

        [HttpPost("system-alert")]
        //[Authorize(Roles = "admin,system")] // ✅ COMMENTATO PER TEST
        public async Task<ActionResult> NotifySystemAlert([FromBody] SystemAlertRequestDTO request)
        {
            try
            {
                // ✅ Validazione del DTO della richiesta
                if (!IsModelValid(request))
                    return SafeBadRequest("Dati allarme sistema non validi");

                await _notificationService.NotifySystemAlertAsync(request.Messaggio, request.Priorita);

                // ✅ Log per audit e sicurezza
                LogAuditTrail("NOTIFY_SYSTEM_ALERT", "NotificationService", request.Priorita);
                LogSecurityEvent("SystemAlertTriggered", new
                {
                    Message = request.Messaggio,
                    Priority = request.Priorita,
                    User = User.Identity?.Name ?? "System",
                    Timestamp = DateTime.UtcNow,
                    IPAddress = HttpContext.Connection.RemoteIpAddress?.ToString()
                });

                return Ok(new { message = "Allarme sistema notificato" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Errore notifica allarme sistema: {request.Messaggio}");
                return SafeInternalError(
                    _environment.IsDevelopment()
                        ? $"Errore notifica allarme sistema: {ex.Message}"
                        : "Errore interno nella notifica allarme"
                );
            }
        }

        [HttpPost("new-order/{orderId}")]
        //[Authorize(Roles = "admin,manager,system")] // ✅ COMMENTATO PER TEST
        public async Task<ActionResult> NotifyNewOrder(int orderId)
        {
            try
            {
                if (orderId <= 0)
                    return SafeBadRequest("ID ordine non valido");

                // ✅ Controllo esistenza ordine
                var orderExists = await _context.Ordine.AnyAsync(o => o.OrdineId == orderId);
                if (!orderExists)
                    return SafeNotFound("Ordine non trovato");

                await _notificationService.NotifyNewOrderAsync(orderId);

                // ✅ Log per audit
                LogAuditTrail("NOTIFY_NEW_ORDER", "NotificationService", orderId.ToString());
                LogSecurityEvent("NewOrderNotification", new
                {
                    OrderId = orderId,
                    User = User.Identity?.Name ?? "System",
                    Timestamp = DateTime.UtcNow
                });

                return Ok(new { message = "Nuovo ordine notificato" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Errore notifica nuovo ordine: {orderId}");
                return SafeInternalError(
                    _environment.IsDevelopment()
                        ? $"Errore notifica nuovo ordine {orderId}: {ex.Message}"
                        : "Errore interno nella notifica nuovo ordine"
                );
            }
        }

        [HttpPost("payment-issue/{orderId}")]
        //[Authorize(Roles = "admin,manager,system")] // ✅ COMMENTATO PER TEST
        public async Task<ActionResult> NotifyPaymentIssue(int orderId)
        {
            try
            {
                if (orderId <= 0)
                    return SafeBadRequest("ID ordine non valido");

                // ✅ Controllo esistenza ordine
                var orderExists = await _context.Ordine.AnyAsync(o => o.OrdineId == orderId);
                if (!orderExists)
                    return SafeNotFound("Ordine non trovato");

                await _notificationService.NotifyPaymentIssueAsync(orderId);

                // ✅ Log per audit
                LogAuditTrail("NOTIFY_PAYMENT_ISSUE", "NotificationService", orderId.ToString());
                LogSecurityEvent("PaymentIssueNotification", new
                {
                    OrderId = orderId,
                    User = User.Identity?.Name ?? "System",
                    Timestamp = DateTime.UtcNow,
                    IPAddress = HttpContext.Connection.RemoteIpAddress?.ToString()
                });

                return Ok(new { message = "Problema pagamento notificato" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Errore notifica problema pagamento: {orderId}");
                return SafeInternalError(
                    _environment.IsDevelopment()
                        ? $"Errore notifica problema pagamento {orderId}: {ex.Message}"
                        : "Errore interno nella notifica problema pagamento"
                );
            }
        }

        [HttpPost("clean-old")]
        //[Authorize(Roles = "admin,system")] // ✅ COMMENTATO PER TEST
        public async Task<ActionResult> CleanOldNotifications([FromQuery] int giorni = 30)
        {
            try
            {
                if (giorni <= 0)
                    return SafeBadRequest("Numero giorni non valido");

                await _notificationService.CleanOldNotificationsAsync(giorni);

                // ✅ Log per audit
                LogAuditTrail("CLEAN_OLD_NOTIFICATIONS", "NotificationService", $"Days: {giorni}");
                LogSecurityEvent("OldNotificationsCleaned", new
                {
                    Days = giorni,
                    User = User.Identity?.Name ?? "System",
                    Timestamp = DateTime.UtcNow
                });

                return Ok(new { message = "Notifiche vecchie pulite" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore pulizia notifiche vecchie");
                return SafeInternalError(
                    _environment.IsDevelopment()
                        ? $"Errore pulizia notifiche vecchie: {ex.Message}"
                        : "Errore interno nella pulizia notifiche"
                );
            }
        }

        [HttpGet("pending-count")]
        [AllowAnonymous] // ✅ AGGIUNTO ESPLICITAMENTE
        public async Task<ActionResult<int>> GetPendingNotificationsCount()
        {
            try
            {
                var count = await _notificationService.GetPendingNotificationsCountAsync();

                // ✅ Log per audit
                LogAuditTrail("GET_PENDING_NOTIFICATIONS_COUNT", "NotificationService", count.ToString());

                return Ok(count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore conteggio notifiche pendenti");
                return SafeInternalError<int>(
                    _environment.IsDevelopment()
                        ? $"Errore conteggio notifiche pendenti: {ex.Message}"
                        : "Errore interno nel conteggio notifiche"
                );
            }
        }

        [HttpGet("exists/{notificationId}")]
        [AllowAnonymous]
        public async Task<ActionResult<bool>> Exists(int notificationId)
        {
            try
            {
                if (notificationId <= 0)
                    return SafeBadRequest<bool>("ID notifica non valido");

                var exists = await _notificationService.ExistsAsync(notificationId);
                return Ok(exists);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore verifica esistenza notifica: {NotificationId}", notificationId);
                return SafeInternalError<bool>("Errore durante la verifica esistenza");
            }
        }
    }
}