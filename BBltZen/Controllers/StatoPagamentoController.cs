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
    public class StatoPagamentoController : ControllerBase
    {
        private readonly IStatoPagamentoRepository _repository;

        public StatoPagamentoController(IStatoPagamentoRepository repository)
        {
            _repository = repository;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<StatoPagamentoDTO>>> GetAll()
        {
            var result = await _repository.GetAllAsync();
            return Ok(result);
        }

        [HttpGet("{statoPagamentoId}")]
        public async Task<ActionResult<StatoPagamentoDTO>> GetById(int statoPagamentoId)
        {
            var result = await _repository.GetByIdAsync(statoPagamentoId);

            if (result == null)
                return NotFound();

            return Ok(result);
        }

        [HttpGet("nome/{nomeStatoPagamento}")]
        public async Task<ActionResult<StatoPagamentoDTO>> GetByNome(string nomeStatoPagamento)
        {
            var result = await _repository.GetByNomeAsync(nomeStatoPagamento);

            if (result == null)
                return NotFound();

            return Ok(result);
        }

        [HttpPost]
        public async Task<ActionResult<StatoPagamentoDTO>> Create(StatoPagamentoDTO statoPagamentoDto)
        {
            await _repository.AddAsync(statoPagamentoDto);
            return CreatedAtAction(nameof(GetById), new { statoPagamentoId = statoPagamentoDto.StatoPagamentoId }, statoPagamentoDto);
        }

        [HttpPut("{statoPagamentoId}")]
        public async Task<ActionResult> Update(int statoPagamentoId, StatoPagamentoDTO statoPagamentoDto)
        {
            await _repository.UpdateAsync(statoPagamentoDto);
            return NoContent();
        }

        [HttpDelete("{statoPagamentoId}")]
        public async Task<ActionResult> Delete(int statoPagamentoId)
        {
            await _repository.DeleteAsync(statoPagamentoId);
            return NoContent();
        }
    }
}
