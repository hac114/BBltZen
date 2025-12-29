using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Repository.Interface;
using DTO;

namespace BBltZen.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    // [Authorize] // ✅ Commentato per test Swagger
    public class IngredientiPersonalizzazioneController(
        IIngredientiPersonalizzazioneRepository repository,
        ILogger<IngredientiPersonalizzazioneController> logger) : ControllerBase
    {
        private readonly IIngredientiPersonalizzazioneRepository _repository = repository;
        private readonly ILogger<IngredientiPersonalizzazioneController> _logger = logger;

        // GET: api/ingredienti-personalizzazione
        [HttpGet("")]
        [AllowAnonymous]
        public async Task<ActionResult<PaginatedResponseDTO<IngredientiPersonalizzazioneDTO>>> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var result = await _repository.GetAllAsync(page, pageSize);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetAll ingredienti personalizzazione errore");
                return StatusCode(500, "Errore server");
            }
        }

        // GET: api/ingredienti-personalizzazione/{id}
        [HttpGet("{id:int}")]
        [AllowAnonymous]
        public async Task<ActionResult<SingleResponseDTO<IngredientiPersonalizzazioneDTO>>> GetById(int id)
        {
            try
            {
                var result = await _repository.GetByIdAsync(id);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetById ingredienti personalizzazione errore ID: {Id}", id);
                return StatusCode(500, "Errore server");
            }
        }

        // GET: api/ingredienti-personalizzazione/personalizzazione/{persCustomId}
        [HttpGet("personalizzazione/{persCustomId:int}")]
        [AllowAnonymous]
        public async Task<ActionResult<PaginatedResponseDTO<IngredientiPersonalizzazioneDTO>>> GetByPersCustomId(int persCustomId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var result = await _repository.GetByPersCustomIdAsync(persCustomId, page, pageSize);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetByPersCustomId errore persCustomId: {PersCustomId}", persCustomId);
                return StatusCode(500, "Errore server");
            }
        }

        // GET: api/ingredienti-personalizzazione/ingrediente/{ingredienteId}
        [HttpGet("ingrediente/{ingredienteId:int}")]
        [AllowAnonymous]
        public async Task<ActionResult<PaginatedResponseDTO<IngredientiPersonalizzazioneDTO>>> GetByIngredienteId(int ingredienteId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var result = await _repository.GetByIngredienteIdAsync(ingredienteId, page, pageSize);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetByIngredienteId errore ingredienteId: {IngredienteId}", ingredienteId);
                return StatusCode(500, "Errore server");
            }
        }

        // GET: api/ingredienti-personalizzazione/combinazione
        [HttpGet("combinazione")]
        [AllowAnonymous]
        public async Task<ActionResult<SingleResponseDTO<IngredientiPersonalizzazioneDTO>>> GetByCombinazione([FromQuery] int persCustomId, [FromQuery] int ingredienteId)
        {
            try
            {
                var result = await _repository.GetByCombinazioneAsync(persCustomId, ingredienteId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetByCombinazione errore persCustomId: {PersCustomId}, ingredienteId: {IngredienteId}",
                    persCustomId, ingredienteId);
                return StatusCode(500, "Errore server");
            }
        }

        // POST: api/ingredienti-personalizzazione
        [HttpPost]
        // [Authorize(Roles = "Admin")]
        public async Task<ActionResult<SingleResponseDTO<IngredientiPersonalizzazioneDTO>>> Create([FromBody] IngredientiPersonalizzazioneDTO ingredientiPersonalizzazioneDto)
        {
            try
            {
                if (ingredientiPersonalizzazioneDto == null)
                    return BadRequest();

                var result = await _repository.AddAsync(ingredientiPersonalizzazioneDto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Create ingredienti personalizzazione errore");
                return StatusCode(500, "Errore server");
            }
        }

        // PUT: api/ingredienti-personalizzazione/{id}
        [HttpPut("{id:int}")]
        // [Authorize(Roles = "Admin")]
        public async Task<ActionResult<SingleResponseDTO<bool>>> Update(int id, [FromBody] IngredientiPersonalizzazioneDTO ingredientiPersonalizzazioneDto)
        {
            try
            {
                if (ingredientiPersonalizzazioneDto == null)
                    return BadRequest();

                if (id != ingredientiPersonalizzazioneDto.IngredientePersId)
                    return BadRequest();

                var result = await _repository.UpdateAsync(ingredientiPersonalizzazioneDto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Update ingredienti personalizzazione {Id} errore", id);
                return StatusCode(500, "Errore server");
            }
        }

        // DELETE: api/ingredienti-personalizzazione/{id}
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
                _logger.LogError(ex, "Delete ingredienti personalizzazione {Id} errore", id);
                return StatusCode(500, "Errore server");
            }
        }

        // GET: api/ingredienti-personalizzazione/exists/{id}
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

        // GET: api/ingredienti-personalizzazione/exists/combinazione
        [HttpGet("exists/combinazione")]
        [AllowAnonymous]
        public async Task<ActionResult<SingleResponseDTO<bool>>> ExistsByCombinazione([FromQuery] int persCustomId, [FromQuery] int ingredienteId)
        {
            try
            {
                var result = await _repository.ExistsByCombinazioneAsync(persCustomId, ingredienteId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ExistsByCombinazione errore persCustomId: {PersCustomId}, ingredienteId: {IngredienteId}",
                    persCustomId, ingredienteId);
                return StatusCode(500, "Errore server");
            }
        }

        // GET: api/ingredienti-personalizzazione/count
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
                _logger.LogError(ex, "Count ingredienti personalizzazione errore");
                return StatusCode(500, "Errore server");
            }
        }
    }
}