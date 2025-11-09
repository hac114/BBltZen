using Microsoft.AspNetCore.Mvc;
using Repository.Interface;
using DTO;
using System;
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
        [AllowAnonymous] // ✅ ESPLICITO PER ENDPOINT GET
        public async Task<ActionResult<ClienteDTO>> GetById(int id)
        {
            try
            {
                if (id <= 0)
                    return SafeBadRequest<ClienteDTO>("ID cliente non valido");

                var cliente = await _clienteRepository.GetByIdAsync(id);

                if (cliente == null)
                    return SafeNotFound<ClienteDTO>("Cliente");

                return Ok(cliente);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante il recupero del cliente {Id}", id);
                return SafeInternalError<ClienteDTO>("Errore durante il recupero del cliente");
            }
        }

        // GET: api/Cliente/tavolo/5
        [HttpGet("tavolo/{tavoloId}")]
        [AllowAnonymous] // ✅ ESPLICITO PER ENDPOINT GET
        public async Task<ActionResult<ClienteDTO>> GetByTavoloId(int tavoloId)
        {
            try
            {
                if (tavoloId <= 0)
                    return SafeBadRequest<ClienteDTO>("ID tavolo non valido");

                var cliente = await _clienteRepository.GetByTavoloIdAsync(tavoloId);

                if (cliente == null)
                    return SafeNotFound<ClienteDTO>("Cliente per il tavolo specificato");

                return Ok(cliente);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante il recupero del cliente per tavolo {TavoloId}", tavoloId);
                return SafeInternalError<ClienteDTO>("Errore durante il recupero del cliente");
            }
        }

        // POST: api/Cliente
        [HttpPost]
        [Authorize(Roles = "admin,operatore")] // ✅ Solo admin e operatore possono creare clienti
        public async Task<ActionResult<ClienteDTO>> Create([FromBody] ClienteDTO clienteDto)
        {
            try
            {
                if (!IsModelValid(clienteDto))
                    return SafeBadRequest<ClienteDTO>("Dati cliente non validi");

                // ✅ Verifica se il tavolo esiste
                var tavoloEsiste = await _context.Tavolo.AnyAsync(t => t.TavoloId == clienteDto.TavoloId);
                if (!tavoloEsiste)
                    return SafeBadRequest<ClienteDTO>("Tavolo non trovato");

                // ✅ Verifica se esiste già un cliente per questo tavolo
                var existingCliente = await _clienteRepository.ExistsByTavoloIdAsync(clienteDto.TavoloId);
                if (existingCliente)
                    return SafeBadRequest<ClienteDTO>("Esiste già un cliente per questo tavolo");

                await _clienteRepository.AddAsync(clienteDto);

                // ✅ Audit trail
                LogAuditTrail("CREATE_CLIENTE", "Cliente", clienteDto.ClienteId.ToString());

                // ✅ Security event completo
                LogSecurityEvent("ClienteCreated", new
                {
                    ClienteId = clienteDto.ClienteId,
                    TavoloId = clienteDto.TavoloId,
                    User = User.Identity?.Name,
                    Timestamp = DateTime.UtcNow
                });

                return CreatedAtAction(nameof(GetById), new { id = clienteDto.ClienteId }, clienteDto);
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
        [Authorize(Roles = "admin,operatore")] // ✅ Solo admin e operatore possono modificare clienti
        public async Task<ActionResult> Update(int id, [FromBody] ClienteDTO clienteDto)
        {
            try
            {
                if (id <= 0)
                    return SafeBadRequest("ID cliente non valido");

                if (id != clienteDto.ClienteId)
                    return SafeBadRequest("ID cliente non corrispondente");

                if (!IsModelValid(clienteDto))
                    return SafeBadRequest("Dati cliente non validi");

                // ✅ Verifica se il cliente esiste
                var existingCliente = await _clienteRepository.GetByIdAsync(id);
                if (existingCliente == null)
                    return SafeNotFound("Cliente");

                // ✅ Verifica se il tavolo esiste
                var tavoloEsiste = await _context.Tavolo.AnyAsync(t => t.TavoloId == clienteDto.TavoloId);
                if (!tavoloEsiste)
                    return SafeBadRequest("Tavolo non trovato");

                // ✅ Verifica se esiste già un altro cliente per questo tavolo
                var clienteByTavolo = await _clienteRepository.GetByTavoloIdAsync(clienteDto.TavoloId);
                if (clienteByTavolo != null && clienteByTavolo.ClienteId != id)
                    return SafeBadRequest("Esiste già un altro cliente per questo tavolo");

                await _clienteRepository.UpdateAsync(clienteDto);

                // ✅ Audit trail
                LogAuditTrail("UPDATE_CLIENTE", "Cliente", clienteDto.ClienteId.ToString());

                // ✅ Security event completo
                LogSecurityEvent("ClienteUpdated", new
                {
                    ClienteId = clienteDto.ClienteId,
                    OldTavoloId = existingCliente.TavoloId,
                    NewTavoloId = clienteDto.TavoloId,
                    User = User.Identity?.Name,
                    Timestamp = DateTime.UtcNow
                });

                return NoContent();
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Cliente non trovato durante l'aggiornamento {Id}", id);
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
        [Authorize(Roles = "admin")] // ✅ Solo admin può eliminare clienti
        public async Task<ActionResult> Delete(int id)
        {
            try
            {
                if (id <= 0)
                    return SafeBadRequest("ID cliente non valido");

                // ✅ Verifica se il cliente esiste
                var existingCliente = await _clienteRepository.GetByIdAsync(id);
                if (existingCliente == null)
                    return SafeNotFound("Cliente");

                // ✅ Controllo se il cliente ha ordini associati
                var hasOrdini = await _context.Ordine.AnyAsync(o => o.ClienteId == id);
                if (hasOrdini)
                    return SafeBadRequest("Impossibile eliminare: il cliente ha ordini associati");

                // ✅ Controllo se il cliente ha preferiti associati
                var hasPreferiti = await _context.PreferitiCliente.AnyAsync(p => p.ClienteId == id);
                if (hasPreferiti)
                    return SafeBadRequest("Impossibile eliminare: il cliente ha preferiti associati");

                await _clienteRepository.DeleteAsync(id);

                // ✅ Audit trail
                LogAuditTrail("DELETE_CLIENTE", "Cliente", id.ToString());

                // ✅ Security event completo
                LogSecurityEvent("ClienteDeleted", new
                {
                    ClienteId = id,
                    TavoloId = existingCliente.TavoloId,
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
        [AllowAnonymous] // ✅ ESPLICITO PER ENDPOINT GET
        public async Task<ActionResult<bool>> Exists(int id)
        {
            try
            {
                if (id <= 0)
                    return SafeBadRequest<bool>("ID cliente non valido");

                var exists = await _clienteRepository.ExistsAsync(id);
                return Ok(exists);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante la verifica esistenza cliente {Id}", id);
                return SafeInternalError<bool>("Errore durante la verifica");
            }
        }

        // GET: api/Cliente/exists/tavolo/5
        [HttpGet("exists/tavolo/{tavoloId}")]
        [AllowAnonymous] // ✅ ESPLICITO PER ENDPOINT GET
        public async Task<ActionResult<bool>> ExistsByTavoloId(int tavoloId)
        {
            try
            {
                if (tavoloId <= 0)
                    return SafeBadRequest<bool>("ID tavolo non valido");

                var exists = await _clienteRepository.ExistsByTavoloIdAsync(tavoloId);
                return Ok(exists);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante la verifica esistenza cliente per tavolo {TavoloId}", tavoloId);
                return SafeInternalError<bool>("Errore durante la verifica");
            }
        }
    }
}