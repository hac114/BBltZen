using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Repository.Interface;
using DTO;

namespace BBltZen.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    // [Authorize] // ✅ Commentato per test Swagger
    public class PersonalizzazioneCustomController(
        IPersonalizzazioneCustomRepository repository,
        ILogger<PersonalizzazioneCustomController> logger) : ControllerBase
    {
        private readonly IPersonalizzazioneCustomRepository _repository = repository;
        private readonly ILogger<PersonalizzazioneCustomController> _logger = logger;

        // GET: api/personalizzazione-custom
        [HttpGet("")]
        [AllowAnonymous]
        public async Task<ActionResult<PaginatedResponseDTO<PersonalizzazioneCustomDTO>>> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var result = await _repository.GetAllAsync(page, pageSize);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetAll personalizzazioni custom errore");
                return StatusCode(500, "Errore server");
            }
        }

        // GET: api/personalizzazione-custom/{id}
        [HttpGet("{id:int}")]
        [AllowAnonymous]
        public async Task<ActionResult<SingleResponseDTO<PersonalizzazioneCustomDTO>>> GetById(int id)
        {
            try
            {
                var result = await _repository.GetByIdAsync(id);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetById personalizzazione custom errore ID: {Id}", id);
                return StatusCode(500, "Errore server");
            }
        }

        // GET: api/personalizzazione-custom/bicchiere/{bicchiereId}
        [HttpGet("bicchiere/{bicchiereId:int}")]
        [AllowAnonymous]
        public async Task<ActionResult<PaginatedResponseDTO<PersonalizzazioneCustomDTO>>> GetByBicchiereId(int bicchiereId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var result = await _repository.GetBicchiereByIdAsync(bicchiereId, page, pageSize);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetByBicchiereId errore bicchiereId: {BicchiereId}", bicchiereId);
                return StatusCode(500, "Errore server");
            }
        }

        // GET: api/personalizzazione-custom/grado-dolcezza/{gradoDolcezza}
        [HttpGet("grado-dolcezza/{gradoDolcezza:int}")]
        [AllowAnonymous]
        public async Task<ActionResult<PaginatedResponseDTO<PersonalizzazioneCustomDTO>>> GetByGradoDolcezza(byte gradoDolcezza, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var result = await _repository.GetByGradoDolcezzaAsync(gradoDolcezza, page, pageSize);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetByGradoDolcezza errore gradoDolcezza: {GradoDolcezza}", gradoDolcezza);
                return StatusCode(500, "Errore server");
            }
        }

        // GET: api/personalizzazione-custom/bicchiere-descrizione
        [HttpGet("bicchiere-descrizione")]
        [AllowAnonymous]
        public async Task<ActionResult<PaginatedResponseDTO<PersonalizzazioneCustomDTO>>> GetByBicchiereDescrizione([FromQuery] string descrizioneBicchiere, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var result = await _repository.GetBicchiereByDescrizioneAsync(descrizioneBicchiere, page, pageSize);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetByBicchiereDescrizione errore descrizioneBicchiere: {DescrizioneBicchiere}", descrizioneBicchiere);
                return StatusCode(500, "Errore server");
            }
        }

        // GET: api/personalizzazione-custom/nome
        [HttpGet("nome")]
        [AllowAnonymous]
        public async Task<ActionResult<PaginatedResponseDTO<PersonalizzazioneCustomDTO>>> GetByNome([FromQuery] string nome, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
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

        // POST: api/personalizzazione-custom
        [HttpPost]
        // [Authorize(Roles = "Admin")]
        public async Task<ActionResult<SingleResponseDTO<PersonalizzazioneCustomDTO>>> Create([FromBody] PersonalizzazioneCustomDTO personalizzazioneCustomDto)
        {
            try
            {
                if (personalizzazioneCustomDto == null)
                    return BadRequest();

                var result = await _repository.AddAsync(personalizzazioneCustomDto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Create personalizzazione custom errore");
                return StatusCode(500, "Errore server");
            }
        }

        // PUT: api/personalizzazione-custom/{id}
        [HttpPut("{id:int}")]
        // [Authorize(Roles = "Admin")]
        public async Task<ActionResult<SingleResponseDTO<bool>>> Update(int id, [FromBody] PersonalizzazioneCustomDTO personalizzazioneCustomDto)
        {
            try
            {
                if (personalizzazioneCustomDto == null)
                    return BadRequest();

                if (id != personalizzazioneCustomDto.PersCustomId)
                    return BadRequest();

                var result = await _repository.UpdateAsync(personalizzazioneCustomDto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Update personalizzazione custom {Id} errore", id);
                return StatusCode(500, "Errore server");
            }
        }

        // DELETE: api/personalizzazione-custom/{id}
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
                _logger.LogError(ex, "Delete personalizzazione custom {Id} errore", id);
                return StatusCode(500, "Errore server");
            }
        }

        // GET: api/personalizzazione-custom/exists/{id}
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

        // GET: api/personalizzazione-custom/count
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
                _logger.LogError(ex, "Count personalizzazioni custom errore");
                return StatusCode(500, "Errore server");
            }
        }

        // GET: api/personalizzazione-custom/count/bicchiere-descrizione
        [HttpGet("count/bicchiere-descrizione")]
        [AllowAnonymous]
        public async Task<ActionResult<SingleResponseDTO<int>>> CountByBicchiereDescrizione([FromQuery] string descrizioneBicchiere)
        {
            try
            {
                var result = await _repository.CountBicchiereByDescrizioneAsync(descrizioneBicchiere);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "CountByBicchiereDescrizione errore descrizioneBicchiere: {DescrizioneBicchiere}",
                    descrizioneBicchiere);
                return StatusCode(500, "Errore server");
            }
        }

        // GET: api/personalizzazione-custom/count/grado-dolcezza/{gradoDolcezza}
        [HttpGet("count/grado-dolcezza/{gradoDolcezza:int}")]
        [AllowAnonymous]
        public async Task<ActionResult<SingleResponseDTO<int>>> CountByGradoDolcezza(byte gradoDolcezza)
        {
            try
            {
                var result = await _repository.CountByGradoDolcezzaAsync(gradoDolcezza);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "CountByGradoDolcezza errore gradoDolcezza: {GradoDolcezza}",
                    gradoDolcezza);
                return StatusCode(500, "Errore server");
            }
        }
    }
}