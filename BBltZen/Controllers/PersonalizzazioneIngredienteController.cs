using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Repository.Interface;
using DTO;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Database;

namespace BBltZen.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [AllowAnonymous]
    public class PersonalizzazioneIngredienteController : SecureBaseController
    {
        private readonly IPersonalizzazioneIngredienteRepository _personalizzazioneIngredienteRepository;
        private readonly BubbleTeaContext _context; // ✅ AGGIUNTO

        public PersonalizzazioneIngredienteController(
            IPersonalizzazioneIngredienteRepository personalizzazioneIngredienteRepository,
            BubbleTeaContext context, // ✅ AGGIUNTO
            IWebHostEnvironment environment,
            ILogger<PersonalizzazioneIngredienteController> logger)
            : base(environment, logger)
        {
            _personalizzazioneIngredienteRepository = personalizzazioneIngredienteRepository;
            _context = context; // ✅ AGGIUNTO
        }

        // GET: api/PersonalizzazioneIngrediente
        [HttpGet]
        [AllowAnonymous] // ✅ AGGIUNTO
        public async Task<ActionResult<IEnumerable<PersonalizzazioneIngredienteDTO>>> GetAll()
        {
            try
            {
                var personalizzazioneIngredienti = await _personalizzazioneIngredienteRepository.GetAllAsync();
                return Ok(personalizzazioneIngredienti);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante il recupero di tutte le associazioni personalizzazione-ingrediente");
                return SafeInternalError("Errore durante il recupero delle associazioni");
            }
        }

        // GET: api/PersonalizzazioneIngrediente/5
        [HttpGet("{id}")]
        [AllowAnonymous] // ✅ AGGIUNTO
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante il recupero dell'associazione personalizzazione-ingrediente {Id}", id);
                return SafeInternalError("Errore durante il recupero dell'associazione");
            }
        }

        // GET: api/PersonalizzazioneIngrediente/personalizzazione/5
        [HttpGet("personalizzazione/{personalizzazioneId}")]
        [AllowAnonymous] // ✅ AGGIUNTO
        public async Task<ActionResult<IEnumerable<PersonalizzazioneIngredienteDTO>>> GetByPersonalizzazioneId(int personalizzazioneId)
        {
            try
            {
                if (personalizzazioneId <= 0)
                    return SafeBadRequest<IEnumerable<PersonalizzazioneIngredienteDTO>>("ID personalizzazione non valido");

                var personalizzazioneIngredienti = await _personalizzazioneIngredienteRepository.GetByPersonalizzazioneIdAsync(personalizzazioneId);
                return Ok(personalizzazioneIngredienti);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante il recupero delle associazioni per personalizzazione {PersonalizzazioneId}", personalizzazioneId);
                return SafeInternalError("Errore durante il recupero delle associazioni");
            }
        }

        // GET: api/PersonalizzazioneIngrediente/ingrediente/5
        [HttpGet("ingrediente/{ingredienteId}")]
        [AllowAnonymous] // ✅ AGGIUNTO
        public async Task<ActionResult<IEnumerable<PersonalizzazioneIngredienteDTO>>> GetByIngredienteId(int ingredienteId)
        {
            try
            {
                if (ingredienteId <= 0)
                    return SafeBadRequest<IEnumerable<PersonalizzazioneIngredienteDTO>>("ID ingrediente non valido");

                var personalizzazioneIngredienti = await _personalizzazioneIngredienteRepository.GetByIngredienteIdAsync(ingredienteId);
                return Ok(personalizzazioneIngredienti);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante il recupero delle associazioni per ingrediente {IngredienteId}", ingredienteId);
                return SafeInternalError("Errore durante il recupero delle associazioni");
            }
        }

        // GET: api/PersonalizzazioneIngrediente/personalizzazione/5/ingrediente/5
        [HttpGet("personalizzazione/{personalizzazioneId}/ingrediente/{ingredienteId}")]
        [AllowAnonymous] // ✅ AGGIUNTO
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante il recupero dell'associazione per personalizzazione {PersonalizzazioneId} e ingrediente {IngredienteId}", personalizzazioneId, ingredienteId);
                return SafeInternalError("Errore durante il recupero dell'associazione");
            }
        }

        // POST: api/PersonalizzazioneIngrediente
        [HttpPost]
        // [Authorize(Roles = "admin,manager")] // ✅ KEYCLOAK READY - COMMENTATO PER TEST
        public async Task<ActionResult<PersonalizzazioneIngredienteDTO>> Create([FromBody] PersonalizzazioneIngredienteDTO personalizzazioneIngredienteDto)
        {
            try
            {
                if (!IsModelValid(personalizzazioneIngredienteDto))
                    return SafeBadRequest<PersonalizzazioneIngredienteDTO>("Dati associazione non validi");

                // ✅ USA IL RISULTATO DI AddAsync (PATTERN STANDARD)
                var result = await _personalizzazioneIngredienteRepository.AddAsync(personalizzazioneIngredienteDto);

                // ✅ AUDIT & SECURITY
                LogAuditTrail("CREATE", "PersonalizzazioneIngrediente", result.PersonalizzazioneIngredienteId.ToString());
                LogSecurityEvent("PersonalizzazioneIngredienteCreated", new
                {
                    result.PersonalizzazioneIngredienteId,
                    result.PersonalizzazioneId,
                    result.IngredienteId,
                    UserId = GetCurrentUserIdOrDefault()
                });

                return CreatedAtAction(nameof(GetById), new { id = result.PersonalizzazioneIngredienteId }, result);
            }
            catch (ArgumentException argEx)
            {
                return SafeBadRequest<PersonalizzazioneIngredienteDTO>(argEx.Message);
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "Errore database durante la creazione associazione personalizzazione-ingrediente");
                return SafeInternalError<PersonalizzazioneIngredienteDTO>("Errore durante il salvataggio");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante la creazione associazione personalizzazione-ingrediente");
                return SafeInternalError<PersonalizzazioneIngredienteDTO>("Errore durante la creazione");
            }
        }

        // PUT: api/PersonalizzazioneIngrediente/5
        [HttpPut("{id}")]
        // [Authorize(Roles = "admin,manager")] // ✅ KEYCLOAK READY - COMMENTATO PER TEST
        public async Task<ActionResult> Update(int id, [FromBody] PersonalizzazioneIngredienteDTO personalizzazioneIngredienteDto)
        {
            try
            {
                if (id <= 0 || id != personalizzazioneIngredienteDto.PersonalizzazioneIngredienteId)
                    return SafeBadRequest("ID associazione non valido");

                if (!IsModelValid(personalizzazioneIngredienteDto))
                    return SafeBadRequest("Dati associazione non validi");

                // ✅ VERIFICA ESISTENZA
                if (!await _personalizzazioneIngredienteRepository.ExistsAsync(id))
                    return SafeNotFound("Associazione personalizzazione-ingrediente");

                await _personalizzazioneIngredienteRepository.UpdateAsync(personalizzazioneIngredienteDto);

                // ✅ AUDIT & SECURITY
                LogAuditTrail("UPDATE", "PersonalizzazioneIngrediente", id.ToString());
                LogSecurityEvent("PersonalizzazioneIngredienteUpdated", new
                {
                    PersonalizzazioneIngredienteId = id,
                    UserId = GetCurrentUserIdOrDefault()
                });

                return NoContent();
            }
            catch (ArgumentException argEx)
            {
                return SafeBadRequest(argEx.Message);
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "Errore database durante l'aggiornamento associazione personalizzazione-ingrediente {Id}", id);
                return SafeInternalError("Errore durante l'aggiornamento");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante l'aggiornamento associazione personalizzazione-ingrediente {Id}", id);
                return SafeInternalError("Errore durante l'aggiornamento");
            }
        }

        // DELETE: api/PersonalizzazioneIngrediente/5
        [HttpDelete("{id}")]
        // [Authorize(Roles = "admin")] // ✅ KEYCLOAK READY - COMMENTATO PER TEST
        public async Task<ActionResult> Delete(int id)
        {
            try
            {
                if (id <= 0)
                    return SafeBadRequest("ID associazione non valido");

                var associazione = await _personalizzazioneIngredienteRepository.GetByIdAsync(id);
                if (associazione == null)
                    return SafeNotFound("Associazione personalizzazione-ingrediente");

                await _personalizzazioneIngredienteRepository.DeleteAsync(id);

                // ✅ AUDIT & SECURITY
                LogAuditTrail("DELETE", "PersonalizzazioneIngrediente", id.ToString());
                LogSecurityEvent("PersonalizzazioneIngredienteDeleted", new
                {
                    PersonalizzazioneIngredienteId = id,
                    UserId = GetCurrentUserIdOrDefault()
                });

                return NoContent();
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "Errore database durante l'eliminazione associazione personalizzazione-ingrediente {Id}", id);
                return SafeInternalError("Errore durante l'eliminazione");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante l'eliminazione associazione personalizzazione-ingrediente {Id}", id);
                return SafeInternalError("Errore durante l'eliminazione");
            }
        }

        // GET: api/PersonalizzazioneIngrediente/exists/{id}
        [HttpGet("exists/{id}")]
        [AllowAnonymous] // ✅ AGGIUNTO
        public async Task<ActionResult<bool>> Exists(int id)
        {
            try
            {
                if (id <= 0)
                    return SafeBadRequest<bool>("ID associazione non valido");

                var exists = await _personalizzazioneIngredienteRepository.ExistsAsync(id);
                return Ok(exists);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante la verifica esistenza associazione {Id}", id);
                return SafeInternalError("Errore durante la verifica dell'esistenza");
            }
        }

        // GET: api/PersonalizzazioneIngrediente/exists/personalizzazione/5/ingrediente/{id}
        [HttpGet("exists/personalizzazione/{personalizzazioneId}/ingrediente/{ingredienteId}")]
        [AllowAnonymous] // ✅ AGGIUNTO
        public async Task<ActionResult<bool>> ExistsByPersonalizzazioneAndIngrediente(int personalizzazioneId, int ingredienteId)
        {
            try
            {
                if (personalizzazioneId <= 0 || ingredienteId <= 0)
                    return SafeBadRequest<bool>("ID personalizzazione o ingrediente non validi");

                var exists = await _personalizzazioneIngredienteRepository.ExistsByPersonalizzazioneAndIngredienteAsync(personalizzazioneId, ingredienteId);
                return Ok(exists);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante la verifica esistenza associazione per personalizzazione {PersonalizzazioneId} e ingrediente {IngredienteId}", personalizzazioneId, ingredienteId);
                return SafeInternalError("Errore durante la verifica dell'esistenza");
            }
        }

        // GET: api/PersonalizzazioneIngrediente/count/personalizzazione/{id}
        [HttpGet("count/personalizzazione/{personalizzazioneId}")]
        [AllowAnonymous] // ✅ AGGIUNTO
        public async Task<ActionResult<int>> GetCountByPersonalizzazione(int personalizzazioneId)
        {
            try
            {
                if (personalizzazioneId <= 0)
                    return SafeBadRequest<int>("ID personalizzazione non valido");

                var count = await _personalizzazioneIngredienteRepository.GetCountByPersonalizzazioneAsync(personalizzazioneId);
                return Ok(count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante il conteggio delle associazioni per personalizzazione {PersonalizzazioneId}", personalizzazioneId);
                return SafeInternalError("Errore durante il conteggio delle associazioni");
            }
        }
    }
}