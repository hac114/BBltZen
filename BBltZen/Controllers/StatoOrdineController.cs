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
    public class StatoOrdineController(
        IStatoOrdineRepository repository,
        ILogger<StatoOrdineController> logger) : ControllerBase
    {
        private readonly IStatoOrdineRepository _repository = repository;
        private readonly ILogger<StatoOrdineController> _logger = logger;

        // GET: api/stato-ordine
        [HttpGet("")]
        [AllowAnonymous]
        public async Task<ActionResult<PaginatedResponseDTO<StatoOrdineDTO>>> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var result = await _repository.GetAllAsync(page, pageSize);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetAll stati ordine errore");
                return StatusCode(500, "Errore server");
            }
        }

        // GET: api/stato-ordine/{id}
        [HttpGet("{id:int}")]
        [AllowAnonymous]
        public async Task<ActionResult<SingleResponseDTO<StatoOrdineDTO>>> GetById(int id)
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

        // GET: api/stato-ordine/nome/{nome}
        [HttpGet("nome/{nome}")]
        [AllowAnonymous]
        public async Task<ActionResult<SingleResponseDTO<StatoOrdineDTO>>> GetByNome(string nome)
        {
            try
            {
                var result = await _repository.GetByNomeAsync(nome);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetByNome errore nome: {Nome}", nome);
                return StatusCode(500, "Errore server");
            }
        }

        // GET: api/stato-ordine/non-terminali
        [HttpGet("non-terminali")]
        [AllowAnonymous]
        public async Task<ActionResult<PaginatedResponseDTO<StatoOrdineDTO>>> GetStatiNonTerminali([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var result = await _repository.GetStatiNonTerminaliAsync(page, pageSize);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetStatiNonTerminali errore");
                return StatusCode(500, "Errore server");
            }
        }

        // GET: api/stato-ordine/terminali
        [HttpGet("terminali")]
        [AllowAnonymous]
        public async Task<ActionResult<PaginatedResponseDTO<StatoOrdineDTO>>> GetStatiTerminali([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var result = await _repository.GetStatiTerminaliAsync(page, pageSize);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetStatiTerminali errore");
                return StatusCode(500, "Errore server");
            }
        }

        // POST: api/stato-ordine
        [HttpPost]
        // [Authorize(Roles = "Admin")]
        public async Task<ActionResult<SingleResponseDTO<StatoOrdineDTO>>> Create([FromBody] StatoOrdineDTO statoOrdineDto)
        {
            try
            {
                if (statoOrdineDto == null)
                    return BadRequest();

                var result = await _repository.AddAsync(statoOrdineDto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Create stato ordine errore");
                return StatusCode(500, "Errore server");
            }
        }

        // PUT: api/stato-ordine/{id}
        [HttpPut("{id:int}")]
        // [Authorize(Roles = "Admin")]
        public async Task<ActionResult<SingleResponseDTO<bool>>> Update(int id, [FromBody] StatoOrdineDTO statoOrdineDto)
        {
            try
            {
                if (statoOrdineDto == null)
                    return BadRequest();

                if (id != statoOrdineDto.StatoOrdineId)
                    return BadRequest();

                var result = await _repository.UpdateAsync(statoOrdineDto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Update stato ordine {Id} errore", id);
                return StatusCode(500, "Errore server");
            }
        }

        // DELETE: api/stato-ordine/{id}
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
                _logger.LogError(ex, "Delete stato ordine {Id} errore", id);
                return StatusCode(500, "Errore server");
            }
        }

        // GET: api/stato-ordine/exists/{id}
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

        // GET: api/stato-ordine/exists/nome/{nome}
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