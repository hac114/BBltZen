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
    public class StatoStoricoOrdineController : ControllerBase
    {
        private readonly IStatoStoricoOrdineRepository _repository;

        public StatoStoricoOrdineController(IStatoStoricoOrdineRepository repository)
        {
            _repository = repository;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<StatoStoricoOrdineDTO>>> GetAll()
        {
            var result = await _repository.GetAllAsync();
            return Ok(result);
        }

        [HttpGet("{statoStoricoOrdineId}")]
        public async Task<ActionResult<StatoStoricoOrdineDTO>> GetById(int statoStoricoOrdineId)
        {
            var result = await _repository.GetByIdAsync(statoStoricoOrdineId);

            if (result == null)
                return NotFound();

            return Ok(result);
        }

        [HttpGet("ordine/{ordineId}")]
        public async Task<ActionResult<IEnumerable<StatoStoricoOrdineDTO>>> GetByOrdineId(int ordineId)
        {
            var result = await _repository.GetByOrdineIdAsync(ordineId);
            return Ok(result);
        }

        [HttpGet("stato-ordine/{statoOrdineId}")]
        public async Task<ActionResult<IEnumerable<StatoStoricoOrdineDTO>>> GetByStatoOrdineId(int statoOrdineId)
        {
            var result = await _repository.GetByStatoOrdineIdAsync(statoOrdineId);
            return Ok(result);
        }

        [HttpGet("ordine/{ordineId}/storico-completo")]
        public async Task<ActionResult<IEnumerable<StatoStoricoOrdineDTO>>> GetStoricoCompletoOrdine(int ordineId)
        {
            var result = await _repository.GetStoricoCompletoOrdineAsync(ordineId);
            return Ok(result);
        }

        [HttpGet("ordine/{ordineId}/stato-attuale")]
        public async Task<ActionResult<StatoStoricoOrdineDTO>> GetStatoAttualeOrdine(int ordineId)
        {
            var result = await _repository.GetStatoAttualeOrdineAsync(ordineId);

            if (result == null)
                return NotFound();

            return Ok(result);
        }

        [HttpPost]
        public async Task<ActionResult<StatoStoricoOrdineDTO>> Create(StatoStoricoOrdineDTO statoStoricoOrdineDto)
        {
            await _repository.AddAsync(statoStoricoOrdineDto);
            return CreatedAtAction(nameof(GetById), new { statoStoricoOrdineId = statoStoricoOrdineDto.StatoStoricoOrdineId }, statoStoricoOrdineDto);
        }

        [HttpPut("{statoStoricoOrdineId}")]
        public async Task<ActionResult> Update(int statoStoricoOrdineId, StatoStoricoOrdineDTO statoStoricoOrdineDto)
        {
            await _repository.UpdateAsync(statoStoricoOrdineDto);
            return NoContent();
        }

        [HttpDelete("{statoStoricoOrdineId}")]
        public async Task<ActionResult> Delete(int statoStoricoOrdineId)
        {
            await _repository.DeleteAsync(statoStoricoOrdineId);
            return NoContent();
        }

        [HttpPost("ordine/{ordineId}/chiudi-stato-attuale")]
        public async Task<ActionResult> ChiudiStatoAttuale(int ordineId)
        {
            var result = await _repository.ChiudiStatoAttualeAsync(ordineId, DateTime.Now);

            if (!result)
                return NotFound("Nessuno stato attuale trovato per l'ordine specificato");

            return NoContent();
        }
    }
}