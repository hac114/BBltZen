using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using DTO;
using Repository.Interface;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Database;

namespace BBltZen.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [AllowAnonymous] // ✅ AGGIUNTO
    public class BevandaStandardController : SecureBaseController
    {
        private readonly IBevandaStandardRepository _repository;
        private readonly BubbleTeaContext _context; // ✅ AGGIUNTO

        public BevandaStandardController(
            IBevandaStandardRepository repository,
            BubbleTeaContext context, // ✅ AGGIUNTO
            IWebHostEnvironment environment,
            ILogger<BevandaStandardController> logger)
            : base(environment, logger)
        {
            _repository = repository;
            _context = context; // ✅ AGGIUNTO
        }

        // ✅ ENDPOINT PUBBLICO: Card prodotto per consumatori
        [HttpGet("card-prodotti")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<BevandaStandardCardDTO>>> GetCardProdotti()
        {
            try
            {
                var result = await _repository.GetCardProdottiAsync();
                return Ok(result);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore durante il recupero delle card prodotto");
                return SafeInternalError("Errore durante il recupero dei prodotti");
            }
        }

        // ✅ ENDPOINT PUBBLICO: Card prodotto singola
        [HttpGet("card-prodotto/{articoloId}")]
        [AllowAnonymous]
        public async Task<ActionResult<BevandaStandardCardDTO>> GetCardProdottoById(int articoloId)
        {
            try
            {
                if (articoloId <= 0)
                    return SafeBadRequest<BevandaStandardCardDTO>("ID articolo non valido");

                var result = await _repository.GetCardProdottoByIdAsync(articoloId);

                if (result == null)
                    return SafeNotFound<BevandaStandardCardDTO>("Prodotto");

                return Ok(result);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore durante il recupero della card prodotto {ArticoloId}", articoloId);
                return SafeInternalError("Errore durante il recupero del prodotto");
            }
        }

        // ✅ ENDPOINT PUBBLICO: Bevande disponibili
        [HttpGet("disponibili")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<BevandaStandardDTO>>> GetDisponibili()
        {
            try
            {
                var result = await _repository.GetDisponibiliAsync();
                return Ok(result);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore durante il recupero delle bevande standard disponibili");
                return SafeInternalError("Errore durante il recupero delle bevande disponibili");
            }
        }

        // ✅ ENDPOINT PUBBLICO: Bevanda specifica
        [HttpGet("{articoloId}")]
        [AllowAnonymous]
        public async Task<ActionResult<BevandaStandardDTO>> GetById(int articoloId)
        {
            try
            {
                if (articoloId <= 0)
                    return SafeBadRequest<BevandaStandardDTO>("ID articolo non valido");

                var result = await _repository.GetByIdAsync(articoloId);

                if (result == null)
                    return SafeNotFound<BevandaStandardDTO>("Bevanda standard");

                return Ok(result);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore durante il recupero della bevanda standard {ArticoloId}", articoloId);
                return SafeInternalError("Errore durante il recupero della bevanda standard");
            }
        }

        // ✅ ENDPOINT PUBBLICO: Filtri per dimensioni
        [HttpGet("dimensione/{dimensioneBicchiereId}")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<BevandaStandardDTO>>> GetByDimensioneBicchiere(int dimensioneBicchiereId)
        {
            try
            {
                if (dimensioneBicchiereId <= 0)
                    return SafeBadRequest<IEnumerable<BevandaStandardDTO>>("ID dimensione bicchiere non valido");

                var result = await _repository.GetByDimensioneBicchiereAsync(dimensioneBicchiereId);
                return Ok(result);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore durante il recupero delle bevande per dimensione {DimensioneBicchiereId}", dimensioneBicchiereId);
                return SafeInternalError("Errore durante il recupero delle bevande per dimensione");
            }
        }

        // ✅ ENDPOINT PUBBLICO: Filtri per personalizzazione
        [HttpGet("personalizzazione/{personalizzazioneId}")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<BevandaStandardDTO>>> GetByPersonalizzazione(int personalizzazioneId)
        {
            try
            {
                if (personalizzazioneId <= 0)
                    return SafeBadRequest<IEnumerable<BevandaStandardDTO>>("ID personalizzazione non valido");

                var result = await _repository.GetByPersonalizzazioneAsync(personalizzazioneId);
                return Ok(result);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore durante il recupero delle bevande per personalizzazione {PersonalizzazioneId}", personalizzazioneId);
                return SafeInternalError("Errore durante il recupero delle bevande per personalizzazione");
            }
        }

        // ✅ ENDPOINT PUBBLICO: Primo piano
        [HttpGet("primo-piano")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<BevandaStandardDTO>>> GetPrimoPiano()
        {
            try
            {
                var result = await _repository.GetPrimoPianoAsync();
                return Ok(result);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore durante il recupero delle bevande in primo piano");
                return SafeInternalError("Errore durante il recupero delle bevande in primo piano");
            }
        }

        // ✅ ENDPOINT PUBBLICO: Secondo piano
        [HttpGet("secondo-piano")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<BevandaStandardDTO>>> GetSecondoPiano()
        {
            try
            {
                var result = await _repository.GetSecondoPianoAsync();
                return Ok(result);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore durante il recupero delle bevande in secondo piano");
                return SafeInternalError("Errore durante il recupero delle bevande in secondo piano");
            }
        }

        // ✅ ENDPOINT PUBBLICO: Card prodotti primo piano
        [HttpGet("card-prodotti-primo-piano")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<BevandaStandardCardDTO>>> GetCardProdottiPrimoPiano()
        {
            try
            {
                var result = await _repository.GetCardProdottiPrimoPianoAsync();
                return Ok(result);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore durante il recupero delle card prodotto in primo piano");
                return SafeInternalError("Errore durante il recupero dei prodotti in primo piano");
            }
        }

        // ✅ ENDPOINT ADMIN: Tutte le bevande
        [HttpGet]
        //[Authorize(Roles = "admin")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<BevandaStandardDTO>>> GetAll()
        {
            try
            {
                var result = await _repository.GetAllAsync();

                LogAuditTrail("GET_ALL_BEVANDE_STANDARD", "BevandaStandard", "All");

                return Ok(result);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore durante il recupero di tutte le bevande standard");
                return SafeInternalError("Errore durante il recupero delle bevande standard");
            }
        }

        // ✅ ENDPOINT ADMIN: Creazione bevanda
        [HttpPost]
        //[Authorize(Roles = "admin")]
        [AllowAnonymous]
        public async Task<ActionResult<BevandaStandardDTO>> Create([FromBody] BevandaStandardDTO bevandaStandardDto)
        {
            try
            {
                if (!IsModelValid(bevandaStandardDto))
                    return SafeBadRequest<BevandaStandardDTO>("Dati bevanda standard non validi");

                if (bevandaStandardDto.ArticoloId > 0 && await _repository.ExistsAsync(bevandaStandardDto.ArticoloId))
                    return SafeBadRequest<BevandaStandardDTO>($"Esiste già una bevanda standard con ArticoloId {bevandaStandardDto.ArticoloId}");

                if (await _repository.ExistsByCombinazioneAsync(
                    bevandaStandardDto.PersonalizzazioneId, bevandaStandardDto.DimensioneBicchiereId))
                {
                    return SafeBadRequest<BevandaStandardDTO>("Esiste già una bevanda standard con la stessa combinazione di personalizzazione e dimensione bicchiere");
                }

                await _repository.AddAsync(bevandaStandardDto);

                LogAuditTrail("CREATE_BEVANDA_STANDARD", "BevandaStandard", bevandaStandardDto.ArticoloId.ToString());
                LogSecurityEvent("BevandaStandardCreated", new
                {
                    ArticoloId = bevandaStandardDto.ArticoloId,
                    PersonalizzazioneId = bevandaStandardDto.PersonalizzazioneId,
                    DimensioneBicchiereId = bevandaStandardDto.DimensioneBicchiereId,
                    User = User.Identity?.Name,
                    Timestamp = DateTime.UtcNow // ✅ AGGIUNTO
                });

                return CreatedAtAction(nameof(GetById), new { articoloId = bevandaStandardDto.ArticoloId }, bevandaStandardDto);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore durante la creazione della bevanda standard");
                return SafeInternalError("Errore durante la creazione della bevanda standard");
            }
        }

        // ✅ ENDPOINT ADMIN: Aggiornamento bevanda
        [HttpPut("{articoloId}")]
        //[Authorize(Roles = "admin")]
        [AllowAnonymous]
        public async Task<ActionResult> Update(int articoloId, [FromBody] BevandaStandardDTO bevandaStandardDto)
        {
            try
            {
                if (articoloId <= 0)
                    return SafeBadRequest("ID articolo non valido");

                if (articoloId != bevandaStandardDto.ArticoloId)
                    return SafeBadRequest("ID della bevanda standard non corrisponde");

                if (!IsModelValid(bevandaStandardDto))
                    return SafeBadRequest("Dati bevanda standard non validi");

                var existing = await _repository.GetByIdAsync(articoloId);
                if (existing == null)
                    return SafeNotFound("Bevanda standard");

                await _repository.UpdateAsync(bevandaStandardDto);

                LogAuditTrail("UPDATE_BEVANDA_STANDARD", "BevandaStandard", bevandaStandardDto.ArticoloId.ToString());
                LogSecurityEvent("BevandaStandardUpdated", new
                {
                    ArticoloId = bevandaStandardDto.ArticoloId,
                    User = User.Identity?.Name,
                    Timestamp = DateTime.UtcNow // ✅ AGGIUNTO
                });

                return NoContent();
            }
            catch (System.ArgumentException ex)
            {
                _logger.LogWarning(ex, "Tentativo di aggiornamento di bevanda standard inesistente {ArticoloId}", articoloId);
                return SafeNotFound("Bevanda standard");
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore durante l'aggiornamento della bevanda standard {ArticoloId}", articoloId);
                return SafeInternalError("Errore durante l'aggiornamento della bevanda standard");
            }
        }

        // ✅ ENDPOINT ADMIN: Eliminazione bevanda
        [HttpDelete("{articoloId}")]
        //[Authorize(Roles = "admin")]
        [AllowAnonymous]
        public async Task<ActionResult> Delete(int articoloId)
        {
            try
            {
                if (articoloId <= 0)
                    return SafeBadRequest("ID articolo non valido");

                var existing = await _repository.GetByIdAsync(articoloId);
                if (existing == null)
                    return SafeNotFound("Bevanda standard");

                // ✅ AGGIUNTO: CONTROLLO DIPENDENZE
                var hasOrderItems = await _context.OrderItem
                    .AnyAsync(oi => oi.ArticoloId == articoloId);

                if (hasOrderItems)
                    return SafeBadRequest("Impossibile eliminare: la bevanda è associata a ordini");

                await _repository.DeleteAsync(articoloId);

                LogAuditTrail("DELETE_BEVANDA_STANDARD", "BevandaStandard", articoloId.ToString());
                LogSecurityEvent("BevandaStandardDeleted", new
                {
                    ArticoloId = articoloId,
                    User = User.Identity?.Name,
                    Timestamp = DateTime.UtcNow // ✅ AGGIUNTO
                });

                return NoContent();
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore durante l'eliminazione della bevanda standard {ArticoloId}", articoloId);
                return SafeInternalError("Errore durante l'eliminazione della bevanda standard");
            }
        }
    }
}