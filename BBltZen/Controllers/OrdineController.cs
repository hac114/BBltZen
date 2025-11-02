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
    public class OrdineController : SecureBaseController
    {
        private readonly IOrdineRepository _repository;

        public OrdineController(
            IOrdineRepository repository,
            IWebHostEnvironment environment,
            ILogger<OrdineController> logger)
            : base(environment, logger)
        {
            _repository = repository;
        }

        // GET: api/Ordine
        [HttpGet]
        //[Authorize(Roles = "admin,barista")] // ✅ COMMENTATO PER TEST
        [AllowAnonymous] // ✅ TEMPORANEAMENTE PERMESSO A TUTTI PER TEST
        public async Task<ActionResult<IEnumerable<OrdineDTO>>> GetAll()
        {
            try
            {
                var result = await _repository.GetAllAsync();
                return Ok(result);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore durante il recupero di tutti gli ordini");
                return SafeInternalError("Errore durante il recupero degli ordini");
            }
        }

        // GET: api/Ordine/5
        [HttpGet("{ordineId}")]
        //[Authorize(Roles = "admin,barista,cliente")] // ✅ COMMENTATO PER TEST
        [AllowAnonymous] // ✅ TEMPORANEAMENTE PERMESSO A TUTTI PER TEST
        public async Task<ActionResult<OrdineDTO>> GetById(int ordineId)
        {
            try
            {
                if (ordineId <= 0)
                    return SafeBadRequest<OrdineDTO>("ID ordine non valido");

                var result = await _repository.GetByIdAsync(ordineId);

                if (result == null)
                    return SafeNotFound<OrdineDTO>("Ordine");

                // ✅ Verifica che l'utente abbia accesso a questo ordine
                // (se implementi autorizzazione per cliente)
                // if (User.IsInRole("cliente") && result.ClienteId != GetCurrentUserId())
                //     return Forbid();

                return Ok(result);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore durante il recupero dell'ordine {OrdineId}", ordineId);
                return SafeInternalError("Errore durante il recupero dell'ordine");
            }
        }

        // GET: api/Ordine/cliente/5
        [HttpGet("cliente/{clienteId}")]
        //[Authorize(Roles = "admin,barista,cliente")] // ✅ COMMENTATO PER TEST
        [AllowAnonymous] // ✅ TEMPORANEAMENTE PERMESSO A TUTTI PER TEST
        public async Task<ActionResult<IEnumerable<OrdineDTO>>> GetByClienteId(int clienteId)
        {
            try
            {
                if (clienteId <= 0)
                    return SafeBadRequest<IEnumerable<OrdineDTO>>("ID cliente non valido");

                // ✅ Verifica che l'utente abbia accesso a questi ordini
                // (se implementi autorizzazione per cliente)
                // if (User.IsInRole("cliente") && clienteId != GetCurrentUserId())
                //     return Forbid();

                var result = await _repository.GetByClienteIdAsync(clienteId);
                return Ok(result);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore durante il recupero degli ordini per cliente {ClienteId}", clienteId);
                return SafeInternalError("Errore durante il recupero degli ordini per cliente");
            }
        }

        // GET: api/Ordine/stato-ordine/2
        [HttpGet("stato-ordine/{statoOrdineId}")]
        //[Authorize(Roles = "admin,barista")] // ✅ COMMENTATO PER TEST
        [AllowAnonymous] // ✅ TEMPORANEAMENTE PERMESSO A TUTTI PER TEST
        public async Task<ActionResult<IEnumerable<OrdineDTO>>> GetByStatoOrdineId(int statoOrdineId)
        {
            try
            {
                if (statoOrdineId <= 0)
                    return SafeBadRequest<IEnumerable<OrdineDTO>>("ID stato ordine non valido");

                var result = await _repository.GetByStatoOrdineIdAsync(statoOrdineId);
                return Ok(result);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore durante il recupero degli ordini per stato ordine {StatoOrdineId}", statoOrdineId);
                return SafeInternalError("Errore durante il recupero degli ordini per stato ordine");
            }
        }

        // GET: api/Ordine/stato-pagamento/1
        [HttpGet("stato-pagamento/{statoPagamentoId}")]
        //[Authorize(Roles = "admin,barista")] // ✅ COMMENTATO PER TEST
        [AllowAnonymous] // ✅ TEMPORANEAMENTE PERMESSO A TUTTI PER TEST
        public async Task<ActionResult<IEnumerable<OrdineDTO>>> GetByStatoPagamentoId(int statoPagamentoId)
        {
            try
            {
                if (statoPagamentoId <= 0)
                    return SafeBadRequest<IEnumerable<OrdineDTO>>("ID stato pagamento non valido");

                var result = await _repository.GetByStatoPagamentoIdAsync(statoPagamentoId);
                return Ok(result);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore durante il recupero degli ordini per stato pagamento {StatoPagamentoId}", statoPagamentoId);
                return SafeInternalError("Errore durante il recupero degli ordini per stato pagamento");
            }
        }

        // POST: api/Ordine
        [HttpPost]
        //[Authorize(Roles = "admin,barista,cliente")] // ✅ COMMENTATO PER TEST
        [AllowAnonymous] // ✅ TEMPORANEAMENTE PERMESSO A TUTTI PER TEST
        public async Task<ActionResult<OrdineDTO>> Create(OrdineDTO ordineDto)
        {
            try
            {
                if (!IsModelValid(ordineDto))
                    return SafeBadRequest<OrdineDTO>("Dati ordine non validi");

                // Verifica se esiste già un ordine con lo stesso ID
                if (ordineDto.OrdineId > 0 && await _repository.ExistsAsync(ordineDto.OrdineId))
                    return Conflict($"Esiste già un ordine con ID {ordineDto.OrdineId}");

                // ✅ Imposta valori di default se non specificati
                ordineDto.StatoOrdineId ??= 1; // Default: In attesa
                ordineDto.StatoPagamentoId ??= 1; // Default: In attesa

                var result = await _repository.AddAsync(ordineDto);

                // ✅ Audit trail
                LogAuditTrail("CREATE_ORDINE", "Ordine", ordineDto.OrdineId.ToString());
                LogSecurityEvent("OrdineCreated", new
                {
                    OrdineId = ordineDto.OrdineId,
                    ClienteId = ordineDto.ClienteId,
                    StatoOrdineId = ordineDto.StatoOrdineId,
                    StatoPagamentoId = ordineDto.StatoPagamentoId,
                    Totale = ordineDto.Totale,
                    User = User.Identity?.Name
                });

                return CreatedAtAction(nameof(GetById),
                    new { ordineId = ordineDto.OrdineId },
                    ordineDto);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore durante la creazione dell'ordine");
                return SafeInternalError("Errore durante la creazione dell'ordine");
            }
        }

        // PUT: api/Ordine/5
        [HttpPut("{ordineId}")]
        //[Authorize(Roles = "admin,barista")] // ✅ COMMENTATO PER TEST
        [AllowAnonymous] // ✅ TEMPORANEAMENTE PERMESSO A TUTTI PER TEST
        public async Task<ActionResult> Update(int ordineId, OrdineDTO ordineDto)
        {
            try
            {
                if (ordineId <= 0)
                    return SafeBadRequest("ID ordine non valido");

                if (ordineId != ordineDto.OrdineId)
                    return SafeBadRequest("ID ordine non corrispondente");

                if (!IsModelValid(ordineDto))
                    return SafeBadRequest("Dati ordine non validi");

                var existing = await _repository.GetByIdAsync(ordineId);
                if (existing == null)
                    return SafeNotFound("Ordine");

                await _repository.UpdateAsync(ordineDto);

                // ✅ Audit trail
                LogAuditTrail("UPDATE_ORDINE", "Ordine", ordineDto.OrdineId.ToString());
                LogSecurityEvent("OrdineUpdated", new
                {
                    OrdineId = ordineDto.OrdineId,
                    StatoOrdineId = ordineDto.StatoOrdineId,
                    StatoPagamentoId = ordineDto.StatoPagamentoId,
                    User = User.Identity?.Name
                });

                return NoContent();
            }
            catch (System.InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Tentativo di aggiornamento di un ordine non trovato {OrdineId}", ordineId);
                return SafeNotFound("Ordine");
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore durante l'aggiornamento dell'ordine {OrdineId}", ordineId);
                return SafeInternalError("Errore durante l'aggiornamento dell'ordine");
            }
        }

        // DELETE: api/Ordine/5
        [HttpDelete("{ordineId}")]
        //[Authorize(Roles = "admin")] // ✅ COMMENTATO PER TEST
        [AllowAnonymous] // ✅ TEMPORANEAMENTE PERMESSO A TUTTI PER TEST
        public async Task<ActionResult> Delete(int ordineId)
        {
            try
            {
                if (ordineId <= 0)
                    return SafeBadRequest("ID ordine non valido");

                var existing = await _repository.GetByIdAsync(ordineId);
                if (existing == null)
                    return SafeNotFound("Ordine");

                await _repository.DeleteAsync(ordineId);

                // ✅ Audit trail
                LogAuditTrail("DELETE_ORDINE", "Ordine", ordineId.ToString());
                LogSecurityEvent("OrdineDeleted", new
                {
                    OrdineId = ordineId,
                    User = User.Identity?.Name
                });

                return NoContent();
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore durante l'eliminazione dell'ordine {OrdineId}", ordineId);
                return SafeInternalError("Errore durante l'eliminazione dell'ordine");
            }
        }
    }
}