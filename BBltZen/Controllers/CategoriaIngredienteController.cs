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
    public class CategoriaIngredienteController(ICategoriaIngredienteRepository repository, ILogger<CategoriaIngredienteController> logger) : ControllerBase
    {
        private readonly ICategoriaIngredienteRepository _repository = repository;
        private readonly ILogger<CategoriaIngredienteController> _logger = logger;

        // GET: api/categoria-ingrediente
        [HttpGet("")]
        [AllowAnonymous]
        public async Task<ActionResult<PaginatedResponseDTO<CategoriaIngredienteDTO>>> GetAll(
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
                _logger.LogError(ex, "GetAll categorie ingredienti errore");
                return StatusCode(500, "Errore server");
            }
        }

        // GET: api/categoria-ingrediente/{id}
        [HttpGet("{id:int}")]
        [AllowAnonymous]
        public async Task<ActionResult<SingleResponseDTO<CategoriaIngredienteDTO>>> GetById(int id)
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

        // GET: api/categoria-ingrediente/nome/{nome}
        [HttpGet("nome/{nome}")]
        [AllowAnonymous]
        public async Task<ActionResult<PaginatedResponseDTO<CategoriaIngredienteDTO>>> GetByNome(string nome, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var result = await _repository.GetByNomeAsync(nome, page, pageSize);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetByNome errore nome: {Nome}", nome);
                return StatusCode(500, "Errore server");
            }
        }

        // POST: api/categoria-ingrediente
        [HttpPost]
        // [Authorize(Roles = "Admin")]
        public async Task<ActionResult<SingleResponseDTO<CategoriaIngredienteDTO>>> Create([FromBody] CategoriaIngredienteDTO categoriaDto)
        {
            try
            {
                if (categoriaDto == null)
                    return BadRequest("Dati categoria ingredienti mancanti");

                var result = await _repository.AddAsync(categoriaDto);

                if (!result.Success)
                    return BadRequest(result);

                return CreatedAtAction(nameof(GetById), new { id = result.Data?.CategoriaId }, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Create categoria ingredienti errore");
                return StatusCode(500, "Errore server");
            }
        }

        // PUT: api/categoria-ingrediente/{id}
        [HttpPut("{id:int}")]
        // [Authorize(Roles = "Admin")]
        public async Task<ActionResult<SingleResponseDTO<bool>>> Update(int id, [FromBody] CategoriaIngredienteDTO categoriaDto)
        {
            try
            {
                if (categoriaDto == null)
                    return BadRequest("Dati mancanti");

                if (id != categoriaDto.CategoriaId)
                    return BadRequest("ID non corrispondente");

                var result = await _repository.UpdateAsync(categoriaDto);

                if (!result.Success)
                    return result.Message.Contains("non trovata") ? NotFound(result) : BadRequest(result);

                return result.Data ? NoContent() : Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Update categoria ingredienti {Id} errore", id);
                return StatusCode(500, "Errore server");
            }
        }
                
        // DELETE: api/categoria-ingrediente/{id}
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
                _logger.LogError(ex, "Delete categoria ingredienti {Id} errore", id);
                return StatusCode(500, "Errore server");
            }
        }

        // GET: api/categoria-ingrediente/exists/{id}
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

        // GET: api/categoria-ingrediente/exists/nome/{nome}
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
                _logger.LogError(ex, "NomeExists {Nome} errore", nome);
                return StatusCode(500, "Errore server");
            }
        }
    }
}