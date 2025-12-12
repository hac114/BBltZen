using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using DTO;
using Repository.Interface;
using System;
using System.Threading.Tasks;
using Database.Models;

namespace BBltZen.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [AllowAnonymous] // ✅ OVERRIDE DELL'[Authorize] DEL BASE CONTROLLER
    public class DimensioneBicchiereController : SecureBaseController
    {
        private readonly IDimensioneBicchiereRepository _repository;
        private readonly BubbleTeaContext _context;

        public DimensioneBicchiereController(
            IDimensioneBicchiereRepository repository,
            BubbleTeaContext context,
            IWebHostEnvironment environment,
            ILogger<DimensioneBicchiereController> logger)
            : base(environment, logger)
        {
            _repository = repository;
            _context = context;
        }

        // ✅ ENDPOINT BACKEND CON PARAMETRI OPZIONALI E PAGINAZIONE

        // GET: api/DimensioneBicchiere?id={id}&page={page}&pageSize={pageSize}
        [HttpGet("id")]
        [AllowAnonymous]
        public async Task<ActionResult> GetById([FromQuery] int? id, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                if (!id.HasValue)
                {
                    var result = await _repository.GetAllAsync(page, pageSize);
                    return Ok(new
                    {
                        Message = $"Trovati {result.TotalCount} tipi di bicchiere",
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

                if (id <= 0)
                    return SafeBadRequest("ID bicchiere non valido");

                var dimensione = await _repository.GetByIdAsync(id.Value);
                return dimensione == null ? SafeNotFound("Bicchiere") : Ok(dimensione);                
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante il recupero del bicchiere {Id}", id);
                return SafeInternalError("Errore durante il recupero del bicchiere");
            }
        }

        // GET: api/DimensioneBicchiere/sigla?sigla={sigla}&page={page}&pageSize={pageSize}
        [HttpGet("sigla")]
        [AllowAnonymous]
        public async Task<ActionResult<DimensioneBicchiereDTO>> GetBySigla([FromQuery] string? sigla)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(sigla))
                    return SafeBadRequest<DimensioneBicchiereDTO>("Sigla obbligatoria");

                var dimensione = await _repository.GetBySiglaAsync(sigla);

                if (dimensione == null)
                    return SafeNotFound<DimensioneBicchiereDTO>("Dimensione bicchiere");

                return Ok(dimensione);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore nel recupero per sigla: {Sigla}", sigla);
                return SafeInternalError<DimensioneBicchiereDTO>("Errore durante il recupero");
            }
        }

        // GET: api/DimensioneBicchiere/descrizione?descrizione={descrizione}&page={page}&pageSize={pageSize}
        [HttpGet("frontend/descrizione")]
        [AllowAnonymous]
        public async Task<ActionResult<object>> GetFrontendByDescrizione(
            [FromQuery] string? descrizione = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                var result = await _repository.GetFrontendByDescrizioneAsync(descrizione, page, pageSize);

                return Ok(new
                {
                    result.Message,
                    SearchTerm = descrizione,
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore nel filtro frontend per descrizione: {Descrizione}", descrizione);
                return SafeInternalError<object>("Errore durante il recupero");
            }
        }

        // GET: api/DimensioneBicchiere/filtri?parametri multipli (FRONTEND)
        [HttpGet("filtri")]
        [AllowAnonymous]
        public async Task<ActionResult<object>> GetByFiltri(
            [FromQuery] string? sigla = null,
            [FromQuery] string? descrizione = null,
            [FromQuery] decimal? capienza = null,
            [FromQuery] decimal? prezzoBase = null,
            [FromQuery] decimal? moltiplicatore = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                var result = await _repository.GetFrontendAsync(
                    sigla: sigla,
                    descrizione: descrizione,
                    capienza: capienza,
                    prezzoBase: prezzoBase,
                    moltiplicatore: moltiplicatore,
                    page: page,
                    pageSize: pageSize);

                // Costruzione messaggio dinamico
                var filters = new System.Text.StringBuilder();
                if (!string.IsNullOrWhiteSpace(sigla)) filters.Append($"Sigla: {sigla}, ");
                if (!string.IsNullOrWhiteSpace(descrizione)) filters.Append($"Descrizione: {descrizione}, ");
                if (capienza.HasValue) filters.Append($"Capienza: {capienza}, ");
                if (prezzoBase.HasValue) filters.Append($"Prezzo base: {prezzoBase}, ");
                if (moltiplicatore.HasValue) filters.Append($"Moltiplicatore: {moltiplicatore}, ");

                var filterMessage = filters.Length > 0
                    ? filters.ToString().TrimEnd(',', ' ')
                    : "nessun filtro";

                return Ok(new
                {
                    Message = $"Trovate {result.TotalCount} dimensioni bicchiere con filtri: {filterMessage}",
                    SearchTerms = new { sigla, descrizione, capienza, prezzoBase, moltiplicatore },
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore nei filtri compositi");
                return SafeInternalError<object>("Errore durante il recupero delle dimensioni");
            }
        }

        // ✅ ENDPOINT FRONTEND SEPARATI

        // GET: api/DimensioneBicchiere/frontend/id?id={id}
        [HttpGet("frontend/id")]
        [AllowAnonymous]
        public async Task<ActionResult<object>> GetFrontendById([FromQuery] int? id)
        {
            try
            {
                if (!id.HasValue)
                    return SafeBadRequest<object>("ID obbligatorio per endpoint frontend");

                if (id <= 0)
                    return SafeBadRequest<object>("ID non valido");

                var item = await _repository.GetFrontendByIdAsync(id.Value);

                if (item == null)
                    return SafeNotFound<object>("Dimensione bicchiere");

                return Ok(new
                {
                    Message = "Dimensione bicchiere trovata",
                    Data = item
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore nel recupero frontend ID: {Id}", id);
                return SafeInternalError<object>("Errore durante il recupero");
            }
        }

        // GET: api/DimensioneBicchiere/frontend/ricerca?search={search}&page={page}&pageSize={pageSize}
        [HttpGet("frontend/ricerca")]
        [AllowAnonymous]
        public async Task<ActionResult<object>> GetFrontendRicerca(
            [FromQuery] string? sigla = null,
            [FromQuery] string? descrizione = null,
            [FromQuery] decimal? capienza = null,
            [FromQuery] decimal? prezzoBase = null,
            [FromQuery] decimal? moltiplicatore = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                var result = await _repository.GetFrontendAsync(
                    sigla: sigla,
                    descrizione: descrizione,
                    capienza: capienza,
                    prezzoBase: prezzoBase,
                    moltiplicatore: moltiplicatore,
                    page: page,
                    pageSize: pageSize);

                // Costruzione messaggio dinamico
                var filterInfo = new System.Text.StringBuilder();
                if (!string.IsNullOrWhiteSpace(sigla)) filterInfo.Append($"Sigla: {sigla}, ");
                if (!string.IsNullOrWhiteSpace(descrizione)) filterInfo.Append($"Descrizione: {descrizione}, ");
                if (capienza.HasValue) filterInfo.Append($"Capienza: {capienza}, ");
                if (prezzoBase.HasValue) filterInfo.Append($"Prezzo base: {prezzoBase}, ");
                if (moltiplicatore.HasValue) filterInfo.Append($"Moltiplicatore: {moltiplicatore}, ");

                var filterStr = filterInfo.Length > 0
                    ? $" con filtri: {filterInfo.ToString().TrimEnd(',', ' ')}"
                    : "";

                return Ok(new
                {
                    Message = $"Trovate {result.TotalCount} dimensioni bicchiere{filterStr}",
                    SearchTerms = new { sigla, descrizione, capienza, prezzoBase, moltiplicatore },
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore nella ricerca frontend");
                return SafeInternalError<object>("Errore durante la ricerca");
            }
        }

        // ✅ ENDPOINT COMPATIBILITÀ - Mantiene gli endpoint originali

        // GET: api/DimensioneBicchiere (COMPATIBILITÀ - senza paginazione)
        [HttpGet("tutte")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<DimensioneBicchiereDTO>>> GetAll()
        {
            try
            {
                var dimensioni = await _repository.GetAllAsync();
                return Ok(dimensioni);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante il recupero di tutte le dimensioni bicchieri");
                return SafeInternalError<IEnumerable<DimensioneBicchiereDTO>>("Errore durante il recupero");
            }
        }

        // GET: api/DimensioneBicchiere/{id} (COMPATIBILITÀ - route tradizionale)
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<DimensioneBicchiereDTO>> GetByIdRoute(int id)
        {
            try
            {
                if (id <= 0)
                    return SafeBadRequest<DimensioneBicchiereDTO>("ID dimensione non valido");

                var dimensione = await _repository.GetByIdAsync(id);

                if (dimensione == null)
                    return SafeNotFound<DimensioneBicchiereDTO>("Dimensione bicchiere");

                return Ok(dimensione);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante il recupero della dimensione bicchiere {Id}", id);
                return SafeInternalError<DimensioneBicchiereDTO>("Errore durante il recupero della dimensione");
            }
        }        

        //// POST: api/DimensioneBicchiere
        //[HttpPost]
        //// [Authorize(Roles = "admin")]
        //public async Task<ActionResult<DimensioneBicchiereDTO>> Create([FromBody] DimensioneBicchiereDTO dimensioneDto)
        //{
        //    try
        //    {
        //        if (!IsModelValid(dimensioneDto))
        //            return SafeBadRequest<DimensioneBicchiereDTO>("Dati dimensione non validi");

        //        // ✅ Verifica unità di misura
        //        var unitaMisuraEsiste = await _context.UnitaDiMisura.AnyAsync(u => u.UnitaMisuraId == dimensioneDto.UnitaMisuraId);
        //        if (!unitaMisuraEsiste)
        //            return SafeBadRequest<DimensioneBicchiereDTO>("Unità di misura non trovata");

        //        // ✅ Verifica sigla univoca usando repository
        //        if (await _repository.SiglaExistsAsync(dimensioneDto.Sigla))
        //            return SafeBadRequest<DimensioneBicchiereDTO>("Esiste già una dimensione con questa sigla");

        //        var result = await _repository.AddAsync(dimensioneDto);

        //        // ✅ Audit trail
        //        LogAuditTrail("CREATE", "DimensioneBicchiere", result.DimensioneBicchiereId.ToString());
        //        LogSecurityEvent("DimensioneBicchiereCreated", new { result.DimensioneBicchiereId, result.Sigla, User = User.Identity?.Name });

        //        return CreatedAtAction(nameof(GetById), new { id = result.DimensioneBicchiereId }, result);
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Errore durante la creazione della dimensione bicchiere");
        //        return SafeInternalError<DimensioneBicchiereDTO>("Errore durante la creazione");
        //    }
        //}

        //// PUT: api/DimensioneBicchiere/5
        //[HttpPut("{id}")]
        //// [Authorize(Roles = "admin")]
        //public async Task<ActionResult> Update(int id, [FromBody] DimensioneBicchiereDTO dimensioneDto)
        //{
        //    try
        //    {
        //        if (id <= 0 || id != dimensioneDto.DimensioneBicchiereId)
        //            return SafeBadRequest("ID dimensione non valido");

        //        if (!IsModelValid(dimensioneDto))
        //            return SafeBadRequest("Dati dimensione non validi");

        //        if (!await _repository.ExistsAsync(id))
        //            return SafeNotFound("Dimensione bicchiere");

        //        // ✅ Verifica unità di misura
        //        var unitaMisuraEsiste = await _context.UnitaDiMisura.AnyAsync(u => u.UnitaMisuraId == dimensioneDto.UnitaMisuraId);
        //        if (!unitaMisuraEsiste)
        //            return SafeBadRequest("Unità di misura non trovata");

        //        // ✅ Verifica sigla duplicata usando repository
        //        if (await _repository.SiglaExistsForOtherAsync(id, dimensioneDto.Sigla))
        //            return SafeBadRequest("Esiste già un'altra dimensione con questa sigla");

        //        await _repository.UpdateAsync(dimensioneDto);

        //        LogAuditTrail("UPDATE", "DimensioneBicchiere", id.ToString());
        //        LogSecurityEvent("DimensioneBicchiereUpdated", new { DimensioneId = id, dimensioneDto.Sigla, User = User.Identity?.Name });

        //        return NoContent();
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Errore durante l'aggiornamento della dimensione bicchiere {Id}", id);
        //        return SafeInternalError("Errore durante l'aggiornamento");
        //    }
        //}

        // DELETE: api/DimensioneBicchiere/5
        [HttpDelete("{id}")]
        // [Authorize(Roles = "admin")]
        public async Task<ActionResult> Delete(int id)
        {
            try
            {
                if (id <= 0)
                    return SafeBadRequest("ID dimensione non valido");

                var dimensione = await _repository.GetByIdAsync(id);
                if (dimensione == null)
                    return SafeNotFound("Dimensione bicchiere");

                // ✅ Controllo dipendenze
                var hasDependencies = await _context.BevandaStandard.AnyAsync(b => b.DimensioneBicchiereId == id) ||
                                     await _context.DimensioneQuantitaIngredienti.AnyAsync(d => d.DimensioneBicchiereId == id) ||
                                     await _context.PersonalizzazioneCustom.AnyAsync(p => p.DimensioneBicchiereId == id);

                if (hasDependencies)
                    return SafeBadRequest("Impossibile eliminare: la dimensione è utilizzata in altri elementi");

                await _repository.DeleteAsync(id);

                LogAuditTrail("DELETE", "DimensioneBicchiere", id.ToString());
                LogSecurityEvent("DimensioneBicchiereDeleted", new { DimensioneId = id, dimensione.Sigla, User = User.Identity?.Name });

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante l'eliminazione della dimensione bicchiere {Id}", id);
                return SafeInternalError("Errore durante l'eliminazione");
            }
        }

        [HttpGet("frontend/sigla")]
        [AllowAnonymous]
        public async Task<ActionResult<object>> GetFrontendBySigla(
            [FromQuery] string? sigla = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                var result = await _repository.GetFrontendBySiglaAsync(sigla, page, pageSize);

                return Ok(new
                {
                    result.Message,
                    SearchTerm = sigla,
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore nel filtro frontend per sigla: {Sigla}", sigla);
                return SafeInternalError<object>("Errore durante il recupero");
            }
        }
    }
}