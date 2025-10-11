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
    public class IngredienteController : ControllerBase
    {
        private readonly IIngredienteRepository _ingredienteRepository;

        public IngredienteController(IIngredienteRepository ingredienteRepository)
        {
            _ingredienteRepository = ingredienteRepository;
        }

        // GET: api/Ingrediente
        [HttpGet]
        public async Task<ActionResult<IEnumerable<IngredienteDTO>>> GetAll()
        {
            try
            {
                var ingredienti = await _ingredienteRepository.GetAllAsync();
                return Ok(ingredienti);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Errore interno del server: {ex.Message}");
            }
        }

        // GET: api/Ingrediente/5
        [HttpGet("{id}")]
        public async Task<ActionResult<IngredienteDTO>> GetById(int id)
        {
            try
            {
                var ingrediente = await _ingredienteRepository.GetByIdAsync(id);

                if (ingrediente == null)
                {
                    return NotFound($"Ingrediente con ID {id} non trovato");
                }

                return Ok(ingrediente);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Errore interno del server: {ex.Message}");
            }
        }

        // GET: api/Ingrediente/categoria/5
        [HttpGet("categoria/{categoriaId}")]
        public async Task<ActionResult<IEnumerable<IngredienteDTO>>> GetByCategoria(int categoriaId)
        {
            try
            {
                var ingredienti = await _ingredienteRepository.GetByCategoriaAsync(categoriaId);
                return Ok(ingredienti);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Errore interno del server: {ex.Message}");
            }
        }

        // GET: api/Ingrediente/disponibili
        [HttpGet("disponibili")]
        public async Task<ActionResult<IEnumerable<IngredienteDTO>>> GetDisponibili()
        {
            try
            {
                var ingredienti = await _ingredienteRepository.GetDisponibiliAsync();
                return Ok(ingredienti);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Errore interno del server: {ex.Message}");
            }
        }

        // POST: api/Ingrediente
        [HttpPost]
        public async Task<ActionResult<IngredienteDTO>> Create(IngredienteDTO ingredienteDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                await _ingredienteRepository.AddAsync(ingredienteDto);

                return CreatedAtAction(nameof(GetById), new { id = ingredienteDto.IngredienteId }, ingredienteDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Errore durante la creazione dell'ingrediente: {ex.Message}");
            }
        }

        // PUT: api/Ingrediente/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, IngredienteDTO ingredienteDto)
        {
            try
            {
                if (id != ingredienteDto.IngredienteId)
                {
                    return BadRequest("ID nell'URL non corrisponde all'ID nel corpo della richiesta");
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Verifica se l'ingrediente esiste
                var existingIngrediente = await _ingredienteRepository.GetByIdAsync(id);
                if (existingIngrediente == null)
                {
                    return NotFound($"Ingrediente con ID {id} non trovato");
                }

                await _ingredienteRepository.UpdateAsync(ingredienteDto);

                return NoContent();
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Errore durante l'aggiornamento dell'ingrediente: {ex.Message}");
            }
        }

        // DELETE: api/Ingrediente/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                // Verifica se l'ingrediente esiste
                var existingIngrediente = await _ingredienteRepository.GetByIdAsync(id);
                if (existingIngrediente == null)
                {
                    return NotFound($"Ingrediente con ID {id} non trovato");
                }

                await _ingredienteRepository.DeleteAsync(id);

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Errore durante l'eliminazione dell'ingrediente: {ex.Message}");
            }
        }

        // GET: api/Ingrediente/exists/5
        [HttpGet("exists/{id}")]
        public async Task<ActionResult<bool>> Exists(int id)
        {
            try
            {
                var exists = await _ingredienteRepository.ExistsAsync(id);
                return Ok(exists);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Errore interno del server: {ex.Message}");
            }
        }
    }
}
