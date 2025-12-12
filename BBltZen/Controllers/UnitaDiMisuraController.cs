using DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Repository.Interface;
using System;
using System.Threading.Tasks;

namespace BBltZen.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    // [Authorize] // ✅ Commentato per test Swagger
    public class UnitaDiMisuraController(IUnitaDiMisuraRepository repository, ILogger<UnitaDiMisuraController> logger) : ControllerBase
    {
        private readonly IUnitaDiMisuraRepository _repository = repository;
        private readonly ILogger<UnitaDiMisuraController> _logger = logger;

        // GET: api/unita-dimisura
        [HttpGet("")]
        [AllowAnonymous]
        public async Task<ActionResult<PaginatedResponseDTO<UnitaDiMisuraDTO>>> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var result = await _repository.GetAllAsync(page, pageSize);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetAll unità di misura errore");
                return StatusCode(500, "Errore server");
            }
        }

        // GET: api/unita-dimisura/{id}
        [HttpGet("{id:int}")]
        [AllowAnonymous]
        public async Task<ActionResult<SingleResponseDTO<UnitaDiMisuraDTO>>> GetById(int id)
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

        // GET: api/unita-dimisura/sigla/{sigla}
        [HttpGet("sigla/{sigla}")]
        [AllowAnonymous]
        public async Task<ActionResult<PaginatedResponseDTO<UnitaDiMisuraDTO>>> GetBySigla(string sigla, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var result = await _repository.GetBySiglaAsync(sigla, page, pageSize);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetBySigla errore sigla: {Sigla}", sigla);
                return StatusCode(500, "Errore server");
            }
        }

        // GET: api/unita-dimisura/descrizione/{descrizione}
        [HttpGet("descrizione/{descrizione}")]
        [AllowAnonymous]
        public async Task<ActionResult<PaginatedResponseDTO<UnitaDiMisuraDTO>>> GetByDescrizione(string descrizione, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var result = await _repository.GetByDescrizioneAsync(descrizione, page, pageSize);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetByDescrizione errore descrizione: {Descrizione}", descrizione);
                return StatusCode(500, "Errore server");
            }
        }

        // POST: api/unita-dimisura
        [HttpPost]
        // [Authorize(Roles = "Admin")]
        public async Task<ActionResult<SingleResponseDTO<UnitaDiMisuraDTO>>> Create([FromBody] UnitaDiMisuraDTO unitaDto)
        {
            try
            {
                if (unitaDto == null)
                    return BadRequest("Dati unità di misura mancanti");

                var result = await _repository.AddAsync(unitaDto);

                if (!result.Success)
                    return BadRequest(result);

                return CreatedAtAction(nameof(GetById), new { id = result.Data?.UnitaMisuraId }, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Create unità di misura errore");
                return StatusCode(500, "Errore server");
            }
        }

        // PUT: api/unita-dimisura/{id}
        [HttpPut("{id:int}")]
        // [Authorize(Roles = "Admin")]
        public async Task<ActionResult<SingleResponseDTO<bool>>> Update(int id, [FromBody] UnitaDiMisuraDTO unitaDto)
        {
            try
            {
                if (unitaDto == null)
                    return BadRequest("Dati mancanti");

                if (id != unitaDto.UnitaMisuraId)
                    return BadRequest("ID non corrispondente");

                var result = await _repository.UpdateAsync(unitaDto);

                if (!result.Success)
                    return result.Message.Contains("non trovata") ? NotFound(result) : BadRequest(result);

                return result.Data ? NoContent() : Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Update unità di misura {Id} errore", id);
                return StatusCode(500, "Errore server");
            }
        }

        // DELETE: api/unita-dimisura/{id}
        [HttpDelete("{id:int}")]
        // [Authorize(Roles = "Admin")]
        public async Task<ActionResult<SingleResponseDTO<bool>>> Delete(int id)
        {
            try
            {
                var result = await _repository.DeleteAsync(id);

                if (!result.Success)
                    return result.Message.Contains("non trovata") ? NotFound(result) : BadRequest(result);

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Delete unità di misura {Id} errore", id);
                return StatusCode(500, "Errore server");
            }
        }

        // GET: api/unita-dimisura/exists/{id}
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

        // GET: api/unita-dimisura/exists/sigla/{sigla}
        [HttpGet("exists/sigla/{sigla}")]
        [AllowAnonymous]
        public async Task<ActionResult<SingleResponseDTO<bool>>> SiglaExists(string sigla)
        {
            try
            {
                var result = await _repository.SiglaExistsAsync(sigla);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SiglaExists {Sigla} errore", sigla);
                return StatusCode(500, "Errore server");
            }
        }

        // GET: api/unita-dimisura/exists/descrizione/{descrizione}
        [HttpGet("exists/descrizione/{descrizione}")]
        [AllowAnonymous]
        public async Task<ActionResult<SingleResponseDTO<bool>>> DescrizioneExists(string descrizione)
        {
            try
            {
                var result = await _repository.DescrizioneExistsAsync(descrizione);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "DescrizioneExists {Descrizione} errore", descrizione);
                return StatusCode(500, "Errore server");
            }
        }
    }
}