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
    public class StatisticheCacheController : ControllerBase
    {
        private readonly IStatisticheCacheRepository _repository;

        public StatisticheCacheController(IStatisticheCacheRepository repository)
        {
            _repository = repository;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<StatisticheCacheDTO>>> GetAll()
        {
            var result = await _repository.GetAllAsync();
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<StatisticheCacheDTO>> GetById(int id)
        {
            var result = await _repository.GetByIdAsync(id);

            if (result == null)
                return NotFound();

            return Ok(result);
        }

        [HttpGet("tipo/{tipoStatistica}")]
        public async Task<ActionResult<IEnumerable<StatisticheCacheDTO>>> GetByTipo(string tipoStatistica)
        {
            var result = await _repository.GetByTipoAsync(tipoStatistica);
            return Ok(result);
        }

        [HttpGet("tipo/{tipoStatistica}/periodo/{periodo}")]
        public async Task<ActionResult<StatisticheCacheDTO>> GetByTipoAndPeriodo(string tipoStatistica, string periodo)
        {
            var result = await _repository.GetByTipoAndPeriodoAsync(tipoStatistica, periodo);

            if (result == null)
                return NotFound();

            return Ok(result);
        }

        [HttpPost]
        public async Task<ActionResult<StatisticheCacheDTO>> Create(StatisticheCacheDTO statisticheCacheDto)
        {
            await _repository.AddAsync(statisticheCacheDto);
            return CreatedAtAction(nameof(GetById), new { id = statisticheCacheDto.Id }, statisticheCacheDto);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> Update(int id, StatisticheCacheDTO statisticheCacheDto)
        {
            await _repository.UpdateAsync(statisticheCacheDto);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            await _repository.DeleteAsync(id);
            return NoContent();
        }

        [HttpPost("aggiorna")]
        public async Task<ActionResult> AggiornaCache([FromBody] AggiornaCacheRequest request)
        {
            await _repository.AggiornaCacheAsync(request.TipoStatistica, request.Periodo, request.Metriche);
            return NoContent();
        }

        [HttpGet("valida/tipo/{tipoStatistica}/periodo/{periodo}")]
        public async Task<ActionResult<bool>> IsCacheValida(string tipoStatistica, string periodo, [FromQuery] int oreValidita = 24)
        {
            var validita = TimeSpan.FromHours(oreValidita);
            var result = await _repository.IsCacheValidaAsync(tipoStatistica, periodo, validita);
            return Ok(result);
        }
    }

    public class AggiornaCacheRequest
    {
        public string TipoStatistica { get; set; } = null!;
        public string Periodo { get; set; } = null!;
        public string Metriche { get; set; } = null!;
    }
}