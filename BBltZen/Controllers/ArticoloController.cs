using Microsoft.AspNetCore.Mvc;
using DTO;
using Repository.Interface;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Database;
using System.Linq;
using System;

namespace BBltZen.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [AllowAnonymous] // ✅ Commentato per testing Swagger
    public class ArticoloController : SecureBaseController
    {
        private readonly IArticoloRepository _repository;
        private readonly BubbleTeaContext _context;

        public ArticoloController(
            IArticoloRepository repository,
            BubbleTeaContext context,
            IWebHostEnvironment environment,
            ILogger<ArticoloController> logger)
            : base(environment, logger)
        {
            _repository = repository;
            _context = context;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<ArticoloDTO>>> GetAll()
        {
            try
            {
                LogSecurityEvent("GetAllArticoli", new { Timestamp = DateTime.UtcNow });

                var result = await _repository.GetAllAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante il recupero di tutti gli articoli");
                return SafeInternalError<IEnumerable<ArticoloDTO>>("Errore durante il recupero degli articoli");
            }
        }

        [HttpGet("{articoloId:int}")]
        [AllowAnonymous]
        public async Task<ActionResult<ArticoloDTO>> GetById(int articoloId)
        {
            try
            {
                LogSecurityEvent("GetArticoloById", new { ArticoloId = articoloId, Timestamp = DateTime.UtcNow });

                if (articoloId <= 0)
                    return SafeBadRequest<ArticoloDTO>("ID articolo non valido");

                var result = await _repository.GetByIdAsync(articoloId);
                if (result == null)
                    return SafeNotFound<ArticoloDTO>("Articolo");

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante il recupero dell'articolo {ArticoloId}", articoloId);
                return SafeInternalError<ArticoloDTO>("Errore durante il recupero dell'articolo");
            }
        }

        [HttpGet("tipo/{tipo}")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<ArticoloDTO>>> GetByTipo(string tipo)
        {
            try
            {
                LogSecurityEvent("GetArticoliByTipo", new { Tipo = tipo, Timestamp = DateTime.UtcNow });

                if (string.IsNullOrWhiteSpace(tipo))
                    return SafeBadRequest<IEnumerable<ArticoloDTO>>("Tipo articolo non valido");

                if (!IsTipoValido(tipo))
                    return SafeBadRequest<IEnumerable<ArticoloDTO>>($"Tipo articolo non valido: {tipo}");

                var result = await _repository.GetByTipoAsync(tipo);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante il recupero degli articoli per tipo {Tipo}", tipo);
                return SafeInternalError<IEnumerable<ArticoloDTO>>("Errore durante il recupero degli articoli per tipo");
            }
        }

        [HttpGet("ordinabili")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<ArticoloDTO>>> GetArticoliOrdinabili()
        {
            try
            {
                LogSecurityEvent("GetArticoliOrdinabili", new { Timestamp = DateTime.UtcNow });

                var result = await _repository.GetArticoliOrdinabiliAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante il recupero degli articoli ordinabili");
                return SafeInternalError<IEnumerable<ArticoloDTO>>("Errore durante il recupero degli articoli ordinabili");
            }
        }

        [HttpGet("dolci-disponibili")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<ArticoloDTO>>> GetDolciDisponibili()
        {
            try
            {
                LogSecurityEvent("GetDolciDisponibili", new { Timestamp = DateTime.UtcNow });

                var result = await _repository.GetDolciDisponibiliAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante il recupero dei dolci disponibili");
                return SafeInternalError<IEnumerable<ArticoloDTO>>("Errore durante il recupero dei dolci disponibili");
            }
        }

        [HttpGet("bevande-standard-disponibili")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<ArticoloDTO>>> GetBevandeStandardDisponibili()
        {
            try
            {
                LogSecurityEvent("GetBevandeStandardDisponibili", new { Timestamp = DateTime.UtcNow });

                var result = await _repository.GetBevandeStandardDisponibiliAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante il recupero delle bevande standard disponibili");
                return SafeInternalError<IEnumerable<ArticoloDTO>>("Errore durante il recupero delle bevande standard disponibili");
            }
        }

        [HttpGet("bevande-custom-base")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<ArticoloDTO>>> GetBevandeCustomBase()
        {
            try
            {
                LogSecurityEvent("GetBevandeCustomBase", new { Timestamp = DateTime.UtcNow });

                var result = await _repository.GetBevandeCustomBaseAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante il recupero delle bevande custom base");
                return SafeInternalError<IEnumerable<ArticoloDTO>>("Errore durante il recupero delle bevande custom base");
            }
        }

        [HttpGet("ingredienti-disponibili")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<IngredienteDTO>>> GetIngredientiDisponibili()
        {
            try
            {
                LogSecurityEvent("GetIngredientiDisponibili", new { Timestamp = DateTime.UtcNow });

                var result = await _repository.GetIngredientiDisponibiliPerBevandaCustomAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante il recupero degli ingredienti disponibili");
                return SafeInternalError<IEnumerable<IngredienteDTO>>("Errore durante il recupero degli ingredienti disponibili");
            }
        }

        [HttpGet("completo")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<ArticoloDTO>>> GetAllCompleto()
        {
            try
            {
                LogSecurityEvent("GetAllArticoliCompleto", new { Timestamp = DateTime.UtcNow });

                var result = await _repository.GetAllArticoliCompletoAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante il recupero di tutti gli articoli completi");
                return SafeInternalError<IEnumerable<ArticoloDTO>>("Errore durante il recupero degli articoli completi");
            }
        }

        [HttpPost]
        //[Authorize(Roles = "Admin,Manager")] // ✅ Commentato per testing
        [AllowAnonymous] // ✅ Temporaneo per testing
        public async Task<ActionResult<ArticoloDTO>> Create([FromBody] ArticoloDTO articoloDto)
        {
            try
            {
                LogSecurityEvent("CreateArticoloAttempt", new
                {
                    Tipo = articoloDto?.Tipo,
                    User = User.Identity?.Name ?? "Anonymous",
                    Timestamp = DateTime.UtcNow
                });

                // ✅ CORREZIONE: Controllo esplicito per null prima di IsModelValid
                if (articoloDto == null)
                    return SafeBadRequest<ArticoloDTO>("Dati articolo non validi");

                if (!IsModelValid(articoloDto))
                    return SafeBadRequest<ArticoloDTO>("Dati articolo non validi");

                // Controllo duplicati per ID
                if (articoloDto.ArticoloId > 0 && await _repository.ExistsAsync(articoloDto.ArticoloId))
                    return SafeBadRequest<ArticoloDTO>($"Esiste già un articolo con ID {articoloDto.ArticoloId}");

                if (!IsTipoValido(articoloDto.Tipo))
                    return SafeBadRequest<ArticoloDTO>($"Tipo articolo non valido: {articoloDto.Tipo}");

                await _repository.AddAsync(articoloDto);

                LogAuditTrail("CREATE", "Articolo", articoloDto.ArticoloId.ToString());
                LogSecurityEvent("ArticoloCreated", new
                {
                    ArticoloId = articoloDto.ArticoloId,
                    Tipo = articoloDto.Tipo,
                    User = User.Identity?.Name ?? "Anonymous",
                    Timestamp = DateTime.UtcNow
                });

                return CreatedAtAction(nameof(GetById), new { articoloId = articoloDto.ArticoloId }, articoloDto);
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "Errore database durante la creazione dell'articolo");
                return SafeInternalError<ArticoloDTO>("Errore di sistema durante la creazione dell'articolo");
            }
            catch (ArgumentException argEx)
            {
                _logger.LogWarning(argEx, "Argomento non valido durante la creazione dell'articolo");
                return SafeBadRequest<ArticoloDTO>(argEx.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante la creazione dell'articolo");
                return SafeInternalError<ArticoloDTO>("Errore durante la creazione dell'articolo");
            }
        }

        [HttpPut("{articoloId:int}")]
        //[Authorize(Roles = "Admin,Manager")] // ✅ Commentato per testing
        [AllowAnonymous] // ✅ Temporaneo per testing
        public async Task<ActionResult> Update(int articoloId, [FromBody] ArticoloDTO articoloDto)
        {
            try
            {
                LogSecurityEvent("UpdateArticoloAttempt", new
                {
                    ArticoloId = articoloId,
                    User = User.Identity?.Name ?? "Anonymous",
                    Timestamp = DateTime.UtcNow
                });

                if (articoloId <= 0)
                    return SafeBadRequest("ID articolo non valido");

                if (articoloDto == null)
                    return SafeBadRequest("Dati articolo non validi");

                if (articoloId != articoloDto.ArticoloId)
                    return SafeBadRequest("ID articolo non corrispondente");

                if (!IsModelValid(articoloDto))
                    return SafeBadRequest("Dati articolo non validi");

                var existing = await _repository.GetByIdAsync(articoloId);
                if (existing == null)
                    return SafeNotFound("Articolo");

                if (!IsTipoValido(articoloDto.Tipo))
                    return SafeBadRequest($"Tipo articolo non valido: {articoloDto.Tipo}");

                await _repository.UpdateAsync(articoloDto);

                LogAuditTrail("UPDATE", "Articolo", articoloDto.ArticoloId.ToString());
                LogSecurityEvent("ArticoloUpdated", new
                {
                    ArticoloId = articoloDto.ArticoloId,
                    Tipo = articoloDto.Tipo,
                    User = User.Identity?.Name ?? "Anonymous",
                    Timestamp = DateTime.UtcNow
                });

                return NoContent();
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "Errore database durante l'aggiornamento dell'articolo {ArticoloId}", articoloId);
                return SafeInternalError("Errore di sistema durante l'aggiornamento dell'articolo");
            }
            catch (ArgumentException argEx)
            {
                _logger.LogWarning(argEx, "Argomento non valido durante l'aggiornamento dell'articolo {ArticoloId}", articoloId);
                return SafeNotFound("Articolo");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante l'aggiornamento dell'articolo {ArticoloId}", articoloId);
                return SafeInternalError("Errore durante l'aggiornamento dell'articolo");
            }
        }

        /*[HttpDelete("{articoloId:int}")]
        //[Authorize(Roles = "Admin")] // ✅ Commentato per testing
        [AllowAnonymous] // ✅ Temporaneo per testing
        public async Task<ActionResult> Delete(int articoloId)
        {
            try
            {
                LogSecurityEvent("DeleteArticoloAttempt", new
                {
                    ArticoloId = articoloId,
                    User = User.Identity?.Name ?? "Anonymous",
                    Timestamp = DateTime.UtcNow
                });

                if (articoloId <= 0)
                    return SafeBadRequest("ID articolo non valido");

                var existing = await _repository.GetByIdAsync(articoloId);
                if (existing == null)
                    return SafeNotFound("Articolo");

                // ✅ CONTROLLO DIPENDENZE AVANZATO
                var hasOrderItems = await _context.OrderItem
                    .AnyAsync(oi => oi.ArticoloId == articoloId);

                if (hasOrderItems)
                    return SafeBadRequest("Impossibile eliminare: l'articolo è associato a ordini");

                // ✅ Controllo aggiuntivo per BevandaStandard
                //var hasBevandaStandard = await _context.BevandaStandard
                //    .AnyAsync(bs => bs.ArticoloId == articoloId);

                //if (hasBevandaStandard)
                //    return SafeBadRequest("Impossibile eliminare: l'articolo ha una bevanda standard associata");

                await _repository.DeleteAsync(articoloId);

                LogAuditTrail("DELETE", "Articolo", articoloId.ToString());
                LogSecurityEvent("ArticoloDeleted", new
                {
                    ArticoloId = articoloId,
                    User = User.Identity?.Name ?? "Anonymous",
                    Timestamp = DateTime.UtcNow
                });

                return NoContent();
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "Errore database durante l'eliminazione dell'articolo {ArticoloId}", articoloId);
                return SafeInternalError("Errore di sistema durante l'eliminazione dell'articolo");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante l'eliminazione dell'articolo {ArticoloId}", articoloId);
                return SafeInternalError("Errore durante l'eliminazione dell'articolo");
            }
        }*/

        [HttpGet("exists/{articoloId:int}")]
        [AllowAnonymous]
        public async Task<ActionResult<bool>> Exists(int articoloId)
        {
            try
            {
                if (articoloId <= 0)
                    return SafeBadRequest<bool>("ID articolo non valido");

                var exists = await _repository.ExistsAsync(articoloId);
                return Ok(exists);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante il controllo esistenza articolo {ArticoloId}", articoloId);
                return SafeInternalError<bool>("Errore durante il controllo esistenza articolo");
            }
        }

        [HttpGet("exists-tipo/{tipo}")]
        [AllowAnonymous]
        public async Task<ActionResult<bool>> ExistsByTipo(string tipo, [FromQuery] int? excludeArticoloId = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(tipo))
                    return SafeBadRequest<bool>("Tipo articolo non valido");

                if (!IsTipoValido(tipo))
                    return SafeBadRequest<bool>($"Tipo articolo non valido: {tipo}");

                var exists = await _repository.ExistsByTipoAsync(tipo, excludeArticoloId);
                return Ok(exists);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante il controllo esistenza articolo per tipo {Tipo}", tipo);
                return SafeInternalError<bool>("Errore durante il controllo esistenza articolo per tipo");
            }
        }

        private bool IsTipoValido(string tipo)
        {
            var tipiValidi = new[] { "BS", "BC", "D" };
            return !string.IsNullOrWhiteSpace(tipo) && tipiValidi.Contains(tipo.ToUpper());
        }
    }
}