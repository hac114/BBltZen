using DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Repository.Interface;

namespace BBltZen.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    // [Authorize] // ✅ Commentato per test Swagger
    public class BevandaCustomController(IBevandaCustomRepository repository, ILogger<BevandaCustomController> logger) : ControllerBase
    {
        private readonly IBevandaCustomRepository _repository = repository;
        private readonly ILogger<BevandaCustomController> _logger = logger;

        // GET: api/bevanda-custom
        [HttpGet("")]
        [AllowAnonymous]
        public async Task<ActionResult<PaginatedResponseDTO<BevandaCustomDTO>>> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var result = await _repository.GetAllAsync(page, pageSize);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetAll bevande custom errore");
                return StatusCode(500, "Errore server");
            }
        }

        // GET: api/bevanda-custom/{id}
        [HttpGet("{id:int}")]
        [AllowAnonymous]
        public async Task<ActionResult<BevandaCustomDTO>> GetById(int id)
        {
            try
            {
                var result = await _repository.GetByIdAsync(id);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "GetById ID: {Id} non trovato", id);
                return NotFound(ex.Message);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "GetById ID: {Id} non valido", id);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetById errore ID: {Id}", id);
                return StatusCode(500, "Errore server");
            }
        }

        // GET: api/bevanda-custom/pers-custom/{persCustomId}
        [HttpGet("pers-custom/{persCustomId:int}")]
        [AllowAnonymous]
        public async Task<ActionResult<SingleResponseDTO<BevandaCustomDTO>>> GetByPersCustomId(int persCustomId)
        {
            try
            {
                var result = await _repository.GetByPersCustomIdAsync(persCustomId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetByPersCustomId errore persCustomId: {PersCustomId}", persCustomId);
                return StatusCode(500, "Errore server");
            }
        }

        // GET: api/bevanda-custom/ordinate-per-dimensione
        [HttpGet("ordinate-per-dimensione")]
        [AllowAnonymous]
        public async Task<ActionResult<PaginatedResponseDTO<BevandaCustomDTO>>> GetAllOrderedByDimensione([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var result = await _repository.GetAllOrderedByDimensioneAsync(page, pageSize);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetAllOrderedByDimensione errore");
                return StatusCode(500, "Errore server");
            }
        }

        // GET: api/bevanda-custom/ordinate-per-personalizzazione
        [HttpGet("ordinate-per-personalizzazione")]
        [AllowAnonymous]
        public async Task<ActionResult<PaginatedResponseDTO<BevandaCustomDTO>>> GetAllOrderedByPersonalizzazione([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var result = await _repository.GetAllOrderedByPersonalizzazioneAsync(page, pageSize);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetAllOrderedByPersonalizzazione errore");
                return StatusCode(500, "Errore server");
            }
        }

        // POST: api/bevanda-custom
        [HttpPost]
        // [Authorize(Roles = "Admin")]
        public async Task<ActionResult<SingleResponseDTO<BevandaCustomDTO>>> Create([FromBody] BevandaCustomDTO bevandaCustomDto)
        {
            try
            {
                if (bevandaCustomDto == null)
                    return BadRequest("DTO non può essere null");

                var result = await _repository.AddAsync(bevandaCustomDto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Create bevanda custom errore");
                return StatusCode(500, "Errore server");
            }
        }

        // PUT: api/bevanda-custom/{id}
        [HttpPut("{id:int}")]
        // [Authorize(Roles = "Admin")]
        public async Task<ActionResult<SingleResponseDTO<bool>>> Update(int id, [FromBody] BevandaCustomDTO bevandaCustomDto)
        {
            try
            {
                if (bevandaCustomDto == null)
                    return BadRequest("DTO non può essere null");

                if (id != bevandaCustomDto.ArticoloId)
                    return BadRequest("ID nel percorso non corrisponde all'ID nel DTO");

                var result = await _repository.UpdateAsync(bevandaCustomDto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Update bevanda custom {Id} errore", id);
                return StatusCode(500, "Errore server");
            }
        }

        // DELETE: api/bevanda-custom/{id}
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
                _logger.LogError(ex, "Delete bevanda custom {Id} errore", id);
                return StatusCode(500, "Errore server");
            }
        }

        // GET: api/bevanda-custom/exists/{id}
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

        // GET: api/bevanda-custom/exists/pers-custom/{persCustomId}
        [HttpGet("exists/pers-custom/{persCustomId:int}")]
        [AllowAnonymous]
        public async Task<ActionResult<SingleResponseDTO<bool>>> ExistsByPersCustomId(int persCustomId)
        {
            try
            {
                var result = await _repository.ExistsByPersCustomIdAsync(persCustomId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ExistsByPersCustomId {PersCustomId} errore", persCustomId);
                return StatusCode(500, "Errore server");
            }
        }

        // GET: api/bevanda-custom/card-prodotti
        [HttpGet("card-prodotti")]
        [AllowAnonymous]
        public async Task<ActionResult<PaginatedResponseDTO<BevandaCustomCardDTO>>> GetCardProdotti([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var result = await _repository.GetCardProdottiAsync(page, pageSize);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetCardProdotti errore");
                return StatusCode(500, "Errore server");
            }
        }

        // GET: api/bevanda-custom/card-prodotto/{id}
        [HttpGet("card-prodotto/{id:int}")]
        [AllowAnonymous]
        public async Task<ActionResult<SingleResponseDTO<BevandaCustomCardDTO>>> GetCardProdottoById(int id)
        {
            try
            {
                var result = await _repository.GetCardProdottoByIdAsync(id);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetCardProdottoById errore ID: {Id}", id);
                return StatusCode(500, "Errore server");
            }
        }

        // GET: api/bevanda-custom/card/personalizzazione
        [HttpGet("card/personalizzazione")]
        [AllowAnonymous]
        public async Task<ActionResult<PaginatedResponseDTO<BevandaCustomCardDTO>>> GetCardPersonalizzazione([FromQuery] string nomePersonalizzazione, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(nomePersonalizzazione))
                    return BadRequest("nomePersonalizzazione è obbligatorio");

                var result = await _repository.GetCardPersonalizzazioneAsync(nomePersonalizzazione, page, pageSize);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetCardPersonalizzazione errore nomePersonalizzazione: {NomePersonalizzazione}", nomePersonalizzazione);
                return StatusCode(500, "Errore server");
            }
        }

        // GET: api/bevanda-custom/card/dimensione-bicchiere
        [HttpGet("card/dimensione-bicchiere")]
        [AllowAnonymous]
        public async Task<ActionResult<PaginatedResponseDTO<BevandaCustomCardDTO>>> GetCardDimensioneBicchiere([FromQuery] string nomePersonalizzazione, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(nomePersonalizzazione))
                    return BadRequest("nomePersonalizzazione è obbligatorio");

                var result = await _repository.GetCardDimensioneBicchiereAsync(nomePersonalizzazione, page, pageSize);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetCardDimensioneBicchiere errore nomePersonalizzazione: {NomePersonalizzazione}", nomePersonalizzazione);
                return StatusCode(500, "Errore server");
            }
        }

        // GET: api/bevanda-custom/count
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
                _logger.LogError(ex, "Count errore");
                return StatusCode(500, "Errore server");
            }
        }

        // GET: api/bevanda-custom/count/dimensione-bicchiere
        [HttpGet("count/dimensione-bicchiere")]
        [AllowAnonymous]
        public async Task<ActionResult<SingleResponseDTO<int>>> CountDimensioneBicchiere([FromQuery] string descrizionBicchiere)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(descrizionBicchiere))
                    return BadRequest("descrizionBicchiere è obbligatorio");

                var result = await _repository.CountDimensioneBicchiereAsync(descrizionBicchiere);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "CountDimensioneBicchiere errore descrizionBicchiere: {DescrizionBicchiere}", descrizionBicchiere);
                return StatusCode(500, "Errore server");
            }
        }

        // GET: api/bevanda-custom/count/personalizzazione
        [HttpGet("count/personalizzazione")]
        [AllowAnonymous]
        public async Task<ActionResult<SingleResponseDTO<int>>> CountPersonalizzazione([FromQuery] string nomePersonalizzazione)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(nomePersonalizzazione))
                    return BadRequest("nomePersonalizzazione è obbligatorio");

                var result = await _repository.CountPersonalizzazioneAsync(nomePersonalizzazione);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "CountPersonalizzazione errore nomePersonalizzazione: {NomePersonalizzazione}", nomePersonalizzazione);
                return StatusCode(500, "Errore server");
            }
        }
    }
}