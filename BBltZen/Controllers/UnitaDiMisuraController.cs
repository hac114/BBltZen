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
    [AllowAnonymous]
    public class UnitaDiMisuraController : SecureBaseController
    {
        private readonly IUnitaDiMisuraRepository _repository;

        public UnitaDiMisuraController(
            IUnitaDiMisuraRepository repository,
            IWebHostEnvironment environment,
            ILogger<UnitaDiMisuraController> logger)
            : base(environment, logger)
        {
            _repository = repository;
        }

        // GET: api/UnitaDiMisura
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UnitaDiMisuraDTO>>> GetAll()
        {
            try
            {
                var unita = await _repository.GetAllAsync();
                return Ok(unita);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore durante il recupero di tutte le unità di misura");
                return SafeInternalError<IEnumerable<UnitaDiMisuraDTO>>("Errore durante il recupero delle unità di misura");
            }
        }

        // GET: api/UnitaDiMisura/5
        [HttpGet("{id}")]
        public async Task<ActionResult<UnitaDiMisuraDTO>> GetById(int id)
        {
            try
            {
                if (id <= 0)
                    return SafeBadRequest<UnitaDiMisuraDTO>("ID unità di misura non valido");

                var unita = await _repository.GetByIdAsync(id);

                if (unita == null)
                    return SafeNotFound<UnitaDiMisuraDTO>("Unità di misura");

                return Ok(unita);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore durante il recupero dell'unità di misura {Id}", id);
                return SafeInternalError<UnitaDiMisuraDTO>("Errore durante il recupero dell'unità di misura");
            }
        }

        // POST: api/UnitaDiMisura
        [HttpPost]
        // [Authorize(Roles = "admin")]  // COMMENTATO PER TEST
        public async Task<ActionResult<UnitaDiMisuraDTO>> Create(UnitaDiMisuraDTO unitaDto)
        {
            try
            {
                if (!IsModelValid(unitaDto))
                    return SafeBadRequest<UnitaDiMisuraDTO>("Dati unità di misura non validi");

                await _repository.AddAsync(unitaDto);

                LogAuditTrail("CREATE_UNITA_MISURA", "UnitaDiMisura", unitaDto.UnitaMisuraId.ToString());
                LogSecurityEvent("UnitaMisuraCreated", new
                {
                    UnitaId = unitaDto.UnitaMisuraId,
                    Sigla = unitaDto.Sigla,
                    Descrizione = unitaDto.Descrizione
                });

                return CreatedAtAction(nameof(GetById),
                    new { id = unitaDto.UnitaMisuraId },
                    unitaDto);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore durante la creazione dell'unità di misura");
                return SafeInternalError<UnitaDiMisuraDTO>("Errore durante la creazione dell'unità di misura");
            }
        }

        // PUT: api/UnitaDiMisura/5
        [HttpPut("{id}")]
        // [Authorize(Roles = "admin")]  // COMMENTATO PER TEST
        public async Task<ActionResult> Update(int id, UnitaDiMisuraDTO unitaDto)
        {
            try
            {
                if (id <= 0)
                    return SafeBadRequest("ID unità di misura non valido");

                if (id != unitaDto.UnitaMisuraId)
                    return SafeBadRequest("ID unità di misura non corrispondente");

                if (!IsModelValid(unitaDto))
                    return SafeBadRequest("Dati unità di misura non validi");

                var existing = await _repository.GetByIdAsync(id);
                if (existing == null)
                    return SafeNotFound("Unità di misura");

                await _repository.UpdateAsync(unitaDto);

                LogAuditTrail("UPDATE_UNITA_MISURA", "UnitaDiMisura", unitaDto.UnitaMisuraId.ToString());
                LogSecurityEvent("UnitaMisuraUpdated", new
                {
                    UnitaId = unitaDto.UnitaMisuraId,
                    Sigla = unitaDto.Sigla,
                    User = User.Identity?.Name
                });

                return NoContent();
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore durante l'aggiornamento dell'unità di misura {Id}", id);
                return SafeInternalError("Errore durante l'aggiornamento dell'unità di misura");
            }
        }

        // DELETE: api/UnitaDiMisura/5
        [HttpDelete("{id}")]
        // [Authorize(Roles = "admin")]  // COMMENTATO PER TEST
        public async Task<ActionResult> Delete(int id)
        {
            try
            {
                if (id <= 0)
                    return SafeBadRequest("ID unità di misura non valido");

                var unita = await _repository.GetByIdAsync(id);
                if (unita == null)
                    return SafeNotFound("Unità di misura");

                await _repository.DeleteAsync(id);

                LogAuditTrail("DELETE_UNITA_MISURA", "UnitaDiMisura", id.ToString());
                LogSecurityEvent("UnitaMisuraDeleted", new
                {
                    UnitaId = id,
                    User = User.Identity?.Name
                });

                return NoContent();
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore durante l'eliminazione dell'unità di misura {Id}", id);
                return SafeInternalError("Errore durante l'eliminazione dell'unità di misura");
            }
        }
    }
}