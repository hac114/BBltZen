using Microsoft.AspNetCore.Mvc;
using DTO;
using Repository.Interface;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;

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
        [AllowAnonymous] // ✅ VISTA PUBBLICA
        public async Task<ActionResult<List<VwArticoliCompletiDTO>>> GetAll()
        {
            try
            {
                var articoli = await _repository.GetAllAsync();

                // ✅ Log per audit
                LogAuditTrail("GET_ALL_ARTICOLI_COMPLETI", "VwArticoliCompleti", $"Count: {articoli?.Count}");

                return Ok(articoli);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore nel recupero di tutti gli articoli completi");
                return SafeInternalError(
                    _environment.IsDevelopment()
                        ? $"Errore nel recupero degli articoli completi: {ex.Message}"
                        : "Errore interno nel recupero articoli"
                );
            }
        }

        [HttpGet("{id}")]
        [AllowAnonymous] // ✅ VISTA PUBBLICA
        public async Task<ActionResult<VwArticoliCompletiDTO>> GetById(int id)
        {
            try
            {
                if (id <= 0)
                    return SafeBadRequest(
                        _environment.IsDevelopment()
                            ? "ID articolo non valido: deve essere maggiore di 0"
                            : "ID articolo non valido"
                    );

                var articolo = await _repository.GetByIdAsync(id);
                if (articolo == null)
                    return SafeNotFound(
                        _environment.IsDevelopment()
                            ? $"Articolo completo con ID {id} non trovato"
                            : "Articolo non trovato"
                    );

                // ✅ Log per audit
                LogAuditTrail("GET_ARTICOLO_COMPLETO_BY_ID", "VwArticoliCompleti", id.ToString());

                return Ok(articolo);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, $"Errore nel recupero articolo con ID: {id}");
                return SafeInternalError(
                    _environment.IsDevelopment()
                        ? $"Errore nel recupero articolo con ID {id}: {ex.Message}"
                        : "Errore interno nel recupero articolo"
                );
            }
        }

        [HttpGet("tipo/{tipoArticolo}")]
        [AllowAnonymous] // ✅ VISTA PUBBLICA
        public async Task<ActionResult<List<VwArticoliCompletiDTO>>> GetByTipo(string tipoArticolo)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(tipoArticolo))
                    return SafeBadRequest(
                        _environment.IsDevelopment()
                            ? "Tipo articolo non valido: non può essere vuoto"
                            : "Tipo articolo non valido"
                    );

                var articoli = await _repository.GetByTipoAsync(tipoArticolo);

                // ✅ Log per audit
                LogAuditTrail("GET_ARTICOLI_BY_TIPO", "VwArticoliCompleti", tipoArticolo);

                return Ok(articoli);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, $"Errore nel recupero articoli di tipo: {tipoArticolo}");
                return SafeInternalError(
                    _environment.IsDevelopment()
                        ? $"Errore nel recupero articoli di tipo {tipoArticolo}: {ex.Message}"
                        : "Errore interno nel recupero articoli per tipo"
                );
            }
        }

        [HttpGet("categoria/{categoria}")]
        [AllowAnonymous] // ✅ VISTA PUBBLICA
        public async Task<ActionResult<List<VwArticoliCompletiDTO>>> GetByCategoria(string categoria)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(categoria))
                    return SafeBadRequest(
                        _environment.IsDevelopment()
                            ? "Categoria non valida: non può essere vuota"
                            : "Categoria non valida"
                    );

                var articoli = await _repository.GetByCategoriaAsync(categoria);

                // ✅ Log per audit
                LogAuditTrail("GET_ARTICOLI_BY_CATEGORIA", "VwArticoliCompleti", categoria);

                return Ok(articoli);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, $"Errore nel recupero articoli della categoria: {categoria}");
                return SafeInternalError(
                    _environment.IsDevelopment()
                        ? $"Errore nel recupero articoli della categoria {categoria}: {ex.Message}"
                        : "Errore interno nel recupero articoli per categoria"
                );
            }
        }

        [HttpGet("disponibili")]
        [AllowAnonymous] // ✅ VISTA PUBBLICA
        public async Task<ActionResult<List<VwArticoliCompletiDTO>>> GetDisponibili()
        {
            try
            {
                var articoli = await _repository.GetDisponibiliAsync();

                // ✅ Log per audit
                LogAuditTrail("GET_ARTICOLI_DISPONIBILI", "VwArticoliCompleti", $"Count: {articoli?.Count}");

                return Ok(articoli);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore nel recupero articoli disponibili");
                return SafeInternalError(
                    _environment.IsDevelopment()
                        ? $"Errore nel recupero articoli disponibili: {ex.Message}"
                        : "Errore interno nel recupero articoli disponibili"
                );
            }
        }

        [HttpGet("cerca/{nome}")]
        [AllowAnonymous] // ✅ VISTA PUBBLICA
        public async Task<ActionResult<List<VwArticoliCompletiDTO>>> SearchByName(string nome)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(nome))
                    return SafeBadRequest(
                        _environment.IsDevelopment()
                            ? "Nome ricerca non valido: non può essere vuoto"
                            : "Nome ricerca non valido"
                    );

                var articoli = await _repository.SearchByNameAsync(nome);

                // ✅ Log per audit
                LogAuditTrail("SEARCH_ARTICOLI_BY_NAME", "VwArticoliCompleti", nome);

                return Ok(articoli);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, $"Errore nella ricerca articoli per nome: {nome}");
                return SafeInternalError(
                    _environment.IsDevelopment()
                        ? $"Errore nella ricerca articoli per nome {nome}: {ex.Message}"
                        : "Errore interno nella ricerca articoli"
                );
            }
        }

        [HttpGet("prezzo")]
        [AllowAnonymous] // ✅ VISTA PUBBLICA
        public async Task<ActionResult<List<VwArticoliCompletiDTO>>> GetByPriceRange(
            [FromQuery] decimal prezzoMin,
            [FromQuery] decimal prezzoMax)
        {
            try
            {
                if (prezzoMin < 0 || prezzoMax < 0)
                    return SafeBadRequest(
                        _environment.IsDevelopment()
                            ? "I prezzi non possono essere negativi"
                            : "Prezzi non validi"
                    );

                if (prezzoMin > prezzoMax)
                    return SafeBadRequest(
                        _environment.IsDevelopment()
                            ? $"Il prezzo minimo ({prezzoMin}) non può essere maggiore del prezzo massimo ({prezzoMax})"
                            : "Intervallo prezzi non valido"
                    );

                var articoli = await _repository.GetByPriceRangeAsync(prezzoMin, prezzoMax);

                // ✅ Log per audit
                LogAuditTrail("GET_ARTICOLI_BY_PRICE_RANGE", "VwArticoliCompleti", $"{prezzoMin}-{prezzoMax}");

                return Ok(articoli);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, $"Errore nel recupero articoli nel range prezzi {prezzoMin}-{prezzoMax}");
                return SafeInternalError(
                    _environment.IsDevelopment()
                        ? $"Errore nel recupero articoli nel range prezzi {prezzoMin}-{prezzoMax}: {ex.Message}"
                        : "Errore interno nel recupero articoli per prezzo"
                );
            }
        }

        [HttpGet("con-iva")]
        [AllowAnonymous] // ✅ VISTA PUBBLICA
        public async Task<ActionResult<List<VwArticoliCompletiDTO>>> GetArticoliConIva()
        {
            try
            {
                var articoli = await _repository.GetArticoliConIvaAsync();

                // ✅ Log per audit
                LogAuditTrail("GET_ARTICOLI_CON_IVA", "VwArticoliCompleti", $"Count: {articoli?.Count}");

                return Ok(articoli);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore nel recupero articoli con IVA");
                return SafeInternalError(
                    _environment.IsDevelopment()
                        ? $"Errore nel recupero articoli con IVA: {ex.Message}"
                        : "Errore interno nel recupero articoli con IVA"
                );
            }
        }

        [HttpGet("count")]
        [AllowAnonymous] // ✅ VISTA PUBBLICA
        public async Task<ActionResult<int>> GetCount()
        {
            try
            {
                var count = await _repository.GetCountAsync();

                // ✅ Log per audit
                LogAuditTrail("GET_ARTICOLI_COUNT", "VwArticoliCompleti", count.ToString());

                return Ok(count);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore nel conteggio articoli");
                return SafeInternalError(
                    _environment.IsDevelopment()
                        ? $"Errore nel conteggio articoli: {ex.Message}"
                        : "Errore interno nel conteggio articoli"
                );
            }
        }

        [HttpGet("categorie")]
        [AllowAnonymous] // ✅ VISTA PUBBLICA
        public async Task<ActionResult<List<string>>> GetCategorie()
        {
            try
            {
                var categorie = await _repository.GetCategorieAsync();

                // ✅ Log per audit
                LogAuditTrail("GET_CATEGORIE_ARTICOLI", "VwArticoliCompleti", $"Count: {categorie?.Count}");

                return Ok(categorie);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore nel recupero categorie");
                return SafeInternalError(
                    _environment.IsDevelopment()
                        ? $"Errore nel recupero categorie: {ex.Message}"
                        : "Errore interno nel recupero categorie"
                );
            }
        }

        [HttpGet("tipi-articolo")]
        [AllowAnonymous] // ✅ VISTA PUBBLICA
        public async Task<ActionResult<List<string>>> GetTipiArticolo()
        {
            try
            {
                var tipi = await _repository.GetTipiArticoloAsync();

                // ✅ Log per audit
                LogAuditTrail("GET_TIPI_ARTICOLO", "VwArticoliCompleti", $"Count: {tipi?.Count}");

                return Ok(tipi);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore nel recupero tipi articolo");
                return SafeInternalError(
                    _environment.IsDevelopment()
                        ? $"Errore nel recupero tipi articolo: {ex.Message}"
                        : "Errore interno nel recupero tipi articolo"
                );
            }
        }

        [HttpGet("stats")]
        [AllowAnonymous] // ✅ VISTA PUBBLICA
        public async Task<ActionResult<object>> GetStats()
        {
            try
            {
                var totalCount = await _repository.GetCountAsync();
                var disponibiliCount = (await _repository.GetDisponibiliAsync()).Count;
                var categorie = await _repository.GetCategorieAsync();
                var tipi = await _repository.GetTipiArticoloAsync();

                var stats = new
                {
                    TotaleArticoli = totalCount,
                    ArticoliDisponibili = disponibiliCount,
                    ArticoliNonDisponibili = totalCount - disponibiliCount,
                    NumeroCategorie = categorie.Count,
                    NumeroTipi = tipi.Count,
                    UltimoAggiornamento = DateTime.Now
                };

                // ✅ Log per audit
                LogAuditTrail("GET_ARTICOLI_STATS", "VwArticoliCompleti",
                    $"Totali: {totalCount}, Disponibili: {disponibiliCount}");

                return Ok(stats);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore nel recupero statistiche articoli");
                return SafeInternalError(
                    _environment.IsDevelopment()
                        ? $"Errore nel recupero statistiche articoli: {ex.Message}"
                        : "Errore interno nel recupero statistiche articoli"
                );
            }
        }
    }
}