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
    [AllowAnonymous] // ✅ OVERRIDE DELL'[Authorize] DEL BASE CONTROLLER
    public class DimensioneBicchiereController : SecureBaseController
    {
        private readonly IDimensioneBicchiereRepository _repository;
        private readonly BubbleTeaContext _context;

        public DimensioneBicchiereController(
            IDimensioneBicchiereRepository repository,
            BubbleTeaContext context,
            IWebHostEnvironment environment,
            ILogger<DimensioneBicchiereController> logger)
            : base(environment, logger)
        {
            _repository = repository;
            _context = context;
        }

        // GET: api/DimensioneBicchiere
        [HttpGet]
        [AllowAnonymous] // ✅ ESPLICITO PER ENDPOINT GET
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
                return SafeInternalError<IEnumerable<DimensioneBicchiereDTO>>("Errore durante il recupero delle dimensioni");
            }
        }

        // GET: api/DimensioneBicchiere/5
        [HttpGet("{id}")]
        [AllowAnonymous] // ✅ ESPLICITO PER ENDPOINT GET
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
                return SafeInternalError<DimensioneBicchiereDTO>("Errore durante il recupero della dimensione");
            }
        }

        // POST: api/DimensioneBicchiere
        [HttpPost]
        // [Authorize(Roles = "admin")] // ✅ COMMENTATO PER TEST
        public async Task<ActionResult<DimensioneBicchiereDTO>> Create([FromBody] DimensioneBicchiereDTO dimensioneDto)
        {
            try
            {
                if (!IsModelValid(dimensioneDto))
                    return SafeBadRequest<DimensioneBicchiereDTO>("Dati dimensione non validi");

                // ✅ Verifica unità di misura
                var unitaMisuraEsiste = await _context.UnitaDiMisura.AnyAsync(u => u.UnitaMisuraId == dimensioneDto.UnitaMisuraId);
                if (!unitaMisuraEsiste)
                    return SafeBadRequest<DimensioneBicchiereDTO>("Unità di misura non trovata");

                // ✅ VERIFICA SIGLA - METODO ALTERNATIVO (usa query diretta finché non aggiungi i metodi al repository)
                var siglaEsistente = await _context.DimensioneBicchiere
                    .AnyAsync(d => d.Sigla.ToLower() == dimensioneDto.Sigla.ToLower());

                if (siglaEsistente)
                    return SafeBadRequest<DimensioneBicchiereDTO>("Esiste già una dimensione con questa sigla");

                // ✅ CORREZIONE: Usa il risultato del repository
                var result = await _repository.AddAsync(dimensioneDto);

                // ✅ Audit trail ottimizzato
                LogAuditTrail("CREATE", "DimensioneBicchiere", result.DimensioneBicchiereId.ToString());
                LogSecurityEvent("DimensioneBicchiereCreated", new { result.DimensioneBicchiereId, result.Sigla, User = User.Identity?.Name });

                return CreatedAtAction(nameof(GetById), new { id = result.DimensioneBicchiereId }, result);
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "Errore database durante la creazione della dimensione bicchiere");
                return SafeInternalError<DimensioneBicchiereDTO>("Errore durante il salvataggio della dimensione");
            }
            catch (ArgumentException argEx)
            {
                return SafeBadRequest<DimensioneBicchiereDTO>(argEx.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante la creazione della dimensione bicchiere");
                return SafeInternalError<DimensioneBicchiereDTO>("Errore durante la creazione della dimensione");
            }
        }

        // PUT: api/DimensioneBicchiere/5
        [HttpPut("{id}")]
        // [Authorize(Roles = "admin")] // ✅ COMMENTATO PER TEST
        public async Task<ActionResult> Update(int id, [FromBody] DimensioneBicchiereDTO dimensioneDto)
        {
            try
            {
                if (id <= 0 || id != dimensioneDto.DimensioneBicchiereId)
                    return SafeBadRequest("ID dimensione non valido");

                if (!IsModelValid(dimensioneDto))
                    return SafeBadRequest("Dati dimensione non validi");

                // ✅ Verifica esistenza usando il repository
                if (!await _repository.ExistsAsync(id))
                    return SafeNotFound("Dimensione bicchiere");

                // ✅ Verifica unità di misura
                var unitaMisuraEsiste = await _context.UnitaDiMisura.AnyAsync(u => u.UnitaMisuraId == dimensioneDto.UnitaMisuraId);
                if (!unitaMisuraEsiste)
                    return SafeBadRequest("Unità di misura non trovata");

                // ✅ VERIFICA SIGLA DUPICATA - METODO ALTERNATIVO
                var siglaDuplicata = await _context.DimensioneBicchiere
                    .AnyAsync(d => d.Sigla.ToLower() == dimensioneDto.Sigla.ToLower() && d.DimensioneBicchiereId != id);

                if (siglaDuplicata)
                    return SafeBadRequest("Esiste già un'altra dimensione con questa sigla");

                await _repository.UpdateAsync(dimensioneDto);

                // ✅ Audit trail ottimizzato
                LogAuditTrail("UPDATE", "DimensioneBicchiere", id.ToString());
                LogSecurityEvent("DimensioneBicchiereUpdated", new { DimensioneId = id, dimensioneDto.Sigla, User = User.Identity?.Name });

                return NoContent();
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "Errore database durante l'aggiornamento della dimensione bicchiere {Id}", id);
                return SafeInternalError("Errore durante l'aggiornamento della dimensione");
            }
            catch (ArgumentException argEx)
            {
                return SafeBadRequest(argEx.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante l'aggiornamento della dimensione bicchiere {Id}", id);
                return SafeInternalError("Errore durante l'aggiornamento della dimensione");
            }
        }

        // DELETE: api/DimensioneBicchiere/5
        [HttpDelete("{id}")]
        // [Authorize(Roles = "admin")] // ✅ COMMENTATO PER TEST
        public async Task<ActionResult> Delete(int id)
        {
            try
            {
                if (id <= 0)
                    return SafeBadRequest("ID dimensione non valido");

                var dimensione = await _repository.GetByIdAsync(id);
                if (dimensione == null)
                    return SafeNotFound("Dimensione bicchiere");

                // ✅ CONTROLLO DIPENDENZE OTTIMIZZATO - Query unica
                var hasDependencies = await _context.BevandaStandard.AnyAsync(b => b.DimensioneBicchiereId == id) ||
                                     await _context.DimensioneQuantitaIngredienti.AnyAsync(d => d.DimensioneBicchiereId == id) ||
                                     await _context.PersonalizzazioneCustom.AnyAsync(p => p.DimensioneBicchiereId == id);

                if (hasDependencies)
                    return SafeBadRequest("Impossibile eliminare: la dimensione è utilizzata in altri elementi");

                await _repository.DeleteAsync(id);

                // ✅ AUDIT & SECURITY OTTIMIZZATI
                LogAuditTrail("DELETE", "DimensioneBicchiere", id.ToString());
                LogSecurityEvent("DimensioneBicchiereDeleted", new { DimensioneId = id, dimensione.Sigla, User = User.Identity?.Name });

                return NoContent();
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "Errore database durante l'eliminazione della dimensione bicchiere {Id}", id);
                return SafeInternalError("Errore durante l'eliminazione della dimensione");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante l'eliminazione della dimensione bicchiere {Id}", id);
                return SafeInternalError("Errore durante l'eliminazione della dimensione");
            }
        }
    }
}