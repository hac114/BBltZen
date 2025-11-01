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
    [AllowAnonymous] // ✅ OVERRIDE DELL'[Authorize] DEL BASE CONTROLLER
    public class DimensioneBicchiereController : SecureBaseController
    {
        private readonly IDimensioneBicchiereRepository _repository;

        public DimensioneBicchiereController(
            IDimensioneBicchiereRepository repository,
            IWebHostEnvironment environment,
            ILogger<DimensioneBicchiereController> logger)
            : base(environment, logger)
        {
            _repository = repository;
        }

        // GET: api/DimensioneBicchiere
        [HttpGet]
        public async Task<ActionResult<IEnumerable<DimensioneBicchiereDTO>>> GetAll()
        {
            try
            {
                var dimensioni = await _repository.GetAllAsync();
                return Ok(dimensioni);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore durante il recupero di tutte le dimensioni bicchieri");
                return SafeInternalError("Errore durante il recupero delle dimensioni");
            }
        }

        // GET: api/DimensioneBicchiere/5
        [HttpGet("{id}")]
        public async Task<ActionResult<DimensioneBicchiereDTO>> GetById(int id)
        {
            try
            {
                if (id <= 0)
                    return SafeBadRequest<DimensioneBicchiereDTO>("ID dimensione non valido");

                var dimensione = await _repository.GetByIdAsync(id);

                if (dimensione == null)
                    return SafeNotFound<DimensioneBicchiereDTO>("Dimensione bicchiere");

                return Ok(dimensione);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore durante il recupero della dimensione bicchiere {Id}", id);
                return SafeInternalError("Errore durante il recupero della dimensione");
            }
        }

        // POST: api/DimensioneBicchiere
        [HttpPost]
        [Authorize(Roles = "admin")] // ✅ Solo admin può creare dimensioni
        public async Task<ActionResult<DimensioneBicchiereDTO>> Create(DimensioneBicchiereDTO dimensioneDto)
        {
            try
            {
                if (!IsModelValid(dimensioneDto))
                    return SafeBadRequest<DimensioneBicchiereDTO>("Dati dimensione non validi");

                await _repository.AddAsync(dimensioneDto);

                // ✅ Audit trail
                LogAuditTrail("CREATE_DIMENSIONE_BICCHIERE", "DimensioneBicchiere", dimensioneDto.DimensioneBicchiereId.ToString());
                LogSecurityEvent("DimensioneBicchiereCreated", new
                {
                    DimensioneId = dimensioneDto.DimensioneBicchiereId,
                    Sigla = dimensioneDto.Sigla,
                    Descrizione = dimensioneDto.Descrizione,
                    Capienza = dimensioneDto.Capienza,
                    PrezzoBase = dimensioneDto.PrezzoBase
                });

                return CreatedAtAction(nameof(GetById),
                    new { id = dimensioneDto.DimensioneBicchiereId },
                    dimensioneDto);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore durante la creazione della dimensione bicchiere");
                return SafeInternalError("Errore durante la creazione della dimensione");
            }
        }

        // PUT: api/DimensioneBicchiere/5
        [HttpPut("{id}")]
        [Authorize(Roles = "admin")] // ✅ Solo admin può modificare dimensioni
        public async Task<ActionResult> Update(int id, DimensioneBicchiereDTO dimensioneDto)
        {
            try
            {
                if (id <= 0)
                    return SafeBadRequest("ID dimensione non valido");

                if (id != dimensioneDto.DimensioneBicchiereId)
                    return SafeBadRequest("ID dimensione non corrispondente");

                if (!IsModelValid(dimensioneDto))
                    return SafeBadRequest("Dati dimensione non validi");

                var existing = await _repository.GetByIdAsync(id);
                if (existing == null)
                    return SafeNotFound("Dimensione bicchiere");

                await _repository.UpdateAsync(dimensioneDto);

                // ✅ Audit trail
                LogAuditTrail("UPDATE_DIMENSIONE_BICCHIERE", "DimensioneBicchiere", dimensioneDto.DimensioneBicchiereId.ToString());
                LogSecurityEvent("DimensioneBicchiereUpdated", new
                {
                    DimensioneId = dimensioneDto.DimensioneBicchiereId,
                    User = User.Identity?.Name
                });

                return NoContent();
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore durante l'aggiornamento della dimensione bicchiere {Id}", id);
                return SafeInternalError("Errore durante l'aggiornamento della dimensione");
            }
        }

        // DELETE: api/DimensioneBicchiere/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "admin")] // ✅ Solo admin può cancellare dimensioni
        public async Task<ActionResult> Delete(int id)
        {
            try
            {
                if (id <= 0)
                    return SafeBadRequest("ID dimensione non valido");

                var dimensione = await _repository.GetByIdAsync(id);
                if (dimensione == null)
                    return SafeNotFound("Dimensione bicchiere");

                await _repository.DeleteAsync(id);

                // ✅ Audit trail
                LogAuditTrail("DELETE_DIMENSIONE_BICCHIERE", "DimensioneBicchiere", id.ToString());
                LogSecurityEvent("DimensioneBicchiereDeleted", new
                {
                    DimensioneId = id,
                    User = User.Identity?.Name
                });

                return NoContent();
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore durante l'eliminazione della dimensione bicchiere {Id}", id);
                return SafeInternalError("Errore durante l'eliminazione della dimensione");
            }
        }
    }
}