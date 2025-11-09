using Microsoft.AspNetCore.Mvc;
using DTO;
using Repository.Interface;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;
using Database;
using Microsoft.EntityFrameworkCore;

namespace BBltZen.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    //[Authorize] // ✅ COMMENTATO PER TEST CON SWAGGER
    public class NotificheOperativeController : SecureBaseController
    {
        private readonly INotificheOperativeRepository _repository;
        private readonly BubbleTeaContext _context;

        public NotificheOperativeController(
            INotificheOperativeRepository repository,
            BubbleTeaContext context,
            IWebHostEnvironment environment,
            ILogger<NotificheOperativeController> logger)
            : base(environment, logger)
        {
            _repository = repository;
            _context = context;
        }

        [HttpGet]
        [AllowAnonymous] // ✅ AGGIUNTO ESPLICITAMENTE
        public async Task<ActionResult<IEnumerable<NotificheOperativeDTO>>> GetAll()
        {
            try
            {
                var result = await _repository.GetAllAsync();
                LogAuditTrail("GET_ALL_NOTIFICHE_OPERATIVE", "NotificheOperative", "All");
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore nel recupero di tutte le notifiche operative");
                return SafeInternalError<IEnumerable<NotificheOperativeDTO>>(
                    _environment.IsDevelopment()
                        ? $"Errore nel recupero delle notifiche operative: {ex.Message}"
                        : "Errore interno nel recupero delle notifiche"
                );
            }
        }

        [HttpGet("{notificaId}")]
        [AllowAnonymous] // ✅ AGGIUNTO ESPLICITAMENTE
        public async Task<ActionResult<NotificheOperativeDTO>> GetById(int notificaId)
        {
            try
            {
                if (notificaId <= 0)
                    return SafeBadRequest<NotificheOperativeDTO>("ID notifica non valido");

                var result = await _repository.GetByIdAsync(notificaId);

                if (result == null)
                    return SafeNotFound<NotificheOperativeDTO>("Notifica operativa non trovata");

                LogAuditTrail("GET_NOTIFICA_OPERATIVA_BY_ID", "NotificheOperative", notificaId.ToString());
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore nel recupero notifica operativa {NotificaId}", notificaId);
                return SafeInternalError<NotificheOperativeDTO>(
                    _environment.IsDevelopment()
                        ? $"Errore nel recupero notifica operativa {notificaId}: {ex.Message}"
                        : "Errore interno nel recupero notifica"
                );
            }
        }

        [HttpGet("stato/{stato}")]
        [AllowAnonymous] // ✅ AGGIUNTO ESPLICITAMENTE
        public async Task<ActionResult<IEnumerable<NotificheOperativeDTO>>> GetByStato(string stato)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(stato))
                    return SafeBadRequest<IEnumerable<NotificheOperativeDTO>>("Stato notifica non valido");

                var result = await _repository.GetByStatoAsync(stato);
                LogAuditTrail("GET_NOTIFICHE_BY_STATO", "NotificheOperative", stato);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore nel recupero notifiche per stato {Stato}", stato);
                return SafeInternalError<IEnumerable<NotificheOperativeDTO>>(
                    _environment.IsDevelopment()
                        ? $"Errore nel recupero notifiche per stato {stato}: {ex.Message}"
                        : "Errore interno nel recupero notifiche per stato"
                );
            }
        }

        [HttpGet("priorita/{priorita}")]
        [AllowAnonymous] // ✅ AGGIUNTO ESPLICITAMENTE
        public async Task<ActionResult<IEnumerable<NotificheOperativeDTO>>> GetByPriorita(int priorita)
        {
            try
            {
                if (priorita <= 0 || priorita > 10)
                    return SafeBadRequest<IEnumerable<NotificheOperativeDTO>>("Priorità non valida");

                var result = await _repository.GetByPrioritaAsync(priorita);
                LogAuditTrail("GET_NOTIFICHE_BY_PRIORITA", "NotificheOperative", priorita.ToString());
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore nel recupero notifiche per priorità {Priorita}", priorita);
                return SafeInternalError<IEnumerable<NotificheOperativeDTO>>(
                    _environment.IsDevelopment()
                        ? $"Errore nel recupero notifiche per priorità {priorita}: {ex.Message}"
                        : "Errore interno nel recupero notifiche per priorità"
                );
            }
        }

        [HttpGet("pendenti")]
        [AllowAnonymous] // ✅ AGGIUNTO ESPLICITAMENTE
        public async Task<ActionResult<IEnumerable<NotificheOperativeDTO>>> GetPendenti()
        {
            try
            {
                var result = await _repository.GetPendentiAsync();
                LogAuditTrail("GET_NOTIFICHE_PENDENTI", "NotificheOperative", $"Count: {result?.Count()}");
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore nel recupero notifiche pendenti");
                return SafeInternalError<IEnumerable<NotificheOperativeDTO>>(
                    _environment.IsDevelopment()
                        ? $"Errore nel recupero notifiche pendenti: {ex.Message}"
                        : "Errore interno nel recupero notifiche pendenti"
                );
            }
        }

        [HttpGet("periodo")]
        [AllowAnonymous] // ✅ AGGIUNTO ESPLICITAMENTE
        public async Task<ActionResult<IEnumerable<NotificheOperativeDTO>>> GetByPeriodo([FromQuery] DateTime dataInizio, [FromQuery] DateTime dataFine)
        {
            try
            {
                if (dataInizio > dataFine)
                    return SafeBadRequest<IEnumerable<NotificheOperativeDTO>>("Intervallo date non valido");

                var result = await _repository.GetByPeriodoAsync(dataInizio, dataFine);
                LogAuditTrail("GET_NOTIFICHE_BY_PERIODO", "NotificheOperative", $"{dataInizio:yyyy-MM-dd}_{dataFine:yyyy-MM-dd}");
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore nel recupero notifiche per periodo {DataInizio} - {DataFine}", dataInizio, dataFine);
                return SafeInternalError<IEnumerable<NotificheOperativeDTO>>(
                    _environment.IsDevelopment()
                        ? $"Errore nel recupero notifiche per periodo {dataInizio:yyyy-MM-dd} - {dataFine:yyyy-MM-dd}: {ex.Message}"
                        : "Errore interno nel recupero notifiche per periodo"
                );
            }
        }

        [HttpGet("pendenti/numero")]
        [AllowAnonymous] // ✅ AGGIUNTO ESPLICITAMENTE
        public async Task<ActionResult<int>> GetNumeroPendenti()
        {
            try
            {
                var result = await _repository.GetNumeroNotifichePendentiAsync();
                LogAuditTrail("GET_NUMERO_NOTIFICHE_PENDENTI", "NotificheOperative", result.ToString());
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore nel recupero numero notifiche pendenti");
                return SafeInternalError<int>(
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
                // ✅ SOLO CONTROLLO GENERICO - LE VALIDAZIONI SPECIFICHE SONO NEL DTO
                if (!IsModelValid(notificaDto))
                    return SafeBadRequest<NotificheOperativeDTO>("Dati notifica non validi");

                await _repository.AddAsync(notificaDto);

                LogAuditTrail("CREATE_NOTIFICA_OPERATIVA", "NotificheOperative", notificaDto.NotificaId.ToString());
                LogSecurityEvent("NotificaOperativaCreated", new
                {
                    NotificaId = notificaDto.NotificaId,
                    Priorita = notificaDto.Priorita,
                    Stato = notificaDto.Stato,
                    User = User.Identity?.Name ?? "Unknown",
                    Timestamp = DateTime.UtcNow,
                    IPAddress = HttpContext.Connection.RemoteIpAddress?.ToString()
                });

                return CreatedAtAction(nameof(GetById), new { notificaId = notificaDto.NotificaId }, notificaDto);
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "Errore database nella creazione notifica operativa");
                return SafeInternalError<NotificheOperativeDTO>(
                    _environment.IsDevelopment()
                        ? $"Errore database nella creazione notifica operativa: {dbEx.InnerException?.Message ?? dbEx.Message}"
                        : "Errore di sistema nella creazione notifica"
                );
            }
            catch (ArgumentException argEx)
            {
                _logger.LogWarning(argEx, "Argomento non valido nella creazione notifica operativa");
                return SafeBadRequest<NotificheOperativeDTO>(
                    _environment.IsDevelopment()
                        ? $"Dati non validi: {argEx.Message}"
                        : "Dati non validi"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore nella creazione notifica operativa");
                return SafeInternalError<NotificheOperativeDTO>(
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
                    return SafeBadRequest("ID notifica non valido");

                // ✅ SOLO CONTROLLO GENERICO - LE VALIDAZIONI SPECIFICHE SONO NEL DTO
                if (!IsModelValid(notificaDto))
                    return SafeBadRequest("Dati notifica non validi");

                if (notificaDto.NotificaId != notificaId)
                    return SafeBadRequest("Identificativi non corrispondenti");

                var exists = await _repository.ExistsAsync(notificaId);
                if (!exists)
                    return SafeNotFound("Notifica operativa non trovata");

                await _repository.UpdateAsync(notificaDto);

                LogAuditTrail("UPDATE_NOTIFICA_OPERATIVA", "NotificheOperative", notificaId.ToString());
                LogSecurityEvent("NotificaOperativaUpdated", new
                {
                    NotificaId = notificaId,
                    Stato = notificaDto.Stato,
                    Priorita = notificaDto.Priorita,
                    User = User.Identity?.Name ?? "Unknown",
                    Timestamp = DateTime.UtcNow,
                    IPAddress = HttpContext.Connection.RemoteIpAddress?.ToString()
                });

                return NoContent();
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "Errore database nell'aggiornamento notifica operativa {NotificaId}", notificaId);
                return SafeInternalError(
                    _environment.IsDevelopment()
                        ? $"Errore database nell'aggiornamento notifica operativa {notificaId}: {dbEx.InnerException?.Message ?? dbEx.Message}"
                        : "Errore di sistema nell'aggiornamento notifica"
                );
            }
            catch (ArgumentException argEx)
            {
                _logger.LogWarning(argEx, "Argomento non valido nell'aggiornamento notifica operativa {NotificaId}", notificaId);
                return SafeBadRequest(
                    _environment.IsDevelopment()
                        ? $"Dati non validi: {argEx.Message}"
                        : "Dati non validi"
                );
            }
            catch (Exception ex)
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
                    return SafeBadRequest("ID notifica non valido");

                var exists = await _repository.ExistsAsync(notificaId);
                if (!exists)
                    return SafeNotFound("Notifica operativa non trovata");

                await _repository.DeleteAsync(notificaId);

                LogAuditTrail("DELETE_NOTIFICA_OPERATIVA", "NotificheOperative", notificaId.ToString());
                LogSecurityEvent("NotificaOperativaDeleted", new
                {
                    NotificaId = notificaId,
                    User = User.Identity?.Name ?? "Unknown",
                    Timestamp = DateTime.UtcNow,
                    IPAddress = HttpContext.Connection.RemoteIpAddress?.ToString()
                });

                return NoContent();
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "Errore database nell'eliminazione notifica operativa {NotificaId}", notificaId);
                return SafeInternalError(
                    _environment.IsDevelopment()
                        ? $"Errore database nell'eliminazione notifica operativa {notificaId}: {dbEx.InnerException?.Message ?? dbEx.Message}"
                        : "Errore di sistema nell'eliminazione notifica"
                );
            }
            catch (Exception ex)
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