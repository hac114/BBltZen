using DTO;
using Microsoft.AspNetCore.Mvc;
using Repository.Interface;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VwMenuDinamicoController : ControllerBase
    {
        private readonly IVwMenuDinamicoRepository _repository;
        private readonly ILogger<VwMenuDinamicoController> _logger;

        public VwMenuDinamicoController(
            IVwMenuDinamicoRepository repository,
            ILogger<VwMenuDinamicoController> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        [HttpGet("menu-completo")]
        public async Task<ActionResult<List<VwMenuDinamicoDTO>>> GetMenuCompleto()
        {
            try
            {
                var menu = await _repository.GetMenuCompletoAsync();
                return Ok(menu);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore nel recupero menu completo");
                return StatusCode(500, "Errore interno nel recupero menu completo");
            }
        }

        [HttpGet("primo-piano")]
        public async Task<ActionResult<List<VwMenuDinamicoDTO>>> GetPrimoPiano([FromQuery] int numeroElementi = 6)
        {
            try
            {
                if (numeroElementi <= 0 || numeroElementi > 20)
                    return BadRequest("Il numero di elementi deve essere tra 1 e 20");

                var primoPiano = await _repository.GetPrimoPianoAsync(numeroElementi);
                return Ok(primoPiano);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore nel recupero elementi primo piano");
                return StatusCode(500, "Errore interno nel recupero elementi primo piano");
            }
        }

        [HttpGet("disponibili")]
        public async Task<ActionResult<List<VwMenuDinamicoDTO>>> GetBevandeDisponibili()
        {
            try
            {
                var bevande = await _repository.GetBevandeDisponibiliAsync();
                return Ok(bevande);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore nel recupero bevande disponibili");
                return StatusCode(500, "Errore interno nel recupero bevande disponibili");
            }
        }

        [HttpGet("categoria/{categoria}")]
        public async Task<ActionResult<List<VwMenuDinamicoDTO>>> GetBevandePerCategoria(string categoria)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(categoria))
                    return BadRequest("La categoria non può essere vuota");

                var bevande = await _repository.GetBevandePerCategoriaAsync(categoria);
                return Ok(bevande);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Errore nel recupero bevande per categoria: {categoria}");
                return StatusCode(500, $"Errore interno nel recupero bevande per categoria {categoria}");
            }
        }

        [HttpGet("priorita")]
        public async Task<ActionResult<List<VwMenuDinamicoDTO>>> GetBevandePerPriorita(
            [FromQuery] int prioritaMinima = 0,
            [FromQuery] int prioritaMassima = 10)
        {
            try
            {
                if (prioritaMinima < 0 || prioritaMassima < prioritaMinima)
                    return BadRequest("Range priorità non valido");

                var bevande = await _repository.GetBevandePerPrioritaAsync(prioritaMinima, prioritaMassima);
                return Ok(bevande);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Errore nel recupero bevande per priorità {prioritaMinima}-{prioritaMassima}");
                return StatusCode(500, $"Errore interno nel recupero bevande per priorità {prioritaMinima}-{prioritaMassima}");
            }
        }

        [HttpGet("sconti")]
        public async Task<ActionResult<List<VwMenuDinamicoDTO>>> GetBevandeConSconto()
        {
            try
            {
                var bevande = await _repository.GetBevandeConScontoAsync();
                return Ok(bevande);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore nel recupero bevande con sconto");
                return StatusCode(500, "Errore interno nel recupero bevande con sconto");
            }
        }

        [HttpGet("dettaglio/{tipo}/{id}")]
        public async Task<ActionResult<VwMenuDinamicoDTO>> GetBevandaById(string tipo, int id)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(tipo))
                    return BadRequest("Il tipo non può essere vuoto");

                var bevanda = await _repository.GetBevandaByIdAsync(id, tipo);
                if (bevanda == null)
                    return NotFound($"Bevanda con ID {id} e tipo {tipo} non trovata");

                return Ok(bevanda);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Errore nel recupero bevanda ID: {id}, Tipo: {tipo}");
                return StatusCode(500, $"Errore interno nel recupero bevanda ID: {id}, Tipo: {tipo}");
            }
        }

        [HttpGet("categorie")]
        public async Task<ActionResult<List<string>>> GetCategorieDisponibili()
        {
            try
            {
                var categorie = await _repository.GetCategorieDisponibiliAsync();
                return Ok(categorie);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore nel recupero categorie disponibili");
                return StatusCode(500, "Errore interno nel recupero categorie disponibili");
            }
        }

        [HttpGet("cerca")]
        public async Task<ActionResult<List<VwMenuDinamicoDTO>>> SearchBevande([FromQuery] string searchTerm)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm) || searchTerm.Length < 2)
                    return BadRequest("Il termine di ricerca deve avere almeno 2 caratteri");

                var bevande = await _repository.SearchBevandeAsync(searchTerm);
                return Ok(bevande);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Errore nella ricerca bevande per: '{searchTerm}'");
                return StatusCode(500, $"Errore interno nella ricerca bevande per: '{searchTerm}'");
            }
        }

        [HttpGet("count-disponibili")]
        public async Task<ActionResult<int>> GetCountBevandeDisponibili()
        {
            try
            {
                var count = await _repository.GetCountBevandeDisponibiliAsync();
                return Ok(count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore nel conteggio bevande disponibili");
                return StatusCode(500, "Errore interno nel conteggio bevande disponibili");
            }
        }

        [HttpGet("stats")]
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

                return Ok(new
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
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore nel recupero statistiche menu");
                return StatusCode(500, "Errore interno nel recupero statistiche menu");
            }
        }

        [HttpGet("health")]
        public async Task<ActionResult<object>> HealthCheck()
        {
            try
            {
                var menuCompleto = await _repository.GetMenuCompletoAsync();
                var categorie = await _repository.GetCategorieDisponibiliAsync();
                var countDisponibili = await _repository.GetCountBevandeDisponibiliAsync();

                return Ok(new
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
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore nel health check menu dinamico");
                return StatusCode(500, new
                {
                    Status = "Unhealthy",
                    Timestamp = DateTime.Now,
                    Error = ex.Message
                });
            }
        }
    }
}