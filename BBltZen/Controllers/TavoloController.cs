using Database.Models;
using DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Repository.Interface;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BBltZen.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    // [Authorize] // ✅ Commentato per test Swagger
    public class TavoloController(ITavoloRepository repository, ILogger<TavoloController> logger) : ControllerBase
    {
        private readonly ITavoloRepository _repository = repository;
        private readonly ILogger<TavoloController> _logger = logger;

        // GET: api/tavolo
        [HttpGet("")]
        [AllowAnonymous]
        public async Task<ActionResult<PaginatedResponseDTO<TavoloDTO>>> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var result = await _repository.GetAllAsync(page, pageSize);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetAll tavoli errore");
                return StatusCode(500, "Errore server");
            }
        }

        // GET: api/tavolo/{id}
        [HttpGet("{id:int}")]
        [AllowAnonymous]
        public async Task<ActionResult<SingleResponseDTO<TavoloDTO>>> GetById(int id)
        {
            try
            {
                var result = await _repository.GetByIdAsync(id);
                return result.Success ? Ok(result) : NotFound(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetById errore ID: {Id}", id);
                return StatusCode(500, "Errore server");
            }
        }

        [HttpGet("numero/{numero:int}")]
        [AllowAnonymous]
        public async Task<ActionResult<SingleResponseDTO<TavoloDTO>>> GetByNumero(int numero)
        {
            try
            {
                var result = await _repository.GetByNumeroAsync(numero);
                return result.Success ? Ok(result) : NotFound(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetByNumero errore numero: {Numero}", numero);
                return StatusCode(500, "Errore server");
            }
        }

        // GET: api/tavolo/disponibili
        [HttpGet("disponibili")]
        [AllowAnonymous]
        public async Task<ActionResult<PaginatedResponseDTO<TavoloDTO>>> GetDisponibili([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var result = await _repository.GetDisponibiliAsync(page, pageSize);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetDisponibili errore pagina: {Page}", page);
                return StatusCode(500, "Errore server");
            }
        }

        // GET: api/tavolo/occupati
        [HttpGet("occupati")]
        [AllowAnonymous]
        public async Task<ActionResult<PaginatedResponseDTO<TavoloDTO>>> GetOccupati([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var result = await _repository.GetOccupatiAsync(page, pageSize);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetOccupati errore");
                return StatusCode(500, "Errore server");
            }
        }

        // GET: api/tavolo/zona/{zona}
        [HttpGet("zona/{zona}")]
        [AllowAnonymous]
        public async Task<ActionResult<PaginatedResponseDTO<TavoloDTO>>> GetByZona(string zona, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var result = await _repository.GetByZonaAsync(zona, page, pageSize);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetByZona errore zona: {Zona}", zona);
                return StatusCode(500, "Errore server");
            }
        }

        [HttpPost]
        // [Authorize(Roles = "Admin")]
        public async Task<ActionResult<SingleResponseDTO<TavoloDTO>>> Create([FromBody] TavoloDTO tavoloDto)
        {
            try
            {
                if (tavoloDto == null)
                    return BadRequest("Dati tavolo mancanti");

                var result = await _repository.AddAsync(tavoloDto);

                if (!result.Success)
                    return BadRequest(result);

                return CreatedAtAction(nameof(GetById), new { id = result.Data?.TavoloId }, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Create tavolo errore");
                return StatusCode(500, "Errore server");
            }
        }

        [HttpPut("{id:int}")]
        // [Authorize(Roles = "Admin")]
        public async Task<ActionResult<SingleResponseDTO<bool>>> Update(int id, [FromBody] TavoloDTO tavoloDto)
        {
            try
            {
                if (tavoloDto == null)
                    return BadRequest("Dati mancanti");

                if (id != tavoloDto.TavoloId)
                    return BadRequest("ID non corrispondente");

                var result = await _repository.UpdateAsync(tavoloDto);

                if (!result.Success)
                    return result.Message.Contains("non trovato") ? NotFound(result) : BadRequest(result);

                return result.Data ? NoContent() : Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Update tavolo {Id} errore", id);
                return StatusCode(500, "Errore server");
            }
        }

        [HttpDelete("{id:int}")]
        // [Authorize(Roles = "Admin")]
        public async Task<ActionResult<SingleResponseDTO<bool>>> Delete(int id)
        {
            try
            {
                var result = await _repository.DeleteAsync(id);

                if (!result.Success)
                    return result.Message.Contains("non trovato") ? NotFound(result) : BadRequest(result);

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Delete tavolo {Id} errore", id);
                return StatusCode(500, "Errore server");
            }
        }

        // GET: api/tavolo/exists/{id}
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

        // GET: api/tavolo/numero-exists/{numero}
        [HttpGet("numero-exists/{numero:int}")]
        [AllowAnonymous]
        public async Task<ActionResult<SingleResponseDTO<bool>>> NumeroExists(int numero)
        {
            try
            {
                var result = await _repository.NumeroExistsAsync(numero);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "NumeroExists {Numero} errore", numero);
                return StatusCode(500, "Errore server");
            }
        }

        [HttpPatch("{id:int}/toggle-disponibilita")]
        // [Authorize(Roles = "Admin,Impiegato")]
        public async Task<ActionResult<SingleResponseDTO<bool>>> ToggleDisponibilita(int id)
        {
            try
            {
                var result = await _repository.ToggleDisponibilitaAsync(id);

                if (!result.Success)
                    return result.Message.Contains("non trovato") ? NotFound(result) : BadRequest(result);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ToggleDisponibilita {Id} errore", id);
                return StatusCode(500, "Errore server");
            }
        }

        [HttpPatch("numero/{numero:int}/toggle-disponibilita")]
        // [Authorize(Roles = "Admin,Impiegato")]
        public async Task<ActionResult<SingleResponseDTO<bool>>> ToggleDisponibilitaByNumero(int numero)
        {
            try
            {
                var result = await _repository.ToggleDisponibilitaByNumeroAsync(numero);

                if (!result.Success)
                    return result.Message.Contains("non trovato") ? NotFound(result) : BadRequest(result);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ToggleDisponibilitaByNumero {Numero} errore", numero);
                return StatusCode(500, "Errore server");
            }
        }

        // GET: api/tavolo/count
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
                _logger.LogError(ex, "Count errore");
                return StatusCode(500, "Errore server");
            }
        }

        // GET: api/tavolo/count/disponibili
        [HttpGet("count/disponibili")]
        [AllowAnonymous]
        public async Task<ActionResult<SingleResponseDTO<int>>> CountDisponibili()
        {
            try
            {
                var result = await _repository.CountDisponibiliAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "CountDisponibili errore");
                return StatusCode(500, "Errore server");
            }
        }

        // GET: api/tavolo/count/occupati
        [HttpGet("count/occupati")]
        [AllowAnonymous]
        public async Task<ActionResult<SingleResponseDTO<int>>> CountOccupati()
        {
            try
            {
                var result = await _repository.CountOccupatiAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "CountOccupati errore");
                return StatusCode(500, "Errore server");
            }
        }
    }
}