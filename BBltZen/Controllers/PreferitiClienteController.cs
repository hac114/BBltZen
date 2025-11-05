// BBltZen/Controllers/PreferitiClienteController.cs
using Microsoft.AspNetCore.Mvc;
using DTO;
using Repository.Interface;
using Microsoft.AspNetCore.Authorization;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BBltZen.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    //[Authorize] // ✅ COMMENTATO PER TEST CON SWAGGER
    public class PreferitiClienteController : SecureBaseController
    {
        private readonly IPreferitiClienteRepository _repository;

        public PreferitiClienteController(
            IPreferitiClienteRepository repository,
            IWebHostEnvironment environment,
            ILogger<PreferitiClienteController> logger)
            : base(environment, logger)
        {
            _repository = repository;
        }

        // GET: api/PreferitiCliente
        [HttpGet]
        //[Authorize(Roles = "admin,staff")] // ✅ COMMENTATO PER TEST CON SWAGGER
        public async Task<ActionResult<IEnumerable<PreferitiClienteDTO>>> GetAll()
        {
            try
            {
                var preferiti = await _repository.GetAllAsync();
                return Ok(preferiti);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore durante il recupero di tutti i preferiti");
                return SafeInternalError("Errore durante il recupero dei preferiti");
            }
        }

        // GET: api/PreferitiCliente/5
        [HttpGet("{id}")]
        //[Authorize] // ✅ COMMENTATO PER TEST CON SWAGGER
        public async Task<ActionResult<PreferitiClienteDTO>> GetById(int id)
        {
            try
            {
                if (id <= 0)
                    return SafeBadRequest<PreferitiClienteDTO>("ID preferito non valido");

                var preferito = await _repository.GetByIdAsync(id);

                if (preferito == null)
                    return SafeNotFound<PreferitiClienteDTO>("Preferito");

                return Ok(preferito);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore durante il recupero del preferito {Id}", id);
                return SafeInternalError("Errore durante il recupero del preferito");
            }
        }

        // GET: api/PreferitiCliente/cliente/5
        [HttpGet("cliente/{clienteId}")]
        //[Authorize] // ✅ COMMENTATO PER TEST CON SWAGGER
        public async Task<ActionResult<IEnumerable<PreferitiClienteDTO>>> GetByClienteId(int clienteId)
        {
            try
            {
                if (clienteId <= 0)
                    return SafeBadRequest<IEnumerable<PreferitiClienteDTO>>("ID cliente non valido");

                var preferiti = await _repository.GetByClienteIdAsync(clienteId);
                return Ok(preferiti);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore durante il recupero dei preferiti per cliente {ClienteId}", clienteId);
                return SafeInternalError("Errore durante il recupero dei preferiti");
            }
        }

        // GET: api/PreferitiCliente/bevanda/5
        [HttpGet("bevanda/{bevandaId}")]
        //[Authorize(Roles = "admin,staff")] // ✅ COMMENTATO PER TEST CON SWAGGER
        public async Task<ActionResult<IEnumerable<PreferitiClienteDTO>>> GetByBevandaId(int bevandaId)
        {
            try
            {
                if (bevandaId <= 0)
                    return SafeBadRequest<IEnumerable<PreferitiClienteDTO>>("ID bevanda non valido");

                var preferiti = await _repository.GetByBevandaIdAsync(bevandaId);
                return Ok(preferiti);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore durante il recupero dei preferiti per bevanda {BevandaId}", bevandaId);
                return SafeInternalError("Errore durante il recupero dei preferiti");
            }
        }

        // GET: api/PreferitiCliente/cliente/5/bevanda/5
        [HttpGet("cliente/{clienteId}/bevanda/{bevandaId}")]
        //[Authorize] // ✅ COMMENTATO PER TEST CON SWAGGER
        public async Task<ActionResult<PreferitiClienteDTO>> GetByClienteAndBevanda(int clienteId, int bevandaId)
        {
            try
            {
                if (clienteId <= 0)
                    return SafeBadRequest<PreferitiClienteDTO>("ID cliente non valido");

                if (bevandaId <= 0)
                    return SafeBadRequest<PreferitiClienteDTO>("ID bevanda non valido");

                var preferito = await _repository.GetByClienteAndBevandaAsync(clienteId, bevandaId);

                if (preferito == null)
                    return SafeNotFound<PreferitiClienteDTO>("Preferito");

                return Ok(preferito);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore durante il recupero del preferito per cliente {ClienteId} e bevanda {BevandaId}", clienteId, bevandaId);
                return SafeInternalError("Errore durante il recupero del preferito");
            }
        }

        // POST: api/PreferitiCliente
        [HttpPost]
        //[Authorize] // ✅ COMMENTATO PER TEST CON SWAGGER
        public async Task<ActionResult<PreferitiClienteDTO>> Create(PreferitiClienteDTO preferitoDto)
        {
            try
            {
                if (!IsModelValid(preferitoDto))
                    return SafeBadRequest<PreferitiClienteDTO>("Dati preferito non validi");

                // Verifica se il preferito esiste già
                var exists = await _repository.ExistsByClienteAndBevandaAsync(
                    preferitoDto.ClienteId, preferitoDto.BevandaId);

                if (exists)
                    return SafeBadRequest<PreferitiClienteDTO>("Questa bevanda è già nei preferiti del cliente");

                await _repository.AddAsync(preferitoDto);

                // ✅ Audit trail
                LogAuditTrail("CREATE_PREFERITO", "PreferitiCliente", preferitoDto.PreferitoId.ToString());
                LogSecurityEvent("PreferitoCreated", new
                {
                    PreferitoId = preferitoDto.PreferitoId,
                    ClienteId = preferitoDto.ClienteId,
                    BevandaId = preferitoDto.BevandaId,
                    User = User.Identity?.Name
                });

                return CreatedAtAction(nameof(GetById),
                    new { id = preferitoDto.PreferitoId },
                    preferitoDto);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore durante l'aggiunta ai preferiti");
                return SafeInternalError("Errore durante l'aggiunta ai preferiti");
            }
        }

        // PUT: api/PreferitiCliente/5
        [HttpPut("{id}")]
        //[Authorize] // ✅ COMMENTATO PER TEST CON SWAGGER
        public async Task<ActionResult> Update(int id, PreferitiClienteDTO preferitoDto)
        {
            try
            {
                if (id <= 0)
                    return SafeBadRequest("ID preferito non valido");

                if (id != preferitoDto.PreferitoId)
                    return SafeBadRequest("ID preferito non corrispondente");

                if (!IsModelValid(preferitoDto))
                    return SafeBadRequest("Dati preferito non validi");

                var existing = await _repository.GetByIdAsync(id);
                if (existing == null)
                    return SafeNotFound("Preferito");

                await _repository.UpdateAsync(preferitoDto);

                // ✅ Audit trail
                LogAuditTrail("UPDATE_PREFERITO", "PreferitiCliente", preferitoDto.PreferitoId.ToString());
                LogSecurityEvent("PreferitoUpdated", new
                {
                    PreferitoId = preferitoDto.PreferitoId,
                    ClienteId = preferitoDto.ClienteId,
                    BevandaId = preferitoDto.BevandaId,
                    User = User.Identity?.Name
                });

                return NoContent();
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Preferito non trovato per aggiornamento: {Id}", id);
                return SafeNotFound(ex.Message);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore durante l'aggiornamento del preferito {Id}", id);
                return SafeInternalError("Errore durante l'aggiornamento del preferito");
            }
        }

        // DELETE: api/PreferitiCliente/5
        [HttpDelete("{id}")]
        //[Authorize] // ✅ COMMENTATO PER TEST CON SWAGGER
        public async Task<ActionResult> Delete(int id)
        {
            try
            {
                if (id <= 0)
                    return SafeBadRequest("ID preferito non valido");

                var preferito = await _repository.GetByIdAsync(id);
                if (preferito == null)
                    return SafeNotFound("Preferito");

                await _repository.DeleteAsync(id);

                // ✅ Audit trail
                LogAuditTrail("DELETE_PREFERITO", "PreferitiCliente", id.ToString());
                LogSecurityEvent("PreferitoDeleted", new
                {
                    PreferitoId = id,
                    ClienteId = preferito.ClienteId,
                    BevandaId = preferito.BevandaId,
                    User = User.Identity?.Name
                });

                return NoContent();
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore durante l'eliminazione del preferito {Id}", id);
                return SafeInternalError("Errore durante l'eliminazione del preferito");
            }
        }

        // DELETE: api/PreferitiCliente/cliente/5/bevanda/5
        [HttpDelete("cliente/{clienteId}/bevanda/{bevandaId}")]
        //[Authorize] // ✅ COMMENTATO PER TEST CON SWAGGER
        public async Task<ActionResult> DeleteByClienteAndBevanda(int clienteId, int bevandaId)
        {
            try
            {
                if (clienteId <= 0)
                    return SafeBadRequest("ID cliente non valido");

                if (bevandaId <= 0)
                    return SafeBadRequest("ID bevanda non valido");

                var preferito = await _repository.GetByClienteAndBevandaAsync(clienteId, bevandaId);
                if (preferito == null)
                    return SafeNotFound("Preferito");

                await _repository.DeleteByClienteAndBevandaAsync(clienteId, bevandaId);

                // ✅ Audit trail
                LogAuditTrail("DELETE_PREFERITO_BY_CLIENTE_BEVANDA", "PreferitiCliente", $"{clienteId}_{bevandaId}");
                LogSecurityEvent("PreferitoDeletedByClienteBevanda", new
                {
                    ClienteId = clienteId,
                    BevandaId = bevandaId,
                    User = User.Identity?.Name
                });

                return NoContent();
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore durante l'eliminazione del preferito per cliente {ClienteId} e bevanda {BevandaId}", clienteId, bevandaId);
                return SafeInternalError("Errore durante l'eliminazione del preferito");
            }
        }

        // GET: api/PreferitiCliente/exists/5
        [HttpGet("exists/{id}")]
        //[Authorize] // ✅ COMMENTATO PER TEST CON SWAGGER
        public async Task<ActionResult<bool>> Exists(int id)
        {
            try
            {
                if (id <= 0)
                    return SafeBadRequest<bool>("ID preferito non valido");

                var exists = await _repository.ExistsAsync(id);
                return Ok(exists);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore durante la verifica dell'esistenza del preferito {Id}", id);
                return SafeInternalError("Errore durante la verifica del preferito");
            }
        }

        // GET: api/PreferitiCliente/exists/cliente/5/bevanda/5
        [HttpGet("exists/cliente/{clienteId}/bevanda/{bevandaId}")]
        //[Authorize] // ✅ COMMENTATO PER TEST CON SWAGGER
        public async Task<ActionResult<bool>> ExistsByClienteAndBevanda(int clienteId, int bevandaId)
        {
            try
            {
                if (clienteId <= 0)
                    return SafeBadRequest<bool>("ID cliente non valido");

                if (bevandaId <= 0)
                    return SafeBadRequest<bool>("ID bevanda non valido");

                var exists = await _repository.ExistsByClienteAndBevandaAsync(clienteId, bevandaId);
                return Ok(exists);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore durante la verifica dell'esistenza del preferito per cliente {ClienteId} e bevanda {BevandaId}", clienteId, bevandaId);
                return SafeInternalError("Errore durante la verifica del preferito");
            }
        }

        // GET: api/PreferitiCliente/count/cliente/5
        [HttpGet("count/cliente/{clienteId}")]
        //[Authorize] // ✅ COMMENTATO PER TEST CON SWAGGER
        public async Task<ActionResult<int>> GetCountByCliente(int clienteId)
        {
            try
            {
                if (clienteId <= 0)
                    return SafeBadRequest<int>("ID cliente non valido");

                var count = await _repository.GetCountByClienteAsync(clienteId);
                return Ok(count);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore durante il conteggio dei preferiti per cliente {ClienteId}", clienteId);
                return SafeInternalError("Errore durante il conteggio dei preferiti");
            }
        }
    }
}