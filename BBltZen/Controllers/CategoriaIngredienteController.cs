using Microsoft.AspNetCore.Mvc;
using DTO;
using Repository.Interface;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace BBltZen.Controllers
{
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
        public class CategoriaIngredienteController(
            ICategoriaIngredienteRepository repository,
            Database.BubbleTeaContext context,
            IWebHostEnvironment environment,
            ILogger<CategoriaIngredienteController> logger)
            : SecureBaseController(environment, logger)
        {
            private readonly ICategoriaIngredienteRepository _repository = repository;
            private readonly Database.BubbleTeaContext _context = context;

            //[HttpGet]
            //public async Task<ActionResult<IEnumerable<CategoriaIngredienteDTO>>> GetAll()
            //{
            //    try
            //    {
            //        var categorie = await _repository.GetAllAsync();
            //        return Ok(categorie);
            //    }
            //    catch (Exception ex)
            //    {
            //        _logger.LogError(ex, "Errore durante il recupero di tutte le categorie ingredienti");
            //        return SafeInternalError<IEnumerable<CategoriaIngredienteDTO>>("Errore durante il recupero delle categorie");
            //    }
            //}

            [HttpGet("id")]
            [AllowAnonymous]
            public async Task<ActionResult> GetById([FromQuery] int? id = null, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
            {
                try
                {
                    // ✅ SE ID NULL → LISTA COMPLETA PAGINATA
                    if (!id.HasValue)
                    {
                        var result = await _repository.GetAllAsync(page, pageSize);
                        return Ok(new
                        {
                            Message = $"Trovate {result.TotalCount} categorie",
                            result.Data,
                            Pagination = new
                            {
                                result.Page,
                                result.PageSize,
                                result.TotalCount,
                                result.TotalPages,
                                result.HasPrevious,
                                result.HasNext
                            }
                        });
                    }

                    // ✅ SE ID VALORIZZATO → SINGOLO ELEMENTO
                    if (id <= 0) return SafeBadRequest("ID categoria non valido");

                    var categoria = await _repository.GetByIdAsync(id.Value);
                    return categoria == null ? SafeNotFound("Categoria ingrediente") : Ok(categoria);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Errore durante il recupero della categoria ingrediente {Id}", id);
                    return SafeInternalError("Errore durante il recupero della categoria");
                }
            }

            // ✅ 2. POST /api/CategoriaIngrediente (AGGIORNATO)
            [HttpPost]
            // [Authorize(Roles = "admin,manager")]
            public async Task<ActionResult<CategoriaIngredienteDTO>> Create([FromBody] CategoriaIngredienteDTO categoriaDto)
            {
                try
                {
                    if (!IsModelValid(categoriaDto))
                        return SafeBadRequest("Dati categoria non validi");

                    // ✅ VERIFICA UNICITÀ NOME (case-insensitive)
                    var existingByNome = await _context.CategoriaIngrediente
                        .FirstOrDefaultAsync(c => c.Categoria.ToUpper() == categoriaDto.Categoria.ToUpper());

                    if (existingByNome != null)
                        return SafeBadRequest($"Esiste già una categoria con nome '{categoriaDto.Categoria}'");

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
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Errore durante la creazione categoria ingrediente");
                    return SafeInternalError("Errore durante la creazione");
                }
            }

            // ✅ 4. PUT /api/CategoriaIngrediente/{id} (AGGIORNATO)
            [HttpPut("{id}")]
            // [Authorize(Roles = "admin,manager")]
            public async Task<ActionResult> Update(int id, [FromBody] CategoriaIngredienteDTO categoriaDto)
            {
                try
                {
                    if (id != categoriaDto.CategoriaId)
                        return SafeBadRequest("ID non corrispondente");

                    // ✅ CONTROLLO DUPICATI NOME (case-insensitive)
                    var existingByNome = await _context.CategoriaIngrediente
                        .FirstOrDefaultAsync(c => c.CategoriaId != id &&
                                                 c.Categoria.ToUpper() == categoriaDto.Categoria.ToUpper());

                    if (existingByNome != null)
                        return SafeBadRequest($"Esiste già una categoria con nome '{categoriaDto.Categoria}'");

                    await _repository.UpdateAsync(categoriaDto);

                    // ✅ AUDIT & SECURITY
                    LogAuditTrail("UPDATE", "CategoriaIngrediente", id.ToString());
                    LogSecurityEvent("CategoriaIngredienteUpdated", new
                    {
                        CategoriaId = id,
                        categoriaDto.Categoria,
                        UserId = GetCurrentUserIdOrDefault()
                    });

                    return NoContent();
                }
                catch (KeyNotFoundException ex)
                {
                    return SafeNotFound(ex.Message);
                }
                catch (InvalidOperationException ex)
                {
                    return SafeBadRequest(ex.Message);
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

            //[HttpGet("exists/{nome}")]
            //public async Task<ActionResult<bool>> CheckNomeExists(string nome)
            //{
            //    try
            //    {
            //        if (string.IsNullOrWhiteSpace(nome))
            //            return SafeBadRequest<bool>("Nome categoria non valido");

            //        bool exists = await _repository.ExistsByNomeAsync(nome);
            //        return Ok(exists);
            //    }
            //    catch (Exception ex)
            //    {
            //        _logger.LogError(ex, "Errore durante verifica esistenza nome categoria {Nome}", nome);
            //        return SafeInternalError<bool>("Errore durante la verifica");
            //    }

            //}

            // ✅ NUOVI ENDPOINT PER FRONTEND

            //[HttpGet("frontend")]
            //public async Task<ActionResult<IEnumerable<CategoriaIngredienteFrontendDTO>>> GetAllPerFrontend()
            //{
            //    try
            //    {
            //        var categorie = await _repository.GetAllPerFrontendAsync();
            //        return Ok(categorie);
            //    }
            //    catch (Exception ex)
            //    {
            //        _logger.LogError(ex, "Errore durante il recupero di tutte le categorie per frontend");
            //        return SafeInternalError<IEnumerable<CategoriaIngredienteFrontendDTO>>("Errore durante il recupero delle categorie");
            //    }
            //}

            //[HttpGet("frontend/{nome}")]
            //public async Task<ActionResult<CategoriaIngredienteFrontendDTO>> GetByNomePerFrontend(string nome)
            //{
            //    try
            //    {
            //        if (string.IsNullOrWhiteSpace(nome))
            //            return SafeBadRequest<CategoriaIngredienteFrontendDTO>("Nome categoria non valido");

            //        var categoria = await _repository.GetByNomePerFrontendAsync(nome);
            //        return categoria == null
            //            ? SafeNotFound<CategoriaIngredienteFrontendDTO>("Categoria ingrediente")
            //            : Ok(categoria);
            //    }
            //    catch (Exception ex)
            //    {
            //        _logger.LogError(ex, "Errore durante il recupero della categoria per nome {Nome}", nome);
            //        return SafeInternalError<CategoriaIngredienteFrontendDTO>("Errore durante il recupero della categoria");
            //    }
            //}

            // ✅ 9. GET /api/CategoriaIngrediente/nome (parametro opzionale)
            [HttpGet("nome")]
            [AllowAnonymous]
            public async Task<ActionResult> GetByNome(
                [FromQuery] string? nome = null,
                [FromQuery] int page = 1,
                [FromQuery] int pageSize = 10)
            {
                try
                {
                    // ✅ VALIDAZIONE SICUREZZA
                    if (!Repository.Service.Helper.SecurityHelper.IsValidInput(nome, maxLength: 50))
                        return SafeBadRequest("Input non valido");

                    var result = await _repository.GetByNomeAsync(nome, page, pageSize);

                    // ✅ MESSAGGIO DINAMICO
                    result.Message = !string.IsNullOrWhiteSpace(nome)
                        ? (result.TotalCount > 0
                            ? $"Trovate {result.TotalCount} categorie che iniziano con '{nome}' (pagina {result.Page} di {result.TotalPages})"
                            : $"Nessuna categoria trovata che inizia con '{nome}'")
                        : $"Trovate {result.TotalCount} categorie (pagina {result.Page} di {result.TotalPages})";

                    return Ok(result);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Errore durante il recupero categorie per nome {Nome}", nome);
                    return SafeInternalError("Errore durante il recupero delle categorie");
                }
            }

            // ✅ 10. GET /api/CategoriaIngrediente/frontend/nome (parametro opzionale)
            [HttpGet("frontend/nome")]
            [AllowAnonymous]
            public async Task<ActionResult> GetByNomePerFrontend(
                [FromQuery] string? nome = null,
                [FromQuery] int page = 1,
                [FromQuery] int pageSize = 10)
            {
                try
                {
                    if (!Repository.Service.Helper.SecurityHelper.IsValidInput(nome, maxLength: 50))
                        return SafeBadRequest("Input non valido");

                    var result = await _repository.GetByNomePerFrontendAsync(nome, page, pageSize);

                    result.Message = !string.IsNullOrWhiteSpace(nome)
                        ? (result.TotalCount > 0
                            ? $"Trovate {result.TotalCount} categorie che iniziano con '{nome}' (pagina {result.Page} di {result.TotalPages})"
                            : $"Nessuna categoria trovata che inizia con '{nome}'")
                        : $"Trovate {result.TotalCount} categorie (pagina {result.Page} di {result.TotalPages})";

                    return Ok(result);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Errore durante il recupero categorie frontend per nome {Nome}", nome);
                    return SafeInternalError("Errore durante il recupero delle categorie");
                }
            }
        }
    }
}

   