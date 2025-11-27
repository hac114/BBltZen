using Database;
using DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Repository.Interface;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BBltZen.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    //[Authorize] // ✅ COMMENTATO PER TEST CON SWAGGER
    public class TavoloController(
    ITavoloRepository repository,
    BubbleTeaContext context, // ✅ AGGIUNTO IL CONTEXT
    IWebHostEnvironment environment,
    ILogger<TavoloController> logger)
    : SecureBaseController(environment, logger)

    {
        private readonly ITavoloRepository _repository = repository;
        private readonly BubbleTeaContext _context = context; // ✅ INIZIALIZZATO IL CONTEXT    

        // ✅ ENDPOINT CRUD ESISTENTI (per admin/backoffice)

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
                return SafeInternalError<IEnumerable<TavoloDTO>>("Errore durante il recupero dei tavoli");
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
                return SafeInternalError<IEnumerable<TavoloDTO>>("Errore durante il recupero dei tavoli disponibili");
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

                if (await _repository.NumeroExistsAsync(tavoloDto.Numero))
                    return SafeBadRequest<TavoloDTO>("Numero tavolo già esistente");

                var result = await _repository.AddAsync(tavoloDto);

                LogAuditTrail("CREATE", "Tavolo", result.TavoloId.ToString());
                LogSecurityEvent("TavoloCreated", new
                {
                    result.TavoloId,
                    result.Numero,
                    result.Zona,
                    UserId = GetCurrentUserIdOrDefault()
                });

                return CreatedAtAction(nameof(GetById), new { id = result.TavoloId }, result);
            }
            catch (ArgumentException argEx)
            {
                return SafeBadRequest<TavoloDTO>(argEx.Message);
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "Errore database durante la creazione del tavolo");
                return SafeInternalError<TavoloDTO>("Errore durante il salvataggio");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante la creazione del tavolo");
                return SafeInternalError<TavoloDTO>("Errore durante la creazione");
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

                if (!await _repository.ExistsAsync(id))
                    return SafeNotFound("Tavolo");

                if (await _repository.NumeroExistsAsync(tavoloDto.Numero, id))
                    return SafeBadRequest("Numero tavolo già esistente");

                await _repository.UpdateAsync(tavoloDto);

                LogAuditTrail("UPDATE", "Tavolo", tavoloDto.TavoloId.ToString());
                LogSecurityEvent("TavoloUpdated", new
                {
                    tavoloDto.TavoloId,
                    tavoloDto.Numero,
                    UserId = GetCurrentUserIdOrDefault()
                });

                return NoContent();
            }
            catch (ArgumentException argEx)
            {
                return SafeBadRequest(argEx.Message);
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "Errore database durante l'aggiornamento del tavolo {Id}", id);
                return SafeInternalError("Errore durante l'aggiornamento");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante l'aggiornamento del tavolo {Id}", id);
                return SafeInternalError("Errore durante l'aggiornamento");
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

                // ✅ CONTROLLO VINCOLI REFERENZIALI NEL CONTROLLER
                bool hasClienti = await _context.Cliente.AnyAsync(c => c.TavoloId == id);
                bool hasSessioni = await _context.SessioniQr.AnyAsync(sq => sq.TavoloId == id);

                if (hasClienti || hasSessioni)
                {
                    var errorMessage = "Impossibile eliminare il tavolo: ";
                    if (hasClienti) errorMessage += "sono presenti clienti associati. ";
                    if (hasSessioni) errorMessage += "sono presenti sessioni QR associate.";

                    return SafeBadRequest(errorMessage.Trim());
                }

                await _repository.DeleteAsync(id);

                // ✅ AUDIT & SECURITY
                LogAuditTrail("DELETE", "Tavolo", id.ToString());
                LogSecurityEvent("TavoloDeleted", new
                {
                    id,
                    tavolo.Numero,
                    UserId = GetCurrentUserIdOrDefault()
                });

                return NoContent();
            }
            catch (InvalidOperationException invOpEx) // ✅ INMEMORY EXCEPTION
            {
                // ✅ GESTIONE SPECIFICA PER DIPENDENZE IN MEMORY
                if (_environment.IsDevelopment())
                    return SafeBadRequest($"Errore eliminazione: {invOpEx.Message}");
                else
                    return SafeBadRequest("Impossibile eliminare il tavolo: sono presenti dipendenze");
            }
            catch (DbUpdateException dbEx) // ✅ DATABASE REAL EXCEPTION
            {
                // ✅ GESTIONE SPECIFICA PER DIPENDENZE DATABASE
                _logger.LogError(dbEx, "Errore database durante l'eliminazione del tavolo {Id}", id);

                if (_environment.IsDevelopment())
                    return SafeBadRequest($"Errore database: {dbEx.Message}");
                else
                    return SafeBadRequest("Impossibile eliminare il tavolo: sono presenti dipendenze");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante l'eliminazione del tavolo {Id}", id);
                return SafeInternalError("Errore durante l'eliminazione");
            }
        }

        // ✅ NUOVI ENDPOINT PER FRONTEND (formattati per clienti)

        // GET: api/Tavolo/frontend
        [HttpGet("frontend")]
        [AllowAnonymous] // ✅ ACCESSO PUBBLICO PER CLIENTI
        public async Task<ActionResult<IEnumerable<TavoloFrontendDTO>>> GetAllPerFrontend()
        {
            try
            {
                var tavoli = await _repository.GetAllPerFrontendAsync();
                return Ok(tavoli);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante il recupero dei tavoli per frontend");

                if (_environment.IsDevelopment())
                    return SafeInternalError<IEnumerable<TavoloFrontendDTO>>($"Errore: {ex.Message}");
                else
                    return SafeInternalError<IEnumerable<TavoloFrontendDTO>>("Errore durante il caricamento dei tavoli");
            }
        }

        // GET: api/Tavolo/frontend/disponibili
        [HttpGet("frontend/disponibili")]
        [AllowAnonymous] // ✅ ACCESSO PUBBLICO PER CLIENTI
        public async Task<ActionResult<IEnumerable<TavoloFrontendDTO>>> GetDisponibiliPerFrontend()
        {
            try
            {
                var tavoli = await _repository.GetDisponibiliPerFrontendAsync();
                return Ok(tavoli);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante il recupero dei tavoli disponibili per frontend");

                if (_environment.IsDevelopment())
                    return SafeInternalError<IEnumerable<TavoloFrontendDTO>>($"Errore: {ex.Message}");
                else
                    return SafeInternalError<IEnumerable<TavoloFrontendDTO>>("Errore durante il caricamento dei tavoli disponibili");
            }
        }

        // GET: api/Tavolo/frontend/zona/{zona}
        [HttpGet("frontend/zona/{zona}")]
        [AllowAnonymous] // ✅ ACCESSO PUBBLICO PER CLIENTI
        public async Task<ActionResult<IEnumerable<TavoloFrontendDTO>>> GetByZonaPerFrontend(string zona)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(zona))
                    return SafeBadRequest<IEnumerable<TavoloFrontendDTO>>("Zona non valida");

                var tavoli = await _repository.GetByZonaPerFrontendAsync(zona);
                return Ok(tavoli);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante il recupero dei tavoli per zona {Zona} per frontend", zona);

                if (_environment.IsDevelopment())
                    return SafeInternalError<IEnumerable<TavoloFrontendDTO>>($"Errore: {ex.Message}");
                else
                    return SafeInternalError<IEnumerable<TavoloFrontendDTO>>("Errore durante il caricamento dei tavoli");
            }
        }

        // GET: api/Tavolo/frontend/numero/{numero}
        [HttpGet("frontend/numero/{numero}")]
        [AllowAnonymous] // ✅ ACCESSO PUBBLICO PER CLIENTI
        public async Task<ActionResult<TavoloFrontendDTO>> GetByNumeroPerFrontend(int numero)
        {
            try
            {
                if (numero <= 0)
                    return SafeBadRequest<TavoloFrontendDTO>("Numero tavolo non valido");

                var tavolo = await _repository.GetByNumeroPerFrontendAsync(numero);

                if (tavolo == null)
                    return SafeNotFound<TavoloFrontendDTO>("Tavolo");

                return Ok(tavolo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante il recupero del tavolo numero {Numero} per frontend", numero);

                if (_environment.IsDevelopment())
                    return SafeInternalError<TavoloFrontendDTO>($"Errore: {ex.Message}");
                else
                    return SafeInternalError<TavoloFrontendDTO>("Errore durante il caricamento del tavolo");
            }
        }

        // ✅ NUOVI ENDPOINT BUSINESS

        // PATCH: api/Tavolo/5/toggle-disponibilita
        [HttpPatch("{id}/toggle-disponibilita")]
        //[Authorize(Roles = "Admin,Impiegato")] // ✅ COMMENTATO PER TEST CON SWAGGER
        public async Task<ActionResult<object>> ToggleDisponibilita(int id)
        {
            try
            {
                if (id <= 0)
                    return SafeBadRequest<object>("ID tavolo non valido");

                if (!await _repository.ExistsAsync(id))
                    return SafeNotFound<object>("Tavolo");

                var nuovaDisponibilita = await _repository.ToggleDisponibilitaAsync(id);

                LogAuditTrail("TOGGLE_DISPONIBILITA", "Tavolo", id.ToString());
                LogSecurityEvent("TavoloDisponibilitaToggled", new
                {
                    id,
                    NuovaDisponibilita = nuovaDisponibilita,
                    UserId = GetCurrentUserIdOrDefault()
                });

                // ✅ RISPOSTA DETTAGLIATA PER SVILUPPO, SEMPLICE PER PRODUZIONE
                if (_environment.IsDevelopment())
                {
                    return Ok(new
                    {
                        TavoloId = id,
                        Disponibile = nuovaDisponibilita,
                        Messaggio = $"Disponibilità aggiornata a: {(nuovaDisponibilita ? "SI" : "NO")}",
                        Timestamp = DateTime.UtcNow
                    });
                }
                else
                {
                    return Ok(new
                    {
                        Messaggio = "Disponibilità aggiornata con successo",
                        Disponibile = nuovaDisponibilita ? "SI" : "NO"
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante il toggle disponibilità del tavolo {Id}", id);

                if (_environment.IsDevelopment())
                    return SafeInternalError<object>($"Errore: {ex.Message}");
                else
                    return SafeInternalError<object>("Errore durante l'aggiornamento della disponibilità");
            }
        }

        // PATCH: api/Tavolo/numero/5/toggle-disponibilita
        [HttpPatch("numero/{numero}/toggle-disponibilita")]
        //[Authorize(Roles = "Admin,Impiegato")] // ✅ COMMENTATO PER TEST CON SWAGGER
        public async Task<ActionResult<object>> ToggleDisponibilitaByNumero(int numero)
        {
            try
            {
                if (numero <= 0)
                    return SafeBadRequest<object>("Numero tavolo non valido");

                var nuovaDisponibilita = await _repository.ToggleDisponibilitaByNumeroAsync(numero);

                if (!nuovaDisponibilita && !await _repository.NumeroExistsAsync(numero))
                    return SafeNotFound<object>("Tavolo");

                LogAuditTrail("TOGGLE_DISPONIBILITA_BY_NUMERO", "Tavolo", numero.ToString());
                LogSecurityEvent("TavoloDisponibilitaToggledByNumero", new
                {
                    NumeroTavolo = numero,
                    NuovaDisponibilita = nuovaDisponibilita,
                    UserId = GetCurrentUserIdOrDefault()
                });

                if (_environment.IsDevelopment())
                {
                    return Ok(new
                    {
                        NumeroTavolo = numero,
                        Disponibile = nuovaDisponibilita,
                        Messaggio = $"Disponibilità aggiornata a: {(nuovaDisponibilita ? "SI" : "NO")}",
                        Timestamp = DateTime.UtcNow
                    });
                }
                else
                {
                    return Ok(new
                    {
                        Messaggio = "Disponibilità aggiornata con successo",
                        Disponibile = nuovaDisponibilita ? "SI" : "NO"
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante il toggle disponibilità del tavolo numero {Numero}", numero);

                if (_environment.IsDevelopment())
                    return SafeInternalError<object>($"Errore: {ex.Message}");
                else
                    return SafeInternalError<object>("Errore durante l'aggiornamento della disponibilità");
            }
        }
    }
}