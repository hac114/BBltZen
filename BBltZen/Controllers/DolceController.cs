using DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Repository.Interface;

namespace BBltZen.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    // [Authorize] // ✅ Commentato per test Swagger
    public class DolceController(
        IDolceRepository repository,
        ILogger<DolceController> logger) : ControllerBase
    {
        private readonly IDolceRepository _repository = repository;
        private readonly ILogger<DolceController> _logger = logger;

        // GET: api/dolce
        [HttpGet("")]
        [AllowAnonymous]
        public async Task<ActionResult<PaginatedResponseDTO<DolceDTO>>> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var result = await _repository.GetAllAsync(page, pageSize);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetAll dolci errore");
                return StatusCode(500, "Errore server");
            }
        }

        // GET: api/dolce/{id}
        [HttpGet("{id:int}")]
        [AllowAnonymous]
        public async Task<ActionResult<SingleResponseDTO<DolceDTO>>> GetById(int id)
        {
            try
            {
                var result = await _repository.GetByIdAsync(id);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetById dolce errore ID: {Id}", id);
                return StatusCode(500, "Errore server");
            }
        }

        // GET: api/dolce/disponibili
        [HttpGet("disponibili")]
        [AllowAnonymous]
        public async Task<ActionResult<PaginatedResponseDTO<DolceDTO>>> GetDisponibili([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var result = await _repository.GetDisponibiliAsync(page, pageSize);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetDisponibili dolci errore");
                return StatusCode(500, "Errore server");
            }
        }

        // GET: api/dolce/nondisponibili
        [HttpGet("nondisponibili")]
        [AllowAnonymous]
        public async Task<ActionResult<PaginatedResponseDTO<DolceDTO>>> GetNonDisponibili([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var result = await _repository.GetNonDisponibiliAsync(page, pageSize);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetNonDisponibili dolci errore");
                return StatusCode(500, "Errore server");
            }
        }

        // GET: api/dolce/priorita/{priorita}
        [HttpGet("priorita/{priorita:int}")]
        [AllowAnonymous]
        public async Task<ActionResult<PaginatedResponseDTO<DolceDTO>>> GetByPriorita(int priorita, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var result = await _repository.GetByPrioritaAsync(priorita, page, pageSize);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetByPriorita dolci errore priorita: {Priorita}", priorita);
                return StatusCode(500, "Errore server");
            }
        }        

        // POST: api/dolce
        [HttpPost]
        // [Authorize(Roles = "Admin")]
        public async Task<ActionResult<SingleResponseDTO<DolceDTO>>> Create([FromBody] DolceDTO dolceDto)
        {
            try
            {
                if (dolceDto == null)
                    return BadRequest();

                var result = await _repository.AddAsync(dolceDto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Create dolce errore");
                return StatusCode(500, "Errore server");
            }
        }

        // PUT: api/dolce/{id}
        [HttpPut("{id:int}")]
        // [Authorize(Roles = "Admin")]
        public async Task<ActionResult<SingleResponseDTO<bool>>> Update(int id, [FromBody] DolceDTO dolceDto)
        {
            try
            {
                if (dolceDto == null)
                    return BadRequest();

                if (id != dolceDto.ArticoloId)
                    return BadRequest();

                var result = await _repository.UpdateAsync(dolceDto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Update dolce {Id} errore", id);
                return StatusCode(500, "Errore server");
            }
        }

        // DELETE: api/dolce/{id}
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
                _logger.LogError(ex, "Delete dolce {Id} errore", id);
                return StatusCode(500, "Errore server");
            }
        }        

        // GET: api/dolce/exists/{id}
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
                _logger.LogError(ex, "Exists dolce {Id} errore", id);
                return StatusCode(500, "Errore server");
            }
        }        

        // GET: api/dolce/count
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
                _logger.LogError(ex, "Count dolci errore");
                return StatusCode(500, "Errore server");
            }
        }

        // GET: api/dolce/count/disponibili
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
                _logger.LogError(ex, "CountDisponibili dolci errore");
                return StatusCode(500, "Errore server");
            }
        }

        // GET: api/dolce/count/nondisponibili
        [HttpGet("count/nondisponibili")]
        [AllowAnonymous]
        public async Task<ActionResult<SingleResponseDTO<int>>> CountNonDisponibili()
        {
            try
            {
                var result = await _repository.CountNonDisponibiliAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "CountNonDisponibili dolci errore");
                return StatusCode(500, "Errore server");
            }
        }

        // PATCH: api/dolce/{id}/toggle-disponibilita
        [HttpPatch("{id:int}/toggle-disponibilita")]
        // [Authorize(Roles = "Admin")]
        public async Task<ActionResult<SingleResponseDTO<bool>>> ToggleDisponibilita(int id)
        {
            try
            {
                var result = await _repository.ToggleDisponibilitaAsync(id);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ToggleDisponibilita articolo {Id} errore", id);
                return StatusCode(500, "Errore server");
            }
        }
    }
}