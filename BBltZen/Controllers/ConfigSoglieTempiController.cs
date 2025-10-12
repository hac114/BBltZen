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
    public class ConfigSoglieTempiController : ControllerBase
    {
        private readonly IConfigSoglieTempiRepository _repository;

        public ConfigSoglieTempiController(IConfigSoglieTempiRepository repository)
        {
            _repository = repository;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ConfigSoglieTempiDTO>>> GetAll()
        {
            var result = await _repository.GetAllAsync();
            return Ok(result);
        }

        [HttpGet("{sogliaId}")]
        public async Task<ActionResult<ConfigSoglieTempiDTO>> GetById(int sogliaId)
        {
            var result = await _repository.GetByIdAsync(sogliaId);

            if (result == null)
                return NotFound();

            return Ok(result);
        }

        [HttpGet("stato-ordine/{statoOrdineId}")]
        public async Task<ActionResult<ConfigSoglieTempiDTO>> GetByStatoOrdineId(int statoOrdineId)
        {
            var result = await _repository.GetByStatoOrdineIdAsync(statoOrdineId);

            if (result == null)
                return NotFound();

            return Ok(result);
        }

        [HttpPost]
        public async Task<ActionResult<ConfigSoglieTempiDTO>> Create(ConfigSoglieTempiDTO configSoglieTempiDto)
        {
            // Verifica se esiste già una configurazione per questo stato ordine
            if (await _repository.ExistsByStatoOrdineIdAsync(configSoglieTempiDto.StatoOrdineId))
            {
                return BadRequest("Esiste già una configurazione per questo stato ordine.");
            }

            await _repository.AddAsync(configSoglieTempiDto);
            return CreatedAtAction(nameof(GetById), new { sogliaId = configSoglieTempiDto.SogliaId }, configSoglieTempiDto);
        }

        [HttpPut("{sogliaId}")]
        public async Task<ActionResult> Update(int sogliaId, ConfigSoglieTempiDTO configSoglieTempiDto)
        {
            // Verifica se esiste già un'altra configurazione per questo stato ordine
            if (await _repository.ExistsByStatoOrdineIdAsync(configSoglieTempiDto.StatoOrdineId, sogliaId))
            {
                return BadRequest("Esiste già un'altra configurazione per questo stato ordine.");
            }

            await _repository.UpdateAsync(configSoglieTempiDto);
            return NoContent();
        }

        [HttpDelete("{sogliaId}")]
        public async Task<ActionResult> Delete(int sogliaId)
        {
            await _repository.DeleteAsync(sogliaId);
            return NoContent();
        }
    }
}