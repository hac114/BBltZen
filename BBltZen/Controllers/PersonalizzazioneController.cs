using BBltZen;
using DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Repository.Interface;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BBltZen.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [AllowAnonymous] // ✅ OVERRIDE DELL'[Authorize] DEL BASE CONTROLLER
    public class PersonalizzazioneController : SecureBaseController
    {
        private readonly IPersonalizzazioneRepository _personalizzazioneRepository;
        private readonly BubbleTeaContext _context;

        public PersonalizzazioneController(
            IPersonalizzazioneRepository personalizzazioneRepository,
            BubbleTeaContext context,
            IWebHostEnvironment environment,
            ILogger<PersonalizzazioneController> logger)
            : base(environment, logger)
        {
            _personalizzazioneRepository = personalizzazioneRepository;
            _context = context;
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
            catch (Exception ex)
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante il recupero della personalizzazione con ID {Id}", id);
                return SafeInternalError("Errore durante il recupero della personalizzazione");
            }
        }

        [HttpGet("exists/nome/{nome}")]
        public async Task<ActionResult<bool>> CheckNomeExists(string nome, [FromQuery] int? excludeId = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(nome))
                    return SafeBadRequest<bool>("Nome personalizzazione non valido");

                // ✅ USA IL METODO DEL REPOSITORY CON ESCLUSIONE ID
                var exists = await _personalizzazioneRepository.ExistsByNameAsync(nome, excludeId);

                return Ok(exists);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante verifica esistenza nome personalizzazione {Nome}", nome);
                return SafeInternalError<bool>("Errore durante la verifica");
            }
        }

        // POST: api/Personalizzazione
        //[Authorize(Roles = "admin")]
        [HttpPost]
        // [Authorize(Roles = "admin,manager")] // ✅ KEYCLOAK READY - COMMENTATO PER TEST
        public async Task<ActionResult<PersonalizzazioneDTO>> Create([FromBody] PersonalizzazioneDTO personalizzazioneDto)
        {
            try
            {
                if (!IsModelValid(personalizzazioneDto))
                    return SafeBadRequest<PersonalizzazioneDTO>("Dati personalizzazione non validi");

                // ✅ USA IL RISULTATO DI AddAsync (PATTERN STANDARD)
                var result = await _personalizzazioneRepository.AddAsync(personalizzazioneDto);

                // ✅ AUDIT & SECURITY
                LogAuditTrail("CREATE", "Personalizzazione", result.PersonalizzazioneId.ToString());
                LogSecurityEvent("PersonalizzazioneCreated", new
                {
                    result.PersonalizzazioneId,
                    result.Nome,
                    UserId = GetCurrentUserIdOrDefault()
                });

                return CreatedAtAction(nameof(GetById), new { id = result.PersonalizzazioneId }, result);
            }
            catch (ArgumentException argEx)
            {
                return SafeBadRequest<PersonalizzazioneDTO>(argEx.Message);
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "Errore database durante la creazione personalizzazione");
                return SafeInternalError<PersonalizzazioneDTO>("Errore durante il salvataggio");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante la creazione personalizzazione");
                return SafeInternalError<PersonalizzazioneDTO>("Errore durante la creazione");
            }
        }

        // PUT: api/Personalizzazione/{id}
        //[Authorize(Roles = "admin")]
        [HttpPut("{id}")]
        // [Authorize(Roles = "admin,manager")] // ✅ KEYCLOAK READY - COMMENTATO PER TEST
        public async Task<ActionResult> Update(int id, [FromBody] PersonalizzazioneDTO personalizzazioneDto)
        {
            try
            {
                if (id <= 0 || id != personalizzazioneDto.PersonalizzazioneId)
                    return SafeBadRequest("ID personalizzazione non valido");

                if (!IsModelValid(personalizzazioneDto))
                    return SafeBadRequest("Dati personalizzazione non validi");

                // ✅ VERIFICA ESISTENZA
                if (!await _personalizzazioneRepository.ExistsAsync(id))
                    return SafeNotFound("Personalizzazione");

                await _personalizzazioneRepository.UpdateAsync(personalizzazioneDto);

                // ✅ AUDIT & SECURITY
                LogAuditTrail("UPDATE", "Personalizzazione", id.ToString());
                LogSecurityEvent("PersonalizzazioneUpdated", new
                {
                    PersonalizzazioneId = id,
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
                _logger.LogError(dbEx, "Errore database durante l'aggiornamento personalizzazione {Id}", id);
                return SafeInternalError("Errore durante l'aggiornamento");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante l'aggiornamento personalizzazione {Id}", id);
                return SafeInternalError("Errore durante l'aggiornamento");
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
            catch (Exception ex)
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
        [HttpDelete("{id}")]
        // [Authorize(Roles = "admin")] // ✅ KEYCLOAK READY - COMMENTATO PER TEST
        public async Task<ActionResult> Delete(int id)
        {
            try
            {
                if (id <= 0)
                    return SafeBadRequest("ID personalizzazione non valido");

                var personalizzazione = await _personalizzazioneRepository.GetByIdAsync(id);
                if (personalizzazione == null)
                    return SafeNotFound("Personalizzazione");

                await _personalizzazioneRepository.DeleteAsync(id);

                // ✅ AUDIT & SECURITY
                LogAuditTrail("DELETE", "Personalizzazione", id.ToString());
                LogSecurityEvent("PersonalizzazioneDeleted", new
                {
                    PersonalizzazioneId = id,
                    UserId = GetCurrentUserIdOrDefault()
                });

                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Tentativo eliminazione personalizzazione {Id} con dipendenze", id);
                return SafeBadRequest(ex.Message);
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "Errore database durante l'eliminazione personalizzazione {Id}", id);
                return SafeInternalError("Errore durante l'eliminazione");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante l'eliminazione personalizzazione {Id}", id);
                return SafeInternalError("Errore durante l'eliminazione");
            }
        }
    }
}