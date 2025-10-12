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
    public class DimensioneQuantitaIngredientiController : ControllerBase
    {
        private readonly IDimensioneQuantitaIngredientiRepository _repository;

        public DimensioneQuantitaIngredientiController(IDimensioneQuantitaIngredientiRepository repository)
        {
            _repository = repository;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<DimensioneQuantitaIngredientiDTO>>> GetAll()
        {
            var result = await _repository.GetAllAsync();
            return Ok(result);
        }

        [HttpGet("{dimensioneId}/{personalizzazioneIngredienteId}")]
        public async Task<ActionResult<DimensioneQuantitaIngredientiDTO>> GetById(int dimensioneId, int personalizzazioneIngredienteId)
        {
            var result = await _repository.GetByIdAsync(dimensioneId, personalizzazioneIngredienteId);

            if (result == null)
                return NotFound();

            return Ok(result);
        }

        [HttpGet("dimensione-bicchiere/{dimensioneBicchiereId}")]
        public async Task<ActionResult<IEnumerable<DimensioneQuantitaIngredientiDTO>>> GetByDimensioneBicchiere(int dimensioneBicchiereId)
        {
            var result = await _repository.GetByDimensioneBicchiereAsync(dimensioneBicchiereId);
            return Ok(result);
        }

        [HttpGet("personalizzazione-ingrediente/{personalizzazioneIngredienteId}")]
        public async Task<ActionResult<IEnumerable<DimensioneQuantitaIngredientiDTO>>> GetByPersonalizzazioneIngrediente(int personalizzazioneIngredienteId)
        {
            var result = await _repository.GetByPersonalizzazioneIngredienteAsync(personalizzazioneIngredienteId);
            return Ok(result);
        }

        [HttpGet("combinazione/{dimensioneBicchiereId}/{personalizzazioneIngredienteId}")]
        public async Task<ActionResult<DimensioneQuantitaIngredientiDTO>> GetByCombinazione(int dimensioneBicchiereId, int personalizzazioneIngredienteId)
        {
            var result = await _repository.GetByCombinazioneAsync(dimensioneBicchiereId, personalizzazioneIngredienteId);

            if (result == null)
                return NotFound();

            return Ok(result);
        }

        [HttpPost]
        public async Task<ActionResult<DimensioneQuantitaIngredientiDTO>> Create(DimensioneQuantitaIngredientiDTO dimensioneQuantitaDto)
        {
            await _repository.AddAsync(dimensioneQuantitaDto);
            return CreatedAtAction(nameof(GetById), new
            {
                dimensioneId = dimensioneQuantitaDto.DimensioneId,
                personalizzazioneIngredienteId = dimensioneQuantitaDto.PersonalizzazioneIngredienteId
            }, dimensioneQuantitaDto);
        }

        [HttpPut("{dimensioneId}/{personalizzazioneIngredienteId}")]
        public async Task<ActionResult> Update(int dimensioneId, int personalizzazioneIngredienteId, DimensioneQuantitaIngredientiDTO dimensioneQuantitaDto)
        {
            await _repository.UpdateAsync(dimensioneQuantitaDto);
            return NoContent();
        }

        [HttpDelete("{dimensioneId}/{personalizzazioneIngredienteId}")]
        public async Task<ActionResult> Delete(int dimensioneId, int personalizzazioneIngredienteId)
        {
            await _repository.DeleteAsync(dimensioneId, personalizzazioneIngredienteId);
            return NoContent();
        }
    }
}