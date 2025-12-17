using DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Repository.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BBltZen.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    // [Authorize] // ✅ Commentato per test Swagger
    public class ConfigSoglieTempiController(
        IConfigSoglieTempiRepository repository,
        ILogger<ConfigSoglieTempiController> logger) : ControllerBase
    {
        private readonly IConfigSoglieTempiRepository _repository = repository;
        private readonly ILogger<ConfigSoglieTempiController> _logger = logger;

        // GET: api/config-soglie-tempi
        [HttpGet("")]
        [AllowAnonymous]
        public async Task<ActionResult<PaginatedResponseDTO<ConfigSoglieTempiDTO>>> GetAll(
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
                _logger.LogError(ex, "GetAll configurazioni soglie tempi errore");
                return StatusCode(500, "Errore interno del server");
            }
        }

        // GET: api/config-soglie-tempi/{id}
        [HttpGet("{id:int}")]
        [AllowAnonymous]
        public async Task<ActionResult<SingleResponseDTO<ConfigSoglieTempiDTO>>> GetById(int id)
        {
            try
            {
                var result = await _repository.GetByIdAsync(id);

                if (!result.Success)
                    return NotFound(new { message = result.Message });

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetById configurazione soglie tempi ID: {Id} errore", id);
                return StatusCode(500, "Errore interno del server");
            }
        }

        // GET: api/config-soglie-tempi/stato-ordine/{nome}
        [HttpGet("stato-ordine/{nome}")]
        [AllowAnonymous]
        public async Task<ActionResult<SingleResponseDTO<ConfigSoglieTempiDTO>>> GetByStatoOrdine(string nome)
        {
            try
            {
                var result = await _repository.GetByStatoOrdineAsync(nome);

                if (!result.Success)
                    return NotFound(new { message = result.Message });

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetByStatoOrdine configurazione soglie tempi nome: {Nome} errore", nome);
                return StatusCode(500, "Errore interno del server");
            }
        }

        // GET: api/config-soglie-tempi/configurazioni-per-stati
        [HttpGet("configurazioni-per-stati")]
        [AllowAnonymous]
        public async Task<ActionResult<SingleResponseDTO<Dictionary<int, ConfigSoglieTempiDTO>>>> GetConfigurazioniPerStati(
            [FromQuery] List<int> stati)
        {
            try
            {
                if (stati == null || !stati.Any())
                    return BadRequest(new { message = "Specificare almeno uno stato ordine" });

                var result = await _repository.GetSoglieByStatiOrdineAsync(stati);

                if (!result.Success)
                    return BadRequest(new { message = result.Message });

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetConfigurazioniPerStati configurazione soglie tempi stati: {Stati} errore",
                    stati != null ? string.Join(", ", stati) : "null");
                return StatusCode(500, "Errore interno del server");
            }
        }

        // POST: api/config-soglie-tempi
        [HttpPost]
        // [Authorize(Roles = "Admin")]
        public async Task<ActionResult<SingleResponseDTO<ConfigSoglieTempiDTO>>> Create(
            [FromBody] ConfigSoglieTempiDTO configSoglieTempiDto)
        {
            try
            {
                if (configSoglieTempiDto == null)
                    return BadRequest(new { message = "Il corpo della richiesta non può essere vuoto" });

                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var result = await _repository.AddAsync(configSoglieTempiDto);

                if (!result.Success)
                    return BadRequest(new { message = result.Message });

                // Controllo esplicito per evitare dereferenziamento null
                if (result.Data == null)
                {
                    _logger.LogWarning("Create configurazione: operazione riuscita ma dati nulli");
                    return StatusCode(500, "Errore interno del server: dati non disponibili");
                }

                return CreatedAtAction(nameof(GetById), new { id = result.Data.SogliaId }, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Create configurazione soglie tempi errore");
                return StatusCode(500, "Errore interno del server");
            }
        }

        // PUT: api/config-soglie-tempi/{id}
        [HttpPut("{id:int}")]
        // [Authorize(Roles = "Admin")]
        public async Task<ActionResult<SingleResponseDTO<bool>>> Update(
            int id,
            [FromBody] ConfigSoglieTempiDTO configSoglieTempiDto)
        {
            try
            {
                if (configSoglieTempiDto == null)
                    return BadRequest(new { message = "Il corpo della richiesta non può essere vuoto" });

                if (id != configSoglieTempiDto.SogliaId)
                    return BadRequest(new { message = "L'ID nella route non corrisponde all'ID nel corpo" });

                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var result = await _repository.UpdateAsync(configSoglieTempiDto);

                if (!result.Success)
                    return BadRequest(new { message = result.Message });

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Update configurazione soglie tempi {Id} errore", id);
                return StatusCode(500, "Errore interno del server");
            }
        }

        // DELETE: api/config-soglie-tempi/{id}
        [HttpDelete("{id:int}")]
        // [Authorize(Roles = "Admin")]
        public async Task<ActionResult<SingleResponseDTO<bool>>> Delete(int id)
        {
            try
            {
                var utenteRichiedente = User.Identity?.Name ?? "System";

                var result = await _repository.DeleteAsync(id, utenteRichiedente);

                if (!result.Success)
                    return BadRequest(new { message = result.Message });

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Delete configurazione soglie tempi {Id} errore", id);
                return StatusCode(500, "Errore interno del server");
            }
        }

        // GET: api/config-soglie-tempi/exists/{id}
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
                _logger.LogError(ex, "Exists configurazione soglie tempi {Id} errore", id);
                return StatusCode(500, "Errore interno del server");
            }
        }

        // GET: api/config-soglie-tempi/exists/stato-ordine/{nome}
        [HttpGet("exists/stato-ordine/{nome}")]
        [AllowAnonymous]
        public async Task<ActionResult<SingleResponseDTO<bool>>> StatoOrdineExists(string nome)
        {
            try
            {
                var result = await _repository.ExistsByStatoOrdine(nome);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "StatoOrdineExists configurazione soglie tempi nome: {Nome} errore", nome);
                return StatusCode(500, "Errore interno del server");
            }
        }
    }
}