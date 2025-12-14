using Microsoft.AspNetCore.Mvc;
using Repository.Interface;
using DTO;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using BBltZen;

namespace BBltZen.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [AllowAnonymous] // ✅ OVERRIDE DELL'[Authorize] DEL BASE CONTROLLER
    public class ClienteController : SecureBaseController
    {
        private readonly IClienteRepository _clienteRepository;
        private readonly BubbleTeaContext _context;

        public ClienteController(
            IClienteRepository clienteRepository,
            BubbleTeaContext context,
            IWebHostEnvironment environment,
            ILogger<ClienteController> logger)
            : base(environment, logger)
        {
            _clienteRepository = clienteRepository;
            _context = context;
        }

        // GET: api/Cliente
        [HttpGet]
        [AllowAnonymous] // ✅ ESPLICITO PER ENDPOINT GET
        public async Task<ActionResult<IEnumerable<ClienteDTO>>> GetAll()
        {
            try
            {
                var clienti = await _clienteRepository.GetAllAsync();
                return Ok(clienti);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante il recupero di tutti i clienti");
                return SafeInternalError<IEnumerable<ClienteDTO>>("Errore durante il recupero dei clienti");
            }
        }

        // GET: api/Cliente/5
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<ClienteDTO>> GetById(int id)
        {
            try
            {
                if (id <= 0)
                    return SafeBadRequest<ClienteDTO>("ID cliente non valido");

                var cliente = await _clienteRepository.GetByIdAsync(id);
                return cliente == null ? SafeNotFound<ClienteDTO>("Cliente") : Ok(cliente);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante il recupero del cliente {Id}", id);
                return SafeInternalError<ClienteDTO>("Errore durante il recupero del cliente");
            }
        }

        // GET: api/Cliente/tavolo/5
        [HttpGet("tavolo/{tavoloId}")]
        [AllowAnonymous]
        public async Task<ActionResult<ClienteDTO>> GetByTavoloId(int tavoloId)
        {
            try
            {
                if (tavoloId <= 0)
                    return SafeBadRequest<ClienteDTO>("ID tavolo non valido");

                var cliente = await _clienteRepository.GetByTavoloIdAsync(tavoloId);
                return cliente == null ? SafeNotFound<ClienteDTO>("Cliente per il tavolo specificato") : Ok(cliente);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante il recupero del cliente per tavolo {TavoloId}", tavoloId);
                return SafeInternalError<ClienteDTO>("Errore durante il recupero del cliente");
            }
        }

        // POST: api/Cliente
        [HttpPost]
        [Authorize(Roles = "admin,operatore")]
        public async Task<ActionResult<ClienteDTO>> Create([FromBody] ClienteDTO clienteDto)
        {
            try
            {
                if (!IsModelValid(clienteDto))
                    return SafeBadRequest<ClienteDTO>("Dati cliente non validi");

                if (!await _context.Tavolo.AnyAsync(t => t.TavoloId == clienteDto.TavoloId))
                    return SafeBadRequest<ClienteDTO>("Tavolo non trovato");

                if (await _clienteRepository.ExistsByTavoloIdAsync(clienteDto.TavoloId))
                    return SafeBadRequest<ClienteDTO>("Esiste già un cliente per questo tavolo");

                var result = await _clienteRepository.AddAsync(clienteDto);

                LogAuditTrail("CREATE_CLIENTE", "Cliente", result.ClienteId.ToString());

                // ✅ CORRETTO: Sintassi semplificata
                LogSecurityEvent("ClienteCreated", new
                {
                    result.ClienteId,
                    result.TavoloId,
                    User = User.Identity?.Name,
                    Timestamp = DateTime.UtcNow
                });

                return CreatedAtAction(nameof(GetById), new { id = result.ClienteId }, result);
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "Errore database durante la creazione del cliente");
                return SafeInternalError<ClienteDTO>("Errore durante il salvataggio del cliente");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante la creazione del cliente");
                return SafeInternalError<ClienteDTO>("Errore durante la creazione del cliente");
            }
        }

        // PUT: api/Cliente/5
        [HttpPut("{id}")]
        [Authorize(Roles = "admin,operatore")]
        public async Task<ActionResult> Update(int id, [FromBody] ClienteDTO clienteDto)
        {
            try
            {
                if (id <= 0 || id != clienteDto.ClienteId || !IsModelValid(clienteDto))
                    return SafeBadRequest("Dati cliente non validi");

                var existingCliente = await _clienteRepository.GetByIdAsync(id);
                if (existingCliente == null)
                    return SafeNotFound("Cliente");

                if (!await _context.Tavolo.AnyAsync(t => t.TavoloId == clienteDto.TavoloId))
                    return SafeBadRequest("Tavolo non trovato");

                var conflictingCliente = await _clienteRepository.GetByTavoloIdAsync(clienteDto.TavoloId);
                if (conflictingCliente != null && conflictingCliente.ClienteId != id)
                    return SafeBadRequest("Esiste già un altro cliente per questo tavolo");

                await _clienteRepository.UpdateAsync(clienteDto);

                LogAuditTrail("UPDATE_CLIENTE", "Cliente", clienteDto.ClienteId.ToString());

                // ✅ CORRETTO: Sintassi semplificata
                LogSecurityEvent("ClienteUpdated", new
                {
                    clienteDto.ClienteId,
                    OldTavoloId = existingCliente.TavoloId,
                    clienteDto.TavoloId,
                    User = User.Identity?.Name,
                    Timestamp = DateTime.UtcNow
                });

                return NoContent();
            }
            catch (ArgumentException)
            {
                return SafeNotFound("Cliente");
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "Errore database durante l'aggiornamento del cliente {Id}", id);
                return SafeInternalError("Errore durante l'aggiornamento del cliente");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante l'aggiornamento del cliente {Id}", id);
                return SafeInternalError("Errore durante l'aggiornamento del cliente");
            }
        }

        // DELETE: api/Cliente/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult> Delete(int id)
        {
            try
            {
                if (id <= 0)
                    return SafeBadRequest("ID cliente non valido");

                var existingCliente = await _clienteRepository.GetByIdAsync(id);
                if (existingCliente == null)
                    return SafeNotFound("Cliente");

                if (await _context.Ordine.AnyAsync(o => o.ClienteId == id) ||
                    await _context.PreferitiCliente.AnyAsync(p => p.ClienteId == id))
                    return SafeBadRequest("Impossibile eliminare: il cliente ha dati associati");

                await _clienteRepository.DeleteAsync(id);

                LogAuditTrail("DELETE_CLIENTE", "Cliente", id.ToString());

                // ✅ CORRETTO: Sintassi semplificata
                LogSecurityEvent("ClienteDeleted", new
                {
                    ClienteId = id,
                    existingCliente.TavoloId,
                    User = User.Identity?.Name,
                    Timestamp = DateTime.UtcNow
                });

                return NoContent();
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "Errore database durante l'eliminazione del cliente {Id}", id);
                return SafeInternalError("Errore durante l'eliminazione del cliente");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante l'eliminazione del cliente {Id}", id);
                return SafeInternalError("Errore durante l'eliminazione del cliente");
            }
        }

        // GET: api/Cliente/exists/5
        [HttpGet("exists/{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<bool>> Exists(int id)
        {
            try
            {
                return id <= 0
                    ? SafeBadRequest<bool>("ID cliente non valido")
                    : Ok(await _clienteRepository.ExistsAsync(id));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante la verifica esistenza cliente {Id}", id);
                return SafeInternalError<bool>("Errore durante la verifica");
            }
        }

        // GET: api/Cliente/exists/tavolo/5
        [HttpGet("exists/tavolo/{tavoloId}")]
        [AllowAnonymous]
        public async Task<ActionResult<bool>> ExistsByTavoloId(int tavoloId)
        {
            try
            {
                return tavoloId <= 0
                    ? SafeBadRequest<bool>("ID tavolo non valido")
                    : Ok(await _clienteRepository.ExistsByTavoloIdAsync(tavoloId));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante la verifica esistenza cliente per tavolo {TavoloId}", tavoloId);
                return SafeInternalError<bool>("Errore durante la verifica");
            }
        }
    }
}