using DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Repository.Interface;
using System.Threading.Tasks;

namespace BBltZen.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    // [Authorize] // ✅ Commentato per test Swagger
    public class IngredienteController(
        IIngredienteRepository repository,
        ILogger<IngredienteController> logger) : ControllerBase
    {
        private readonly IIngredienteRepository _repository = repository;
        private readonly ILogger<IngredienteController> _logger = logger;

        // ============================================
        // METODI GET (LETTURA)
        // ============================================

        // GET: api/ingrediente
        [HttpGet("")]
        [AllowAnonymous]
        public async Task<ActionResult<PaginatedResponseDTO<IngredienteDTO>>> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var result = await _repository.GetAllAsync(page, pageSize);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetAll ingredienti errore");
                return StatusCode(500, "Errore server");
            }
        }

        // GET: api/ingrediente/{id}
        [HttpGet("{id:int}")]
        [AllowAnonymous]
        public async Task<ActionResult<SingleResponseDTO<IngredienteDTO>>> GetById(int id)
        {
            try
            {
                var result = await _repository.GetByIdAsync(id);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetById ingrediente errore ID: {Id}", id);
                return StatusCode(500, "Errore server");
            }
        }

        // GET: api/ingrediente/nome/{nome}
        [HttpGet("nome/{nome}")]
        [AllowAnonymous]
        public async Task<ActionResult<PaginatedResponseDTO<IngredienteDTO>>> GetByNome(string nome, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var result = await _repository.GetByNomeAsync(nome, page, pageSize);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetByNome ingrediente errore nome: {Nome}", nome);
                return StatusCode(500, "Errore server");
            }
        }

        // GET: api/ingrediente/categoria/{categoria}
        [HttpGet("categoria/{categoria}")]
        [AllowAnonymous]
        public async Task<ActionResult<PaginatedResponseDTO<IngredienteDTO>>> GetByCategoria(string categoria, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var result = await _repository.GetByCategoriaAsync(categoria, page, pageSize);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetByCategoria ingrediente errore categoria: {Categoria}", categoria);
                return StatusCode(500, "Errore server");
            }
        }

        // GET: api/ingrediente/disponibili
        [HttpGet("disponibili")]
        [AllowAnonymous]
        public async Task<ActionResult<PaginatedResponseDTO<IngredienteDTO>>> GetDisponibili([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var result = await _repository.GetByDisponibilisync(page, pageSize);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetDisponibili ingredienti errore");
                return StatusCode(500, "Errore server");
            }
        }

        // GET: api/ingrediente/non-disponibili
        [HttpGet("non-disponibili")]
        [AllowAnonymous]
        public async Task<ActionResult<PaginatedResponseDTO<IngredienteDTO>>> GetNonDisponibili([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var result = await _repository.GetByNonDisponibilisync(page, pageSize);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetNonDisponibili ingredienti errore");
                return StatusCode(500, "Errore server");
            }
        }

        // ============================================
        // METODI CRUD (CREAZIONE, MODIFICA, ELIMINAZIONE)
        // ============================================

        // POST: api/ingrediente
        [HttpPost]
        // [Authorize(Roles = "Admin")]
        public async Task<ActionResult<SingleResponseDTO<IngredienteDTO>>> Create([FromBody] IngredienteDTO ingredienteDto)
        {
            try
            {
                if (ingredienteDto == null)
                    return BadRequest();

                var result = await _repository.AddAsync(ingredienteDto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Create ingrediente errore");
                return StatusCode(500, "Errore server");
            }
        }

        // PUT: api/ingrediente/{id}
        [HttpPut("{id:int}")]
        // [Authorize(Roles = "Admin")]
        public async Task<ActionResult<SingleResponseDTO<bool>>> Update(int id, [FromBody] IngredienteDTO ingredienteDto)
        {
            try
            {
                if (ingredienteDto == null)
                    return BadRequest();

                if (id != ingredienteDto.IngredienteId)
                    return BadRequest();

                var result = await _repository.UpdateAsync(ingredienteDto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Update ingrediente {Id} errore", id);
                return StatusCode(500, "Errore server");
            }
        }

        // DELETE: api/ingrediente/{id}
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
                _logger.LogError(ex, "Delete ingrediente {Id} errore", id);
                return StatusCode(500, "Errore server");
            }
        }

        // ============================================
        // METODI DI VERIFICA (EXISTS)
        // ============================================

        // GET: api/ingrediente/exists/{id}
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
                _logger.LogError(ex, "Exists ingrediente {Id} errore", id);
                return StatusCode(500, "Errore server");
            }
        }

        // GET: api/ingrediente/exists/nome/{nome}
        [HttpGet("exists/nome/{nome}")]
        [AllowAnonymous]
        public async Task<ActionResult<SingleResponseDTO<bool>>> NomeExists(string nome)
        {
            try
            {
                var result = await _repository.ExistsByNomeAsync(nome);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "NomeExists ingrediente {Nome} errore", nome);
                return StatusCode(500, "Errore server");
            }
        }

        // ============================================
        // METODI BUSINESS (TOGGLE DISPONIBILITÀ)
        // ============================================

        // PATCH: api/ingrediente/{id}/toggle-disponibilita
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
                _logger.LogError(ex, "ToggleDisponibilita ingrediente {Id} errore", id);
                return StatusCode(500, "Errore server");
            }
        }

        // ============================================
        // METODI DI STATISTICA (COUNT)
        // ============================================

        // GET: api/ingrediente/count
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
                _logger.LogError(ex, "Count ingredienti errore");
                return StatusCode(500, "Errore server");
            }
        }

        // GET: api/ingrediente/count/disponibili
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
                _logger.LogError(ex, "CountDisponibili ingredienti errore");
                return StatusCode(500, "Errore server");
            }
        }

        // GET: api/ingrediente/count/non-disponibili
        [HttpGet("count/non-disponibili")]
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
                _logger.LogError(ex, "CountNonDisponibili ingredienti errore");
                return StatusCode(500, "Errore server");
            }
        }
    }
}