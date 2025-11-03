using Microsoft.AspNetCore.Mvc;
using DTO;
using Repository.Interface;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace BBltZen.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    //[Authorize] // ✅ COMMENTATO PER TEST CON SWAGGER
    public class StatoOrdineController : SecureBaseController
    {
        private readonly IStatoOrdineRepository _repository;

        public StatoOrdineController(
            IStatoOrdineRepository repository,
            IWebHostEnvironment environment,
            ILogger<StatoOrdineController> logger)
            : base(environment, logger)
        {
            _repository = repository;
        }

        // GET: api/StatoOrdine
        [HttpGet]
        [AllowAnonymous] // ✅ PERMESSO A TUTTI PER TEST
        public async Task<ActionResult<IEnumerable<StatoOrdineDTO>>> GetAll()
        {
            try
            {
                var result = await _repository.GetAllAsync();
                return Ok(result);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore durante il recupero di tutti gli stati ordine");
                return SafeInternalError("Errore durante il recupero degli stati ordine");
            }
        }

        // GET: api/StatoOrdine/5
        [HttpGet("{statoOrdineId}")]
        [AllowAnonymous] // ✅ PERMESSO A TUTTI PER TEST
        public async Task<ActionResult<StatoOrdineDTO>> GetById(int statoOrdineId)
        {
            try
            {
                if (statoOrdineId <= 0)
                    return SafeBadRequest<StatoOrdineDTO>("ID stato ordine non valido");

                var result = await _repository.GetByIdAsync(statoOrdineId);

                if (result == null)
                    return SafeNotFound<StatoOrdineDTO>("Stato ordine");

                return Ok(result);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore durante il recupero dello stato ordine {StatoOrdineId}", statoOrdineId);
                return SafeInternalError("Errore durante il recupero dello stato ordine");
            }
        }

        // GET: api/StatoOrdine/nome/In%20Preparazione
        [HttpGet("nome/{nomeStatoOrdine}")]
        [AllowAnonymous] // ✅ PERMESSO A TUTTI PER TEST
        public async Task<ActionResult<StatoOrdineDTO>> GetByNome(string nomeStatoOrdine)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(nomeStatoOrdine))
                    return SafeBadRequest<StatoOrdineDTO>("Nome stato ordine non valido");

                var result = await _repository.GetByNomeAsync(nomeStatoOrdine);

                if (result == null)
                    return SafeNotFound<StatoOrdineDTO>("Stato ordine con il nome specificato");

                return Ok(result);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore durante il recupero dello stato ordine per nome {NomeStatoOrdine}", nomeStatoOrdine);
                return SafeInternalError("Errore durante il recupero dello stato ordine per nome");
            }
        }

        // GET: api/StatoOrdine/terminali
        [HttpGet("terminali")]
        [AllowAnonymous] // ✅ PERMESSO A TUTTI PER TEST
        public async Task<ActionResult<IEnumerable<StatoOrdineDTO>>> GetStatiTerminali()
        {
            try
            {
                var result = await _repository.GetStatiTerminaliAsync();
                return Ok(result);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore durante il recupero degli stati ordine terminali");
                return SafeInternalError("Errore durante il recupero degli stati ordine terminali");
            }
        }

        // GET: api/StatoOrdine/non-terminali
        [HttpGet("non-terminali")]
        [AllowAnonymous] // ✅ PERMESSO A TUTTI PER TEST
        public async Task<ActionResult<IEnumerable<StatoOrdineDTO>>> GetStatiNonTerminali()
        {
            try
            {
                var result = await _repository.GetStatiNonTerminaliAsync();
                return Ok(result);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore durante il recupero degli stati ordine non terminali");
                return SafeInternalError("Errore durante il recupero degli stati ordine non terminali");
            }
        }

        // POST: api/StatoOrdine
        [HttpPost]
        //[Authorize(Roles = "admin")] // ✅ COMMENTATO PER TEST
        [AllowAnonymous] // ✅ TEMPORANEAMENTE PERMESSO A TUTTI PER TEST
        public async Task<ActionResult<StatoOrdineDTO>> Create(StatoOrdineDTO statoOrdineDto)
        {
            try
            {
                if (!IsModelValid(statoOrdineDto))
                    return SafeBadRequest<StatoOrdineDTO>("Dati stato ordine non validi");

                // Verifica se esiste già uno stato ordine con lo stesso ID
                if (statoOrdineDto.StatoOrdineId > 0 && await _repository.ExistsAsync(statoOrdineDto.StatoOrdineId))
                    return Conflict($"Esiste già uno stato ordine con ID {statoOrdineDto.StatoOrdineId}");

                // Verifica se esiste già uno stato ordine con lo stesso nome
                var existingByName = await _repository.GetByNomeAsync(statoOrdineDto.StatoOrdine1);
                if (existingByName != null)
                    return Conflict($"Esiste già uno stato ordine con il nome '{statoOrdineDto.StatoOrdine1}'");

                await _repository.AddAsync(statoOrdineDto);

                // ✅ Audit trail
                LogAuditTrail("CREATE_STATO_ORDINE", "StatoOrdine", statoOrdineDto.StatoOrdineId.ToString());
                LogSecurityEvent("StatoOrdineCreated", new
                {
                    StatoOrdineId = statoOrdineDto.StatoOrdineId,
                    Nome = statoOrdineDto.StatoOrdine1,
                    Terminale = statoOrdineDto.Terminale,
                    User = User.Identity?.Name
                });

                return CreatedAtAction(nameof(GetById),
                    new { statoOrdineId = statoOrdineDto.StatoOrdineId },
                    statoOrdineDto);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore durante la creazione dello stato ordine");
                return SafeInternalError("Errore durante la creazione dello stato ordine");
            }
        }

        // PUT: api/StatoOrdine/5
        [HttpPut("{statoOrdineId}")]
        //[Authorize(Roles = "admin")] // ✅ COMMENTATO PER TEST
        [AllowAnonymous] // ✅ TEMPORANEAMENTE PERMESSO A TUTTI PER TEST
        public async Task<ActionResult> Update(int statoOrdineId, StatoOrdineDTO statoOrdineDto)
        {
            try
            {
                if (statoOrdineId <= 0)
                    return SafeBadRequest("ID stato ordine non valido");

                if (statoOrdineId != statoOrdineDto.StatoOrdineId)
                    return SafeBadRequest("ID stato ordine non corrispondente");

                if (!IsModelValid(statoOrdineDto))
                    return SafeBadRequest("Dati stato ordine non validi");

                var existing = await _repository.GetByIdAsync(statoOrdineId);
                if (existing == null)
                    return SafeNotFound("Stato ordine");

                // Verifica se esiste già un altro stato ordine con lo stesso nome (escludendo il corrente)
                var existingByName = await _repository.GetByNomeAsync(statoOrdineDto.StatoOrdine1);
                if (existingByName != null && existingByName.StatoOrdineId != statoOrdineId)
                    return Conflict($"Esiste già un altro stato ordine con il nome '{statoOrdineDto.StatoOrdine1}'");

                await _repository.UpdateAsync(statoOrdineDto);

                // ✅ Audit trail
                LogAuditTrail("UPDATE_STATO_ORDINE", "StatoOrdine", statoOrdineDto.StatoOrdineId.ToString());
                LogSecurityEvent("StatoOrdineUpdated", new
                {
                    StatoOrdineId = statoOrdineDto.StatoOrdineId,
                    Nome = statoOrdineDto.StatoOrdine1,
                    User = User.Identity?.Name
                });

                return NoContent();
            }
            catch (System.ArgumentException ex)
            {
                _logger.LogWarning(ex, "Tentativo di aggiornamento di uno stato ordine non trovato {StatoOrdineId}", statoOrdineId);
                return SafeNotFound("Stato ordine");
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore durante l'aggiornamento dello stato ordine {StatoOrdineId}", statoOrdineId);
                return SafeInternalError("Errore durante l'aggiornamento dello stato ordine");
            }
        }

        // DELETE: api/StatoOrdine/5
        [HttpDelete("{statoOrdineId}")]
        //[Authorize(Roles = "admin")] // ✅ COMMENTATO PER TEST
        [AllowAnonymous] // ✅ TEMPORANEAMENTE PERMESSO A TUTTI PER TEST
        public async Task<ActionResult> Delete(int statoOrdineId)
        {
            try
            {
                if (statoOrdineId <= 0)
                    return SafeBadRequest("ID stato ordine non valido");

                var existing = await _repository.GetByIdAsync(statoOrdineId);
                if (existing == null)
                    return SafeNotFound("Stato ordine");

                await _repository.DeleteAsync(statoOrdineId);

                // ✅ Audit trail
                LogAuditTrail("DELETE_STATO_ORDINE", "StatoOrdine", statoOrdineId.ToString());
                LogSecurityEvent("StatoOrdineDeleted", new
                {
                    StatoOrdineId = statoOrdineId,
                    User = User.Identity?.Name
                });

                return NoContent();
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore durante l'eliminazione dello stato ordine {StatoOrdineId}", statoOrdineId);
                return SafeInternalError("Errore durante l'eliminazione dello stato ordine");
            }
        }
    }
}