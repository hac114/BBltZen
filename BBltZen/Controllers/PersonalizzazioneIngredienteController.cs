using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Repository.Interface;
using DTO;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using BBltZen;

namespace BBltZen.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    // [Authorize] // ✅ Commentato per test Swagger
    public class PersonalizzazioneIngredienteController(
        IPersonalizzazioneIngredienteRepository repository,
        ILogger<PersonalizzazioneIngredienteController> logger) : ControllerBase
    {
        private readonly IPersonalizzazioneIngredienteRepository _repository = repository;
        private readonly ILogger<PersonalizzazioneIngredienteController> _logger = logger;

        // GET: api/personalizzazione-ingrediente
        [HttpGet("")]
        [AllowAnonymous]
        public async Task<ActionResult<PaginatedResponseDTO<PersonalizzazioneIngredienteDTO>>> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var result = await _repository.GetAllAsync(page, pageSize);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetAll personalizzazioni ingrediente errore");
                return StatusCode(500, "Errore server");
            }
        }

        // GET: api/personalizzazione-ingrediente/{id}
        [HttpGet("{id:int}")]
        [AllowAnonymous]
        public async Task<ActionResult<SingleResponseDTO<PersonalizzazioneIngredienteDTO>>> GetById(int id)
        {
            try
            {
                var result = await _repository.GetByIdAsync(id);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetById errore ID: {Id}", id);
                return StatusCode(500, "Errore server");
            }
        }

        // GET: api/personalizzazione-ingrediente/personalizzazione/{nomePersonalizzazione}
        [HttpGet("personalizzazione/{nomePersonalizzazione}")]
        [AllowAnonymous]
        public async Task<ActionResult<PaginatedResponseDTO<PersonalizzazioneIngredienteDTO>>> GetByPersonalizzazione(string nomePersonalizzazione, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var result = await _repository.GetByPersonalizzazioneAsync(nomePersonalizzazione, page, pageSize);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetByPersonalizzazione errore nome: {NomePersonalizzazione}", nomePersonalizzazione);
                return StatusCode(500, "Errore server");
            }
        }

        // GET: api/personalizzazione-ingrediente/ingrediente/{nomeIngrediente}
        [HttpGet("ingrediente/{nomeIngrediente}")]
        [AllowAnonymous]
        public async Task<ActionResult<PaginatedResponseDTO<PersonalizzazioneIngredienteDTO>>> GetByIngrediente(string nomeIngrediente, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var result = await _repository.GetByIngredienteAsync(nomeIngrediente, page, pageSize);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetByIngrediente errore nome: {NomeIngrediente}", nomeIngrediente);
                return StatusCode(500, "Errore server");
            }
        }

        // POST: api/personalizzazione-ingrediente
        [HttpPost]
        // [Authorize(Roles = "Admin")]
        public async Task<ActionResult<SingleResponseDTO<PersonalizzazioneIngredienteDTO>>> Create([FromBody] PersonalizzazioneIngredienteDTO personalizzazioneIngredienteDto)
        {
            try
            {
                if (personalizzazioneIngredienteDto == null)
                    return BadRequest();

                var result = await _repository.AddAsync(personalizzazioneIngredienteDto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Create personalizzazione ingrediente errore");
                return StatusCode(500, "Errore server");
            }
        }

        // PUT: api/personalizzazione-ingrediente/{id}
        [HttpPut("{id:int}")]
        // [Authorize(Roles = "Admin")]
        public async Task<ActionResult<SingleResponseDTO<bool>>> Update(int id, [FromBody] PersonalizzazioneIngredienteDTO personalizzazioneIngredienteDto)
        {
            try
            {
                if (personalizzazioneIngredienteDto == null)
                    return BadRequest();

                if (id != personalizzazioneIngredienteDto.PersonalizzazioneIngredienteId)
                    return BadRequest();

                var result = await _repository.UpdateAsync(personalizzazioneIngredienteDto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Update personalizzazione ingrediente {Id} errore", id);
                return StatusCode(500, "Errore server");
            }
        }

        // DELETE: api/personalizzazione-ingrediente/{id}
        [HttpDelete("{id:int}")]
        // [Authorize(Roles = "Admin")]
        public async Task<ActionResult<SingleResponseDTO<bool>>> Delete(int id, [FromQuery] bool forceDelete = false)
        {
            try
            {
                var result = await _repository.DeleteAsync(id, forceDelete);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Delete personalizzazione ingrediente {Id} errore", id);
                return StatusCode(500, "Errore server");
            }
        }

        // GET: api/personalizzazione-ingrediente/exists/{id}
        [HttpGet("exists/{id:int}")]
        [AllowAnonymous]
        public async Task<ActionResult<SingleResponseDTO<bool>>> Exists(int id)
        {
            try
            {
                var result = await _repository.ExistsAsync(id);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exists {Id} errore", id);
                return StatusCode(500, "Errore server");
            }
        }

        // GET: api/personalizzazione-ingrediente/exists/personalizzazione-ingrediente
        [HttpGet("exists/personalizzazione-ingrediente")]
        [AllowAnonymous]
        public async Task<ActionResult<SingleResponseDTO<bool>>> ExistsByPersonalizzazioneAndIngrediente([FromQuery] int personalizzazioneId, [FromQuery] int ingredienteId)
        {
            try
            {
                var result = await _repository.ExistsByPersonalizzazioneAndIngredienteAsync(personalizzazioneId, ingredienteId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ExistsByPersonalizzazioneAndIngrediente errore per personalizzazioneId: {PersonalizzazioneId}, ingredienteId: {IngredienteId}",
                    personalizzazioneId, ingredienteId);
                return StatusCode(500, "Errore server");
            }
        }

        // GET: api/personalizzazione-ingrediente/count
        [HttpGet("count")]
        [AllowAnonymous]
        public async Task<ActionResult<SingleResponseDTO<int>>> Count()
        {
            try
            {
                var result = await _repository.CountAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Count personalizzazione ingrediente errore");
                return StatusCode(500, "Errore server");
            }
        }

        // GET: api/personalizzazione-ingrediente/count/personalizzazione/{nomePersonalizzazione}
        [HttpGet("count/personalizzazione/{nomePersonalizzazione}")]
        [AllowAnonymous]
        public async Task<ActionResult<SingleResponseDTO<int>>> GetCountByPersonalizzazione(string nomePersonalizzazione)
        {
            try
            {
                var result = await _repository.GetCountByPersonalizzazioneAsync(nomePersonalizzazione);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetCountByPersonalizzazione errore per nome: {NomePersonalizzazione}", nomePersonalizzazione);
                return StatusCode(500, "Errore server");
            }
        }
    }
}