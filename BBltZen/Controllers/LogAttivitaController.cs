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
    public class LogAttivitaController : SecureBaseController
    {
        private readonly ILogAttivitaRepository _repository;
        private readonly BubbleTeaContext _context;

        public LogAttivitaController(
            ILogAttivitaRepository repository,
            BubbleTeaContext context,
            IWebHostEnvironment environment,
            ILogger<LogAttivitaController> logger)
            : base(environment, logger)
        {
            _repository = repository;
            _context = context;
        }

        [HttpGet]
        [AllowAnonymous] // ✅ AGGIUNTO ESPLICITAMENTE
        public async Task<ActionResult<IEnumerable<LogAttivitaDTO>>> GetAll()
        {
            try
            {
                var result = await _repository.GetAllAsync();

                LogAuditTrail("GET_ALL", "LogAttivita", "All");
                LogSecurityEvent("LogAttivitaGetAll", new
                {
                    UserId = GetCurrentUserId(),
                    Count = result.Count()
                });

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore nel recupero di tutti i log attività");
                return SafeInternalError<IEnumerable<LogAttivitaDTO>>("Errore durante il recupero dei log attività");
            }
        }

        [HttpGet("{logId}")]
        [AllowAnonymous] // ✅ AGGIUNTO ESPLICITAMENTE
        public async Task<ActionResult<LogAttivitaDTO>> GetById(int logId)
        {
            try
            {
                if (logId <= 0)
                    return SafeBadRequest<LogAttivitaDTO>(
                        _environment.IsDevelopment()
                            ? "ID log attività non valido: deve essere maggiore di 0"
                            : "ID log attività non valido"
                    );

                var result = await _repository.GetByIdAsync(logId);

                if (result == null)
                    return SafeNotFound<LogAttivitaDTO>(
                        _environment.IsDevelopment()
                            ? $"Log attività con ID {logId} non trovato"
                            : "Log attività non trovato"
                    );

                // ✅ Log per audit
                LogAuditTrail("GET_LOG_ATTIVITA_BY_ID", "LogAttivita", logId.ToString());

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore nel recupero log attività {LogId}", logId);
                return SafeInternalError<LogAttivitaDTO>(
                    _environment.IsDevelopment()
                        ? $"Errore nel recupero log attività {logId}: {ex.Message}"
                        : "Errore interno nel recupero log attività"
                );
            }
        }

        [HttpGet("tipo-attivita/{tipoAttivita}")]
        [AllowAnonymous] // ✅ AGGIUNTO ESPLICITAMENTE
        public async Task<ActionResult<IEnumerable<LogAttivitaDTO>>> GetByTipoAttivita(string tipoAttivita)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(tipoAttivita))
                    return SafeBadRequest<IEnumerable<LogAttivitaDTO>>("Tipo attività non valido");

                var result = await _repository.GetByTipoAttivitaAsync(tipoAttivita);

                LogAuditTrail("GET_BY_TIPO", "LogAttivita", tipoAttivita);
                LogSecurityEvent("LogAttivitaGetByTipo", new
                {
                    tipoAttivita,
                    UserId = GetCurrentUserId(),
                    Count = result.Count()
                });

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore nel recupero log attività per tipo {TipoAttivita}", tipoAttivita);
                return SafeInternalError<IEnumerable<LogAttivitaDTO>>("Errore durante il recupero dei log attività per tipo");
            }
        }

        [HttpGet("periodo")]
        [AllowAnonymous] // ✅ AGGIUNTO ESPLICITAMENTE
        public async Task<ActionResult<IEnumerable<LogAttivitaDTO>>> GetByPeriodo([FromQuery] DateTime dataInizio, [FromQuery] DateTime dataFine)
        {
            try
            {
                if (dataInizio > dataFine)
                    return SafeBadRequest<IEnumerable<LogAttivitaDTO>>(
                        _environment.IsDevelopment()
                            ? $"Data inizio ({dataInizio:yyyy-MM-dd}) non può essere successiva alla data fine ({dataFine:yyyy-MM-dd})"
                            : "Intervallo date non valido"
                    );

                var result = await _repository.GetByPeriodoAsync(dataInizio, dataFine);

                // ✅ Log per audit
                LogAuditTrail("GET_LOG_ATTIVITA_BY_PERIODO", "LogAttivita", $"{dataInizio:yyyy-MM-dd}_{dataFine:yyyy-MM-dd}");

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore nel recupero log attività per periodo {DataInizio} - {DataFine}", dataInizio, dataFine);
                return SafeInternalError<IEnumerable<LogAttivitaDTO>>(
                    _environment.IsDevelopment()
                        ? $"Errore nel recupero log attività per periodo {dataInizio:yyyy-MM-dd} - {dataFine:yyyy-MM-dd}: {ex.Message}"
                        : "Errore interno nel recupero log attività per periodo"
                );
            }
        }

        [HttpGet("statistiche/numero-attivita")]
        [AllowAnonymous] // ✅ AGGIUNTO ESPLICITAMENTE
        public async Task<ActionResult<int>> GetNumeroAttivita([FromQuery] DateTime? dataInizio = null, [FromQuery] DateTime? dataFine = null)
        {
            try
            {
                if (dataInizio.HasValue && dataFine.HasValue && dataInizio > dataFine)
                    return SafeBadRequest<int>(
                        _environment.IsDevelopment()
                            ? "Data inizio non può essere successiva alla data fine"
                            : "Intervallo date non valido"
                    );

                var result = await _repository.GetNumeroAttivitaAsync(dataInizio, dataFine);

                // ✅ Log per audit
                LogAuditTrail("GET_NUMERO_ATTIVITA_STATISTICHE", "LogAttivita",
                    $"Inizio: {dataInizio?.ToString("yyyy-MM-dd") ?? "null"}, Fine: {dataFine?.ToString("yyyy-MM-dd") ?? "null"}");

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore nel recupero statistiche numero attività");
                return SafeInternalError<int>(
                    _environment.IsDevelopment()
                        ? $"Errore nel recupero statistiche numero attività: {ex.Message}"
                        : "Errore interno nel recupero statistiche attività"
                );
            }
        }

        [HttpPost]
        //[Authorize(Roles = "admin,system")] // ✅ COMMENTATO PER TEST
        public async Task<ActionResult<LogAttivitaDTO>> Create([FromBody] LogAttivitaDTO logAttivitaDto)
        {
            try
            {
                if (!IsModelValid(logAttivitaDto))
                    return SafeBadRequest<LogAttivitaDTO>("Dati log attività non validi");

                // ✅ VERIFICA UTENTE (se specificato)
                if (logAttivitaDto.UtenteId.HasValue && !await _context.Utenti.AnyAsync(u => u.UtenteId == logAttivitaDto.UtenteId.Value))
                    return SafeBadRequest<LogAttivitaDTO>("Utente non trovato");

                // ✅ CORREZIONE: Usa il risultato del repository
                var result = await _repository.AddAsync(logAttivitaDto);

                // ✅ AUDIT OTTIMIZZATO
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

        [HttpPut("{logId}")]
        //[Authorize(Roles = "admin")] // ✅ COMMENTATO PER TEST
        public async Task<ActionResult> Update(int logId, [FromBody] LogAttivitaDTO logAttivitaDto)
        {
            try
            {
                if (logId <= 0 || logAttivitaDto.LogId != logId)
                    return SafeBadRequest("ID log attività non valido");

                if (!IsModelValid(logAttivitaDto))
                    return SafeBadRequest("Dati log attività non validi");

                if (!await _repository.ExistsAsync(logId))
                    return SafeNotFound("Log attività");

                // ✅ VERIFICA UTENTE (se specificato)
                if (logAttivitaDto.UtenteId.HasValue && !await _context.Utenti.AnyAsync(u => u.UtenteId == logAttivitaDto.UtenteId.Value))
                    return SafeBadRequest("Utente non trovato");

                await _repository.UpdateAsync(logAttivitaDto);

                // ✅ AUDIT OTTIMIZZATO
                LogAuditTrail("UPDATE", "LogAttivita", logId.ToString());
                LogSecurityEvent("LogAttivitaUpdated", new
                {
                    logId,
                    logAttivitaDto.TipoAttivita,
                    UserId = GetCurrentUserId(),
                    UserName = User.Identity?.Name
                });

                return NoContent();
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "Errore database durante l'aggiornamento del log attività {LogId}", logId);
                return SafeInternalError("Errore durante l'aggiornamento del log attività");
            }
            catch (ArgumentException argEx)
            {
                return SafeBadRequest(argEx.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore nell'aggiornamento log attività {LogId}", logId);
                return SafeInternalError("Errore durante l'aggiornamento del log attività");
            }
        }

        [HttpDelete("{logId}")]
        //[Authorize(Roles = "admin")] // ✅ COMMENTATO PER TEST
        public async Task<ActionResult> Delete(int logId)
        {
            try
            {
                if (logId <= 0)
                    return SafeBadRequest("ID log attività non valido");

                var existing = await _repository.GetByIdAsync(logId);
                if (existing == null)
                    return SafeNotFound("Log attività");

                await _repository.DeleteAsync(logId);

                // ✅ AUDIT OTTIMIZZATO
                LogAuditTrail("DELETE", "LogAttivita", logId.ToString());
                LogSecurityEvent("LogAttivitaDeleted", new
                {
                    logId,
                    existing.TipoAttivita,
                    UserId = GetCurrentUserId(),
                    UserName = User.Identity?.Name
                });

                return NoContent();
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "Errore database durante l'eliminazione del log attività {LogId}", logId);
                return SafeInternalError("Errore durante l'eliminazione del log attività");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore nell'eliminazione log attività {LogId}", logId);
                return SafeInternalError("Errore durante l'eliminazione del log attività");
            }
        }

        [HttpGet("utente/{utenteId}")]
        [AllowAnonymous] // ✅ AGGIUNTO ESPLICITAMENTE
        public async Task<ActionResult<IEnumerable<LogAttivitaDTO>>> GetByUtenteId(int utenteId)
        {
            try
            {
                if (utenteId <= 0)
                    return SafeBadRequest<IEnumerable<LogAttivitaDTO>>("ID utente non valido");

                if (!await _context.Utenti.AnyAsync(u => u.UtenteId == utenteId))
                    return SafeNotFound<IEnumerable<LogAttivitaDTO>>("Utente");

                var result = await _repository.GetByUtenteIdAsync(utenteId);

                LogAuditTrail("GET_BY_UTENTE", "LogAttivita", utenteId.ToString());
                LogSecurityEvent("LogAttivitaGetByUtente", new
                {
                    utenteId,
                    UserId = GetCurrentUserId(),
                    Count = result.Count()
                });

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore nel recupero log attività per utente {UtenteId}", utenteId);
                return SafeInternalError<IEnumerable<LogAttivitaDTO>>("Errore durante il recupero dei log attività per utente");
            }
        }

        [HttpGet("statistiche/tipi-attivita")]
        [AllowAnonymous] // ✅ AGGIUNTO ESPLICITAMENTE
        public async Task<ActionResult<Dictionary<string, int>>> GetStatisticheAttivita([FromQuery] DateTime? dataInizio = null, [FromQuery] DateTime? dataFine = null)
        {
            try
            {
                if (dataInizio.HasValue && dataFine.HasValue && dataInizio > dataFine)
                    return SafeBadRequest<Dictionary<string, int>>("Data inizio non può essere successiva alla data fine");

                var result = await _repository.GetStatisticheAttivitaAsync(dataInizio, dataFine);

                LogAuditTrail("GET_STATISTICHE_ATTIVITA", "LogAttivita",
                    $"Inizio: {dataInizio?.ToString("yyyy-MM-dd") ?? "null"}, Fine: {dataFine?.ToString("yyyy-MM-dd") ?? "null"}");

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore nel recupero statistiche attività");
                return SafeInternalError<Dictionary<string, int>>("Errore durante il recupero delle statistiche");
            }
        }
    }
}