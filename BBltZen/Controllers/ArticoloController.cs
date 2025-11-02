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
    public class ArticoloController : SecureBaseController
    {
        private readonly IArticoloRepository _repository;

        public ArticoloController(
            IArticoloRepository repository,
            IWebHostEnvironment environment,
            ILogger<ArticoloController> logger)
            : base(environment, logger)
        {
            _repository = repository;
        }

        // GET: api/Articolo
        [HttpGet]
        [AllowAnonymous] // ✅ PERMESSO A TUTTI PER TEST
        public async Task<ActionResult<IEnumerable<ArticoloDTO>>> GetAll()
        {
            try
            {
                var result = await _repository.GetAllAsync();
                return Ok(result);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore durante il recupero di tutti gli articoli");
                return SafeInternalError("Errore durante il recupero degli articoli");
            }
        }

        // GET: api/Articolo/5
        [HttpGet("{articoloId}")]
        [AllowAnonymous] // ✅ PERMESSO A TUTTI PER TEST
        public async Task<ActionResult<ArticoloDTO>> GetById(int articoloId)
        {
            try
            {
                if (articoloId <= 0)
                    return SafeBadRequest<ArticoloDTO>("ID articolo non valido");

                var result = await _repository.GetByIdAsync(articoloId);

                if (result == null)
                    return SafeNotFound<ArticoloDTO>("Articolo");

                return Ok(result);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore durante il recupero dell'articolo {ArticoloId}", articoloId);
                return SafeInternalError("Errore durante il recupero dell'articolo");
            }
        }

        // GET: api/Articolo/tipo/BS
        [HttpGet("tipo/{tipo}")]
        [AllowAnonymous] // ✅ PERMESSO A TUTTI PER TEST
        public async Task<ActionResult<IEnumerable<ArticoloDTO>>> GetByTipo(string tipo)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(tipo))
                    return SafeBadRequest<IEnumerable<ArticoloDTO>>("Tipo articolo non valido");

                var result = await _repository.GetByTipoAsync(tipo);
                return Ok(result);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore durante il recupero degli articoli per tipo {Tipo}", tipo);
                return SafeInternalError("Errore durante il recupero degli articoli per tipo");
            }
        }

        // GET: api/Articolo/ordinabili
        [HttpGet("ordinabili")]
        [AllowAnonymous] // ✅ PERMESSO A TUTTI PER TEST
        public async Task<ActionResult<IEnumerable<ArticoloDTO>>> GetArticoliOrdinabili()
        {
            try
            {
                var result = await _repository.GetArticoliOrdinabiliAsync();
                return Ok(result);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore durante il recupero degli articoli ordinabili");
                return SafeInternalError("Errore durante il recupero degli articoli ordinabili");
            }
        }

        // POST: api/Articolo
        [HttpPost]
        //[Authorize(Roles = "admin")] // ✅ COMMENTATO PER TEST
        [AllowAnonymous] // ✅ TEMPORANEAMENTE PERMESSO A TUTTI PER TEST
        public async Task<ActionResult<ArticoloDTO>> Create(ArticoloDTO articoloDto)
        {
            try
            {
                if (!IsModelValid(articoloDto))
                    return SafeBadRequest<ArticoloDTO>("Dati articolo non validi");

                // Verifica se esiste già un articolo con lo stesso ID
                if (await _repository.ExistsAsync(articoloDto.ArticoloId))
                    return Conflict($"Esiste già un articolo con ID {articoloDto.ArticoloId}");

                // Verifica se il tipo è valido
                if (!IsTipoValido(articoloDto.Tipo))
                    return SafeBadRequest<ArticoloDTO>($"Tipo articolo non valido: {articoloDto.Tipo}");

                await _repository.AddAsync(articoloDto);

                // ✅ Audit trail
                LogAuditTrail("CREATE_ARTICOLO", "Articolo", articoloDto.ArticoloId.ToString());
                LogSecurityEvent("ArticoloCreated", new
                {
                    ArticoloId = articoloDto.ArticoloId,
                    Tipo = articoloDto.Tipo,
                    User = User.Identity?.Name
                });

                return CreatedAtAction(nameof(GetById),
                    new { articoloId = articoloDto.ArticoloId },
                    articoloDto);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore durante la creazione dell'articolo");
                return SafeInternalError("Errore durante la creazione dell'articolo");
            }
        }

        // PUT: api/Articolo/5
        [HttpPut("{articoloId}")]
        //[Authorize(Roles = "admin")] // ✅ COMMENTATO PER TEST
        [AllowAnonymous] // ✅ TEMPORANEAMENTE PERMESSO A TUTTI PER TEST
        public async Task<ActionResult> Update(int articoloId, ArticoloDTO articoloDto)
        {
            try
            {
                if (articoloId <= 0)
                    return SafeBadRequest("ID articolo non valido");

                if (articoloId != articoloDto.ArticoloId)
                    return SafeBadRequest("ID articolo non corrispondente");

                if (!IsModelValid(articoloDto))
                    return SafeBadRequest("Dati articolo non validi");

                var existing = await _repository.GetByIdAsync(articoloId);
                if (existing == null)
                    return SafeNotFound("Articolo");

                // Verifica se il tipo è valido
                if (!IsTipoValido(articoloDto.Tipo))
                    return SafeBadRequest($"Tipo articolo non valido: {articoloDto.Tipo}");

                await _repository.UpdateAsync(articoloDto);

                // ✅ Audit trail
                LogAuditTrail("UPDATE_ARTICOLO", "Articolo", articoloDto.ArticoloId.ToString());
                LogSecurityEvent("ArticoloUpdated", new
                {
                    ArticoloId = articoloDto.ArticoloId,
                    Tipo = articoloDto.Tipo,
                    User = User.Identity?.Name
                });

                return NoContent();
            }
            catch (System.ArgumentException ex)
            {
                _logger.LogWarning(ex, "Tentativo di aggiornamento di un articolo non trovato {ArticoloId}", articoloId);
                return SafeNotFound("Articolo");
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore durante l'aggiornamento dell'articolo {ArticoloId}", articoloId);
                return SafeInternalError("Errore durante l'aggiornamento dell'articolo");
            }
        }

        // DELETE: api/Articolo/5
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
                    return SafeNotFound("Articolo");

                await _repository.DeleteAsync(articoloId);

                // ✅ Audit trail
                LogAuditTrail("DELETE_ARTICOLO", "Articolo", articoloId.ToString());
                LogSecurityEvent("ArticoloDeleted", new
                {
                    ArticoloId = articoloId,
                    User = User.Identity?.Name
                });

                return NoContent();
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore durante l'eliminazione dell'articolo {ArticoloId}", articoloId);
                return SafeInternalError("Errore durante l'eliminazione dell'articolo");
            }
        }

        // ✅ METODO PRIVATO PER VALIDAZIONE TIPO
        private bool IsTipoValido(string tipo)
        {
            var tipiValidi = new[] { "BS", "BC", "DOLCE" }; // BevandaStandard, BevandaCustom, Dolce
            return !string.IsNullOrWhiteSpace(tipo) && tipiValidi.Contains(tipo.ToUpper());
        }
    }
}