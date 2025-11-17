using Microsoft.AspNetCore.Mvc;
using DTO;
using Repository.Interface;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Database;

namespace BBltZen.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [AllowAnonymous] // ✅ AGGIUNTO
    public class BevandaCustomController : SecureBaseController
    {
        private readonly IBevandaCustomRepository _repository;
        private readonly BubbleTeaContext _context; // ✅ AGGIUNTO

        public BevandaCustomController(
            IBevandaCustomRepository repository,
            BubbleTeaContext context, // ✅ AGGIUNTO
            IWebHostEnvironment environment,
            ILogger<BevandaCustomController> logger)
            : base(environment, logger)
        {
            _repository = repository;
            _context = context; // ✅ AGGIUNTO
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<BevandaCustomDTO>>> GetAll()
        {
            try
            {
                var result = await _repository.GetAllAsync();
                return Ok(result);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore durante il recupero di tutte le bevande custom");
                return SafeInternalError("Errore durante il recupero delle bevande custom");
            }
        }

        [HttpGet("{bevandaCustomId}")]
        [AllowAnonymous]
        public async Task<ActionResult<BevandaCustomDTO>> GetById(int bevandaCustomId)
        {
            try
            {
                if (bevandaCustomId <= 0)
                    return SafeBadRequest<BevandaCustomDTO>("ID bevanda custom non valido");

                var result = await _repository.GetByIdAsync(bevandaCustomId);
                if (result == null)
                    return SafeNotFound<BevandaCustomDTO>("Bevanda custom");

                return Ok(result);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore durante il recupero della bevanda custom {BevandaCustomId}", bevandaCustomId);
                return SafeInternalError("Errore durante il recupero della bevanda custom");
            }
        }

        [HttpGet("articolo/{articoloId}")]
        [AllowAnonymous]
        public async Task<ActionResult<BevandaCustomDTO>> GetByArticoloId(int articoloId)
        {
            try
            {
                if (articoloId <= 0)
                    return SafeBadRequest<BevandaCustomDTO>("ID articolo non valido");

                var result = await _repository.GetByArticoloIdAsync(articoloId);
                if (result == null)
                    return SafeNotFound<BevandaCustomDTO>("Bevanda custom per articolo");

                return Ok(result);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore durante il recupero della bevanda custom per articolo {ArticoloId}", articoloId);
                return SafeInternalError("Errore durante il recupero della bevanda custom");
            }
        }

        [HttpGet("personalizzazione-custom/{persCustomId}")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<BevandaCustomDTO>>> GetByPersCustomId(int persCustomId)
        {
            try
            {
                if (persCustomId <= 0)
                    return SafeBadRequest<IEnumerable<BevandaCustomDTO>>("ID personalizzazione custom non valido");

                var result = await _repository.GetByPersCustomIdAsync(persCustomId);
                return Ok(result);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore durante il recupero delle bevande custom per personalizzazione {PersCustomId}", persCustomId);
                return SafeInternalError("Errore durante il recupero delle bevande custom");
            }
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<ActionResult<BevandaCustomDTO>> Create([FromBody] BevandaCustomDTO bevandaCustomDto)
        {
            try
            {
                if (!IsModelValid(bevandaCustomDto))
                    return SafeBadRequest<BevandaCustomDTO>("Dati bevanda custom non validi");

                if (bevandaCustomDto.ArticoloId > 0)
                    return SafeBadRequest<BevandaCustomDTO>("Non specificare ArticoloId - verrà generato automaticamente");

                if (bevandaCustomDto.BevandaCustomId > 0)
                    return SafeBadRequest<BevandaCustomDTO>("Non specificare BevandaCustomId - verrà generato automaticamente");

                // ✅ VERIFICA ESISTENZA PERSONALIZZAZIONE CUSTOM
                var persCustomEsiste = await _context.PersonalizzazioneCustom
                    .AnyAsync(p => p.PersCustomId == bevandaCustomDto.PersCustomId);
                if (!persCustomEsiste)
                    return SafeBadRequest<BevandaCustomDTO>("Personalizzazione custom non trovata");

                if (await _repository.ExistsByPersCustomIdAsync(bevandaCustomDto.PersCustomId))
                    return SafeBadRequest<BevandaCustomDTO>($"Esiste già una bevanda custom per la personalizzazione {bevandaCustomDto.PersCustomId}");

                // ✅ CORREZIONE: AddAsync ora ritorna il DTO con gli ID generati
                var createdBevanda = await _repository.AddAsync(bevandaCustomDto);

                // ✅ SEMPLIFICATO: Audit trail
                LogAuditTrail("CREATE", "BevandaCustom", createdBevanda.BevandaCustomId.ToString());
                LogSecurityEvent("BevandaCustomCreated", $"Created BevandaCustom ID: {createdBevanda.BevandaCustomId}");

                return CreatedAtAction(nameof(GetById),
                    new { bevandaCustomId = createdBevanda.BevandaCustomId },
                    createdBevanda);
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "Errore database durante la creazione della bevanda custom");
                return SafeInternalError<BevandaCustomDTO>("Errore durante il salvataggio dei dati");
            }
            catch (ArgumentException argEx)
            {
                _logger.LogWarning(argEx, "Argomento non valido durante la creazione della bevanda custom");
                return SafeBadRequest<BevandaCustomDTO>(argEx.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante la creazione della bevanda custom");
                return SafeInternalError<BevandaCustomDTO>(ex.Message);
            }
        }

        [HttpPut("{bevandaCustomId}")]
        [AllowAnonymous]
        public async Task<ActionResult> Update(int bevandaCustomId, [FromBody] BevandaCustomDTO bevandaCustomDto)
        {
            try
            {
                if (bevandaCustomId <= 0)
                    return SafeBadRequest("ID bevanda custom non valido");

                if (bevandaCustomId != bevandaCustomDto.BevandaCustomId)
                    return SafeBadRequest("ID bevanda custom non corrispondente");

                if (!IsModelValid(bevandaCustomDto))
                    return SafeBadRequest("Dati bevanda custom non validi");

                var existing = await _repository.GetByIdAsync(bevandaCustomId);
                if (existing == null)
                    return SafeNotFound("Bevanda custom");

                if (existing.ArticoloId != bevandaCustomDto.ArticoloId)
                    return SafeBadRequest("Non è possibile modificare l'ArticoloId");

                // ✅ VERIFICA ESISTENZA PERSONALIZZAZIONE CUSTOM
                var persCustomEsiste = await _context.PersonalizzazioneCustom
                    .AnyAsync(p => p.PersCustomId == bevandaCustomDto.PersCustomId);
                if (!persCustomEsiste)
                    return SafeBadRequest("Personalizzazione custom non trovata");

                // ✅ CORREZIONE: Validazione duplicati (escludendo l'ID corrente)
                var existingWithSamePersCustom = await _context.BevandaCustom
                    .FirstOrDefaultAsync(bc => bc.PersCustomId == bevandaCustomDto.PersCustomId &&
                                             bc.BevandaCustomId != bevandaCustomId);

                if (existingWithSamePersCustom != null)
                    return SafeBadRequest("Esiste già una bevanda custom per questa personalizzazione");

                await _repository.UpdateAsync(bevandaCustomDto);

                // ✅ SEMPLIFICATO: Audit trail
                LogAuditTrail("UPDATE", "BevandaCustom", bevandaCustomDto.BevandaCustomId.ToString());
                LogSecurityEvent("BevandaCustomUpdated", $"Updated BevandaCustom ID: {bevandaCustomDto.BevandaCustomId}");

                return NoContent();
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "Errore database durante l'aggiornamento della bevanda custom {BevandaCustomId}", bevandaCustomId);
                return SafeInternalError("Errore durante l'aggiornamento dei dati");
            }
            catch (ArgumentException argEx)
            {
                _logger.LogWarning(argEx, "Argomento non valido durante l'aggiornamento della bevanda custom {BevandaCustomId}", bevandaCustomId);
                return SafeBadRequest(argEx.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante l'aggiornamento della bevanda custom {BevandaCustomId}", bevandaCustomId);
                return SafeInternalError(ex.Message);
            }
        }

        [HttpDelete("{bevandaCustomId}")]
        [AllowAnonymous]
        public async Task<ActionResult> Delete(int bevandaCustomId)
        {
            try
            {
                if (bevandaCustomId <= 0)
                    return SafeBadRequest("ID bevanda custom non valido");

                var existing = await _repository.GetByIdAsync(bevandaCustomId);
                if (existing == null)
                    return SafeNotFound("Bevanda custom");

                // ✅ CONTROLLO DIPENDENZE
                var hasOrderItems = await _context.OrderItem
                    .AnyAsync(oi => oi.ArticoloId == existing.ArticoloId);
                if (hasOrderItems)
                    return SafeBadRequest("Impossibile eliminare: la bevanda è associata a ordini");

                await _repository.DeleteAsync(bevandaCustomId);

                // ✅ SEMPLIFICATO: Audit trail
                LogAuditTrail("DELETE", "BevandaCustom", bevandaCustomId.ToString());
                LogSecurityEvent("BevandaCustomDeleted", $"Deleted BevandaCustom ID: {bevandaCustomId}");

                return NoContent();
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "Errore database durante l'eliminazione della bevanda custom {BevandaCustomId}", bevandaCustomId);
                return SafeInternalError("Errore durante l'eliminazione dei dati");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante l'eliminazione della bevanda custom {BevandaCustomId}", bevandaCustomId);
                return SafeInternalError(ex.Message);
            }
        }
    }
}