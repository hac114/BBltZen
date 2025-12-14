using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using DTO;
using Repository.Interface;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using BBltZen;

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

                // ✅ CORREZIONE: AddAsync ora ritorna il DTO con ArticoloId generato
                if (bevandaStandardDto.ArticoloId > 0 && await _repository.ExistsAsync(bevandaStandardDto.ArticoloId))
                    return SafeBadRequest<BevandaStandardDTO>($"Esiste già una bevanda standard con ArticoloId {bevandaStandardDto.ArticoloId}");

                if (await _repository.ExistsByCombinazioneAsync(
                    bevandaStandardDto.PersonalizzazioneId, bevandaStandardDto.DimensioneBicchiereId))
                {
                    return SafeBadRequest<BevandaStandardDTO>("Esiste già una bevanda standard con la stessa combinazione di personalizzazione e dimensione bicchiere");
                }

                // ✅ CORREZIONE: AddAsync ora ritorna il DTO aggiornato
                var createdBevanda = await _repository.AddAsync(bevandaStandardDto);

                // ✅ SEMPLIFICATO: Audit trail
                LogAuditTrail("CREATE", "BevandaStandard", createdBevanda.ArticoloId.ToString());
                LogSecurityEvent("BevandaStandardCreated", $"Created BevandaStandard ID: {createdBevanda.ArticoloId}");

                return CreatedAtAction(nameof(GetById), new { articoloId = createdBevanda.ArticoloId }, createdBevanda);
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "Errore database durante la creazione della bevanda standard");
                return SafeInternalError<BevandaStandardDTO>("Errore durante il salvataggio dei dati");
            }
            catch (ArgumentException argEx)
            {
                _logger.LogWarning(argEx, "Argomento non valido durante la creazione della bevanda standard");
                return SafeBadRequest<BevandaStandardDTO>(argEx.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante la creazione della bevanda standard");
                return SafeInternalError<BevandaStandardDTO>(ex.Message);
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

                // ✅ CORREZIONE: Validazione duplicati (escludendo l'ID corrente)
                if (await _repository.ExistsByCombinazioneAsync(
                    bevandaStandardDto.PersonalizzazioneId,
                    bevandaStandardDto.DimensioneBicchiereId))
                {
                    // ✅ VERIFICA se la combinazione appartiene a un'altra bevanda
                    var existingWithSameCombo = await _context.BevandaStandard
                        .FirstOrDefaultAsync(bs => bs.PersonalizzazioneId == bevandaStandardDto.PersonalizzazioneId &&
                                                 bs.DimensioneBicchiereId == bevandaStandardDto.DimensioneBicchiereId &&
                                                 bs.ArticoloId != articoloId);

                    if (existingWithSameCombo != null)
                        return SafeBadRequest("Esiste già una bevanda standard con la stessa combinazione di personalizzazione e dimensione bicchiere");
                }

                await _repository.UpdateAsync(bevandaStandardDto);

                // ✅ SEMPLIFICATO: Audit trail
                LogAuditTrail("UPDATE", "BevandaStandard", bevandaStandardDto.ArticoloId.ToString());
                LogSecurityEvent("BevandaStandardUpdated", $"Updated BevandaStandard ID: {bevandaStandardDto.ArticoloId}");

                return NoContent();
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "Errore database durante l'aggiornamento della bevanda standard {ArticoloId}", articoloId);
                return SafeInternalError("Errore durante l'aggiornamento dei dati");
            }
            catch (ArgumentException argEx)
            {
                _logger.LogWarning(argEx, "Argomento non valido durante l'aggiornamento della bevanda standard {ArticoloId}", articoloId);
                return SafeBadRequest(argEx.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante l'aggiornamento della bevanda standard {ArticoloId}", articoloId);
                return SafeInternalError(ex.Message);
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

                // ✅ SEMPLIFICATO: Audit trail
                LogAuditTrail("DELETE", "BevandaStandard", articoloId.ToString());
                LogSecurityEvent("BevandaStandardDeleted", $"Deleted BevandaStandard ID: {articoloId}");

                return NoContent();
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "Errore database durante l'eliminazione della bevanda standard {ArticoloId}", articoloId);
                return SafeInternalError("Errore durante l'eliminazione dei dati");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante l'eliminazione della bevanda standard {ArticoloId}", articoloId);
                return SafeInternalError(ex.Message);
            }
        }
    }
}