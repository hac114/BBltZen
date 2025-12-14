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
using BBltZen;

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

                LogAuditTrail("GET_ALL", "NotificheOperative", "All");
                LogSecurityEvent("NotificaOperativaGetAll", new
                {
                    UserId = GetCurrentUserId(),
                    Count = result.Count()
                });

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore nel recupero di tutte le notifiche operative");
                return SafeInternalError<IEnumerable<NotificheOperativeDTO>>("Errore durante il recupero delle notifiche");
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

                LogAuditTrail("GET_PENDENTI", "NotificheOperative", $"Count: {result.Count()}");
                LogSecurityEvent("NotificaOperativaGetPendenti", new
                {
                    UserId = GetCurrentUserId(),
                    Count = result.Count()
                });

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore nel recupero notifiche pendenti");
                return SafeInternalError<IEnumerable<NotificheOperativeDTO>>("Errore durante il recupero delle notifiche pendenti");
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
                if (!IsModelValid(notificaDto))
                    return SafeBadRequest<NotificheOperativeDTO>("Dati notifica non validi");

                // ✅ CORREZIONE: Usa il risultato del repository
                var result = await _repository.AddAsync(notificaDto);

                // ✅ AUDIT OTTIMIZZATO
                LogAuditTrail("CREATE", "NotificheOperative", result.NotificaId.ToString());
                LogSecurityEvent("NotificaOperativaCreated", new
                {
                    result.NotificaId,
                    result.Priorita,
                    result.Stato,
                    UserId = GetCurrentUserId(),
                    UserName = User.Identity?.Name
                });

                return CreatedAtAction(nameof(GetById), new { notificaId = result.NotificaId }, result);
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "Errore database durante la creazione della notifica");
                return SafeInternalError<NotificheOperativeDTO>("Errore durante il salvataggio della notifica");
            }
            catch (ArgumentException argEx)
            {
                return SafeBadRequest<NotificheOperativeDTO>(argEx.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore nella creazione notifica operativa");
                return SafeInternalError<NotificheOperativeDTO>("Errore durante la creazione della notifica");
            }
        }

        [HttpPut("{notificaId}")]
        //[Authorize(Roles = "admin,manager")] // ✅ COMMENTATO PER TEST
        public async Task<ActionResult> Update(int notificaId, [FromBody] NotificheOperativeDTO notificaDto)
        {
            try
            {
                if (notificaId <= 0 || notificaDto.NotificaId != notificaId)
                    return SafeBadRequest("ID notifica non valido");

                if (!IsModelValid(notificaDto))
                    return SafeBadRequest("Dati notifica non validi");

                if (!await _repository.ExistsAsync(notificaId))
                    return SafeNotFound("Notifica operativa");

                await _repository.UpdateAsync(notificaDto);

                // ✅ AUDIT OTTIMIZZATO
                LogAuditTrail("UPDATE", "NotificheOperative", notificaId.ToString());
                LogSecurityEvent("NotificaOperativaUpdated", new
                {
                    notificaId,
                    notificaDto.Stato,
                    notificaDto.Priorita,
                    UserId = GetCurrentUserId(),
                    UserName = User.Identity?.Name
                });

                return NoContent();
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "Errore database durante l'aggiornamento della notifica {NotificaId}", notificaId);
                return SafeInternalError("Errore durante l'aggiornamento della notifica");
            }
            catch (ArgumentException argEx)
            {
                return SafeBadRequest(argEx.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore nell'aggiornamento notifica {NotificaId}", notificaId);
                return SafeInternalError("Errore durante l'aggiornamento della notifica");
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

                var existing = await _repository.GetByIdAsync(notificaId);
                if (existing == null)
                    return SafeNotFound("Notifica operativa");

                await _repository.DeleteAsync(notificaId);

                // ✅ AUDIT OTTIMIZZATO
                LogAuditTrail("DELETE", "NotificheOperative", notificaId.ToString());
                LogSecurityEvent("NotificaOperativaDeleted", new
                {
                    notificaId,
                    existing.Stato,
                    UserId = GetCurrentUserId(),
                    UserName = User.Identity?.Name
                });

                return NoContent();
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "Errore database durante l'eliminazione della notifica {NotificaId}", notificaId);
                return SafeInternalError("Errore durante l'eliminazione della notifica");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore nell'eliminazione notifica {NotificaId}", notificaId);
                return SafeInternalError("Errore durante l'eliminazione della notifica");
            }
        }

        [HttpGet("tipo/{tipoNotifica}")]
        [AllowAnonymous] // ✅ AGGIUNTO ESPLICITAMENTE
        public async Task<ActionResult<IEnumerable<NotificheOperativeDTO>>> GetByTipoNotifica(string tipoNotifica)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(tipoNotifica))
                    return SafeBadRequest<IEnumerable<NotificheOperativeDTO>>("Tipo notifica non valido");

                var result = await _repository.GetByTipoNotificaAsync(tipoNotifica);

                LogAuditTrail("GET_BY_TIPO", "NotificheOperative", tipoNotifica);
                LogSecurityEvent("NotificaOperativaGetByTipo", new
                {
                    tipoNotifica,
                    UserId = GetCurrentUserId(),
                    Count = result.Count()
                });

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore nel recupero notifiche per tipo {TipoNotifica}", tipoNotifica);
                return SafeInternalError<IEnumerable<NotificheOperativeDTO>>("Errore durante il recupero delle notifiche per tipo");
            }
        }

        [HttpGet("statistiche")]
        [AllowAnonymous] // ✅ AGGIUNTO ESPLICITAMENTE
        public async Task<ActionResult<Dictionary<string, int>>> GetStatisticheNotifiche()
        {
            try
            {
                var result = await _repository.GetStatisticheNotificheAsync();

                LogAuditTrail("GET_STATISTICHE", "NotificheOperative", "All");
                LogSecurityEvent("NotificaOperativaGetStatistics", new
                {
                    UserId = GetCurrentUserId(),
                    Statistiche = result
                });

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore nel recupero statistiche notifiche");
                return SafeInternalError<Dictionary<string, int>>("Errore durante il recupero delle statistiche");
            }
        }

        [HttpGet("stato/{stato}/numero")]
        [AllowAnonymous] // ✅ AGGIUNTO ESPLICITAMENTE
        public async Task<ActionResult<int>> GetNumeroNotificheByStato(string stato)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(stato))
                    return SafeBadRequest<int>("Stato notifica non valido");

                var result = await _repository.GetNumeroNotificheByStatoAsync(stato);

                LogAuditTrail("GET_NUMERO_BY_STATO", "NotificheOperative", stato);
                LogSecurityEvent("NotificaOperativaGetCountByStato", new
                {
                    stato,
                    UserId = GetCurrentUserId(),
                    Count = result
                });

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore nel recupero numero notifiche per stato {Stato}", stato);
                return SafeInternalError<int>("Errore durante il recupero del numero notifiche per stato");
            }
        }
    }
}