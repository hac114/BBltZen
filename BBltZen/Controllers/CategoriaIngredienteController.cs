using Microsoft.AspNetCore.Mvc;
using DTO;
using Repository.Interface;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace BBltZen.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [AllowAnonymous] // ✅ OVERRIDE PER TESTING SWAGGER
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
        public async Task<ActionResult<IEnumerable<CategoriaIngredienteDTO>>> GetAll()
        {
            try
            {
                var categorie = await _repository.GetAllAsync();
                return Ok(categorie);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante il recupero di tutte le categorie ingredienti");
                return SafeInternalError<IEnumerable<CategoriaIngredienteDTO>>("Errore durante il recupero delle categorie");
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
                return categoria == null
                    ? SafeNotFound<CategoriaIngredienteDTO>("Categoria ingrediente")
                    : Ok(categoria);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante il recupero della categoria ingrediente {Id}", id);
                return SafeInternalError<CategoriaIngredienteDTO>("Errore durante il recupero della categoria");
            }
        }

        [HttpPost]
        // [Authorize(Roles = "admin,manager")] // ✅ KEYCLOAK READY - COMMENTATO PER TEST
        public async Task<ActionResult<CategoriaIngredienteDTO>> Create([FromBody] CategoriaIngredienteDTO categoriaDto)
        {
            try
            {
                if (!IsModelValid(categoriaDto))
                    return SafeBadRequest<CategoriaIngredienteDTO>("Dati categoria non validi");

                // ✅ VERIFICA UNICITÀ NOME
                if (await _repository.ExistsByNomeAsync(categoriaDto.Categoria))
                    return SafeBadRequest<CategoriaIngredienteDTO>("Esiste già una categoria con questo nome");

                // ✅ USA IL RISULTATO DI AddAsync (PATTERN STANDARD)
                var result = await _repository.AddAsync(categoriaDto);

                // ✅ AUDIT & SECURITY
                LogAuditTrail("CREATE", "CategoriaIngrediente", result.CategoriaId.ToString());
                LogSecurityEvent("CategoriaIngredienteCreated", new
                {
                    result.CategoriaId,
                    result.Categoria,
                    UserId = GetCurrentUserIdOrDefault()
                });

                return CreatedAtAction(nameof(GetById), new { id = result.CategoriaId }, result);
            }
            catch (ArgumentException argEx)
            {
                return SafeBadRequest<CategoriaIngredienteDTO>(argEx.Message);
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "Errore database durante la creazione categoria ingrediente");
                return SafeInternalError<CategoriaIngredienteDTO>("Errore durante il salvataggio");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante la creazione categoria ingrediente");
                return SafeInternalError<CategoriaIngredienteDTO>("Errore durante la creazione");
            }
        }

        [HttpPut("{id}")]
        // [Authorize(Roles = "admin,manager")] // ✅ KEYCLOAK READY - COMMENTATO PER TEST
        public async Task<ActionResult> Update(int id, [FromBody] CategoriaIngredienteDTO categoriaDto)
        {
            try
            {
                if (id <= 0 || id != categoriaDto.CategoriaId)
                    return SafeBadRequest("ID categoria non valido");

                if (!IsModelValid(categoriaDto))
                    return SafeBadRequest("Dati categoria non validi");

                // ✅ VERIFICA ESISTENZA
                if (!await _repository.ExistsAsync(id))
                    return SafeNotFound("Categoria ingrediente");

                // ✅ VERIFICA UNICITÀ NOME (escludendo corrente)
                if (await _repository.ExistsByNomeAsync(categoriaDto.Categoria, id))
                    return SafeBadRequest("Esiste già un'altra categoria con questo nome");

                await _repository.UpdateAsync(categoriaDto);

                // ✅ AUDIT & SECURITY
                LogAuditTrail("UPDATE", "CategoriaIngrediente", id.ToString());
                LogSecurityEvent("CategoriaIngredienteUpdated", new
                {
                    CategoriaId = id,
                    UserId = GetCurrentUserIdOrDefault()
                });

                return NoContent();
            }
            catch (ArgumentException argEx)
            {
                return SafeBadRequest(argEx.Message);
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "Errore database durante l'aggiornamento categoria ingrediente {Id}", id);
                return SafeInternalError("Errore durante l'aggiornamento");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante l'aggiornamento categoria ingrediente {Id}", id);
                return SafeInternalError("Errore durante l'aggiornamento");
            }
        }

        [HttpDelete("{id}")]
        // [Authorize(Roles = "admin")] // ✅ KEYCLOAK READY - COMMENTATO PER TEST
        public async Task<ActionResult> Delete(int id)
        {
            try
            {
                if (id <= 0)
                    return SafeBadRequest("ID categoria non valido");

                var categoria = await _repository.GetByIdAsync(id);
                if (categoria == null)
                    return SafeNotFound("Categoria ingrediente");

                // ✅ CONTROLLO VINCOLI REFERENZIALI
                bool hasIngredienti = await _context.Ingrediente.AnyAsync(i => i.CategoriaId == id);
                if (hasIngredienti)
                    return SafeBadRequest("Impossibile eliminare: categoria ha ingredienti associati");

                await _repository.DeleteAsync(id);

                // ✅ AUDIT & SECURITY
                LogAuditTrail("DELETE", "CategoriaIngrediente", id.ToString());
                LogSecurityEvent("CategoriaIngredienteDeleted", new
                {
                    CategoriaId = id,
                    UserId = GetCurrentUserIdOrDefault()
                });

                return NoContent();
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "Errore database durante l'eliminazione categoria ingrediente {Id}", id);
                return SafeInternalError("Errore durante l'eliminazione");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante l'eliminazione categoria ingrediente {Id}", id);
                return SafeInternalError("Errore durante l'eliminazione");
            }
        }

        [HttpGet("exists/{nome}")]
        public async Task<ActionResult<bool>> CheckNomeExists(string nome, [FromQuery] int? excludeId = null)
        {
            try
            {
                bool exists = await _repository.ExistsByNomeAsync(nome, excludeId);
                return Ok(exists);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante verifica esistenza nome categoria {Nome}", nome);
                return SafeInternalError<bool>("Errore durante la verifica");
            }
        }
    }
}