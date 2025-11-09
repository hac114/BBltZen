using Microsoft.AspNetCore.Mvc;
using DTO;
using Repository.Interface;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace BBltZen.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [AllowAnonymous] // ✅ OVERRIDE DELL'[Authorize] DEL BASE CONTROLLER
    public class CategoriaIngredienteController : SecureBaseController
    {
        private readonly ICategoriaIngredienteRepository _repository;
        private readonly Database.BubbleTeaContext _context;

        public CategoriaIngredienteController(
            ICategoriaIngredienteRepository repository,
            Database.BubbleTeaContext context,
            IWebHostEnvironment environment,
            ILogger<CategoriaIngredienteController> logger)
            : base(environment, logger)
        {
            _repository = repository;
            _context = context;
        }

        [HttpGet]
        [AllowAnonymous] // ✅ ESPLICITO PER ENDPOINT GET
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
                return SafeInternalError<IEnumerable<CategoriaIngredienteDTO>>("Errore durante il recupero delle categorie");
            }
        }

        [HttpGet("{id}")]
        [AllowAnonymous] // ✅ ESPLICITO PER ENDPOINT GET
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
                return SafeInternalError<CategoriaIngredienteDTO>("Errore durante il recupero della categoria");
            }
        }

        [HttpPost]
        // [Authorize(Roles = "admin")] // ✅ COMMENTATO PER TEST
        public async Task<ActionResult<CategoriaIngredienteDTO>> Create([FromBody] CategoriaIngredienteDTO categoriaDto)
        {
            try
            {
                if (!IsModelValid(categoriaDto))
                    return SafeBadRequest<CategoriaIngredienteDTO>("Dati categoria non validi");

                // ✅ CONTROLLO DUPLICATI
                var categoriaEsistente = await _context.CategoriaIngrediente
                    .FirstOrDefaultAsync(c => c.Categoria.ToLower() == categoriaDto.Categoria.ToLower());

                if (categoriaEsistente != null)
                    return SafeBadRequest<CategoriaIngredienteDTO>("Categoria già esistente");

                await _repository.AddAsync(categoriaDto);

                // ✅ AUDIT TRAIL COMPLETO
                LogAuditTrail("CREATE_CATEGORIA_INGREDIENTE", "CategoriaIngrediente", categoriaDto.CategoriaId.ToString());

                // ✅ SECURITY EVENT COMPLETO CON TIMESTAMP
                LogSecurityEvent("CategoriaIngredienteCreated", new
                {
                    CategoriaId = categoriaDto.CategoriaId,
                    Categoria = categoriaDto.Categoria,
                    User = User.Identity?.Name,
                    Timestamp = DateTime.UtcNow
                });

                return CreatedAtAction(nameof(GetById), new { id = categoriaDto.CategoriaId }, categoriaDto);
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "Errore database durante la creazione della categoria ingrediente");
                return SafeInternalError<CategoriaIngredienteDTO>("Errore durante il salvataggio della categoria");
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore durante la creazione della categoria ingrediente");
                return SafeInternalError<CategoriaIngredienteDTO>("Errore durante la creazione della categoria");
            }
        }

        [HttpPut("{id}")]
        // [Authorize(Roles = "admin")] // ✅ COMMENTATO PER TEST
        public async Task<ActionResult> Update(int id, [FromBody] CategoriaIngredienteDTO categoriaDto)
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

                // ✅ CONTROLLO DUPLICATI (escludendo la categoria corrente)
                var categoriaDuplicata = await _context.CategoriaIngrediente
                    .FirstOrDefaultAsync(c =>
                        c.Categoria.ToLower() == categoriaDto.Categoria.ToLower() &&
                        c.CategoriaId != id);

                if (categoriaDuplicata != null)
                    return SafeBadRequest("Categoria già esistente");

                await _repository.UpdateAsync(categoriaDto);

                // ✅ AUDIT TRAIL COMPLETO
                LogAuditTrail("UPDATE_CATEGORIA_INGREDIENTE", "CategoriaIngrediente", categoriaDto.CategoriaId.ToString());

                // ✅ SECURITY EVENT COMPLETO CON TIMESTAMP
                LogSecurityEvent("CategoriaIngredienteUpdated", new
                {
                    CategoriaId = categoriaDto.CategoriaId,
                    OldCategory = existing.Categoria,
                    NewCategory = categoriaDto.Categoria,
                    User = User.Identity?.Name,
                    Timestamp = DateTime.UtcNow
                });

                return NoContent();
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "Errore database durante l'aggiornamento della categoria ingrediente {Id}", id);
                return SafeInternalError("Errore durante l'aggiornamento della categoria");
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore durante l'aggiornamento della categoria ingrediente {Id}", id);
                return SafeInternalError("Errore durante l'aggiornamento della categoria");
            }
        }

        [HttpDelete("{id}")]
        // [Authorize(Roles = "admin")] // ✅ COMMENTATO PER TEST
        public async Task<ActionResult> Delete(int id)
        {
            try
            {
                if (id <= 0)
                    return SafeBadRequest("ID categoria non valido");

                var categoria = await _repository.GetByIdAsync(id);
                if (categoria == null)
                    return SafeNotFound("Categoria ingrediente");

                // ✅ CONTROLLO SE LA CATEGORIA HA INGREDIENTI ASSOCIATI
                var hasIngredienti = await _context.Ingrediente
                    .AnyAsync(i => i.CategoriaId == id);

                if (hasIngredienti)
                    return SafeBadRequest("Impossibile eliminare: la categoria ha ingredienti associati");

                await _repository.DeleteAsync(id);

                // ✅ AUDIT TRAIL COMPLETO
                LogAuditTrail("DELETE_CATEGORIA_INGREDIENTE", "CategoriaIngrediente", id.ToString());

                // ✅ SECURITY EVENT COMPLETO CON TIMESTAMP
                LogSecurityEvent("CategoriaIngredienteDeleted", new
                {
                    CategoriaId = id,
                    Categoria = categoria.Categoria,
                    User = User.Identity?.Name,
                    Timestamp = DateTime.UtcNow
                });

                return NoContent();
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "Errore database durante l'eliminazione della categoria ingrediente {Id}", id);
                return SafeInternalError("Errore durante l'eliminazione della categoria");
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore durante l'eliminazione della categoria ingrediente {Id}", id);
                return SafeInternalError("Errore durante l'eliminazione della categoria");
            }
        }
    }
}