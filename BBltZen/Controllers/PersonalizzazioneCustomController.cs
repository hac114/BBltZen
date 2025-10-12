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
    public class PersonalizzazioneCustomController : ControllerBase
    {
        private readonly IPersonalizzazioneCustomRepository _repository;

        public PersonalizzazioneCustomController(IPersonalizzazioneCustomRepository repository)
        {
            _repository = repository;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<PersonalizzazioneCustomDTO>>> GetAll()
        {
            var result = await _repository.GetAllAsync();
            return Ok(result);
        }

        [HttpGet("{persCustomId}")]
        public async Task<ActionResult<PersonalizzazioneCustomDTO>> GetById(int persCustomId)
        {
            var result = await _repository.GetByIdAsync(persCustomId);

            if (result == null)
                return NotFound();

            return Ok(result);
        }

        [HttpGet("dimensione-bicchiere/{dimensioneBicchiereId}")]
        public async Task<ActionResult<IEnumerable<PersonalizzazioneCustomDTO>>> GetByDimensioneBicchiere(int dimensioneBicchiereId)
        {
            var result = await _repository.GetByDimensioneBicchiereAsync(dimensioneBicchiereId);
            return Ok(result);
        }

        [HttpGet("grado-dolcezza/{gradoDolcezza}")]
        public async Task<ActionResult<IEnumerable<PersonalizzazioneCustomDTO>>> GetByGradoDolcezza(byte gradoDolcezza)
        {
            var result = await _repository.GetByGradoDolcezzaAsync(gradoDolcezza);
            return Ok(result);
        }

        [HttpPost]
        public async Task<ActionResult<PersonalizzazioneCustomDTO>> Create(PersonalizzazioneCustomDTO personalizzazioneCustomDto)
        {
            await _repository.AddAsync(personalizzazioneCustomDto);
            return CreatedAtAction(nameof(GetById), new { persCustomId = personalizzazioneCustomDto.PersCustomId }, personalizzazioneCustomDto);
        }

        [HttpPut("{persCustomId}")]
        public async Task<ActionResult> Update(int persCustomId, PersonalizzazioneCustomDTO personalizzazioneCustomDto)
        {
            await _repository.UpdateAsync(personalizzazioneCustomDto);
            return NoContent();
        }

        [HttpDelete("{persCustomId}")]
        public async Task<ActionResult> Delete(int persCustomId)
        {
            await _repository.DeleteAsync(persCustomId);
            return NoContent();
        }
    }
}
