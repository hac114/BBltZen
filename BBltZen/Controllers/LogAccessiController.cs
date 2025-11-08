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
    public class LogAccessiController : SecureBaseController
    {
        private readonly ILogAccessiRepository _repository;

        public LogAccessiController(
            ILogAccessiRepository repository,
            IWebHostEnvironment environment,
            ILogger<LogAccessiController> logger)
            : base(environment, logger)
        {
            _repository = repository;
        }

        [HttpGet]
        //[Authorize(Roles = "admin,auditor")] // ✅ COMMENTATO PER TEST
        public async Task<ActionResult<IEnumerable<LogAccessiDTO>>> GetAll()
        {
            try
            {
                var result = await _repository.GetAllAsync();

                // ✅ Log per audit
                LogAuditTrail("GET_ALL_LOG_ACCESSI", "LogAccessi", "All");

                return Ok(result);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore nel recupero di tutti i log accessi");
                return SafeInternalError(
                    _environment.IsDevelopment()
                        ? $"Errore nel recupero dei log accessi: {ex.Message}"
                        : "Errore interno nel recupero dei log accessi"
                );
            }
        }

        [HttpGet("{logId}")]
        //[Authorize(Roles = "admin,auditor")] // ✅ COMMENTATO PER TEST
        public async Task<ActionResult<LogAccessiDTO>> GetById(int logId)
        {
            try
            {
                if (logId <= 0)
                    return SafeBadRequest(
                        _environment.IsDevelopment()
                            ? "ID log accessi non valido: deve essere maggiore di 0"
                            : "ID log accessi non valido"
                    );

                var result = await _repository.GetByIdAsync(logId);

                if (result == null)
                    return SafeNotFound(
                        _environment.IsDevelopment()
                            ? $"Log accessi con ID {logId} non trovato"
                            : "Log accessi non trovato"
                    );

                // ✅ Log per audit
                LogAuditTrail("GET_LOG_ACCESSI_BY_ID", "LogAccessi", logId.ToString());

                return Ok(result);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore nel recupero log accessi {LogId}", logId);
                return SafeInternalError(
                    _environment.IsDevelopment()
                        ? $"Errore nel recupero log accessi {logId}: {ex.Message}"
                        : "Errore interno nel recupero log accessi"
                );
            }
        }

        [HttpGet("utente/{utenteId}")]
        //[Authorize(Roles = "admin,auditor")] // ✅ COMMENTATO PER TEST
        public async Task<ActionResult<IEnumerable<LogAccessiDTO>>> GetByUtenteId(int utenteId)
        {
            try
            {
                if (utenteId <= 0)
                    return SafeBadRequest(
                        _environment.IsDevelopment()
                            ? "ID utente non valido: deve essere maggiore di 0"
                            : "ID utente non valido"
                    );

                var result = await _repository.GetByUtenteIdAsync(utenteId);

                // ✅ Log per audit
                LogAuditTrail("GET_LOG_ACCESSI_BY_UTENTE", "LogAccessi", utenteId.ToString());

                return Ok(result);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore nel recupero log accessi per utente {UtenteId}", utenteId);
                return SafeInternalError(
                    _environment.IsDevelopment()
                        ? $"Errore nel recupero log accessi per utente {utenteId}: {ex.Message}"
                        : "Errore interno nel recupero log accessi per utente"
                );
            }
        }

        [HttpGet("cliente/{clienteId}")]
        //[Authorize(Roles = "admin,auditor")] // ✅ COMMENTATO PER TEST
        public async Task<ActionResult<IEnumerable<LogAccessiDTO>>> GetByClienteId(int clienteId)
        {
            try
            {
                if (clienteId <= 0)
                    return SafeBadRequest(
                        _environment.IsDevelopment()
                            ? "ID cliente non valido: deve essere maggiore di 0"
                            : "ID cliente non valido"
                    );

                var result = await _repository.GetByClienteIdAsync(clienteId);

                // ✅ Log per audit
                LogAuditTrail("GET_LOG_ACCESSI_BY_CLIENTE", "LogAccessi", clienteId.ToString());

                return Ok(result);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore nel recupero log accessi per cliente {ClienteId}", clienteId);
                return SafeInternalError(
                    _environment.IsDevelopment()
                        ? $"Errore nel recupero log accessi per cliente {clienteId}: {ex.Message}"
                        : "Errore interno nel recupero log accessi per cliente"
                );
            }
        }

        [HttpGet("tipo-accesso/{tipoAccesso}")]
        //[Authorize(Roles = "admin,auditor")] // ✅ COMMENTATO PER TEST
        public async Task<ActionResult<IEnumerable<LogAccessiDTO>>> GetByTipoAccesso(string tipoAccesso)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(tipoAccesso))
                    return SafeBadRequest(
                        _environment.IsDevelopment()
                            ? "Tipo accesso non valido: non può essere vuoto"
                            : "Tipo accesso non valido"
                    );

                var result = await _repository.GetByTipoAccessoAsync(tipoAccesso);

                // ✅ Log per audit
                LogAuditTrail("GET_LOG_ACCESSI_BY_TIPO_ACCESSO", "LogAccessi", tipoAccesso);

                return Ok(result);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore nel recupero log accessi per tipo accesso {TipoAccesso}", tipoAccesso);
                return SafeInternalError(
                    _environment.IsDevelopment()
                        ? $"Errore nel recupero log accessi per tipo accesso {tipoAccesso}: {ex.Message}"
                        : "Errore interno nel recupero log accessi per tipo accesso"
                );
            }
        }

        [HttpGet("esito/{esito}")]
        //[Authorize(Roles = "admin,auditor")] // ✅ COMMENTATO PER TEST
        public async Task<ActionResult<IEnumerable<LogAccessiDTO>>> GetByEsito(string esito)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(esito))
                    return SafeBadRequest(
                        _environment.IsDevelopment()
                            ? "Esito accesso non valido: non può essere vuoto"
                            : "Esito accesso non valido"
                    );

                var result = await _repository.GetByEsitoAsync(esito);

                // ✅ Log per audit
                LogAuditTrail("GET_LOG_ACCESSI_BY_ESITO", "LogAccessi", esito);

                return Ok(result);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore nel recupero log accessi per esito {Esito}", esito);
                return SafeInternalError(
                    _environment.IsDevelopment()
                        ? $"Errore nel recupero log accessi per esito {esito}: {ex.Message}"
                        : "Errore interno nel recupero log accessi per esito"
                );
            }
        }

        [HttpGet("periodo")]
        //[Authorize(Roles = "admin,auditor")] // ✅ COMMENTATO PER TEST
        public async Task<ActionResult<IEnumerable<LogAccessiDTO>>> GetByPeriodo([FromQuery] DateTime dataInizio, [FromQuery] DateTime dataFine)
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
                LogAuditTrail("GET_LOG_ACCESSI_BY_PERIODO", "LogAccessi", $"{dataInizio:yyyy-MM-dd}_{dataFine:yyyy-MM-dd}");

                return Ok(result);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore nel recupero log accessi per periodo {DataInizio} - {DataFine}", dataInizio, dataFine);
                return SafeInternalError(
                    _environment.IsDevelopment()
                        ? $"Errore nel recupero log accessi per periodo {dataInizio:yyyy-MM-dd} - {dataFine:yyyy-MM-dd}: {ex.Message}"
                        : "Errore interno nel recupero log accessi per periodo"
                );
            }
        }

        [HttpGet("statistiche/numero-accessi")]
        //[Authorize(Roles = "admin,auditor")] // ✅ COMMENTATO PER TEST
        public async Task<ActionResult<int>> GetNumeroAccessi([FromQuery] DateTime? dataInizio = null, [FromQuery] DateTime? dataFine = null)
        {
            try
            {
                if (dataInizio.HasValue && dataFine.HasValue && dataInizio > dataFine)
                    return SafeBadRequest(
                        _environment.IsDevelopment()
                            ? "Data inizio non può essere successiva alla data fine"
                            : "Intervallo date non valido"
                    );

                var result = await _repository.GetNumeroAccessiAsync(dataInizio, dataFine);

                // ✅ Log per audit
                LogAuditTrail("GET_NUMERO_ACCESSI_STATISTICHE", "LogAccessi",
                    $"Inizio: {dataInizio?.ToString("yyyy-MM-dd") ?? "null"}, Fine: {dataFine?.ToString("yyyy-MM-dd") ?? "null"}");

                return Ok(result);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore nel recupero statistiche numero accessi");
                return SafeInternalError(
                    _environment.IsDevelopment()
                        ? $"Errore nel recupero statistiche numero accessi: {ex.Message}"
                        : "Errore interno nel recupero statistiche accessi"
                );
            }
        }

        [HttpPost]
        //[Authorize(Roles = "admin,system")] // ✅ COMMENTATO PER TEST
        public async Task<ActionResult<LogAccessiDTO>> Create([FromBody] LogAccessiDTO logAccessiDto)
        {
            try
            {
                // ✅ La validazione dei campi è gestita automaticamente dai Data Annotations del DTO
                if (!IsModelValid(logAccessiDto))
                    return SafeBadRequest(
                        _environment.IsDevelopment()
                            ? "Dati log accessi non validi: modello di binding fallito"
                            : "Dati log accessi non validi"
                    );

                await _repository.AddAsync(logAccessiDto);

                // ✅ Log per audit e sicurezza
                LogAuditTrail("CREATE_LOG_ACCESSI", "LogAccessi", logAccessiDto.LogId.ToString());
                LogSecurityEvent("LogAccessiCreated", new
                {
                    LogId = logAccessiDto.LogId,
                    UtenteId = logAccessiDto.UtenteId,
                    ClienteId = logAccessiDto.ClienteId,
                    TipoAccesso = logAccessiDto.TipoAccesso,
                    Esito = logAccessiDto.Esito,
                    IpAddress = logAccessiDto.IpAddress,
                    User = User.Identity?.Name ?? "System",
                    Timestamp = DateTime.UtcNow
                });

                return CreatedAtAction(nameof(GetById), new { logId = logAccessiDto.LogId }, logAccessiDto);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore nella creazione log accessi");
                return SafeInternalError(
                    _environment.IsDevelopment()
                        ? $"Errore nella creazione log accessi: {ex.Message}"
                        : "Errore interno nella creazione log accessi"
                );
            }
        }

        [HttpPut("{logId}")]
        //[Authorize(Roles = "admin")] // ✅ COMMENTATO PER TEST
        public async Task<ActionResult> Update(int logId, [FromBody] LogAccessiDTO logAccessiDto)
        {
            try
            {
                if (logId <= 0)
                    return SafeBadRequest(
                        _environment.IsDevelopment()
                            ? "ID log accessi non valido: deve essere maggiore di 0"
                            : "ID log accessi non valido"
                    );

                // ✅ La validazione dei campi è gestita automaticamente dai Data Annotations del DTO
                if (!IsModelValid(logAccessiDto))
                    return SafeBadRequest(
                        _environment.IsDevelopment()
                            ? "Dati log accessi non validi: modello di binding fallito"
                            : "Dati log accessi non validi"
                    );

                if (logAccessiDto.LogId != logId)
                    return SafeBadRequest(
                        _environment.IsDevelopment()
                            ? $"ID log accessi non corrispondente: URL={logId}, Body={logAccessiDto.LogId}"
                            : "Identificativi non corrispondenti"
                    );

                // Verifica esistenza
                var exists = await _repository.ExistsAsync(logId);
                if (!exists)
                    return SafeNotFound(
                        _environment.IsDevelopment()
                            ? $"Log accessi con ID {logId} non trovato per l'aggiornamento"
                            : "Log accessi non trovato"
                    );

                await _repository.UpdateAsync(logAccessiDto);

                // ✅ Log per audit e sicurezza
                LogAuditTrail("UPDATE_LOG_ACCESSI", "LogAccessi", logId.ToString());
                LogSecurityEvent("LogAccessiUpdated", new
                {
                    LogId = logId,
                    User = User.Identity?.Name ?? "Unknown",
                    Timestamp = DateTime.UtcNow
                });

                return NoContent();
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore nell'aggiornamento log accessi {LogId}", logId);
                return SafeInternalError(
                    _environment.IsDevelopment()
                        ? $"Errore nell'aggiornamento log accessi {logId}: {ex.Message}"
                        : "Errore interno nell'aggiornamento log accessi"
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
                            ? "ID log accessi non valido: deve essere maggiore di 0"
                            : "ID log accessi non valido"
                    );

                // Verifica esistenza
                var exists = await _repository.ExistsAsync(logId);
                if (!exists)
                    return SafeNotFound(
                        _environment.IsDevelopment()
                            ? $"Log accessi con ID {logId} non trovato per l'eliminazione"
                            : "Log accessi non trovato"
                    );

                await _repository.DeleteAsync(logId);

                // ✅ Log per audit e sicurezza
                LogAuditTrail("DELETE_LOG_ACCESSI", "LogAccessi", logId.ToString());
                LogSecurityEvent("LogAccessiDeleted", new
                {
                    LogId = logId,
                    User = User.Identity?.Name ?? "Unknown",
                    Timestamp = DateTime.UtcNow
                });

                return NoContent();
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore nell'eliminazione log accessi {LogId}", logId);
                return SafeInternalError(
                    _environment.IsDevelopment()
                        ? $"Errore nell'eliminazione log accessi {logId}: {ex.Message}"
                        : "Errore interno nell'eliminazione log accessi"
                );
            }
        }
    }
}