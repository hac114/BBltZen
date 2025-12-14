using Microsoft.AspNetCore.Mvc;
using DTO;
using Repository.Interface;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using BBltZen;

namespace BBltZen.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [AllowAnonymous] // ✅ OVERRIDE DELL'[Authorize] DEL BASE CONTROLLER
    public class DimensioneQuantitaIngredientiController : SecureBaseController
    {
        private readonly IDimensioneQuantitaIngredientiRepository _repository;
        private readonly BubbleTeaContext _context;

        public DimensioneQuantitaIngredientiController(
            IDimensioneQuantitaIngredientiRepository repository,
            BubbleTeaContext context,
            IWebHostEnvironment environment,
            ILogger<DimensioneQuantitaIngredientiController> logger)
            : base(environment, logger)
        {
            _repository = repository;
            _context = context;
        }

        // GET: api/DimensioneQuantitaIngredienti
        [HttpGet]
        [AllowAnonymous] // ✅ ESPLICITO PER ENDPOINT GET
        public async Task<ActionResult<IEnumerable<DimensioneQuantitaIngredientiDTO>>> GetAll()
        {
            try
            {
                var dimensioniQuantita = await _repository.GetAllAsync();
                return Ok(dimensioniQuantita);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante il recupero di tutte le dimensioni quantità ingredienti");
                return SafeInternalError<IEnumerable<DimensioneQuantitaIngredientiDTO>>("Errore durante il recupero delle dimensioni quantità");
            }
        }

        // GET: api/DimensioneQuantitaIngredienti/5/10
        [HttpGet("{dimensioneId}/{personalizzazioneIngredienteId}")]
        [AllowAnonymous] // ✅ ESPLICITO PER ENDPOINT GET
        public async Task<ActionResult<DimensioneQuantitaIngredientiDTO>> GetById(int dimensioneId, int personalizzazioneIngredienteId)
        {
            try
            {
                if (dimensioneId <= 0 || personalizzazioneIngredienteId <= 0)
                    return SafeBadRequest<DimensioneQuantitaIngredientiDTO>("ID dimensione o personalizzazione ingrediente non validi");

                var dimensioneQuantita = await _repository.GetByIdAsync(dimensioneId, personalizzazioneIngredienteId);

                if (dimensioneQuantita == null)
                    return SafeNotFound<DimensioneQuantitaIngredientiDTO>("Dimensione quantità ingredienti");

                return Ok(dimensioneQuantita);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante il recupero della dimensione quantità ingredienti {DimensioneId}/{PersonalizzazioneIngredienteId}", dimensioneId, personalizzazioneIngredienteId);
                return SafeInternalError<DimensioneQuantitaIngredientiDTO>("Errore durante il recupero della dimensione quantità");
            }
        }

        // GET: api/DimensioneQuantitaIngredienti/dimensione-bicchiere/5
        [HttpGet("dimensione-bicchiere/{dimensioneBicchiereId}")]
        [AllowAnonymous] // ✅ ESPLICITO PER ENDPOINT GET
        public async Task<ActionResult<IEnumerable<DimensioneQuantitaIngredientiDTO>>> GetByDimensioneBicchiere(int dimensioneBicchiereId)
        {
            try
            {
                if (dimensioneBicchiereId <= 0)
                    return SafeBadRequest<IEnumerable<DimensioneQuantitaIngredientiDTO>>("ID dimensione bicchiere non valido");

                // ✅ Verifica se la dimensione bicchiere esiste
                var dimensioneBicchiereEsiste = await _context.DimensioneBicchiere.AnyAsync(d => d.DimensioneBicchiereId == dimensioneBicchiereId);
                if (!dimensioneBicchiereEsiste)
                    return SafeNotFound<IEnumerable<DimensioneQuantitaIngredientiDTO>>("Dimensione bicchiere");

                var dimensioniQuantita = await _repository.GetByDimensioneBicchiereAsync(dimensioneBicchiereId);
                return Ok(dimensioniQuantita);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante il recupero delle dimensioni quantità per dimensione bicchiere {DimensioneBicchiereId}", dimensioneBicchiereId);
                return SafeInternalError<IEnumerable<DimensioneQuantitaIngredientiDTO>>("Errore durante il recupero delle dimensioni quantità");
            }
        }

        // GET: api/DimensioneQuantitaIngredienti/personalizzazione-ingrediente/5
        [HttpGet("personalizzazione-ingrediente/{personalizzazioneIngredienteId}")]
        [AllowAnonymous] // ✅ ESPLICITO PER ENDPOINT GET
        public async Task<ActionResult<IEnumerable<DimensioneQuantitaIngredientiDTO>>> GetByPersonalizzazioneIngrediente(int personalizzazioneIngredienteId)
        {
            try
            {
                if (personalizzazioneIngredienteId <= 0)
                    return SafeBadRequest<IEnumerable<DimensioneQuantitaIngredientiDTO>>("ID personalizzazione ingrediente non valido");

                // ✅ Verifica se la personalizzazione ingrediente esiste
                var personalizzazioneEsiste = await _context.PersonalizzazioneIngrediente.AnyAsync(p => p.PersonalizzazioneIngredienteId == personalizzazioneIngredienteId);
                if (!personalizzazioneEsiste)
                    return SafeNotFound<IEnumerable<DimensioneQuantitaIngredientiDTO>>("Personalizzazione ingrediente");

                var dimensioniQuantita = await _repository.GetByPersonalizzazioneIngredienteAsync(personalizzazioneIngredienteId);
                return Ok(dimensioniQuantita);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante il recupero delle dimensioni quantità per personalizzazione ingrediente {PersonalizzazioneIngredienteId}", personalizzazioneIngredienteId);
                return SafeInternalError<IEnumerable<DimensioneQuantitaIngredientiDTO>>("Errore durante il recupero delle dimensioni quantità");
            }
        }

        // GET: api/DimensioneQuantitaIngredienti/combinazione/5/10
        [HttpGet("combinazione/{dimensioneBicchiereId}/{personalizzazioneIngredienteId}")]
        [AllowAnonymous] // ✅ ESPLICITO PER ENDPOINT GET
        public async Task<ActionResult<DimensioneQuantitaIngredientiDTO>> GetByCombinazione(int dimensioneBicchiereId, int personalizzazioneIngredienteId)
        {
            try
            {
                if (dimensioneBicchiereId <= 0 || personalizzazioneIngredienteId <= 0)
                    return SafeBadRequest<DimensioneQuantitaIngredientiDTO>("ID dimensione bicchiere o personalizzazione ingrediente non validi");

                var dimensioneQuantita = await _repository.GetByCombinazioneAsync(dimensioneBicchiereId, personalizzazioneIngredienteId);

                if (dimensioneQuantita == null)
                    return SafeNotFound<DimensioneQuantitaIngredientiDTO>("Dimensione quantità ingredienti per la combinazione specificata");

                return Ok(dimensioneQuantita);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante il recupero della dimensione quantità per combinazione {DimensioneBicchiereId}/{PersonalizzazioneIngredienteId}", dimensioneBicchiereId, personalizzazioneIngredienteId);
                return SafeInternalError<DimensioneQuantitaIngredientiDTO>("Errore durante il recupero della dimensione quantità");
            }
        }

        // POST: api/DimensioneQuantitaIngredienti
        [HttpPost]
        // [Authorize(Roles = "admin,manager")] // ✅ KEYCLOAK READY - COMMENTATO PER TEST
        public async Task<ActionResult<DimensioneQuantitaIngredientiDTO>> Create([FromBody] DimensioneQuantitaIngredientiDTO dimensioneQuantitaDto)
        {
            try
            {
                if (!IsModelValid(dimensioneQuantitaDto))
                    return SafeBadRequest<DimensioneQuantitaIngredientiDTO>("Dati dimensione quantità non validi");

                // ✅ Verifica se la dimensione bicchiere esiste
                var dimensioneBicchiereEsiste = await _context.DimensioneBicchiere.AnyAsync(d => d.DimensioneBicchiereId == dimensioneQuantitaDto.DimensioneBicchiereId);
                if (!dimensioneBicchiereEsiste)
                    return SafeBadRequest<DimensioneQuantitaIngredientiDTO>("Dimensione bicchiere non trovata");

                // ✅ Verifica se la personalizzazione ingrediente esiste
                var personalizzazioneEsiste = await _context.PersonalizzazioneIngrediente.AnyAsync(p => p.PersonalizzazioneIngredienteId == dimensioneQuantitaDto.PersonalizzazioneIngredienteId);
                if (!personalizzazioneEsiste)
                    return SafeBadRequest<DimensioneQuantitaIngredientiDTO>("Personalizzazione ingrediente non trovata");

                // ✅ USA IL RISULTATO DI AddAsync (PATTERN STANDARD)
                var result = await _repository.AddAsync(dimensioneQuantitaDto);

                // ✅ AUDIT & SECURITY
                LogAuditTrail("CREATE", "DimensioneQuantitaIngredienti", $"{result.DimensioneId}/{result.PersonalizzazioneIngredienteId}");
                LogSecurityEvent("DimensioneQuantitaIngredientiCreated", new
                {
                    result.DimensioneId,
                    result.PersonalizzazioneIngredienteId,
                    result.DimensioneBicchiereId,
                    UserId = GetCurrentUserIdOrDefault()
                });

                return CreatedAtAction(nameof(GetById), new
                {
                    dimensioneId = result.DimensioneId,
                    personalizzazioneIngredienteId = result.PersonalizzazioneIngredienteId
                }, result);
            }
            catch (ArgumentException argEx)
            {
                return SafeBadRequest<DimensioneQuantitaIngredientiDTO>(argEx.Message);
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "Errore database durante la creazione dimensione quantità ingredienti");
                return SafeInternalError<DimensioneQuantitaIngredientiDTO>("Errore durante il salvataggio");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante la creazione dimensione quantità ingredienti");
                return SafeInternalError<DimensioneQuantitaIngredientiDTO>("Errore durante la creazione");
            }
        }

        // PUT: api/DimensioneQuantitaIngredienti/5/10
        [HttpPut("{dimensioneId}/{personalizzazioneIngredienteId}")]
        // [Authorize(Roles = "admin,manager")] // ✅ KEYCLOAK READY - COMMENTATO PER TEST
        public async Task<ActionResult> Update(int dimensioneId, int personalizzazioneIngredienteId, [FromBody] DimensioneQuantitaIngredientiDTO dimensioneQuantitaDto)
        {
            try
            {
                if (dimensioneId <= 0 || personalizzazioneIngredienteId <= 0 ||
                    dimensioneId != dimensioneQuantitaDto.DimensioneId ||
                    personalizzazioneIngredienteId != dimensioneQuantitaDto.PersonalizzazioneIngredienteId)
                    return SafeBadRequest("ID dimensione o personalizzazione ingrediente non validi");

                if (!IsModelValid(dimensioneQuantitaDto))
                    return SafeBadRequest("Dati dimensione quantità non validi");

                // ✅ VERIFICA ESISTENZA
                if (!await _repository.ExistsAsync(dimensioneId, personalizzazioneIngredienteId))
                    return SafeNotFound("Dimensione quantità ingredienti");

                // ✅ Verifica se la dimensione bicchiere esiste
                var dimensioneBicchiereEsiste = await _context.DimensioneBicchiere.AnyAsync(d => d.DimensioneBicchiereId == dimensioneQuantitaDto.DimensioneBicchiereId);
                if (!dimensioneBicchiereEsiste)
                    return SafeBadRequest("Dimensione bicchiere non trovata");

                await _repository.UpdateAsync(dimensioneQuantitaDto);

                // ✅ AUDIT & SECURITY
                LogAuditTrail("UPDATE", "DimensioneQuantitaIngredienti", $"{dimensioneId}/{personalizzazioneIngredienteId}");
                LogSecurityEvent("DimensioneQuantitaIngredientiUpdated", new
                {
                    DimensioneId = dimensioneId,
                    PersonalizzazioneIngredienteId = personalizzazioneIngredienteId,
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
                _logger.LogError(dbEx, "Errore database durante l'aggiornamento dimensione quantità ingredienti {DimensioneId}/{PersonalizzazioneIngredienteId}", dimensioneId, personalizzazioneIngredienteId);
                return SafeInternalError("Errore durante l'aggiornamento");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante l'aggiornamento dimensione quantità ingredienti {DimensioneId}/{PersonalizzazioneIngredienteId}", dimensioneId, personalizzazioneIngredienteId);
                return SafeInternalError("Errore durante l'aggiornamento");
            }
        }

        // DELETE: api/DimensioneQuantitaIngredienti/5/10
        [HttpDelete("{dimensioneId}/{personalizzazioneIngredienteId}")]
        // [Authorize(Roles = "admin")] // ✅ KEYCLOAK READY - COMMENTATO PER TEST
        public async Task<ActionResult> Delete(int dimensioneId, int personalizzazioneIngredienteId)
        {
            try
            {
                if (dimensioneId <= 0 || personalizzazioneIngredienteId <= 0)
                    return SafeBadRequest("ID dimensione o personalizzazione ingrediente non validi");

                var dimensioneQuantita = await _repository.GetByIdAsync(dimensioneId, personalizzazioneIngredienteId);
                if (dimensioneQuantita == null)
                    return SafeNotFound("Dimensione quantità ingredienti");

                await _repository.DeleteAsync(dimensioneId, personalizzazioneIngredienteId);

                // ✅ AUDIT & SECURITY
                LogAuditTrail("DELETE", "DimensioneQuantitaIngredienti", $"{dimensioneId}/{personalizzazioneIngredienteId}");
                LogSecurityEvent("DimensioneQuantitaIngredientiDeleted", new
                {
                    DimensioneId = dimensioneId,
                    PersonalizzazioneIngredienteId = personalizzazioneIngredienteId,
                    UserId = GetCurrentUserIdOrDefault()
                });

                return NoContent();
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "Errore database durante l'eliminazione dimensione quantità ingredienti {DimensioneId}/{PersonalizzazioneIngredienteId}", dimensioneId, personalizzazioneIngredienteId);
                return SafeInternalError("Errore durante l'eliminazione");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante l'eliminazione dimensione quantità ingredienti {DimensioneId}/{PersonalizzazioneIngredienteId}", dimensioneId, personalizzazioneIngredienteId);
                return SafeInternalError("Errore durante l'eliminazione");
            }
        }
    }
}