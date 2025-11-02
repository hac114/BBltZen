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
    public class IngredientiPersonalizzazioneController : SecureBaseController
    {
        private readonly IIngredientiPersonalizzazioneRepository _repository;

        public IngredientiPersonalizzazioneController(
            IIngredientiPersonalizzazioneRepository repository,
            IWebHostEnvironment environment,
            ILogger<IngredientiPersonalizzazioneController> logger)
            : base(environment, logger)
        {
            _repository = repository;
        }

        /// <summary>
        /// Ottiene tutti gli ingredienti personalizzazione
        /// </summary>
        [HttpGet]
        [AllowAnonymous] // ✅ PERMESSO A TUTTI PER TEST
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
                return SafeInternalError("Errore durante il recupero degli ingredienti personalizzazione");
            }
        }

        /// <summary>
        /// Ottiene un ingrediente personalizzazione specifico tramite ID
        /// </summary>
        [HttpGet("{ingredientePersId}")]
        [AllowAnonymous] // ✅ PERMESSO A TUTTI PER TEST
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
                return SafeInternalError("Errore durante il recupero dell'ingrediente personalizzazione");
            }
        }

        /// <summary>
        /// Ottiene gli ingredienti per personalizzazione custom
        /// </summary>
        [HttpGet("personalizzazione-custom/{persCustomId}")]
        [AllowAnonymous] // ✅ PERMESSO A TUTTI PER TEST
        public async Task<ActionResult<IEnumerable<IngredientiPersonalizzazioneDTO>>> GetByPersCustomId(int persCustomId)
        {
            try
            {
                if (persCustomId <= 0)
                    return SafeBadRequest<IEnumerable<IngredientiPersonalizzazioneDTO>>("ID personalizzazione custom non valido");

                var result = await _repository.GetByPersCustomIdAsync(persCustomId);
                return Ok(result);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore durante il recupero degli ingredienti per personalizzazione custom {PersCustomId}", persCustomId);
                return SafeInternalError("Errore durante il recupero degli ingredienti per personalizzazione custom");
            }
        }

        /// <summary>
        /// Ottiene le personalizzazioni per ingrediente
        /// </summary>
        [HttpGet("ingrediente/{ingredienteId}")]
        [AllowAnonymous] // ✅ PERMESSO A TUTTI PER TEST
        public async Task<ActionResult<IEnumerable<IngredientiPersonalizzazioneDTO>>> GetByIngredienteId(int ingredienteId)
        {
            try
            {
                if (ingredienteId <= 0)
                    return SafeBadRequest<IEnumerable<IngredientiPersonalizzazioneDTO>>("ID ingrediente non valido");

                var result = await _repository.GetByIngredienteIdAsync(ingredienteId);
                return Ok(result);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore durante il recupero delle personalizzazioni per ingrediente {IngredienteId}", ingredienteId);
                return SafeInternalError("Errore durante il recupero delle personalizzazioni per ingrediente");
            }
        }

        /// <summary>
        /// Ottiene un ingrediente personalizzazione per combinazione
        /// </summary>
        [HttpGet("combinazione/{persCustomId}/{ingredienteId}")]
        [AllowAnonymous] // ✅ PERMESSO A TUTTI PER TEST
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
                return SafeInternalError("Errore durante il recupero dell'ingrediente personalizzazione per combinazione");
            }
        }

        /// <summary>
        /// Crea un nuovo ingrediente personalizzazione
        /// </summary>
        [HttpPost]
        //[Authorize(Roles = "admin,barista")] // ✅ COMMENTATO PER TEST
        [AllowAnonymous] // ✅ TEMPORANEAMENTE PERMESSO A TUTTI PER TEST
        public async Task<ActionResult<IngredientiPersonalizzazioneDTO>> Create(IngredientiPersonalizzazioneDTO ingredientiPersDto)
        {
            try
            {
                if (!IsModelValid(ingredientiPersDto))
                    return SafeBadRequest<IngredientiPersonalizzazioneDTO>("Dati ingrediente personalizzazione non validi");

                // Verifica se esiste già un record con lo stesso ID
                if (await _repository.ExistsAsync(ingredientiPersDto.IngredientePersId))
                    return Conflict($"Esiste già un ingrediente personalizzazione con ID {ingredientiPersDto.IngredientePersId}");

                // Verifica se esiste già la stessa combinazione
                if (await _repository.ExistsByCombinazioneAsync(ingredientiPersDto.PersCustomId, ingredientiPersDto.IngredienteId))
                    return Conflict("Esiste già un ingrediente personalizzazione con la stessa combinazione di personalizzazione custom e ingrediente");

                await _repository.AddAsync(ingredientiPersDto);

                // ✅ Audit trail
                LogAuditTrail("CREATE_INGREDIENTE_PERSONALIZZAZIONE", "IngredientiPersonalizzazione", ingredientiPersDto.IngredientePersId.ToString());
                LogSecurityEvent("IngredientePersonalizzazioneCreated", new
                {
                    IngredientePersId = ingredientiPersDto.IngredientePersId,
                    PersCustomId = ingredientiPersDto.PersCustomId,
                    IngredienteId = ingredientiPersDto.IngredienteId
                });

                return CreatedAtAction(nameof(GetById),
                    new { ingredientePersId = ingredientiPersDto.IngredientePersId },
                    ingredientiPersDto);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore durante la creazione dell'ingrediente personalizzazione");
                return SafeInternalError("Errore durante la creazione dell'ingrediente personalizzazione");
            }
        }

        /// <summary>
        /// Aggiorna un ingrediente personalizzazione esistente
        /// </summary>
        [HttpPut("{ingredientePersId}")]
        //[Authorize(Roles = "admin,barista")] // ✅ COMMENTATO PER TEST
        [AllowAnonymous] // ✅ TEMPORANEAMENTE PERMESSO A TUTTI PER TEST
        public async Task<ActionResult> Update(int ingredientePersId, IngredientiPersonalizzazioneDTO ingredientiPersDto)
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

                await _repository.UpdateAsync(ingredientiPersDto);

                // ✅ Audit trail
                LogAuditTrail("UPDATE_INGREDIENTE_PERSONALIZZAZIONE", "IngredientiPersonalizzazione", ingredientiPersDto.IngredientePersId.ToString());
                LogSecurityEvent("IngredientePersonalizzazioneUpdated", new
                {
                    IngredientePersId = ingredientiPersDto.IngredientePersId,
                    User = User.Identity?.Name
                });

                return NoContent();
            }
            catch (System.ArgumentException ex)
            {
                _logger.LogWarning(ex, "Tentativo di aggiornamento di un ingrediente personalizzazione non trovato {IngredientePersId}", ingredientePersId);
                return SafeNotFound("Ingrediente personalizzazione");
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
        //[Authorize(Roles = "admin")] // ✅ COMMENTATO PER TEST
        [AllowAnonymous] // ✅ TEMPORANEAMENTE PERMESSO A TUTTI PER TEST
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
                LogSecurityEvent("IngredientePersonalizzazioneDeleted", new
                {
                    IngredientePersId = ingredientePersId,
                    User = User.Identity?.Name
                });

                return NoContent();
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore durante l'eliminazione dell'ingrediente personalizzazione {IngredientePersId}", ingredientePersId);
                return SafeInternalError("Errore durante l'eliminazione dell'ingrediente personalizzazione");
            }
        }
    }
}