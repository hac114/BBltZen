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
    public class IngredientiPersonalizzazioneController : SecureBaseController
    {
        private readonly IIngredientiPersonalizzazioneRepository _repository;
        private readonly BubbleTeaContext _context;

        public IngredientiPersonalizzazioneController(
            IIngredientiPersonalizzazioneRepository repository,
            BubbleTeaContext context,
            IWebHostEnvironment environment,
            ILogger<IngredientiPersonalizzazioneController> logger)
            : base(environment, logger)
        {
            _repository = repository;
            _context = context;
        }

        /// <summary>
        /// Ottiene tutti gli ingredienti personalizzazione
        /// </summary>
        [HttpGet]
        [AllowAnonymous] // ✅ ESPLICITO PER ENDPOINT GET
        public async Task<ActionResult<IEnumerable<IngredientiPersonalizzazioneDTO>>> GetAll()
        {
            try
            {
                var result = await _repository.GetAllAsync();
                return Ok(result);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore durante il recupero di tutti gli ingredienti personalizzazione");
                return SafeInternalError<IEnumerable<IngredientiPersonalizzazioneDTO>>("Errore durante il recupero degli ingredienti personalizzazione");
            }
        }

        /// <summary>
        /// Ottiene un ingrediente personalizzazione specifico tramite ID
        /// </summary>
        [HttpGet("{ingredientePersId}")]
        [AllowAnonymous] // ✅ ESPLICITO PER ENDPOINT GET
        public async Task<ActionResult<IngredientiPersonalizzazioneDTO>> GetById(int ingredientePersId)
        {
            try
            {
                if (ingredientePersId <= 0)
                    return SafeBadRequest<IngredientiPersonalizzazioneDTO>("ID ingrediente personalizzazione non valido");

                var result = await _repository.GetByIdAsync(ingredientePersId);

                if (result == null)
                    return SafeNotFound<IngredientiPersonalizzazioneDTO>("Ingrediente personalizzazione");

                return Ok(result);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore durante il recupero dell'ingrediente personalizzazione {IngredientePersId}", ingredientePersId);
                return SafeInternalError<IngredientiPersonalizzazioneDTO>("Errore durante il recupero dell'ingrediente personalizzazione");
            }
        }

        /// <summary>
        /// Ottiene gli ingredienti per personalizzazione custom
        /// </summary>
        [HttpGet("personalizzazione-custom/{persCustomId}")]
        [AllowAnonymous] // ✅ ESPLICITO PER ENDPOINT GET
        public async Task<ActionResult<IEnumerable<IngredientiPersonalizzazioneDTO>>> GetByPersCustomId(int persCustomId)
        {
            try
            {
                if (persCustomId <= 0)
                    return SafeBadRequest<IEnumerable<IngredientiPersonalizzazioneDTO>>("ID personalizzazione custom non valido");

                // ✅ Verifica se la personalizzazione custom esiste
                var persCustomEsiste = await _context.PersonalizzazioneCustom.AnyAsync(p => p.PersCustomId == persCustomId);
                if (!persCustomEsiste)
                    return SafeNotFound<IEnumerable<IngredientiPersonalizzazioneDTO>>("Personalizzazione custom");

                var result = await _repository.GetByPersCustomIdAsync(persCustomId);
                return Ok(result);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore durante il recupero degli ingredienti per personalizzazione custom {PersCustomId}", persCustomId);
                return SafeInternalError<IEnumerable<IngredientiPersonalizzazioneDTO>>("Errore durante il recupero degli ingredienti per personalizzazione custom");
            }
        }

        /// <summary>
        /// Ottiene le personalizzazioni per ingrediente
        /// </summary>
        [HttpGet("ingrediente/{ingredienteId}")]
        [AllowAnonymous] // ✅ ESPLICITO PER ENDPOINT GET
        public async Task<ActionResult<IEnumerable<IngredientiPersonalizzazioneDTO>>> GetByIngredienteId(int ingredienteId)
        {
            try
            {
                if (ingredienteId <= 0)
                    return SafeBadRequest<IEnumerable<IngredientiPersonalizzazioneDTO>>("ID ingrediente non valido");

                // ✅ Verifica se l'ingrediente esiste
                var ingredienteEsiste = await _context.Ingrediente.AnyAsync(i => i.IngredienteId == ingredienteId);
                if (!ingredienteEsiste)
                    return SafeNotFound<IEnumerable<IngredientiPersonalizzazioneDTO>>("Ingrediente");

                var result = await _repository.GetByIngredienteIdAsync(ingredienteId);
                return Ok(result);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore durante il recupero delle personalizzazioni per ingrediente {IngredienteId}", ingredienteId);
                return SafeInternalError<IEnumerable<IngredientiPersonalizzazioneDTO>>("Errore durante il recupero delle personalizzazioni per ingrediente");
            }
        }

        /// <summary>
        /// Ottiene un ingrediente personalizzazione per combinazione
        /// </summary>
        [HttpGet("combinazione/{persCustomId}/{ingredienteId}")]
        [AllowAnonymous] // ✅ ESPLICITO PER ENDPOINT GET
        public async Task<ActionResult<IngredientiPersonalizzazioneDTO>> GetByCombinazione(int persCustomId, int ingredienteId)
        {
            try
            {
                if (persCustomId <= 0)
                    return SafeBadRequest<IngredientiPersonalizzazioneDTO>("ID personalizzazione custom non valido");

                if (ingredienteId <= 0)
                    return SafeBadRequest<IngredientiPersonalizzazioneDTO>("ID ingrediente non valido");

                var result = await _repository.GetByCombinazioneAsync(persCustomId, ingredienteId);

                if (result == null)
                    return SafeNotFound<IngredientiPersonalizzazioneDTO>("Ingrediente personalizzazione per combinazione");

                return Ok(result);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore durante il recupero dell'ingrediente personalizzazione per combinazione {PersCustomId}/{IngredienteId}", persCustomId, ingredienteId);
                return SafeInternalError<IngredientiPersonalizzazioneDTO>("Errore durante il recupero dell'ingrediente personalizzazione per combinazione");
            }
        }

        /// <summary>
        /// Crea un nuovo ingrediente personalizzazione
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "admin,barista")] // ✅ Solo admin e barista possono creare
        public async Task<ActionResult<IngredientiPersonalizzazioneDTO>> Create([FromBody] IngredientiPersonalizzazioneDTO ingredientiPersDto)
        {
            try
            {
                if (!IsModelValid(ingredientiPersDto))
                    return SafeBadRequest<IngredientiPersonalizzazioneDTO>("Dati ingrediente personalizzazione non validi");

                // ✅ Verifica se la personalizzazione custom esiste
                var persCustomEsiste = await _context.PersonalizzazioneCustom.AnyAsync(p => p.PersCustomId == ingredientiPersDto.PersCustomId);
                if (!persCustomEsiste)
                    return SafeBadRequest<IngredientiPersonalizzazioneDTO>("Personalizzazione custom non trovata");

                // ✅ Verifica se l'ingrediente esiste
                var ingredienteEsiste = await _context.Ingrediente.AnyAsync(i => i.IngredienteId == ingredientiPersDto.IngredienteId);
                if (!ingredienteEsiste)
                    return SafeBadRequest<IngredientiPersonalizzazioneDTO>("Ingrediente non trovato");

                // ✅ Verifica se esiste già un record con lo stesso ID
                if (await _repository.ExistsAsync(ingredientiPersDto.IngredientePersId))
                    return SafeBadRequest<IngredientiPersonalizzazioneDTO>("Esiste già un ingrediente personalizzazione con questo ID");

                // ✅ Verifica se esiste già la stessa combinazione
                if (await _repository.ExistsByCombinazioneAsync(ingredientiPersDto.PersCustomId, ingredientiPersDto.IngredienteId))
                    return SafeBadRequest<IngredientiPersonalizzazioneDTO>("Esiste già un ingrediente personalizzazione con la stessa combinazione");

                await _repository.AddAsync(ingredientiPersDto);

                // ✅ Audit trail
                LogAuditTrail("CREATE_INGREDIENTE_PERSONALIZZAZIONE", "IngredientiPersonalizzazione", ingredientiPersDto.IngredientePersId.ToString());

                // ✅ Security event completo con timestamp
                LogSecurityEvent("IngredientePersonalizzazioneCreated", new
                {
                    IngredientePersId = ingredientiPersDto.IngredientePersId,
                    PersCustomId = ingredientiPersDto.PersCustomId,
                    IngredienteId = ingredientiPersDto.IngredienteId,
                    User = User.Identity?.Name,
                    Timestamp = DateTime.UtcNow
                });

                return CreatedAtAction(nameof(GetById),
                    new { ingredientePersId = ingredientiPersDto.IngredientePersId },
                    ingredientiPersDto);
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "Errore database durante la creazione dell'ingrediente personalizzazione");
                return SafeInternalError<IngredientiPersonalizzazioneDTO>("Errore durante il salvataggio dell'ingrediente personalizzazione");
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore durante la creazione dell'ingrediente personalizzazione");
                return SafeInternalError<IngredientiPersonalizzazioneDTO>("Errore durante la creazione dell'ingrediente personalizzazione");
            }
        }

        /// <summary>
        /// Aggiorna un ingrediente personalizzazione esistente
        /// </summary>
        [HttpPut("{ingredientePersId}")]
        [Authorize(Roles = "admin,barista")] // ✅ Solo admin e barista possono modificare
        public async Task<ActionResult> Update(int ingredientePersId, [FromBody] IngredientiPersonalizzazioneDTO ingredientiPersDto)
        {
            try
            {
                if (ingredientePersId <= 0)
                    return SafeBadRequest("ID ingrediente personalizzazione non valido");

                if (ingredientePersId != ingredientiPersDto.IngredientePersId)
                    return SafeBadRequest("ID ingrediente personalizzazione non corrispondente");

                if (!IsModelValid(ingredientiPersDto))
                    return SafeBadRequest("Dati ingrediente personalizzazione non validi");

                var existing = await _repository.GetByIdAsync(ingredientePersId);
                if (existing == null)
                    return SafeNotFound("Ingrediente personalizzazione");

                // ✅ Verifica se la personalizzazione custom esiste
                var persCustomEsiste = await _context.PersonalizzazioneCustom.AnyAsync(p => p.PersCustomId == ingredientiPersDto.PersCustomId);
                if (!persCustomEsiste)
                    return SafeBadRequest("Personalizzazione custom non trovata");

                // ✅ Verifica se l'ingrediente esiste
                var ingredienteEsiste = await _context.Ingrediente.AnyAsync(i => i.IngredienteId == ingredientiPersDto.IngredienteId);
                if (!ingredienteEsiste)
                    return SafeBadRequest("Ingrediente non trovato");

                // ✅ Verifica se esiste già un'altra combinazione
                var combinazioneDuplicata = await _repository.ExistsByCombinazioneAsync(ingredientiPersDto.PersCustomId, ingredientiPersDto.IngredienteId);
                if (combinazioneDuplicata &&
                    (ingredientiPersDto.PersCustomId != existing.PersCustomId ||
                     ingredientiPersDto.IngredienteId != existing.IngredienteId))
                    return SafeBadRequest("Esiste già un'altra combinazione per questa personalizzazione custom e ingrediente");

                await _repository.UpdateAsync(ingredientiPersDto);

                // ✅ Audit trail
                LogAuditTrail("UPDATE_INGREDIENTE_PERSONALIZZAZIONE", "IngredientiPersonalizzazione", ingredientiPersDto.IngredientePersId.ToString());

                // ✅ Security event completo con timestamp
                LogSecurityEvent("IngredientePersonalizzazioneUpdated", new
                {
                    IngredientePersId = ingredientiPersDto.IngredientePersId,
                    OldPersCustomId = existing.PersCustomId,
                    NewPersCustomId = ingredientiPersDto.PersCustomId,
                    OldIngredienteId = existing.IngredienteId,
                    NewIngredienteId = ingredientiPersDto.IngredienteId,
                    User = User.Identity?.Name,
                    Timestamp = DateTime.UtcNow
                });

                return NoContent();
            }
            catch (System.ArgumentException ex)
            {
                _logger.LogWarning(ex, "Tentativo di aggiornamento di un ingrediente personalizzazione non trovato {IngredientePersId}", ingredientePersId);
                return SafeNotFound("Ingrediente personalizzazione");
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "Errore database durante l'aggiornamento dell'ingrediente personalizzazione {IngredientePersId}", ingredientePersId);
                return SafeInternalError("Errore durante l'aggiornamento dell'ingrediente personalizzazione");
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore durante l'aggiornamento dell'ingrediente personalizzazione {IngredientePersId}", ingredientePersId);
                return SafeInternalError("Errore durante l'aggiornamento dell'ingrediente personalizzazione");
            }
        }

        /// <summary>
        /// Elimina un ingrediente personalizzazione
        /// </summary>
        [HttpDelete("{ingredientePersId}")]
        [Authorize(Roles = "admin")] // ✅ Solo admin può eliminare
        public async Task<ActionResult> Delete(int ingredientePersId)
        {
            try
            {
                if (ingredientePersId <= 0)
                    return SafeBadRequest("ID ingrediente personalizzazione non valido");

                var existing = await _repository.GetByIdAsync(ingredientePersId);
                if (existing == null)
                    return SafeNotFound("Ingrediente personalizzazione");

                await _repository.DeleteAsync(ingredientePersId);

                // ✅ Audit trail
                LogAuditTrail("DELETE_INGREDIENTE_PERSONALIZZAZIONE", "IngredientiPersonalizzazione", ingredientePersId.ToString());

                // ✅ Security event completo con timestamp
                LogSecurityEvent("IngredientePersonalizzazioneDeleted", new
                {
                    IngredientePersId = ingredientePersId,
                    PersCustomId = existing.PersCustomId,
                    IngredienteId = existing.IngredienteId,
                    User = User.Identity?.Name,
                    Timestamp = DateTime.UtcNow
                });

                return NoContent();
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "Errore database durante l'eliminazione dell'ingrediente personalizzazione {IngredientePersId}", ingredientePersId);
                return SafeInternalError("Errore durante l'eliminazione dell'ingrediente personalizzazione");
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore durante l'eliminazione dell'ingrediente personalizzazione {IngredientePersId}", ingredientePersId);
                return SafeInternalError("Errore durante l'eliminazione dell'ingrediente personalizzazione");
            }
        }
    }
}