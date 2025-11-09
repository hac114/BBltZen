using Microsoft.AspNetCore.Mvc;
using DTO;
using Repository.Interface;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;
using Database;
using Microsoft.EntityFrameworkCore;

namespace BBltZen.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    //[Authorize] // ✅ COMMENTATO PER TEST CON SWAGGER
    public class OrdineController : SecureBaseController
    {
        private readonly IOrdineRepository _ordineRepository; // ✅ CAMBIATO NOME
        private readonly BubbleTeaContext _context;

        public OrdineController(
            IOrdineRepository repository,
            BubbleTeaContext context,
            IWebHostEnvironment environment,
            ILogger<OrdineController> logger)
            : base(environment, logger)
        {
            _ordineRepository = repository; // ✅ CAMBIATO NOME
            _context = context;
        }

        [HttpGet]
        [AllowAnonymous] // ✅ AGGIUNTO ESPLICITAMENTE
        public async Task<ActionResult<IEnumerable<OrdineDTO>>> GetAll()
        {
            try
            {
                var result = await _ordineRepository.GetAllAsync(); // ✅ CAMBIATO NOME

                // ✅ Log per audit
                LogAuditTrail("GET_ALL_ORDINI", "Ordine", "All");

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante il recupero di tutti gli ordini");
                return SafeInternalError<IEnumerable<OrdineDTO>>(
                    _environment.IsDevelopment()
                        ? $"Errore durante il recupero degli ordini: {ex.Message}"
                        : "Errore interno nel recupero ordini"
                );
            }
        }

        [HttpGet("{ordineId}")]
        [AllowAnonymous] // ✅ AGGIUNTO ESPLICITAMENTE
        public async Task<ActionResult<OrdineDTO>> GetById(int ordineId)
        {
            try
            {
                if (ordineId <= 0)
                    return SafeBadRequest<OrdineDTO>("ID ordine non valido");

                var result = await _ordineRepository.GetByIdAsync(ordineId); // ✅ CAMBIATO NOME

                if (result == null)
                    return SafeNotFound<OrdineDTO>("Ordine");

                // ✅ Log per audit
                LogAuditTrail("GET_ORDINE_BY_ID", "Ordine", ordineId.ToString());

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante il recupero dell'ordine {OrdineId}", ordineId);
                return SafeInternalError<OrdineDTO>(
                    _environment.IsDevelopment()
                        ? $"Errore durante il recupero dell'ordine {ordineId}: {ex.Message}"
                        : "Errore interno nel recupero ordine"
                );
            }
        }

        [HttpGet("cliente/{clienteId}")]
        [AllowAnonymous] // ✅ AGGIUNTO ESPLICITAMENTE
        public async Task<ActionResult<IEnumerable<OrdineDTO>>> GetByClienteId(int clienteId)
        {
            try
            {
                if (clienteId <= 0)
                    return SafeBadRequest<IEnumerable<OrdineDTO>>("ID cliente non valido");

                // ✅ Controllo esistenza cliente con BubbleTeaContext
                var clienteExists = await _context.Cliente.AnyAsync(c => c.ClienteId == clienteId);
                if (!clienteExists)
                    return SafeNotFound<IEnumerable<OrdineDTO>>("Cliente non trovato");

                var result = await _ordineRepository.GetByClienteIdAsync(clienteId); // ✅ CAMBIATO NOME

                // ✅ Log per audit
                LogAuditTrail("GET_ORDINI_BY_CLIENTE", "Ordine", clienteId.ToString());

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante il recupero degli ordini per cliente {ClienteId}", clienteId);
                return SafeInternalError<IEnumerable<OrdineDTO>>(
                    _environment.IsDevelopment()
                        ? $"Errore durante il recupero degli ordini per cliente {clienteId}: {ex.Message}"
                        : "Errore interno nel recupero ordini per cliente"
                );
            }
        }

        [HttpGet("stato-ordine/{statoOrdineId}")]
        [AllowAnonymous] // ✅ AGGIUNTO ESPLICITAMENTE
        public async Task<ActionResult<IEnumerable<OrdineDTO>>> GetByStatoOrdineId(int statoOrdineId)
        {
            try
            {
                if (statoOrdineId <= 0)
                    return SafeBadRequest<IEnumerable<OrdineDTO>>("ID stato ordine non valido");

                // ✅ Controllo esistenza stato ordine con BubbleTeaContext
                var statoExists = await _context.StatoOrdine.AnyAsync(so => so.StatoOrdineId == statoOrdineId);
                if (!statoExists)
                    return SafeNotFound<IEnumerable<OrdineDTO>>("Stato ordine non trovato");

                var result = await _ordineRepository.GetByStatoOrdineIdAsync(statoOrdineId); // ✅ CAMBIATO NOME

                // ✅ Log per audit
                LogAuditTrail("GET_ORDINI_BY_STATO_ORDINE", "Ordine", statoOrdineId.ToString());

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante il recupero degli ordini per stato ordine {StatoOrdineId}", statoOrdineId);
                return SafeInternalError<IEnumerable<OrdineDTO>>(
                    _environment.IsDevelopment()
                        ? $"Errore durante il recupero degli ordini per stato ordine {statoOrdineId}: {ex.Message}"
                        : "Errore interno nel recupero ordini per stato ordine"
                );
            }
        }

        [HttpGet("stato-pagamento/{statoPagamentoId}")]
        [AllowAnonymous] // ✅ AGGIUNTO ESPLICITAMENTE
        public async Task<ActionResult<IEnumerable<OrdineDTO>>> GetByStatoPagamentoId(int statoPagamentoId)
        {
            try
            {
                if (statoPagamentoId <= 0)
                    return SafeBadRequest<IEnumerable<OrdineDTO>>("ID stato pagamento non valido");

                // ✅ Controllo esistenza stato pagamento con BubbleTeaContext
                var statoExists = await _context.StatoPagamento.AnyAsync(sp => sp.StatoPagamentoId == statoPagamentoId);
                if (!statoExists)
                    return SafeNotFound<IEnumerable<OrdineDTO>>("Stato pagamento non trovato");

                var result = await _ordineRepository.GetByStatoPagamentoIdAsync(statoPagamentoId); // ✅ CAMBIATO NOME

                // ✅ Log per audit
                LogAuditTrail("GET_ORDINI_BY_STATO_PAGAMENTO", "Ordine", statoPagamentoId.ToString());

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante il recupero degli ordini per stato pagamento {StatoPagamentoId}", statoPagamentoId);
                return SafeInternalError<IEnumerable<OrdineDTO>>(
                    _environment.IsDevelopment()
                        ? $"Errore durante il recupero degli ordini per stato pagamento {statoPagamentoId}: {ex.Message}"
                        : "Errore interno nel recupero ordini per stato pagamento"
                );
            }
        }

        [HttpPost]
        //[Authorize(Roles = "admin,barista,cliente")] // ✅ COMMENTATO PER TEST
        public async Task<ActionResult<OrdineDTO>> Create([FromBody] OrdineDTO ordineDto)
        {
            try
            {
                if (!IsModelValid(ordineDto))
                    return SafeBadRequest<OrdineDTO>("Dati ordine non validi");

                // ✅ Controlli avanzati con BubbleTeaContext
                var clienteExists = await _context.Cliente.AnyAsync(c => c.ClienteId == ordineDto.ClienteId);
                if (!clienteExists)
                    return SafeBadRequest<OrdineDTO>("Cliente non trovato");

                if (ordineDto.StatoOrdineId.HasValue)
                {
                    var statoOrdineExists = await _context.StatoOrdine.AnyAsync(so => so.StatoOrdineId == ordineDto.StatoOrdineId);
                    if (!statoOrdineExists)
                        return SafeBadRequest<OrdineDTO>("Stato ordine non trovato");
                }

                if (ordineDto.StatoPagamentoId.HasValue)
                {
                    var statoPagamentoExists = await _context.StatoPagamento.AnyAsync(sp => sp.StatoPagamentoId == ordineDto.StatoPagamentoId);
                    if (!statoPagamentoExists)
                        return SafeBadRequest<OrdineDTO>("Stato pagamento non trovato");
                }

                // Verifica se esiste già un ordine con lo stesso ID
                if (ordineDto.OrdineId > 0 && await _ordineRepository.ExistsAsync(ordineDto.OrdineId)) // ✅ CAMBIATO NOME
                    return SafeBadRequest<OrdineDTO>("Esiste già un ordine con questo ID");

                // ✅ Imposta valori di default se non specificati
                ordineDto.StatoOrdineId ??= 1; // Default: In attesa
                ordineDto.StatoPagamentoId ??= 1; // Default: In attesa

                var result = await _ordineRepository.AddAsync(ordineDto); // ✅ CAMBIATO NOME

                // ✅ Audit trail e security event
                LogAuditTrail("CREATE_ORDINE", "Ordine", ordineDto.OrdineId.ToString());
                LogSecurityEvent("OrdineCreated", new
                {
                    OrdineId = ordineDto.OrdineId,
                    ClienteId = ordineDto.ClienteId,
                    StatoOrdineId = ordineDto.StatoOrdineId,
                    StatoPagamentoId = ordineDto.StatoPagamentoId,
                    Totale = ordineDto.Totale,
                    Priorita = ordineDto.Priorita,
                    User = User.Identity?.Name ?? "Anonymous",
                    Timestamp = DateTime.UtcNow,
                    IPAddress = HttpContext.Connection.RemoteIpAddress?.ToString()
                });

                return CreatedAtAction(nameof(GetById),
                    new { ordineId = ordineDto.OrdineId },
                    ordineDto);
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "Errore database nella creazione ordine");
                return SafeInternalError<OrdineDTO>(
                    _environment.IsDevelopment()
                        ? $"Errore database nella creazione ordine: {dbEx.InnerException?.Message ?? dbEx.Message}"
                        : "Errore di sistema nella creazione ordine"
                );
            }
            catch (ArgumentException argEx)
            {
                _logger.LogWarning(argEx, "Argomento non valido nella creazione ordine");
                return SafeBadRequest<OrdineDTO>(
                    _environment.IsDevelopment()
                        ? $"Dati non validi: {argEx.Message}"
                        : "Dati non validi"
                );
            }
            catch (InvalidOperationException opEx)
            {
                _logger.LogWarning(opEx, "Operazione non valida nella creazione ordine");
                return SafeBadRequest<OrdineDTO>(
                    _environment.IsDevelopment()
                        ? $"Operazione non valida: {opEx.Message}"
                        : "Operazione non valida"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante la creazione dell'ordine");
                return SafeInternalError<OrdineDTO>(
                    _environment.IsDevelopment()
                        ? $"Errore durante la creazione dell'ordine: {ex.Message}"
                        : "Errore interno nella creazione ordine"
                );
            }
        }

        [HttpPut("{ordineId}")]
        //[Authorize(Roles = "admin,barista")] // ✅ COMMENTATO PER TEST
        public async Task<ActionResult> Update(int ordineId, [FromBody] OrdineDTO ordineDto)
        {
            try
            {
                if (ordineId <= 0)
                    return SafeBadRequest("ID ordine non valido");

                if (ordineId != ordineDto.OrdineId)
                    return SafeBadRequest("ID ordine non corrispondente");

                if (!IsModelValid(ordineDto))
                    return SafeBadRequest("Dati ordine non validi");

                // ✅ Controlli avanzati con BubbleTeaContext
                var clienteExists = await _context.Cliente.AnyAsync(c => c.ClienteId == ordineDto.ClienteId);
                if (!clienteExists)
                    return SafeBadRequest("Cliente non trovato");

                if (ordineDto.StatoOrdineId.HasValue)
                {
                    var statoOrdineExists = await _context.StatoOrdine.AnyAsync(so => so.StatoOrdineId == ordineDto.StatoOrdineId);
                    if (!statoOrdineExists)
                        return SafeBadRequest("Stato ordine non trovato");
                }

                if (ordineDto.StatoPagamentoId.HasValue)
                {
                    var statoPagamentoExists = await _context.StatoPagamento.AnyAsync(sp => sp.StatoPagamentoId == ordineDto.StatoPagamentoId);
                    if (!statoPagamentoExists)
                        return SafeBadRequest("Stato pagamento non trovato");
                }

                var existing = await _ordineRepository.GetByIdAsync(ordineId); // ✅ CAMBIATO NOME
                if (existing == null)
                    return SafeNotFound("Ordine");

                await _ordineRepository.UpdateAsync(ordineDto); // ✅ CAMBIATO NOME

                // ✅ Audit trail e security event
                LogAuditTrail("UPDATE_ORDINE", "Ordine", ordineDto.OrdineId.ToString());
                LogSecurityEvent("OrdineUpdated", new
                {
                    OrdineId = ordineDto.OrdineId,
                    StatoOrdineId = ordineDto.StatoOrdineId,
                    StatoPagamentoId = ordineDto.StatoPagamentoId,
                    Priorita = ordineDto.Priorita,
                    User = User.Identity?.Name ?? "Anonymous",
                    Timestamp = DateTime.UtcNow,
                    IPAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                    Changes = $"StatoOrdine: {existing.StatoOrdineId} → {ordineDto.StatoOrdineId}, StatoPagamento: {existing.StatoPagamentoId} → {ordineDto.StatoPagamentoId}, Priorità: {existing.Priorita} → {ordineDto.Priorita}"
                });

                return NoContent();
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "Errore database nell'aggiornamento ordine {OrdineId}", ordineId);
                return SafeInternalError(
                    _environment.IsDevelopment()
                        ? $"Errore database nell'aggiornamento ordine {ordineId}: {dbEx.InnerException?.Message ?? dbEx.Message}"
                        : "Errore di sistema nell'aggiornamento ordine"
                );
            }
            catch (InvalidOperationException opEx)
            {
                _logger.LogWarning(opEx, "Tentativo di aggiornamento di un ordine non trovato {OrdineId}", ordineId);
                return SafeNotFound("Ordine");
            }
            catch (ArgumentException argEx)
            {
                _logger.LogWarning(argEx, "Argomento non valido nell'aggiornamento ordine {OrdineId}", ordineId);
                return SafeBadRequest(
                    _environment.IsDevelopment()
                        ? $"Dati non validi: {argEx.Message}"
                        : "Dati non validi"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante l'aggiornamento dell'ordine {OrdineId}", ordineId);
                return SafeInternalError(
                    _environment.IsDevelopment()
                        ? $"Errore durante l'aggiornamento dell'ordine {ordineId}: {ex.Message}"
                        : "Errore interno nell'aggiornamento ordine"
                );
            }
        }

        [HttpDelete("{ordineId}")]
        //[Authorize(Roles = "admin")] // ✅ COMMENTATO PER TEST
        public async Task<ActionResult> Delete(int ordineId)
        {
            try
            {
                if (ordineId <= 0)
                    return SafeBadRequest("ID ordine non valido");

                var existing = await _ordineRepository.GetByIdAsync(ordineId); // ✅ CAMBIATO NOME
                if (existing == null)
                    return SafeNotFound("Ordine");

                // ✅ Controlli avanzati con BubbleTeaContext - Verifica se ci sono order items collegati
                var orderItemsCollegati = await _context.OrderItem
                    .AnyAsync(oi => oi.OrdineId == ordineId);
                if (orderItemsCollegati)
                    return SafeBadRequest("Impossibile eliminare: esistono order items collegati a questo ordine");

                await _ordineRepository.DeleteAsync(ordineId); // ✅ CAMBIATO NOME

                // ✅ Audit trail e security event
                LogAuditTrail("DELETE_ORDINE", "Ordine", ordineId.ToString());
                LogSecurityEvent("OrdineDeleted", new
                {
                    OrdineId = ordineId,
                    User = User.Identity?.Name ?? "Anonymous",
                    Timestamp = DateTime.UtcNow,
                    IPAddress = HttpContext.Connection.RemoteIpAddress?.ToString()
                });

                return NoContent();
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "Errore database nell'eliminazione ordine {OrdineId}", ordineId);
                return SafeInternalError(
                    _environment.IsDevelopment()
                        ? $"Errore database nell'eliminazione ordine {ordineId}: {dbEx.InnerException?.Message ?? dbEx.Message}"
                        : "Errore di sistema nell'eliminazione ordine"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante l'eliminazione dell'ordine {OrdineId}", ordineId);
                return SafeInternalError(
                    _environment.IsDevelopment()
                        ? $"Errore durante l'eliminazione dell'ordine {ordineId}: {ex.Message}"
                        : "Errore interno nell'eliminazione ordine"
                );
            }
        }
    }
}