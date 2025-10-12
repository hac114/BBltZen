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
    public class BevandaCustomController : ControllerBase
    {
        private readonly IBevandaCustomRepository _repository;

        public BevandaCustomController(IBevandaCustomRepository repository)
        {
            _repository = repository;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<BevandaCustomDTO>>> GetAll()
        {
            var result = await _repository.GetAllAsync();
            return Ok(result);
        }

        [HttpGet("{bevandaCustomId}")]
        public async Task<ActionResult<BevandaCustomDTO>> GetById(int bevandaCustomId)
        {
            var result = await _repository.GetByIdAsync(bevandaCustomId);

            if (result == null)
                return NotFound();

            return Ok(result);
        }

        [HttpGet("articolo/{articoloId}")]
        public async Task<ActionResult<BevandaCustomDTO>> GetByArticoloId(int articoloId)
        {
            var result = await _repository.GetByArticoloIdAsync(articoloId);

            if (result == null)
                return NotFound();

            return Ok(result);
        }

        [HttpGet("personalizzazione-custom/{persCustomId}")]
        public async Task<ActionResult<IEnumerable<BevandaCustomDTO>>> GetByPersCustomId(int persCustomId)
        {
            var result = await _repository.GetByPersCustomIdAsync(persCustomId);
            return Ok(result);
        }

        [HttpPost]
        public async Task<ActionResult<BevandaCustomDTO>> Create(BevandaCustomDTO bevandaCustomDto)
        {
            await _repository.AddAsync(bevandaCustomDto);
            return CreatedAtAction(nameof(GetById), new { bevandaCustomId = bevandaCustomDto.BevandaCustomId }, bevandaCustomDto);
        }

        [HttpPut("{bevandaCustomId}")]
        public async Task<ActionResult> Update(int bevandaCustomId, BevandaCustomDTO bevandaCustomDto)
        {
            await _repository.UpdateAsync(bevandaCustomDto);
            return NoContent();
        }

        [HttpDelete("{bevandaCustomId}")]
        public async Task<ActionResult> Delete(int bevandaCustomId)
        {
            await _repository.DeleteAsync(bevandaCustomId);
            return NoContent();
        }
    }
}
