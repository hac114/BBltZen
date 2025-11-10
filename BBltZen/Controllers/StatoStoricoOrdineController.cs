// BBltZen/Controllers/StatoStoricoOrdineController.cs
using Microsoft.AspNetCore.Mvc;
using DTO;
using Repository.Interface;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

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
        [AllowAnonymous] // ✅ ACCESSO PUBBLICO
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
                return SafeInternalError<IEnumerable<StatoStoricoOrdineDTO>>("Errore nel recupero degli stati storici");
            }
        }

        // GET: api/StatoStoricoOrdine/5
        [HttpGet("{statoStoricoOrdineId}")]
        [AllowAnonymous] // ✅ ACCESSO PUBBLICO
        public async Task<ActionResult<StatoStoricoOrdineDTO>> GetById(int statoStoricoOrdineId)
        {
            try
            {
                if (statoStoricoOrdineId <= 0)
                    return SafeBadRequest<StatoStoricoOrdineDTO>("ID stato storico ordine non valido");

                var statoStorico = await _repository.GetByIdAsync(statoStoricoOrdineId);

                if (statoStorico == null)
                    return SafeNotFound<StatoStoricoOrdineDTO>("Stato storico ordine");

                return Ok(statoStorico);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore durante il recupero dello stato storico ordine {StatoStoricoOrdineId}", statoStoricoOrdineId);
                return SafeInternalError<StatoStoricoOrdineDTO>("Errore nel recupero dello stato storico");
            }
        }

        // GET: api/StatoStoricoOrdine/ordine/5
        [HttpGet("ordine/{ordineId}")]
        [AllowAnonymous] // ✅ ACCESSO PUBBLICO
        public async Task<ActionResult<IEnumerable<StatoStoricoOrdineDTO>>> GetByOrdineId(int ordineId)
        {
            try
            {
                if (ordineId <= 0)
                    return SafeBadRequest<IEnumerable<StatoStoricoOrdineDTO>>("ID ordine non valido");

                var statiStorici = await _repository.GetByOrdineIdAsync(ordineId);

                LogAuditTrail("GET_STATI_STORICI_ORDINE", "StatoStoricoOrdine", ordineId.ToString());
                return Ok(statiStorici);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore durante il recupero degli stati storici per ordine {OrdineId}", ordineId);
                return SafeInternalError<IEnumerable<StatoStoricoOrdineDTO>>("Errore nel recupero degli stati storici ordine");
            }
        }

        // GET: api/StatoStoricoOrdine/stato-ordine/5
        [HttpGet("stato-ordine/{statoOrdineId}")]
        [AllowAnonymous] // ✅ ACCESSO PUBBLICO
        public async Task<ActionResult<IEnumerable<StatoStoricoOrdineDTO>>> GetByStatoOrdineId(int statoOrdineId)
        {
            try
            {
                if (statoOrdineId <= 0)
                    return SafeBadRequest<IEnumerable<StatoStoricoOrdineDTO>>("ID stato ordine non valido");

                var statiStorici = await _repository.GetByStatoOrdineIdAsync(statoOrdineId);

                LogAuditTrail("GET_STATI_STORICI_BY_STATO", "StatoStoricoOrdine", statoOrdineId.ToString());
                return Ok(statiStorici);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore durante il recupero degli stati storici per stato ordine {StatoOrdineId}", statoOrdineId);
                return SafeInternalError<IEnumerable<StatoStoricoOrdineDTO>>("Errore nel recupero degli stati storici per stato");
            }
        }

        // GET: api/StatoStoricoOrdine/ordine/5/storico-completo
        [HttpGet("ordine/{ordineId}/storico-completo")]
        [AllowAnonymous] // ✅ ACCESSO PUBBLICO
        public async Task<ActionResult<IEnumerable<StatoStoricoOrdineDTO>>> GetStoricoCompletoOrdine(int ordineId)
        {
            try
            {
                if (ordineId <= 0)
                    return SafeBadRequest<IEnumerable<StatoStoricoOrdineDTO>>("ID ordine non valido");

                var storicoCompleto = await _repository.GetStoricoCompletoOrdineAsync(ordineId);

                LogAuditTrail("GET_STORICO_COMPLETO_ORDINE", "StatoStoricoOrdine", ordineId.ToString());
                return Ok(storicoCompleto);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore durante il recupero dello storico completo per ordine {OrdineId}", ordineId);
                return SafeInternalError<IEnumerable<StatoStoricoOrdineDTO>>("Errore nel recupero dello storico completo");
            }
        }

        // GET: api/StatoStoricoOrdine/ordine/5/stato-attuale
        [HttpGet("ordine/{ordineId}/stato-attuale")]
        [AllowAnonymous] // ✅ ACCESSO PUBBLICO
        public async Task<ActionResult<StatoStoricoOrdineDTO>> GetStatoAttualeOrdine(int ordineId)
        {
            try
            {
                if (ordineId <= 0)
                    return SafeBadRequest<StatoStoricoOrdineDTO>("ID ordine non valido");

                var statoAttuale = await _repository.GetStatoAttualeOrdineAsync(ordineId);

                if (statoAttuale == null)
                    return SafeNotFound<StatoStoricoOrdineDTO>("Stato attuale ordine");

                LogAuditTrail("GET_STATO_ATTUALE_ORDINE", "StatoStoricoOrdine", ordineId.ToString());
                return Ok(statoAttuale);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore durante il recupero dello stato attuale per ordine {OrdineId}", ordineId);
                return SafeInternalError<StatoStoricoOrdineDTO>("Errore nel recupero dello stato attuale");
            }
        }

        // POST: api/StatoStoricoOrdine
        [HttpPost]
        //[Authorize(Roles = "admin,staff")] // ✅ COMMENTATO PER TEST SWAGGER
        public async Task<ActionResult<StatoStoricoOrdineDTO>> Create([FromBody] StatoStoricoOrdineDTO statoStoricoOrdineDto)
        {
            try
            {
                if (!IsModelValid(statoStoricoOrdineDto))
                    return SafeBadRequest<StatoStoricoOrdineDTO>("Dati stato storico ordine non validi");

                await _repository.AddAsync(statoStoricoOrdineDto);

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
                return SafeInternalError<StatoStoricoOrdineDTO>("Errore nella creazione dello stato storico");
            }
        }

        // PUT: api/StatoStoricoOrdine/5
        [HttpPut("{statoStoricoOrdineId}")]
        //[Authorize(Roles = "admin,staff")] // ✅ COMMENTATO PER TEST SWAGGER
        public async Task<ActionResult> Update(int statoStoricoOrdineId, [FromBody] StatoStoricoOrdineDTO statoStoricoOrdineDto)
        {
            try
            {
                if (statoStoricoOrdineId <= 0)
                    return SafeBadRequest("ID stato storico ordine non valido");

                if (statoStoricoOrdineId != statoStoricoOrdineDto.StatoStoricoOrdineId)
                    return SafeBadRequest("Identificativi non corrispondenti");

                if (!IsModelValid(statoStoricoOrdineDto))
                    return SafeBadRequest("Dati stato storico ordine non validi");

                var existing = await _repository.GetByIdAsync(statoStoricoOrdineId);
                if (existing == null)
                    return SafeNotFound("Stato storico ordine");

                await _repository.UpdateAsync(statoStoricoOrdineDto);

                LogAuditTrail("UPDATE_STATO_STORICO_ORDINE", "StatoStoricoOrdine", statoStoricoOrdineDto.StatoStoricoOrdineId.ToString());
                LogSecurityEvent("StatoStoricoOrdineUpdated", new
                {
                    StatoStoricoOrdineId = statoStoricoOrdineDto.StatoStoricoOrdineId,
                    OrdineId = statoStoricoOrdineDto.OrdineId,
                    User = User.Identity?.Name ?? "Anonymous"
                });

                return NoContent();
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore durante l'aggiornamento dello stato storico ordine {StatoStoricoOrdineId}", statoStoricoOrdineId);
                return SafeInternalError("Errore nell'aggiornamento dello stato storico");
            }
        }

        // DELETE: api/StatoStoricoOrdine/5
        [HttpDelete("{statoStoricoOrdineId}")]
        //[Authorize(Roles = "admin")] // ✅ COMMENTATO PER TEST SWAGGER
        public async Task<ActionResult> Delete(int statoStoricoOrdineId)
        {
            try
            {
                if (statoStoricoOrdineId <= 0)
                    return SafeBadRequest("ID stato storico ordine non valido");

                var statoStorico = await _repository.GetByIdAsync(statoStoricoOrdineId);
                if (statoStorico == null)
                    return SafeNotFound("Stato storico ordine");

                await _repository.DeleteAsync(statoStoricoOrdineId);

                LogAuditTrail("DELETE_STATO_STORICO_ORDINE", "StatoStoricoOrdine", statoStoricoOrdineId.ToString());
                LogSecurityEvent("StatoStoricoOrdineDeleted", new
                {
                    StatoStoricoOrdineId = statoStoricoOrdineId,
                    User = User.Identity?.Name ?? "Anonymous"
                });

                return NoContent();
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore durante l'eliminazione dello stato storico ordine {StatoStoricoOrdineId}", statoStoricoOrdineId);
                return SafeInternalError("Errore nell'eliminazione dello stato storico");
            }
        }

        // POST: api/StatoStoricoOrdine/ordine/5/chiudi-stato-attuale
        [HttpPost("ordine/{ordineId}/chiudi-stato-attuale")]
        //[Authorize(Roles = "admin,staff")] // ✅ COMMENTATO PER TEST SWAGGER
        public async Task<ActionResult> ChiudiStatoAttuale(int ordineId)
        {
            try
            {
                if (ordineId <= 0)
                    return SafeBadRequest("ID ordine non valido");

                var result = await _repository.ChiudiStatoAttualeAsync(ordineId, DateTime.Now);

                if (!result)
                    return SafeNotFound("Nessuno stato attuale trovato per l'ordine specificato");

                LogAuditTrail("CHIUDI_STATO_ATTUALE_ORDINE", "StatoStoricoOrdine", ordineId.ToString());
                LogSecurityEvent("StatoAttualeChiuso", new
                {
                    OrdineId = ordineId,
                    User = User.Identity?.Name ?? "Anonymous",
                    Timestamp = DateTime.Now
                });

                return NoContent();
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore durante la chiusura dello stato attuale per ordine {OrdineId}", ordineId);
                return SafeInternalError("Errore nella chiusura dello stato attuale");
            }
        }
    }
}