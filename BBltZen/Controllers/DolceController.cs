using Microsoft.AspNetCore.Mvc;
using DTO;
using Repository.Interface;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using Database.Models;

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
            catch (Exception ex)
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
            catch (Exception ex)
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
            catch (Exception ex)
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
                // ✅ CORREZIONE: Priorità può essere 0 (valore minimo consentito)
                if (priorita < 0 || priorita > 10)
                    return SafeBadRequest<IEnumerable<DolceDTO>>("Priorità non valida (deve essere tra 0 e 10)");

                var result = await _repository.GetByPrioritaAsync(priorita);
                return Ok(result);
            }
            catch (Exception ex)
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

                // ✅ CORREZIONE: AddAsync ora ritorna il DTO con ArticoloId generato
                var createdDolce = await _repository.AddAsync(dolceDto);

                // ✅ SEMPLIFICATO: Audit trail
                LogAuditTrail("CREATE", "Dolce", createdDolce.ArticoloId.ToString());
                LogSecurityEvent("DolceCreated", $"Created Dolce ID: {createdDolce.ArticoloId}");

                return CreatedAtAction(nameof(GetById),
                    new { articoloId = createdDolce.ArticoloId },
                    createdDolce);
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "Errore database durante la creazione del dolce");
                return SafeInternalError<DolceDTO>("Errore durante il salvataggio dei dati");
            }
            catch (ArgumentException argEx)
            {
                _logger.LogWarning(argEx, "Argomento non valido durante la creazione del dolce");
                return SafeBadRequest<DolceDTO>(argEx.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante la creazione del dolce");
                return SafeInternalError<DolceDTO>(ex.Message);
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

                // ✅ SEMPLIFICATO: Audit trail
                LogAuditTrail("UPDATE", "Dolce", dolceDto.ArticoloId.ToString());
                LogSecurityEvent("DolceUpdated", $"Updated Dolce ID: {dolceDto.ArticoloId}");

                return NoContent();
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "Errore database durante l'aggiornamento del dolce {ArticoloId}", articoloId);
                return SafeInternalError("Errore durante l'aggiornamento dei dati");
            }
            catch (ArgumentException argEx)
            {
                _logger.LogWarning(argEx, "Argomento non valido durante l'aggiornamento del dolce {ArticoloId}", articoloId);
                return SafeBadRequest(argEx.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante l'aggiornamento del dolce {ArticoloId}", articoloId);
                return SafeInternalError(ex.Message);
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

                // ✅ SEMPLIFICATO: Audit trail
                LogAuditTrail("DELETE", "Dolce", articoloId.ToString());
                LogSecurityEvent("DolceDeleted", $"Deleted Dolce ID: {articoloId}");

                return NoContent();
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "Errore database durante l'eliminazione del dolce {ArticoloId}", articoloId);
                return SafeInternalError("Errore durante l'eliminazione dei dati");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante l'eliminazione del dolce {ArticoloId}", articoloId);
                return SafeInternalError(ex.Message);
            }
        }
    }
}