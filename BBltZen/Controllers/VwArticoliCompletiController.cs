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
    public class VwArticoliCompletiController : ControllerBase
    {
        private readonly IVwArticoliCompletiRepository _repository;
        private readonly ILogger<VwArticoliCompletiController> _logger;

        public VwArticoliCompletiController(
            IVwArticoliCompletiRepository repository,
            ILogger<VwArticoliCompletiController> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<List<VwArticoliCompletiDTO>>> GetAll()
        {
            try
            {
                var articoli = await _repository.GetAllAsync();
                return Ok(articoli);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore nel recupero di tutti gli articoli completi");
                return StatusCode(500, "Errore interno nel recupero articoli");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<VwArticoliCompletiDTO>> GetById(int id)
        {
            try
            {
                var articolo = await _repository.GetByIdAsync(id);
                if (articolo == null)
                    return NotFound($"Articolo con ID {id} non trovato");

                return Ok(articolo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Errore nel recupero articolo con ID: {id}");
                return StatusCode(500, $"Errore interno nel recupero articolo con ID {id}");
            }
        }

        [HttpGet("tipo/{tipoArticolo}")]
        public async Task<ActionResult<List<VwArticoliCompletiDTO>>> GetByTipo(string tipoArticolo)
        {
            try
            {
                var articoli = await _repository.GetByTipoAsync(tipoArticolo);
                return Ok(articoli);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Errore nel recupero articoli di tipo: {tipoArticolo}");
                return StatusCode(500, $"Errore interno nel recupero articoli di tipo {tipoArticolo}");
            }
        }

        [HttpGet("categoria/{categoria}")]
        public async Task<ActionResult<List<VwArticoliCompletiDTO>>> GetByCategoria(string categoria)
        {
            try
            {
                var articoli = await _repository.GetByCategoriaAsync(categoria);
                return Ok(articoli);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Errore nel recupero articoli della categoria: {categoria}");
                return StatusCode(500, $"Errore interno nel recupero articoli della categoria {categoria}");
            }
        }

        [HttpGet("disponibili")]
        public async Task<ActionResult<List<VwArticoliCompletiDTO>>> GetDisponibili()
        {
            try
            {
                var articoli = await _repository.GetDisponibiliAsync();
                return Ok(articoli);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore nel recupero articoli disponibili");
                return StatusCode(500, "Errore interno nel recupero articoli disponibili");
            }
        }

        [HttpGet("cerca/{nome}")]
        public async Task<ActionResult<List<VwArticoliCompletiDTO>>> SearchByName(string nome)
        {
            try
            {
                var articoli = await _repository.SearchByNameAsync(nome);
                return Ok(articoli);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Errore nella ricerca articoli per nome: {nome}");
                return StatusCode(500, $"Errore interno nella ricerca articoli per nome {nome}");
            }
        }

        [HttpGet("prezzo")]
        public async Task<ActionResult<List<VwArticoliCompletiDTO>>> GetByPriceRange(
            [FromQuery] decimal prezzoMin,
            [FromQuery] decimal prezzoMax)
        {
            try
            {
                if (prezzoMin > prezzoMax)
                    return BadRequest("Il prezzo minimo non può essere maggiore del prezzo massimo");

                var articoli = await _repository.GetByPriceRangeAsync(prezzoMin, prezzoMax);
                return Ok(articoli);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Errore nel recupero articoli nel range prezzi {prezzoMin}-{prezzoMax}");
                return StatusCode(500, $"Errore interno nel recupero articoli nel range prezzi {prezzoMin}-{prezzoMax}");
            }
        }

        [HttpGet("con-iva")]
        public async Task<ActionResult<List<VwArticoliCompletiDTO>>> GetArticoliConIva()
        {
            try
            {
                var articoli = await _repository.GetArticoliConIvaAsync();
                return Ok(articoli);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore nel recupero articoli con IVA");
                return StatusCode(500, "Errore interno nel recupero articoli con IVA");
            }
        }

        [HttpGet("count")]
        public async Task<ActionResult<int>> GetCount()
        {
            try
            {
                var count = await _repository.GetCountAsync();
                return Ok(count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore nel conteggio articoli");
                return StatusCode(500, "Errore interno nel conteggio articoli");
            }
        }

        [HttpGet("categorie")]
        public async Task<ActionResult<List<string>>> GetCategorie()
        {
            try
            {
                var categorie = await _repository.GetCategorieAsync();
                return Ok(categorie);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore nel recupero categorie");
                return StatusCode(500, "Errore interno nel recupero categorie");
            }
        }

        [HttpGet("tipi-articolo")]
        public async Task<ActionResult<List<string>>> GetTipiArticolo()
        {
            try
            {
                var tipi = await _repository.GetTipiArticoloAsync();
                return Ok(tipi);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore nel recupero tipi articolo");
                return StatusCode(500, "Errore interno nel recupero tipi articolo");
            }
        }

        [HttpGet("stats")]
        public async Task<ActionResult<object>> GetStats()
        {
            try
            {
                var totalCount = await _repository.GetCountAsync();
                var disponibiliCount = (await _repository.GetDisponibiliAsync()).Count;
                var categorie = await _repository.GetCategorieAsync();
                var tipi = await _repository.GetTipiArticoloAsync();

                return Ok(new
                {
                    TotaleArticoli = totalCount,
                    ArticoliDisponibili = disponibiliCount,
                    ArticoliNonDisponibili = totalCount - disponibiliCount,
                    NumeroCategorie = categorie.Count,
                    NumeroTipi = tipi.Count,
                    UltimoAggiornamento = DateTime.Now
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore nel recupero statistiche articoli");
                return StatusCode(500, "Errore interno nel recupero statistiche articoli");
            }
        }
    }
}