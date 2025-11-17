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
        public async Task<ActionResult<UtentiDTO>> GetById(int id)
        {
            try
            {
                if (id <= 0)
                    return SafeBadRequest<UtentiDTO>("ID utente non valido");

                var utente = await _repository.GetByIdAsync(id);
                return utente == null ? SafeNotFound<UtentiDTO>("Utente") : Ok(utente);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante il recupero dell'utente {Id}", id);
                return SafeInternalError<UtentiDTO>(ex.Message);
            }
        }

        // GET: api/Utenti/email/test@example.com
        [HttpGet("email/{email}")]
        public async Task<ActionResult<UtentiDTO>> GetByEmail(string email)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(email))
                    return SafeBadRequest<UtentiDTO>("Email non valida");

                var utente = await _repository.GetByEmailAsync(email);
                return utente == null ? SafeNotFound<UtentiDTO>("Utente") : Ok(utente);
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
        [AllowAnonymous]
        public async Task<ActionResult<UtentiDTO>> Create([FromBody] UtentiDTO utenteDto)
        {
            try
            {
                if (!IsModelValid(utenteDto))
                    return SafeBadRequest<UtentiDTO>("Dati utente non validi");

                // ✅ Verifica se l'email esiste già
                if (!string.IsNullOrEmpty(utenteDto.Email) && await _repository.EmailExistsAsync(utenteDto.Email))
                    return SafeBadRequest<UtentiDTO>("Email già registrata");

                // ✅ CORRETTO: Assegna il risultato di AddAsync
                var result = await _repository.AddAsync(utenteDto);

                // ✅ Audit trail completo
                LogAuditTrail("CREATE", "Utenti", result.UtenteId.ToString());

                // ✅ CORRETTO: Sintassi semplificata
                LogSecurityEvent("UtenteCreated", new
                {
                    result.UtenteId,
                    result.Email,
                    result.TipoUtente,
                    User = User.Identity?.Name ?? "Anonymous",
                    Timestamp = DateTime.UtcNow
                });

                return CreatedAtAction(nameof(GetById), new { id = result.UtenteId }, result);
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
        public async Task<ActionResult> Update(int id, [FromBody] UtentiDTO utenteDto)
        {
            try
            {
                if (id <= 0 || id != utenteDto.UtenteId || !IsModelValid(utenteDto))
                    return SafeBadRequest("Dati utente non validi");

                var existing = await _repository.GetByIdAsync(id);
                if (existing == null)
                    return SafeNotFound("Utente");

                await _repository.UpdateAsync(utenteDto);

                LogAuditTrail("UPDATE", "Utenti", utenteDto.UtenteId.ToString());

                // ✅ CORRETTO: Sintassi semplificata
                LogSecurityEvent("UtenteUpdated", new
                {
                    utenteDto.UtenteId,
                    utenteDto.Email,
                    utenteDto.TipoUtente,
                    utenteDto.Attivo,
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
            catch (ArgumentException)
            {
                return SafeBadRequest("Utente non trovato");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante l'aggiornamento dell'utente {Id}", id);
                return SafeInternalError(ex.Message);
            }
        }

        // DELETE: api/Utenti/5
        [HttpDelete("{id}")]
        //[Authorize(Roles = "Admin")]
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

                // ✅ CORRETTO: Sintassi semplificata
                LogSecurityEvent("UtenteDeleted", new
                {
                    UtenteId = id,
                    utente.Email, // ✅ SEMPLIFICATO: invece di Email = utente.Email
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
        [AllowAnonymous]
        public async Task<ActionResult<bool>> CheckEmailExists(string email)
        {
            try
            {
                return string.IsNullOrWhiteSpace(email)
                    ? SafeBadRequest<bool>("Email non valida")
                    : Ok(await _repository.EmailExistsAsync(email));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante la verifica dell'email {Email}", email);
                return SafeInternalError<bool>(ex.Message);
            }
        }

        // GET: api/Utenti/exists/5
        [HttpGet("exists/{id}")]
        public async Task<ActionResult<bool>> Exists(int id)
        {
            try
            {
                return id <= 0
                    ? SafeBadRequest<bool>("ID utente non valido")
                    : Ok(await _repository.ExistsAsync(id));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante la verifica dell'esistenza dell'utente {Id}", id);
                return SafeInternalError<bool>(ex.Message);
            }
        }

        /// PATCH: api/Utenti/5/attiva
        [HttpPatch("{id}/attiva")]
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
                await _repository.UpdateAsync(utente);

                LogAuditTrail("ACTIVATE", "Utenti", id.ToString());

                // ✅ CORRETTO: Sintassi semplificata
                LogSecurityEvent("UtenteActivated", new
                {
                    UtenteId = id,
                    utente.Email,
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
                await _repository.UpdateAsync(utente);

                LogAuditTrail("DEACTIVATE", "Utenti", id.ToString());

                // ✅ CORRETTO: Sintassi semplificata
                LogSecurityEvent("UtenteDeactivated", new
                {
                    UtenteId = id,
                    utente.Email,
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