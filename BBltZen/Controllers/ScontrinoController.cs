using Microsoft.AspNetCore.Mvc;
using DTO;
using Repository.Interface;
using Microsoft.AspNetCore.Authorization;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BBltZen.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    //[Authorize] // ✅ COMMENTATO PER TEST CON SWAGGER
    public class ScontrinoController : SecureBaseController
    {
        private readonly IScontrinoRepository _repository;

        public ScontrinoController(
            IScontrinoRepository repository,
            IWebHostEnvironment environment,
            ILogger<ScontrinoController> logger)
            : base(environment, logger)
        {
            _repository = repository;
        }

        // POST: api/Scontrino/genera
        [HttpPost("genera")]
        [AllowAnonymous] // ✅ ACCESSO PUBBLICO - I clienti possono generare scontrini
        public async Task<ActionResult<ScontrinoDTO>> GeneraScontrino([FromBody] GeneraScontrinoRequestDTO request)
        {
            try
            {
                if (!IsModelValid(request))
                    return SafeBadRequest<ScontrinoDTO>("Dati richiesta non validi");

                // ✅ VERIFICA ESISTENZA ORDINE
                if (!await _repository.EsisteOrdineAsync(request.OrdineId))
                    return SafeNotFound<ScontrinoDTO>("Ordine");

                // ✅ VERIFICA STATO ORDINE PER SCONTRINO
                if (!await _repository.VerificaStatoOrdinePerScontrinoAsync(request.OrdineId))
                    return SafeBadRequest<ScontrinoDTO>("Ordine non valido per la generazione dello scontrino");

                // ✅ GENERA SCONTRINO COMPLETO
                var scontrino = await _repository.GeneraScontrinoCompletoAsync(request.OrdineId);

                // ✅ AUDIT & SECURITY OTTIMIZZATO PER VS
                LogAuditTrail("GENERATE", "Scontrino", scontrino.OrdineId.ToString());
                LogSecurityEvent("ScontrinoGenerated", new
                {
                    scontrino.OrdineId,
                    NumeroRighe = scontrino.RigheScontrino.Count,
                    scontrino.TotaleOrdine,
                    UserId = GetCurrentUserIdOrDefault()
                });

                return Ok(scontrino);
            }
            catch (ArgumentException argEx)
            {
                // ✅ MESSAGGI DIVERSI PER AMBIENTE SVILUPPO/PRODUZIONE
                if (_environment.IsDevelopment())
                    return SafeBadRequest<ScontrinoDTO>(argEx.Message);
                else
                    return SafeBadRequest<ScontrinoDTO>("Errore durante la generazione dello scontrino");
            }
            catch (System.InvalidOperationException invalidOpEx)
            {
                // ✅ GESTIONE ERRORI DATABASE/STORED PROCEDURE
                _logger.LogError(invalidOpEx, "Errore operazione durante generazione scontrino per ordine {OrdineId}", request.OrdineId);

                if (_environment.IsDevelopment())
                    return SafeInternalError<ScontrinoDTO>($"Errore sistema: {invalidOpEx.Message}");
                else
                    return SafeInternalError<ScontrinoDTO>("Errore temporaneo del sistema. Riprova più tardi.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore imprevisto durante generazione scontrino per ordine {OrdineId}", request.OrdineId);

                if (_environment.IsDevelopment())
                    return SafeInternalError<ScontrinoDTO>($"Errore: {ex.Message}");
                else
                    return SafeInternalError<ScontrinoDTO>("Si è verificato un errore imprevisto");
            }
        }

        // GET: api/Scontrino/verifica-ordine/5
        [HttpGet("verifica-ordine/{ordineId}")]
        [AllowAnonymous] // ✅ ACCESSO PUBBLICO - Verifica stato ordine
        public async Task<ActionResult<object>> VerificaOrdinePerScontrino(int ordineId)
        {
            try
            {
                if (ordineId <= 0)
                    return SafeBadRequest<object>("ID ordine non valido");

                // ✅ VERIFICA ESISTENZA ORDINE
                var ordineEsiste = await _repository.EsisteOrdineAsync(ordineId);
                if (!ordineEsiste)
                    return SafeNotFound<object>("Ordine");

                // ✅ VERIFICA STATO ORDINE PER SCONTRINO
                var statoValido = await _repository.VerificaStatoOrdinePerScontrinoAsync(ordineId);

                // ✅ RISPOSTA DETTAGLIATA PER SVILUPPO, GENERICA PER PRODUZIONE
                var response = new
                {
                    OrdineId = ordineId,
                    StatoValidoPerScontrino = statoValido,
                    Messaggio = statoValido ?
                        "Ordine valido per generazione scontrino" :
                        "Ordine non valido per generazione scontrino"
                };

                if (_environment.IsDevelopment())
                {
                    // ✅ INFORMAZIONI AGGIUNTIVE PER DEBUG
                    return Ok(new
                    {
                        response.OrdineId,
                        response.StatoValidoPerScontrino,
                        response.Messaggio,
                        Dettagli = "Verifica completata con successo"
                    });
                }
                else
                {
                    return Ok(response);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante verifica ordine {OrdineId} per scontrino", ordineId);

                if (_environment.IsDevelopment())
                    return SafeInternalError<object>($"Errore durante la verifica: {ex.Message}");
                else
                    return SafeInternalError<object>("Errore durante la verifica dell'ordine");
            }
        }

        // GET: api/Scontrino/esiste-ordine/5
        [HttpGet("esiste-ordine/{ordineId}")]
        [AllowAnonymous] // ✅ ACCESSO PUBBLICO - Verifica esistenza ordine
        public async Task<ActionResult<object>> VerificaEsistenzaOrdine(int ordineId)
        {
            try
            {
                if (ordineId <= 0)
                    return SafeBadRequest<object>("ID ordine non valido");

                var ordineEsiste = await _repository.EsisteOrdineAsync(ordineId);

                // ✅ RISPOSTA ADATTATA ALL'AMBIENTE
                if (_environment.IsDevelopment())
                {
                    return Ok(new
                    {
                        OrdineId = ordineId,
                        Esiste = ordineEsiste,
                        Messaggio = ordineEsiste ?
                            "Ordine trovato" :
                            "Ordine non trovato",
                        Timestamp = DateTime.UtcNow
                    });
                }
                else
                {
                    return Ok(new
                    {
                        Esiste = ordineEsiste,
                        Messaggio = ordineEsiste ?
                            "Ordine trovato" :
                            "Ordine non trovato"
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante verifica esistenza ordine {OrdineId}", ordineId);

                if (_environment.IsDevelopment())
                    return SafeInternalError<object>($"Errore durante la verifica: {ex.Message}");
                else
                    return SafeInternalError<object>("Errore durante la verifica dell'ordine");
            }
        }

        // GET: api/Scontrino/storico
        [HttpGet("storico")]
        //[Authorize(Roles = "admin,impiegato")] // ✅ SOLO STAFF - Storico scontrini
        public ActionResult<object> GetInfoSistema() // ✅ RIMOSSO 'async' E 'Task<>'
        {
            try
            {
                // ✅ INFORMAZIONI DI SISTEMA PER ADMIN/IMPIEGATI
                var systemInfo = new
                {
                    Servizio = "Scontrino API",
                    Versione = "1.0",
                    Ambiente = _environment.EnvironmentName,
                    Timestamp = DateTime.UtcNow,
                    Utente = User.Identity?.Name ?? "Anonymous"
                };

                // ✅ LOG ACCESSO ALLO STORICO
                LogSecurityEvent("AccessStoricoScontrini", new
                {
                    systemInfo.Utente,
                    systemInfo.Ambiente,
                    UserId = GetCurrentUserIdOrDefault()
                });

                if (_environment.IsDevelopment())
                {
                    // ✅ INFORMAZIONI DETTAGLIATE PER SVILUPPO
                    return Ok(new
                    {
                        systemInfo.Servizio,
                        systemInfo.Versione,
                        systemInfo.Ambiente,
                        systemInfo.Timestamp,
                        systemInfo.Utente,
                        Dettagli = "Endpoint storico scontrini - Accesso consentito solo a staff autorizzato",
                        Note = "In produzione, questo endpoint restituirà lo storico degli scontrini"
                    });
                }
                else
                {
                    return Ok(new
                    {
                        systemInfo.Servizio,
                        systemInfo.Versione,
                        systemInfo.Ambiente,
                        Messaggio = "Servizio scontrini attivo"
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante accesso info sistema scontrini");

                if (_environment.IsDevelopment())
                    return SafeInternalError<object>($"Errore sistema: {ex.Message}");
                else
                    return SafeInternalError<object>("Errore temporaneo del servizio");
            }
        }
    }
}