using DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Repository.Interface;

namespace BBltZen.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    // [Authorize] // ✅ Commentato per test Swagger
    public class BevandaStandardController(IBevandaStandardRepository repository, ILogger<BevandaStandardController> logger) : ControllerBase
    {
        private readonly IBevandaStandardRepository _repository = repository;
        private readonly ILogger<BevandaStandardController> _logger = logger;

        // GET: api/bevanda-standard
        [HttpGet("")]
        [AllowAnonymous]
        public async Task<ActionResult<PaginatedResponseDTO<BevandaStandardDTO>>> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var result = await _repository.GetAllAsync(page, pageSize);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetAll bevande standard errore");
                return StatusCode(500, "Errore server");
            }
        }

        // GET: api/bevanda-standard/{id}
        [HttpGet("{id:int}")]
        [AllowAnonymous]
        public async Task<ActionResult<SingleResponseDTO<BevandaStandardDTO>>> GetById(int id)
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

        // GET: api/bevanda-standard/disponibili
        [HttpGet("disponibili")]
        [AllowAnonymous]
        public async Task<ActionResult<PaginatedResponseDTO<BevandaStandardDTO>>> GetDisponibili([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var result = await _repository.GetDisponibiliAsync(page, pageSize);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetDisponibili errore");
                return StatusCode(500, "Errore server");
            }
        }

        // GET: api/bevanda-standard/ordinate-per-dimensione
        [HttpGet("ordinate-per-dimensione")]
        [AllowAnonymous]
        public async Task<ActionResult<PaginatedResponseDTO<BevandaStandardDTO>>> GetAllOrderedByDimensione([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
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

        // GET: api/bevanda-standard/ordinate-per-personalizzazione
        [HttpGet("ordinate-per-personalizzazione")]
        [AllowAnonymous]
        public async Task<ActionResult<PaginatedResponseDTO<BevandaStandardDTO>>> GetAllOrderedByPersonalizzazione([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
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

        // GET: api/bevanda-standard/dimensione-bicchiere/{dimensioneBicchiereId}
        [HttpGet("dimensione-bicchiere/{dimensioneBicchiereId:int}")]
        [AllowAnonymous]
        public async Task<ActionResult<PaginatedResponseDTO<BevandaStandardDTO>>> GetByDimensioneBicchiere(int dimensioneBicchiereId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var result = await _repository.GetByDimensioneBicchiereAsync(dimensioneBicchiereId, page, pageSize);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetByDimensioneBicchiere errore dimensioneBicchiereId: {DimensioneBicchiereId}",
                    dimensioneBicchiereId);
                return StatusCode(500, "Errore server");
            }
        }

        // GET: api/bevanda-standard/personalizzazione/{personalizzazioneId}
        [HttpGet("personalizzazione/{personalizzazioneId:int}")]
        [AllowAnonymous]
        public async Task<ActionResult<PaginatedResponseDTO<BevandaStandardDTO>>> GetByPersonalizzazione(int personalizzazioneId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var result = await _repository.GetByPersonalizzazioneAsync(personalizzazioneId, page, pageSize);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetByPersonalizzazione errore personalizzazioneId: {PersonalizzazioneId}",
                    personalizzazioneId);
                return StatusCode(500, "Errore server");
            }
        }

        // POST: api/bevanda-standard
        [HttpPost]
        // [Authorize(Roles = "Admin")]
        public async Task<ActionResult<SingleResponseDTO<BevandaStandardDTO>>> Create([FromBody] BevandaStandardDTO bevandaStandardDto)
        {
            try
            {
                if (bevandaStandardDto == null)
                    return BadRequest();

                var result = await _repository.AddAsync(bevandaStandardDto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Create bevanda standard errore");
                return StatusCode(500, "Errore server");
            }
        }

        // PUT: api/bevanda-standard/{id}
        [HttpPut("{id:int}")]
        // [Authorize(Roles = "Admin")]
        public async Task<ActionResult<SingleResponseDTO<bool>>> Update(int id, [FromBody] BevandaStandardDTO bevandaStandardDto)
        {
            try
            {
                if (bevandaStandardDto == null)
                    return BadRequest();

                if (id != bevandaStandardDto.ArticoloId)
                    return BadRequest();

                var result = await _repository.UpdateAsync(bevandaStandardDto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Update bevanda standard {Id} errore", id);
                return StatusCode(500, "Errore server");
            }
        }

        // DELETE: api/bevanda-standard/{id}
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
                _logger.LogError(ex, "Delete bevanda standard {Id} errore", id);
                return StatusCode(500, "Errore server");
            }
        }

        // GET: api/bevanda-standard/exists/{id}
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

        // GET: api/bevanda-standard/exists/combinazione
        [HttpGet("exists/combinazione")]
        [AllowAnonymous]
        public async Task<ActionResult<SingleResponseDTO<bool>>> ExistsByCombinazione([FromQuery] int personalizzazioneId, [FromQuery] int dimensioneBicchiereId)
        {
            try
            {
                var result = await _repository.ExistsByCombinazioneAsync(personalizzazioneId, dimensioneBicchiereId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ExistsByCombinazione errore personalizzazioneId: {PersonalizzazioneId}, dimensioneBicchiereId: {DimensioneBicchiereId}",
                    personalizzazioneId, dimensioneBicchiereId);
                return StatusCode(500, "Errore server");
            }
        }

        // GET: api/bevanda-standard/exists/combinazione-stringa
        [HttpGet("exists/combinazione-stringa")]
        [AllowAnonymous]
        public async Task<ActionResult<SingleResponseDTO<bool>>> ExistsByCombinazioneStringa([FromQuery] string personalizzazione, [FromQuery] string descrizioneBicchiere)
        {
            try
            {
                var result = await _repository.ExistsByCombinazioneAsync(personalizzazione, descrizioneBicchiere);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ExistsByCombinazioneStringa errore personalizzazione: {Personalizzazione}, descrizioneBicchiere: {DescrizioneBicchiere}",
                    personalizzazione, descrizioneBicchiere);
                return StatusCode(500, "Errore server");
            }
        }

        // GET: api/bevanda-standard/card-prodotti
        [HttpGet("card-prodotti")]
        [AllowAnonymous]
        public async Task<ActionResult<PaginatedResponseDTO<BevandaStandardCardDTO>>> GetCardProdotti([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
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

        // GET: api/bevanda-standard/card-prodotto/{id}
        [HttpGet("card-prodotto/{id:int}")]
        [AllowAnonymous]
        public async Task<ActionResult<SingleResponseDTO<BevandaStandardCardDTO>>> GetCardProdottoById(int id)
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

        // GET: api/bevanda-standard/primo-piano
        [HttpGet("primo-piano")]
        [AllowAnonymous]
        public async Task<ActionResult<PaginatedResponseDTO<BevandaStandardDTO>>> GetPrimoPiano([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var result = await _repository.GetPrimoPianoAsync(page, pageSize);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetPrimoPiano errore");
                return StatusCode(500, "Errore server");
            }
        }

        // GET: api/bevanda-standard/secondo-piano
        [HttpGet("secondo-piano")]
        [AllowAnonymous]
        public async Task<ActionResult<PaginatedResponseDTO<BevandaStandardDTO>>> GetSecondoPiano([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var result = await _repository.GetSecondoPianoAsync(page, pageSize);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetSecondoPiano errore");
                return StatusCode(500, "Errore server");
            }
        }

        // GET: api/bevanda-standard/card-prodotti-primo-piano
        [HttpGet("card-prodotti-primo-piano")]
        [AllowAnonymous]
        public async Task<ActionResult<PaginatedResponseDTO<BevandaStandardCardDTO>>> GetCardProdottiPrimoPiano([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var result = await _repository.GetCardProdottiPrimoPianoAsync(page, pageSize);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetCardProdottiPrimoPiano errore");
                return StatusCode(500, "Errore server");
            }
        }

        // GET: api/bevanda-standard/count
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

        // GET: api/bevanda-standard/count/primo-piano
        [HttpGet("count/primo-piano")]
        [AllowAnonymous]
        public async Task<ActionResult<SingleResponseDTO<int>>> CountPrimoPiano()
        {
            try
            {
                var result = await _repository.CountPrimoPianoAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "CountPrimoPiano errore");
                return StatusCode(500, "Errore server");
            }
        }

        // GET: api/bevanda-standard/count/secondo-piano
        [HttpGet("count/secondo-piano")]
        [AllowAnonymous]
        public async Task<ActionResult<SingleResponseDTO<int>>> CountSecondoPiano()
        {
            try
            {
                var result = await _repository.CountSecondoPianoAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "CountSecondoPiano errore");
                return StatusCode(500, "Errore server");
            }
        }

        // GET: api/bevanda-standard/count/disponibili
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
                _logger.LogError(ex, "CountDisponibili errore");
                return StatusCode(500, "Errore server");
            }
        }

        // GET: api/bevanda-standard/count/non-disponibili
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
                _logger.LogError(ex, "CountNonDisponibili errore");
                return StatusCode(500, "Errore server");
            }
        }
    }
}