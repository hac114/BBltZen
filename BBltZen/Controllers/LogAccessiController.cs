using Microsoft.AspNetCore.Mvc;
using DTO;
using Repository.Interface;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Database;

namespace BBltZen.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [AllowAnonymous] // ✅ OVERRIDE DELL'[Authorize] DEL BASE CONTROLLER
    public class LogAccessiController : SecureBaseController
    {
        private readonly ILogAccessiRepository _repository;
        private readonly BubbleTeaContext _context;

        // ✅ VALORI AMMESSI SECONDO CHECK CONSTRAINTS DEL DATABASE
        private static readonly HashSet<string> TipiAccessoAmmessi = new() { "qr_login", "logout", "login" };
        private static readonly HashSet<string> EsitiAmmessi = new() { "successo", "fallito" };

        public LogAccessiController(
            ILogAccessiRepository repository,
            BubbleTeaContext context,
            IWebHostEnvironment environment,
            ILogger<LogAccessiController> logger)
            : base(environment, logger)
        {
            _repository = repository;
            _context = context;
        }

        [HttpGet]
        [Authorize(Roles = "admin,auditor,gestore")] // ✅ AGGIUNTO "gestore"
        public async Task<ActionResult<IEnumerable<LogAccessiDTO>>> GetAll()
        {
            try
            {
                var result = await _repository.GetAllAsync();

                LogAuditTrail("GET_ALL_LOG_ACCESSI", "LogAccessi", "All");
                LogSecurityEvent("LogAccessiGetAll", new
                {
                    User = User.Identity?.Name,
                    Timestamp = DateTime.UtcNow,
                    Count = result.Count()
                });

                return Ok(result);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore nel recupero di tutti i log accessi");
                return SafeInternalError<IEnumerable<LogAccessiDTO>>("Errore durante il recupero dei log accessi");
            }
        }

        [HttpGet("{logId}")]
        [Authorize(Roles = "admin,auditor,gestore")] // ✅ AGGIUNTO "gestore"
        public async Task<ActionResult<LogAccessiDTO>> GetById(int logId)
        {
            try
            {
                if (logId <= 0)
                    return SafeBadRequest<LogAccessiDTO>("ID log accessi non valido");

                var result = await _repository.GetByIdAsync(logId);

                if (result == null)
                    return SafeNotFound<LogAccessiDTO>("Log accessi");

                LogAuditTrail("GET_LOG_ACCESSI_BY_ID", "LogAccessi", logId.ToString());
                LogSecurityEvent("LogAccessiGetById", new
                {
                    LogId = logId,
                    User = User.Identity?.Name,
                    Timestamp = DateTime.UtcNow
                });

                return Ok(result);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore nel recupero log accessi {LogId}", logId);
                return SafeInternalError<LogAccessiDTO>("Errore durante il recupero del log accessi");
            }
        }

        [HttpGet("utente/{utenteId}")]
        [Authorize(Roles = "admin,auditor,gestore")] // ✅ AGGIUNTO "gestore"
        public async Task<ActionResult<IEnumerable<LogAccessiDTO>>> GetByUtenteId(int utenteId)
        {
            try
            {
                if (utenteId <= 0)
                    return SafeBadRequest<IEnumerable<LogAccessiDTO>>("ID utente non valido");

                var utenteEsiste = await _context.Utenti.AnyAsync(u => u.UtenteId == utenteId);
                if (!utenteEsiste)
                    return SafeNotFound<IEnumerable<LogAccessiDTO>>("Utente");

                var result = await _repository.GetByUtenteIdAsync(utenteId);

                LogAuditTrail("GET_LOG_ACCESSI_BY_UTENTE", "LogAccessi", utenteId.ToString());
                LogSecurityEvent("LogAccessiGetByUtente", new
                {
                    UtenteId = utenteId,
                    User = User.Identity?.Name,
                    Timestamp = DateTime.UtcNow,
                    Count = result.Count()
                });

                return Ok(result);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore nel recupero log accessi per utente {UtenteId}", utenteId);
                return SafeInternalError<IEnumerable<LogAccessiDTO>>("Errore durante il recupero dei log accessi per utente");
            }
        }

        [HttpGet("cliente/{clienteId}")]
        [Authorize(Roles = "admin,auditor,gestore")] // ✅ AGGIUNTO "gestore"
        public async Task<ActionResult<IEnumerable<LogAccessiDTO>>> GetByClienteId(int clienteId)
        {
            try
            {
                if (clienteId <= 0)
                    return SafeBadRequest<IEnumerable<LogAccessiDTO>>("ID cliente non valido");

                var clienteEsiste = await _context.Cliente.AnyAsync(c => c.ClienteId == clienteId);
                if (!clienteEsiste)
                    return SafeNotFound<IEnumerable<LogAccessiDTO>>("Cliente");

                var result = await _repository.GetByClienteIdAsync(clienteId);

                LogAuditTrail("GET_LOG_ACCESSI_BY_CLIENTE", "LogAccessi", clienteId.ToString());
                LogSecurityEvent("LogAccessiGetByCliente", new
                {
                    ClienteId = clienteId,
                    User = User.Identity?.Name,
                    Timestamp = DateTime.UtcNow,
                    Count = result.Count()
                });

                return Ok(result);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore nel recupero log accessi per cliente {ClienteId}", clienteId);
                return SafeInternalError<IEnumerable<LogAccessiDTO>>("Errore durante il recupero dei log accessi per cliente");
            }
        }

        [HttpGet("tipo-accesso/{tipoAccesso}")]
        [Authorize(Roles = "admin,auditor,gestore")] // ✅ AGGIUNTO "gestore"
        public async Task<ActionResult<IEnumerable<LogAccessiDTO>>> GetByTipoAccesso(string tipoAccesso)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(tipoAccesso))
                    return SafeBadRequest<IEnumerable<LogAccessiDTO>>("Tipo accesso non valido");

                // ✅ VALIDAZIONE TIPO ACCESSO AMMESSO
                if (!TipiAccessoAmmessi.Contains(tipoAccesso.ToLower()))
                    return SafeBadRequest<IEnumerable<LogAccessiDTO>>("Tipo accesso non valido. Valori ammessi: qr_login, logout, login");

                var result = await _repository.GetByTipoAccessoAsync(tipoAccesso);

                LogAuditTrail("GET_LOG_ACCESSI_BY_TIPO_ACCESSO", "LogAccessi", tipoAccesso);
                LogSecurityEvent("LogAccessiGetByTipoAccesso", new
                {
                    TipoAccesso = tipoAccesso,
                    User = User.Identity?.Name,
                    Timestamp = DateTime.UtcNow,
                    Count = result.Count()
                });

                return Ok(result);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore nel recupero log accessi per tipo accesso {TipoAccesso}", tipoAccesso);
                return SafeInternalError<IEnumerable<LogAccessiDTO>>("Errore durante il recupero dei log accessi per tipo accesso");
            }
        }

        [HttpGet("esito/{esito}")]
        [Authorize(Roles = "admin,auditor,gestore")] // ✅ AGGIUNTO "gestore"
        public async Task<ActionResult<IEnumerable<LogAccessiDTO>>> GetByEsito(string esito)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(esito))
                    return SafeBadRequest<IEnumerable<LogAccessiDTO>>("Esito accesso non valido");

                // ✅ VALIDAZIONE ESITO AMMESSO
                if (!EsitiAmmessi.Contains(esito.ToLower()))
                    return SafeBadRequest<IEnumerable<LogAccessiDTO>>("Esito non valido. Valori ammessi: successo, fallito");

                var result = await _repository.GetByEsitoAsync(esito);

                LogAuditTrail("GET_LOG_ACCESSI_BY_ESITO", "LogAccessi", esito);
                LogSecurityEvent("LogAccessiGetByEsito", new
                {
                    Esito = esito,
                    User = User.Identity?.Name,
                    Timestamp = DateTime.UtcNow,
                    Count = result.Count()
                });

                return Ok(result);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore nel recupero log accessi per esito {Esito}", esito);
                return SafeInternalError<IEnumerable<LogAccessiDTO>>("Errore durante il recupero dei log accessi per esito");
            }
        }

        [HttpGet("periodo")]
        [Authorize(Roles = "admin,auditor,gestore")] // ✅ AGGIUNTO "gestore"
        public async Task<ActionResult<IEnumerable<LogAccessiDTO>>> GetByPeriodo([FromQuery] DateTime dataInizio, [FromQuery] DateTime dataFine)
        {
            try
            {
                if (dataInizio > dataFine)
                    return SafeBadRequest<IEnumerable<LogAccessiDTO>>("Data inizio non può essere successiva alla data fine");

                var result = await _repository.GetByPeriodoAsync(dataInizio, dataFine);

                LogAuditTrail("GET_LOG_ACCESSI_BY_PERIODO", "LogAccessi", $"{dataInizio:yyyy-MM-dd}_{dataFine:yyyy-MM-dd}");
                LogSecurityEvent("LogAccessiGetByPeriodo", new
                {
                    DataInizio = dataInizio,
                    DataFine = dataFine,
                    User = User.Identity?.Name,
                    Timestamp = DateTime.UtcNow,
                    Count = result.Count()
                });

                return Ok(result);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore nel recupero log accessi per periodo {DataInizio} - {DataFine}", dataInizio, dataFine);
                return SafeInternalError<IEnumerable<LogAccessiDTO>>("Errore durante il recupero dei log accessi per periodo");
            }
        }

        [HttpGet("statistiche/numero-accessi")]
        [Authorize(Roles = "admin,auditor,gestore")] // ✅ AGGIUNTO "gestore"
        public async Task<ActionResult<int>> GetNumeroAccessi([FromQuery] DateTime? dataInizio = null, [FromQuery] DateTime? dataFine = null)
        {
            try
            {
                if (dataInizio.HasValue && dataFine.HasValue && dataInizio > dataFine)
                    return SafeBadRequest<int>("Data inizio non può essere successiva alla data fine");

                var result = await _repository.GetNumeroAccessiAsync(dataInizio, dataFine);

                LogAuditTrail("GET_NUMERO_ACCESSI_STATISTICHE", "LogAccessi",
                    $"Inizio: {dataInizio?.ToString("yyyy-MM-dd") ?? "null"}, Fine: {dataFine?.ToString("yyyy-MM-dd") ?? "null"}");
                LogSecurityEvent("LogAccessiGetStatistics", new
                {
                    DataInizio = dataInizio,
                    DataFine = dataFine,
                    User = User.Identity?.Name,
                    Timestamp = DateTime.UtcNow,
                    NumeroAccessi = result
                });

                return Ok(result);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore nel recupero statistiche numero accessi");
                return SafeInternalError<int>("Errore durante il recupero delle statistiche accessi");
            }
        }

        [HttpPost]
        [Authorize(Roles = "admin,system,gestore")] // ✅ AGGIUNTO "gestore"
        public async Task<ActionResult<LogAccessiDTO>> Create([FromBody] LogAccessiDTO logAccessiDto)
        {
            try
            {
                if (!IsModelValid(logAccessiDto))
                    return SafeBadRequest<LogAccessiDTO>("Dati log accessi non validi");

                // ✅ VALIDAZIONE TIPO ACCESSO
                if (!TipiAccessoAmmessi.Contains(logAccessiDto.TipoAccesso.ToLower()))
                    return SafeBadRequest<LogAccessiDTO>("Tipo accesso non valido. Valori ammessi: qr_login, logout, login");

                // ✅ VALIDAZIONE ESITO
                if (!EsitiAmmessi.Contains(logAccessiDto.Esito.ToLower()))
                    return SafeBadRequest<LogAccessiDTO>("Esito non valido. Valori ammessi: successo, fallito");

                // ✅ VERIFICA UTENTE (se specificato)
                if (logAccessiDto.UtenteId.HasValue)
                {
                    var utenteEsiste = await _context.Utenti.AnyAsync(u => u.UtenteId == logAccessiDto.UtenteId.Value);
                    if (!utenteEsiste)
                        return SafeBadRequest<LogAccessiDTO>("Utente non trovato");
                }

                // ✅ VERIFICA CLIENTE (se specificato)
                if (logAccessiDto.ClienteId.HasValue)
                {
                    var clienteEsiste = await _context.Cliente.AnyAsync(c => c.ClienteId == logAccessiDto.ClienteId.Value);
                    if (!clienteEsiste)
                        return SafeBadRequest<LogAccessiDTO>("Cliente non trovato");
                }

                await _repository.AddAsync(logAccessiDto);

                LogAuditTrail("CREATE_LOG_ACCESSI", "LogAccessi", logAccessiDto.LogId.ToString());
                LogSecurityEvent("LogAccessiCreated", new
                {
                    LogId = logAccessiDto.LogId,
                    UtenteId = logAccessiDto.UtenteId,
                    ClienteId = logAccessiDto.ClienteId,
                    TipoAccesso = logAccessiDto.TipoAccesso,
                    Esito = logAccessiDto.Esito,
                    IpAddress = logAccessiDto.IpAddress,
                    User = User.Identity?.Name,
                    Timestamp = DateTime.UtcNow
                });

                return CreatedAtAction(nameof(GetById), new { logId = logAccessiDto.LogId }, logAccessiDto);
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "Errore database durante la creazione del log accessi");
                return SafeInternalError<LogAccessiDTO>("Errore durante il salvataggio del log accessi");
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore nella creazione log accessi");
                return SafeInternalError<LogAccessiDTO>("Errore durante la creazione del log accessi");
            }
        }

        [HttpPut("{logId}")]
        [Authorize(Roles = "admin,gestore")] // ✅ AGGIUNTO "gestore"
        public async Task<ActionResult> Update(int logId, [FromBody] LogAccessiDTO logAccessiDto)
        {
            try
            {
                if (logId <= 0)
                    return SafeBadRequest("ID log accessi non valido");

                if (logAccessiDto.LogId != logId)
                    return SafeBadRequest("ID log accessi non corrispondente");

                if (!IsModelValid(logAccessiDto))
                    return SafeBadRequest("Dati log accessi non validi");

                var existing = await _repository.GetByIdAsync(logId);
                if (existing == null)
                    return SafeNotFound("Log accessi");

                // ✅ VALIDAZIONE TIPO ACCESSO
                if (!TipiAccessoAmmessi.Contains(logAccessiDto.TipoAccesso.ToLower()))
                    return SafeBadRequest("Tipo accesso non valido. Valori ammessi: qr_login, logout, login");

                // ✅ VALIDAZIONE ESITO
                if (!EsitiAmmessi.Contains(logAccessiDto.Esito.ToLower()))
                    return SafeBadRequest("Esito non valido. Valori ammessi: successo, fallito");

                // ✅ VERIFICA UTENTE (se specificato)
                if (logAccessiDto.UtenteId.HasValue)
                {
                    var utenteEsiste = await _context.Utenti.AnyAsync(u => u.UtenteId == logAccessiDto.UtenteId.Value);
                    if (!utenteEsiste)
                        return SafeBadRequest("Utente non trovato");
                }

                // ✅ VERIFICA CLIENTE (se specificato)
                if (logAccessiDto.ClienteId.HasValue)
                {
                    var clienteEsiste = await _context.Cliente.AnyAsync(c => c.ClienteId == logAccessiDto.ClienteId.Value);
                    if (!clienteEsiste)
                        return SafeBadRequest("Cliente non trovato");
                }

                await _repository.UpdateAsync(logAccessiDto);

                LogAuditTrail("UPDATE_LOG_ACCESSI", "LogAccessi", logId.ToString());
                LogSecurityEvent("LogAccessiUpdated", new
                {
                    LogId = logId,
                    OldUtenteId = existing.UtenteId,
                    NewUtenteId = logAccessiDto.UtenteId,
                    OldClienteId = existing.ClienteId,
                    NewClienteId = logAccessiDto.ClienteId,
                    OldTipoAccesso = existing.TipoAccesso,
                    NewTipoAccesso = logAccessiDto.TipoAccesso,
                    OldEsito = existing.Esito,
                    NewEsito = logAccessiDto.Esito,
                    User = User.Identity?.Name,
                    Timestamp = DateTime.UtcNow
                });

                return NoContent();
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Log accessi non trovato durante l'aggiornamento {LogId}", logId);
                return SafeNotFound("Log accessi");
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "Errore database durante l'aggiornamento del log accessi {LogId}", logId);
                return SafeInternalError("Errore durante l'aggiornamento del log accessi");
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore nell'aggiornamento log accessi {LogId}", logId);
                return SafeInternalError("Errore durante l'aggiornamento del log accessi");
            }
        }

        [HttpDelete("{logId}")]
        [Authorize(Roles = "admin")] // ❌ SOLO admin può eliminare (per sicurezza)
        public async Task<ActionResult> Delete(int logId)
        {
            try
            {
                if (logId <= 0)
                    return SafeBadRequest("ID log accessi non valido");

                var existing = await _repository.GetByIdAsync(logId);
                if (existing == null)
                    return SafeNotFound("Log accessi");

                await _repository.DeleteAsync(logId);

                LogAuditTrail("DELETE_LOG_ACCESSI", "LogAccessi", logId.ToString());
                LogSecurityEvent("LogAccessiDeleted", new
                {
                    LogId = logId,
                    UtenteId = existing.UtenteId,
                    ClienteId = existing.ClienteId,
                    TipoAccesso = existing.TipoAccesso,
                    Esito = existing.Esito,
                    User = User.Identity?.Name,
                    Timestamp = DateTime.UtcNow
                });

                return NoContent();
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "Errore database durante l'eliminazione del log accessi {LogId}", logId);
                return SafeInternalError("Errore durante l'eliminazione del log accessi");
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore nell'eliminazione log accessi {LogId}", logId);
                return SafeInternalError("Errore durante l'eliminazione del log accessi");
            }
        }

        // ✅ METODI AGGIUNTIVI UTILI
        [HttpGet("exists")]
        [Authorize(Roles = "admin,auditor,gestore")]
        public async Task<ActionResult<bool>> HasRecords()
        {
            try
            {
                var records = await _repository.GetAllAsync();
                return Ok(records.Any());
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore nella verifica esistenza record LogAccessi");
                return SafeInternalError<bool>("Errore durante la verifica");
            }
        }

        [HttpGet("statistiche")]
        [Authorize(Roles = "admin,auditor,gestore")]
        public async Task<ActionResult<object>> GetStatistiche()
        {
            try
            {
                var records = await _repository.GetAllAsync();
                var count = records.Count();

                return Ok(new
                {
                    TotalRecords = count,
                    IsEmpty = count == 0,
                    FirstRecordDate = count > 0 ? records.Min(r => r.DataCreazione) : null,
                    LastRecordDate = count > 0 ? records.Max(r => r.DataCreazione) : null,
                    TipiAccessoUtilizzati = count > 0 ? records.Select(r => r.TipoAccesso).Distinct() : Array.Empty<string>(),
                    EsitiUtilizzati = count > 0 ? records.Select(r => r.Esito).Distinct() : Array.Empty<string>()
                });
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore nel recupero statistiche LogAccessi");
                return SafeInternalError<object>("Errore durante il recupero statistiche");
            }
        }
    }
}