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
    [AllowAnonymous] // ✅ AGGIUNTO
    public class PersonalizzazioneCustomController : SecureBaseController
    {
        private readonly IPersonalizzazioneCustomRepository _repository;
        private readonly BubbleTeaContext _context; // ✅ AGGIUNTO

        public PersonalizzazioneCustomController(
            IPersonalizzazioneCustomRepository repository,
            BubbleTeaContext context, // ✅ AGGIUNTO
            IWebHostEnvironment environment,
            ILogger<PersonalizzazioneCustomController> logger)
            : base(environment, logger)
        {
            _repository = repository;
            _context = context; // ✅ AGGIUNTO
        }

        // GET: api/PersonalizzazioneCustom
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<PersonalizzazioneCustomDTO>>> GetAll()
        {
            try
            {
                var result = await _repository.GetAllAsync();
                return Ok(result);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore durante il recupero di tutte le personalizzazioni custom");
                return SafeInternalError("Errore durante il recupero delle personalizzazioni");
            }
        }

        // GET: api/PersonalizzazioneCustom/5
        [HttpGet("{persCustomId}")]
        [AllowAnonymous]
        public async Task<ActionResult<PersonalizzazioneCustomDTO>> GetById(int persCustomId)
        {
            try
            {
                if (persCustomId <= 0)
                    return SafeBadRequest<PersonalizzazioneCustomDTO>("ID personalizzazione non valido");

                var result = await _repository.GetByIdAsync(persCustomId);

                if (result == null)
                    return SafeNotFound<PersonalizzazioneCustomDTO>("Personalizzazione custom");

                return Ok(result);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore durante il recupero della personalizzazione custom {PersCustomId}", persCustomId);
                return SafeInternalError("Errore durante il recupero della personalizzazione");
            }
        }

        // GET: api/PersonalizzazioneCustom/dimensione-bicchiere/5
        [HttpGet("dimensione-bicchiere/{dimensioneBicchiereId}")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<PersonalizzazioneCustomDTO>>> GetByDimensioneBicchiere(int dimensioneBicchiereId)
        {
            try
            {
                if (dimensioneBicchiereId <= 0)
                    return SafeBadRequest<IEnumerable<PersonalizzazioneCustomDTO>>("ID dimensione bicchiere non valido");

                var result = await _repository.GetByDimensioneBicchiereAsync(dimensioneBicchiereId);
                return Ok(result);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore durante il recupero delle personalizzazioni per dimensione bicchiere {DimensioneBicchiereId}", dimensioneBicchiereId);
                return SafeInternalError("Errore durante il recupero delle personalizzazioni");
            }
        }

        // GET: api/PersonalizzazioneCustom/grado-dolcezza/3
        [HttpGet("grado-dolcezza/{gradoDolcezza}")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<PersonalizzazioneCustomDTO>>> GetByGradoDolcezza(byte gradoDolcezza)
        {
            try
            {
                if (gradoDolcezza <= 0 || gradoDolcezza > 5)
                    return SafeBadRequest<IEnumerable<PersonalizzazioneCustomDTO>>("Grado dolcezza non valido (deve essere tra 1 e 5)");

                var result = await _repository.GetByGradoDolcezzaAsync(gradoDolcezza);
                return Ok(result);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore durante il recupero delle personalizzazioni per grado dolcezza {GradoDolcezza}", gradoDolcezza);
                return SafeInternalError("Errore durante il recupero delle personalizzazioni");
            }
        }

        // POST: api/PersonalizzazioneCustom
        [HttpPost]
        //[Authorize(Roles = "admin,barista")]
        [AllowAnonymous]
        public async Task<ActionResult<PersonalizzazioneCustomDTO>> Create([FromBody] PersonalizzazioneCustomDTO personalizzazioneCustomDto) // ✅ AGGIUNTO [FromBody]
        {
            try
            {
                if (!IsModelValid(personalizzazioneCustomDto))
                    return SafeBadRequest<PersonalizzazioneCustomDTO>("Dati personalizzazione non validi");

                await _repository.AddAsync(personalizzazioneCustomDto);

                LogAuditTrail("CREATE_PERSONALIZZAZIONE_CUSTOM", "PersonalizzazioneCustom", personalizzazioneCustomDto.PersCustomId.ToString());
                LogSecurityEvent("PersonalizzazioneCustomCreated", new
                {
                    PersCustomId = personalizzazioneCustomDto.PersCustomId,
                    Nome = personalizzazioneCustomDto.Nome,
                    GradoDolcezza = personalizzazioneCustomDto.GradoDolcezza,
                    User = User.Identity?.Name,
                    Timestamp = DateTime.UtcNow // ✅ AGGIUNTO
                });

                return CreatedAtAction(nameof(GetById),
                    new { persCustomId = personalizzazioneCustomDto.PersCustomId },
                    personalizzazioneCustomDto);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore durante la creazione della personalizzazione custom");
                return SafeInternalError("Errore durante la creazione della personalizzazione");
            }
        }

        // PUT: api/PersonalizzazioneCustom/5
        [HttpPut("{persCustomId}")]
        //[Authorize(Roles = "admin,barista")]
        [AllowAnonymous]
        public async Task<ActionResult> Update(int persCustomId, [FromBody] PersonalizzazioneCustomDTO personalizzazioneCustomDto) // ✅ AGGIUNTO [FromBody]
        {
            try
            {
                if (persCustomId <= 0)
                    return SafeBadRequest("ID personalizzazione non valido");

                if (persCustomId != personalizzazioneCustomDto.PersCustomId)
                    return SafeBadRequest("ID personalizzazione non corrispondente");

                if (!IsModelValid(personalizzazioneCustomDto))
                    return SafeBadRequest("Dati personalizzazione non validi");

                var existing = await _repository.GetByIdAsync(persCustomId);
                if (existing == null)
                    return SafeNotFound("Personalizzazione custom");

                await _repository.UpdateAsync(personalizzazioneCustomDto);

                LogAuditTrail("UPDATE_PERSONALIZZAZIONE_CUSTOM", "PersonalizzazioneCustom", personalizzazioneCustomDto.PersCustomId.ToString());
                LogSecurityEvent("PersonalizzazioneCustomUpdated", new
                {
                    PersCustomId = personalizzazioneCustomDto.PersCustomId,
                    User = User.Identity?.Name,
                    Timestamp = DateTime.UtcNow // ✅ AGGIUNTO
                });

                return NoContent();
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore durante l'aggiornamento della personalizzazione custom {PersCustomId}", persCustomId);
                return SafeInternalError("Errore durante l'aggiornamento della personalizzazione");
            }
        }

        // DELETE: api/PersonalizzazioneCustom/5
        [HttpDelete("{persCustomId}")]
        //[Authorize(Roles = "admin")]
        [AllowAnonymous]
        public async Task<ActionResult> Delete(int persCustomId)
        {
            try
            {
                if (persCustomId <= 0)
                    return SafeBadRequest("ID personalizzazione non valido");

                var existing = await _repository.GetByIdAsync(persCustomId);
                if (existing == null)
                    return SafeNotFound("Personalizzazione custom");

                // ✅ AGGIUNTO: CONTROLLO DIPENDENZE
                var hasBevandeCustom = await _context.BevandaCustom
                    .AnyAsync(bc => bc.PersCustomId == persCustomId);

                if (hasBevandeCustom)
                    return SafeBadRequest("Impossibile eliminare: la personalizzazione è utilizzata in bevande custom");

                await _repository.DeleteAsync(persCustomId);

                LogAuditTrail("DELETE_PERSONALIZZAZIONE_CUSTOM", "PersonalizzazioneCustom", persCustomId.ToString());
                LogSecurityEvent("PersonalizzazioneCustomDeleted", new
                {
                    PersCustomId = persCustomId,
                    User = User.Identity?.Name,
                    Timestamp = DateTime.UtcNow // ✅ AGGIUNTO
                });

                return NoContent();
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore durante l'eliminazione della personalizzazione custom {PersCustomId}", persCustomId);
                return SafeInternalError("Errore durante l'eliminazione della personalizzazione");
            }
        }
    }
}