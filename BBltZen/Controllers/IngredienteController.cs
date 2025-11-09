using Microsoft.AspNetCore.Mvc;
using Repository.Interface;
using DTO;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Database;
using System.Linq;

namespace BBltZen.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [AllowAnonymous] // ✅ OVERRIDE DELL'[Authorize] DEL BASE CONTROLLER
    public class IngredienteController : SecureBaseController
    {
        private readonly IIngredienteRepository _ingredienteRepository;
        private readonly BubbleTeaContext _context;

        public IngredienteController(
            IIngredienteRepository ingredienteRepository,
            BubbleTeaContext context,
            IWebHostEnvironment environment,
            ILogger<IngredienteController> logger)
            : base(environment, logger)
        {
            _ingredienteRepository = ingredienteRepository;
            _context = context;
        }

        // GET: api/Ingrediente
        // ✅ PER ADMIN: Mostra TUTTI gli ingredienti (anche non disponibili)
        [HttpGet]
        [Authorize(Roles = "admin")] // ✅ Solo admin può vedere tutti gli ingredienti
        public async Task<ActionResult<IEnumerable<IngredienteDTO>>> GetAll()
        {
            try
            {
                var ingredienti = await _ingredienteRepository.GetAllAsync();
                return Ok(ingredienti);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore durante il recupero di tutti gli ingredienti");
                return SafeInternalError<IEnumerable<IngredienteDTO>>("Errore durante il recupero degli ingredienti");
            }
        }

        // GET: api/Ingrediente/5
        // ✅ PER TUTTI: Cerca per ID (se DISPONIBILE) - altrimenti 404 per clienti
        [HttpGet("{id}")]
        [AllowAnonymous] // ✅ ESPLICITO PER ENDPOINT GET
        public async Task<ActionResult<IngredienteDTO>> GetById(int id)
        {
            try
            {
                if (id <= 0)
                    return SafeBadRequest<IngredienteDTO>("ID ingrediente non valido");

                var ingrediente = await _ingredienteRepository.GetByIdAsync(id);

                if (ingrediente == null)
                    return SafeNotFound<IngredienteDTO>("Ingrediente");

                // ✅ Se l'utente NON è admin, mostra solo quelli DISPONIBILI
                if (!User.IsInRole("admin") && !ingrediente.Disponibile)
                {
                    LogSecurityEvent("AttemptAccessUnavailableIngredient", new
                    {
                        IngredienteId = id,
                        User = User.Identity?.Name,
                        Timestamp = DateTime.UtcNow
                    });
                    return SafeNotFound<IngredienteDTO>("Ingrediente");
                }

                return Ok(ingrediente);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore durante il recupero dell'ingrediente {Id}", id);
                return SafeInternalError<IngredienteDTO>("Errore durante il recupero dell'ingrediente");
            }
        }

        // GET: api/Ingrediente/categoria/5
        // ✅ PER TUTTI: Mostra ingredienti per categoria (solo disponibili per clienti)
        [HttpGet("categoria/{categoriaId}")]
        [AllowAnonymous] // ✅ ESPLICITO PER ENDPOINT GET
        public async Task<ActionResult<IEnumerable<IngredienteDTO>>> GetByCategoria(int categoriaId)
        {
            try
            {
                if (categoriaId <= 0)
                    return SafeBadRequest<IEnumerable<IngredienteDTO>>("ID categoria non valido");

                // ✅ Verifica se la categoria esiste
                var categoriaEsiste = await _context.CategoriaIngrediente.AnyAsync(c => c.CategoriaId == categoriaId);
                if (!categoriaEsiste)
                    return SafeNotFound<IEnumerable<IngredienteDTO>>("Categoria ingrediente");

                var ingredienti = await _ingredienteRepository.GetByCategoriaAsync(categoriaId);

                // ✅ Se l'utente NON è admin, filtra solo quelli disponibili
                if (!User.IsInRole("admin"))
                {
                    ingredienti = ingredienti.Where(i => i.Disponibile);
                }

                return Ok(ingredienti);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore durante il recupero degli ingredienti per categoria {CategoriaId}", categoriaId);
                return SafeInternalError<IEnumerable<IngredienteDTO>>("Errore durante il recupero degli ingredienti");
            }
        }

        // GET: api/Ingrediente/disponibili
        // ✅ PER TUTTI: Solo ingredienti DISPONIBILI
        [HttpGet("disponibili")]
        [AllowAnonymous] // ✅ ESPLICITO PER ENDPOINT GET
        public async Task<ActionResult<IEnumerable<IngredienteDTO>>> GetDisponibili()
        {
            try
            {
                var ingredienti = await _ingredienteRepository.GetDisponibiliAsync();
                return Ok(ingredienti);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore durante il recupero degli ingredienti disponibili");
                return SafeInternalError<IEnumerable<IngredienteDTO>>("Errore durante il recupero degli ingredienti disponibili");
            }
        }

        // POST: api/Ingrediente
        // ✅ SOLO ADMIN: Crea nuovo ingrediente
        [HttpPost]
        [Authorize(Roles = "admin")] // ✅ Solo admin può creare ingredienti
        public async Task<ActionResult<IngredienteDTO>> Create([FromBody] IngredienteDTO ingredienteDto)
        {
            try
            {
                if (!IsModelValid(ingredienteDto))
                    return SafeBadRequest<IngredienteDTO>("Dati ingrediente non validi");

                // ✅ Verifica se la categoria esiste
                var categoriaEsiste = await _context.CategoriaIngrediente.AnyAsync(c => c.CategoriaId == ingredienteDto.CategoriaId);
                if (!categoriaEsiste)
                    return SafeBadRequest<IngredienteDTO>("Categoria ingrediente non trovata");

                // ✅ Verifica se esiste già un ingrediente con lo stesso nome
                var nomeEsistente = await _context.Ingrediente
                    .AnyAsync(i => i.Ingrediente1.ToLower() == ingredienteDto.Nome.ToLower());

                if (nomeEsistente)
                    return SafeBadRequest<IngredienteDTO>("Esiste già un ingrediente con questo nome");

                await _ingredienteRepository.AddAsync(ingredienteDto);

                // ✅ Audit trail
                LogAuditTrail("CREATE_INGREDIENTE", "Ingrediente", ingredienteDto.IngredienteId.ToString());

                // ✅ Security event completo con timestamp
                LogSecurityEvent("IngredienteCreated", new
                {
                    IngredienteId = ingredienteDto.IngredienteId,
                    Nome = ingredienteDto.Nome,
                    CategoriaId = ingredienteDto.CategoriaId,
                    PrezzoAggiunto = ingredienteDto.PrezzoAggiunto,
                    IsAvailable = ingredienteDto.Disponibile,
                    User = User.Identity?.Name,
                    Timestamp = DateTime.UtcNow
                });

                return CreatedAtAction(nameof(GetById), new { id = ingredienteDto.IngredienteId }, ingredienteDto);
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "Errore database durante la creazione dell'ingrediente");
                return SafeInternalError<IngredienteDTO>("Errore durante il salvataggio dell'ingrediente");
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore durante la creazione dell'ingrediente");
                return SafeInternalError<IngredienteDTO>("Errore durante la creazione dell'ingrediente");
            }
        }

        // PUT: api/Ingrediente/5
        // ✅ SOLO ADMIN: Aggiorna ingrediente (può modificare anche disponibilità)
        [HttpPut("{id}")]
        [Authorize(Roles = "admin")] // ✅ Solo admin può modificare ingredienti
        public async Task<ActionResult> Update(int id, [FromBody] IngredienteDTO ingredienteDto)
        {
            try
            {
                if (id <= 0)
                    return SafeBadRequest("ID ingrediente non valido");

                if (id != ingredienteDto.IngredienteId)
                    return SafeBadRequest("ID ingrediente non corrispondente");

                if (!IsModelValid(ingredienteDto))
                    return SafeBadRequest("Dati ingrediente non validi");

                var existingIngrediente = await _ingredienteRepository.GetByIdAsync(id);
                if (existingIngrediente == null)
                    return SafeNotFound("Ingrediente");

                // ✅ Verifica se la categoria esiste
                var categoriaEsiste = await _context.CategoriaIngrediente.AnyAsync(c => c.CategoriaId == ingredienteDto.CategoriaId);
                if (!categoriaEsiste)
                    return SafeBadRequest("Categoria ingrediente non trovata");

                // ✅ Verifica se esiste già un altro ingrediente con lo stesso nome
                var nomeDuplicato = await _context.Ingrediente
                    .AnyAsync(i => i.Ingrediente1.ToLower() == ingredienteDto.Nome.ToLower() && i.IngredienteId != id);

                if (nomeDuplicato)
                    return SafeBadRequest("Esiste già un altro ingrediente con questo nome");

                await _ingredienteRepository.UpdateAsync(ingredienteDto);

                // ✅ Audit trail
                LogAuditTrail("UPDATE_INGREDIENTE", "Ingrediente", ingredienteDto.IngredienteId.ToString());

                // ✅ Security event completo con timestamp
                LogSecurityEvent("IngredienteUpdated", new
                {
                    IngredienteId = ingredienteDto.IngredienteId,
                    OldNome = existingIngrediente.Nome,
                    NewNome = ingredienteDto.Nome,
                    OldCategoriaId = existingIngrediente.CategoriaId,
                    NewCategoriaId = ingredienteDto.CategoriaId,
                    OldPrezzoAggiunto = existingIngrediente.PrezzoAggiunto,
                    NewPrezzoAggiunto = ingredienteDto.PrezzoAggiunto,
                    OldDisponibile = existingIngrediente.Disponibile,
                    NewDisponibile = ingredienteDto.Disponibile,
                    User = User.Identity?.Name,
                    Timestamp = DateTime.UtcNow
                });

                return NoContent();
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "Errore database durante l'aggiornamento dell'ingrediente {Id}", id);
                return SafeInternalError("Errore durante l'aggiornamento dell'ingrediente");
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore durante l'aggiornamento dell'ingrediente {Id}", id);
                return SafeInternalError("Errore durante l'aggiornamento dell'ingrediente");
            }
        }

        // DELETE: api/Ingrediente/5
        // ✅ SOLO ADMIN: Eliminazione definitiva (HARD DELETE)
        [HttpDelete("{id}")]
        [Authorize(Roles = "admin")] // ✅ Solo admin può eliminare ingredienti
        public async Task<ActionResult> Delete(int id)
        {
            try
            {
                if (id <= 0)
                    return SafeBadRequest("ID ingrediente non valido");

                var existingIngrediente = await _ingredienteRepository.GetByIdAsync(id);
                if (existingIngrediente == null)
                    return SafeNotFound("Ingrediente");

                await _ingredienteRepository.DeleteAsync(id);

                // ✅ Audit trail
                LogAuditTrail("HARD_DELETE_INGREDIENTE", "Ingrediente", id.ToString());

                // ✅ Security event completo con timestamp
                LogSecurityEvent("IngredienteHardDeleted", new
                {
                    IngredienteId = id,
                    Nome = existingIngrediente.Nome,
                    CategoriaId = existingIngrediente.CategoriaId,
                    User = User.Identity?.Name,
                    Timestamp = DateTime.UtcNow
                });

                return NoContent();
            }
            catch (System.InvalidOperationException ex)
            {
                // ✅ Gestione specifica per dipendenze
                _logger.LogWarning(ex, "Tentativo di eliminazione ingrediente {Id} con dipendenze", id);
                return SafeBadRequest(ex.Message);
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "Errore database durante l'eliminazione definitiva dell'ingrediente {Id}", id);
                return SafeInternalError("Errore durante l'eliminazione dell'ingrediente");
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore durante l'eliminazione definitiva dell'ingrediente {Id}", id);
                return SafeInternalError("Errore durante l'eliminazione dell'ingrediente");
            }
        }

        // POST: api/Ingrediente/{id}/toggle-disponibilita
        [HttpPost("{id}/toggle-disponibilita")]
        [Authorize(Roles = "admin")] // ✅ Solo admin può modificare disponibilità
        public async Task<IActionResult> ToggleDisponibilita(int id)
        {
            try
            {
                if (id <= 0)
                    return SafeBadRequest("ID ingrediente non valido");

                var existingIngrediente = await _ingredienteRepository.GetByIdAsync(id);
                if (existingIngrediente == null)
                    return SafeNotFound("Ingrediente");

                // ✅ SALVA lo stato PRIMA del toggle
                var statoPrima = existingIngrediente.Disponibile;

                // ✅ Esegui il toggle (inverte Disponibile)
                await _ingredienteRepository.ToggleDisponibilitaAsync(id);

                // ✅ Ricarica l'ingrediente per ottenere il nuovo stato
                var ingredienteDopoToggle = await _ingredienteRepository.GetByIdAsync(id);

                if (ingredienteDopoToggle == null)
                    return SafeNotFound("Ingrediente dopo toggle");

                // ✅ LOGICA MESSAGGI CORRETTA:
                var messaggio = ingredienteDopoToggle.Disponibile ? "attivato" : "disattivato";

                // ✅ Audit trail
                LogAuditTrail("TOGGLE_DISPONIBILITA_INGREDIENTE", "Ingrediente", id.ToString());

                // ✅ Security event completo con timestamp
                LogSecurityEvent("IngredienteAvailabilityToggled", new
                {
                    IngredienteId = id,
                    DaDisponibile = statoPrima,
                    ADisponibile = ingredienteDopoToggle.Disponibile,
                    User = User.Identity?.Name,
                    Timestamp = DateTime.UtcNow
                });

                return Ok(new
                {
                    message = $"Ingrediente {messaggio}",
                    disponibile = ingredienteDopoToggle.Disponibile
                });
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "Errore database durante il toggle disponibilità ingrediente {Id}", id);
                return SafeInternalError("Errore durante la modifica della disponibilità");
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore durante il toggle disponibilità ingrediente {Id}", id);
                return SafeInternalError("Errore durante la modifica della disponibilità");
            }
        }

        // PUT: api/Ingrediente/5/disponibilita
        // ✅ SOLO ADMIN: Imposta disponibilità specifica
        [HttpPut("{id}/disponibilita")]
        [Authorize(Roles = "admin")] // ✅ Solo admin può impostare disponibilità
        public async Task<IActionResult> SetDisponibilita(int id, [FromBody] bool disponibile)
        {
            try
            {
                if (id <= 0)
                    return SafeBadRequest("ID ingrediente non valido");

                var existingIngrediente = await _ingredienteRepository.GetByIdAsync(id);
                if (existingIngrediente == null)
                    return SafeNotFound("Ingrediente");

                await _ingredienteRepository.SetDisponibilitaAsync(id, disponibile);

                // ✅ Audit trail
                LogAuditTrail("SET_DISPONIBILITA_INGREDIENTE", "Ingrediente", id.ToString());

                // ✅ Security event completo con timestamp
                LogSecurityEvent("IngredienteAvailabilitySet", new
                {
                    IngredienteId = id,
                    OldDisponibile = existingIngrediente.Disponibile,
                    NewDisponibile = disponibile,
                    User = User.Identity?.Name,
                    Timestamp = DateTime.UtcNow
                });

                return Ok(new
                {
                    message = $"Ingrediente {(disponibile ? "attivato" : "disattivato")}",
                    disponibile = disponibile
                });
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "Errore database durante l'impostazione disponibilità ingrediente {Id}", id);
                return SafeInternalError("Errore durante l'impostazione della disponibilità");
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore durante l'impostazione disponibilità ingrediente {Id}", id);
                return SafeInternalError("Errore durante l'impostazione della disponibilità");
            }
        }

        // GET: api/Ingrediente/exists/5
        // ✅ PER TUTTI: Verifica esistenza (indipendentemente dalla disponibilità)
        [HttpGet("exists/{id}")]
        [AllowAnonymous] // ✅ ESPLICITO PER ENDPOINT GET
        public async Task<ActionResult<bool>> Exists(int id)
        {
            try
            {
                if (id <= 0)
                    return SafeBadRequest<bool>("ID ingrediente non valido");

                var exists = await _ingredienteRepository.ExistsAsync(id);
                return Ok(exists);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore durante la verifica esistenza ingrediente {Id}", id);
                return SafeInternalError<bool>("Errore durante la verifica esistenza ingrediente");
            }
        }
    }
}