// BBltZen/Controllers/VwArticoliCompletiController.cs
using Microsoft.AspNetCore.Mvc;
using DTO;
using Repository.Interface;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;

namespace BBltZen.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    //[Authorize] // ✅ COMMENTATO PER TEST CON SWAGGER
    public class VwArticoliCompletiController : SecureBaseController
    {
        private readonly IVwArticoliCompletiRepository _repository;

        public VwArticoliCompletiController(
            IVwArticoliCompletiRepository repository,
            IWebHostEnvironment environment,
            ILogger<VwArticoliCompletiController> logger)
            : base(environment, logger)
        {
            _repository = repository;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<VwArticoliCompletiDTO>>> GetAll()
        {
            try
            {
                var articoli = await _repository.GetAllAsync();
                LogAuditTrail("GET_ALL", "VwArticoliCompleti", $"Count: {articoli?.Count()}");
                return Ok(articoli);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore nel recupero di tutti gli articoli completi");
                return SafeInternalError<IEnumerable<VwArticoliCompletiDTO>>(ex.Message);
            }
        }

        [HttpGet("{id}")]
        [AllowAnonymous] // ✅ VISTA PUBBLICA
        public async Task<ActionResult<VwArticoliCompletiDTO>> GetById(int id)
        {
            try
            {
                if (id <= 0)
                    return SafeBadRequest<VwArticoliCompletiDTO>("ID articolo non valido");

                var articolo = await _repository.GetByIdAsync(id);
                if (articolo == null)
                    return SafeNotFound<VwArticoliCompletiDTO>("Articolo");

                // ✅ Audit trail completo
                LogAuditTrail("GET_BY_ID", "VwArticoliCompleti", id.ToString());
                LogSecurityEvent("VwArticoliCompletiAccessed", new
                {
                    Operation = "GetById",
                    ArticoloId = id,
                    User = User.Identity?.Name ?? "Anonymous",
                    Timestamp = DateTime.UtcNow
                });

                return Ok(articolo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore nel recupero articolo con ID: {Id}", id);
                return SafeInternalError<VwArticoliCompletiDTO>(ex.Message);
            }
        }

        [HttpGet("tipo/{tipoArticolo}")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<VwArticoliCompletiDTO>>> GetByTipo(string tipoArticolo)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(tipoArticolo))
                    return SafeBadRequest<IEnumerable<VwArticoliCompletiDTO>>("Tipo articolo non valido");

                var articoli = await _repository.GetByTipoAsync(tipoArticolo);
                LogAuditTrail("GET_BY_TIPO", "VwArticoliCompleti", tipoArticolo);
                return Ok(articoli);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore nel recupero articoli di tipo: {TipoArticolo}", tipoArticolo);
                return SafeInternalError<IEnumerable<VwArticoliCompletiDTO>>(ex.Message);
            }
        }

        [HttpGet("categoria/{categoria}")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<VwArticoliCompletiDTO>>> GetByCategoria(string categoria)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(categoria))
                    return SafeBadRequest<IEnumerable<VwArticoliCompletiDTO>>("Categoria non valida");

                var articoli = await _repository.GetByCategoriaAsync(categoria);
                LogAuditTrail("GET_BY_CATEGORIA", "VwArticoliCompleti", categoria);
                return Ok(articoli);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore nel recupero articoli della categoria: {Categoria}", categoria);
                return SafeInternalError<IEnumerable<VwArticoliCompletiDTO>>(ex.Message);
            }
        }

        [HttpGet("disponibili")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<VwArticoliCompletiDTO>>> GetDisponibili()
        {
            try
            {
                var articoli = await _repository.GetDisponibiliAsync();
                LogAuditTrail("GET_DISPONIBILI", "VwArticoliCompleti", $"Count: {articoli?.Count()}");
                return Ok(articoli);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore nel recupero articoli disponibili");
                return SafeInternalError<IEnumerable<VwArticoliCompletiDTO>>(ex.Message);
            }
        }

        [HttpGet("cerca/{nome}")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<VwArticoliCompletiDTO>>> SearchByName(string nome)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(nome))
                    return SafeBadRequest<IEnumerable<VwArticoliCompletiDTO>>("Nome ricerca non valido");

                var articoli = await _repository.SearchByNameAsync(nome);
                LogAuditTrail("SEARCH_BY_NAME", "VwArticoliCompleti", nome);
                return Ok(articoli);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore nella ricerca articoli per nome: {Nome}", nome);
                return SafeInternalError<IEnumerable<VwArticoliCompletiDTO>>(ex.Message);
            }
        }

        [HttpGet("prezzo")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<VwArticoliCompletiDTO>>> GetByPriceRange([FromQuery] decimal prezzoMin, [FromQuery] decimal prezzoMax)
        {
            try
            {
                if (prezzoMin < 0 || prezzoMax < 0)
                    return SafeBadRequest<IEnumerable<VwArticoliCompletiDTO>>("I prezzi non possono essere negativi");

                if (prezzoMin > prezzoMax)
                    return SafeBadRequest<IEnumerable<VwArticoliCompletiDTO>>("Il prezzo minimo non può essere maggiore del prezzo massimo");

                var articoli = await _repository.GetByPriceRangeAsync(prezzoMin, prezzoMax);
                LogAuditTrail("GET_BY_PRICE_RANGE", "VwArticoliCompleti", $"{prezzoMin}-{prezzoMax}");
                return Ok(articoli);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore nel recupero articoli nel range prezzi {PrezzoMin}-{PrezzoMax}", prezzoMin, prezzoMax);
                return SafeInternalError<IEnumerable<VwArticoliCompletiDTO>>(ex.Message);
            }
        }

        [HttpGet("con-iva")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<VwArticoliCompletiDTO>>> GetArticoliConIva()
        {
            try
            {
                var articoli = await _repository.GetArticoliConIvaAsync();
                LogAuditTrail("GET_CON_IVA", "VwArticoliCompleti", $"Count: {articoli?.Count()}");
                return Ok(articoli);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore nel recupero articoli con IVA");
                return SafeInternalError<IEnumerable<VwArticoliCompletiDTO>>(ex.Message);
            }
        }

        [HttpGet("count")]
        [AllowAnonymous] // ✅ VISTA PUBBLICA
        public async Task<ActionResult<int>> GetCount()
        {
            try
            {
                var count = await _repository.GetCountAsync();

                // ✅ Audit trail completo
                LogAuditTrail("GET_COUNT", "VwArticoliCompleti", count.ToString());
                LogSecurityEvent("VwArticoliCompletiAccessed", new
                {
                    Operation = "GetCount",
                    Count = count,
                    User = User.Identity?.Name ?? "Anonymous",
                    Timestamp = DateTime.UtcNow
                });

                return Ok(count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore nel conteggio articoli");
                return SafeInternalError<int>(ex.Message);
            }
        }
        [HttpGet("categorie")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<string>>> GetCategorie()
        {
            try
            {
                var categorie = await _repository.GetCategorieAsync();
                LogAuditTrail("GET_CATEGORIE", "VwArticoliCompleti", $"Count: {categorie?.Count()}");
                return Ok(categorie);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore nel recupero categorie");
                return SafeInternalError<IEnumerable<string>>(ex.Message);
            }
        }

        [HttpGet("tipi-articolo")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<string>>> GetTipiArticolo()
        {
            try
            {
                var tipi = await _repository.GetTipiArticoloAsync();
                LogAuditTrail("GET_TIPI_ARTICOLO", "VwArticoliCompleti", $"Count: {tipi?.Count()}");
                return Ok(tipi);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore nel recupero tipi articolo");
                return SafeInternalError<IEnumerable<string>>(ex.Message);
            }
        }

        [HttpGet("stats")]
        [AllowAnonymous] // ✅ VISTA PUBBLICA
        public async Task<ActionResult<object>> GetStats()
        {
            try
            {
                var totalCount = await _repository.GetCountAsync();
                var disponibiliCount = (await _repository.GetDisponibiliAsync()).Count();
                var categorie = await _repository.GetCategorieAsync();
                var tipi = await _repository.GetTipiArticoloAsync();

                var stats = new
                {
                    TotaleArticoli = totalCount,
                    ArticoliDisponibili = disponibiliCount,
                    ArticoliNonDisponibili = totalCount - disponibiliCount,
                    NumeroCategorie = categorie.Count(),
                    NumeroTipi = tipi.Count(),
                    UltimoAggiornamento = DateTime.UtcNow
                };

                // ✅ Audit trail completo
                LogAuditTrail("GET_STATS", "VwArticoliCompleti", $"Totali: {totalCount}, Disponibili: {disponibiliCount}");
                LogSecurityEvent("VwArticoliCompletiAccessed", new
                {
                    Operation = "GetStats",
                    TotalCount = totalCount,
                    DisponibiliCount = disponibiliCount,
                    User = User.Identity?.Name ?? "Anonymous",
                    Timestamp = DateTime.UtcNow
                });

                return Ok(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore nel recupero statistiche articoli");
                return SafeInternalError<object>(ex.Message);
            }
        }

        [HttpGet("exists/{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<bool>> Exists(int id)
        {
            try
            {
                if (id <= 0)
                    return SafeBadRequest<bool>("ID articolo non valido");

                var exists = await _repository.ExistsAsync(id);

                LogAuditTrail("CHECK_EXISTS", "VwArticoliCompleti", id.ToString());
                return Ok(exists);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore nella verifica esistenza articolo con ID: {Id}", id);
                return SafeInternalError<bool>(ex.Message);
            }
        }
    }
}