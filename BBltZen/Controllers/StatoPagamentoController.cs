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
    public class StatoPagamentoController(
        IStatoPagamentoRepository repository,
        ILogger<StatoPagamentoController> logger) : ControllerBase
    {
        private readonly IStatoPagamentoRepository _repository = repository;
        private readonly ILogger<StatoPagamentoController> _logger = logger;

        // GET: api/stato-pagamento
        [HttpGet("")]
        [AllowAnonymous]
        public async Task<ActionResult<PaginatedResponseDTO<StatoPagamentoDTO>>> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var result = await _repository.GetAllAsync(page, pageSize);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetAll stati pagamento errore");
                return StatusCode(500, "Errore server");
            }
        }

        // GET: api/stato-pagamento/{id}
        [HttpGet("{id:int}")]
        [AllowAnonymous]
        public async Task<ActionResult<SingleResponseDTO<StatoPagamentoDTO>>> GetById(int id)
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

        // GET: api/stato-pagamento/nome/{nome}
        [HttpGet("nome/{nome}")]
        [AllowAnonymous]
        public async Task<ActionResult<SingleResponseDTO<StatoPagamentoDTO>>> GetByNome(string nome)
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

        // POST: api/stato-pagamento
        [HttpPost]
        // [Authorize(Roles = "Admin")]
        public async Task<ActionResult<SingleResponseDTO<StatoPagamentoDTO>>> Create([FromBody] StatoPagamentoDTO statoPagamentoDto)
        {
            try
            {
                if (statoPagamentoDto == null)
                    return BadRequest();

                var result = await _repository.AddAsync(statoPagamentoDto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Create stato pagamento errore");
                return StatusCode(500, "Errore server");
            }
        }

        // PUT: api/stato-pagamento/{id}
        [HttpPut("{id:int}")]
        // [Authorize(Roles = "Admin")]
        public async Task<ActionResult<SingleResponseDTO<bool>>> Update(int id, [FromBody] StatoPagamentoDTO statoPagamentoDto)
        {
            try
            {
                if (statoPagamentoDto == null)
                    return BadRequest();

                if (id != statoPagamentoDto.StatoPagamentoId)
                    return BadRequest();

                var result = await _repository.UpdateAsync(statoPagamentoDto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Update stato pagamento {Id} errore", id);
                return StatusCode(500, "Errore server");
            }
        }

        // DELETE: api/stato-pagamento/{id}
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
                _logger.LogError(ex, "Delete stato pagamento {Id} errore", id);
                return StatusCode(500, "Errore server");
            }
        }

        // GET: api/stato-pagamento/exists/{id}
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

        // GET: api/stato-pagamento/exists/nome/{nome}
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