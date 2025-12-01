using Database;
using DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Repository.Interface;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BBltZen.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    // [Authorize] // ✅ COMMENTATO PER TEST CON SWAGGER
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
        //[HttpGet]
        //[AllowAnonymous] // ✅ OVERRIDE PER ACCESSO PUBBLICO
        //public async Task<ActionResult<IEnumerable<TavoloDTO>>> GetAll()
        //{
        //    try
        //    {
        //        var tavoli = await _repository.GetAllAsync();
        //        return Ok(tavoli);
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Errore durante il recupero di tutti i tavoli");
        //        return SafeInternalError<IEnumerable<TavoloDTO>>("Errore durante il recupero dei tavoli");
        //    }
        //}

        // GET: api/Tavolo/{id?} - ENDPOINT CRUD (BACKOFFICE)
        // GET: api/Tavolo/{id?}
        // GET: api/Tavolo/{id}
        // ✅ 1. GET /api/Tavolo - USA [FromQuery] PER ID
        [HttpGet("id")]
        [AllowAnonymous]
        // [EnableRateLimiting("Default")]
        public async Task<ActionResult> GetById([FromQuery] int? id, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                // ✅ SE ID NULL → LISTA COMPLETA
                if (!id.HasValue)
                {
                    var result = await _repository.GetAllAsync(page, pageSize);
                    return Ok(new
                    {
                        Message = $"Trovati {result.TotalCount} tavoli",
                        result.Data,
                        Pagination = new
                        {
                            result.Page,
                            result.PageSize,
                            result.TotalCount,
                            result.TotalPages,
                            result.HasPrevious,
                            result.HasNext
                        }
                    });
                }

                // ✅ SE ID VALORIZZATO → SINGOLO ELEMENTO
                if (id <= 0) return SafeBadRequest("ID tavolo non valido");

                var tavolo = await _repository.GetByIdAsync(id.Value);
                return tavolo == null ? SafeNotFound("Tavolo") : Ok(tavolo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante il recupero del tavolo {Id}", id);
                return SafeInternalError("Errore durante il recupero del tavolo");
            }
        }

        // GET: api/Tavolo/numero/{numero}
        [HttpGet("numero")]
        // [AllowAnonymous]
        // [EnableRateLimiting("Default")]
        public async Task<ActionResult> GetByNumero([FromQuery] int? numero, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                // ✅ SE NUMERO NULL → LISTA COMPLETA
                if (!numero.HasValue)
                {
                    var result = await _repository.GetAllAsync(page, pageSize);
                    return Ok(new
                    {
                        Message = $"Trovati {result.TotalCount} tavoli",
                        result.Data,
                        Pagination = new
                        {
                            result.Page,
                            result.PageSize,
                            result.TotalCount,
                            result.TotalPages,
                            result.HasPrevious,
                            result.HasNext
                        }
                    });
                }

                // ✅ SE NUMERO VALORIZZATO → SINGOLO ELEMENTO
                if (numero <= 0) return SafeBadRequest("Numero tavolo non valido");

                var tavolo = await _repository.GetByNumeroAsync(numero.Value);
                return tavolo == null ? SafeNotFound("Tavolo") : Ok(tavolo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante il recupero del tavolo numero {Numero}", numero);
                return SafeInternalError("Errore durante il recupero del tavolo");
            }
        }

        // GET: api/Tavolo/disponibili
        [HttpGet("disponibili")]
        [AllowAnonymous]
        // [EnableRateLimiting("Default")]
        public async Task<ActionResult> GetDisponibili([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var result = await _repository.GetDisponibiliAsync(page, pageSize);

                result.Message = result.TotalCount > 0
                    ? $"Trovati {result.TotalCount} tavoli disponibili (pagina {result.Page} di {result.TotalPages})"
                    : "Nessun tavolo disponibile trovato";

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante il recupero dei tavoli disponibili pagina {Page}", page);
                return SafeInternalError("Errore durante il recupero dei tavoli disponibili");
            }
        }

        // GET: api/Tavolo/zona/{zona}
        // GET: api/Tavolo/zona
        [HttpGet("zona")]
        // [AllowAnonymous]
        // [EnableRateLimiting("Default")]
        public async Task<ActionResult> GetByZona([FromQuery] string? zona = null, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var result = await _repository.GetByZonaAsync(zona, page, pageSize);

                // ✅ MESSAGGIO DINAMICO
                result.Message = !string.IsNullOrWhiteSpace(zona)
                    ? (result.TotalCount > 0
                        ? $"Trovati {result.TotalCount} tavoli per la zona '{zona}' (pagina {result.Page} di {result.TotalPages})"
                        : $"Nessun tavolo trovato per la zona '{zona}'")
                    : $"Trovati {result.TotalCount} tavoli (tutte le zone, pagina {result.Page} di {result.TotalPages})";

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante il recupero dei tavoli per zona {Zona}", zona);
                return SafeInternalError("Errore durante il recupero dei tavoli");
            }
        }

        // POST: api/Tavolo
        [HttpPost]
        // [Authorize(Roles = "Admin")] // ✅ COMMENTATO PER TEST CON SWAGGER
        // [EnableRateLimiting("Default")]
        public async Task<ActionResult<TavoloDTO>> Create([FromBody] TavoloDTO tavoloDto)
        {
            try
            {
                if (!IsModelValid(tavoloDto))
                    return SafeBadRequest("Dati tavolo non validi"); // ✅ CORRETTO: senza <T>

                if (await _repository.NumeroExistsAsync(tavoloDto.Numero))
                    return SafeBadRequest("Numero tavolo già esistente"); // ✅ CORRETTO: senza <T>

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
                return SafeBadRequest(argEx.Message); // ✅ CORRETTO: senza <T>
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "Errore database durante la creazione del tavolo");
                return SafeInternalError("Errore durante il salvataggio"); // ✅ CORRETTO: senza <T>
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante la creazione del tavolo");
                return SafeInternalError("Errore durante la creazione"); // ✅ CORRETTO: senza <T>
            }
        }

        // PUT: api/Tavolo/5
        [HttpPut("{id}")]
        // [Authorize(Roles = "Admin")] // ✅ COMMENTATO PER TEST CON SWAGGER
        // [EnableRateLimiting("Default")]
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
        // [Authorize(Roles = "Admin")] // ✅ COMMENTATO PER TEST CON SWAGGER
        // [EnableRateLimiting("Default")]
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
        //[HttpGet("frontend")]
        //[AllowAnonymous]
        //public async Task<ActionResult> GetAllPerFrontend([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        //{
        //    try
        //    {
        //        var result = await _repository.GetAllPerFrontendAsync(page, pageSize);
        
        //        result.Message = result.TotalCount > 0
        //            ? $"Trovati {result.TotalCount} tavoli (pagina {result.Page} di {result.TotalPages})"
        //            : "Nessun tavolo trovato";

        //        return Ok(result);
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Errore durante il recupero dei tavoli per frontend pagina {Page}", page);
        //        return SafeInternalError("Errore durante il caricamento dei tavoli");
        //    }
        //}


        // GET: api/Tavolo/frontend/disponibili
        [HttpGet("frontend/disponibili")]
        [AllowAnonymous]
        // [EnableRateLimiting("Default")]
        public async Task<ActionResult> GetDisponibiliPerFrontend([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var result = await _repository.GetDisponibiliPerFrontendAsync(page, pageSize);

                result.Message = result.TotalCount > 0
                    ? $"Trovati {result.TotalCount} tavoli disponibili (pagina {result.Page} di {result.TotalPages})"
                    : "Nessun tavolo disponibile trovato";

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante il recupero dei tavoli disponibili per frontend pagina {Page}", page);
                return SafeInternalError("Errore durante il caricamento dei tavoli disponibili");
            }
        }

        // GET: api/Tavolo/frontend/zona
        [HttpGet("frontend/zona")]
        [AllowAnonymous]
        // [EnableRateLimiting("Default")]
        public async Task<ActionResult> GetByZonaPerFrontend([FromQuery] string? zona = null, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var result = await _repository.GetByZonaPerFrontendAsync(zona, page, pageSize);

                // ✅ MESSAGGIO DINAMICO
                result.Message = !string.IsNullOrWhiteSpace(zona)
                    ? (result.TotalCount > 0
                        ? $"Trovati {result.TotalCount} tavoli per la zona '{zona}' (pagina {result.Page} di {result.TotalPages})"
                        : $"Nessun tavolo trovato per la zona '{zona}'")
                    : $"Trovati {result.TotalCount} tavoli (tutte le zone, pagina {result.Page} di {result.TotalPages})";

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante il recupero dei tavoli per zona {Zona} per frontend", zona);
                return SafeInternalError("Errore durante il caricamento dei tavoli");
            }
        }

        // GET: api/Tavolo/frontend/numero/{numero?} - ENDPOINT FRONTEND (CLIENTI)
        // GET: api/Tavolo/frontend/numero/{numero?}
        [HttpGet("frontend/numero")]
        [AllowAnonymous]
        // [EnableRateLimiting("Default")]
        public async Task<ActionResult> GetByNumeroPerFrontend([FromQuery] int? numero, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                // ✅ SE NUMERO NULL → LISTA FRONTEND COMPLETA
                if (!numero.HasValue)
                {
                    var result = await _repository.GetByZonaPerFrontendAsync(null, page, pageSize);
                    return Ok(new
                    {
                        Message = $"Trovati {result.TotalCount} tavoli",
                        result.Data,
                        Pagination = new
                        {
                            result.Page,
                            result.PageSize,
                            result.TotalCount,
                            result.TotalPages,
                            result.HasPrevious,
                            result.HasNext
                        }
                    });
                }

                // ✅ SE NUMERO VALORIZZATO → SINGOLO ELEMENTO FRONTEND
                if (numero <= 0) return SafeBadRequest("Numero tavolo non valido");

                var tavolo = await _repository.GetByNumeroPerFrontendAsync(numero.Value);
                return tavolo == null ? SafeNotFound("Tavolo") : Ok(tavolo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante il recupero del tavolo numero {Numero} per frontend", numero);
                return SafeInternalError("Errore durante il caricamento del tavolo");
            }
        }

        // ✅ NUOVI ENDPOINT BUSINESS

        // PATCH: api/Tavolo/5/toggle-disponibilita
        [HttpPatch("{id}/toggle-disponibilita")]
        // [Authorize(Roles = "Admin,Impiegato")] // ✅ COMMENTATO PER TEST CON SWAGGER
        // [EnableRateLimiting("Default")]
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
        // [Authorize(Roles = "Admin,Impiegato")] // ✅ COMMENTATO PER TEST CON SWAGGER
        // [EnableRateLimiting("Default")]
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