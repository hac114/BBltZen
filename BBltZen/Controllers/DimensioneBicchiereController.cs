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
    public class DimensioneBicchiereController(
        IDimensioneBicchiereRepository repository,
        ILogger<DimensioneBicchiereController> logger) : ControllerBase
    {
        private readonly IDimensioneBicchiereRepository _repository = repository;
        private readonly ILogger<DimensioneBicchiereController> _logger = logger;

        // GET: api/dimensione-bicchiere
        [HttpGet("")]
        [AllowAnonymous]
        public async Task<ActionResult<PaginatedResponseDTO<DimensioneBicchiereDTO>>> GetAll(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                var result = await _repository.GetAllAsync(page, pageSize);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetAll dimensioni bicchiere errore");
                return StatusCode(500, "Errore server");
            }
        }

        // GET: api/dimensione-bicchiere/{id}
        [HttpGet("{id:int}")]
        [AllowAnonymous]
        public async Task<ActionResult<SingleResponseDTO<DimensioneBicchiereDTO>>> GetById(int id)
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

        // GET: api/dimensione-bicchiere/sigla/{sigla}
        [HttpGet("sigla/{sigla}")]
        [AllowAnonymous]
        public async Task<ActionResult<PaginatedResponseDTO<DimensioneBicchiereDTO>>> GetBySigla(
            string sigla,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
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

        // GET: api/dimensione-bicchiere/descrizione/{descrizione}
        [HttpGet("descrizione/{descrizione}")]
        [AllowAnonymous]
        public async Task<ActionResult<PaginatedResponseDTO<DimensioneBicchiereDTO>>> GetByDescrizione(
            string descrizione,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
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

        // POST: api/dimensione-bicchiere
        [HttpPost]
        // [Authorize(Roles = "Admin")]
        public async Task<ActionResult<SingleResponseDTO<DimensioneBicchiereDTO>>> Create(
            [FromBody] DimensioneBicchiereDTO bicchiereDto)
        {
            try
            {
                if (bicchiereDto == null)
                    return BadRequest();

                var result = await _repository.AddAsync(bicchiereDto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Create dimensione bicchiere errore");
                return StatusCode(500, "Errore server");
            }
        }

        // PUT: api/dimensione-bicchiere/{id}
        [HttpPut("{id:int}")]
        // [Authorize(Roles = "Admin")]
        public async Task<ActionResult<SingleResponseDTO<bool>>> Update(
            int id,
            [FromBody] DimensioneBicchiereDTO bicchiereDto)
        {
            try
            {
                if (bicchiereDto == null)
                    return BadRequest();

                if (id != bicchiereDto.DimensioneBicchiereId)
                    return BadRequest();

                var result = await _repository.UpdateAsync(bicchiereDto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Update dimensione bicchiere {Id} errore", id);
                return StatusCode(500, "Errore server");
            }
        }

        // DELETE: api/dimensione-bicchiere/{id}
        [HttpDelete("{id:int}")]
        // [Authorize(Roles = "Admin")]
        public async Task<ActionResult<SingleResponseDTO<bool>>> Delete(int id)
        {
            try
            {
                var result = await _repository.DeleteAsync(id);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Delete dimensione bicchiere {Id} errore", id);
                return StatusCode(500, "Errore server");
            }
        }

        // GET: api/dimensione-bicchiere/exists/{id}
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

        // GET: api/dimensione-bicchiere/exists/sigla/{sigla}
        [HttpGet("exists/sigla/{sigla}")]
        [AllowAnonymous]
        public async Task<ActionResult<SingleResponseDTO<bool>>> SiglaExists(string sigla)
        {
            try
            {
                var result = await _repository.ExistsSiglaAsync(sigla);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SiglaExists {Sigla} errore", sigla);
                return StatusCode(500, "Errore server");
            }
        }

        // GET: api/dimensione-bicchiere/exists/descrizione/{descrizione}
        [HttpGet("exists/descrizione/{descrizione}")]
        [AllowAnonymous]
        public async Task<ActionResult<SingleResponseDTO<bool>>> DescrizioneExists(string descrizione)
        {
            try
            {
                var result = await _repository.ExistsDescrizioneAsync(descrizione);
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