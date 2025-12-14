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
            catch (Exception ex)
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
            catch (Exception ex)
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
        //[Authorize(Roles = "admin,barista")] // ✅ Solo admin e barista possono creare
        [AllowAnonymous]
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

                // ✅ Verifica se esiste già la stessa combinazione
                if (await _repository.ExistsByCombinazioneAsync(ingredientiPersDto.PersCustomId, ingredientiPersDto.IngredienteId))
                    return SafeBadRequest<IngredientiPersonalizzazioneDTO>("Esiste già un ingrediente personalizzazione con la stessa combinazione");

                // ✅ CORREZIONE: USA IL RISULTATO DI AddAsync (PATTERN STANDARD)
                var result = await _repository.AddAsync(ingredientiPersDto);

                // ✅ AUDIT & SECURITY OTTIMIZZATO PER VS
                LogAuditTrail("CREATE", "IngredientiPersonalizzazione", result.IngredientePersId.ToString());
                LogSecurityEvent("IngredientiPersonalizzazioneCreated", new
                {
                    result.IngredientePersId,
                    result.PersCustomId,
                    result.IngredienteId,
                    UserId = GetCurrentUserIdOrDefault()
                });

                return CreatedAtAction(nameof(GetById), new { ingredientePersId = result.IngredientePersId }, result);
            }
            catch (ArgumentException argEx)
            {
                return SafeBadRequest<IngredientiPersonalizzazioneDTO>(argEx.Message);
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "Errore database durante la creazione dell'ingrediente personalizzazione");
                return SafeInternalError<IngredientiPersonalizzazioneDTO>("Errore durante il salvataggio");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante la creazione dell'ingrediente personalizzazione");
                return SafeInternalError<IngredientiPersonalizzazioneDTO>("Errore durante la creazione");
            }
        }

        /// <summary>
        /// Aggiorna un ingrediente personalizzazione esistente
        /// </summary>
        [HttpPut("{ingredientePersId}")]
        //[Authorize(Roles = "admin,barista")] // ✅ Solo admin e barista possono modificare
        [AllowAnonymous]
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

                // ✅ VERIFICA ESISTENZA
                if (!await _repository.ExistsAsync(ingredientePersId))
                    return SafeNotFound("Ingrediente personalizzazione");

                // ✅ Verifica se la personalizzazione custom esiste
                var persCustomEsiste = await _context.PersonalizzazioneCustom.AnyAsync(p => p.PersCustomId == ingredientiPersDto.PersCustomId);
                if (!persCustomEsiste)
                    return SafeBadRequest("Personalizzazione custom non trovata");

                // ✅ Verifica se l'ingrediente esiste
                var ingredienteEsiste = await _context.Ingrediente.AnyAsync(i => i.IngredienteId == ingredientiPersDto.IngredienteId);
                if (!ingredienteEsiste)
                    return SafeBadRequest("Ingrediente non trovato");

                await _repository.UpdateAsync(ingredientiPersDto);

                // ✅ AUDIT & SECURITY OTTIMIZZATO PER VS
                LogAuditTrail("UPDATE", "IngredientiPersonalizzazione", ingredientiPersDto.IngredientePersId.ToString());
                LogSecurityEvent("IngredientiPersonalizzazioneUpdated", new
                {
                    ingredientiPersDto.IngredientePersId,
                    ingredientiPersDto.PersCustomId,
                    ingredientiPersDto.IngredienteId,
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
                _logger.LogError(dbEx, "Errore database durante l'aggiornamento dell'ingrediente personalizzazione {IngredientePersId}", ingredientePersId);
                return SafeInternalError("Errore durante l'aggiornamento");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante l'aggiornamento dell'ingrediente personalizzazione {IngredientePersId}", ingredientePersId);
                return SafeInternalError("Errore durante l'aggiornamento");
            }
        }

        /// <summary>
        /// Elimina un ingrediente personalizzazione
        /// </summary>
        [HttpDelete("{ingredientePersId}")]
        //[Authorize(Roles = "admin")] // ✅ Solo admin può eliminare
        [AllowAnonymous]
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

                // ✅ AUDIT & SECURITY OTTIMIZZATO PER VS
                LogAuditTrail("DELETE", "IngredientiPersonalizzazione", ingredientePersId.ToString());
                LogSecurityEvent("IngredientiPersonalizzazioneDeleted", new
                {
                    IngredientePersId = ingredientePersId,
                    PersCustomId = existing.PersCustomId,
                    IngredienteId = existing.IngredienteId,
                    UserId = GetCurrentUserIdOrDefault()
                });

                return NoContent();
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "Errore database durante l'eliminazione dell'ingrediente personalizzazione {IngredientePersId}", ingredientePersId);
                return SafeInternalError("Errore durante l'eliminazione");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante l'eliminazione dell'ingrediente personalizzazione {IngredientePersId}", ingredientePersId);
                return SafeInternalError("Errore durante l'eliminazione");
            }
        }
    }
}