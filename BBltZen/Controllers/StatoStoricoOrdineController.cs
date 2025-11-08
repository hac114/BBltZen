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
    public class StatoStoricoOrdineController : SecureBaseController
    {
        private readonly IStatoStoricoOrdineRepository _repository;

        public StatoStoricoOrdineController(
            IStatoStoricoOrdineRepository repository,
            IWebHostEnvironment environment,
            ILogger<StatoStoricoOrdineController> logger)
            : base(environment, logger)
        {
            _repository = repository;
        }

        // GET: api/StatoStoricoOrdine
        [HttpGet]
        [AllowAnonymous] // ✅ CALCOLO PREZZI PUBBLICO
        public async Task<ActionResult<IEnumerable<StatoStoricoOrdineDTO>>> GetAll()
        {
            try
            {
                var statiStorici = await _repository.GetAllAsync();
                return Ok(statiStorici);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore durante il recupero di tutti gli stati storici ordine");
                return SafeInternalError(
                    _environment.IsDevelopment()
                        ? $"Errore durante il recupero degli stati storici ordine: {ex.Message}"
                        : "Errore interno nel recupero degli stati storici"
                );
            }
        }

        // GET: api/StatoStoricoOrdine/5
        [HttpGet("{statoStoricoOrdineId}")]
        [AllowAnonymous] // ✅ CALCOLO PREZZI PUBBLICO
        public async Task<ActionResult<StatoStoricoOrdineDTO>> GetById(int statoStoricoOrdineId)
        {
            try
            {
                if (statoStoricoOrdineId <= 0)
                    return SafeBadRequest(
                        _environment.IsDevelopment()
                            ? "ID stato storico ordine non valido: deve essere maggiore di 0"
                            : "ID stato storico non valido"
                    );

                var statoStorico = await _repository.GetByIdAsync(statoStoricoOrdineId);

                if (statoStorico == null)
                    return SafeNotFound(
                        _environment.IsDevelopment()
                            ? $"Stato storico ordine con ID {statoStoricoOrdineId} non trovato"
                            : "Stato storico ordine non trovato"
                    );

                return Ok(statoStorico);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore durante il recupero dello stato storico ordine {StatoStoricoOrdineId}", statoStoricoOrdineId);
                return SafeInternalError(
                    _environment.IsDevelopment()
                        ? $"Errore durante il recupero dello stato storico ordine {statoStoricoOrdineId}: {ex.Message}"
                        : "Errore interno nel recupero dello stato storico"
                );
            }
        }

        // GET: api/StatoStoricoOrdine/ordine/5
        [HttpGet("ordine/{ordineId}")]
        [AllowAnonymous] // ✅ CALCOLO PREZZI PUBBLICO
        public async Task<ActionResult<IEnumerable<StatoStoricoOrdineDTO>>> GetByOrdineId(int ordineId)
        {
            try
            {
                if (ordineId <= 0)
                    return SafeBadRequest(
                        _environment.IsDevelopment()
                            ? "ID ordine non valido: deve essere maggiore di 0"
                            : "ID ordine non valido"
                    );

                var statiStorici = await _repository.GetByOrdineIdAsync(ordineId);

                // ✅ Log per audit
                LogAuditTrail("GET_STATI_STORICI_ORDINE", "StatoStoricoOrdine", ordineId.ToString());

                return Ok(statiStorici);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore durante il recupero degli stati storici per ordine {OrdineId}", ordineId);
                return SafeInternalError(
                    _environment.IsDevelopment()
                        ? $"Errore durante il recupero degli stati storici per ordine {ordineId}: {ex.Message}"
                        : "Errore interno nel recupero degli stati storici ordine"
                );
            }
        }

        // GET: api/StatoStoricoOrdine/stato-ordine/5
        [HttpGet("stato-ordine/{statoOrdineId}")]
        [AllowAnonymous] // ✅ CALCOLO PREZZI PUBBLICO
        public async Task<ActionResult<IEnumerable<StatoStoricoOrdineDTO>>> GetByStatoOrdineId(int statoOrdineId)
        {
            try
            {
                if (statoOrdineId <= 0)
                    return SafeBadRequest(
                        _environment.IsDevelopment()
                            ? "ID stato ordine non valido: deve essere maggiore di 0"
                            : "ID stato ordine non valido"
                    );

                var statiStorici = await _repository.GetByStatoOrdineIdAsync(statoOrdineId);

                // ✅ Log per audit
                LogAuditTrail("GET_STATI_STORICI_BY_STATO", "StatoStoricoOrdine", statoOrdineId.ToString());

                return Ok(statiStorici);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore durante il recupero degli stati storici per stato ordine {StatoOrdineId}", statoOrdineId);
                return SafeInternalError(
                    _environment.IsDevelopment()
                        ? $"Errore durante il recupero degli stati storici per stato ordine {statoOrdineId}: {ex.Message}"
                        : "Errore interno nel recupero degli stati storici per stato"
                );
            }
        }

        // GET: api/StatoStoricoOrdine/ordine/5/storico-completo
        [HttpGet("ordine/{ordineId}/storico-completo")]
        [AllowAnonymous] // ✅ CALCOLO PREZZI PUBBLICO
        public async Task<ActionResult<IEnumerable<StatoStoricoOrdineDTO>>> GetStoricoCompletoOrdine(int ordineId)
        {
            try
            {
                if (ordineId <= 0)
                    return SafeBadRequest(
                        _environment.IsDevelopment()
                            ? "ID ordine non valido: deve essere maggiore di 0"
                            : "ID ordine non valido"
                    );

                var storicoCompleto = await _repository.GetStoricoCompletoOrdineAsync(ordineId);

                // ✅ Log per audit
                LogAuditTrail("GET_STORICO_COMPLETO_ORDINE", "StatoStoricoOrdine", ordineId.ToString());

                return Ok(storicoCompleto);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore durante il recupero dello storico completo per ordine {OrdineId}", ordineId);
                return SafeInternalError(
                    _environment.IsDevelopment()
                        ? $"Errore durante il recupero dello storico completo per ordine {ordineId}: {ex.Message}"
                        : "Errore interno nel recupero dello storico completo"
                );
            }
        }

        // GET: api/StatoStoricoOrdine/ordine/5/stato-attuale
        [HttpGet("ordine/{ordineId}/stato-attuale")]
        [AllowAnonymous] // ✅ CALCOLO PREZZI PUBBLICO
        public async Task<ActionResult<StatoStoricoOrdineDTO>> GetStatoAttualeOrdine(int ordineId)
        {
            try
            {
                if (ordineId <= 0)
                    return SafeBadRequest(
                        _environment.IsDevelopment()
                            ? "ID ordine non valido: deve essere maggiore di 0"
                            : "ID ordine non valido"
                    );

                var statoAttuale = await _repository.GetStatoAttualeOrdineAsync(ordineId);

                if (statoAttuale == null)
                    return SafeNotFound(
                        _environment.IsDevelopment()
                            ? $"Stato attuale non trovato per l'ordine {ordineId}"
                            : "Stato attuale ordine non trovato"
                    );

                // ✅ Log per audit
                LogAuditTrail("GET_STATO_ATTUALE_ORDINE", "StatoStoricoOrdine", ordineId.ToString());

                return Ok(statoAttuale);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore durante il recupero dello stato attuale per ordine {OrdineId}", ordineId);
                return SafeInternalError(
                    _environment.IsDevelopment()
                        ? $"Errore durante il recupero dello stato attuale per ordine {ordineId}: {ex.Message}"
                        : "Errore interno nel recupero dello stato attuale"
                );
            }
        }

        // POST: api/StatoStoricoOrdine
        [HttpPost]
        [AllowAnonymous] // ✅ COMMENTATO PER TEST SWAGGER
        public async Task<ActionResult<StatoStoricoOrdineDTO>> Create(StatoStoricoOrdineDTO statoStoricoOrdineDto)
        {
            try
            {
                if (!IsModelValid(statoStoricoOrdineDto))
                    return SafeBadRequest(
                        _environment.IsDevelopment()
                            ? "Dati stato storico ordine non validi: modello di binding fallito"
                            : "Dati stato storico ordine non validi"
                    );

                await _repository.AddAsync(statoStoricoOrdineDto);

                // ✅ Log per audit e sicurezza
                LogAuditTrail("CREATE_STATO_STORICO_ORDINE", "StatoStoricoOrdine", statoStoricoOrdineDto.StatoStoricoOrdineId.ToString());
                LogSecurityEvent("StatoStoricoOrdineCreated", new
                {
                    StatoStoricoOrdineId = statoStoricoOrdineDto.StatoStoricoOrdineId,
                    OrdineId = statoStoricoOrdineDto.OrdineId,
                    StatoOrdineId = statoStoricoOrdineDto.StatoOrdineId,
                    Inizio = statoStoricoOrdineDto.Inizio,
                    User = User.Identity?.Name ?? "Anonymous"
                });

                return CreatedAtAction(nameof(GetById),
                    new { statoStoricoOrdineId = statoStoricoOrdineDto.StatoStoricoOrdineId },
                    statoStoricoOrdineDto);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore durante la creazione dello stato storico ordine");
                return SafeInternalError(
                    _environment.IsDevelopment()
                        ? $"Errore durante la creazione dello stato storico ordine: {ex.Message}"
                        : "Errore interno nella creazione dello stato storico"
                );
            }
        }

        // PUT: api/StatoStoricoOrdine/5
        [HttpPut("{statoStoricoOrdineId}")]
        [AllowAnonymous] // ✅ COMMENTATO PER TEST SWAGGER
        public async Task<ActionResult> Update(int statoStoricoOrdineId, StatoStoricoOrdineDTO statoStoricoOrdineDto)
        {
            try
            {
                if (statoStoricoOrdineId <= 0)
                    return SafeBadRequest(
                        _environment.IsDevelopment()
                            ? "ID stato storico ordine non valido: deve essere maggiore di 0"
                            : "ID stato storico ordine non valido"
                    );

                if (statoStoricoOrdineId != statoStoricoOrdineDto.StatoStoricoOrdineId)
                    return SafeBadRequest(
                        _environment.IsDevelopment()
                            ? $"ID stato storico ordine non corrispondente: URL={statoStoricoOrdineId}, Body={statoStoricoOrdineDto.StatoStoricoOrdineId}"
                            : "Identificativi non corrispondenti"
                    );

                if (!IsModelValid(statoStoricoOrdineDto))
                    return SafeBadRequest(
                        _environment.IsDevelopment()
                            ? "Dati stato storico ordine non validi: modello di binding fallito"
                            : "Dati stato storico ordine non validi"
                    );

                var existing = await _repository.GetByIdAsync(statoStoricoOrdineId);
                if (existing == null)
                    return SafeNotFound(
                        _environment.IsDevelopment()
                            ? $"Stato storico ordine con ID {statoStoricoOrdineId} non trovato per l'aggiornamento"
                            : "Stato storico ordine non trovato"
                    );

                await _repository.UpdateAsync(statoStoricoOrdineDto);

                // ✅ Log per audit e sicurezza
                LogAuditTrail("UPDATE_STATO_STORICO_ORDINE", "StatoStoricoOrdine", statoStoricoOrdineDto.StatoStoricoOrdineId.ToString());
                LogSecurityEvent("StatoStoricoOrdineUpdated", new
                {
                    StatoStoricoOrdineId = statoStoricoOrdineDto.StatoStoricoOrdineId,
                    OrdineId = statoStoricoOrdineDto.OrdineId,
                    User = User.Identity?.Name
                });

                return NoContent();
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore durante l'aggiornamento dello stato storico ordine {StatoStoricoOrdineId}", statoStoricoOrdineId);
                return SafeInternalError(
                    _environment.IsDevelopment()
                        ? $"Errore durante l'aggiornamento dello stato storico ordine {statoStoricoOrdineId}: {ex.Message}"
                        : "Errore interno nell'aggiornamento dello stato storico"
                );
            }
        }

        // DELETE: api/StatoStoricoOrdine/5
        [HttpDelete("{statoStoricoOrdineId}")]
        [AllowAnonymous] // ✅ COMMENTATO PER TEST SWAGGER
        public async Task<ActionResult> Delete(int statoStoricoOrdineId)
        {
            try
            {
                if (statoStoricoOrdineId <= 0)
                    return SafeBadRequest(
                        _environment.IsDevelopment()
                            ? "ID stato storico ordine non valido: deve essere maggiore di 0"
                            : "ID stato storico ordine non valido"
                    );

                var statoStorico = await _repository.GetByIdAsync(statoStoricoOrdineId);
                if (statoStorico == null)
                    return SafeNotFound(
                        _environment.IsDevelopment()
                            ? $"Stato storico ordine con ID {statoStoricoOrdineId} non trovato per l'eliminazione"
                            : "Stato storico ordine non trovato"
                    );

                await _repository.DeleteAsync(statoStoricoOrdineId);

                // ✅ Log per audit e sicurezza
                LogAuditTrail("DELETE_STATO_STORICO_ORDINE", "StatoStoricoOrdine", statoStoricoOrdineId.ToString());
                LogSecurityEvent("StatoStoricoOrdineDeleted", new
                {
                    StatoStoricoOrdineId = statoStoricoOrdineId,
                    User = User.Identity?.Name
                });

                return NoContent();
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore durante l'eliminazione dello stato storico ordine {StatoStoricoOrdineId}", statoStoricoOrdineId);
                return SafeInternalError(
                    _environment.IsDevelopment()
                        ? $"Errore durante l'eliminazione dello stato storico ordine {statoStoricoOrdineId}: {ex.Message}"
                        : "Errore interno nell'eliminazione dello stato storico"
                );
            }
        }

        // POST: api/StatoStoricoOrdine/ordine/5/chiudi-stato-attuale
        [HttpPost("ordine/{ordineId}/chiudi-stato-attuale")]
        [AllowAnonymous] // ✅ COMMENTATO PER TEST SWAGGER
        public async Task<ActionResult> ChiudiStatoAttuale(int ordineId)
        {
            try
            {
                if (ordineId <= 0)
                    return SafeBadRequest(
                        _environment.IsDevelopment()
                            ? "ID ordine non valido: deve essere maggiore di 0"
                            : "ID ordine non valido"
                    );

                var result = await _repository.ChiudiStatoAttualeAsync(ordineId, DateTime.Now);

                if (!result)
                    return SafeNotFound(
                        _environment.IsDevelopment()
                            ? $"Nessuno stato attuale trovato per l'ordine {ordineId}"
                            : "Nessuno stato attuale trovato per l'ordine specificato"
                    );

                // ✅ Log per audit e sicurezza
                LogAuditTrail("CHIUDI_STATO_ATTUALE_ORDINE", "StatoStoricoOrdine", ordineId.ToString());
                LogSecurityEvent("StatoAttualeChiuso", new
                {
                    OrdineId = ordineId,
                    User = User.Identity?.Name,
                    Timestamp = DateTime.Now
                });

                return NoContent();
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore durante la chiusura dello stato attuale per ordine {OrdineId}", ordineId);
                return SafeInternalError(
                    _environment.IsDevelopment()
                        ? $"Errore durante la chiusura dello stato attuale per ordine {ordineId}: {ex.Message}"
                        : "Errore interno nella chiusura dello stato attuale"
                );
            }
        }
    }
}