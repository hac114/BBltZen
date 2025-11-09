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
    public class DolceController : SecureBaseController
    {
        private readonly IDolceRepository _repository;

        public DolceController(
            IDolceRepository repository,
            IWebHostEnvironment environment,
            ILogger<DolceController> logger)
            : base(environment, logger)
        {
            _repository = repository;
        }

        /// <summary>
        /// Ottiene tutti i dolci
        /// </summary>
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<DolceDTO>>> GetAll()
        {
            try
            {
                var result = await _repository.GetAllAsync();
                return Ok(result);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore durante il recupero di tutti i dolci");
                return SafeInternalError<IEnumerable<DolceDTO>>("Errore durante il recupero dei dolci");
            }
        }

        /// <summary>
        /// Ottiene un dolce specifico tramite ID articolo
        /// </summary>
        [HttpGet("{articoloId}")]
        [AllowAnonymous]
        public async Task<ActionResult<DolceDTO>> GetById(int articoloId)
        {
            try
            {
                if (articoloId <= 0)
                    return SafeBadRequest<DolceDTO>("ID articolo non valido");

                var result = await _repository.GetByIdAsync(articoloId);

                if (result == null)
                    return SafeNotFound<DolceDTO>("Dolce");

                return Ok(result);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore durante il recupero del dolce {ArticoloId}", articoloId);
                return SafeInternalError<DolceDTO>("Errore durante il recupero del dolce");
            }
        }

        /// <summary>
        /// Ottiene solo i dolci disponibili
        /// </summary>
        [HttpGet("disponibili")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<DolceDTO>>> GetDisponibili()
        {
            try
            {
                var result = await _repository.GetDisponibiliAsync();
                return Ok(result);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore durante il recupero dei dolci disponibili");
                return SafeInternalError<IEnumerable<DolceDTO>>("Errore durante il recupero dei dolci disponibili");
            }
        }

        /// <summary>
        /// Ottiene i dolci per priorità
        /// </summary>
        [HttpGet("priorita/{priorita}")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<DolceDTO>>> GetByPriorita(int priorita)
        {
            try
            {
                if (priorita <= 0)
                    return SafeBadRequest<IEnumerable<DolceDTO>>("Priorità non valida");

                var result = await _repository.GetByPrioritaAsync(priorita);
                return Ok(result);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore durante il recupero dei dolci per priorità {Priorita}", priorita);
                return SafeInternalError<IEnumerable<DolceDTO>>("Errore durante il recupero dei dolci per priorità");
            }
        }

        /// <summary>
        /// Crea un nuovo dolce
        /// </summary>
        [HttpPost]
        //[Authorize(Roles = "admin,barista")] // ✅ COMMENTATO PER TEST
        [AllowAnonymous] // ✅ TEMPORANEAMENTE PERMESSO A TUTTI PER TEST
        public async Task<ActionResult<DolceDTO>> Create([FromBody] DolceDTO dolceDto)
        {
            try
            {
                if (!IsModelValid(dolceDto))
                    return SafeBadRequest<DolceDTO>("Dati dolce non validi");

                // ✅ CORREZIONE: Il client NON deve specificare ArticoloId
                if (dolceDto.ArticoloId > 0)
                    return SafeBadRequest<DolceDTO>("Non specificare ArticoloId - verrà generato automaticamente");

                // ✅ CORREZIONE: Verifica duplicati nome in modo efficiente
                var existingWithSameName = await _repository.GetAllAsync();
                if (existingWithSameName.Any(d => d.Nome?.ToLower() == dolceDto.Nome?.ToLower()))
                    return Conflict($"Esiste già un dolce con il nome '{dolceDto.Nome}'");

                await _repository.AddAsync(dolceDto);

                // ✅ Audit trail
                LogAuditTrail("CREATE_DOLCE", "Dolce", dolceDto.ArticoloId.ToString());
                LogSecurityEvent("DolceCreated", new
                {
                    ArticoloId = dolceDto.ArticoloId,
                    Nome = dolceDto.Nome,
                    Prezzo = dolceDto.Prezzo,
                    Disponibile = dolceDto.Disponibile,
                    Priorita = dolceDto.Priorita,
                    User = User.Identity?.Name ?? "Unknown",
                    Timestamp = System.DateTime.UtcNow
                });

                return CreatedAtAction(nameof(GetById),
                    new { articoloId = dolceDto.ArticoloId },
                    dolceDto);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore durante la creazione del dolce");
                return SafeInternalError<DolceDTO>("Errore durante la creazione del dolce");
            }
        }

        /// <summary>
        /// Aggiorna un dolce esistente
        /// </summary>
        [HttpPut("{articoloId}")]
        //[Authorize(Roles = "admin,barista")] // ✅ COMMENTATO PER TEST
        [AllowAnonymous] // ✅ TEMPORANEAMENTE PERMESSO A TUTTI PER TEST
        public async Task<ActionResult> Update(int articoloId, [FromBody] DolceDTO dolceDto)
        {
            try
            {
                if (articoloId <= 0)
                    return SafeBadRequest("ID articolo non valido");

                if (articoloId != dolceDto.ArticoloId)
                    return SafeBadRequest("ID articolo non corrispondente");

                if (!IsModelValid(dolceDto))
                    return SafeBadRequest("Dati dolce non validi");

                var existing = await _repository.GetByIdAsync(articoloId);
                if (existing == null)
                    return SafeNotFound("Dolce");

                // ✅ CORREZIONE: Verifica duplicati nome in modo efficiente
                var allDolci = await _repository.GetAllAsync();
                if (allDolci.Any(d => d.ArticoloId != articoloId && d.Nome?.ToLower() == dolceDto.Nome?.ToLower()))
                    return Conflict($"Esiste già un altro dolce con il nome '{dolceDto.Nome}'");

                await _repository.UpdateAsync(dolceDto);

                // ✅ Audit trail
                LogAuditTrail("UPDATE_DOLCE", "Dolce", dolceDto.ArticoloId.ToString());
                LogSecurityEvent("DolceUpdated", new
                {
                    ArticoloId = dolceDto.ArticoloId,
                    Nome = dolceDto.Nome,
                    Prezzo = dolceDto.Prezzo,
                    Disponibile = dolceDto.Disponibile,
                    Priorita = dolceDto.Priorita,
                    User = User.Identity?.Name ?? "Unknown",
                    Timestamp = System.DateTime.UtcNow
                });

                return NoContent();
            }
            catch (System.ArgumentException ex)
            {
                _logger.LogWarning(ex, "Tentativo di aggiornamento di un dolce non trovato {ArticoloId}", articoloId);
                return SafeNotFound("Dolce");
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore durante l'aggiornamento del dolce {ArticoloId}", articoloId);
                return SafeInternalError("Errore durante l'aggiornamento del dolce");
            }
        }

        /// <summary>
        /// Elimina un dolce
        /// </summary>
        [HttpDelete("{articoloId}")]
        //[Authorize(Roles = "admin")] // ✅ COMMENTATO PER TEST
        [AllowAnonymous] // ✅ TEMPORANEAMENTE PERMESSO A TUTTI PER TEST
        public async Task<ActionResult> Delete(int articoloId)
        {
            try
            {
                if (articoloId <= 0)
                    return SafeBadRequest("ID articolo non valido");

                var existing = await _repository.GetByIdAsync(articoloId);
                if (existing == null)
                    return SafeNotFound("Dolce");

                await _repository.DeleteAsync(articoloId);

                // ✅ Audit trail
                LogAuditTrail("DELETE_DOLCE", "Dolce", articoloId.ToString());
                LogSecurityEvent("DolceDeleted", new
                {
                    ArticoloId = articoloId,
                    Nome = existing.Nome,
                    User = User.Identity?.Name ?? "Unknown",
                    Timestamp = System.DateTime.UtcNow
                });

                return NoContent();
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore durante l'eliminazione del dolce {ArticoloId}", articoloId);
                return SafeInternalError("Errore durante l'eliminazione del dolce");
            }
        }
    }
}