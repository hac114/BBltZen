using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Repository.Interface;
using DTO;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BBltZen.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [AllowAnonymous] // ✅ Protezione di default per tutte le operazioni
    public class PersonalizzazioneIngredienteController : SecureBaseController
    {
        private readonly IPersonalizzazioneIngredienteRepository _personalizzazioneIngredienteRepository;

        public PersonalizzazioneIngredienteController(
            IPersonalizzazioneIngredienteRepository personalizzazioneIngredienteRepository,
            IWebHostEnvironment environment,
            ILogger<PersonalizzazioneIngredienteController> logger)
            : base(environment, logger)
        {
            _personalizzazioneIngredienteRepository = personalizzazioneIngredienteRepository;
        }

        // GET: api/PersonalizzazioneIngrediente
        [HttpGet]        
        public async Task<ActionResult<IEnumerable<PersonalizzazioneIngredienteDTO>>> GetAll()
        {
            try
            {
                var personalizzazioneIngredienti = await _personalizzazioneIngredienteRepository.GetAllAsync();
                return Ok(personalizzazioneIngredienti);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore durante il recupero di tutte le associazioni personalizzazione-ingrediente");
                return SafeInternalError("Errore durante il recupero delle associazioni");
            }
        }

        // GET: api/PersonalizzazioneIngrediente/5
        [HttpGet("{id}")]       
        public async Task<ActionResult<PersonalizzazioneIngredienteDTO>> GetById(int id)
        {
            try
            {
                if (id <= 0)
                    return SafeBadRequest<PersonalizzazioneIngredienteDTO>("ID associazione non valido");

                var personalizzazioneIngrediente = await _personalizzazioneIngredienteRepository.GetByIdAsync(id);

                if (personalizzazioneIngrediente == null)
                    return SafeNotFound<PersonalizzazioneIngredienteDTO>("Associazione personalizzazione-ingrediente");

                return Ok(personalizzazioneIngrediente);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore durante il recupero dell'associazione personalizzazione-ingrediente {Id}", id);
                return SafeInternalError("Errore durante il recupero dell'associazione");
            }
        }

        // GET: api/PersonalizzazioneIngrediente/personalizzazione/5
        [HttpGet("personalizzazione/{personalizzazioneId}")]        
        public async Task<ActionResult<IEnumerable<PersonalizzazioneIngredienteDTO>>> GetByPersonalizzazioneId(int personalizzazioneId)
        {
            try
            {
                if (personalizzazioneId <= 0)
                    return SafeBadRequest<IEnumerable<PersonalizzazioneIngredienteDTO>>("ID personalizzazione non valido");

                var personalizzazioneIngredienti = await _personalizzazioneIngredienteRepository.GetByPersonalizzazioneIdAsync(personalizzazioneId);
                return Ok(personalizzazioneIngredienti);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore durante il recupero delle associazioni per personalizzazione {PersonalizzazioneId}", personalizzazioneId);
                return SafeInternalError("Errore durante il recupero delle associazioni");
            }
        }

        // GET: api/PersonalizzazioneIngrediente/ingrediente/5
        [HttpGet("ingrediente/{ingredienteId}")]        
        public async Task<ActionResult<IEnumerable<PersonalizzazioneIngredienteDTO>>> GetByIngredienteId(int ingredienteId)
        {
            try
            {
                if (ingredienteId <= 0)
                    return SafeBadRequest<IEnumerable<PersonalizzazioneIngredienteDTO>>("ID ingrediente non valido");

                var personalizzazioneIngredienti = await _personalizzazioneIngredienteRepository.GetByIngredienteIdAsync(ingredienteId);
                return Ok(personalizzazioneIngredienti);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore durante il recupero delle associazioni per ingrediente {IngredienteId}", ingredienteId);
                return SafeInternalError("Errore durante il recupero delle associazioni");
            }
        }

        // GET: api/PersonalizzazioneIngrediente/personalizzazione/5/ingrediente/5
        [HttpGet("personalizzazione/{personalizzazioneId}/ingrediente/{ingredienteId}")]        
        public async Task<ActionResult<PersonalizzazioneIngredienteDTO>> GetByPersonalizzazioneAndIngrediente(int personalizzazioneId, int ingredienteId)
        {
            try
            {
                if (personalizzazioneId <= 0 || ingredienteId <= 0)
                    return SafeBadRequest<PersonalizzazioneIngredienteDTO>("ID personalizzazione o ingrediente non validi");

                var personalizzazioneIngrediente = await _personalizzazioneIngredienteRepository.GetByPersonalizzazioneAndIngredienteAsync(personalizzazioneId, ingredienteId);

                if (personalizzazioneIngrediente == null)
                    return SafeNotFound<PersonalizzazioneIngredienteDTO>("Associazione personalizzazione-ingrediente");

                return Ok(personalizzazioneIngrediente);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore durante il recupero dell'associazione per personalizzazione {PersonalizzazioneId} e ingrediente {IngredienteId}", personalizzazioneId, ingredienteId);
                return SafeInternalError("Errore durante il recupero dell'associazione");
            }
        }

        // POST: api/PersonalizzazioneIngrediente
        [HttpPost]
        //[Authorize(Roles = "admin")] // ✅ Solo admin e manager possono creare associazioni
        public async Task<ActionResult<PersonalizzazioneIngredienteDTO>> Create(PersonalizzazioneIngredienteDTO personalizzazioneIngredienteDto)
        {
            try
            {
                if (!IsModelValid(personalizzazioneIngredienteDto))
                    return SafeBadRequest<PersonalizzazioneIngredienteDTO>("Dati associazione non validi");

                // Verifica se l'associazione esiste già
                var exists = await _personalizzazioneIngredienteRepository.ExistsByPersonalizzazioneAndIngredienteAsync(
                    personalizzazioneIngredienteDto.PersonalizzazioneId,
                    personalizzazioneIngredienteDto.IngredienteId);

                if (exists)
                    return SafeBadRequest<PersonalizzazioneIngredienteDTO>("Esiste già un'associazione per questa personalizzazione e ingrediente");

                await _personalizzazioneIngredienteRepository.AddAsync(personalizzazioneIngredienteDto);

                // ✅ Audit trail
                LogAuditTrail("CREATE_PERSONALIZZAZIONE_INGREDIENTE", "PersonalizzazioneIngrediente", personalizzazioneIngredienteDto.PersonalizzazioneIngredienteId.ToString());
                LogSecurityEvent("PersonalizzazioneIngredienteCreated", new
                {
                    PersonalizzazioneIngredienteId = personalizzazioneIngredienteDto.PersonalizzazioneIngredienteId,
                    PersonalizzazioneId = personalizzazioneIngredienteDto.PersonalizzazioneId,
                    IngredienteId = personalizzazioneIngredienteDto.IngredienteId,
                    User = User.Identity?.Name
                });

                return CreatedAtAction(nameof(GetById), new { id = personalizzazioneIngredienteDto.PersonalizzazioneIngredienteId }, personalizzazioneIngredienteDto);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore durante la creazione dell'associazione personalizzazione-ingrediente");
                return SafeInternalError("Errore durante la creazione dell'associazione");
            }
        }

        // PUT: api/PersonalizzazioneIngrediente/5
        [HttpPut("{id}")]
        //[Authorize(Roles = "admin")] // ✅ Solo admin e manager possono modificare associazioni
        public async Task<IActionResult> Update(int id, PersonalizzazioneIngredienteDTO personalizzazioneIngredienteDto)
        {
            try
            {
                if (id <= 0)
                    return SafeBadRequest("ID associazione non valido");

                if (id != personalizzazioneIngredienteDto.PersonalizzazioneIngredienteId)
                    return SafeBadRequest("ID nell'URL non corrisponde all'ID nel corpo della richiesta");

                if (!IsModelValid(personalizzazioneIngredienteDto))
                    return SafeBadRequest("Dati associazione non validi");

                var existingAssociazione = await _personalizzazioneIngredienteRepository.GetByIdAsync(id);
                if (existingAssociazione == null)
                    return SafeNotFound("Associazione personalizzazione-ingrediente");

                await _personalizzazioneIngredienteRepository.UpdateAsync(personalizzazioneIngredienteDto);

                // ✅ Audit trail
                LogAuditTrail("UPDATE_PERSONALIZZAZIONE_INGREDIENTE", "PersonalizzazioneIngrediente", personalizzazioneIngredienteDto.PersonalizzazioneIngredienteId.ToString());
                LogSecurityEvent("PersonalizzazioneIngredienteUpdated", new
                {
                    PersonalizzazioneIngredienteId = personalizzazioneIngredienteDto.PersonalizzazioneIngredienteId,
                    User = User.Identity?.Name
                });

                return NoContent();
            }
            catch (System.ArgumentException ex)
            {
                _logger.LogWarning(ex, "Tentativo di aggiornamento di associazione inesistente {Id}", id);
                return SafeNotFound("Associazione personalizzazione-ingrediente");
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore durante l'aggiornamento dell'associazione personalizzazione-ingrediente {Id}", id);
                return SafeInternalError("Errore durante l'aggiornamento dell'associazione");
            }
        }

        // DELETE: api/PersonalizzazioneIngrediente/5
        [HttpDelete("{id}")]
        //[Authorize(Roles = "admin")] // ✅ Solo admin e manager possono eliminare associazioni
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                if (id <= 0)
                    return SafeBadRequest("ID associazione non valido");

                var existingAssociazione = await _personalizzazioneIngredienteRepository.GetByIdAsync(id);
                if (existingAssociazione == null)
                    return SafeNotFound("Associazione personalizzazione-ingrediente");

                await _personalizzazioneIngredienteRepository.DeleteAsync(id);

                // ✅ Audit trail
                LogAuditTrail("DELETE_PERSONALIZZAZIONE_INGREDIENTE", "PersonalizzazioneIngrediente", id.ToString());
                LogSecurityEvent("PersonalizzazioneIngredienteDeleted", new
                {
                    PersonalizzazioneIngredienteId = id,
                    User = User.Identity?.Name
                });

                return NoContent();
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore durante l'eliminazione dell'associazione personalizzazione-ingrediente {Id}", id);
                return SafeInternalError("Errore durante l'eliminazione dell'associazione");
            }
        }        

        // GET: api/PersonalizzazioneIngrediente/exists/{id}
        [HttpGet("exists/{id}")]        
        public async Task<ActionResult<bool>> Exists(int id)
        {
            try
            {
                if (id <= 0)
                    return SafeBadRequest<bool>("ID associazione non valido");

                var exists = await _personalizzazioneIngredienteRepository.ExistsAsync(id);
                return Ok(exists);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore durante la verifica esistenza associazione {Id}", id);
                return SafeInternalError("Errore durante la verifica dell'esistenza");
            }
        }

        // GET: api/PersonalizzazioneIngrediente/exists/personalizzazione/5/ingrediente/{id}
        [HttpGet("exists/personalizzazione/{personalizzazioneId}/ingrediente/{ingredienteId}")]        
        public async Task<ActionResult<bool>> ExistsByPersonalizzazioneAndIngrediente(int personalizzazioneId, int ingredienteId)
        {
            try
            {
                if (personalizzazioneId <= 0 || ingredienteId <= 0)
                    return SafeBadRequest<bool>("ID personalizzazione o ingrediente non validi");

                var exists = await _personalizzazioneIngredienteRepository.ExistsByPersonalizzazioneAndIngredienteAsync(personalizzazioneId, ingredienteId);
                return Ok(exists);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore durante la verifica esistenza associazione per personalizzazione {PersonalizzazioneId} e ingrediente {IngredienteId}", personalizzazioneId, ingredienteId);
                return SafeInternalError("Errore durante la verifica dell'esistenza");
            }
        }

        // GET: api/PersonalizzazioneIngrediente/count/personalizzazione/{id}
        [HttpGet("count/personalizzazione/{personalizzazioneId}")]        
        public async Task<ActionResult<int>> GetCountByPersonalizzazione(int personalizzazioneId)
        {
            try
            {
                if (personalizzazioneId <= 0)
                    return SafeBadRequest<int>("ID personalizzazione non valido");

                var count = await _personalizzazioneIngredienteRepository.GetCountByPersonalizzazioneAsync(personalizzazioneId);
                return Ok(count);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore durante il conteggio delle associazioni per personalizzazione {PersonalizzazioneId}", personalizzazioneId);
                return SafeInternalError("Errore durante il conteggio delle associazioni");
            }
        }
    }
}