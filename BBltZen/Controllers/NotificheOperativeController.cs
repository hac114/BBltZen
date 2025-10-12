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
    public class NotificheOperativeController : ControllerBase
    {
        private readonly INotificheOperativeRepository _repository;

        public NotificheOperativeController(INotificheOperativeRepository repository)
        {
            _repository = repository;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<NotificheOperativeDTO>>> GetAll()
        {
            var result = await _repository.GetAllAsync();
            return Ok(result);
        }

        [HttpGet("{notificaId}")]
        public async Task<ActionResult<NotificheOperativeDTO>> GetById(int notificaId)
        {
            var result = await _repository.GetByIdAsync(notificaId);

            if (result == null)
                return NotFound();

            return Ok(result);
        }

        [HttpGet("stato/{stato}")]
        public async Task<ActionResult<IEnumerable<NotificheOperativeDTO>>> GetByStato(string stato)
        {
            var result = await _repository.GetByStatoAsync(stato);
            return Ok(result);
        }

        [HttpGet("priorita/{priorita}")]
        public async Task<ActionResult<IEnumerable<NotificheOperativeDTO>>> GetByPriorita(int priorita)
        {
            var result = await _repository.GetByPrioritaAsync(priorita);
            return Ok(result);
        }

        [HttpGet("pendenti")]
        public async Task<ActionResult<IEnumerable<NotificheOperativeDTO>>> GetPendenti()
        {
            var result = await _repository.GetPendentiAsync();
            return Ok(result);
        }

        [HttpGet("periodo")]
        public async Task<ActionResult<IEnumerable<NotificheOperativeDTO>>> GetByPeriodo([FromQuery] DateTime dataInizio, [FromQuery] DateTime dataFine)
        {
            var result = await _repository.GetByPeriodoAsync(dataInizio, dataFine);
            return Ok(result);
        }

        [HttpGet("pendenti/numero")]
        public async Task<ActionResult<int>> GetNumeroPendenti()
        {
            var result = await _repository.GetNumeroNotifichePendentiAsync();
            return Ok(result);
        }

        [HttpPost]
        public async Task<ActionResult<NotificheOperativeDTO>> Create(NotificheOperativeDTO notificaDto)
        {
            await _repository.AddAsync(notificaDto);
            return CreatedAtAction(nameof(GetById), new { notificaId = notificaDto.NotificaId }, notificaDto);
        }

        [HttpPut("{notificaId}")]
        public async Task<ActionResult> Update(int notificaId, NotificheOperativeDTO notificaDto)
        {
            await _repository.UpdateAsync(notificaDto);
            return NoContent();
        }

        [HttpDelete("{notificaId}")]
        public async Task<ActionResult> Delete(int notificaId)
        {
            await _repository.DeleteAsync(notificaId);
            return NoContent();
        }
    }
}