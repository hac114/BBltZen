using Microsoft.AspNetCore.Mvc;
using DTO;
using Repository.Interface;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BBltZen.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DimensioneBicchiereController : ControllerBase
    {
        private readonly IDimensioneBicchiereRepository _repository;

        public DimensioneBicchiereController(IDimensioneBicchiereRepository repository)
        {
            _repository = repository;
        }

        // GET: api/DimensioneBicchiere
        [HttpGet]
        public async Task<ActionResult<IEnumerable<DimensioneBicchiereDTO>>> GetAll()
        {
            var dimensioni = await _repository.GetAllAsync();
            return Ok(dimensioni);
        }

        // GET: api/DimensioneBicchiere/5
        [HttpGet("{id}")]
        public async Task<ActionResult<DimensioneBicchiereDTO>> GetById(int id)
        {
            var dimensione = await _repository.GetByIdAsync(id);

            if (dimensione == null)
            {
                return NotFound();
            }

            return Ok(dimensione);
        }

        // POST: api/DimensioneBicchiere
        [HttpPost]
        public async Task<ActionResult<DimensioneBicchiereDTO>> Create(DimensioneBicchiereDTO dimensioneDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            await _repository.AddAsync(dimensioneDto);

            return CreatedAtAction(nameof(GetById),
                new { id = dimensioneDto.DimensioneBicchiereId },
                dimensioneDto);
        }

        // PUT: api/DimensioneBicchiere/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, DimensioneBicchiereDTO dimensioneDto)
        {
            if (id != dimensioneDto.DimensioneBicchiereId)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var existing = await _repository.GetByIdAsync(id);
            if (existing == null)
            {
                return NotFound();
            }

            await _repository.UpdateAsync(dimensioneDto);

            return NoContent();
        }

        // DELETE: api/DimensioneBicchiere/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var dimensione = await _repository.GetByIdAsync(id);
            if (dimensione == null)
            {
                return NotFound();
            }

            await _repository.DeleteAsync(id);

            return NoContent();
        }
    }
}