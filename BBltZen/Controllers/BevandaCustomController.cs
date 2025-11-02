using Microsoft.AspNetCore.Mvc;
using DTO;
using Repository.Interface;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace BBltZen.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    //[Authorize] // ✅ COMMENTATO PER TEST CON SWAGGER
    public class BevandaCustomController : SecureBaseController
    {
        private readonly IBevandaCustomRepository _repository;

        public BevandaCustomController(
            IBevandaCustomRepository repository,
            IWebHostEnvironment environment,
            ILogger<BevandaCustomController> logger)
            : base(environment, logger)
        {
            _repository = repository;
        }

        // GET: api/BevandaCustom
        [HttpGet]
        [AllowAnonymous] // ✅ PERMESSO A TUTTI PER TEST
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

        // GET: api/BevandaCustom/5
        [HttpGet("{bevandaCustomId}")]
        [AllowAnonymous] // ✅ PERMESSO A TUTTI PER TEST
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

        // GET: api/BevandaCustom/articolo/5
        [HttpGet("articolo/{articoloId}")]
        [AllowAnonymous] // ✅ PERMESSO A TUTTI PER TEST
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

        // GET: api/BevandaCustom/personalizzazione-custom/5
        [HttpGet("personalizzazione-custom/{persCustomId}")]
        [AllowAnonymous] // ✅ PERMESSO A TUTTI PER TEST
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

        // POST: api/BevandaCustom
        [HttpPost]
        //[Authorize(Roles = "admin,barista")] // ✅ COMMENTATO PER TEST
        [AllowAnonymous] // ✅ TEMPORANEAMENTE PERMESSO A TUTTI PER TEST
        public async Task<ActionResult<BevandaCustomDTO>> Create(BevandaCustomDTO bevandaCustomDto)
        {
            try
            {
                if (!IsModelValid(bevandaCustomDto))
                    return SafeBadRequest<BevandaCustomDTO>("Dati bevanda custom non validi");

                // ⚠️ CORREZIONE: Il client NON deve specificare ArticoloId/BevandaCustomId
                if (bevandaCustomDto.ArticoloId > 0)
                    return SafeBadRequest<BevandaCustomDTO>("Non specificare ArticoloId - verrà generato automaticamente");

                if (bevandaCustomDto.BevandaCustomId > 0)
                    return SafeBadRequest<BevandaCustomDTO>("Non specificare BevandaCustomId - verrà generato automaticamente");

                // ⚠️ CORREZIONE: Verifica se esiste già la stessa personalizzazione custom
                if (await _repository.ExistsByPersCustomIdAsync(bevandaCustomDto.PersCustomId))
                    return Conflict($"Esiste già una bevanda custom per la personalizzazione {bevandaCustomDto.PersCustomId}");

                await _repository.AddAsync(bevandaCustomDto);

                // ✅ Audit trail
                LogAuditTrail("CREATE_BEVANDA_CUSTOM", "BevandaCustom", bevandaCustomDto.BevandaCustomId.ToString());
                LogSecurityEvent("BevandaCustomCreated", new
                {
                    BevandaCustomId = bevandaCustomDto.BevandaCustomId,
                    ArticoloId = bevandaCustomDto.ArticoloId,
                    PersCustomId = bevandaCustomDto.PersCustomId,
                    Prezzo = bevandaCustomDto.Prezzo
                });

                return CreatedAtAction(nameof(GetById),
                    new { bevandaCustomId = bevandaCustomDto.BevandaCustomId },
                    bevandaCustomDto);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore durante la creazione della bevanda custom");
                return SafeInternalError("Errore durante la creazione della bevanda custom");
            }
        }

        // PUT: api/BevandaCustom/5
        [HttpPut("{bevandaCustomId}")]
        //[Authorize(Roles = "admin,barista")] // ✅ COMMENTATO PER TEST
        [AllowAnonymous] // ✅ TEMPORANEAMENTE PERMESSO A TUTTI PER TEST
        public async Task<ActionResult> Update(int bevandaCustomId, BevandaCustomDTO bevandaCustomDto)
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

                // ⚠️ CORREZIONE: Verifica che l'ArticoloId non venga modificato
                if (existing.ArticoloId != bevandaCustomDto.ArticoloId)
                    return SafeBadRequest("Non è possibile modificare l'ArticoloId");

                await _repository.UpdateAsync(bevandaCustomDto);

                // ✅ Audit trail
                LogAuditTrail("UPDATE_BEVANDA_CUSTOM", "BevandaCustom", bevandaCustomDto.BevandaCustomId.ToString());
                LogSecurityEvent("BevandaCustomUpdated", new
                {
                    BevandaCustomId = bevandaCustomDto.BevandaCustomId,
                    User = User.Identity?.Name
                });

                return NoContent();
            }
            catch (System.ArgumentException ex)
            {
                _logger.LogWarning(ex, "Tentativo di aggiornamento di una bevanda custom non trovata {BevandaCustomId}", bevandaCustomId);
                return SafeNotFound("Bevanda custom");
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore durante l'aggiornamento della bevanda custom {BevandaCustomId}", bevandaCustomId);
                return SafeInternalError("Errore durante l'aggiornamento della bevanda custom");
            }
        }

        // DELETE: api/BevandaCustom/5
        [HttpDelete("{bevandaCustomId}")]
        //[Authorize(Roles = "admin")] // ✅ COMMENTATO PER TEST
        [AllowAnonymous] // ✅ TEMPORANEAMENTE PERMESSO A TUTTI PER TEST
        public async Task<ActionResult> Delete(int bevandaCustomId)
        {
            try
            {
                if (bevandaCustomId <= 0)
                    return SafeBadRequest("ID bevanda custom non valido");

                var existing = await _repository.GetByIdAsync(bevandaCustomId);
                if (existing == null)
                    return SafeNotFound("Bevanda custom");

                await _repository.DeleteAsync(bevandaCustomId);

                // ✅ Audit trail
                LogAuditTrail("DELETE_BEVANDA_CUSTOM", "BevandaCustom", bevandaCustomId.ToString());
                LogSecurityEvent("BevandaCustomDeleted", new
                {
                    BevandaCustomId = bevandaCustomId,
                    User = User.Identity?.Name
                });

                return NoContent();
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore durante l'eliminazione della bevanda custom {BevandaCustomId}", bevandaCustomId);
                return SafeInternalError("Errore durante l'eliminazione della bevanda custom");
            }
        }
    }
}