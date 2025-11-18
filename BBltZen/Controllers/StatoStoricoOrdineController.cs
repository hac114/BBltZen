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

                var result = await _repository.GetByOrdineIdAsync(ordineId);

                LogAuditTrail("GET_BY_ORDINE", "StatoStoricoOrdine", ordineId.ToString());
                LogSecurityEvent("StatoStoricoOrdineGetByOrdine", new
                {
                    ordineId,
                    UserId = GetCurrentUserId(),
                    Count = result.Count()
                });

                return Ok(result);
            }
            catch (Exception ex)
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

                var result = await _repository.GetStatoAttualeOrdineAsync(ordineId);

                if (result == null)
                    return SafeNotFound<StatoStoricoOrdineDTO>("Stato attuale ordine");

                LogAuditTrail("GET_STATO_ATTUALE", "StatoStoricoOrdine", ordineId.ToString());
                LogSecurityEvent("StatoStoricoOrdineGetCurrent", new
                {
                    ordineId,
                    UserId = GetCurrentUserId(),
                    StatoOrdineId = result.StatoOrdineId
                });

                return Ok(result);
            }
            catch (Exception ex)
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

                // ✅ CORREZIONE: Usa il risultato del repository
                var result = await _repository.AddAsync(statoStoricoOrdineDto);

                // ✅ AUDIT OTTIMIZZATO
                LogAuditTrail("CREATE", "StatoStoricoOrdine", result.StatoStoricoOrdineId.ToString());
                LogSecurityEvent("StatoStoricoOrdineCreated", new
                {
                    result.StatoStoricoOrdineId,
                    result.OrdineId,
                    result.StatoOrdineId,
                    UserId = GetCurrentUserId(),
                    UserName = User.Identity?.Name
                });

                return CreatedAtAction(nameof(GetById),
                    new { statoStoricoOrdineId = result.StatoStoricoOrdineId },
                    result);
            }
            catch (ArgumentException argEx)
            {
                return SafeBadRequest<StatoStoricoOrdineDTO>(argEx.Message);
            }
            catch (Exception ex)
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
                if (statoStoricoOrdineId <= 0 || statoStoricoOrdineId != statoStoricoOrdineDto.StatoStoricoOrdineId)
                    return SafeBadRequest("ID stato storico ordine non valido");

                if (!IsModelValid(statoStoricoOrdineDto))
                    return SafeBadRequest("Dati stato storico ordine non validi");

                if (!await _repository.ExistsAsync(statoStoricoOrdineId))
                    return SafeNotFound("Stato storico ordine");

                await _repository.UpdateAsync(statoStoricoOrdineDto);

                // ✅ AUDIT OTTIMIZZATO
                LogAuditTrail("UPDATE", "StatoStoricoOrdine", statoStoricoOrdineId.ToString());
                LogSecurityEvent("StatoStoricoOrdineUpdated", new
                {
                    statoStoricoOrdineId,
                    statoStoricoOrdineDto.OrdineId,
                    UserId = GetCurrentUserId(),
                    UserName = User.Identity?.Name
                });

                return NoContent();
            }
            catch (ArgumentException argEx)
            {
                return SafeBadRequest(argEx.Message);
            }
            catch (Exception ex)
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

                var existing = await _repository.GetByIdAsync(statoStoricoOrdineId);
                if (existing == null)
                    return SafeNotFound("Stato storico ordine");

                await _repository.DeleteAsync(statoStoricoOrdineId);

                // ✅ AUDIT OTTIMIZZATO
                LogAuditTrail("DELETE", "StatoStoricoOrdine", statoStoricoOrdineId.ToString());
                LogSecurityEvent("StatoStoricoOrdineDeleted", new
                {
                    statoStoricoOrdineId,
                    existing.OrdineId,
                    UserId = GetCurrentUserId(),
                    UserName = User.Identity?.Name
                });

                return NoContent();
            }
            catch (Exception ex)
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

        [HttpGet("periodo")]
        [AllowAnonymous] // ✅ ACCESSO PUBBLICO
        public async Task<ActionResult<IEnumerable<StatoStoricoOrdineDTO>>> GetStoricoByPeriodo([FromQuery] DateTime dataInizio, [FromQuery] DateTime dataFine)
        {
            try
            {
                if (dataInizio > dataFine)
                    return SafeBadRequest<IEnumerable<StatoStoricoOrdineDTO>>("Data inizio non può essere successiva alla data fine");

                var result = await _repository.GetStoricoByPeriodoAsync(dataInizio, dataFine);

                LogAuditTrail("GET_STORICO_PERIODO", "StatoStoricoOrdine", $"{dataInizio:yyyy-MM-dd}_{dataFine:yyyy-MM-dd}");
                LogSecurityEvent("StatoStoricoOrdineGetByPeriodo", new
                {
                    dataInizio,
                    dataFine,
                    UserId = GetCurrentUserId(),
                    Count = result.Count()
                });

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante il recupero dello storico per periodo {DataInizio} - {DataFine}", dataInizio, dataFine);
                return SafeInternalError<IEnumerable<StatoStoricoOrdineDTO>>("Errore nel recupero dello storico per periodo");
            }
        }

        [HttpGet("ordine/{ordineId}/has-stato/{statoOrdineId}")]
        [AllowAnonymous] // ✅ ACCESSO PUBBLICO
        public async Task<ActionResult<bool>> OrdineHasStato(int ordineId, int statoOrdineId)
        {
            try
            {
                if (ordineId <= 0 || statoOrdineId <= 0)
                    return SafeBadRequest<bool>("ID ordine o stato ordine non validi");

                var result = await _repository.OrdineHasStatoAsync(ordineId, statoOrdineId);

                LogAuditTrail("CHECK_ORDINE_HAS_STATO", "StatoStoricoOrdine", $"{ordineId}_{statoOrdineId}");
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante la verifica stato per ordine {OrdineId} e stato {StatoOrdineId}", ordineId, statoOrdineId);
                return SafeInternalError<bool>("Errore nella verifica stato ordine");
            }
        }

        [HttpGet("ordine/{ordineId}/numero-stati")]
        [AllowAnonymous] // ✅ ACCESSO PUBBLICO
        public async Task<ActionResult<int>> GetNumeroStatiByOrdine(int ordineId)
        {
            try
            {
                if (ordineId <= 0)
                    return SafeBadRequest<int>("ID ordine non valido");

                var result = await _repository.GetNumeroStatiByOrdineAsync(ordineId);

                LogAuditTrail("GET_NUMERO_STATI_ORDINE", "StatoStoricoOrdine", ordineId.ToString());
                LogSecurityEvent("StatoStoricoOrdineGetCount", new
                {
                    ordineId,
                    UserId = GetCurrentUserId(),
                    Count = result
                });

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante il recupero numero stati per ordine {OrdineId}", ordineId);
                return SafeInternalError<int>("Errore nel recupero numero stati ordine");
            }
        }
    }
}