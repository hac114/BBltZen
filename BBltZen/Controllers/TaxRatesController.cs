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
    public class TaxRatesController(ITaxRatesRepository repository, ILogger<TaxRatesController> logger) : ControllerBase
    {
        private readonly ITaxRatesRepository _repository = repository;
        private readonly ILogger<TaxRatesController> _logger = logger;

        // GET: api/taxrates
        [HttpGet("")]
        [AllowAnonymous]
        public async Task<ActionResult<PaginatedResponseDTO<TaxRatesDTO>>> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var result = await _repository.GetAllAsync(page, pageSize);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetAll aliquote errore");
                return StatusCode(500, "Errore server");
            }
        }

        // GET: api/taxrates/{id}
        [HttpGet("{id:int}")]
        [AllowAnonymous]
        public async Task<ActionResult<SingleResponseDTO<TaxRatesDTO>>> GetById(int id)
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

        // GET: api/taxrates/aliquota/{aliquota}
        [HttpGet("aliquota/{aliquota:decimal}")]
        [AllowAnonymous]
        public async Task<ActionResult<PaginatedResponseDTO<TaxRatesDTO>>> GetByAliquota(decimal aliquota, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var result = await _repository.GetByAliquotaAsync(aliquota, page, pageSize);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetByAliquota errore aliquota: {Aliquota}", aliquota);
                return StatusCode(500, "Errore server");
            }
        }

        // GET: api/taxrates/descrizione/{descrizione}
        [HttpGet("descrizione/{descrizione}")]
        [AllowAnonymous]
        public async Task<ActionResult<PaginatedResponseDTO<TaxRatesDTO>>> GetByDescrizione(string descrizione, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
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

        // POST: api/taxrates
        [HttpPost]
        // [Authorize(Roles = "Admin")]
        public async Task<ActionResult<SingleResponseDTO<TaxRatesDTO>>> Create([FromBody] TaxRatesDTO taxRateDto)
        {
            try
            {
                if (taxRateDto == null)
                    return BadRequest("Dati aliquota mancanti");

                var result = await _repository.AddAsync(taxRateDto);

                if (!result.Success)
                    return BadRequest(result);

                return CreatedAtAction(nameof(GetById), new { id = result.Data?.TaxRateId }, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Create aliquota errore");
                return StatusCode(500, "Errore server");
            }
        }

        // PUT: api/taxrates/{id}
        [HttpPut("{id:int}")]
        // [Authorize(Roles = "Admin")]
        public async Task<ActionResult<SingleResponseDTO<bool>>> Update(int id, [FromBody] TaxRatesDTO taxRateDto)
        {
            try
            {
                if (taxRateDto == null)
                    return BadRequest("Dati mancanti");

                if (id != taxRateDto.TaxRateId)
                    return BadRequest("ID non corrispondente");

                var result = await _repository.UpdateAsync(taxRateDto);

                if (!result.Success)
                    return result.Message.Contains("non trovata") ? NotFound(result) : BadRequest(result);

                return result.Data ? NoContent() : Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Update aliquota {Id} errore", id);
                return StatusCode(500, "Errore server");
            }
        }

        // DELETE: api/taxrates/{id}
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
                _logger.LogError(ex, "Delete aliquota {Id} errore", id);
                return StatusCode(500, "Errore server");
            }
        }

        // GET: api/taxrates/exists/{id}
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

        // GET: api/taxrates/exists/aliquota/{aliquota}
        [HttpGet("exists/aliquota/{aliquota:decimal}")]
        [AllowAnonymous]
        public async Task<ActionResult<SingleResponseDTO<bool>>> ExistsByAliquota(decimal aliquota)
        {
            try
            {
                var result = await _repository.ExistsByAliquotaAsync(aliquota);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ExistsByAliquota {Aliquota} errore", aliquota);
                return StatusCode(500, "Errore server");
            }
        }

        // GET: api/taxrates/exists/aliquota-descrizione/{aliquota}/{descrizione}
        [HttpGet("exists/aliquota-descrizione/{aliquota:decimal}/{descrizione}")]
        [AllowAnonymous]
        public async Task<ActionResult<SingleResponseDTO<bool>>> ExistsByAliquotaDescrizione(decimal aliquota, string descrizione)
        {
            try
            {
                var result = await _repository.ExistsByAliquotaDescrizioneAsync(aliquota, descrizione);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ExistsByAliquotaDescrizione {Aliquota}/{Descrizione} errore", aliquota, descrizione);
                return StatusCode(500, "Errore server");
            }
        }
    }
}