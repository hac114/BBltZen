using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Repository.Interface;
using DTO;

namespace BBltZen.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    // [Authorize] // ✅ Commentato per test Swagger
    public class DimensioneQuantitaIngredientiController(
        IDimensioneQuantitaIngredientiRepository repository,
        ILogger<DimensioneQuantitaIngredientiController> logger) : ControllerBase
    {
        private readonly IDimensioneQuantitaIngredientiRepository _repository = repository;
        private readonly ILogger<DimensioneQuantitaIngredientiController> _logger = logger;

        // GET: api/dimensione-quantita-ingredienti
        [HttpGet("")]
        [AllowAnonymous]
        public async Task<ActionResult<PaginatedResponseDTO<DimensioneQuantitaIngredientiDTO>>> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var result = await _repository.GetAllAsync(page, pageSize);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetAll dimensioni quantità ingredienti errore");
                return StatusCode(500, "Errore server");
            }
        }

        // GET: api/dimensione-quantita-ingredienti/{id}
        [HttpGet("{id:int}")]
        [AllowAnonymous]
        public async Task<ActionResult<SingleResponseDTO<DimensioneQuantitaIngredientiDTO>>> GetById(int id)
        {
            try
            {
                var result = await _repository.GetByIdAsync(id);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetById dimensione quantità ingredienti errore ID: {Id}", id);
                return StatusCode(500, "Errore server");
            }
        }

        // GET: api/dimensione-quantita-ingredienti/bicchiere/{bicchiereId}
        [HttpGet("bicchiere/{bicchiereId:int}")]
        [AllowAnonymous]
        public async Task<ActionResult<PaginatedResponseDTO<DimensioneQuantitaIngredientiDTO>>> GetByBicchiereId(int bicchiereId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var result = await _repository.GetByBicchiereIdAsync(bicchiereId, page, pageSize);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetByBicchiereId errore bicchiereId: {BicchiereId}", bicchiereId);
                return StatusCode(500, "Errore server");
            }
        }

        // GET: api/dimensione-quantita-ingredienti/personalizzazione-ingrediente/{personalizzazioneIngredienteId}
        [HttpGet("personalizzazione-ingrediente/{personalizzazioneIngredienteId:int}")]
        [AllowAnonymous]
        public async Task<ActionResult<PaginatedResponseDTO<DimensioneQuantitaIngredientiDTO>>> GetByPersonalizzazioneIngredienteId(int personalizzazioneIngredienteId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var result = await _repository.GetByPersonalizzazioneIngredienteIdAsync(personalizzazioneIngredienteId, page, pageSize);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetByPersonalizzazioneIngredienteId errore personalizzazioneIngredienteId: {PersonalizzazioneIngredienteId}", personalizzazioneIngredienteId);
                return StatusCode(500, "Errore server");
            }
        }

        // GET: api/dimensione-quantita-ingredienti/bicchiere-descrizione
        [HttpGet("bicchiere-descrizione")]
        [AllowAnonymous]
        public async Task<ActionResult<PaginatedResponseDTO<DimensioneQuantitaIngredientiDTO>>> GetByBicchiereDescrizione([FromQuery] string descrizioneBicchiere, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var result = await _repository.GetByBicchiereDescrizioneAsync(descrizioneBicchiere, page, pageSize);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetByBicchiereDescrizione errore descrizioneBicchiere: {DescrizioneBicchiere}", descrizioneBicchiere);
                return StatusCode(500, "Errore server");
            }
        }

        // POST: api/dimensione-quantita-ingredienti
        [HttpPost]
        // [Authorize(Roles = "Admin")]
        public async Task<ActionResult<SingleResponseDTO<DimensioneQuantitaIngredientiDTO>>> Create([FromBody] DimensioneQuantitaIngredientiDTO dimensioneQuantitaIngredientiDto)
        {
            try
            {
                if (dimensioneQuantitaIngredientiDto == null)
                    return BadRequest();

                var result = await _repository.AddAsync(dimensioneQuantitaIngredientiDto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Create dimensione quantità ingredienti errore");
                return StatusCode(500, "Errore server");
            }
        }

        // PUT: api/dimensione-quantita-ingredienti/{id}
        [HttpPut("{id:int}")]
        // [Authorize(Roles = "Admin")]
        public async Task<ActionResult<SingleResponseDTO<bool>>> Update(int id, [FromBody] DimensioneQuantitaIngredientiDTO dimensioneQuantitaIngredientiDto)
        {
            try
            {
                if (dimensioneQuantitaIngredientiDto == null)
                    return BadRequest();

                if (id != dimensioneQuantitaIngredientiDto.DimensioneId)
                    return BadRequest();

                var result = await _repository.UpdateAsync(dimensioneQuantitaIngredientiDto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Update dimensione quantità ingredienti {Id} errore", id);
                return StatusCode(500, "Errore server");
            }
        }

        // DELETE: api/dimensione-quantita-ingredienti/{id}
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
                _logger.LogError(ex, "Delete dimensione quantità ingredienti {Id} errore", id);
                return StatusCode(500, "Errore server");
            }
        }

        // GET: api/dimensione-quantita-ingredienti/exists/{id}
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

        // GET: api/dimensione-quantita-ingredienti/exists/combinazione
        [HttpGet("exists/combinazione")]
        [AllowAnonymous]
        public async Task<ActionResult<SingleResponseDTO<bool>>> ExistsByCombinazione([FromQuery] int personalizzazioneIngredienteId, [FromQuery] int bicchiereId)
        {
            try
            {
                var result = await _repository.ExistsByCombinazioneAsync(personalizzazioneIngredienteId, bicchiereId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ExistsByCombinazione errore per personalizzazioneIngredienteId: {PersonalizzazioneIngredienteId}, bicchiereId: {BicchiereId}",
                    personalizzazioneIngredienteId, bicchiereId);
                return StatusCode(500, "Errore server");
            }
        }

        // GET: api/dimensione-quantita-ingredienti/count
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
                _logger.LogError(ex, "Count dimensione quantità ingredienti errore");
                return StatusCode(500, "Errore server");
            }
        }

        // GET: api/dimensione-quantita-ingredienti/count/personalizzazione-ingredienti/{personalizzazioneIngredienteId}
        [HttpGet("count/personalizzazione-ingredienti/{personalizzazioneIngredienteId:int}")]
        [AllowAnonymous]
        public async Task<ActionResult<SingleResponseDTO<int>>> GetCountByPersonalizzazioneIngredienti(int personalizzazioneIngredienteId)
        {
            try
            {
                var result = await _repository.GetCountByPersonalizzazioneIngredientiAsync(personalizzazioneIngredienteId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetCountByPersonalizzazioneIngredienti errore per personalizzazioneIngredienteId: {PersonalizzazioneIngredienteId}", personalizzazioneIngredienteId);
                return StatusCode(500, "Errore server");
            }
        }
    }
}