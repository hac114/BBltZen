using Microsoft.AspNetCore.Mvc;
using DTO;
using Repository.Interface;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BBltZen.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LogAccessiController : ControllerBase
    {
        private readonly ILogAccessiRepository _repository;

        public LogAccessiController(ILogAccessiRepository repository)
        {
            _repository = repository;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<LogAccessiDTO>>> GetAll()
        {
            var result = await _repository.GetAllAsync();
            return Ok(result);
        }

        [HttpGet("{logId}")]
        public async Task<ActionResult<LogAccessiDTO>> GetById(int logId)
        {
            var result = await _repository.GetByIdAsync(logId);

            if (result == null)
                return NotFound();

            return Ok(result);
        }

        [HttpGet("utente/{utenteId}")]
        public async Task<ActionResult<IEnumerable<LogAccessiDTO>>> GetByUtenteId(int utenteId)
        {
            var result = await _repository.GetByUtenteIdAsync(utenteId);
            return Ok(result);
        }

        [HttpGet("cliente/{clienteId}")]
        public async Task<ActionResult<IEnumerable<LogAccessiDTO>>> GetByClienteId(int clienteId)
        {
            var result = await _repository.GetByClienteIdAsync(clienteId);
            return Ok(result);
        }

        [HttpGet("tipo-accesso/{tipoAccesso}")]
        public async Task<ActionResult<IEnumerable<LogAccessiDTO>>> GetByTipoAccesso(string tipoAccesso)
        {
            var result = await _repository.GetByTipoAccessoAsync(tipoAccesso);
            return Ok(result);
        }

        [HttpGet("esito/{esito}")]
        public async Task<ActionResult<IEnumerable<LogAccessiDTO>>> GetByEsito(string esito)
        {
            var result = await _repository.GetByEsitoAsync(esito);
            return Ok(result);
        }

        [HttpGet("periodo")]
        public async Task<ActionResult<IEnumerable<LogAccessiDTO>>> GetByPeriodo([FromQuery] DateTime dataInizio, [FromQuery] DateTime dataFine)
        {
            var result = await _repository.GetByPeriodoAsync(dataInizio, dataFine);
            return Ok(result);
        }

        [HttpGet("statistiche/numero-accessi")]
        public async Task<ActionResult<int>> GetNumeroAccessi([FromQuery] DateTime? dataInizio = null, [FromQuery] DateTime? dataFine = null)
        {
            var result = await _repository.GetNumeroAccessiAsync(dataInizio, dataFine);
            return Ok(result);
        }

        [HttpPost]
        public async Task<ActionResult<LogAccessiDTO>> Create(LogAccessiDTO logAccessiDto)
        {
            await _repository.AddAsync(logAccessiDto);
            return CreatedAtAction(nameof(GetById), new { logId = logAccessiDto.LogId }, logAccessiDto);
        }

        [HttpPut("{logId}")]
        public async Task<ActionResult> Update(int logId, LogAccessiDTO logAccessiDto)
        {
            await _repository.UpdateAsync(logAccessiDto);
            return NoContent();
        }

        [HttpDelete("{logId}")]
        public async Task<ActionResult> Delete(int logId)
        {
            await _repository.DeleteAsync(logId);
            return NoContent();
        }
    }
}