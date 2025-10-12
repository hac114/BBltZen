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
    public class ArticoloController : ControllerBase
    {
        private readonly IArticoloRepository _repository;

        public ArticoloController(IArticoloRepository repository)
        {
            _repository = repository;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ArticoloDTO>>> GetAll()
        {
            var result = await _repository.GetAllAsync();
            return Ok(result);
        }

        [HttpGet("{articoloId}")]
        public async Task<ActionResult<ArticoloDTO>> GetById(int articoloId)
        {
            var result = await _repository.GetByIdAsync(articoloId);

            if (result == null)
                return NotFound();

            return Ok(result);
        }

        [HttpGet("tipo/{tipo}")]
        public async Task<ActionResult<IEnumerable<ArticoloDTO>>> GetByTipo(string tipo)
        {
            var result = await _repository.GetByTipoAsync(tipo);
            return Ok(result);
        }

        [HttpGet("ordinabili")]
        public async Task<ActionResult<IEnumerable<ArticoloDTO>>> GetArticoliOrdinabili()
        {
            var result = await _repository.GetArticoliOrdinabiliAsync();
            return Ok(result);
        }

        [HttpPost]
        public async Task<ActionResult<ArticoloDTO>> Create(ArticoloDTO articoloDto)
        {
            await _repository.AddAsync(articoloDto);
            return CreatedAtAction(nameof(GetById), new { articoloId = articoloDto.ArticoloId }, articoloDto);
        }

        [HttpPut("{articoloId}")]
        public async Task<ActionResult> Update(int articoloId, ArticoloDTO articoloDto)
        {
            await _repository.UpdateAsync(articoloDto);
            return NoContent();
        }

        [HttpDelete("{articoloId}")]
        public async Task<ActionResult> Delete(int articoloId)
        {
            await _repository.DeleteAsync(articoloId);
            return NoContent();
        }
    }
}
