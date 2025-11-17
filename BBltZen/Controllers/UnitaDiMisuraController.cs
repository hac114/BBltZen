using DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Repository.Interface;
using System.Collections.Generic;
using System.Threading.Tasks;

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
        // [Authorize(Roles = "admin,manager")]
        public async Task<ActionResult<UnitaDiMisuraDTO>> Create(UnitaDiMisuraDTO unitaDto)
        {
            try
            {
                if (!IsModelValid(unitaDto))
                    return SafeBadRequest<UnitaDiMisuraDTO>("Dati unità di misura non validi");

                if (await _repository.SiglaExistsAsync(unitaDto.Sigla))
                    return SafeBadRequest<UnitaDiMisuraDTO>("Esiste già un'unità di misura con questa sigla");

                var result = await _repository.AddAsync(unitaDto);

                // ✅ OTTIMIZZATO: Oggetto anonimo semplificato
                LogSecurityEvent("UnitaMisuraCreated", new
                {
                    result.UnitaMisuraId,
                    result.Sigla,
                    UserId = GetCurrentUserId(),
                    UserName = User.Identity?.Name
                });

                LogAuditTrail("CREATE", "UnitaDiMisura", result.UnitaMisuraId.ToString());

                return CreatedAtAction(nameof(GetById), new { id = result.UnitaMisuraId }, result);
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "Errore database durante la creazione dell'unità di misura");
                return SafeInternalError<UnitaDiMisuraDTO>("Errore durante il salvataggio dell'unità di misura");
            }
            catch (ArgumentException argEx)
            {
                return SafeBadRequest<UnitaDiMisuraDTO>(argEx.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante la creazione dell'unità di misura");
                return SafeInternalError<UnitaDiMisuraDTO>("Errore durante la creazione dell'unità di misura");
            }
        }

        // PUT: api/UnitaDiMisura/5
        [HttpPut("{id}")]
        // [Authorize(Roles = "admin,manager")]
        public async Task<ActionResult> Update(int id, UnitaDiMisuraDTO unitaDto)
        {
            try
            {
                if (id <= 0 || id != unitaDto.UnitaMisuraId)
                    return SafeBadRequest("ID unità di misura non valido");

                if (!IsModelValid(unitaDto))
                    return SafeBadRequest("Dati unità di misura non validi");

                if (!await _repository.ExistsAsync(id))
                    return SafeNotFound("Unità di misura");

                if (await _repository.SiglaExistsForOtherAsync(id, unitaDto.Sigla))
                    return SafeBadRequest("Esiste già un'altra unità di misura con questa sigla");

                await _repository.UpdateAsync(unitaDto);

                LogSecurityEvent("UnitaMisuraUpdated", new
                {
                    id,
                    unitaDto.Sigla,
                    UserId = GetCurrentUserId(),
                    UserName = User.Identity?.Name
                });

                LogAuditTrail("UPDATE", "UnitaDiMisura", id.ToString());

                return NoContent();
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "Errore database durante l'aggiornamento dell'unità di misura {Id}", id);
                return SafeInternalError("Errore durante l'aggiornamento dell'unità di misura");
            }
            catch (ArgumentException argEx)
            {
                return SafeBadRequest(argEx.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante l'aggiornamento dell'unità di misura {Id}", id);
                return SafeInternalError("Errore durante l'aggiornamento dell'unità di misura");
            }
        }

        // DELETE: api/UnitaDiMisura/5
        [HttpDelete("{id}")]
        // [Authorize(Roles = "admin")]
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

                LogSecurityEvent("UnitaMisuraDeleted", new
                {
                    id,
                    unita.Sigla,
                    UserId = GetCurrentUserId(),
                    UserName = User.Identity?.Name
                });

                LogAuditTrail("DELETE", "UnitaDiMisura", id.ToString());

                return NoContent();
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "Errore database durante l'eliminazione dell'unità di misura {Id}", id);
                return SafeInternalError("Errore durante l'eliminazione dell'unità di misura");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante l'eliminazione dell'unità di misura {Id}", id);
                return SafeInternalError("Errore durante l'eliminazione dell'unità di misura");
            }
        }

        // GET: api/UnitaDiMisura/sigla/ml
        [HttpGet("sigla/{sigla}")]
        public async Task<ActionResult<UnitaDiMisuraDTO>> GetBySigla(string sigla)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(sigla))
                    return SafeBadRequest<UnitaDiMisuraDTO>("Sigla non valida");

                var unita = await _repository.GetBySiglaAsync(sigla.ToUpper());
                if (unita == null)
                    return SafeNotFound<UnitaDiMisuraDTO>("Unità di misura");

                return Ok(unita);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante il recupero dell'unità di misura con sigla {Sigla}", sigla);
                return SafeInternalError<UnitaDiMisuraDTO>("Errore durante il recupero dell'unità di misura");
            }
        }
    }
}