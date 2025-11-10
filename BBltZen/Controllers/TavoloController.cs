// BBltZen/Controllers/TavoloController.cs
using Microsoft.AspNetCore.Mvc;
using DTO;
using Repository.Interface;
using Microsoft.AspNetCore.Authorization;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace BBltZen.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    //[Authorize] // ✅ COMMENTATO PER TEST CON SWAGGER
    public class TavoloController : SecureBaseController
    {
        private readonly ITavoloRepository _repository;

        public TavoloController(
            ITavoloRepository repository,
            IWebHostEnvironment environment,
            ILogger<TavoloController> logger)
            : base(environment, logger)
        {
            _repository = repository;
        }

        // GET: api/Tavolo
        [HttpGet]
        [AllowAnonymous] // ✅ OVERRIDE PER ACCESSO PUBBLICO
        public async Task<ActionResult<IEnumerable<TavoloDTO>>> GetAll()
        {
            try
            {
                var tavoli = await _repository.GetAllAsync();
                return Ok(tavoli);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante il recupero di tutti i tavoli");
                return SafeInternalError<IEnumerable<TavoloDTO>>(ex.Message);
            }
        }

        // GET: api/Tavolo/5
        [HttpGet("{id}")]
        [AllowAnonymous] // ✅ OVERRIDE PER ACCESSO PUBBLICO
        public async Task<ActionResult<TavoloDTO>> GetById(int id)
        {
            try
            {
                if (id <= 0)
                    return SafeBadRequest<TavoloDTO>("ID tavolo non valido");

                var tavolo = await _repository.GetByIdAsync(id);

                if (tavolo == null)
                    return SafeNotFound<TavoloDTO>("Tavolo");

                return Ok(tavolo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante il recupero del tavolo {Id}", id);
                return SafeInternalError<TavoloDTO>(ex.Message);
            }
        }

        // GET: api/Tavolo/numero/5
        [HttpGet("numero/{numero}")]
        [AllowAnonymous] // ✅ OVERRIDE PER ACCESSO PUBBLICO
        public async Task<ActionResult<TavoloDTO>> GetByNumero(int numero)
        {
            try
            {
                if (numero <= 0)
                    return SafeBadRequest<TavoloDTO>("Numero tavolo non valido");

                var tavolo = await _repository.GetByNumeroAsync(numero);

                if (tavolo == null)
                    return SafeNotFound<TavoloDTO>("Tavolo");

                return Ok(tavolo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante il recupero del tavolo numero {Numero}", numero);
                return SafeInternalError<TavoloDTO>(ex.Message);
            }
        }

        // GET: api/Tavolo/disponibili
        [HttpGet("disponibili")]
        [AllowAnonymous] // ✅ OVERRIDE PER ACCESSO PUBBLICO
        public async Task<ActionResult<IEnumerable<TavoloDTO>>> GetDisponibili()
        {
            try
            {
                var tavoli = await _repository.GetDisponibiliAsync();
                return Ok(tavoli);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante il recupero dei tavoli disponibili");
                return SafeInternalError<IEnumerable<TavoloDTO>>(ex.Message);
            }
        }

        // GET: api/Tavolo/zona/{zona}
        [HttpGet("zona/{zona}")]
        [AllowAnonymous] // ✅ OVERRIDE PER ACCESSO PUBBLICO
        public async Task<ActionResult<IEnumerable<TavoloDTO>>> GetByZona(string zona)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(zona))
                    return SafeBadRequest<IEnumerable<TavoloDTO>>("Zona non valida");

                var tavoli = await _repository.GetByZonaAsync(zona);
                return Ok(tavoli);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante il recupero dei tavoli per zona {Zona}", zona);
                return SafeInternalError<IEnumerable<TavoloDTO>>(ex.Message);
            }
        }

        // POST: api/Tavolo
        [HttpPost]
        //[Authorize(Roles = "Admin")] // ✅ COMMENTATO PER TEST CON SWAGGER
        public async Task<ActionResult<TavoloDTO>> Create([FromBody] TavoloDTO tavoloDto)
        {
            try
            {
                if (!IsModelValid(tavoloDto))
                    return SafeBadRequest<TavoloDTO>("Dati tavolo non validi");

                // Validazione numero univoco
                if (await _repository.NumeroExistsAsync(tavoloDto.Numero))
                    return SafeBadRequest<TavoloDTO>("Numero tavolo già esistente");

                await _repository.AddAsync(tavoloDto);

                // ✅ Recupera il tavolo creato per ottenere l'ID generato
                var createdTavolo = await _repository.GetByNumeroAsync(tavoloDto.Numero);
                if (createdTavolo == null)
                    return SafeInternalError<TavoloDTO>("Errore durante il recupero del tavolo creato");

                // ✅ Audit trail completo
                LogAuditTrail("CREATE", "Tavolo", createdTavolo.TavoloId.ToString());
                LogSecurityEvent("TavoloCreated", new
                {
                    TavoloId = createdTavolo.TavoloId,
                    Numero = createdTavolo.Numero,
                    Zona = createdTavolo.Zona,
                    Disponibile = createdTavolo.Disponibile,
                    User = User.Identity?.Name ?? "Anonymous",
                    Timestamp = DateTime.UtcNow
                });

                return CreatedAtAction(nameof(GetById), new { id = createdTavolo.TavoloId }, createdTavolo);
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "Errore database durante la creazione del tavolo");
                return SafeInternalError<TavoloDTO>("Errore durante il salvataggio dei dati");
            }
            catch (ArgumentException argEx)
            {
                _logger.LogWarning(argEx, "Argomento non valido durante la creazione del tavolo");
                return SafeBadRequest<TavoloDTO>(argEx.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante la creazione del tavolo");
                return SafeInternalError<TavoloDTO>(ex.Message);
            }
        }

        // PUT: api/Tavolo/5
        [HttpPut("{id}")]
        //[Authorize(Roles = "Admin")] // ✅ COMMENTATO PER TEST CON SWAGGER
        public async Task<ActionResult> Update(int id, [FromBody] TavoloDTO tavoloDto)
        {
            try
            {
                if (id <= 0)
                    return SafeBadRequest("ID tavolo non valido");

                if (id != tavoloDto.TavoloId)
                    return SafeBadRequest("ID tavolo non corrispondente");

                if (!IsModelValid(tavoloDto))
                    return SafeBadRequest("Dati tavolo non validi");

                var existing = await _repository.GetByIdAsync(id);
                if (existing == null)
                    return SafeNotFound("Tavolo");

                // Validazione numero univoco (escludendo l'ID corrente)
                if (await _repository.NumeroExistsAsync(tavoloDto.Numero, id))
                    return SafeBadRequest("Numero tavolo già esistente");

                await _repository.UpdateAsync(tavoloDto);

                // ✅ Audit trail completo
                LogAuditTrail("UPDATE", "Tavolo", tavoloDto.TavoloId.ToString());
                LogSecurityEvent("TavoloUpdated", new
                {
                    TavoloId = tavoloDto.TavoloId,
                    Numero = tavoloDto.Numero,
                    Zona = tavoloDto.Zona,
                    User = User.Identity?.Name ?? "Anonymous",
                    Timestamp = DateTime.UtcNow
                });

                return NoContent();
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "Errore database durante l'aggiornamento del tavolo {Id}", id);
                return SafeInternalError("Errore durante l'aggiornamento dei dati");
            }
            catch (ArgumentException argEx)
            {
                _logger.LogWarning(argEx, "Argomento non valido durante l'aggiornamento del tavolo {Id}", id);
                return SafeBadRequest(argEx.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante l'aggiornamento del tavolo {Id}", id);
                return SafeInternalError(ex.Message);
            }
        }

        // DELETE: api/Tavolo/5
        [HttpDelete("{id}")]
        //[Authorize(Roles = "Admin")] // ✅ COMMENTATO PER TEST CON SWAGGER
        public async Task<ActionResult> Delete(int id)
        {
            try
            {
                if (id <= 0)
                    return SafeBadRequest("ID tavolo non valido");

                var tavolo = await _repository.GetByIdAsync(id);
                if (tavolo == null)
                    return SafeNotFound("Tavolo");

                await _repository.DeleteAsync(id);

                // ✅ Audit trail completo
                LogAuditTrail("DELETE", "Tavolo", id.ToString());
                LogSecurityEvent("TavoloDeleted", new
                {
                    TavoloId = id,
                    Numero = tavolo.Numero,
                    User = User.Identity?.Name ?? "Anonymous",
                    Timestamp = DateTime.UtcNow
                });

                return NoContent();
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "Errore database durante l'eliminazione del tavolo {Id}", id);
                return SafeInternalError("Errore durante l'eliminazione dei dati");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante l'eliminazione del tavolo {Id}", id);
                return SafeInternalError(ex.Message);
            }
        }
    }
}