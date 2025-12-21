using DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Repository.Interface;

namespace BBltZen.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    // [Authorize] // ✅ Commentato per test Swagger
    public class PersonalizzazioneController(
        IPersonalizzazioneRepository repository,
        ILogger<PersonalizzazioneController> logger) : ControllerBase
    {
        private readonly IPersonalizzazioneRepository _repository = repository;
        private readonly ILogger<PersonalizzazioneController> _logger = logger;

        // GET: api/personalizzazione
        [HttpGet("")]
        [AllowAnonymous]
        public async Task<ActionResult<PaginatedResponseDTO<PersonalizzazioneDTO>>> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var result = await _repository.GetAllAsync(page, pageSize);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetAll personalizzazioni errore");
                return StatusCode(500, "Errore server");
            }
        }

        // GET: api/personalizzazione/{id}
        [HttpGet("{id:int}")]
        [AllowAnonymous]
        public async Task<ActionResult<SingleResponseDTO<PersonalizzazioneDTO>>> GetById(int id)
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

        // GET: api/personalizzazione/nome/{nome}
        [HttpGet("nome/{nome}")]
        [AllowAnonymous]
        public async Task<ActionResult<PaginatedResponseDTO<PersonalizzazioneDTO>>> GetByNome(string nome, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
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

        // POST: api/personalizzazione
        [HttpPost]
        // [Authorize(Roles = "Admin")]
        public async Task<ActionResult<SingleResponseDTO<PersonalizzazioneDTO>>> Create([FromBody] PersonalizzazioneDTO personalizzazioneDto)
        {
            try
            {
                if (personalizzazioneDto == null)
                    return BadRequest();

                var result = await _repository.AddAsync(personalizzazioneDto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Create personalizzazione errore");
                return StatusCode(500, "Errore server");
            }
        }

        // PUT: api/personalizzazione/{id}
        [HttpPut("{id:int}")]
        // [Authorize(Roles = "Admin")]
        public async Task<ActionResult<SingleResponseDTO<bool>>> Update(int id, [FromBody] PersonalizzazioneDTO personalizzazioneDto)
        {
            try
            {
                if (personalizzazioneDto == null)
                    return BadRequest();

                if (id != personalizzazioneDto.PersonalizzazioneId)
                    return BadRequest();

                var result = await _repository.UpdateAsync(personalizzazioneDto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Update personalizzazione {Id} errore", id);
                return StatusCode(500, "Errore server");
            }
        }

        // DELETE: api/personalizzazione/{id}
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
                _logger.LogError(ex, "Delete personalizzazione {Id} errore", id);
                return StatusCode(500, "Errore server");
            }
        }

        // GET: api/personalizzazione/exists/{id}
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

        // GET: api/personalizzazione/exists/nome/{nome}
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