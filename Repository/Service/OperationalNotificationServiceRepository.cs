using Database;
using DTO;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Repository.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Repository.Service
{
    public class OperationalNotificationServiceRepository : IOperationalNotificationServiceRepository
    {
        private readonly BubbleTeaContext _context;
        private readonly ILogger<OperationalNotificationServiceRepository> _logger;

        public OperationalNotificationServiceRepository(
            BubbleTeaContext context,
            ILogger<OperationalNotificationServiceRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        private NotificationDTO MapToNotificationDTO(NotificheOperative notifica)
        {
            return new NotificationDTO
            {
                NotificationId = notifica.NotificaId,
                TipoNotifica = "SISTEMA",
                Titolo = $"Notifica #{notifica.NotificaId}",
                Messaggio = notifica.Messaggio,
                Priorita = ConvertIntToPriorita(notifica.Priorita),
                Letta = notifica.Stato == "Gestita",
                DataCreazione = notifica.DataCreazione
            };
        }

        private LowStockNotificationDTO MapToLowStockNotificationDTO(int ingredienteId, string nomeIngrediente, string categoria, int bevandeAffette)
        {
            return new LowStockNotificationDTO
            {
                IngredienteId = ingredienteId,
                NomeIngrediente = nomeIngrediente,
                Categoria = categoria,
                BevandeAffette = bevandeAffette,
                DataRilevamento = DateTime.UtcNow
            };
        }

        public async Task<IEnumerable<LowStockNotificationDTO>> NotifyLowStockAsync()
        {
            try
            {
                _logger.LogInformation("Controllo ingredienti in esaurimento");

                var ingredientiNonDisponibili = await _context.Ingrediente
                    .Where(i => !i.Disponibile)
                    .Include(i => i.Categoria)
                    .Select(i => new
                    {
                        i.IngredienteId,
                        i.Ingrediente1,
                        i.Categoria,
                        BevandeAffette = _context.PersonalizzazioneIngrediente
                            .Count(pi => pi.IngredienteId == i.IngredienteId)
                    })
                    .ToListAsync();

                var notifiche = new List<LowStockNotificationDTO>();

                foreach (var ingrediente in ingredientiNonDisponibili)
                {
                    // ✅ USA SOLO QUESTO MapToDTO - UNICO E CORRETTO
                    var notifica = MapToLowStockNotificationDTO(
                        ingrediente.IngredienteId,
                        ingrediente.Ingrediente1,
                        ingrediente.Categoria.Categoria,
                        ingrediente.BevandeAffette
                    );

                    notifiche.Add(notifica);

                    await CreateNotificationAsync(
                        "LOW_STOCK",
                        $"Ingrediente Esaurito: {ingrediente.Ingrediente1}",
                        $"L'ingrediente {ingrediente.Ingrediente1} è esaurito. {ingrediente.BevandeAffette} bevande affette.",
                        "Alta"
                    );
                }

                _logger.LogInformation("Trovati {Count} ingredienti esauriti", notifiche.Count);
                return notifiche;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore controllo ingredienti esauriti");
                return Enumerable.Empty<LowStockNotificationDTO>();
            }
        }

        public async Task<OrderStatusNotificationDTO> NotifyOrderStatusChangeAsync(int orderId, string nuovoStato)
        {
            try
            {
                _logger.LogInformation("Notifica cambio stato ordine: {OrderId} -> {NuovoStato}", orderId, nuovoStato);

                var order = await _context.Ordine
                    .FirstOrDefaultAsync(o => o.OrdineId == orderId);

                if (order == null)
                    throw new ArgumentException($"Ordine non trovato: {orderId}");

                // ✅ USA COSTRUTTORE DIRETTO - NIENTE MapDTO
                var notifica = new OrderStatusNotificationDTO
                {
                    OrderId = orderId,
                    VecchioStato = order.StatoOrdineId.ToString(),
                    NuovoStato = nuovoStato,
                    ClienteId = order.ClienteId,
                    DataCambiamento = DateTime.UtcNow
                };

                await CreateNotificationAsync(
                    "ORDER_STATUS_CHANGE",
                    $"Ordine {orderId} Aggiornato",
                    $"Ordine {orderId}: {order.StatoOrdineId} → {nuovoStato}",
                    "Media"
                );

                _logger.LogInformation("Notifica cambio stato ordine {OrderId} creata", orderId);
                return notifica;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore notifica cambio stato ordine: {OrderId}", orderId);
                throw;
            }
        }

        public async Task<NotificationDTO> CreateNotificationAsync(string tipo, string titolo, string messaggio, string priorita)
        {
            try
            {
                var notifica = new NotificheOperative
                {
                    DataCreazione = DateTime.UtcNow,
                    OrdiniCoinvolti = "",
                    Messaggio = messaggio,
                    Stato = "Attiva",
                    Priorita = ConvertPrioritaToInt(priorita)
                };

                _context.NotificheOperative.Add(notifica);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Notifica creata: {Titolo}", titolo);

                // ✅ USA MapToNotificationDTO CHE HAI GIÀ - MA ORA CREA UNA NOTIFICA PERSONALIZZATA
                var result = MapToNotificationDTO(notifica);

                // ✅ SOVRASCRIVI I CAMPI CHE VOGLIAMO PERSONALIZZARE
                result.TipoNotifica = tipo; // "TEST" invece di "SISTEMA"
                result.Titolo = titolo;     // "Test Notification" invece di "Notifica #{id}"

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore creazione notifica: {Titolo}", titolo);
                throw;
            }
        }

        public async Task<IEnumerable<NotificationDTO>> GetUnreadNotificationsAsync()
        {
            try
            {
                var notifiche = await _context.NotificheOperative
                    .Where(n => n.Stato != "Gestita")
                    .OrderByDescending(n => n.DataCreazione)
                    .Select(n => MapToNotificationDTO(n)) // ✅ USA SOLO MapToNotificationDTO CHE HAI GIÀ
                    .ToListAsync();

                return notifiche;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore recupero notifiche non lette");
                return Enumerable.Empty<NotificationDTO>();
            }
        }

        public async Task<bool> MarkNotificationAsReadAsync(int notificationId)
        {
            try
            {
                var notifica = await _context.NotificheOperative
                    .FirstOrDefaultAsync(n => n.NotificaId == notificationId);

                if (notifica == null)
                    return false;

                notifica.Stato = "Gestita";
                notifica.DataGestione = DateTime.Now;
                notifica.UtenteGestione = "Sistema";

                await _context.SaveChangesAsync();

                _logger.LogInformation($"Notifica {notificationId} segnata come gestita");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Errore marcatura notifica come gestita: {notificationId}");
                return false;
            }
        }

        public async Task<NotificationSummaryDTO> GetNotificationSummaryAsync()
        {
            try
            {
                var notificheTotali = await _context.NotificheOperative.CountAsync();
                var notificheNonLette = await _context.NotificheOperative.CountAsync(n => n.Stato != "Gestita");
                var notificheAltaPriorita = await _context.NotificheOperative.CountAsync(n => n.Priorita == 1 && n.Stato != "Gestita");

                var ultimeNotifiche = await _context.NotificheOperative
                    .Where(n => n.Stato != "Gestita")
                    .OrderByDescending(n => n.DataCreazione)
                    .Take(5)
                    .Select(n => MapToNotificationDTO(n)) // ✅ USA SOLO MapToNotificationDTO CHE HAI GIÀ
                    .ToListAsync();

                return new NotificationSummaryDTO
                {
                    TotalNotifiche = notificheTotali,
                    NotificheNonLette = notificheNonLette,
                    NotificheAltaPriorita = notificheAltaPriorita,
                    UltimeNotifiche = ultimeNotifiche,
                    DataAggiornamento = DateTime.UtcNow
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore recupero summary notifiche");
                return new NotificationSummaryDTO();
            }
        }

        public async Task NotifySystemAlertAsync(string messaggio, string priorita = "Media")
        {
            try
            {
                await CreateNotificationAsync(
                    "SYSTEM_ALERT",
                    "Allarme Sistema",
                    messaggio,
                    priorita
                );

                _logger.LogWarning($"Allarme sistema: {messaggio}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Errore notifica allarme sistema: {messaggio}");
            }
        }

        public async Task NotifyNewOrderAsync(int orderId)
        {
            try
            {
                var notifica = new NotificheOperative
                {
                    DataCreazione = DateTime.Now,
                    OrdiniCoinvolti = orderId.ToString(),
                    Messaggio = $"È stato ricevuto un nuovo ordine: #{orderId}",
                    Stato = "Attiva",
                    Priorita = 3 // Bassa priorità
                };

                _context.NotificheOperative.Add(notifica);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Notifica nuovo ordine: {orderId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Errore notifica nuovo ordine: {orderId}");
            }
        }

        public async Task NotifyPaymentIssueAsync(int orderId)
        {
            try
            {
                var notifica = new NotificheOperative
                {
                    DataCreazione = DateTime.Now,
                    OrdiniCoinvolti = orderId.ToString(),
                    Messaggio = $"Problema con il pagamento dell'ordine #{orderId}",
                    Stato = "Attiva",
                    Priorita = 1 // Alta priorità
                };

                _context.NotificheOperative.Add(notifica);
                await _context.SaveChangesAsync();

                _logger.LogWarning($"Notifica problema pagamento: {orderId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Errore notifica problema pagamento: {orderId}");
            }
        }

        public async Task CleanOldNotificationsAsync(int giorni = 30)
        {
            try
            {
                var dataLimite = DateTime.Now.AddDays(-giorni);

                var notificheVecchie = await _context.NotificheOperative
                    .Where(n => n.DataCreazione < dataLimite && n.Stato == "Gestita")
                    .ToListAsync();

                _context.NotificheOperative.RemoveRange(notificheVecchie);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Rimosse {notificheVecchie.Count} notifiche vecchie");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore pulizia notifiche vecchie");
            }
        }

        public async Task<int> GetPendingNotificationsCountAsync()
        {
            try
            {
                var count = await _context.NotificheOperative
                    .CountAsync(n => n.Stato != "Gestita");

                return count;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore conteggio notifiche pendenti");
                return 0;
            }
        }

        // Metodi helper per conversione priorità
        private int ConvertPrioritaToInt(string priorita)
        {
            return priorita.ToLower() switch
            {
                "alta" => 1,
                "media" => 2,
                "bassa" => 3,
                _ => 2
            };
        }

        private string ConvertIntToPriorita(int priorita)
        {
            return priorita switch
            {
                1 => "Alta",
                2 => "Media",
                3 => "Bassa",
                _ => "Media"
            };
        }

        public async Task<bool> ExistsAsync(int notificationId)
        {
            try
            {
                var exists = await _context.NotificheOperative
                    .AnyAsync(n => n.NotificaId == notificationId);
                return exists;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore verifica esistenza notifica: {NotificationId}", notificationId);
                return false;
            }
        }
    }
}