using Microsoft.AspNetCore.Mvc;
using DTO;
using Repository.Interface;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;

namespace BBltZen.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    //[Authorize] // ✅ COMMENTATO PER TEST CON SWAGGER
    public class NotificheOperativeController : SecureBaseController
    {
        private readonly INotificheOperativeRepository _repository;

        public NotificheOperativeController(
            INotificheOperativeRepository repository,
            IWebHostEnvironment environment,
            ILogger<NotificheOperativeController> logger)
            : base(environment, logger)
        {
            _repository = repository;
        }

        [HttpGet]
        //[Authorize(Roles = "admin,manager,operator")] // ✅ COMMENTATO PER TEST
        public async Task<ActionResult<IEnumerable<NotificheOperativeDTO>>> GetAll()
        {
            try
            {
                var result = await _repository.GetAllAsync();

                // ✅ Log per audit
                LogAuditTrail("GET_ALL_NOTIFICHE_OPERATIVE", "NotificheOperative", "All");

                return Ok(result);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore nel recupero di tutte le notifiche operative");
                return SafeInternalError(
                    _environment.IsDevelopment()
                        ? $"Errore nel recupero delle notifiche operative: {ex.Message}"
                        : "Errore interno nel recupero delle notifiche"
                );
            }
        }

        [HttpGet("{notificaId}")]
        //[Authorize(Roles = "admin,manager,operator")] // ✅ COMMENTATO PER TEST
        public async Task<ActionResult<NotificheOperativeDTO>> GetById(int notificaId)
        {
            try
            {
                if (notificaId <= 0)
                    return SafeBadRequest(
                        _environment.IsDevelopment()
                            ? "ID notifica non valido: deve essere maggiore di 0"
                            : "ID notifica non valido"
                    );

                var result = await _repository.GetByIdAsync(notificaId);

                if (result == null)
                    return SafeNotFound(
                        _environment.IsDevelopment()
                            ? $"Notifica operativa con ID {notificaId} non trovata"
                            : "Notifica operativa non trovata"
                    );

                // ✅ Log per audit
                LogAuditTrail("GET_NOTIFICA_OPERATIVA_BY_ID", "NotificheOperative", notificaId.ToString());

                return Ok(result);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore nel recupero notifica operativa {NotificaId}", notificaId);
                return SafeInternalError(
                    _environment.IsDevelopment()
                        ? $"Errore nel recupero notifica operativa {notificaId}: {ex.Message}"
                        : "Errore interno nel recupero notifica"
                );
            }
        }

        [HttpGet("stato/{stato}")]
        //[Authorize(Roles = "admin,manager,operator")] // ✅ COMMENTATO PER TEST
        public async Task<ActionResult<IEnumerable<NotificheOperativeDTO>>> GetByStato(string stato)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(stato))
                    return SafeBadRequest(
                        _environment.IsDevelopment()
                            ? "Stato notifica non valido: non può essere vuoto"
                            : "Stato notifica non valido"
                    );

                var result = await _repository.GetByStatoAsync(stato);

                // ✅ Log per audit
                LogAuditTrail("GET_NOTIFICHE_BY_STATO", "NotificheOperative", stato);

                return Ok(result);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore nel recupero notifiche per stato {Stato}", stato);
                return SafeInternalError(
                    _environment.IsDevelopment()
                        ? $"Errore nel recupero notifiche per stato {stato}: {ex.Message}"
                        : "Errore interno nel recupero notifiche per stato"
                );
            }
        }

        [HttpGet("priorita/{priorita}")]
        //[Authorize(Roles = "admin,manager,operator")] // ✅ COMMENTATO PER TEST
        public async Task<ActionResult<IEnumerable<NotificheOperativeDTO>>> GetByPriorita(int priorita)
        {
            try
            {
                if (priorita <= 0)
                    return SafeBadRequest(
                        _environment.IsDevelopment()
                            ? "Priorità non valida: deve essere maggiore di 0"
                            : "Priorità non valida"
                    );

                var result = await _repository.GetByPrioritaAsync(priorita);

                // ✅ Log per audit
                LogAuditTrail("GET_NOTIFICHE_BY_PRIORITA", "NotificheOperative", priorita.ToString());

                return Ok(result);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore nel recupero notifiche per priorità {Priorita}", priorita);
                return SafeInternalError(
                    _environment.IsDevelopment()
                        ? $"Errore nel recupero notifiche per priorità {priorita}: {ex.Message}"
                        : "Errore interno nel recupero notifiche per priorità"
                );
            }
        }

        [HttpGet("pendenti")]
        //[Authorize(Roles = "admin,manager,operator")] // ✅ COMMENTATO PER TEST
        public async Task<ActionResult<IEnumerable<NotificheOperativeDTO>>> GetPendenti()
        {
            try
            {
                var result = await _repository.GetPendentiAsync();

                // ✅ Log per audit
                LogAuditTrail("GET_NOTIFICHE_PENDENTI", "NotificheOperative", $"Count: {result?.Count()}");

                return Ok(result);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore nel recupero notifiche pendenti");
                return SafeInternalError(
                    _environment.IsDevelopment()
                        ? $"Errore nel recupero notifiche pendenti: {ex.Message}"
                        : "Errore interno nel recupero notifiche pendenti"
                );
            }
        }

        [HttpGet("periodo")]
        //[Authorize(Roles = "admin,manager,operator")] // ✅ COMMENTATO PER TEST
        public async Task<ActionResult<IEnumerable<NotificheOperativeDTO>>> GetByPeriodo([FromQuery] DateTime dataInizio, [FromQuery] DateTime dataFine)
        {
            try
            {
                if (dataInizio > dataFine)
                    return SafeBadRequest(
                        _environment.IsDevelopment()
                            ? $"Data inizio ({dataInizio:yyyy-MM-dd}) non può essere successiva alla data fine ({dataFine:yyyy-MM-dd})"
                            : "Intervallo date non valido"
                    );

                var result = await _repository.GetByPeriodoAsync(dataInizio, dataFine);

                // ✅ Log per audit
                LogAuditTrail("GET_NOTIFICHE_BY_PERIODO", "NotificheOperative", $"{dataInizio:yyyy-MM-dd}_{dataFine:yyyy-MM-dd}");

                return Ok(result);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore nel recupero notifiche per periodo {DataInizio} - {DataFine}", dataInizio, dataFine);
                return SafeInternalError(
                    _environment.IsDevelopment()
                        ? $"Errore nel recupero notifiche per periodo {dataInizio:yyyy-MM-dd} - {dataFine:yyyy-MM-dd}: {ex.Message}"
                        : "Errore interno nel recupero notifiche per periodo"
                );
            }
        }

        [HttpGet("pendenti/numero")]
        //[Authorize(Roles = "admin,manager,operator")] // ✅ COMMENTATO PER TEST
        public async Task<ActionResult<int>> GetNumeroPendenti()
        {
            try
            {
                var result = await _repository.GetNumeroNotifichePendentiAsync();

                // ✅ Log per audit
                LogAuditTrail("GET_NUMERO_NOTIFICHE_PENDENTI", "NotificheOperative", result.ToString());

                return Ok(result);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore nel recupero numero notifiche pendenti");
                return SafeInternalError(
                    _environment.IsDevelopment()
                        ? $"Errore nel recupero numero notifiche pendenti: {ex.Message}"
                        : "Errore interno nel recupero numero notifiche pendenti"
                );
            }
        }

        [HttpPost]
        //[Authorize(Roles = "admin,manager")] // ✅ COMMENTATO PER TEST
        public async Task<ActionResult<NotificheOperativeDTO>> Create([FromBody] NotificheOperativeDTO notificaDto)
        {
            try
            {
                // ✅ La validazione dei campi è gestita automaticamente dai Data Annotations del DTO
                if (!IsModelValid(notificaDto))
                    return SafeBadRequest(
                        _environment.IsDevelopment()
                            ? "Dati notifica operativa non validi: modello di binding fallito"
                            : "Dati notifica non validi"
                    );

                await _repository.AddAsync(notificaDto);

                // ✅ Log per audit e sicurezza
                LogAuditTrail("CREATE_NOTIFICA_OPERATIVA", "NotificheOperative", notificaDto.NotificaId.ToString());
                LogSecurityEvent("NotificaOperativaCreated", new
                {
                    NotificaId = notificaDto.NotificaId,
                    Priorita = notificaDto.Priorita,
                    Stato = notificaDto.Stato,
                    OrdiniCoinvolti = notificaDto.OrdiniCoinvolti,
                    User = User.Identity?.Name ?? "Unknown",
                    Timestamp = DateTime.UtcNow
                });

                return CreatedAtAction(nameof(GetById), new { notificaId = notificaDto.NotificaId }, notificaDto);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore nella creazione notifica operativa");
                return SafeInternalError(
                    _environment.IsDevelopment()
                        ? $"Errore nella creazione notifica operativa: {ex.Message}"
                        : "Errore interno nella creazione notifica"
                );
            }
        }

        [HttpPut("{notificaId}")]
        //[Authorize(Roles = "admin,manager")] // ✅ COMMENTATO PER TEST
        public async Task<ActionResult> Update(int notificaId, [FromBody] NotificheOperativeDTO notificaDto)
        {
            try
            {
                if (notificaId <= 0)
                    return SafeBadRequest(
                        _environment.IsDevelopment()
                            ? "ID notifica non valido: deve essere maggiore di 0"
                            : "ID notifica non valido"
                    );

                // ✅ La validazione dei campi è gestita automaticamente dai Data Annotations del DTO
                if (!IsModelValid(notificaDto))
                    return SafeBadRequest(
                        _environment.IsDevelopment()
                            ? "Dati notifica operativa non validi: modello di binding fallito"
                            : "Dati notifica non validi"
                    );

                if (notificaDto.NotificaId != notificaId)
                    return SafeBadRequest(
                        _environment.IsDevelopment()
                            ? $"ID notifica non corrispondente: URL={notificaId}, Body={notificaDto.NotificaId}"
                            : "Identificativi non corrispondenti"
                    );

                // Verifica esistenza
                var exists = await _repository.ExistsAsync(notificaId);
                if (!exists)
                    return SafeNotFound(
                        _environment.IsDevelopment()
                            ? $"Notifica operativa con ID {notificaId} non trovata per l'aggiornamento"
                            : "Notifica operativa non trovata"
                    );

                await _repository.UpdateAsync(notificaDto);

                // ✅ Log per audit e sicurezza
                LogAuditTrail("UPDATE_NOTIFICA_OPERATIVA", "NotificheOperative", notificaId.ToString());
                LogSecurityEvent("NotificaOperativaUpdated", new
                {
                    NotificaId = notificaId,
                    Stato = notificaDto.Stato,
                    Priorita = notificaDto.Priorita,
                    User = User.Identity?.Name ?? "Unknown",
                    Timestamp = DateTime.UtcNow
                });

                return NoContent();
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore nell'aggiornamento notifica operativa {NotificaId}", notificaId);
                return SafeInternalError(
                    _environment.IsDevelopment()
                        ? $"Errore nell'aggiornamento notifica operativa {notificaId}: {ex.Message}"
                        : "Errore interno nell'aggiornamento notifica"
                );
            }
        }

        [HttpDelete("{notificaId}")]
        //[Authorize(Roles = "admin")] // ✅ COMMENTATO PER TEST
        public async Task<ActionResult> Delete(int notificaId)
        {
            try
            {
                if (notificaId <= 0)
                    return SafeBadRequest(
                        _environment.IsDevelopment()
                            ? "ID notifica non valido: deve essere maggiore di 0"
                            : "ID notifica non valido"
                    );

                // Verifica esistenza
                var exists = await _repository.ExistsAsync(notificaId);
                if (!exists)
                    return SafeNotFound(
                        _environment.IsDevelopment()
                            ? $"Notifica operativa con ID {notificaId} non trovata per l'eliminazione"
                            : "Notifica operativa non trovata"
                    );

                await _repository.DeleteAsync(notificaId);

                // ✅ Log per audit e sicurezza
                LogAuditTrail("DELETE_NOTIFICA_OPERATIVA", "NotificheOperative", notificaId.ToString());
                LogSecurityEvent("NotificaOperativaDeleted", new
                {
                    NotificaId = notificaId,
                    User = User.Identity?.Name ?? "Unknown",
                    Timestamp = DateTime.UtcNow
                });

                return NoContent();
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore nell'eliminazione notifica operativa {NotificaId}", notificaId);
                return SafeInternalError(
                    _environment.IsDevelopment()
                        ? $"Errore nell'eliminazione notifica operativa {notificaId}: {ex.Message}"
                        : "Errore interno nell'eliminazione notifica"
                );
            }
        }
    }
}