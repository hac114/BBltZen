// BBltZen/Controllers/UtentiController.cs
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
    public class UtentiController : SecureBaseController
    {
        private readonly IUtentiRepository _repository;

        public UtentiController(
            IUtentiRepository repository,
            IWebHostEnvironment environment,
            ILogger<UtentiController> logger)
            : base(environment, logger)
        {
            _repository = repository;
        }

        // GET: api/Utenti
        [HttpGet]
        //[Authorize(Roles = "Admin")] // ✅ COMMENTATO PER TEST CON SWAGGER
        public async Task<ActionResult<IEnumerable<UtentiDTO>>> GetAll()
        {
            try
            {
                var utenti = await _repository.GetAllAsync();
                return Ok(utenti);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante il recupero di tutti gli utenti");
                return SafeInternalError<IEnumerable<UtentiDTO>>(ex.Message);
            }
        }

        // GET: api/Utenti/5
        [HttpGet("{id}")]
        //[Authorize] // ✅ COMMENTATO PER TEST CON SWAGGER
        public async Task<ActionResult<UtentiDTO>> GetById(int id)
        {
            try
            {
                if (id <= 0)
                    return SafeBadRequest<UtentiDTO>("ID utente non valido");

                var utente = await _repository.GetByIdAsync(id);

                if (utente == null)
                    return SafeNotFound<UtentiDTO>("Utente");

                return Ok(utente);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante il recupero dell'utente {Id}", id);
                return SafeInternalError<UtentiDTO>(ex.Message);
            }
        }

        // GET: api/Utenti/email/test@example.com
        [HttpGet("email/{email}")]
        //[Authorize] // ✅ COMMENTATO PER TEST CON SWAGGER
        public async Task<ActionResult<UtentiDTO>> GetByEmail(string email)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(email))
                    return SafeBadRequest<UtentiDTO>("Email non valida");

                var utente = await _repository.GetByEmailAsync(email);

                if (utente == null)
                    return SafeNotFound<UtentiDTO>("Utente");

                return Ok(utente);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante il recupero dell'utente per email {Email}", email);
                return SafeInternalError<UtentiDTO>(ex.Message);
            }
        }

        // GET: api/Utenti/tipo/admin
        [HttpGet("tipo/{tipoUtente}")]
        //[Authorize(Roles = "Admin")] // ✅ COMMENTATO PER TEST CON SWAGGER
        public async Task<ActionResult<IEnumerable<UtentiDTO>>> GetByTipo(string tipoUtente)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(tipoUtente))
                    return SafeBadRequest<IEnumerable<UtentiDTO>>("Tipo utente non valido");

                var utenti = await _repository.GetByTipoUtenteAsync(tipoUtente);
                return Ok(utenti);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante il recupero degli utenti per tipo {TipoUtente}", tipoUtente);
                return SafeInternalError<IEnumerable<UtentiDTO>>(ex.Message);
            }
        }

        // GET: api/Utenti/attivi
        [HttpGet("attivi")]
        //[Authorize(Roles = "Admin")] // ✅ COMMENTATO PER TEST CON SWAGGER
        public async Task<ActionResult<IEnumerable<UtentiDTO>>> GetAttivi()
        {
            try
            {
                var utenti = await _repository.GetAttiviAsync();
                return Ok(utenti);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante il recupero degli utenti attivi");
                return SafeInternalError<IEnumerable<UtentiDTO>>(ex.Message);
            }
        }

        // POST: api/Utenti
        [HttpPost]
        [AllowAnonymous] // ✅ REGISTRAZIONE PUBBLICA
        public async Task<ActionResult<UtentiDTO>> Create([FromBody] UtentiDTO utenteDto)
        {
            try
            {
                if (!IsModelValid(utenteDto))
                    return SafeBadRequest<UtentiDTO>("Dati utente non validi");

                // Verifica se l'email esiste già
                if (await _repository.EmailExistsAsync(utenteDto.Email))
                    return SafeBadRequest<UtentiDTO>("Email già registrata");

                await _repository.AddAsync(utenteDto);

                // ✅ Recupera l'utente creato per ottenere i valori dal database
                var createdUtente = await _repository.GetByEmailAsync(utenteDto.Email);
                if (createdUtente == null)
                    return SafeInternalError<UtentiDTO>("Errore durante il recupero dell'utente creato");

                // ✅ Audit trail completo
                LogAuditTrail("CREATE", "Utenti", createdUtente.UtenteId.ToString());
                LogSecurityEvent("UtenteCreated", new
                {
                    UtenteId = createdUtente.UtenteId,
                    Email = createdUtente.Email,
                    TipoUtente = createdUtente.TipoUtente,
                    User = User.Identity?.Name ?? "Anonymous",
                    Timestamp = DateTime.UtcNow
                });

                return CreatedAtAction(nameof(GetById), new { id = createdUtente.UtenteId }, createdUtente);
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "Errore database durante la creazione dell'utente");
                return SafeInternalError<UtentiDTO>("Errore durante il salvataggio dei dati");
            }
            catch (ArgumentException argEx)
            {
                _logger.LogWarning(argEx, "Argomento non valido durante la creazione dell'utente");
                return SafeBadRequest<UtentiDTO>(argEx.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante la creazione dell'utente");
                return SafeInternalError<UtentiDTO>(ex.Message);
            }
        }

        // PUT: api/Utenti/5
        [HttpPut("{id}")]
        //[Authorize] // ✅ COMMENTATO PER TEST CON SWAGGER
        public async Task<ActionResult> Update(int id, [FromBody] UtentiDTO utenteDto)
        {
            try
            {
                if (id <= 0)
                    return SafeBadRequest("ID utente non valido");

                if (id != utenteDto.UtenteId)
                    return SafeBadRequest("ID utente non corrispondente");

                if (!IsModelValid(utenteDto))
                    return SafeBadRequest("Dati utente non validi");

                var existing = await _repository.GetByIdAsync(id);
                if (existing == null)
                    return SafeNotFound("Utente");

                await _repository.UpdateAsync(utenteDto);

                // ✅ Audit trail completo
                LogAuditTrail("UPDATE", "Utenti", utenteDto.UtenteId.ToString());
                LogSecurityEvent("UtenteUpdated", new
                {
                    UtenteId = utenteDto.UtenteId,
                    Email = utenteDto.Email,
                    TipoUtente = utenteDto.TipoUtente,
                    Attivo = utenteDto.Attivo,
                    User = User.Identity?.Name ?? "Anonymous",
                    Timestamp = DateTime.UtcNow
                });

                return NoContent();
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "Errore database durante l'aggiornamento dell'utente {Id}", id);
                return SafeInternalError("Errore durante l'aggiornamento dei dati");
            }
            catch (ArgumentException argEx)
            {
                _logger.LogWarning(argEx, "Argomento non valido durante l'aggiornamento dell'utente {Id}", id);
                return SafeBadRequest(argEx.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante l'aggiornamento dell'utente {Id}", id);
                return SafeInternalError(ex.Message);
            }
        }

        // DELETE: api/Utenti/5
        [HttpDelete("{id}")]
        //[Authorize(Roles = "Admin")] // ✅ COMMENTATO PER TEST CON SWAGGER
        public async Task<ActionResult> Delete(int id)
        {
            try
            {
                if (id <= 0)
                    return SafeBadRequest("ID utente non valido");

                var utente = await _repository.GetByIdAsync(id);
                if (utente == null)
                    return SafeNotFound("Utente");

                await _repository.DeleteAsync(id);

                // ✅ Audit trail completo
                LogAuditTrail("DELETE", "Utenti", id.ToString());
                LogSecurityEvent("UtenteDeleted", new
                {
                    UtenteId = id,
                    Email = utente.Email,
                    User = User.Identity?.Name ?? "Anonymous",
                    Timestamp = DateTime.UtcNow
                });

                return NoContent();
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "Errore database durante l'eliminazione dell'utente {Id}", id);
                return SafeInternalError("Errore durante l'eliminazione dei dati");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante l'eliminazione dell'utente {Id}", id);
                return SafeInternalError(ex.Message);
            }
        }

        // GET: api/Utenti/check-email/test@example.com
        [HttpGet("check-email/{email}")]
        [AllowAnonymous] // ✅ VERIFICA EMAIL PUBBLICA
        public async Task<ActionResult<bool>> CheckEmailExists(string email)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(email))
                    return SafeBadRequest<bool>("Email non valida");

                var exists = await _repository.EmailExistsAsync(email);
                return Ok(exists);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante la verifica dell'email {Email}", email);
                return SafeInternalError<bool>(ex.Message);
            }
        }

        // GET: api/Utenti/exists/5
        [HttpGet("exists/{id}")]
        //[Authorize] // ✅ COMMENTATO PER TEST CON SWAGGER
        public async Task<ActionResult<bool>> Exists(int id)
        {
            try
            {
                if (id <= 0)
                    return SafeBadRequest<bool>("ID utente non valido");

                var exists = await _repository.ExistsAsync(id);
                return Ok(exists);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante la verifica dell'esistenza dell'utente {Id}", id);
                return SafeInternalError<bool>(ex.Message);
            }
        }

        // PATCH: api/Utenti/5/attiva
        [HttpPatch("{id}/attiva")]
        //[Authorize(Roles = "Admin")] // ✅ COMMENTATO PER TEST CON SWAGGER
        public async Task<ActionResult> AttivaUtente(int id)
        {
            try
            {
                if (id <= 0)
                    return SafeBadRequest("ID utente non valido");

                var utente = await _repository.GetByIdAsync(id);
                if (utente == null)
                    return SafeNotFound("Utente");

                utente.Attivo = true;
                utente.DataAggiornamento = DateTime.Now;

                await _repository.UpdateAsync(utente);

                // ✅ Audit trail completo
                LogAuditTrail("ACTIVATE", "Utenti", id.ToString());
                LogSecurityEvent("UtenteActivated", new
                {
                    UtenteId = id,
                    Email = utente.Email,
                    User = User.Identity?.Name ?? "Anonymous",
                    Timestamp = DateTime.UtcNow
                });

                return NoContent();
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "Errore database durante l'attivazione dell'utente {Id}", id);
                return SafeInternalError("Errore durante l'attivazione dell'utente");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante l'attivazione dell'utente {Id}", id);
                return SafeInternalError(ex.Message);
            }
        }

        // PATCH: api/Utenti/5/disattiva
        [HttpPatch("{id}/disattiva")]
        //[Authorize(Roles = "Admin")] // ✅ COMMENTATO PER TEST CON SWAGGER
        public async Task<ActionResult> DisattivaUtente(int id)
        {
            try
            {
                if (id <= 0)
                    return SafeBadRequest("ID utente non valido");

                var utente = await _repository.GetByIdAsync(id);
                if (utente == null)
                    return SafeNotFound("Utente");

                utente.Attivo = false;
                utente.DataAggiornamento = DateTime.Now;

                await _repository.UpdateAsync(utente);

                // ✅ Audit trail completo
                LogAuditTrail("DEACTIVATE", "Utenti", id.ToString());
                LogSecurityEvent("UtenteDeactivated", new
                {
                    UtenteId = id,
                    Email = utente.Email,
                    User = User.Identity?.Name ?? "Anonymous",
                    Timestamp = DateTime.UtcNow
                });

                return NoContent();
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "Errore database durante la disattivazione dell'utente {Id}", id);
                return SafeInternalError("Errore durante la disattivazione dell'utente");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante la disattivazione dell'utente {Id}", id);
                return SafeInternalError(ex.Message);
            }
        }
    }
}