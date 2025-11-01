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
    [AllowAnonymous] // ✅ OVERRIDE DELL'[Authorize] DEL BASE CONTROLLER
    public class PersonalizzazioneController : SecureBaseController
    {
        private readonly IPersonalizzazioneRepository _personalizzazioneRepository;

        public PersonalizzazioneController(
            IPersonalizzazioneRepository personalizzazioneRepository,
            IWebHostEnvironment environment,
            ILogger<PersonalizzazioneController> logger)
            : base(environment, logger)
        {
            _personalizzazioneRepository = personalizzazioneRepository;
        }

        // GET: api/Personalizzazione
        //[Authorize(Roles = "admin")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PersonalizzazioneDTO>>> GetAll()
        {
            try
            {
                var personalizzazioni = await _personalizzazioneRepository.GetAllAsync();
                return Ok(personalizzazioni);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore durante il recupero di tutte le personalizzazioni");
                return SafeInternalError("Errore durante il recupero delle personalizzazioni");
            }
        }

        // GET: api/Personalizzazione/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<PersonalizzazioneDTO>> GetById(int id)
        {
            try
            {
                if (id <= 0)
                    return SafeBadRequest<PersonalizzazioneDTO>("ID personalizzazione non valido");

                var personalizzazione = await _personalizzazioneRepository.GetByIdAsync(id);

                if (personalizzazione == null)
                    return SafeNotFound<PersonalizzazioneDTO>("Personalizzazione");

                return Ok(personalizzazione);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore durante il recupero della personalizzazione con ID {Id}", id);
                return SafeInternalError("Errore durante il recupero della personalizzazione");
            }
        }

        // POST: api/Personalizzazione
        //[Authorize(Roles = "admin")]
        [HttpPost]
        public async Task<ActionResult<PersonalizzazioneDTO>> Create(PersonalizzazioneDTO personalizzazioneDto)
        {
            try
            {
                if (!IsModelValid(personalizzazioneDto))
                    return SafeBadRequest<PersonalizzazioneDTO>("Dati personalizzazione non validi");

                // Verifica se esiste già una personalizzazione con lo stesso nome
                var exists = await _personalizzazioneRepository.ExistsByNameAsync(personalizzazioneDto.Nome);
                if (exists)
                {
                    return SafeBadRequest<PersonalizzazioneDTO>($"Esiste già una personalizzazione con il nome '{personalizzazioneDto.Nome}'");
                }

                await _personalizzazioneRepository.AddAsync(personalizzazioneDto);

                // ✅ Audit trail
                LogAuditTrail("CREATE_PERSONALIZZAZIONE", "Personalizzazione", personalizzazioneDto.PersonalizzazioneId.ToString());
                LogSecurityEvent("PersonalizzazioneCreated", new
                {
                    PersonalizzazioneId = personalizzazioneDto.PersonalizzazioneId,
                    Nome = personalizzazioneDto.Nome
                });

                return CreatedAtAction(nameof(GetById), new { id = personalizzazioneDto.PersonalizzazioneId }, personalizzazioneDto);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore durante la creazione della personalizzazione: {Nome}", personalizzazioneDto.Nome);
                return SafeInternalError("Errore durante la creazione della personalizzazione");
            }
        }

        // PUT: api/Personalizzazione/{id}
        //[Authorize(Roles = "admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, PersonalizzazioneDTO personalizzazioneDto)
        {
            try
            {
                if (id <= 0)
                    return SafeBadRequest("ID personalizzazione non valido");

                if (id != personalizzazioneDto.PersonalizzazioneId)
                    return SafeBadRequest("ID personalizzazione non corrispondente");

                if (!IsModelValid(personalizzazioneDto))
                    return SafeBadRequest("Dati personalizzazione non validi");

                // Verifica se la personalizzazione esiste
                var existingPersonalizzazione = await _personalizzazioneRepository.GetByIdAsync(id);
                if (existingPersonalizzazione == null)
                {
                    return SafeNotFound("Personalizzazione");
                }

                // Verifica se esiste già un'altra personalizzazione con lo stesso nome
                var existingByName = await _personalizzazioneRepository.ExistsByNameAsync(personalizzazioneDto.Nome);
                if (existingByName && existingPersonalizzazione.Nome != personalizzazioneDto.Nome)
                {
                    return SafeBadRequest($"Esiste già una personalizzazione con il nome '{personalizzazioneDto.Nome}'");
                }

                await _personalizzazioneRepository.UpdateAsync(personalizzazioneDto);

                // ✅ Audit trail
                LogAuditTrail("UPDATE_PERSONALIZZAZIONE", "Personalizzazione", personalizzazioneDto.PersonalizzazioneId.ToString());
                LogSecurityEvent("PersonalizzazioneUpdated", new
                {
                    PersonalizzazioneId = personalizzazioneDto.PersonalizzazioneId,
                    Nome = personalizzazioneDto.Nome,
                    User = User.Identity?.Name
                });

                return NoContent();
            }
            catch (System.ArgumentException ex)
            {
                _logger.LogWarning(ex, "Personalizzazione non trovata durante l'aggiornamento");
                return SafeNotFound("Personalizzazione");
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore durante l'aggiornamento della personalizzazione con ID {Id}", id);
                return SafeInternalError("Errore durante l'aggiornamento della personalizzazione");
            }
        }

        // GET: api/Personalizzazione/exists/{id}
        [HttpGet("exists/{id}")]
        public async Task<ActionResult<bool>> Exists(int id)
        {
            try
            {
                if (id <= 0)
                    return SafeBadRequest<bool>("ID personalizzazione non valido");

                var exists = await _personalizzazioneRepository.ExistsAsync(id);
                return Ok(exists);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore durante la verifica esistenza personalizzazione con ID {Id}", id);
                return SafeInternalError("Errore durante la verifica esistenza personalizzazione");
            }
        }

        // GET: api/Personalizzazione/exists/name/NomePersonalizzazione
        [HttpGet("exists/name/{nome}")]
        public async Task<ActionResult<bool>> ExistsByName(string nome)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(nome))
                    return SafeBadRequest<bool>("Nome personalizzazione non valido");

                var exists = await _personalizzazioneRepository.ExistsByNameAsync(nome);
                return Ok(exists);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore durante la verifica esistenza personalizzazione con nome {Nome}", nome);
                return SafeInternalError("Errore durante la verifica esistenza personalizzazione");
            }
        }

        // DELETE: api/Personalizzazione/{id}
        //[Authorize(Roles = "admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                if (id <= 0)
                    return SafeBadRequest("ID personalizzazione non valido");

                var existingPersonalizzazione = await _personalizzazioneRepository.GetByIdAsync(id);
                if (existingPersonalizzazione == null)
                {
                    return SafeNotFound("Personalizzazione");
                }

                await _personalizzazioneRepository.DeleteAsync(id);

                // ✅ Audit trail
                LogAuditTrail("HARD_DELETE_PERSONALIZZAZIONE", "Personalizzazione", id.ToString());
                LogSecurityEvent("PersonalizzazioneHardDeleted", new
                {
                    PersonalizzazioneId = id,
                    Nome = existingPersonalizzazione.Nome,
                    User = User.Identity?.Name
                });

                return NoContent();
            }
            catch (System.InvalidOperationException ex)
            {
                // ✅ Gestione specifica per dipendenze
                _logger.LogWarning(ex, "Tentativo di eliminazione personalizzazione {Id} con dipendenze", id);
                return SafeBadRequest(ex.Message);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore durante l'eliminazione definitiva della personalizzazione con ID {Id}", id);
                return SafeInternalError("Errore durante l'eliminazione della personalizzazione");
            }
        }
    }
}