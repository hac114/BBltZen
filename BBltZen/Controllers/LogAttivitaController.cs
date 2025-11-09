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

                // ✅ Log per audit
                LogAuditTrail("GET_ALL_LOG_ATTIVITA", "LogAttivita", "All");

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore nel recupero di tutti i log attività");
                return SafeInternalError<IEnumerable<LogAttivitaDTO>>(
                    _environment.IsDevelopment()
                        ? $"Errore nel recupero dei log attività: {ex.Message}"
                        : "Errore interno nel recupero dei log attività"
                );
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
                    return SafeBadRequest<IEnumerable<LogAttivitaDTO>>(
                        _environment.IsDevelopment()
                            ? "Tipo attività non valido: non può essere vuoto"
                            : "Tipo attività non valido"
                    );

                var result = await _repository.GetByTipoAttivitaAsync(tipoAttivita);

                // ✅ Log per audit
                LogAuditTrail("GET_LOG_ATTIVITA_BY_TIPO", "LogAttivita", tipoAttivita);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore nel recupero log attività per tipo {TipoAttivita}", tipoAttivita);
                return SafeInternalError<IEnumerable<LogAttivitaDTO>>(
                    _environment.IsDevelopment()
                        ? $"Errore nel recupero log attività per tipo {tipoAttivita}: {ex.Message}"
                        : "Errore interno nel recupero log attività per tipo"
                );
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
                // ✅ La validazione dei campi è gestita automaticamente dai Data Annotations del DTO
                if (!IsModelValid(logAttivitaDto))
                    return SafeBadRequest<LogAttivitaDTO>(
                        _environment.IsDevelopment()
                            ? "Dati log attività non validi: modello di binding fallito"
                            : "Dati log attività non validi"
                    );

                // ✅ Controlli avanzati con BubbleTeaContext
                var tipoAttivitaEsistente = await _context.LogAttivita
                    .AnyAsync(l => l.TipoAttivita == logAttivitaDto.TipoAttivita);

                if (!tipoAttivitaEsistente)
                {
                    _logger.LogWarning("Tentativo di creazione log con tipo attività non esistente: {TipoAttivita}", logAttivitaDto.TipoAttivita);
                }

                await _repository.AddAsync(logAttivitaDto);

                // ✅ Log per audit e sicurezza
                LogAuditTrail("CREATE_LOG_ATTIVITA", "LogAttivita", logAttivitaDto.LogId.ToString());
                LogSecurityEvent("LogAttivitaCreated", new
                {
                    LogId = logAttivitaDto.LogId,
                    TipoAttivita = logAttivitaDto.TipoAttivita,
                    Descrizione = logAttivitaDto.Descrizione,
                    User = User.Identity?.Name ?? "System",
                    Timestamp = DateTime.UtcNow,
                    IPAddress = HttpContext.Connection.RemoteIpAddress?.ToString()
                });

                return CreatedAtAction(nameof(GetById), new { logId = logAttivitaDto.LogId }, logAttivitaDto);
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "Errore database nella creazione log attività");
                return SafeInternalError<LogAttivitaDTO>(
                    _environment.IsDevelopment()
                        ? $"Errore database nella creazione log attività: {dbEx.InnerException?.Message ?? dbEx.Message}"
                        : "Errore di sistema nella creazione log attività"
                );
            }
            catch (ArgumentException argEx)
            {
                _logger.LogWarning(argEx, "Argomento non valido nella creazione log attività");
                return SafeBadRequest<LogAttivitaDTO>(
                    _environment.IsDevelopment()
                        ? $"Dati non validi: {argEx.Message}"
                        : "Dati non validi"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore nella creazione log attività");
                return SafeInternalError<LogAttivitaDTO>(
                    _environment.IsDevelopment()
                        ? $"Errore nella creazione log attività: {ex.Message}"
                        : "Errore interno nella creazione log attività"
                );
            }
        }

        [HttpPut("{logId}")]
        //[Authorize(Roles = "admin")] // ✅ COMMENTATO PER TEST
        public async Task<ActionResult> Update(int logId, [FromBody] LogAttivitaDTO logAttivitaDto)
        {
            try
            {
                if (logId <= 0)
                    return SafeBadRequest(
                        _environment.IsDevelopment()
                            ? "ID log attività non valido: deve essere maggiore di 0"
                            : "ID log attività non valido"
                    );

                // ✅ La validazione dei campi è gestita automaticamente dai Data Annotations del DTO
                if (!IsModelValid(logAttivitaDto))
                    return SafeBadRequest(
                        _environment.IsDevelopment()
                            ? "Dati log attività non validi: modello di binding fallito"
                            : "Dati log attività non validi"
                    );

                if (logAttivitaDto.LogId != logId)
                    return SafeBadRequest(
                        _environment.IsDevelopment()
                            ? $"ID log attività non corrispondente: URL={logId}, Body={logAttivitaDto.LogId}"
                            : "Identificativi non corrispondenti"
                    );

                // Verifica esistenza
                var exists = await _repository.ExistsAsync(logId);
                if (!exists)
                    return SafeNotFound(
                        _environment.IsDevelopment()
                            ? $"Log attività con ID {logId} non trovato per l'aggiornamento"
                            : "Log attività non trovato"
                    );

                await _repository.UpdateAsync(logAttivitaDto);

                // ✅ Log per audit e sicurezza
                LogAuditTrail("UPDATE_LOG_ATTIVITA", "LogAttivita", logId.ToString());
                LogSecurityEvent("LogAttivitaUpdated", new
                {
                    LogId = logId,
                    User = User.Identity?.Name ?? "Unknown",
                    Timestamp = DateTime.UtcNow,
                    IPAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                    Changes = $"Tipo: {logAttivitaDto.TipoAttivita}, Descrizione: {logAttivitaDto.Descrizione}"
                });

                return NoContent();
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "Errore database nell'aggiornamento log attività {LogId}", logId);
                return SafeInternalError(
                    _environment.IsDevelopment()
                        ? $"Errore database nell'aggiornamento log attività {logId}: {dbEx.InnerException?.Message ?? dbEx.Message}"
                        : "Errore di sistema nell'aggiornamento log attività"
                );
            }
            catch (ArgumentException argEx)
            {
                _logger.LogWarning(argEx, "Argomento non valido nell'aggiornamento log attività {LogId}", logId);
                return SafeBadRequest(
                    _environment.IsDevelopment()
                        ? $"Dati non validi: {argEx.Message}"
                        : "Dati non validi"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore nell'aggiornamento log attività {LogId}", logId);
                return SafeInternalError(
                    _environment.IsDevelopment()
                        ? $"Errore nell'aggiornamento log attività {logId}: {ex.Message}"
                        : "Errore interno nell'aggiornamento log attività"
                );
            }
        }

        [HttpDelete("{logId}")]
        //[Authorize(Roles = "admin")] // ✅ COMMENTATO PER TEST
        public async Task<ActionResult> Delete(int logId)
        {
            try
            {
                if (logId <= 0)
                    return SafeBadRequest(
                        _environment.IsDevelopment()
                            ? "ID log attività non valido: deve essere maggiore di 0"
                            : "ID log attività non valido"
                    );

                // Verifica esistenza
                var exists = await _repository.ExistsAsync(logId);
                if (!exists)
                    return SafeNotFound(
                        _environment.IsDevelopment()
                            ? $"Log attività con ID {logId} non trovato per l'eliminazione"
                            : "Log attività non trovato"
                    );

                await _repository.DeleteAsync(logId);

                // ✅ Log per audit e sicurezza
                LogAuditTrail("DELETE_LOG_ATTIVITA", "LogAttivita", logId.ToString());
                LogSecurityEvent("LogAttivitaDeleted", new
                {
                    LogId = logId,
                    User = User.Identity?.Name ?? "Unknown",
                    Timestamp = DateTime.UtcNow,
                    IPAddress = HttpContext.Connection.RemoteIpAddress?.ToString()
                });

                return NoContent();
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "Errore database nell'eliminazione log attività {LogId}", logId);
                return SafeInternalError(
                    _environment.IsDevelopment()
                        ? $"Errore database nell'eliminazione log attività {logId}: {dbEx.InnerException?.Message ?? dbEx.Message}"
                        : "Errore di sistema nell'eliminazione log attività"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore nell'eliminazione log attività {LogId}", logId);
                return SafeInternalError(
                    _environment.IsDevelopment()
                        ? $"Errore nell'eliminazione log attività {logId}: {ex.Message}"
                        : "Errore interno nell'eliminazione log attività"
                );
            }
        }
    }
}