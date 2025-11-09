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

                // ✅ Verifica se l'unità di misura esiste
                var unitaMisuraEsiste = await _context.UnitaDiMisura.AnyAsync(u => u.UnitaMisuraId == dimensioneDto.UnitaMisuraId);
                if (!unitaMisuraEsiste)
                    return SafeBadRequest<DimensioneBicchiereDTO>("Unità di misura non trovata");

                // ✅ Verifica se esiste già una dimensione con la stessa sigla
                var siglaEsistente = await _context.DimensioneBicchiere
                    .AnyAsync(d => d.Sigla.ToLower() == dimensioneDto.Sigla.ToLower());

                if (siglaEsistente)
                    return SafeBadRequest<DimensioneBicchiereDTO>("Esiste già una dimensione con questa sigla");

                await _repository.AddAsync(dimensioneDto);

                // ✅ Audit trail
                LogAuditTrail("CREATE_DIMENSIONE_BICCHIERE", "DimensioneBicchiere", dimensioneDto.DimensioneBicchiereId.ToString());

                // ✅ Security event completo con timestamp
                LogSecurityEvent("DimensioneBicchiereCreated", new
                {
                    DimensioneId = dimensioneDto.DimensioneBicchiereId,
                    Sigla = dimensioneDto.Sigla,
                    Descrizione = dimensioneDto.Descrizione,
                    Capienza = dimensioneDto.Capienza,
                    UnitaMisuraId = dimensioneDto.UnitaMisuraId,
                    PrezzoBase = dimensioneDto.PrezzoBase,
                    Moltiplicatore = dimensioneDto.Moltiplicatore,
                    User = User.Identity?.Name,
                    Timestamp = DateTime.UtcNow
                });

                return CreatedAtAction(nameof(GetById),
                    new { id = dimensioneDto.DimensioneBicchiereId },
                    dimensioneDto);
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "Errore database durante la creazione della dimensione bicchiere");
                return SafeInternalError<DimensioneBicchiereDTO>("Errore durante il salvataggio della dimensione");
            }
            catch (System.Exception ex)
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
                if (id <= 0)
                    return SafeBadRequest("ID dimensione non valido");

                if (id != dimensioneDto.DimensioneBicchiereId)
                    return SafeBadRequest("ID dimensione non corrispondente");

                if (!IsModelValid(dimensioneDto))
                    return SafeBadRequest("Dati dimensione non validi");

                var existing = await _repository.GetByIdAsync(id);
                if (existing == null)
                    return SafeNotFound("Dimensione bicchiere");

                // ✅ Verifica se l'unità di misura esiste
                var unitaMisuraEsiste = await _context.UnitaDiMisura.AnyAsync(u => u.UnitaMisuraId == dimensioneDto.UnitaMisuraId);
                if (!unitaMisuraEsiste)
                    return SafeBadRequest("Unità di misura non trovata");

                // ✅ Verifica se esiste già un'altra dimensione con la stessa sigla
                var siglaDuplicata = await _context.DimensioneBicchiere
                    .AnyAsync(d => d.Sigla.ToLower() == dimensioneDto.Sigla.ToLower() && d.DimensioneBicchiereId != id);

                if (siglaDuplicata)
                    return SafeBadRequest("Esiste già un'altra dimensione con questa sigla");

                await _repository.UpdateAsync(dimensioneDto);

                // ✅ Audit trail
                LogAuditTrail("UPDATE_DIMENSIONE_BICCHIERE", "DimensioneBicchiere", dimensioneDto.DimensioneBicchiereId.ToString());

                // ✅ Security event completo con timestamp
                LogSecurityEvent("DimensioneBicchiereUpdated", new
                {
                    DimensioneId = dimensioneDto.DimensioneBicchiereId,
                    OldSigla = existing.Sigla,
                    NewSigla = dimensioneDto.Sigla,
                    OldDescrizione = existing.Descrizione,
                    NewDescrizione = dimensioneDto.Descrizione,
                    OldCapienza = existing.Capienza,
                    NewCapienza = dimensioneDto.Capienza,
                    OldPrezzoBase = existing.PrezzoBase,
                    NewPrezzoBase = dimensioneDto.PrezzoBase,
                    OldMoltiplicatore = existing.Moltiplicatore,
                    NewMoltiplicatore = dimensioneDto.Moltiplicatore,
                    User = User.Identity?.Name,
                    Timestamp = DateTime.UtcNow
                });

                return NoContent();
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "Errore database durante l'aggiornamento della dimensione bicchiere {Id}", id);
                return SafeInternalError("Errore durante l'aggiornamento della dimensione");
            }
            catch (System.Exception ex)
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

                // ✅ Controllo se la dimensione è utilizzata in bevande standard
                var hasBevandeStandard = await _context.BevandaStandard.AnyAsync(b => b.DimensioneBicchiereId == id);
                if (hasBevandeStandard)
                    return SafeBadRequest("Impossibile eliminare: la dimensione è utilizzata in bevande standard");

                // ✅ Controllo se la dimensione è utilizzata in quantità ingredienti
                var hasQuantitaIngredienti = await _context.DimensioneQuantitaIngredienti.AnyAsync(d => d.DimensioneBicchiereId == id);
                if (hasQuantitaIngredienti)
                    return SafeBadRequest("Impossibile eliminare: la dimensione è utilizzata in quantità ingredienti");

                // ✅ Controllo se la dimensione è utilizzata in personalizzazioni custom
                var hasPersonalizzazioniCustom = await _context.PersonalizzazioneCustom.AnyAsync(p => p.DimensioneBicchiereId == id);
                if (hasPersonalizzazioniCustom)
                    return SafeBadRequest("Impossibile eliminare: la dimensione è utilizzata in personalizzazioni custom");

                await _repository.DeleteAsync(id);

                // ✅ Audit trail
                LogAuditTrail("DELETE_DIMENSIONE_BICCHIERE", "DimensioneBicchiere", id.ToString());

                // ✅ Security event completo con timestamp
                LogSecurityEvent("DimensioneBicchiereDeleted", new
                {
                    DimensioneId = id,
                    Sigla = dimensione.Sigla,
                    Descrizione = dimensione.Descrizione,
                    User = User.Identity?.Name,
                    Timestamp = DateTime.UtcNow
                });

                return NoContent();
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "Errore database durante l'eliminazione della dimensione bicchiere {Id}", id);
                return SafeInternalError("Errore durante l'eliminazione della dimensione");
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore durante l'eliminazione della dimensione bicchiere {Id}", id);
                return SafeInternalError("Errore durante l'eliminazione della dimensione");
            }
        }
    }
}