using Microsoft.AspNetCore.Mvc;
using DTO;
using Repository.Interface;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Database;
using System.Linq;

namespace BBltZen.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [AllowAnonymous] // ✅ AGGIUNTO
    public class ArticoloController : SecureBaseController
    {
        private readonly IArticoloRepository _repository;
        private readonly BubbleTeaContext _context; // ✅ AGGIUNTO

        public ArticoloController(
            IArticoloRepository repository,
            BubbleTeaContext context, // ✅ AGGIUNTO
            IWebHostEnvironment environment,
            ILogger<ArticoloController> logger)
            : base(environment, logger)
        {
            _repository = repository;
            _context = context; // ✅ AGGIUNTO
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<ArticoloDTO>>> GetAll()
        {
            try
            {
                var result = await _repository.GetAllAsync();
                return Ok(result);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore durante il recupero di tutti gli articoli");
                return SafeInternalError("Errore durante il recupero degli articoli");
            }
        }

        [HttpGet("{articoloId}")]
        [AllowAnonymous]
        public async Task<ActionResult<ArticoloDTO>> GetById(int articoloId)
        {
            try
            {
                if (articoloId <= 0)
                    return SafeBadRequest<ArticoloDTO>("ID articolo non valido");

                var result = await _repository.GetByIdAsync(articoloId);
                if (result == null)
                    return SafeNotFound<ArticoloDTO>("Articolo");

                return Ok(result);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore durante il recupero dell'articolo {ArticoloId}", articoloId);
                return SafeInternalError("Errore durante il recupero dell'articolo");
            }
        }

        [HttpGet("tipo/{tipo}")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<ArticoloDTO>>> GetByTipo(string tipo)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(tipo))
                    return SafeBadRequest<IEnumerable<ArticoloDTO>>("Tipo articolo non valido");

                var result = await _repository.GetByTipoAsync(tipo);
                return Ok(result);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore durante il recupero degli articoli per tipo {Tipo}", tipo);
                return SafeInternalError("Errore durante il recupero degli articoli per tipo");
            }
        }

        [HttpGet("ordinabili")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<ArticoloDTO>>> GetArticoliOrdinabili()
        {
            try
            {
                var result = await _repository.GetArticoliOrdinabiliAsync();
                return Ok(result);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore durante il recupero degli articoli ordinabili");
                return SafeInternalError("Errore durante il recupero degli articoli ordinabili");
            }
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<ActionResult<ArticoloDTO>> Create([FromBody] ArticoloDTO articoloDto) // ✅ AGGIUNTO [FromBody]
        {
            try
            {
                if (!IsModelValid(articoloDto))
                    return SafeBadRequest<ArticoloDTO>("Dati articolo non validi");

                if (await _repository.ExistsAsync(articoloDto.ArticoloId))
                    return SafeBadRequest<ArticoloDTO>($"Esiste già un articolo con ID {articoloDto.ArticoloId}");

                if (!IsTipoValido(articoloDto.Tipo))
                    return SafeBadRequest<ArticoloDTO>($"Tipo articolo non valido: {articoloDto.Tipo}");

                await _repository.AddAsync(articoloDto);

                LogAuditTrail("CREATE_ARTICOLO", "Articolo", articoloDto.ArticoloId.ToString());
                LogSecurityEvent("ArticoloCreated", new
                {
                    ArticoloId = articoloDto.ArticoloId,
                    Tipo = articoloDto.Tipo,
                    User = User.Identity?.Name,
                    Timestamp = DateTime.UtcNow // ✅ AGGIUNTO
                });

                return CreatedAtAction(nameof(GetById),
                    new { articoloId = articoloDto.ArticoloId },
                    articoloDto);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore durante la creazione dell'articolo");
                return SafeInternalError("Errore durante la creazione dell'articolo");
            }
        }

        [HttpPut("{articoloId}")]
        [AllowAnonymous]
        public async Task<ActionResult> Update(int articoloId, [FromBody] ArticoloDTO articoloDto) // ✅ AGGIUNTO [FromBody]
        {
            try
            {
                if (articoloId <= 0)
                    return SafeBadRequest("ID articolo non valido");

                if (articoloId != articoloDto.ArticoloId)
                    return SafeBadRequest("ID articolo non corrispondente");

                if (!IsModelValid(articoloDto))
                    return SafeBadRequest("Dati articolo non validi");

                var existing = await _repository.GetByIdAsync(articoloId);
                if (existing == null)
                    return SafeNotFound("Articolo");

                if (!IsTipoValido(articoloDto.Tipo))
                    return SafeBadRequest($"Tipo articolo non valido: {articoloDto.Tipo}");

                await _repository.UpdateAsync(articoloDto);

                LogAuditTrail("UPDATE_ARTICOLO", "Articolo", articoloDto.ArticoloId.ToString());
                LogSecurityEvent("ArticoloUpdated", new
                {
                    ArticoloId = articoloDto.ArticoloId,
                    Tipo = articoloDto.Tipo,
                    User = User.Identity?.Name,
                    Timestamp = DateTime.UtcNow // ✅ AGGIUNTO
                });

                return NoContent();
            }
            catch (System.ArgumentException ex)
            {
                _logger.LogWarning(ex, "Tentativo di aggiornamento di un articolo non trovato {ArticoloId}", articoloId);
                return SafeNotFound("Articolo");
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore durante l'aggiornamento dell'articolo {ArticoloId}", articoloId);
                return SafeInternalError("Errore durante l'aggiornamento dell'articolo");
            }
        }

        [HttpDelete("{articoloId}")]
        [AllowAnonymous]
        public async Task<ActionResult> Delete(int articoloId)
        {
            try
            {
                if (articoloId <= 0)
                    return SafeBadRequest("ID articolo non valido");

                var existing = await _repository.GetByIdAsync(articoloId);
                if (existing == null)
                    return SafeNotFound("Articolo");

                // ✅ CONTROLLO DIPENDENZE
                var hasOrderItems = await _context.OrderItem
                    .AnyAsync(oi => oi.ArticoloId == articoloId);
                if (hasOrderItems)
                    return SafeBadRequest("Impossibile eliminare: l'articolo è associato a ordini");

                await _repository.DeleteAsync(articoloId);

                LogAuditTrail("DELETE_ARTICOLO", "Articolo", articoloId.ToString());
                LogSecurityEvent("ArticoloDeleted", new
                {
                    ArticoloId = articoloId,
                    User = User.Identity?.Name,
                    Timestamp = DateTime.UtcNow // ✅ AGGIUNTO
                });

                return NoContent();
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore durante l'eliminazione dell'articolo {ArticoloId}", articoloId);
                return SafeInternalError("Errore durante l'eliminazione dell'articolo");
            }
        }

        private bool IsTipoValido(string tipo)
        {
            var tipiValidi = new[] { "BS", "BC", "DOLCE" };
            return !string.IsNullOrWhiteSpace(tipo) && tipiValidi.Contains(tipo.ToUpper());
        }
    }
}