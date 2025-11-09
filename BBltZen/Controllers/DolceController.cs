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
    public class DolceController : SecureBaseController
    {
        private readonly IDolceRepository _repository;
        private readonly BubbleTeaContext _context; // ✅ AGGIUNTO

        public DolceController(
            IDolceRepository repository,
            BubbleTeaContext context, // ✅ AGGIUNTO
            IWebHostEnvironment environment,
            ILogger<DolceController> logger)
            : base(environment, logger)
        {
            _repository = repository;
            _context = context; // ✅ AGGIUNTO
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<DolceDTO>>> GetAll()
        {
            try
            {
                var result = await _repository.GetAllAsync();
                return Ok(result);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore durante il recupero di tutti i dolci");
                return SafeInternalError<IEnumerable<DolceDTO>>("Errore durante il recupero dei dolci");
            }
        }

        [HttpGet("{articoloId}")]
        [AllowAnonymous]
        public async Task<ActionResult<DolceDTO>> GetById(int articoloId)
        {
            try
            {
                if (articoloId <= 0)
                    return SafeBadRequest<DolceDTO>("ID articolo non valido");

                var result = await _repository.GetByIdAsync(articoloId);

                if (result == null)
                    return SafeNotFound<DolceDTO>("Dolce");

                return Ok(result);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore durante il recupero del dolce {ArticoloId}", articoloId);
                return SafeInternalError<DolceDTO>("Errore durante il recupero del dolce");
            }
        }

        [HttpGet("disponibili")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<DolceDTO>>> GetDisponibili()
        {
            try
            {
                var result = await _repository.GetDisponibiliAsync();
                return Ok(result);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore durante il recupero dei dolci disponibili");
                return SafeInternalError<IEnumerable<DolceDTO>>("Errore durante il recupero dei dolci disponibili");
            }
        }

        [HttpGet("priorita/{priorita}")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<DolceDTO>>> GetByPriorita(int priorita)
        {
            try
            {
                if (priorita <= 0)
                    return SafeBadRequest<IEnumerable<DolceDTO>>("Priorità non valida");

                var result = await _repository.GetByPrioritaAsync(priorita);
                return Ok(result);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore durante il recupero dei dolci per priorità {Priorita}", priorita);
                return SafeInternalError<IEnumerable<DolceDTO>>("Errore durante il recupero dei dolci per priorità");
            }
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<ActionResult<DolceDTO>> Create([FromBody] DolceDTO dolceDto)
        {
            try
            {
                if (!IsModelValid(dolceDto))
                    return SafeBadRequest<DolceDTO>("Dati dolce non validi");

                if (dolceDto.ArticoloId > 0)
                    return SafeBadRequest<DolceDTO>("Non specificare ArticoloId - verrà generato automaticamente");

                // ✅ VERIFICA DUPLICATI OTTIMIZZATA
                var nomeEsistente = await _context.Dolce
                    .AnyAsync(d => d.Nome.ToLower() == dolceDto.Nome.ToLower());
                if (nomeEsistente)
                    return SafeBadRequest<DolceDTO>($"Esiste già un dolce con il nome '{dolceDto.Nome}'");

                await _repository.AddAsync(dolceDto);

                LogAuditTrail("CREATE_DOLCE", "Dolce", dolceDto.ArticoloId.ToString());
                LogSecurityEvent("DolceCreated", new
                {
                    ArticoloId = dolceDto.ArticoloId,
                    Nome = dolceDto.Nome,
                    Prezzo = dolceDto.Prezzo,
                    Disponibile = dolceDto.Disponibile,
                    Priorita = dolceDto.Priorita,
                    User = User.Identity?.Name ?? "Unknown",
                    Timestamp = System.DateTime.UtcNow
                });

                return CreatedAtAction(nameof(GetById),
                    new { articoloId = dolceDto.ArticoloId },
                    dolceDto);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore durante la creazione del dolce");
                return SafeInternalError<DolceDTO>("Errore durante la creazione del dolce");
            }
        }

        [HttpPut("{articoloId}")]
        [AllowAnonymous]
        public async Task<ActionResult> Update(int articoloId, [FromBody] DolceDTO dolceDto)
        {
            try
            {
                if (articoloId <= 0)
                    return SafeBadRequest("ID articolo non valido");

                if (articoloId != dolceDto.ArticoloId)
                    return SafeBadRequest("ID articolo non corrispondente");

                if (!IsModelValid(dolceDto))
                    return SafeBadRequest("Dati dolce non validi");

                var existing = await _repository.GetByIdAsync(articoloId);
                if (existing == null)
                    return SafeNotFound("Dolce");

                // ✅ VERIFICA DUPLICATI OTTIMIZZATA
                var nomeDuplicato = await _context.Dolce
                    .AnyAsync(d => d.Nome.ToLower() == dolceDto.Nome.ToLower() && d.ArticoloId != articoloId);
                if (nomeDuplicato)
                    return SafeBadRequest($"Esiste già un altro dolce con il nome '{dolceDto.Nome}'");

                await _repository.UpdateAsync(dolceDto);

                LogAuditTrail("UPDATE_DOLCE", "Dolce", dolceDto.ArticoloId.ToString());
                LogSecurityEvent("DolceUpdated", new
                {
                    ArticoloId = dolceDto.ArticoloId,
                    Nome = dolceDto.Nome,
                    Prezzo = dolceDto.Prezzo,
                    Disponibile = dolceDto.Disponibile,
                    Priorita = dolceDto.Priorita,
                    User = User.Identity?.Name ?? "Unknown",
                    Timestamp = System.DateTime.UtcNow
                });

                return NoContent();
            }
            catch (System.ArgumentException ex)
            {
                _logger.LogWarning(ex, "Tentativo di aggiornamento di un dolce non trovato {ArticoloId}", articoloId);
                return SafeNotFound("Dolce");
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore durante l'aggiornamento del dolce {ArticoloId}", articoloId);
                return SafeInternalError("Errore durante l'aggiornamento del dolce");
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
                    return SafeNotFound("Dolce");

                // ✅ CONTROLLO DIPENDENZE
                var hasOrderItems = await _context.OrderItem
                    .AnyAsync(oi => oi.ArticoloId == articoloId);
                if (hasOrderItems)
                    return SafeBadRequest("Impossibile eliminare: il dolce è associato a ordini");

                await _repository.DeleteAsync(articoloId);

                LogAuditTrail("DELETE_DOLCE", "Dolce", articoloId.ToString());
                LogSecurityEvent("DolceDeleted", new
                {
                    ArticoloId = articoloId,
                    Nome = existing.Nome,
                    User = User.Identity?.Name ?? "Unknown",
                    Timestamp = System.DateTime.UtcNow
                });

                return NoContent();
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore durante l'eliminazione del dolce {ArticoloId}", articoloId);
                return SafeInternalError("Errore durante l'eliminazione del dolce");
            }
        }
    }
}