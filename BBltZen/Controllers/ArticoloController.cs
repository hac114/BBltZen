using DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Repository.Interface;

namespace BBltZen.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    // [Authorize] // ✅ Commentato per test Swagger
    public class ArticoloController(
        IArticoloRepository repository,
        ILogger<ArticoloController> logger) : ControllerBase
    {
        private readonly IArticoloRepository _repository = repository;
        private readonly ILogger<ArticoloController> _logger = logger;

        // ============================================
        // METODI GET (LETTURA)
        // ============================================

        // GET: api/articolo
        [HttpGet("")]
        [AllowAnonymous]
        public async Task<ActionResult<PaginatedResponseDTO<ArticoloDTO>>> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var result = await _repository.GetAllAsync(page, pageSize);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetAll articoli errore");
                return StatusCode(500, "Errore server");
            }
        }

        // GET: api/articolo/{id}
        [HttpGet("{id:int}")]
        [AllowAnonymous]
        public async Task<ActionResult<SingleResponseDTO<ArticoloDTO>>> GetById(int id)
        {
            try
            {
                var result = await _repository.GetByIdAsync(id);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetById articolo errore ID: {Id}", id);
                return StatusCode(500, "Errore server");
            }
        }

        // GET: api/articolo/tipo/{tipo}
        [HttpGet("tipo/{tipo}")]
        [AllowAnonymous]
        public async Task<ActionResult<PaginatedResponseDTO<ArticoloDTO>>> GetByTipo(string tipo, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var result = await _repository.GetByTipoAsync(tipo, page, pageSize);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetByTipo articolo errore tipo: {Tipo}", tipo);
                return StatusCode(500, "Errore server");
            }
        }

        // ============================================
        // METODI CRUD (CREAZIONE, MODIFICA, ELIMINAZIONE)
        // ============================================

        // POST: api/articolo
        [HttpPost]
        // [Authorize(Roles = "Admin")]
        public async Task<ActionResult<SingleResponseDTO<ArticoloDTO>>> Create([FromBody] ArticoloDTO articoloDto)
        {
            try
            {
                if (articoloDto == null)
                    return BadRequest();

                var result = await _repository.AddAsync(articoloDto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Create articolo errore");
                return StatusCode(500, "Errore server");
            }
        }

        // PUT: api/articolo/{id}
        [HttpPut("{id:int}")]
        // [Authorize(Roles = "Admin")]
        public async Task<ActionResult<SingleResponseDTO<bool>>> Update(int id, [FromBody] ArticoloDTO articoloDto)
        {
            try
            {
                if (articoloDto == null)
                    return BadRequest();

                if (id != articoloDto.ArticoloId)
                    return BadRequest();

                var result = await _repository.UpdateAsync(articoloDto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Update articolo {Id} errore", id);
                return StatusCode(500, "Errore server");
            }
        }

        // DELETE: api/articolo/{id}
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
                _logger.LogError(ex, "Delete articolo {Id} errore", id);
                return StatusCode(500, "Errore server");
            }
        }

        // ============================================
        // METODI DI VERIFICA (EXISTS)
        // ============================================

        // GET: api/articolo/exists/{id}
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
                _logger.LogError(ex, "Exists articolo {Id} errore", id);
                return StatusCode(500, "Errore server");
            }
        }

        // GET: api/articolo/exists/tipo/{tipo}
        [HttpGet("exists/tipo/{tipo}")]
        [AllowAnonymous]
        public async Task<ActionResult<SingleResponseDTO<bool>>> TipoExists(string tipo)
        {
            try
            {
                var result = await _repository.ExistsByTipoAsync(tipo);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "TipoExists articolo {Tipo} errore", tipo);
                return StatusCode(500, "Errore server");
            }
        }
    }
}