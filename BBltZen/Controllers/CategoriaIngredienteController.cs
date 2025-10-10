using Microsoft.AspNetCore.Mvc;
using DTO;
using Repository.Interface;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BBltZen.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoriaIngredienteController : ControllerBase
    {
        private readonly ICategoriaIngredienteRepository _repository;

        public CategoriaIngredienteController(ICategoriaIngredienteRepository repository)
        {
            _repository = repository;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<CategoriaIngredienteDTO>>> GetAll()
        {
            return Ok(await _repository.GetAllAsync());
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<CategoriaIngredienteDTO>> GetById(int id)
        {
            var categoria = await _repository.GetByIdAsync(id);
            return categoria == null ? NotFound() : Ok(categoria);
        }

        [HttpPost]
        public async Task<ActionResult<CategoriaIngredienteDTO>> Create(CategoriaIngredienteDTO categoriaDto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            await _repository.AddAsync(categoriaDto);
            return CreatedAtAction(nameof(GetById), new { id = categoriaDto.CategoriaId }, categoriaDto);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, CategoriaIngredienteDTO categoriaDto)
        {
            if (id != categoriaDto.CategoriaId) return BadRequest();
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var existing = await _repository.GetByIdAsync(id);
            if (existing == null) return NotFound();

            await _repository.UpdateAsync(categoriaDto);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var categoria = await _repository.GetByIdAsync(id);
            if (categoria == null) return NotFound();

            await _repository.DeleteAsync(id);
            return NoContent();
        }
    }
}
