using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using DTO;
using Repository.Interface;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BBltZen.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [AllowAnonymous] // ✅ OVERRIDE DELL'[Authorize] DEL BASE CONTROLLER
    public class BevandaStandardController : SecureBaseController
    {
        private readonly IBevandaStandardRepository _bevandaStandardRepository;

        public BevandaStandardController(
            IBevandaStandardRepository bevandaStandardRepository,
            IWebHostEnvironment environment,
            ILogger<BevandaStandardController> logger)
            : base(environment, logger)
        {
            _bevandaStandardRepository = bevandaStandardRepository;
        }

        // ✅ ENDPOINT PUBBLICO: Card prodotto per consumatori
        /// <summary>
        /// Ottiene tutte le card prodotto per il frontend (solo bevande visibili)
        /// </summary>
        [HttpGet("card-prodotti")]
        public async Task<ActionResult<IEnumerable<BevandaStandardCardDTO>>> GetCardProdotti()
        {
            try
            {
                var cardProdotti = await _bevandaStandardRepository.GetCardProdottiAsync();
                return Ok(cardProdotti);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore durante il recupero delle card prodotto");
                return SafeInternalError("Errore durante il recupero dei prodotti");
            }
        }

        // ✅ ENDPOINT PUBBLICO: Card prodotto singola
        /// <summary>
        /// Ottiene una card prodotto specifica per il frontend
        /// </summary>
        [HttpGet("card-prodotto/{articoloId}")]
        public async Task<ActionResult<BevandaStandardCardDTO>> GetCardProdottoById(int articoloId)
        {
            try
            {
                if (articoloId <= 0)
                    return SafeBadRequest<BevandaStandardCardDTO>("ID articolo non valido");

                var cardProdotto = await _bevandaStandardRepository.GetCardProdottoByIdAsync(articoloId);

                if (cardProdotto == null)
                    return SafeNotFound<BevandaStandardCardDTO>("Prodotto");

                return Ok(cardProdotto);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore durante il recupero della card prodotto con ID {ArticoloId}", articoloId);
                return SafeInternalError("Errore durante il recupero del prodotto");
            }
        }

        // ✅ ENDPOINT PUBBLICO: Bevande disponibili per consumatori
        /// <summary>
        /// Ottiene le bevande standard ATTUALMENTE disponibili (disponibile = true)
        /// </summary>
        [HttpGet("disponibili")]
        public async Task<ActionResult<IEnumerable<BevandaStandardDTO>>> GetDisponibili()
        {
            try
            {
                var bevande = await _bevandaStandardRepository.GetDisponibiliAsync();
                return Ok(bevande);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore durante il recupero delle bevande standard disponibili");
                return SafeInternalError("Errore durante il recupero delle bevande disponibili");
            }
        }

        // ✅ ENDPOINT PUBBLICO: Bevanda specifica
        /// <summary>
        /// Ottiene una bevanda standard specifica tramite ID articolo
        /// </summary>
        [HttpGet("{articoloId}")]
        public async Task<ActionResult<BevandaStandardDTO>> GetById(int articoloId)
        {
            try
            {
                if (articoloId <= 0)
                    return SafeBadRequest<BevandaStandardDTO>("ID articolo non valido");

                var bevanda = await _bevandaStandardRepository.GetByIdAsync(articoloId);

                if (bevanda == null)
                    return SafeNotFound<BevandaStandardDTO>("Bevanda standard");

                return Ok(bevanda);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore durante il recupero della bevanda standard {ArticoloId}", articoloId);
                return SafeInternalError("Errore durante il recupero della bevanda standard");
            }
        }

        // ✅ ENDPOINT PUBBLICO: Filtri per consumatori
        /// <summary>
        /// Ottiene le bevande standard per dimensione bicchiere
        /// </summary>
        [HttpGet("dimensione/{dimensioneBicchiereId}")]
        public async Task<ActionResult<IEnumerable<BevandaStandardDTO>>> GetByDimensioneBicchiere(int dimensioneBicchiereId)
        {
            try
            {
                if (dimensioneBicchiereId <= 0)
                    return SafeBadRequest<IEnumerable<BevandaStandardDTO>>("ID dimensione bicchiere non valido");

                var bevande = await _bevandaStandardRepository.GetByDimensioneBicchiereAsync(dimensioneBicchiereId);
                return Ok(bevande);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore durante il recupero delle bevande standard per dimensione bicchiere {DimensioneBicchiereId}", dimensioneBicchiereId);
                return SafeInternalError("Errore durante il recupero delle bevande per dimensione bicchiere");
            }
        }

        /// <summary>
        /// Ottiene le bevande standard per tipo di personalizzazione
        /// </summary>
        [HttpGet("personalizzazione/{personalizzazioneId}")]
        public async Task<ActionResult<IEnumerable<BevandaStandardDTO>>> GetByPersonalizzazione(int personalizzazioneId)
        {
            try
            {
                if (personalizzazioneId <= 0)
                    return SafeBadRequest<IEnumerable<BevandaStandardDTO>>("ID personalizzazione non valido");

                var bevande = await _bevandaStandardRepository.GetByPersonalizzazioneAsync(personalizzazioneId);
                return Ok(bevande);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore durante il recupero delle bevande standard per personalizzazione {PersonalizzazioneId}", personalizzazioneId);
                return SafeInternalError("Errore durante il recupero delle bevande per personalizzazione");
            }
        }

        // ✅ ENDPOINT ADMIN: Tutte le bevande (anche non visibili)
        /// <summary>
        /// Ottiene tutte le bevande standard (solo admin)
        /// </summary>
        [HttpGet]
        //[Authorize(Roles = "admin")]
        public async Task<ActionResult<IEnumerable<BevandaStandardDTO>>> GetAll()
        {
            try
            {
                var bevande = await _bevandaStandardRepository.GetAllAsync();
                return Ok(bevande);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore durante il recupero di tutte le bevande standard");
                return SafeInternalError("Errore durante il recupero delle bevande standard");
            }
        }

        // ✅ ENDPOINT ADMIN: Creazione bevanda
        /// <summary>
        /// Crea una nuova bevanda standard (solo admin)
        /// </summary>
        [HttpPost]
        //[Authorize(Roles = "admin")]
        public async Task<ActionResult<BevandaStandardDTO>> Create([FromBody] BevandaStandardDTO bevandaStandardDto)
        {
            try
            {
                if (!IsModelValid(bevandaStandardDto))
                    return SafeBadRequest<BevandaStandardDTO>("Dati bevanda standard non validi");

                // Verifica se esiste già una bevanda con lo stesso ArticoloId
                if (bevandaStandardDto.ArticoloId > 0 && await _bevandaStandardRepository.ExistsAsync(bevandaStandardDto.ArticoloId))
                    return SafeBadRequest<BevandaStandardDTO>($"Esiste già una bevanda standard con ArticoloId {bevandaStandardDto.ArticoloId}");

                // Verifica se esiste già la stessa combinazione
                if (await _bevandaStandardRepository.ExistsByCombinazioneAsync(
                    bevandaStandardDto.PersonalizzazioneId, bevandaStandardDto.DimensioneBicchiereId))
                {
                    return SafeBadRequest<BevandaStandardDTO>("Esiste già una bevanda standard con la stessa combinazione di personalizzazione e dimensione bicchiere");
                }

                await _bevandaStandardRepository.AddAsync(bevandaStandardDto);

                // ✅ Audit trail
                LogAuditTrail("CREATE_BEVANDA_STANDARD", "BevandaStandard", bevandaStandardDto.ArticoloId.ToString());
                LogSecurityEvent("BevandaStandardCreated", new
                {
                    ArticoloId = bevandaStandardDto.ArticoloId,
                    PersonalizzazioneId = bevandaStandardDto.PersonalizzazioneId,
                    DimensioneBicchiereId = bevandaStandardDto.DimensioneBicchiereId,
                    User = User.Identity?.Name
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
        /// <summary>
        /// Aggiorna una bevanda standard esistente (solo admin)
        /// </summary>
        [HttpPut("{articoloId}")]
        //[Authorize(Roles = "admin")]
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

                var existingBevanda = await _bevandaStandardRepository.GetByIdAsync(articoloId);
                if (existingBevanda == null)
                    return SafeNotFound("Bevanda standard");

                await _bevandaStandardRepository.UpdateAsync(bevandaStandardDto);

                // ✅ Audit trail
                LogAuditTrail("UPDATE_BEVANDA_STANDARD", "BevandaStandard", bevandaStandardDto.ArticoloId.ToString());
                LogSecurityEvent("BevandaStandardUpdated", new
                {
                    ArticoloId = bevandaStandardDto.ArticoloId,
                    User = User.Identity?.Name
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
        /// <summary>
        /// Elimina una bevanda standard (solo admin)
        /// </summary>
        [HttpDelete("{articoloId}")]
        //[Authorize(Roles = "admin")]
        public async Task<ActionResult> Delete(int articoloId)
        {
            try
            {
                if (articoloId <= 0)
                    return SafeBadRequest("ID articolo non valido");

                var existingBevanda = await _bevandaStandardRepository.GetByIdAsync(articoloId);
                if (existingBevanda == null)
                    return SafeNotFound("Bevanda standard");

                await _bevandaStandardRepository.DeleteAsync(articoloId);

                // ✅ Audit trail
                LogAuditTrail("DELETE_BEVANDA_STANDARD", "BevandaStandard", articoloId.ToString());
                LogSecurityEvent("BevandaStandardDeleted", new
                {
                    ArticoloId = articoloId,
                    User = User.Identity?.Name
                });

                return NoContent();
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore durante l'eliminazione della bevanda standard {ArticoloId}", articoloId);
                return SafeInternalError("Errore durante l'eliminazione della bevanda standard");
            }
        }

        [HttpGet("primo-piano")]
        public async Task<ActionResult<IEnumerable<BevandaStandardDTO>>> GetPrimoPiano()
        {
            try
            {
                var bevande = await _bevandaStandardRepository.GetPrimoPianoAsync();
                return Ok(bevande);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore durante il recupero delle bevande in primo piano");
                return SafeInternalError("Errore durante il recupero delle bevande in primo piano");
            }
        }

        [HttpGet("secondo-piano")]
        public async Task<ActionResult<IEnumerable<BevandaStandardDTO>>> GetSecondoPiano()
        {
            try
            {
                var bevande = await _bevandaStandardRepository.GetSecondoPianoAsync();
                return Ok(bevande);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore durante il recupero delle bevande in secondo piano");
                return SafeInternalError("Errore durante il recupero delle bevande in secondo piano");
            }
        }

        [HttpGet("card-prodotti-primo-piano")]
        public async Task<ActionResult<IEnumerable<BevandaStandardCardDTO>>> GetCardProdottiPrimoPiano()
        {
            try
            {
                var cardProdotti = await _bevandaStandardRepository.GetCardProdottiPrimoPianoAsync();
                return Ok(cardProdotti);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore durante il recupero delle card prodotto in primo piano");
                return SafeInternalError("Errore durante il recupero dei prodotti in primo piano");
            }
        }
    }
}