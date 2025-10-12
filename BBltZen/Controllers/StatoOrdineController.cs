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
    public class StatoOrdineController : ControllerBase
    {
        private readonly IStatoOrdineRepository _repository;

        public StatoOrdineController(IStatoOrdineRepository repository)
        {
            _repository = repository;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<StatoOrdineDTO>>> GetAll()
        {
            var result = await _repository.GetAllAsync();
            return Ok(result);
        }

        [HttpGet("{statoOrdineId}")]
        public async Task<ActionResult<StatoOrdineDTO>> GetById(int statoOrdineId)
        {
            var result = await _repository.GetByIdAsync(statoOrdineId);

            if (result == null)
                return NotFound();

            return Ok(result);
        }

        [HttpGet("nome/{nomeStatoOrdine}")]
        public async Task<ActionResult<StatoOrdineDTO>> GetByNome(string nomeStatoOrdine)
        {
            var result = await _repository.GetByNomeAsync(nomeStatoOrdine);

            if (result == null)
                return NotFound();

            return Ok(result);
        }

        [HttpGet("terminali")]
        public async Task<ActionResult<IEnumerable<StatoOrdineDTO>>> GetStatiTerminali()
        {
            var result = await _repository.GetStatiTerminaliAsync();
            return Ok(result);
        }

        [HttpGet("non-terminali")]
        public async Task<ActionResult<IEnumerable<StatoOrdineDTO>>> GetStatiNonTerminali()
        {
            var result = await _repository.GetStatiNonTerminaliAsync();
            return Ok(result);
        }

        [HttpPost]
        public async Task<ActionResult<StatoOrdineDTO>> Create(StatoOrdineDTO statoOrdineDto)
        {
            await _repository.AddAsync(statoOrdineDto);
            return CreatedAtAction(nameof(GetById), new { statoOrdineId = statoOrdineDto.StatoOrdineId }, statoOrdineDto);
        }

        [HttpPut("{statoOrdineId}")]
        public async Task<ActionResult> Update(int statoOrdineId, StatoOrdineDTO statoOrdineDto)
        {
            await _repository.UpdateAsync(statoOrdineDto);
            return NoContent();
        }

        [HttpDelete("{statoOrdineId}")]
        public async Task<ActionResult> Delete(int statoOrdineId)
        {
            await _repository.DeleteAsync(statoOrdineId);
            return NoContent();
        }
    }
}