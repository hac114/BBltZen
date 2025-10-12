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
    public class LogAttivitaController : ControllerBase
    {
        private readonly ILogAttivitaRepository _repository;

        public LogAttivitaController(ILogAttivitaRepository repository)
        {
            _repository = repository;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<LogAttivitaDTO>>> GetAll()
        {
            var result = await _repository.GetAllAsync();
            return Ok(result);
        }

        [HttpGet("{logId}")]
        public async Task<ActionResult<LogAttivitaDTO>> GetById(int logId)
        {
            var result = await _repository.GetByIdAsync(logId);

            if (result == null)
                return NotFound();

            return Ok(result);
        }

        [HttpGet("tipo-attivita/{tipoAttivita}")]
        public async Task<ActionResult<IEnumerable<LogAttivitaDTO>>> GetByTipoAttivita(string tipoAttivita)
        {
            var result = await _repository.GetByTipoAttivitaAsync(tipoAttivita);
            return Ok(result);
        }

        [HttpGet("periodo")]
        public async Task<ActionResult<IEnumerable<LogAttivitaDTO>>> GetByPeriodo([FromQuery] DateTime dataInizio, [FromQuery] DateTime dataFine)
        {
            var result = await _repository.GetByPeriodoAsync(dataInizio, dataFine);
            return Ok(result);
        }

        [HttpGet("statistiche/numero-attivita")]
        public async Task<ActionResult<int>> GetNumeroAttivita([FromQuery] DateTime? dataInizio = null, [FromQuery] DateTime? dataFine = null)
        {
            var result = await _repository.GetNumeroAttivitaAsync(dataInizio, dataFine);
            return Ok(result);
        }

        [HttpPost]
        public async Task<ActionResult<LogAttivitaDTO>> Create(LogAttivitaDTO logAttivitaDto)
        {
            await _repository.AddAsync(logAttivitaDto);
            return CreatedAtAction(nameof(GetById), new { logId = logAttivitaDto.LogId }, logAttivitaDto);
        }

        [HttpPut("{logId}")]
        public async Task<ActionResult> Update(int logId, LogAttivitaDTO logAttivitaDto)
        {
            await _repository.UpdateAsync(logAttivitaDto);
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