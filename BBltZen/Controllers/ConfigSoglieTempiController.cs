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
        [Authorize(Roles = "admin")] // ✅ Solo admin può creare configurazioni
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

                // ✅ Validazione business: soglia attenzione deve essere minore di soglia critico
                if (configSoglieTempiDto.SogliaAttenzione >= configSoglieTempiDto.SogliaCritico)
                    return SafeBadRequest<ConfigSoglieTempiDTO>("La soglia attenzione deve essere inferiore alla soglia critico");

                // ✅ Verifica se esiste già una configurazione per questo stato ordine
                if (await _repository.ExistsByStatoOrdineIdAsync(configSoglieTempiDto.StatoOrdineId))
                    return SafeBadRequest<ConfigSoglieTempiDTO>("Esiste già una configurazione per questo stato ordine");

                // ✅ Imposta l'utente di aggiornamento
                configSoglieTempiDto.UtenteAggiornamento = User.Identity?.Name ?? "System";

                await _repository.AddAsync(configSoglieTempiDto);

                // ✅ Audit trail
                LogAuditTrail("CREATE_CONFIG_SOGLIE_TEMPI", "ConfigSoglieTempi", configSoglieTempiDto.SogliaId.ToString());

                // ✅ Security event completo con timestamp
                LogSecurityEvent("ConfigSoglieTempiCreated", new
                {
                    SogliaId = configSoglieTempiDto.SogliaId,
                    StatoOrdineId = configSoglieTempiDto.StatoOrdineId,
                    SogliaAttenzione = configSoglieTempiDto.SogliaAttenzione,
                    SogliaCritico = configSoglieTempiDto.SogliaCritico,
                    User = User.Identity?.Name,
                    Timestamp = DateTime.UtcNow
                });

                return CreatedAtAction(nameof(GetById),
                    new { id = configSoglieTempiDto.SogliaId },
                    configSoglieTempiDto);
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "Errore database durante la creazione della configurazione soglie tempi");
                return SafeInternalError<ConfigSoglieTempiDTO>("Errore durante il salvataggio della configurazione");
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore durante la creazione della configurazione soglie tempi");
                return SafeInternalError<ConfigSoglieTempiDTO>("Errore durante la creazione della configurazione");
            }
        }

        // PUT: api/ConfigSoglieTempi/5
        [HttpPut("{id}")]
        [Authorize(Roles = "admin")] // ✅ Solo admin può modificare configurazioni
        public async Task<ActionResult> Update(int id, [FromBody] ConfigSoglieTempiDTO configSoglieTempiDto)
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

                // ✅ Verifica se lo stato ordine esiste
                var statoOrdineEsiste = await _context.StatoOrdine.AnyAsync(s => s.StatoOrdineId == configSoglieTempiDto.StatoOrdineId);
                if (!statoOrdineEsiste)
                    return SafeBadRequest("Stato ordine non trovato");

                // ✅ Validazione business: soglia attenzione deve essere minore di soglia critico
                if (configSoglieTempiDto.SogliaAttenzione >= configSoglieTempiDto.SogliaCritico)
                    return SafeBadRequest("La soglia attenzione deve essere inferiore alla soglia critico");

                // ✅ Verifica se esiste già un'altra configurazione per questo stato ordine
                if (await _repository.ExistsByStatoOrdineIdAsync(configSoglieTempiDto.StatoOrdineId, id))
                    return SafeBadRequest("Esiste già un'altra configurazione per questo stato ordine");

                // ✅ Imposta l'utente di aggiornamento
                configSoglieTempiDto.UtenteAggiornamento = User.Identity?.Name ?? "System";

                await _repository.UpdateAsync(configSoglieTempiDto);

                // ✅ Audit trail
                LogAuditTrail("UPDATE_CONFIG_SOGLIE_TEMPI", "ConfigSoglieTempi", configSoglieTempiDto.SogliaId.ToString());

                // ✅ Security event completo con timestamp
                LogSecurityEvent("ConfigSoglieTempiUpdated", new
                {
                    SogliaId = configSoglieTempiDto.SogliaId,
                    StatoOrdineId = configSoglieTempiDto.StatoOrdineId,
                    OldSogliaAttenzione = existing.SogliaAttenzione,
                    NewSogliaAttenzione = configSoglieTempiDto.SogliaAttenzione,
                    OldSogliaCritico = existing.SogliaCritico,
                    NewSogliaCritico = configSoglieTempiDto.SogliaCritico,
                    User = User.Identity?.Name,
                    Timestamp = DateTime.UtcNow
                });

                return NoContent();
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Configurazione non trovata durante l'aggiornamento {Id}", id);
                return SafeNotFound("Configurazione soglie tempi");
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "Errore database durante l'aggiornamento della configurazione soglie tempi {Id}", id);
                return SafeInternalError("Errore durante l'aggiornamento della configurazione");
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore durante l'aggiornamento della configurazione soglie tempi {Id}", id);
                return SafeInternalError("Errore durante l'aggiornamento della configurazione");
            }
        }

        // DELETE: api/ConfigSoglieTempi/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "admin")] // ✅ Solo admin può eliminare configurazioni
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

                // ✅ Security event completo con timestamp
                LogSecurityEvent("ConfigSoglieTempiDeleted", new
                {
                    SogliaId = id,
                    StatoOrdineId = configSoglieTempi.StatoOrdineId,
                    User = User.Identity?.Name,
                    Timestamp = DateTime.UtcNow
                });

                return NoContent();
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "Errore database durante l'eliminazione della configurazione soglie tempi {Id}", id);
                return SafeInternalError("Errore durante l'eliminazione della configurazione");
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore durante l'eliminazione della configurazione soglie tempi {Id}", id);
                return SafeInternalError("Errore durante l'eliminazione della configurazione");
            }
        }
    }
}