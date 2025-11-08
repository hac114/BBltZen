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
    public class VwMenuDinamicoController : SecureBaseController
    {
        private readonly IVwMenuDinamicoRepository _repository;

        public VwMenuDinamicoController(
            IVwMenuDinamicoRepository repository,
            IWebHostEnvironment environment,
            ILogger<VwMenuDinamicoController> logger)
            : base(environment, logger)
        {
            _repository = repository;
        }

        [HttpGet("menu-completo")]
        [AllowAnonymous] // ✅ MENU PUBBLICO
        public async Task<ActionResult<List<VwMenuDinamicoDTO>>> GetMenuCompleto()
        {
            try
            {
                var menu = await _repository.GetMenuCompletoAsync();

                // ✅ Log per audit
                LogAuditTrail("GET_MENU_COMPLETO", "VwMenuDinamico", $"Count: {menu?.Count}");

                return Ok(menu);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore nel recupero menu completo");
                return SafeInternalError(
                    _environment.IsDevelopment()
                        ? $"Errore nel recupero menu completo: {ex.Message}"
                        : "Errore interno nel recupero menu"
                );
            }
        }

        [HttpGet("primo-piano")]
        [AllowAnonymous] // ✅ MENU PUBBLICO
        public async Task<ActionResult<List<VwMenuDinamicoDTO>>> GetPrimoPiano([FromQuery] int numeroElementi = 6)
        {
            try
            {
                if (numeroElementi <= 0 || numeroElementi > 20)
                    return SafeBadRequest(
                        _environment.IsDevelopment()
                            ? $"Numero elementi non valido: deve essere tra 1 e 20 (ricevuto: {numeroElementi})"
                            : "Numero elementi non valido"
                    );

                var primoPiano = await _repository.GetPrimoPianoAsync(numeroElementi);

                // ✅ Log per audit
                LogAuditTrail("GET_PRIMO_PIANO", "VwMenuDinamico", $"Elementi: {numeroElementi}");

                return Ok(primoPiano);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore nel recupero elementi primo piano");
                return SafeInternalError(
                    _environment.IsDevelopment()
                        ? $"Errore nel recupero elementi primo piano: {ex.Message}"
                        : "Errore interno nel recupero primo piano"
                );
            }
        }

        [HttpGet("disponibili")]
        [AllowAnonymous] // ✅ MENU PUBBLICO
        public async Task<ActionResult<List<VwMenuDinamicoDTO>>> GetBevandeDisponibili()
        {
            try
            {
                var bevande = await _repository.GetBevandeDisponibiliAsync();

                // ✅ Log per audit
                LogAuditTrail("GET_BEVANDE_DISPONIBILI", "VwMenuDinamico", $"Count: {bevande?.Count}");

                return Ok(bevande);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore nel recupero bevande disponibili");
                return SafeInternalError(
                    _environment.IsDevelopment()
                        ? $"Errore nel recupero bevande disponibili: {ex.Message}"
                        : "Errore interno nel recupero bevande disponibili"
                );
            }
        }

        [HttpGet("categoria/{categoria}")]
        [AllowAnonymous] // ✅ MENU PUBBLICO
        public async Task<ActionResult<List<VwMenuDinamicoDTO>>> GetBevandePerCategoria(string categoria)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(categoria))
                    return SafeBadRequest(
                        _environment.IsDevelopment()
                            ? "Categoria non valida: non può essere vuota"
                            : "Categoria non valida"
                    );

                var bevande = await _repository.GetBevandePerCategoriaAsync(categoria);

                // ✅ Log per audit
                LogAuditTrail("GET_BEVANDE_BY_CATEGORIA", "VwMenuDinamico", categoria);

                return Ok(bevande);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, $"Errore nel recupero bevande per categoria: {categoria}");
                return SafeInternalError(
                    _environment.IsDevelopment()
                        ? $"Errore nel recupero bevande per categoria {categoria}: {ex.Message}"
                        : "Errore interno nel recupero bevande per categoria"
                );
            }
        }

        [HttpGet("priorita")]
        [AllowAnonymous] // ✅ MENU PUBBLICO
        public async Task<ActionResult<List<VwMenuDinamicoDTO>>> GetBevandePerPriorita(
            [FromQuery] int prioritaMinima = 0,
            [FromQuery] int prioritaMassima = 10)
        {
            try
            {
                if (prioritaMinima < 0 || prioritaMassima < 0)
                    return SafeBadRequest(
                        _environment.IsDevelopment()
                            ? "Le priorità non possono essere negative"
                            : "Priorità non valide"
                    );

                if (prioritaMassima < prioritaMinima)
                    return SafeBadRequest(
                        _environment.IsDevelopment()
                            ? $"La priorità massima ({prioritaMassima}) non può essere minore della priorità minima ({prioritaMinima})"
                            : "Range priorità non valido"
                    );

                var bevande = await _repository.GetBevandePerPrioritaAsync(prioritaMinima, prioritaMassima);

                // ✅ Log per audit
                LogAuditTrail("GET_BEVANDE_BY_PRIORITA", "VwMenuDinamico", $"{prioritaMinima}-{prioritaMassima}");

                return Ok(bevande);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, $"Errore nel recupero bevande per priorità {prioritaMinima}-{prioritaMassima}");
                return SafeInternalError(
                    _environment.IsDevelopment()
                        ? $"Errore nel recupero bevande per priorità {prioritaMinima}-{prioritaMassima}: {ex.Message}"
                        : "Errore interno nel recupero bevande per priorità"
                );
            }
        }

        [HttpGet("sconti")]
        [AllowAnonymous] // ✅ MENU PUBBLICO
        public async Task<ActionResult<List<VwMenuDinamicoDTO>>> GetBevandeConSconto()
        {
            try
            {
                var bevande = await _repository.GetBevandeConScontoAsync();

                // ✅ Log per audit
                LogAuditTrail("GET_BEVANDE_CON_SCONTO", "VwMenuDinamico", $"Count: {bevande?.Count}");

                return Ok(bevande);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore nel recupero bevande con sconto");
                return SafeInternalError(
                    _environment.IsDevelopment()
                        ? $"Errore nel recupero bevande con sconto: {ex.Message}"
                        : "Errore interno nel recupero bevande con sconto"
                );
            }
        }

        [HttpGet("dettaglio/{tipo}/{id}")]
        [AllowAnonymous] // ✅ MENU PUBBLICO
        public async Task<ActionResult<VwMenuDinamicoDTO>> GetBevandaById(string tipo, int id)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(tipo))
                    return SafeBadRequest(
                        _environment.IsDevelopment()
                            ? "Tipo bevanda non valido: non può essere vuoto"
                            : "Tipo bevanda non valido"
                    );

                if (id <= 0)
                    return SafeBadRequest(
                        _environment.IsDevelopment()
                            ? "ID bevanda non valido: deve essere maggiore di 0"
                            : "ID bevanda non valido"
                    );

                var bevanda = await _repository.GetBevandaByIdAsync(id, tipo);
                if (bevanda == null)
                    return SafeNotFound(
                        _environment.IsDevelopment()
                            ? $"Bevanda con ID {id} e tipo {tipo} non trovata"
                            : "Bevanda non trovata"
                    );

                // ✅ Log per audit
                LogAuditTrail("GET_BEVANDA_BY_ID", "VwMenuDinamico", $"{tipo}_{id}");

                return Ok(bevanda);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, $"Errore nel recupero bevanda ID: {id}, Tipo: {tipo}");
                return SafeInternalError(
                    _environment.IsDevelopment()
                        ? $"Errore nel recupero bevanda ID {id}, Tipo {tipo}: {ex.Message}"
                        : "Errore interno nel recupero bevanda"
                );
            }
        }

        [HttpGet("categorie")]
        [AllowAnonymous] // ✅ MENU PUBBLICO
        public async Task<ActionResult<List<string>>> GetCategorieDisponibili()
        {
            try
            {
                var categorie = await _repository.GetCategorieDisponibiliAsync();

                // ✅ Log per audit
                LogAuditTrail("GET_CATEGORIE_DISPONIBILI", "VwMenuDinamico", $"Count: {categorie?.Count}");

                return Ok(categorie);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore nel recupero categorie disponibili");
                return SafeInternalError(
                    _environment.IsDevelopment()
                        ? $"Errore nel recupero categorie disponibili: {ex.Message}"
                        : "Errore interno nel recupero categorie"
                );
            }
        }

        [HttpGet("cerca")]
        [AllowAnonymous] // ✅ MENU PUBBLICO
        public async Task<ActionResult<List<VwMenuDinamicoDTO>>> SearchBevande([FromQuery] string searchTerm)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                    return SafeBadRequest(
                        _environment.IsDevelopment()
                            ? "Termine di ricerca non valido: non può essere vuoto"
                            : "Termine di ricerca non valido"
                    );

                if (searchTerm.Length < 2)
                    return SafeBadRequest(
                        _environment.IsDevelopment()
                            ? "Il termine di ricerca deve avere almeno 2 caratteri"
                            : "Termine di ricerca troppo corto"
                    );

                var bevande = await _repository.SearchBevandeAsync(searchTerm);

                // ✅ Log per audit
                LogAuditTrail("SEARCH_BEVANDE", "VwMenuDinamico", searchTerm);

                return Ok(bevande);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, $"Errore nella ricerca bevande per: '{searchTerm}'");
                return SafeInternalError(
                    _environment.IsDevelopment()
                        ? $"Errore nella ricerca bevande per '{searchTerm}': {ex.Message}"
                        : "Errore interno nella ricerca bevande"
                );
            }
        }

        [HttpGet("count-disponibili")]
        [AllowAnonymous] // ✅ MENU PUBBLICO
        public async Task<ActionResult<int>> GetCountBevandeDisponibili()
        {
            try
            {
                var count = await _repository.GetCountBevandeDisponibiliAsync();

                // ✅ Log per audit
                LogAuditTrail("GET_COUNT_BEVANDE_DISPONIBILI", "VwMenuDinamico", count.ToString());

                return Ok(count);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore nel conteggio bevande disponibili");
                return SafeInternalError(
                    _environment.IsDevelopment()
                        ? $"Errore nel conteggio bevande disponibili: {ex.Message}"
                        : "Errore interno nel conteggio bevande"
                );
            }
        }

        [HttpGet("stats")]
        [AllowAnonymous] // ✅ MENU PUBBLICO
        public async Task<ActionResult<object>> GetStats()
        {
            try
            {
                var menuCompleto = await _repository.GetMenuCompletoAsync();
                var primoPiano = await _repository.GetPrimoPianoAsync();
                var disponibili = await _repository.GetBevandeDisponibiliAsync();
                var categorie = await _repository.GetCategorieDisponibiliAsync();
                var countDisponibili = await _repository.GetCountBevandeDisponibiliAsync();
                var bevandeConSconto = await _repository.GetBevandeConScontoAsync();

                var stats = new
                {
                    TotaleMenu = menuCompleto.Count,
                    PrimoPiano = primoPiano.Count,
                    Disponibili = disponibili.Count,
                    NonDisponibili = menuCompleto.Count - disponibili.Count,
                    NumeroCategorie = categorie.Count,
                    BevandeConSconto = bevandeConSconto.Count,
                    PercentualeDisponibili = menuCompleto.Count > 0 ?
                        Math.Round((double)disponibili.Count / menuCompleto.Count * 100, 2) : 0,
                    Categorie = categorie,
                    UltimoAggiornamento = DateTime.Now
                };

                // ✅ Log per audit
                LogAuditTrail("GET_MENU_STATS", "VwMenuDinamico",
                    $"Totali: {menuCompleto.Count}, Disponibili: {disponibili.Count}");

                return Ok(stats);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore nel recupero statistiche menu");
                return SafeInternalError(
                    _environment.IsDevelopment()
                        ? $"Errore nel recupero statistiche menu: {ex.Message}"
                        : "Errore interno nel recupero statistiche menu"
                );
            }
        }

        [HttpGet("health")]
        [AllowAnonymous] // ✅ HEALTH CHECK PUBBLICO
        public async Task<ActionResult<object>> HealthCheck()
        {
            try
            {
                var menuCompleto = await _repository.GetMenuCompletoAsync();
                var categorie = await _repository.GetCategorieDisponibiliAsync();
                var countDisponibili = await _repository.GetCountBevandeDisponibiliAsync();

                var health = new
                {
                    Status = "Healthy",
                    Timestamp = DateTime.Now,
                    Menu = new
                    {
                        TotaleElementi = menuCompleto.Count,
                        BevandeDisponibili = countDisponibili,
                        CategorieAttive = categorie.Count
                    },
                    Database = new
                    {
                        Connessione = "OK",
                        Vista = "VwMenuDinamico accessibile"
                    }
                };

                // ✅ Log per audit
                LogAuditTrail("HEALTH_CHECK", "VwMenuDinamico", "OK");

                return Ok(health);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore nel health check menu dinamico");

                // ✅ Log per audit anche in caso di errore
                LogAuditTrail("HEALTH_CHECK_FAILED", "VwMenuDinamico", ex.Message);

                return SafeInternalError(
                    _environment.IsDevelopment()
                        ? $"Health check fallito: {ex.Message}"
                        : "Health check fallito"
                );
            }
        }
    }
}