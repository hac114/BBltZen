using Microsoft.AspNetCore.Mvc;
using DTO;
using Repository.Interface;
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
    public class ConfigSoglieTempiController : SecureBaseController
    {
        private readonly IConfigSoglieTempiRepository _repository;
        private readonly BubbleTeaContext _context;

        public ConfigSoglieTempiController(
            IConfigSoglieTempiRepository repository,
            BubbleTeaContext context,
            IWebHostEnvironment environment,
            ILogger<ConfigSoglieTempiController> logger)
            : base(environment, logger)
        {
            _repository = repository;
            _context = context;
        }

        // GET: api/ConfigSoglieTempi
        [HttpGet]
        [AllowAnonymous] // ✅ ESPLICITO PER ENDPOINT GET
        public async Task<ActionResult<IEnumerable<ConfigSoglieTempiDTO>>> GetAll()
        {
            try
            {
                var configSoglieTempi = await _repository.GetAllAsync();
                return Ok(configSoglieTempi);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore durante il recupero di tutte le configurazioni soglie tempi");
                return SafeInternalError<IEnumerable<ConfigSoglieTempiDTO>>("Errore durante il recupero delle configurazioni");
            }
        }

        // GET: api/ConfigSoglieTempi/5
        [HttpGet("{id}")]
        [AllowAnonymous] // ✅ ESPLICITO PER ENDPOINT GET
        public async Task<ActionResult<ConfigSoglieTempiDTO>> GetById(int id)
        {
            try
            {
                if (id <= 0)
                    return SafeBadRequest<ConfigSoglieTempiDTO>("ID soglia non valido");

                var configSoglieTempi = await _repository.GetByIdAsync(id);

                if (configSoglieTempi == null)
                    return SafeNotFound<ConfigSoglieTempiDTO>("Configurazione soglie tempi");

                return Ok(configSoglieTempi);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore durante il recupero della configurazione soglie tempi {Id}", id);
                return SafeInternalError<ConfigSoglieTempiDTO>("Errore durante il recupero della configurazione");
            }
        }

        // GET: api/ConfigSoglieTempi/stato-ordine/5
        [HttpGet("stato-ordine/{statoOrdineId}")]
        [AllowAnonymous] // ✅ ESPLICITO PER ENDPOINT GET
        public async Task<ActionResult<ConfigSoglieTempiDTO>> GetByStatoOrdineId(int statoOrdineId)
        {
            try
            {
                if (statoOrdineId <= 0)
                    return SafeBadRequest<ConfigSoglieTempiDTO>("ID stato ordine non valido");

                // ✅ Verifica se lo stato ordine esiste
                var statoOrdineEsiste = await _context.StatoOrdine.AnyAsync(s => s.StatoOrdineId == statoOrdineId);
                if (!statoOrdineEsiste)
                    return SafeBadRequest<ConfigSoglieTempiDTO>("Stato ordine non trovato");

                var configSoglieTempi = await _repository.GetByStatoOrdineIdAsync(statoOrdineId);

                if (configSoglieTempi == null)
                    return SafeNotFound<ConfigSoglieTempiDTO>("Configurazione soglie tempi per stato ordine");

                return Ok(configSoglieTempi);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore durante il recupero della configurazione per stato ordine {StatoOrdineId}", statoOrdineId);
                return SafeInternalError<ConfigSoglieTempiDTO>("Errore durante il recupero della configurazione");
            }
        }

        // POST: api/ConfigSoglieTempi
        [HttpPost]
        //[Authorize(Roles = "admin")]
        public async Task<ActionResult<ConfigSoglieTempiDTO>> Create([FromBody] ConfigSoglieTempiDTO configSoglieTempiDto)
        {
            try
            {
                if (!IsModelValid(configSoglieTempiDto))
                    return SafeBadRequest<ConfigSoglieTempiDTO>("Dati configurazione non validi");

                // ✅ Verifica se lo stato ordine esiste
                var statoOrdineEsiste = await _context.StatoOrdine.AnyAsync(s => s.StatoOrdineId == configSoglieTempiDto.StatoOrdineId);
                if (!statoOrdineEsiste)
                    return SafeBadRequest<ConfigSoglieTempiDTO>("Stato ordine non trovato");

                // ✅ Imposta l'utente di aggiornamento
                configSoglieTempiDto.UtenteAggiornamento = User.Identity?.Name ?? "System";

                // ✅ USA IL RISULTATO DI AddAsync - IL REPOSITORY FA TUTTE LE VALIDAZIONI
                var result = await _repository.AddAsync(configSoglieTempiDto);

                // ✅ Audit trail
                LogAuditTrail("CREATE", "ConfigSoglieTempi", result.SogliaId.ToString());
                LogSecurityEvent("ConfigSoglieTempiCreated", new
                {
                    result.SogliaId,
                    result.StatoOrdineId,
                    UserId = GetCurrentUserId()
                });

                return CreatedAtAction(nameof(GetById), new { id = result.SogliaId }, result);
            }
            catch (ArgumentException argEx)
            {
                return SafeBadRequest<ConfigSoglieTempiDTO>(argEx.Message);
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "Errore database durante la creazione");
                return SafeInternalError<ConfigSoglieTempiDTO>("Errore durante il salvataggio");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante la creazione");
                return SafeInternalError<ConfigSoglieTempiDTO>("Errore durante la creazione");
            }
        }

        // PUT: api/ConfigSoglieTempi/5
        [HttpPut("{id}")]
        //[Authorize(Roles = "admin")]
        public async Task<ActionResult> Update(int id, [FromBody] ConfigSoglieTempiDTO configSoglieTempiDto)
        {
            try
            {
                if (id <= 0 || id != configSoglieTempiDto.SogliaId)
                    return SafeBadRequest("ID non valido");

                if (!IsModelValid(configSoglieTempiDto))
                    return SafeBadRequest("Dati configurazione non validi");

                // ✅ Verifica se lo stato ordine esiste
                var statoOrdineEsiste = await _context.StatoOrdine.AnyAsync(s => s.StatoOrdineId == configSoglieTempiDto.StatoOrdineId);
                if (!statoOrdineEsiste)
                    return SafeBadRequest("Stato ordine non trovato");

                // ✅ Imposta l'utente di aggiornamento
                configSoglieTempiDto.UtenteAggiornamento = User.Identity?.Name ?? "System";

                await _repository.UpdateAsync(configSoglieTempiDto);

                // ✅ Audit trail - SILENT FAIL, non verifica esistenza
                LogAuditTrail("UPDATE", "ConfigSoglieTempi", configSoglieTempiDto.SogliaId.ToString());
                LogSecurityEvent("ConfigSoglieTempiUpdated", new
                {
                    configSoglieTempiDto.SogliaId,
                    UserId = GetCurrentUserId()
                });

                return NoContent();
            }
            catch (ArgumentException argEx)
            {
                return SafeBadRequest(argEx.Message);
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "Errore database durante l'aggiornamento {Id}", id);
                return SafeInternalError("Errore durante l'aggiornamento");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante l'aggiornamento {Id}", id);
                return SafeInternalError("Errore durante l'aggiornamento");
            }
        }

        // DELETE: api/ConfigSoglieTempi/5
        [HttpDelete("{id}")]
        //[Authorize(Roles = "admin")]
        public async Task<ActionResult> Delete(int id)
        {
            try
            {
                if (id <= 0)
                    return SafeBadRequest("ID soglia non valido");

                var configSoglieTempi = await _repository.GetByIdAsync(id);
                if (configSoglieTempi == null)
                    return SafeNotFound("Configurazione soglie tempi");

                await _repository.DeleteAsync(id);

                // ✅ Audit trail allineato
                LogAuditTrail("DELETE", "ConfigSoglieTempi", id.ToString());
                LogSecurityEvent("ConfigSoglieTempiDeleted", new
                {
                    SogliaId = id,
                    StatoOrdineId = configSoglieTempi.StatoOrdineId,
                    UserId = GetCurrentUserId()
                });

                return NoContent();
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "Errore database durante l'eliminazione {Id}", id);
                return SafeInternalError("Errore durante l'eliminazione");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante l'eliminazione {Id}", id);
                return SafeInternalError("Errore durante l'eliminazione");
            }
        }

        [HttpGet("stati-ordine")]
        [AllowAnonymous]
        public async Task<ActionResult<Dictionary<int, ConfigSoglieTempiDTO>>> GetSoglieByStatiOrdine([FromQuery] List<int> statiOrdineIds)
        {
            try
            {
                if (statiOrdineIds == null || !statiOrdineIds.Any())
                    return SafeBadRequest<Dictionary<int, ConfigSoglieTempiDTO>>("Lista stati ordine vuota");

                var result = await _repository.GetSoglieByStatiOrdineAsync(statiOrdineIds);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante il recupero soglie per stati ordine");
                return SafeInternalError<Dictionary<int, ConfigSoglieTempiDTO>>("Errore durante il recupero");
            }
        }
        [HttpPost("validate")]
        [AllowAnonymous]
        public async Task<ActionResult<bool>> ValidateSoglie([FromBody] SoglieValidationRequestDTO request)
        {
            try
            {
                if (!IsModelValid(request))
                    return SafeBadRequest<bool>("Dati validazione non validi");

                var result = await _repository.ValidateSoglieAsync(request.SogliaAttenzione, request.SogliaCritico);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante la validazione soglie");
                return SafeInternalError<bool>("Errore durante la validazione");
            }
        }

        [HttpGet("exists/stato-ordine/{statoOrdineId}")]
        [AllowAnonymous]
        public async Task<ActionResult<bool>> ExistsByStatoOrdineId(int statoOrdineId, [FromQuery] int? excludeSogliaId = null)
        {
            try
            {
                if (statoOrdineId <= 0)
                    return SafeBadRequest<bool>("ID stato ordine non valido");

                var result = await _repository.ExistsByStatoOrdineIdAsync(statoOrdineId, excludeSogliaId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante la verifica esistenza per stato ordine {StatoOrdineId}", statoOrdineId);
                return SafeInternalError<bool>("Errore durante la verifica");
            }
        }
    }
}