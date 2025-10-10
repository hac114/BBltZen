using Microsoft.AspNetCore.Mvc;
using DTO;
using Repository.Interface;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BBltZen.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UnitaDiMisuraController : ControllerBase
    {
        private readonly IUnitaDiMisuraRepository _repository;

        public UnitaDiMisuraController(IUnitaDiMisuraRepository repository)
        {
            _repository = repository;
        }

        // GET: api/UnitaDiMisura
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UnitaDiMisuraDTO>>> GetAll()
        {
            var unita = await _repository.GetAllAsync();
            return Ok(unita);
        }

        // GET: api/UnitaDiMisura/5
        [HttpGet("{id}")]
        public async Task<ActionResult<UnitaDiMisuraDTO>> GetById(int id)
        {
            var unita = await _repository.GetByIdAsync(id);

            if (unita == null)
            {
                return NotFound();
            }

            return Ok(unita);
        }

        // POST: api/UnitaDiMisura
        [HttpPost]
        public async Task<ActionResult<UnitaDiMisuraDTO>> Create(UnitaDiMisuraDTO unitaDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            await _repository.AddAsync(unitaDto);

            return CreatedAtAction(nameof(GetById),
                new { id = unitaDto.UnitaMisuraId },
                unitaDto);
        }

        // PUT: api/UnitaDiMisura/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, UnitaDiMisuraDTO unitaDto)
        {
            if (id != unitaDto.UnitaMisuraId)
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

            await _repository.UpdateAsync(unitaDto);

            return NoContent();
        }

        // DELETE: api/UnitaDiMisura/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var unita = await _repository.GetByIdAsync(id);
            if (unita == null)
            {
                return NotFound();
            }

            await _repository.DeleteAsync(id);

            return NoContent();
        }
    }
}
