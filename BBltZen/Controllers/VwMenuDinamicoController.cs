// BBltZen/Controllers/VwMenuDinamicoController.cs
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

                // ✅ Audit trail completo
                LogAuditTrail("GET_MENU_COMPLETO", "VwMenuDinamico", $"Count: {menu?.Count()}");
                LogSecurityEvent("VwMenuDinamicoAccessed", new
                {
                    Operation = "GetMenuCompleto",
                    Count = menu?.Count() ?? 0,
                    User = User.Identity?.Name ?? "Anonymous", 
                    Timestamp = DateTime.UtcNow
                });

                return Ok(menu);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore nel recupero menu completo");
                return SafeInternalError<List<VwMenuDinamicoDTO>>(ex.Message);
            }
        }

        [HttpGet("primo-piano")]
        [AllowAnonymous] // ✅ MENU PUBBLICO
        public async Task<ActionResult<List<VwMenuDinamicoDTO>>> GetPrimoPiano([FromQuery] int numeroElementi = 6)
        {
            try
            {
                if (numeroElementi <= 0 || numeroElementi > 20)
                    return SafeBadRequest<List<VwMenuDinamicoDTO>>("Numero elementi non valido: deve essere tra 1 e 20");

                var primoPiano = await _repository.GetPrimoPianoAsync(numeroElementi);

                // ✅ Audit trail completo
                LogAuditTrail("GET_PRIMO_PIANO", "VwMenuDinamico", $"Elementi: {numeroElementi}");
                LogSecurityEvent("VwMenuDinamicoAccessed", new
                {
                    Operation = "GetPrimoPiano",
                    NumeroElementi = numeroElementi,  
                    Count = primoPiano?.Count() ?? 0,  
                    User = User.Identity?.Name ?? "Anonymous",
                    Timestamp = DateTime.UtcNow 
                });

                return Ok(primoPiano);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore nel recupero elementi primo piano");
                return SafeInternalError<List<VwMenuDinamicoDTO>>(ex.Message);
            }
        }

        [HttpGet("disponibili")]
        [AllowAnonymous] // ✅ MENU PUBBLICO
        public async Task<ActionResult<List<VwMenuDinamicoDTO>>> GetBevandeDisponibili()
        {
            try
            {
                var bevande = await _repository.GetBevandeDisponibiliAsync();

                // ✅ Audit trail completo
                LogAuditTrail("GET_BEVANDE_DISPONIBILI", "VwMenuDinamico", $"Count: {bevande?.Count()}");
                LogSecurityEvent("VwMenuDinamicoAccessed", new
                {
                    Operation = "GetBevandeDisponibili",
                    Count = bevande?.Count() ?? 0,
                    User = User.Identity?.Name ?? "Anonymous",
                    Timestamp = DateTime.UtcNow
                });

                return Ok(bevande);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore nel recupero bevande disponibili");
                return SafeInternalError<List<VwMenuDinamicoDTO>>(ex.Message);
            }
        }

        [HttpGet("categoria/{categoria}")]
        [AllowAnonymous] // ✅ MENU PUBBLICO
        public async Task<ActionResult<List<VwMenuDinamicoDTO>>> GetBevandePerCategoria(string categoria)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(categoria))
                    return SafeBadRequest<List<VwMenuDinamicoDTO>>("Categoria non valida");

                var bevande = await _repository.GetBevandePerCategoriaAsync(categoria);

                // ✅ Audit trail completo
                LogAuditTrail("GET_BEVANDE_BY_CATEGORIA", "VwMenuDinamico", categoria);
                LogSecurityEvent("VwMenuDinamicoAccessed", new
                {
                    Operation = "GetBevandePerCategoria",
                    Categoria = categoria,
                    Count = bevande?.Count() ?? 0,
                    User = User.Identity?.Name ?? "Anonymous",
                    Timestamp = DateTime.UtcNow
                });

                return Ok(bevande);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore nel recupero bevande per categoria: {Categoria}", categoria);
                return SafeInternalError<List<VwMenuDinamicoDTO>>(ex.Message);
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
                    return SafeBadRequest<List<VwMenuDinamicoDTO>>("Le priorità non possono essere negative");

                if (prioritaMassima < prioritaMinima)
                    return SafeBadRequest<List<VwMenuDinamicoDTO>>("La priorità massima non può essere minore della priorità minima");

                var bevande = await _repository.GetBevandePerPrioritaAsync(prioritaMinima, prioritaMassima);

                // ✅ Audit trail completo
                LogAuditTrail("GET_BEVANDE_BY_PRIORITA", "VwMenuDinamico", $"{prioritaMinima}-{prioritaMassima}");
                LogSecurityEvent("VwMenuDinamicoAccessed", new
                {
                    Operation = "GetBevandePerPriorita",
                    PrioritaMinima = prioritaMinima,
                    PrioritaMassima = prioritaMassima,
                    Count = bevande?.Count() ?? 0,
                    User = User.Identity?.Name ?? "Anonymous",
                    Timestamp = DateTime.UtcNow
                });

                return Ok(bevande);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore nel recupero bevande per priorità {PrioritaMinima}-{PrioritaMassima}", prioritaMinima, prioritaMassima);
                return SafeInternalError<List<VwMenuDinamicoDTO>>(ex.Message);
            }
        }

        [HttpGet("sconti")]
        [AllowAnonymous] // ✅ MENU PUBBLICO
        public async Task<ActionResult<List<VwMenuDinamicoDTO>>> GetBevandeConSconto()
        {
            try
            {
                var bevande = await _repository.GetBevandeConScontoAsync();

                // ✅ Audit trail completo
                LogAuditTrail("GET_BEVANDE_CON_SCONTO", "VwMenuDinamico", $"Count: {bevande?.Count()}");
                LogSecurityEvent("VwMenuDinamicoAccessed", new
                {
                    Operation = "GetBevandeConSconto",
                    Count = bevande?.Count() ?? 0,
                    User = User.Identity?.Name ?? "Anonymous",
                    Timestamp = DateTime.UtcNow
                });

                return Ok(bevande);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore nel recupero bevande con sconto");
                return SafeInternalError<List<VwMenuDinamicoDTO>>(ex.Message);
            }
        }

        [HttpGet("dettaglio/{tipo}/{id}")]
        [AllowAnonymous] // ✅ MENU PUBBLICO
        public async Task<ActionResult<VwMenuDinamicoDTO>> GetBevandaById(string tipo, int id)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(tipo))
                    return SafeBadRequest<VwMenuDinamicoDTO>("Tipo bevanda non valido");

                if (id <= 0)
                    return SafeBadRequest<VwMenuDinamicoDTO>("ID bevanda non valido");

                var bevanda = await _repository.GetBevandaByIdAsync(id, tipo);
                if (bevanda == null)
                    return SafeNotFound<VwMenuDinamicoDTO>("Bevanda");

                // ✅ Audit trail completo
                LogAuditTrail("GET_BEVANDA_BY_ID", "VwMenuDinamico", $"{tipo}_{id}");
                LogSecurityEvent("VwMenuDinamicoAccessed", new
                {
                    Operation = "GetBevandaById",
                    Tipo = tipo,
                    Id = id,
                    User = User.Identity?.Name ?? "Anonymous",
                    Timestamp = DateTime.UtcNow
                });

                return Ok(bevanda);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore nel recupero bevanda ID: {Id}, Tipo: {Tipo}", id, tipo);
                return SafeInternalError<VwMenuDinamicoDTO>(ex.Message);
            }
        }

        [HttpGet("categorie")]
        [AllowAnonymous] // ✅ MENU PUBBLICO
        public async Task<ActionResult<List<string>>> GetCategorieDisponibili()
        {
            try
            {
                var categorie = await _repository.GetCategorieDisponibiliAsync();

                // ✅ Audit trail completo
                LogAuditTrail("GET_CATEGORIE_DISPONIBILI", "VwMenuDinamico", $"Count: {categorie?.Count()}");
                LogSecurityEvent("VwMenuDinamicoAccessed", new
                {
                    Operation = "GetCategorieDisponibili",
                    Count = categorie?.Count() ?? 0,
                    User = User.Identity?.Name ?? "Anonymous",
                    Timestamp = DateTime.UtcNow
                });

                return Ok(categorie);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore nel recupero categorie disponibili");
                return SafeInternalError<List<string>>(ex.Message);
            }
        }

        [HttpGet("cerca")]
        [AllowAnonymous] // ✅ MENU PUBBLICO
        public async Task<ActionResult<List<VwMenuDinamicoDTO>>> SearchBevande([FromQuery] string searchTerm)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                    return SafeBadRequest<List<VwMenuDinamicoDTO>>("Termine di ricerca non valido");

                if (searchTerm.Length < 2)
                    return SafeBadRequest<List<VwMenuDinamicoDTO>>("Il termine di ricerca deve avere almeno 2 caratteri");

                var bevande = await _repository.SearchBevandeAsync(searchTerm);

                // ✅ Audit trail completo
                LogAuditTrail("SEARCH_BEVANDE", "VwMenuDinamico", searchTerm);
                LogSecurityEvent("VwMenuDinamicoAccessed", new
                {
                    Operation = "SearchBevande",
                    SearchTerm = searchTerm,
                    Count = bevande?.Count() ?? 0,
                    User = User.Identity?.Name ?? "Anonymous",
                    Timestamp = DateTime.UtcNow
                });

                return Ok(bevande);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore nella ricerca bevande per: '{SearchTerm}'", searchTerm);
                return SafeInternalError<List<VwMenuDinamicoDTO>>(ex.Message);
            }
        }

        [HttpGet("count-disponibili")]
        [AllowAnonymous] // ✅ MENU PUBBLICO
        public async Task<ActionResult<int>> GetCountBevandeDisponibili()
        {
            try
            {
                var count = await _repository.GetCountBevandeDisponibiliAsync();

                // ✅ Audit trail completo
                LogAuditTrail("GET_COUNT_BEVANDE_DISPONIBILI", "VwMenuDinamico", count.ToString());
                LogSecurityEvent("VwMenuDinamicoAccessed", new
                {
                    Operation = "GetCountBevandeDisponibili",
                    Count = count,
                    User = User.Identity?.Name ?? "Anonymous",
                    Timestamp = DateTime.UtcNow
                });

                return Ok(count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore nel conteggio bevande disponibili");
                return SafeInternalError<int>(ex.Message);
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
                    TotaleMenu = menuCompleto.Count(),
                    PrimoPiano = primoPiano.Count(),
                    Disponibili = disponibili.Count(),
                    NonDisponibili = menuCompleto.Count() - disponibili.Count(),
                    NumeroCategorie = categorie.Count(),
                    BevandeConSconto = bevandeConSconto.Count(),
                    PercentualeDisponibili = menuCompleto.Count() > 0 ?
                        Math.Round((double)disponibili.Count() / menuCompleto.Count() * 100, 2) : 0,
                    Categorie = categorie,
                    UltimoAggiornamento = DateTime.UtcNow
                };

                // ✅ Audit trail completo
                LogAuditTrail("GET_MENU_STATS", "VwMenuDinamico", $"Totali: {menuCompleto.Count()}, Disponibili: {disponibili.Count()}");
                LogSecurityEvent("VwMenuDinamicoAccessed", new
                {
                    Operation = "GetStats",
                    TotalCount = menuCompleto.Count(),
                    DisponibiliCount = disponibili.Count(),
                    User = User.Identity?.Name ?? "Anonymous",
                    Timestamp = DateTime.UtcNow
                });

                return Ok(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore nel recupero statistiche menu");
                return SafeInternalError<object>(ex.Message);
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
                    Timestamp = DateTime.UtcNow,
                    Menu = new
                    {
                        TotaleElementi = menuCompleto.Count(),
                        BevandeDisponibili = countDisponibili,
                        CategorieAttive = categorie.Count()
                    },
                    Database = new
                    {
                        Connessione = "OK",
                        Vista = "VwMenuDinamico accessibile"
                    }
                };

                // ✅ Audit trail completo
                LogAuditTrail("HEALTH_CHECK", "VwMenuDinamico", "OK");
                LogSecurityEvent("VwMenuDinamicoHealthCheck", new
                {
                    Status = "Healthy",
                    User = User.Identity?.Name ?? "Anonymous",
                    Timestamp = DateTime.UtcNow
                });

                return Ok(health);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore nel health check menu dinamico");

                // ✅ Audit trail anche in caso di errore
                LogAuditTrail("HEALTH_CHECK_FAILED", "VwMenuDinamico", ex.Message);
                LogSecurityEvent("VwMenuDinamicoHealthCheck", new
                {
                    Status = "Unhealthy",
                    Error = ex.Message,
                    User = User.Identity?.Name ?? "Anonymous",
                    Timestamp = DateTime.UtcNow
                });

                return SafeInternalError<object>(ex.Message);
            }
        }
    }
}