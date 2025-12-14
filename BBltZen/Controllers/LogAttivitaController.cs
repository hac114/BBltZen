using DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Repository.Interface;
using System;
using System.Threading.Tasks;

namespace BBltZen.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    //[Authorize] // ✅ TODO: Riattivare quando l'autenticazione sarà configurata
    public class LogAttivitaController(
        ILogAttivitaRepository repository,
        ILogger<LogAttivitaController> logger) : ControllerBase
    {
        private readonly ILogAttivitaRepository _repository = repository;
        private readonly ILogger<LogAttivitaController> _logger = logger;

        // ✅ GET: api/LogAttivita
        [HttpGet]
        public async Task<ActionResult<PaginatedResponseDTO<LogAttivitaDTO>>> GetAll(
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
                _logger.LogError(ex, "Errore in GetAll");
                return StatusCode(500, "Errore interno del server");
            }
        }

        // ✅ GET: api/LogAttivita/5
        [HttpGet("{id:int}")]
        public async Task<ActionResult<SingleResponseDTO<LogAttivitaDTO>>> GetById(int id)
        {
            try
            {
                var result = await _repository.GetByIdAsync(id);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore in GetById per ID: {Id}", id);
                return StatusCode(500, "Errore interno del server");
            }
        }

        // ✅ GET: api/LogAttivita/tipo/Login
        [HttpGet("tipo/{tipoAttivita}")]
        public async Task<ActionResult<PaginatedResponseDTO<LogAttivitaDTO>>> GetByTipoAttivita(
            string tipoAttivita,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                var result = await _repository.GetByTipoAttivitaAsync(tipoAttivita, page, pageSize);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore in GetByTipoAttivita per tipo: {Tipo}", tipoAttivita);
                return StatusCode(500, "Errore interno del server");
            }
        }

        // ✅ GET: api/LogAttivita/utente/5
        [HttpGet("utente/{utenteId:int}")]
        public async Task<ActionResult<PaginatedResponseDTO<LogAttivitaDTO>>> GetByUtenteId(
            int utenteId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                var result = await _repository.GetByUtenteIdAsync(utenteId, page, pageSize);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore in GetByUtenteId per utenteId: {UtenteId}", utenteId);
                return StatusCode(500, "Errore interno del server");
            }
        }

        // ✅ GET: api/LogAttivita/tipo-utente/Admin
        [HttpGet("tipo-utente/{tipoUtente}")]
        public async Task<ActionResult<PaginatedResponseDTO<LogAttivitaDTO>>> GetByTipoUtente(
            string tipoUtente,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                var result = await _repository.GetByTipoUtenteAsync(tipoUtente, page, pageSize);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore in GetByTipoUtente per tipoUtente: {TipoUtente}", tipoUtente);
                return StatusCode(500, "Errore interno del server");
            }
        }

        // ✅ GET: api/LogAttivita/statistiche/conteggio
        [HttpGet("statistiche/conteggio")]
        public async Task<ActionResult<SingleResponseDTO<int>>> GetNumeroAttivita(
            [FromQuery] DateTime? dataInizio = null,
            [FromQuery] DateTime? dataFine = null)
        {
            try
            {
                var result = await _repository.GetNumeroAttivitaAsync(dataInizio, dataFine);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore in GetNumeroAttivita");
                return StatusCode(500, "Errore interno del server");
            }
        }

        // ✅ GET: api/LogAttivita/filtro/data
        [HttpGet("filtro/data")]
        public async Task<ActionResult<PaginatedResponseDTO<LogAttivitaDTO>>> GetByDateRange(
            [FromQuery] DateTime dataInizio,
            [FromQuery] DateTime dataFine,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                var result = await _repository.GetByDateRangeAsync(dataInizio, dataFine, page, pageSize);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore in GetByDateRange per periodo: {DataInizio} - {DataFine}", dataInizio, dataFine);
                return StatusCode(500, "Errore interno del server");
            }
        }

        // ✅ GET: api/LogAttivita/statistiche/riepilogo
        [HttpGet("statistiche/riepilogo")]
        public async Task<ActionResult<SingleResponseDTO<Dictionary<string, int>>>> GetStatisticheAttivita(
            [FromQuery] DateTime? dataInizio = null,
            [FromQuery] DateTime? dataFine = null)
        {
            try
            {
                var result = await _repository.GetStatisticheAttivitaAsync(dataInizio, dataFine);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore in GetStatisticheAttivita");
                return StatusCode(500, "Errore interno del server");
            }
        }

        // ✅ POST: api/LogAttivita
        [HttpPost]
        public async Task<ActionResult<SingleResponseDTO<LogAttivitaDTO>>> Create(
            [FromBody] LogAttivitaDTO logAttivitaDto)
        {
            try
            {
                if (logAttivitaDto == null)
                    return BadRequest();

                var result = await _repository.AddAsync(logAttivitaDto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore in Create");
                return StatusCode(500, "Errore interno del server");
            }
        }

        // ✅ DELETE: api/LogAttivita/cleanup
        [HttpDelete("cleanup")]
        //[Authorize(Roles = "Admin")] // ✅ TODO: Riattivare quando l'autenticazione sarà configurata
        public async Task<ActionResult<SingleResponseDTO<int>>> CleanupOldLogs(
            [FromQuery] int giorniRitenzione = 90)
        {
            try
            {
                var result = await _repository.CleanupOldLogsAsync(giorniRitenzione);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore in CleanupOldLogs per giorni: {Giorni}", giorniRitenzione);
                return StatusCode(500, "Errore interno del server");
            }
        }

        // ✅ GET: api/LogAttivita/exists/5
        [HttpGet("exists/{logId:int}")]
        public async Task<ActionResult<SingleResponseDTO<bool>>> Exists(int logId)
        {
            try
            {
                var result = await _repository.ExistsAsync(logId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore in Exists per LogId: {LogId}", logId);
                return StatusCode(500, "Errore interno del server");
            }
        }
    }
}