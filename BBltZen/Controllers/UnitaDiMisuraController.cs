using Database;
using DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Repository.Interface;
using Repository.Service.Helper;
using System.Threading.Tasks;

namespace BBltZen.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [AllowAnonymous]
    public class UnitaDiMisuraController(
        IUnitaDiMisuraRepository repository,
        BubbleTeaContext context,
        IWebHostEnvironment environment,
        ILogger<UnitaDiMisuraController> logger)
        : SecureBaseController(environment, logger)
    {
        private readonly IUnitaDiMisuraRepository _repository = repository;
        private readonly BubbleTeaContext _context = context;

        // ✅ 2. POST /api/UnitaDiMisura
        // ✅ 2. POST /api/UnitaDiMisura - CORRETTO
        [HttpPost]
        // [Authorize(Roles = "admin,manager")]
        public async Task<ActionResult<UnitaDiMisuraDTO>> Create([FromBody] UnitaDiMisuraDTO unitaDto)
        {
            try
            {
                if (!IsModelValid(unitaDto))
                    return SafeBadRequest("Dati unità di misura non validi");

                // ✅ SIGLA: Converti in maiuscolo e valida lunghezza
                unitaDto.Sigla = StringHelper.NormalizeSearchTerm(unitaDto.Sigla).ToUpperInvariant();

                if (unitaDto.Sigla.Length > 2)
                    return SafeBadRequest("La sigla non può superare 2 caratteri");

                // ✅ DESCRIZIONE: Normalizza
                unitaDto.Descrizione = StringHelper.NormalizeSearchTerm(unitaDto.Descrizione);

                // ✅ CONTROLLO UNIVOCITÀ SIGLA (case-insensitive)
                var existingBySigla = await _context.UnitaDiMisura
                    .FirstOrDefaultAsync(u => u.Sigla.ToUpper() == unitaDto.Sigla.ToUpper());

                if (existingBySigla != null)
                    return SafeBadRequest($"Esiste già un'unità di misura con la sigla '{unitaDto.Sigla}'");

                // ✅ CONTROLLO UNIVOCITÀ DESCRIZIONE (case-insensitive)
                var existingByDesc = await _context.UnitaDiMisura
                    .FirstOrDefaultAsync(u => u.Descrizione.ToUpper() == unitaDto.Descrizione.ToUpper());

                if (existingByDesc != null)
                    return SafeBadRequest($"Esiste già un'unità di misura con la descrizione '{unitaDto.Descrizione}'");

                var result = await _repository.AddAsync(unitaDto);

                LogSecurityEvent("UnitaMisuraCreated", new
                {
                    result.UnitaMisuraId,
                    result.Sigla,
                    UserId = GetCurrentUserId()
                });

                LogAuditTrail("CREATE", "UnitaDiMisura", result.UnitaMisuraId.ToString());

                return CreatedAtAction(nameof(GetById), new { id = result.UnitaMisuraId }, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante la creazione dell'unità di misura");
                return SafeInternalError("Errore durante la creazione dell'unità di misura");
            }
        }

        // ✅ 3. GET /api/UnitaDiMisura - MODIFICATO (come Tavolo)
        [HttpGet("id")]
        [AllowAnonymous]
        public async Task<ActionResult> GetById([FromQuery] int? id, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                // ✅ SE ID NULL → LISTA COMPLETA
                if (!id.HasValue)
                {
                    var result = await _repository.GetAllAsync(page, pageSize);
                    return Ok(new
                    {
                        Message = $"Trovate {result.TotalCount} unità di misura",
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
                if (id <= 0) return SafeBadRequest("ID non valido");

                var unita = await _repository.GetByIdAsync(id.Value);
                return unita == null ? SafeNotFound("Unità di misura") : Ok(unita);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore recupero unità di misura {Id}", id);
                return SafeInternalError("Errore durante il recupero");
            }
        }

        // ✅ 4. PUT /api/UnitaDiMisura/{id}
        [HttpPut("{id}")]
        // [Authorize(Roles = "admin,manager")]
        public async Task<ActionResult> Update(int id, [FromBody] UnitaDiMisuraDTO unitaDto)
        {
            try
            {
                if (id != unitaDto.UnitaMisuraId)
                    return SafeBadRequest("ID non corrispondente");

                // ✅ SIGLA: Converti in maiuscolo e valida lunghezza
                unitaDto.Sigla = StringHelper.NormalizeSearchTerm(unitaDto.Sigla).ToUpperInvariant();

                if (unitaDto.Sigla.Length > 2)
                    return SafeBadRequest("La sigla non può superare 2 caratteri");

                // ✅ DESCRIZIONE: Normalizza
                unitaDto.Descrizione = StringHelper.NormalizeSearchTerm(unitaDto.Descrizione);

                // ✅ CONTROLLO DUPICATI SIGLA (case-insensitive)
                var existingBySigla = await _context.UnitaDiMisura
                    .FirstOrDefaultAsync(u => u.UnitaMisuraId != id &&
                                             u.Sigla.ToUpper() == unitaDto.Sigla.ToUpper());
                if (existingBySigla != null)
                    return SafeBadRequest($"Esiste già un'unità di misura con la sigla '{unitaDto.Sigla}'");

                // ✅ CONTROLLO DUPICATI DESCRIZIONE (case-insensitive)
                var existingByDesc = await _context.UnitaDiMisura
                    .FirstOrDefaultAsync(u => u.UnitaMisuraId != id &&
                                             u.Descrizione.ToUpper() == unitaDto.Descrizione.ToUpper());
                if (existingByDesc != null)
                    return SafeBadRequest($"Esiste già un'unità di misura con la descrizione '{unitaDto.Descrizione}'");

                await _repository.UpdateAsync(unitaDto);

                LogSecurityEvent("UnitaMisuraUpdated", new
                {
                    id,
                    unitaDto.Sigla,
                    UserId = GetCurrentUserId()
                });

                LogAuditTrail("UPDATE", "UnitaDiMisura", id.ToString());

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
                _logger.LogError(ex, "Errore aggiornamento unità di misura {Id}", id);
                return SafeInternalError("Errore durante l'aggiornamento");
            }
        }

        // ✅ 5. DELETE /api/UnitaDiMisura/{id}
        [HttpDelete("{id}")]
        // [Authorize(Roles = "admin")]
        public async Task<ActionResult> Delete(int id)
        {
            try
            {
                // ✅ CONTROLLO DIPENDENZE (come Tavolo)
                bool hasDimensioneBicchieri = await _context.DimensioneBicchiere
                    .AnyAsync(db => db.UnitaMisuraId == id);

                bool hasPersonalizzazioneIngredienti = await _context.PersonalizzazioneIngrediente
                    .AnyAsync(pi => pi.UnitaMisuraId == id);

                if (hasDimensioneBicchieri || hasPersonalizzazioneIngredienti)
                {
                    var errorMessage = "Impossibile eliminare: ";
                    if (hasDimensioneBicchieri) errorMessage += "sono presenti dimensioni bicchieri associate. ";
                    if (hasPersonalizzazioneIngredienti) errorMessage += "sono presenti personalizzazioni ingredienti associate.";
                    return SafeBadRequest(errorMessage.Trim());
                }

                await _repository.DeleteAsync(id);

                LogAuditTrail("DELETE", "UnitaDiMisura", id.ToString());
                LogSecurityEvent("UnitaMisuraDeleted", new
                {
                    id,
                    UserId = GetCurrentUserId()
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
                _logger.LogError(ex, "Errore eliminazione unità di misura {Id}", id);
                return SafeInternalError("Errore durante l'eliminazione");
            }
        }

        // ✅ 6. GET /api/UnitaDiMisura/sigla - MODIFICATO (come Tavolo)
        [HttpGet("sigla")]
        [AllowAnonymous]
        public async Task<ActionResult> GetBySigla(
            [FromQuery] string? sigla = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                // ✅ VALIDAZIONE SICUREZZA
                if (!SecurityHelper.IsValidInput(sigla, maxLength: 2))
                    return SafeBadRequest("Input non valido");

                var result = await _repository.GetBySiglaAsync(sigla, page, pageSize);

                // ✅ MESSAGGIO DINAMICO (come Tavolo)
                result.Message = !string.IsNullOrWhiteSpace(sigla)
                    ? (result.TotalCount > 0
                        ? $"Trovate {result.TotalCount} unità di misura che iniziano con '{sigla}' (pagina {result.Page} di {result.TotalPages})"
                        : $"Nessuna unità di misura trovata che inizia con '{sigla}'")
                    : $"Trovate {result.TotalCount} unità di misura (pagina {result.Page} di {result.TotalPages})";

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore recupero unità di misura per sigla {Sigla}", sigla);
                return SafeInternalError("Errore durante il recupero");
            }
        }

        // ✅ 8. GET /api/UnitaDiMisura/frontend/sigla - MODIFICATO (come Tavolo)
        [HttpGet("frontend/sigla")]
        [AllowAnonymous]
        public async Task<ActionResult> GetBySiglaPerFrontend(
            [FromQuery] string? sigla = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                if (!SecurityHelper.IsValidInput(sigla, maxLength: 2))
                    return SafeBadRequest("Input non valido");

                var result = await _repository.GetBySiglaPerFrontendAsync(sigla, page, pageSize);

                result.Message = !string.IsNullOrWhiteSpace(sigla)
                    ? (result.TotalCount > 0
                        ? $"Trovate {result.TotalCount} unità di misura che iniziano con '{sigla}' (pagina {result.Page} di {result.TotalPages})"
                        : $"Nessuna unità di misura trovata che inizia con '{sigla}'")
                    : $"Trovate {result.TotalCount} unità di misura (pagina {result.Page} di {result.TotalPages})";

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore recupero frontend unità di misura per sigla {Sigla}", sigla);
                return SafeInternalError("Errore durante il recupero");
            }
        }

        // ✅ 9. GET /api/UnitaDiMisura/descrizione - NUOVO (come Tavolo)
        [HttpGet("descrizione")]
        [AllowAnonymous]
        public async Task<ActionResult> GetByDescrizione(
            [FromQuery] string? descrizione = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                if (!SecurityHelper.IsValidInput(descrizione, maxLength: 50))
                    return SafeBadRequest("Input non valido");

                var result = await _repository.GetByDescrizioneAsync(descrizione, page, pageSize);

                result.Message = !string.IsNullOrWhiteSpace(descrizione)
                    ? (result.TotalCount > 0
                        ? $"Trovate {result.TotalCount} unità di misura che iniziano con '{descrizione}' (pagina {result.Page} di {result.TotalPages})"
                        : $"Nessuna unità di misura trovata che inizia con '{descrizione}'")
                    : $"Trovate {result.TotalCount} unità di misura (pagina {result.Page} di {result.TotalPages})";

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore recupero unità di misura per descrizione {Descrizione}", descrizione);
                return SafeInternalError("Errore durante il recupero");
            }
        }

        // ✅ 10. GET /api/UnitaDiMisura/frontend/descrizione - NUOVO (come Tavolo)
        [HttpGet("frontend/descrizione")]
        [AllowAnonymous]
        public async Task<ActionResult> GetByDescrizionePerFrontend(
            [FromQuery] string? descrizione = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                if (!SecurityHelper.IsValidInput(descrizione, maxLength: 50))
                    return SafeBadRequest("Input non valido");

                var result = await _repository.GetByDescrizionePerFrontendAsync(descrizione, page, pageSize);

                result.Message = !string.IsNullOrWhiteSpace(descrizione)
                    ? (result.TotalCount > 0
                        ? $"Trovate {result.TotalCount} unità di misura che iniziano con '{descrizione}' (pagina {result.Page} di {result.TotalPages})"
                        : $"Nessuna unità di misura trovata che inizia con '{descrizione}'")
                    : $"Trovate {result.TotalCount} unità di misura (pagina {result.Page} di {result.TotalPages})";

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore recupero frontend unità di misura per descrizione {Descrizione}", descrizione);
                return SafeInternalError("Errore durante il recupero");
            }
        }
    }
}