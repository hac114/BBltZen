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
    public class PersonalizzazioneController : ControllerBase
    {
        private readonly IPersonalizzazioneRepository _personalizzazioneRepository;

        public PersonalizzazioneController(IPersonalizzazioneRepository personalizzazioneRepository)
        {
            _personalizzazioneRepository = personalizzazioneRepository;
        }

        // GET: api/Personalizzazione
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
                return StatusCode(500, $"Errore interno del server: {ex.Message}");
            }
        }

        // GET: api/Personalizzazione/attive
        [HttpGet("attive")]
        public async Task<ActionResult<IEnumerable<PersonalizzazioneDTO>>> GetAttive()
        {
            try
            {
                var personalizzazioni = await _personalizzazioneRepository.GetAttiveAsync();
                return Ok(personalizzazioni);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Errore interno del server: {ex.Message}");
            }
        }

        // GET: api/Personalizzazione/5
        [HttpGet("{id}")]
        public async Task<ActionResult<PersonalizzazioneDTO>> GetById(int id)
        {
            try
            {
                var personalizzazione = await _personalizzazioneRepository.GetByIdAsync(id);

                if (personalizzazione == null)
                {
                    return NotFound($"Personalizzazione con ID {id} non trovata");
                }

                return Ok(personalizzazione);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Errore interno del server: {ex.Message}");
            }
        }

        // POST: api/Personalizzazione
        [HttpPost]
        public async Task<ActionResult<PersonalizzazioneDTO>> Create(PersonalizzazioneDTO personalizzazioneDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Verifica se esiste già una personalizzazione con lo stesso nome
                var exists = await _personalizzazioneRepository.ExistsByNameAsync(personalizzazioneDto.Nome);
                if (exists)
                {
                    return BadRequest($"Esiste già una personalizzazione con il nome '{personalizzazioneDto.Nome}'");
                }

                await _personalizzazioneRepository.AddAsync(personalizzazioneDto);

                return CreatedAtAction(nameof(GetById), new { id = personalizzazioneDto.PersonalizzazioneId }, personalizzazioneDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Errore durante la creazione della personalizzazione: {ex.Message}");
            }
        }

        // PUT: api/Personalizzazione/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, PersonalizzazioneDTO personalizzazioneDto)
        {
            try
            {
                if (id != personalizzazioneDto.PersonalizzazioneId)
                {
                    return BadRequest("ID nell'URL non corrisponde all'ID nel corpo della richiesta");
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Verifica se la personalizzazione esiste
                var existingPersonalizzazione = await _personalizzazioneRepository.GetByIdAsync(id);
                if (existingPersonalizzazione == null)
                {
                    return NotFound($"Personalizzazione con ID {id} non trovata");
                }

                // Verifica se esiste già un'altra personalizzazione con lo stesso nome
                var existingByName = await _personalizzazioneRepository.ExistsByNameAsync(personalizzazioneDto.Nome);
                if (existingByName && existingPersonalizzazione.Nome != personalizzazioneDto.Nome)
                {
                    return BadRequest($"Esiste già una personalizzazione con il nome '{personalizzazioneDto.Nome}'");
                }

                await _personalizzazioneRepository.UpdateAsync(personalizzazioneDto);

                return NoContent();
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Errore durante l'aggiornamento della personalizzazione: {ex.Message}");
            }
        }

        // DELETE: api/Personalizzazione/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                // Verifica se la personalizzazione esiste
                var existingPersonalizzazione = await _personalizzazioneRepository.GetByIdAsync(id);
                if (existingPersonalizzazione == null)
                {
                    return NotFound($"Personalizzazione con ID {id} non trovata");
                }

                await _personalizzazioneRepository.DeleteAsync(id);

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Errore durante l'eliminazione della personalizzazione: {ex.Message}");
            }
        }

        // GET: api/Personalizzazione/exists/5
        [HttpGet("exists/{id}")]
        public async Task<ActionResult<bool>> Exists(int id)
        {
            try
            {
                var exists = await _personalizzazioneRepository.ExistsAsync(id);
                return Ok(exists);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Errore interno del server: {ex.Message}");
            }
        }

        // GET: api/Personalizzazione/exists/name/NomePersonalizzazione
        [HttpGet("exists/name/{nome}")]
        public async Task<ActionResult<bool>> ExistsByName(string nome)
        {
            try
            {
                var exists = await _personalizzazioneRepository.ExistsByNameAsync(nome);
                return Ok(exists);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Errore interno del server: {ex.Message}");
            }
        }
    }
}