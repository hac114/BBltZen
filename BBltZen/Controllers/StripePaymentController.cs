// BBltZen/Controllers/StripePaymentController.cs
using DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Repository.Interface;

namespace BBltZen.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    //[Authorize] // ✅ COMMENTATO PER TEST CON SWAGGER
    public class StripePaymentController : SecureBaseController
    {
        private readonly IStripeServiceRepository _stripeService;

        public StripePaymentController(
            IStripeServiceRepository stripeService,
            IWebHostEnvironment environment,
            ILogger<StripePaymentController> logger)
            : base(environment, logger)
        {
            _stripeService = stripeService;
        }

        /// <summary>
        /// Crea un PaymentIntent per un ordine
        /// </summary>
        /// <param name="request">Dati per il pagamento</param>
        /// <returns>ClientSecret per completare il pagamento</returns>
        [HttpPost("create-payment-intent")]
        [ProducesResponseType(typeof(StripePaymentResponseDTO), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<StripePaymentResponseDTO>> CreatePaymentIntent([FromBody] StripePaymentRequestDTO request)
        {
            try
            {
                _logger.LogInformation("Creazione PaymentIntent per ordine {OrdineId}", request.OrdineId);

                if (!IsModelValid(request))
                    return SafeBadRequest<StripePaymentResponseDTO>("Dati pagamento non validi");

                var result = await _stripeService.CreatePaymentIntentAsync(request);

                // ✅ Audit trail
                LogAuditTrail("CREATE_PAYMENT_INTENT", "StripePayment", result.PaymentIntentId);
                LogSecurityEvent("PaymentIntentCreated", new
                {
                    PaymentIntentId = result.PaymentIntentId,
                    OrdineId = request.OrdineId,
                    Amount = request.Amount,
                    Currency = request.Currency,
                    User = User.Identity?.Name
                });

                _logger.LogInformation("PaymentIntent creato con successo per ordine {OrdineId}", request.OrdineId);

                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Ordine non trovato: {OrdineId}", request.OrdineId);
                return SafeBadRequest<StripePaymentResponseDTO>(ex.Message);
            }
            catch (ApplicationException ex)
            {
                _logger.LogError(ex, "Errore durante la creazione del PaymentIntent per ordine {OrdineId}", request.OrdineId);
                return SafeBadRequest<StripePaymentResponseDTO>(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore interno durante la creazione del PaymentIntent per ordine {OrdineId}", request.OrdineId);
                return SafeInternalError("Errore durante la creazione del pagamento");
            }
        }

        /// <summary>
        /// Conferma un pagamento completato
        /// </summary>
        /// <param name="request">Dati conferma pagamento</param>
        /// <returns>Esito della conferma</returns>
        [HttpPost("confirm-payment")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<ActionResult> ConfirmPayment([FromBody] ConfirmPaymentRequestDTO request)
        {
            try
            {
                _logger.LogInformation("Conferma pagamento per PaymentIntent {PaymentIntentId}", request.PaymentIntentId);

                if (!IsModelValid(request))
                    return SafeBadRequest("Dati conferma pagamento non validi");

                var result = await _stripeService.ConfirmPaymentAsync(request.PaymentIntentId);

                if (result)
                {
                    // ✅ Audit trail
                    LogAuditTrail("CONFIRM_PAYMENT", "StripePayment", request.PaymentIntentId);
                    LogSecurityEvent("PaymentConfirmed", new
                    {
                        PaymentIntentId = request.PaymentIntentId,
                        User = User.Identity?.Name
                    });

                    _logger.LogInformation("Pagamento confermato con successo per {PaymentIntentId}", request.PaymentIntentId);

                    if (_environment.IsDevelopment())
                        return Ok(new { success = true, message = "Pagamento confermato con successo" });
                    else
                        return Ok(new { success = true, message = "Operazione completata" });
                }
                else
                {
                    _logger.LogWarning("Pagamento non confermato per {PaymentIntentId}", request.PaymentIntentId);
                    return SafeBadRequest("Impossibile confermare il pagamento");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante la conferma del pagamento {PaymentIntentId}", request.PaymentIntentId);
                return SafeInternalError("Errore durante la conferma del pagamento");
            }
        }

        /// <summary>
        /// Gestisce i webhook da Stripe (per pagamenti completati, falliti, etc.)
        /// </summary>
        [HttpPost("webhook")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [AllowAnonymous] // ✅ WEBHOOK DEVE ESSERE PUBBLICO PER STRIPE
        public async Task<ActionResult> HandleWebhook()
        {
            try
            {
                var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
                var signature = Request.Headers["Stripe-Signature"];

                _logger.LogInformation("Webhook ricevuto da Stripe");

                var result = await _stripeService.HandleWebhookAsync(json, signature);

                if (result)
                {
                    _logger.LogInformation("Webhook processato con successo");
                    return Ok();
                }
                else
                {
                    _logger.LogWarning("Webhook non processato correttamente");
                    return SafeBadRequest("Webhook non processato");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante l'elaborazione del webhook Stripe");
                return SafeBadRequest("Webhook non valido");
            }
        }

        /// <summary>
        /// Effettua un rimborso
        /// </summary>
        [HttpPost("refund")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        //[Authorize(Roles = "admin")] // ✅ COMMENTATO PER TEST CON SWAGGER
        public async Task<ActionResult> RefundPayment([FromBody] RefundPaymentRequestDTO request)
        {
            try
            {
                _logger.LogInformation("Richiesta rimborso per PaymentIntent {PaymentIntentId}", request.PaymentIntentId);

                if (!IsModelValid(request))
                    return SafeBadRequest("Dati rimborso non validi");

                var result = await _stripeService.RefundPaymentAsync(request.PaymentIntentId);

                if (result)
                {
                    // ✅ Audit trail
                    LogAuditTrail("REFUND_PAYMENT", "StripePayment", request.PaymentIntentId);
                    LogSecurityEvent("PaymentRefunded", new
                    {
                        PaymentIntentId = request.PaymentIntentId,
                        Reason = request.Reason,
                        User = User.Identity?.Name
                    });

                    _logger.LogInformation("Rimborso effettuato con successo per {PaymentIntentId}", request.PaymentIntentId);

                    if (_environment.IsDevelopment())
                        return Ok(new { success = true, message = "Rimborso effettuato con successo" });
                    else
                        return Ok(new { success = true, message = "Operazione completata" });
                }
                else
                {
                    _logger.LogWarning("Rimborso fallito per {PaymentIntentId}", request.PaymentIntentId);
                    return SafeBadRequest("Impossibile processare il rimborso");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante il rimborso per {PaymentIntentId}", request.PaymentIntentId);
                return SafeInternalError("Errore durante il rimborso");
            }
        }

        /// <summary>
        /// Verifica lo stato di un pagamento
        /// </summary>
        [HttpGet("payment-status/{paymentIntentId}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<ActionResult> GetPaymentStatus(string paymentIntentId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(paymentIntentId))
                    return SafeBadRequest("ID pagamento non valido");

                _logger.LogInformation("Richiesta stato pagamento per {PaymentIntentId}", paymentIntentId);

                // ✅ Questo è un endpoint di esempio - implementa la logica specifica
                // Per ora restituiamo un placeholder che differenzia i messaggi
                var statusInfo = new
                {
                    paymentIntentId = paymentIntentId,
                    status = "unknown",
                    message = _environment.IsDevelopment()
                        ? "Implementa la logica di recupero stato dal database"
                        : "Stato pagamento non disponibile"
                };

                // ✅ Audit trail per consultazione stato
                LogAuditTrail("CHECK_PAYMENT_STATUS", "StripePayment", paymentIntentId);

                return Ok(statusInfo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante il recupero dello stato per {PaymentIntentId}", paymentIntentId);
                return SafeInternalError("Errore durante il recupero dello stato pagamento");
            }
        }

        /// <summary>
        /// Simula un pagamento per testing (solo in sviluppo)
        /// </summary>
        [HttpPost("simulate-payment")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(403)]
        //[Authorize(Roles = "admin,developer")] // ✅ COMMENTATO PER TEST CON SWAGGER
        public async Task<ActionResult> SimulatePayment([FromBody] SimulatePaymentRequestDTO request)
        {
            try
            {
                // ✅ Solo in ambiente di sviluppo
                if (!_environment.IsDevelopment())
                {
                    _logger.LogWarning("Tentativo di accesso a simulate-payment in ambiente non di sviluppo");
                    return StatusCode(403, new { error = "Funzione disponibile solo in ambiente di sviluppo" });
                }

                if (!IsModelValid(request))
                    return SafeBadRequest("Dati simulazione non validi");

                _logger.LogInformation("Simulazione pagamento per ordine {OrdineId}", request.OrdineId);

                // Crea una richiesta di pagamento simulata
                var paymentRequest = new StripePaymentRequestDTO
                {
                    OrdineId = request.OrdineId,
                    Amount = request.Amount,
                    Currency = request.Currency,
                    Description = request.Description,
                    CustomerEmail = request.CustomerEmail
                };

                var result = await _stripeService.CreatePaymentIntentAsync(paymentRequest);

                // ✅ Simula conferma pagamento
                if (request.AutoConfirm)
                {
                    await _stripeService.ConfirmPaymentAsync(result.PaymentIntentId);
                }

                // ✅ Audit trail per simulazione
                LogAuditTrail("SIMULATE_PAYMENT", "StripePayment", result.PaymentIntentId);
                LogSecurityEvent("PaymentSimulated", new
                {
                    PaymentIntentId = result.PaymentIntentId,
                    OrdineId = request.OrdineId,
                    Amount = request.Amount,
                    AutoConfirm = request.AutoConfirm,
                    User = User.Identity?.Name
                });

                return Ok(new
                {
                    success = true,
                    paymentIntent = result,
                    message = "Pagamento simulato con successo",
                    autoConfirmed = request.AutoConfirm
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante la simulazione del pagamento per ordine {OrdineId}", request.OrdineId);
                return SafeInternalError("Errore durante la simulazione del pagamento");
            }
        }
    }
}