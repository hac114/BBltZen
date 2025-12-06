using Microsoft.AspNetCore.Mvc;
using DTO;
using Repository.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
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
    public class LogAttivitaController(
        ILogAttivitaRepository repository,
        BubbleTeaContext context,
        IWebHostEnvironment environment,
        ILogger<LogAttivitaController> logger)
        : SecureBaseController(environment, logger)
    {
        private readonly ILogAttivitaRepository _repository = repository;
        private readonly BubbleTeaContext _context = context;

        // GET /api/LogAttivita?id=...
        [HttpGet("id")]
        [AllowAnonymous]
        public async Task<ActionResult<object>> GetById([FromQuery] int? id, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var result = await _repository.GetByIdAsync(id, page, pageSize);

                if (id.HasValue && !result.Data.Any())
                    return SafeNotFound<object>("Log attività");

                return Ok(new
                {
                    result.Message,
                    Pagination = new
                    {
                        result.Page,
                        result.PageSize,
                        result.TotalCount,
                        result.TotalPages,
                        result.HasPrevious,
                        result.HasNext
                    },
                    result.Data
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore nel recupero log {Id}", id);
                return SafeInternalError<object>("Errore durante il recupero log");
            }
        }


        // GET /api/LogAttivita/tipo-attivita?tipoAttivita=...
        [HttpGet("tipo-attivita")]
        [AllowAnonymous]
        public async Task<ActionResult<object>> GetByTipoAttivita([FromQuery] string? tipoAttivita, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var result = await _repository.GetByTipoAttivitaAsync(tipoAttivita, page, pageSize);

                return Ok(new
                {
                    result.Message,
                    SearchTerm = tipoAttivita,
                    Pagination = new
                    {
                        result.Page,
                        result.PageSize,
                        result.TotalCount,
                        result.TotalPages,
                        result.HasPrevious,
                        result.HasNext
                    },
                    result.Data
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore nel recupero log per tipo attivita {TipoAttivita}", tipoAttivita);
                return SafeInternalError<object>("Errore durante il recupero log per tipo attivita");
            }
        }

        // GET /api/LogAttivita/frontend/tipo-attivita?tipoAttivita=...
        [HttpGet("frontend/tipo-attivita")]
        [AllowAnonymous]
        public async Task<ActionResult<object>> GetByTipoAttivitaPerFrontend([FromQuery] string? tipoAttivita, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var result = await _repository.GetByTipoAttivitaPerFrontendAsync(tipoAttivita, page, pageSize);

                return Ok(new
                {
                    result.Message,
                    SearchTerm = tipoAttivita,
                    Pagination = new
                    {
                        result.Page,
                        result.PageSize,
                        result.TotalCount,
                        result.TotalPages,
                        result.HasPrevious,
                        result.HasNext
                    },
                    result.Data
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore nel recupero log frontend per tipo attivita {TipoAttivita}", tipoAttivita);
                return SafeInternalError<object>("Errore durante il recupero log frontend per tipo attivita");
            }
        }

        // GET /api/LogAttivita/frontend/tipo-utente?tipoUtente=...
        [HttpGet("frontend/tipo-utente")]
        [AllowAnonymous]
        public async Task<ActionResult<object>> GetByTipoUtente([FromQuery] string? tipoUtente, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var normalized = string.IsNullOrWhiteSpace(tipoUtente) ? null : tipoUtente.Trim().ToUpper();
                var result = await _repository.GetByTipoUtenteAsync(normalized, page, pageSize);

                return Ok(new
                {
                    result.Message,
                    SearchTerm = tipoUtente,
                    Pagination = new
                    {
                        result.Page,
                        result.PageSize,
                        result.TotalCount,
                        result.TotalPages,
                        result.HasPrevious,
                        result.HasNext
                    },
                    result.Data
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore nel recupero log frontend per tipo utente {TipoUtente}", tipoUtente);
                return SafeInternalError<object>("Errore durante il recupero log frontend per tipo utente");
            }
        }

        [HttpGet("statistiche/numero-attivita")]
        [AllowAnonymous]
        public async Task<ActionResult<object>> GetNumeroAttivita([FromQuery] DateTime? dataInizio = null, [FromQuery] DateTime? dataFine = null)
        {
            try
            {
                dataInizio ??= DateTime.Today.AddDays(-30);
                dataFine ??= DateTime.Now;

                if (dataInizio > dataFine)
                    return SafeBadRequest<object>("Intervallo date non valido");

                var result = await _repository.GetNumeroAttivitaAsync(dataInizio, dataFine);

                return Ok(new
                {
                    Message = $"Numero attività: {result} dal {dataInizio:dd/MM/yyyy} al {dataFine:dd/MM/yyyy}",
                    Periodo = new
                    {
                        Da = dataInizio.Value.ToString("dd/MM/yyyy HH:mm"),
                        A = dataFine.Value.ToString("dd/MM/yyyy HH:mm")
                    },
                    Count = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore nel recupero statistiche numero attività");
                return SafeInternalError<object>("Errore interno nel recupero statistiche attività");
            }
        }

        [HttpPost]
        public async Task<ActionResult<LogAttivitaDTO>> Create([FromBody] LogAttivitaDTO logAttivitaDto)
        {
            try
            {
                if (!IsModelValid(logAttivitaDto))
                    return SafeBadRequest<LogAttivitaDTO>("Dati log attività non validi");

                if (logAttivitaDto.UtenteId.HasValue && !await _context.Utenti.AnyAsync(u => u.UtenteId == logAttivitaDto.UtenteId.Value))
                    return SafeBadRequest<LogAttivitaDTO>("Utente non trovato");

                var result = await _repository.AddAsync(logAttivitaDto);

                LogAuditTrail("CREATE", "LogAttivita", result.LogId.ToString());
                LogSecurityEvent("LogAttivitaCreated", new
                {
                    result.LogId,
                    result.TipoAttivita,
                    result.UtenteId,
                    UserId = GetCurrentUserId(),
                    UserName = User.Identity?.Name
                });

                return CreatedAtAction(nameof(GetById), new { logId = result.LogId }, result);
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "Errore database durante la creazione del log attività");
                return SafeInternalError<LogAttivitaDTO>("Errore durante il salvataggio del log attività");
            }
            catch (ArgumentException argEx)
            {
                return SafeBadRequest<LogAttivitaDTO>(argEx.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore nella creazione log attività");
                return SafeInternalError<LogAttivitaDTO>("Errore durante la creazione del log attività");
            }
        }

        [HttpGet("utente")]
        [AllowAnonymous]
        public async Task<ActionResult<object>> GetByUtenteId(
            [FromQuery] int? utenteId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                var result = await _repository.GetByUtenteIdAsync(utenteId, page, pageSize);

                if (utenteId.HasValue && (result.Data == null || !result.Data.Any()))
                    return SafeNotFound<object>("Utente");

                return Ok(new
                {
                    result.Message,
                    Pagination = new
                    {
                        result.Page,
                        result.PageSize,
                        result.TotalCount,
                        result.TotalPages,
                        result.HasPrevious,
                        result.HasNext
                    },
                    result.Data
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore nel recupero attività per utente {UtenteId}", utenteId);
                return SafeInternalError<object>("Errore durante il recupero dei log attività");
            }
        }

        [HttpGet("statistiche/tipi-attivita")]
        [AllowAnonymous]
        public async Task<ActionResult<object>> GetStatisticheAttivita([FromQuery] DateTime? dataInizio = null, [FromQuery] DateTime? dataFine = null)
        {
            try
            {
                dataInizio ??= DateTime.Today.AddDays(-30);
                dataFine ??= DateTime.Now;

                if (dataInizio > dataFine)
                    return SafeBadRequest<object>("Intervallo date non valido");

                var result = await _repository.GetStatisticheAttivitaAsync(dataInizio, dataFine);

                return Ok(new
                {
                    Message = $"Statistiche attività dal {dataInizio:dd/MM/yyyy} al {dataFine:dd/MM/yyyy}",
                    Periodo = new
                    {
                        Da = dataInizio.Value.ToString("dd/MM/yyyy HH:mm"),
                        A = dataFine.Value.ToString("dd/MM/yyyy HH:mm")
                    },
                    Statistiche = result,
                    Totale = result.Values.Sum()
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore nel recupero statistiche attività");
                return SafeInternalError<object>("Errore durante il recupero delle statistiche");
            }
        }

        [HttpGet("frontend/periodo")]
        [AllowAnonymous]
        public async Task<ActionResult<object>> GetByPeriodoPerFrontend(
            [FromQuery] DateTime? dataInizio = null,
            [FromQuery] DateTime? dataFine = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                DateTime da = dataInizio ?? DateTime.Today.AddDays(-7);
                DateTime a = dataFine ?? DateTime.Now;

                if (da > a)
                    return SafeBadRequest<object>("Intervallo date non valido");

                var result = await _repository.GetByPeriodoPerFrontendAsync(da, a, page, pageSize);

                return Ok(new
                {
                    result.Message,
                    Periodo = new { Da = da, A = a },
                    Pagination = new
                    {
                        result.Page,
                        result.PageSize,
                        result.TotalCount,
                        result.TotalPages,
                        result.HasPrevious,
                        result.HasNext
                    },
                    result.Data
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore nel recupero log per periodo {Da} - {A}", dataInizio, dataFine);
                return SafeInternalError<object>("Errore durante il recupero dei log attività");
            }
        }

        [HttpPost("cleanup")]
        public async Task<ActionResult<object>> CleanupOldLogs([FromQuery] int? giorniRitenzione = null)
        {
            try
            {
                int retention = giorniRitenzione ?? -1;

                if (retention < -1)
                    return SafeBadRequest<object>("Giorni ritenzione non validi");

                var deletedCount = await _repository.CleanupOldLogsAsync(retention);

                string message = retention == -1
                    ? $"Puliti tutti i log attività: {deletedCount} record eliminati"
                    : $"Puliti {deletedCount} log attività vecchi di {retention} giorni";

                LogAuditTrail("CLEANUP_LOGS", "LogAttivita",
                    $"Deleted: {deletedCount}, Retention: {(retention == -1 ? "ALL" : retention.ToString())} giorni");

                return Ok(new
                {
                    Message = message,
                    LogsEliminati = deletedCount,
                    GiorniRitenzione = retention == -1 ? "TUTTI" : retention.ToString(),
                    Timestamp = DateTime.Now
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante il cleanup dei log attività");
                return SafeInternalError<object>("Errore durante la pulizia dei log");
            }
        }
    }
}
