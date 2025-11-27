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
    public class LogAttivitaController(
    ILogAttivitaRepository repository,
    BubbleTeaContext context,
    IWebHostEnvironment environment,
    ILogger<LogAttivitaController> logger)
    : SecureBaseController(environment, logger)
    {
        private readonly ILogAttivitaRepository _repository = repository;
        private readonly BubbleTeaContext _context = context;

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
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<LogAttivitaDTO>>> GetByPeriodo([FromQuery] DateTime? dataInizio = null, [FromQuery] DateTime? dataFine = null)
        {
            try
            {
                // ✅ VALORI DI DEFAULT
                dataInizio ??= DateTime.Today.AddDays(-7); // Ultimi 7 giorni
                dataFine ??= DateTime.Now; // Fino ad ora

                if (dataInizio > dataFine)
                    return SafeBadRequest<IEnumerable<LogAttivitaDTO>>("Intervallo date non valido");

                var result = await _repository.GetByPeriodoAsync(dataInizio.Value, dataFine.Value);

                // ✅ MESSAGGIO CON DATE USATE
                return Ok(new
                {
                    Message = $"Trovate {result.Count()} attività dal {dataInizio:dd/MM/yyyy} al {dataFine:dd/MM/yyyy}",
                    Periodo = new { Da = dataInizio, A = dataFine },
                    Count = result.Count(),
                    Data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore nel recupero log attività per periodo {DataInizio} - {DataFine}", dataInizio, dataFine);
                return SafeInternalError<IEnumerable<LogAttivitaDTO>>("Errore interno nel recupero log attività per periodo");
            }
        }

        [HttpGet("statistiche/numero-attivita")]
        [AllowAnonymous]
        public async Task<ActionResult<int>> GetNumeroAttivita([FromQuery] DateTime? dataInizio = null, [FromQuery] DateTime? dataFine = null)
        {
            try
            {
                // ✅ VALORI DI DEFAULT (30 giorni)
                dataInizio ??= DateTime.Today.AddDays(-30);
                dataFine ??= DateTime.Now;

                if (dataInizio > dataFine)
                    return SafeBadRequest<int>("Intervallo date non valido");

                var result = await _repository.GetNumeroAttivitaAsync(dataInizio, dataFine);

                return Ok(new
                {
                    Message = $"Numero attività: {result} dal {dataInizio:dd/MM/yyyy} al {dataFine:dd/MM/yyyy}",
                    Periodo = new { Da = dataInizio, A = dataFine },
                    Count = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore nel recupero statistiche numero attività");
                return SafeInternalError<int>("Errore interno nel recupero statistiche attività");
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

        [HttpGet("utente/{utenteId}")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<LogAttivitaDTO>>> GetByUtenteId(int utenteId)
        {
            try
            {
                if (utenteId <= 0)
                    return SafeBadRequest<IEnumerable<LogAttivitaDTO>>("ID utente non valido");

                if (!await _context.Utenti.AnyAsync(u => u.UtenteId == utenteId))
                    return SafeNotFound<IEnumerable<LogAttivitaDTO>>("Utente");

                var result = await _repository.GetByUtenteIdAsync(utenteId);

                // ✅ MESSAGGIO PER RISULTATI VUOTI
                if (!result.Any())
                    return Ok(new
                    {
                        Message = $"Nessuna attività trovata per l'utente {utenteId}",
                        Count = 0,
                        Data = Array.Empty<LogAttivitaDTO>()
                    });

                LogAuditTrail("GET_BY_UTENTE", "LogAttivita", utenteId.ToString());

                return Ok(new
                {
                    Message = $"Trovate {result.Count()} attività per l'utente {utenteId}",
                    Count = result.Count(),
                    Data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore nel recupero log attività per utente {UtenteId}", utenteId);
                return SafeInternalError<IEnumerable<LogAttivitaDTO>>("Errore durante il recupero dei log attività per utente");
            }
        }

        [HttpGet("statistiche/tipi-attivita")]
        [AllowAnonymous]
        public async Task<ActionResult<Dictionary<string, int>>> GetStatisticheAttivita([FromQuery] DateTime? dataInizio = null, [FromQuery] DateTime? dataFine = null)
        {
            try
            {
                // ✅ VALORI DI DEFAULT (30 giorni)
                dataInizio ??= DateTime.Today.AddDays(-30);
                dataFine ??= DateTime.Now;

                if (dataInizio > dataFine)
                    return SafeBadRequest<Dictionary<string, int>>("Intervallo date non valido");

                var result = await _repository.GetStatisticheAttivitaAsync(dataInizio, dataFine);

                return Ok(new
                {
                    Message = $"Statistiche attività dal {dataInizio:dd/MM/yyyy} al {dataFine:dd/MM/yyyy}",
                    Periodo = new { Da = dataInizio, A = dataFine },
                    Statistiche = result,
                    Totale = result.Values.Sum()
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore nel recupero statistiche attività");
                return SafeInternalError<Dictionary<string, int>>("Errore durante il recupero delle statistiche");
            }
        }

        // ✅ NUOVI ENDPOINT PER FRONTEND

        [HttpGet("frontend")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<LogAttivitaFrontendDTO>>> GetAllPerFrontend()
        {
            try
            {
                var result = await _repository.GetAllPerFrontendAsync();

                LogAuditTrail("GET_ALL_FRONTEND", "LogAttivita", "All");
                LogSecurityEvent("LogAttivitaGetAllFrontend", new
                {
                    UserId = GetCurrentUserId(),
                    Count = result.Count()
                });

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore nel recupero di tutti i log attività per frontend");
                return SafeInternalError<IEnumerable<LogAttivitaFrontendDTO>>("Errore durante il recupero dei log attività");
            }
        }

        [HttpGet("frontend/periodo")]
        [AllowAnonymous]        
        public async Task<ActionResult<IEnumerable<LogAttivitaFrontendDTO>>> GetByPeriodoPerFrontend([FromQuery] DateTime? dataInizio = null, [FromQuery] DateTime? dataFine = null)
        {
            try
            {
                // ✅ VALORI DI DEFAULT
                dataInizio ??= DateTime.Today.AddDays(-7); // Ultimi 7 giorni
                dataFine ??= DateTime.Now; // Fino ad ora

                if (dataInizio > dataFine)
                    return SafeBadRequest<IEnumerable<LogAttivitaFrontendDTO>>("Intervallo date non valido");

                var result = await _repository.GetByPeriodoPerFrontendAsync(dataInizio.Value, dataFine.Value);

                return Ok(new
                {
                    Message = $"Trovate {result.Count()} attività dal {dataInizio:dd/MM/yyyy} al {dataFine:dd/MM/yyyy}",
                    Periodo = new { Da = dataInizio, A = dataFine },
                    Count = result.Count(),
                    Data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore nel recupero log attività per periodo frontend {DataInizio} - {DataFine}", dataInizio, dataFine);
                return SafeInternalError<IEnumerable<LogAttivitaFrontendDTO>>("Errore interno nel recupero log attività per periodo");
            }
        }

        [HttpGet("frontend/tipo-attivita/{tipoAttivita}")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<LogAttivitaFrontendDTO>>> GetByTipoAttivitaPerFrontend(string tipoAttivita)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(tipoAttivita))
                    return SafeBadRequest<IEnumerable<LogAttivitaFrontendDTO>>("Tipo attività non valido");

                var result = await _repository.GetByTipoAttivitaPerFrontendAsync(tipoAttivita);

                // ✅ MESSAGGIO PER RISULTATI VUOTI
                if (!result.Any())
                    return Ok(new
                    {
                        Message = $"Nessuna attività trovata per il tipo '{tipoAttivita}'",
                        Count = 0,
                        Data = Array.Empty<LogAttivitaFrontendDTO>()
                    });

                LogAuditTrail("GET_BY_TIPO_FRONTEND", "LogAttivita", tipoAttivita);

                return Ok(new
                {
                    Message = $"Trovate {result.Count()} attività per il tipo '{tipoAttivita}'",
                    Count = result.Count(),
                    Data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore nel recupero log attività per tipo frontend {TipoAttivita}", tipoAttivita);
                return SafeInternalError<IEnumerable<LogAttivitaFrontendDTO>>("Errore durante il recupero dei log attività per tipo");
            }
        }

        [HttpPost("cleanup")]
        //[Authorize(Roles = "admin,system")] // ✅ SOLO ADMIN
        public async Task<ActionResult<int>> CleanupOldLogs([FromQuery] int giorniRitenzione = 90)
        {
            try
            {
                if (giorniRitenzione < 1)
                    return SafeBadRequest<int>("Giorni ritenzione non validi");

                var deletedCount = await _repository.CleanupOldLogsAsync(giorniRitenzione);

                LogAuditTrail("CLEANUP_LOGS", "LogAttivita", $"Deleted: {deletedCount}, Retention: {giorniRitenzione} giorni");

                return Ok(new
                {
                    LogsEliminati = deletedCount,
                    GiorniRitenzione = giorniRitenzione,
                    Messaggio = $"Puliti {deletedCount} log vecchi di {giorniRitenzione} giorni"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante il cleanup dei log attività");
                return SafeInternalError<int>("Errore durante la pulizia dei log");
            }
        }

        // ✅ ENDPOINT RICERCA INTELLIGENTE
        [HttpGet("frontend/search")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<LogAttivitaFrontendDTO>>> SearchIntelligente([FromQuery] string q)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(q))
                    return SafeBadRequest<IEnumerable<LogAttivitaFrontendDTO>>("Termine di ricerca non valido");

                var result = await _repository.SearchIntelligenteAsync(q);

                return Ok(new
                {
                    Message = result.Any()
                        ? $"Trovate {result.Count()} attività per '{q}'"
                        : $"Nessuna attività trovata per '{q}'",
                    SearchTerm = q,
                    Count = result.Count(),
                    Data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore nella ricerca intelligente per '{SearchTerm}'", q);
                return SafeInternalError<IEnumerable<LogAttivitaFrontendDTO>>("Errore durante la ricerca");
            }
        }

        // ✅ ENDPOINT TIPO ATTIVITÀ INTELLIGENTE
        [HttpGet("frontend/tipo-attivita-intelligente/{tipoAttivita}")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<LogAttivitaFrontendDTO>>> GetByTipoAttivitaIntelligente(string tipoAttivita)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(tipoAttivita))
                    return SafeBadRequest<IEnumerable<LogAttivitaFrontendDTO>>("Tipo attività non valido");

                var result = await _repository.GetByTipoAttivitaIntelligenteAsync(tipoAttivita);

                return Ok(new
                {
                    Message = result.Any()
                        ? $"Trovate {result.Count()} attività per il tipo '{tipoAttivita}'"
                        : $"Nessuna attività trovata per il tipo '{tipoAttivita}'",
                    TipoAttivita = tipoAttivita,
                    Count = result.Count(),
                    Data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore nel recupero log attività per tipo intelligente {TipoAttivita}", tipoAttivita);
                return SafeInternalError<IEnumerable<LogAttivitaFrontendDTO>>("Errore durante il recupero dei log attività");
            }
        }
    }
}