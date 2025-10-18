// Controllers/TavoloController.cs
using Microsoft.AspNetCore.Mvc;
using DTO;
using Repository.Interface;

namespace BBltZen.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TavoloController : ControllerBase
    {
        private readonly ITavoloRepository _tavoloRepository;

        public TavoloController(ITavoloRepository tavoloRepository)
        {
            _tavoloRepository = tavoloRepository;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<TavoloDTO>>> GetAll()
        {
            try
            {
                var tavoli = await _tavoloRepository.GetAllAsync();
                return Ok(tavoli);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Errore interno: {ex.Message}");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<TavoloDTO>> GetById(int id)
        {
            try
            {
                var tavolo = await _tavoloRepository.GetByIdAsync(id);
                if (tavolo == null)
                    return NotFound($"Tavolo con ID {id} non trovato");

                return Ok(tavolo);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Errore interno: {ex.Message}");
            }
        }

        [HttpGet("disponibili")]
        public async Task<ActionResult<IEnumerable<TavoloDTO>>> GetDisponibili()
        {
            try
            {
                var tavoli = await _tavoloRepository.GetDisponibiliAsync();
                return Ok(tavoli);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Errore interno: {ex.Message}");
            }
        }

        [HttpPost]
        public async Task<ActionResult<TavoloDTO>> Create([FromBody] TavoloDTO tavoloDto)
        {
            try
            {
                // Validazione
                if (await _tavoloRepository.NumeroExistsAsync(tavoloDto.Numero))
                    return BadRequest("Numero tavolo già esistente");

                await _tavoloRepository.AddAsync(tavoloDto);

                return CreatedAtAction(nameof(GetById),
                    new { id = tavoloDto.TavoloId },
                    tavoloDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Errore interno: {ex.Message}");
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] TavoloDTO tavoloDto)
        {
            try
            {
                if (id != tavoloDto.TavoloId)
                    return BadRequest("ID non corrispondente");

                if (!await _tavoloRepository.ExistsAsync(id))
                    return NotFound($"Tavolo con ID {id} non trovato");

                // Validazione unique constraints
                if (await _tavoloRepository.NumeroExistsAsync(tavoloDto.Numero, id))
                    return BadRequest("Numero tavolo già esistente");

                await _tavoloRepository.UpdateAsync(tavoloDto);
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Errore interno: {ex.Message}");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                if (!await _tavoloRepository.ExistsAsync(id))
                    return NotFound($"Tavolo con ID {id} non trovato");

                await _tavoloRepository.DeleteAsync(id);
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Errore interno: {ex.Message}");
            }
        }
    }
}