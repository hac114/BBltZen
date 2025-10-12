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
    public class OrdineController : ControllerBase
    {
        private readonly IOrdineRepository _repository;

        public OrdineController(IOrdineRepository repository)
        {
            _repository = repository;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<OrdineDTO>>> GetAll()
        {
            var result = await _repository.GetAllAsync();
            return Ok(result);
        }

        [HttpGet("{ordineId}")]
        public async Task<ActionResult<OrdineDTO>> GetById(int ordineId)
        {
            var result = await _repository.GetByIdAsync(ordineId);

            if (result == null)
                return NotFound();

            return Ok(result);
        }

        [HttpGet("cliente/{clienteId}")]
        public async Task<ActionResult<IEnumerable<OrdineDTO>>> GetByClienteId(int clienteId)
        {
            var result = await _repository.GetByClienteIdAsync(clienteId);
            return Ok(result);
        }

        [HttpGet("stato-ordine/{statoOrdineId}")]
        public async Task<ActionResult<IEnumerable<OrdineDTO>>> GetByStatoOrdineId(int statoOrdineId)
        {
            var result = await _repository.GetByStatoOrdineIdAsync(statoOrdineId);
            return Ok(result);
        }

        [HttpGet("stato-pagamento/{statoPagamentoId}")]
        public async Task<ActionResult<IEnumerable<OrdineDTO>>> GetByStatoPagamentoId(int statoPagamentoId)
        {
            var result = await _repository.GetByStatoPagamentoIdAsync(statoPagamentoId);
            return Ok(result);
        }

        [HttpPost]
        public async Task<ActionResult<OrdineDTO>> Create(OrdineDTO ordineDto)
        {
            await _repository.AddAsync(ordineDto);
            return CreatedAtAction(nameof(GetById), new { ordineId = ordineDto.OrdineId }, ordineDto);
        }

        [HttpPut("{ordineId}")]
        public async Task<ActionResult> Update(int ordineId, OrdineDTO ordineDto)
        {
            await _repository.UpdateAsync(ordineDto);
            return NoContent();
        }

        [HttpDelete("{ordineId}")]
        public async Task<ActionResult> Delete(int ordineId)
        {
            await _repository.DeleteAsync(ordineId);
            return NoContent();
        }
    }
}