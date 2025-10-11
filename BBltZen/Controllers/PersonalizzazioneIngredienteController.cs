using Microsoft.AspNetCore.Mvc;
using Repository.Interface;
using DTO;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace YourNamespace.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PersonalizzazioneIngredienteController : ControllerBase
    {
        private readonly IPersonalizzazioneIngredienteRepository _personalizzazioneIngredienteRepository;

        public PersonalizzazioneIngredienteController(IPersonalizzazioneIngredienteRepository personalizzazioneIngredienteRepository)
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
            catch (Exception ex)
            {
                return StatusCode(500, $"Errore interno del server: {ex.Message}");
            }
        }

        // GET: api/PersonalizzazioneIngrediente/5
        [HttpGet("{id}")]
        public async Task<ActionResult<PersonalizzazioneIngredienteDTO>> GetById(int id)
        {
            try
            {
                var personalizzazioneIngrediente = await _personalizzazioneIngredienteRepository.GetByIdAsync(id);

                if (personalizzazioneIngrediente == null)
                {
                    return NotFound($"Associazione Personalizzazione-Ingrediente con ID {id} non trovata");
                }

                return Ok(personalizzazioneIngrediente);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Errore interno del server: {ex.Message}");
            }
        }

        // GET: api/PersonalizzazioneIngrediente/personalizzazione/5
        [HttpGet("personalizzazione/{personalizzazioneId}")]
        public async Task<ActionResult<IEnumerable<PersonalizzazioneIngredienteDTO>>> GetByPersonalizzazioneId(int personalizzazioneId)
        {
            try
            {
                var personalizzazioneIngredienti = await _personalizzazioneIngredienteRepository.GetByPersonalizzazioneIdAsync(personalizzazioneId);
                return Ok(personalizzazioneIngredienti);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Errore interno del server: {ex.Message}");
            }
        }

        // GET: api/PersonalizzazioneIngrediente/ingrediente/5
        [HttpGet("ingrediente/{ingredienteId}")]
        public async Task<ActionResult<IEnumerable<PersonalizzazioneIngredienteDTO>>> GetByIngredienteId(int ingredienteId)
        {
            try
            {
                var personalizzazioneIngredienti = await _personalizzazioneIngredienteRepository.GetByIngredienteIdAsync(ingredienteId);
                return Ok(personalizzazioneIngredienti);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Errore interno del server: {ex.Message}");
            }
        }

        // GET: api/PersonalizzazioneIngrediente/personalizzazione/5/ingrediente/5
        [HttpGet("personalizzazione/{personalizzazioneId}/ingrediente/{ingredienteId}")]
        public async Task<ActionResult<PersonalizzazioneIngredienteDTO>> GetByPersonalizzazioneAndIngrediente(int personalizzazioneId, int ingredienteId)
        {
            try
            {
                var personalizzazioneIngrediente = await _personalizzazioneIngredienteRepository.GetByPersonalizzazioneAndIngredienteAsync(personalizzazioneId, ingredienteId);

                if (personalizzazioneIngrediente == null)
                {
                    return NotFound($"Associazione non trovata per personalizzazione {personalizzazioneId} e ingrediente {ingredienteId}");
                }

                return Ok(personalizzazioneIngrediente);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Errore interno del server: {ex.Message}");
            }
        }

        // POST: api/PersonalizzazioneIngrediente
        [HttpPost]
        public async Task<ActionResult<PersonalizzazioneIngredienteDTO>> Create(PersonalizzazioneIngredienteDTO personalizzazioneIngredienteDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Verifica se l'associazione esiste già
                var exists = await _personalizzazioneIngredienteRepository.ExistsByPersonalizzazioneAndIngredienteAsync(
                    personalizzazioneIngredienteDto.PersonalizzazioneId,
                    personalizzazioneIngredienteDto.IngredienteId);

                if (exists)
                {
                    return BadRequest($"Esiste già un'associazione per questa personalizzazione e ingrediente");
                }

                await _personalizzazioneIngredienteRepository.AddAsync(personalizzazioneIngredienteDto);

                return CreatedAtAction(nameof(GetById), new { id = personalizzazioneIngredienteDto.PersonalizzazioneIngredienteId }, personalizzazioneIngredienteDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Errore durante la creazione dell'associazione: {ex.Message}");
            }
        }

        // PUT: api/PersonalizzazioneIngrediente/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, PersonalizzazioneIngredienteDTO personalizzazioneIngredienteDto)
        {
            try
            {
                if (id != personalizzazioneIngredienteDto.PersonalizzazioneIngredienteId)
                {
                    return BadRequest("ID nell'URL non corrisponde all'ID nel corpo della richiesta");
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Verifica se l'associazione esiste
                var existingAssociazione = await _personalizzazioneIngredienteRepository.GetByIdAsync(id);
                if (existingAssociazione == null)
                {
                    return NotFound($"Associazione Personalizzazione-Ingrediente con ID {id} non trovata");
                }

                await _personalizzazioneIngredienteRepository.UpdateAsync(personalizzazioneIngredienteDto);

                return NoContent();
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Errore durante l'aggiornamento dell'associazione: {ex.Message}");
            }
        }

        // DELETE: api/PersonalizzazioneIngrediente/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                // Verifica se l'associazione esiste
                var existingAssociazione = await _personalizzazioneIngredienteRepository.GetByIdAsync(id);
                if (existingAssociazione == null)
                {
                    return NotFound($"Associazione Personalizzazione-Ingrediente con ID {id} non trovata");
                }

                await _personalizzazioneIngredienteRepository.DeleteAsync(id);

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Errore durante l'eliminazione dell'associazione: {ex.Message}");
            }
        }

        // DELETE: api/PersonalizzazioneIngrediente/personalizzazione/5/ingrediente/5
        [HttpDelete("personalizzazione/{personalizzazioneId}/ingrediente/{ingredienteId}")]
        public async Task<IActionResult> DeleteByPersonalizzazioneAndIngrediente(int personalizzazioneId, int ingredienteId)
        {
            try
            {
                // Verifica se l'associazione esiste
                var existingAssociazione = await _personalizzazioneIngredienteRepository.GetByPersonalizzazioneAndIngredienteAsync(personalizzazioneId, ingredienteId);
                if (existingAssociazione == null)
                {
                    return NotFound($"Associazione non trovata per personalizzazione {personalizzazioneId} e ingrediente {ingredienteId}");
                }

                await _personalizzazioneIngredienteRepository.DeleteByPersonalizzazioneAndIngredienteAsync(personalizzazioneId, ingredienteId);

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Errore durante l'eliminazione dell'associazione: {ex.Message}");
            }
        }

        // GET: api/PersonalizzazioneIngrediente/exists/5
        [HttpGet("exists/{id}")]
        public async Task<ActionResult<bool>> Exists(int id)
        {
            try
            {
                var exists = await _personalizzazioneIngredienteRepository.ExistsAsync(id);
                return Ok(exists);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Errore interno del server: {ex.Message}");
            }
        }

        // GET: api/PersonalizzazioneIngrediente/exists/personalizzazione/5/ingrediente/5
        [HttpGet("exists/personalizzazione/{personalizzazioneId}/ingrediente/{ingredienteId}")]
        public async Task<ActionResult<bool>> ExistsByPersonalizzazioneAndIngrediente(int personalizzazioneId, int ingredienteId)
        {
            try
            {
                var exists = await _personalizzazioneIngredienteRepository.ExistsByPersonalizzazioneAndIngredienteAsync(personalizzazioneId, ingredienteId);
                return Ok(exists);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Errore interno del server: {ex.Message}");
            }
        }

        // GET: api/PersonalizzazioneIngrediente/count/personalizzazione/5
        [HttpGet("count/personalizzazione/{personalizzazioneId}")]
        public async Task<ActionResult<int>> GetCountByPersonalizzazione(int personalizzazioneId)
        {
            try
            {
                var count = await _personalizzazioneIngredienteRepository.GetCountByPersonalizzazioneAsync(personalizzazioneId);
                return Ok(count);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Errore interno del server: {ex.Message}");
            }
        }
    }
}