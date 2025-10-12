using Microsoft.AspNetCore.Mvc;
using DTO;
using Repository.Interface;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BBltZen.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class IngredientiPersonalizzazioneController : ControllerBase
    {
        private readonly IIngredientiPersonalizzazioneRepository _repository;

        public IngredientiPersonalizzazioneController(IIngredientiPersonalizzazioneRepository repository)
        {
            _repository = repository;
        }

        /// <summary>
        /// Ottiene tutti gli ingredienti personalizzazione
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<IngredientiPersonalizzazioneDTO>>> GetAll()
        {
            try
            {
                var result = await _repository.GetAllAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Errore durante il recupero degli ingredienti personalizzazione: {ex.Message}");
            }
        }

        /// <summary>
        /// Ottiene un ingrediente personalizzazione specifico tramite ID
        /// </summary>
        [HttpGet("{ingredientePersId}")]
        public async Task<ActionResult<IngredientiPersonalizzazioneDTO>> GetById(int ingredientePersId)
        {
            try
            {
                var result = await _repository.GetByIdAsync(ingredientePersId);

                if (result == null)
                {
                    return NotFound($"Ingrediente personalizzazione con ID {ingredientePersId} non trovato");
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Errore durante il recupero dell'ingrediente personalizzazione: {ex.Message}");
            }
        }

        /// <summary>
        /// Ottiene gli ingredienti per personalizzazione custom
        /// </summary>
        [HttpGet("personalizzazione-custom/{persCustomId}")]
        public async Task<ActionResult<IEnumerable<IngredientiPersonalizzazioneDTO>>> GetByPersCustomId(int persCustomId)
        {
            try
            {
                var result = await _repository.GetByPersCustomIdAsync(persCustomId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Errore durante il recupero degli ingredienti per personalizzazione custom: {ex.Message}");
            }
        }

        /// <summary>
        /// Ottiene le personalizzazioni per ingrediente
        /// </summary>
        [HttpGet("ingrediente/{ingredienteId}")]
        public async Task<ActionResult<IEnumerable<IngredientiPersonalizzazioneDTO>>> GetByIngredienteId(int ingredienteId)
        {
            try
            {
                var result = await _repository.GetByIngredienteIdAsync(ingredienteId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Errore durante il recupero delle personalizzazioni per ingrediente: {ex.Message}");
            }
        }

        /// <summary>
        /// Ottiene un ingrediente personalizzazione per combinazione
        /// </summary>
        [HttpGet("combinazione/{persCustomId}/{ingredienteId}")]
        public async Task<ActionResult<IngredientiPersonalizzazioneDTO>> GetByCombinazione(int persCustomId, int ingredienteId)
        {
            try
            {
                var result = await _repository.GetByCombinazioneAsync(persCustomId, ingredienteId);

                if (result == null)
                {
                    return NotFound($"Ingrediente personalizzazione non trovato per la combinazione PersonalizzazioneCustom {persCustomId} e Ingrediente {ingredienteId}");
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Errore durante il recupero dell'ingrediente personalizzazione per combinazione: {ex.Message}");
            }
        }

        /// <summary>
        /// Crea un nuovo ingrediente personalizzazione
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<IngredientiPersonalizzazioneDTO>> Create(IngredientiPersonalizzazioneDTO ingredientiPersDto)
        {
            try
            {
                if (ingredientiPersDto == null)
                {
                    return BadRequest("Dati ingrediente personalizzazione non validi");
                }

                // Verifica se esiste già un record con lo stesso ID
                if (await _repository.ExistsAsync(ingredientiPersDto.IngredientePersId))
                {
                    return Conflict($"Esiste già un ingrediente personalizzazione con ID {ingredientiPersDto.IngredientePersId}");
                }

                // Verifica se esiste già la stessa combinazione
                if (await _repository.ExistsByCombinazioneAsync(ingredientiPersDto.PersCustomId, ingredientiPersDto.IngredienteId))
                {
                    return Conflict("Esiste già un ingrediente personalizzazione con la stessa combinazione di personalizzazione custom e ingrediente");
                }

                await _repository.AddAsync(ingredientiPersDto);

                return CreatedAtAction(nameof(GetById), new { ingredientePersId = ingredientiPersDto.IngredientePersId }, ingredientiPersDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Errore durante la creazione dell'ingrediente personalizzazione: {ex.Message}");
            }
        }

        /// <summary>
        /// Aggiorna un ingrediente personalizzazione esistente
        /// </summary>
        [HttpPut("{ingredientePersId}")]
        public async Task<ActionResult> Update(int ingredientePersId, IngredientiPersonalizzazioneDTO ingredientiPersDto)
        {
            try
            {
                if (ingredientiPersDto == null || ingredientePersId != ingredientiPersDto.IngredientePersId)
                {
                    return BadRequest("ID dell'ingrediente personalizzazione non corrisponde");
                }

                if (!await _repository.ExistsAsync(ingredientePersId))
                {
                    return NotFound($"Ingrediente personalizzazione con ID {ingredientePersId} non trovato");
                }

                await _repository.UpdateAsync(ingredientiPersDto);

                return NoContent();
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Errore durante l'aggiornamento dell'ingrediente personalizzazione: {ex.Message}");
            }
        }

        /// <summary>
        /// Elimina un ingrediente personalizzazione
        /// </summary>
        [HttpDelete("{ingredientePersId}")]
        public async Task<ActionResult> Delete(int ingredientePersId)
        {
            try
            {
                if (!await _repository.ExistsAsync(ingredientePersId))
                {
                    return NotFound($"Ingrediente personalizzazione con ID {ingredientePersId} non trovato");
                }

                await _repository.DeleteAsync(ingredientePersId);

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Errore durante l'eliminazione dell'ingrediente personalizzazione: {ex.Message}");
            }
        }
    }
}