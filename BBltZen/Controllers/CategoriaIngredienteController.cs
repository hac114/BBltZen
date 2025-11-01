using Microsoft.AspNetCore.Mvc;
using DTO;
using Repository.Interface;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace BBltZen.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [AllowAnonymous] // ✅ OVERRIDE DELL'[Authorize] DEL BASE CONTROLLER
    public class CategoriaIngredienteController : SecureBaseController
    {
        private readonly ICategoriaIngredienteRepository _repository;

        public CategoriaIngredienteController(
            ICategoriaIngredienteRepository repository,
            IWebHostEnvironment environment,
            ILogger<CategoriaIngredienteController> logger)
            : base(environment, logger)
        {
            _repository = repository;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<CategoriaIngredienteDTO>>> GetAll()
        {
            try
            {
                var categorie = await _repository.GetAllAsync();
                return Ok(categorie);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore durante il recupero di tutte le categorie ingredienti");
                return SafeInternalError("Errore durante il recupero delle categorie");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<CategoriaIngredienteDTO>> GetById(int id)
        {
            try
            {
                if (id <= 0)
                    return SafeBadRequest<CategoriaIngredienteDTO>("ID categoria non valido");

                var categoria = await _repository.GetByIdAsync(id);

                if (categoria == null)
                    return SafeNotFound<CategoriaIngredienteDTO>("Categoria ingrediente");

                return Ok(categoria);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore durante il recupero della categoria ingrediente {Id}", id);
                return SafeInternalError("Errore durante il recupero della categoria");
            }
        }

        [HttpPost]
        [Authorize(Roles = "admin")] // ✅ Solo admin può creare categorie
        public async Task<ActionResult<CategoriaIngredienteDTO>> Create(CategoriaIngredienteDTO categoriaDto)
        {
            try
            {
                if (!IsModelValid(categoriaDto))
                    return SafeBadRequest<CategoriaIngredienteDTO>("Dati categoria non validi");

                await _repository.AddAsync(categoriaDto);

                // ✅ Audit trail
                LogAuditTrail("CREATE_CATEGORIA_INGREDIENTE", "CategoriaIngrediente", categoriaDto.CategoriaId.ToString());
                LogSecurityEvent("CategoriaIngredienteCreated", new
                {
                    CategoriaId = categoriaDto.CategoriaId,
                    Categoria = categoriaDto.Categoria
                });

                return CreatedAtAction(nameof(GetById), new { id = categoriaDto.CategoriaId }, categoriaDto);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore durante la creazione della categoria ingrediente");
                return SafeInternalError("Errore durante la creazione della categoria");
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "admin")] // ✅ Solo admin può modificare categorie
        public async Task<ActionResult> Update(int id, CategoriaIngredienteDTO categoriaDto)
        {
            try
            {
                if (id <= 0)
                    return SafeBadRequest("ID categoria non valido");

                if (id != categoriaDto.CategoriaId)
                    return SafeBadRequest("ID categoria non corrispondente");

                if (!IsModelValid(categoriaDto))
                    return SafeBadRequest("Dati categoria non validi");

                var existing = await _repository.GetByIdAsync(id);
                if (existing == null)
                    return SafeNotFound("Categoria ingrediente");

                await _repository.UpdateAsync(categoriaDto);

                // ✅ Audit trail
                LogAuditTrail("UPDATE_CATEGORIA_INGREDIENTE", "CategoriaIngrediente", categoriaDto.CategoriaId.ToString());
                LogSecurityEvent("CategoriaIngredienteUpdated", new
                {
                    CategoriaId = categoriaDto.CategoriaId,
                    User = User.Identity?.Name
                });

                return NoContent();
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore durante l'aggiornamento della categoria ingrediente {Id}", id);
                return SafeInternalError("Errore durante l'aggiornamento della categoria");
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "admin")] // ✅ Solo admin può cancellare categorie
        public async Task<ActionResult> Delete(int id)
        {
            try
            {
                if (id <= 0)
                    return SafeBadRequest("ID categoria non valido");

                var categoria = await _repository.GetByIdAsync(id);
                if (categoria == null)
                    return SafeNotFound("Categoria ingrediente");

                await _repository.DeleteAsync(id);

                // ✅ Audit trail
                LogAuditTrail("DELETE_CATEGORIA_INGREDIENTE", "CategoriaIngrediente", id.ToString());
                LogSecurityEvent("CategoriaIngredienteDeleted", new
                {
                    CategoriaId = id,
                    User = User.Identity?.Name
                });

                return NoContent();
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore durante l'eliminazione della categoria ingrediente {Id}", id);
                return SafeInternalError("Errore durante l'eliminazione della categoria");
            }
        }
    }
}