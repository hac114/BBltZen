// BBltZen/Controllers/ConfigSoglieTempiController.cs
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
    public class ConfigSoglieTempiController : SecureBaseController
    {
        private readonly IConfigSoglieTempiRepository _repository;

        public ConfigSoglieTempiController(
            IConfigSoglieTempiRepository repository,
            IWebHostEnvironment environment,
            ILogger<ConfigSoglieTempiController> logger)
            : base(environment, logger)
        {
            _repository = repository;
        }

        // GET: api/ConfigSoglieTempi
        [HttpGet]
        [AllowAnonymous] // ✅ OVERRIDE PER ACCESSO PUBBLICO
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
                return SafeInternalError("Errore durante il recupero delle configurazioni");
            }
        }

        // GET: api/ConfigSoglieTempi/5
        [HttpGet("{id}")]
        [AllowAnonymous] // ✅ OVERRIDE PER ACCESSO PUBBLICO
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
                return SafeInternalError("Errore durante il recupero della configurazione");
            }
        }

        // GET: api/ConfigSoglieTempi/stato-ordine/5
        [HttpGet("stato-ordine/{statoOrdineId}")]
        [AllowAnonymous] // ✅ OVERRIDE PER ACCESSO PUBBLICO
        public async Task<ActionResult<ConfigSoglieTempiDTO>> GetByStatoOrdineId(int statoOrdineId)
        {
            try
            {
                if (statoOrdineId <= 0)
                    return SafeBadRequest<ConfigSoglieTempiDTO>("ID stato ordine non valido");

                var configSoglieTempi = await _repository.GetByStatoOrdineIdAsync(statoOrdineId);

                if (configSoglieTempi == null)
                    return SafeNotFound<ConfigSoglieTempiDTO>("Configurazione soglie tempi per stato ordine");

                return Ok(configSoglieTempi);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore durante il recupero della configurazione per stato ordine {StatoOrdineId}", statoOrdineId);
                return SafeInternalError("Errore durante il recupero della configurazione");
            }
        }

        // POST: api/ConfigSoglieTempi
        [HttpPost]
        //[Authorize(Roles = "admin")] // ✅ COMMENTATO PER TEST CON SWAGGER
        public async Task<ActionResult<ConfigSoglieTempiDTO>> Create(ConfigSoglieTempiDTO configSoglieTempiDto)
        {
            try
            {
                if (!IsModelValid(configSoglieTempiDto))
                    return SafeBadRequest<ConfigSoglieTempiDTO>("Dati configurazione non validi");

                // Verifica se esiste già una configurazione per questo stato ordine
                if (await _repository.ExistsByStatoOrdineIdAsync(configSoglieTempiDto.StatoOrdineId))
                {
                    return SafeBadRequest<ConfigSoglieTempiDTO>("Esiste già una configurazione per questo stato ordine");
                }

                // Imposta l'utente di aggiornamento
                configSoglieTempiDto.UtenteAggiornamento = User.Identity?.Name ?? "System";

                await _repository.AddAsync(configSoglieTempiDto);

                // ✅ Audit trail
                LogAuditTrail("CREATE_CONFIG_SOGLIE_TEMPI", "ConfigSoglieTempi", configSoglieTempiDto.SogliaId.ToString());
                LogSecurityEvent("ConfigSoglieTempiCreated", new
                {
                    SogliaId = configSoglieTempiDto.SogliaId,
                    StatoOrdineId = configSoglieTempiDto.StatoOrdineId,
                    SogliaAttenzione = configSoglieTempiDto.SogliaAttenzione,
                    SogliaCritico = configSoglieTempiDto.SogliaCritico,
                    User = User.Identity?.Name
                });

                return CreatedAtAction(nameof(GetById),
                    new { id = configSoglieTempiDto.SogliaId },
                    configSoglieTempiDto);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore durante la creazione della configurazione soglie tempi");
                return SafeInternalError("Errore durante la creazione della configurazione");
            }
        }

        // PUT: api/ConfigSoglieTempi/5
        [HttpPut("{id}")]
        //[Authorize(Roles = "admin")] // ✅ COMMENTATO PER TEST CON SWAGGER
        public async Task<ActionResult> Update(int id, ConfigSoglieTempiDTO configSoglieTempiDto)
        {
            try
            {
                if (id <= 0)
                    return SafeBadRequest("ID soglia non valido");

                if (id != configSoglieTempiDto.SogliaId)
                    return SafeBadRequest("ID soglia non corrispondente");

                if (!IsModelValid(configSoglieTempiDto))
                    return SafeBadRequest("Dati configurazione non validi");

                var existing = await _repository.GetByIdAsync(id);
                if (existing == null)
                    return SafeNotFound("Configurazione soglie tempi");

                // Verifica se esiste già un'altra configurazione per questo stato ordine
                if (await _repository.ExistsByStatoOrdineIdAsync(configSoglieTempiDto.StatoOrdineId, id))
                {
                    return SafeBadRequest("Esiste già un'altra configurazione per questo stato ordine");
                }

                // Imposta l'utente di aggiornamento
                configSoglieTempiDto.UtenteAggiornamento = User.Identity?.Name ?? "System";

                await _repository.UpdateAsync(configSoglieTempiDto);

                // ✅ Audit trail
                LogAuditTrail("UPDATE_CONFIG_SOGLIE_TEMPI", "ConfigSoglieTempi", configSoglieTempiDto.SogliaId.ToString());
                LogSecurityEvent("ConfigSoglieTempiUpdated", new
                {
                    SogliaId = configSoglieTempiDto.SogliaId,
                    StatoOrdineId = configSoglieTempiDto.StatoOrdineId,
                    User = User.Identity?.Name
                });

                return NoContent();
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore durante l'aggiornamento della configurazione soglie tempi {Id}", id);
                return SafeInternalError("Errore durante l'aggiornamento della configurazione");
            }
        }

        // DELETE: api/ConfigSoglieTempi/5
        [HttpDelete("{id}")]
        //[Authorize(Roles = "admin")] // ✅ COMMENTATO PER TEST CON SWAGGER
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

                // ✅ Audit trail
                LogAuditTrail("DELETE_CONFIG_SOGLIE_TEMPI", "ConfigSoglieTempi", id.ToString());
                LogSecurityEvent("ConfigSoglieTempiDeleted", new
                {
                    SogliaId = id,
                    User = User.Identity?.Name
                });

                return NoContent();
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore durante l'eliminazione della configurazione soglie tempi {Id}", id);
                return SafeInternalError("Errore durante l'eliminazione della configurazione");
            }
        }
    }
}