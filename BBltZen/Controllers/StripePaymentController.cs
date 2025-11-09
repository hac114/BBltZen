// BBltZen/Controllers/StripePaymentController.cs
using DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Repository.Interface;
using Microsoft.EntityFrameworkCore;
using Database;

namespace BBltZen.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    //[Authorize] // ✅ COMMENTATO PER TEST CON SWAGGER
    public class StripePaymentController : SecureBaseController
    {
        private readonly IStripeServiceRepository _stripeService;
        private readonly BubbleTeaContext _context;

        public StripePaymentController(
            IStripeServiceRepository stripeService,
            BubbleTeaContext context,
            IWebHostEnvironment environment,
            ILogger<StripePaymentController> logger)
            : base(environment, logger)
        {
            _stripeService = stripeService;
            _context = context;
        }

        /// <summary>
        /// Crea un PaymentIntent per un ordine
        /// </summary>
        [HttpPost("create-payment-intent")]
        [ProducesResponseType(typeof(StripePaymentResponseDTO), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        //[Authorize(Roles = "cliente,admin")] // ✅ COMMENTATO PER TEST
        public async Task<ActionResult<StripePaymentResponseDTO>> CreatePaymentIntent([FromBody] StripePaymentRequestDTO request)
        {
            try
            {
                _logger.LogInformation("Creazione PaymentIntent per ordine {OrdineId}", request.OrdineId);

                // ✅ Validazione modello
                if (!IsModelValid(request))
                    return SafeBadRequest<StripePaymentResponseDTO>("Dati pagamento non validi");

                // ✅ CORREZIONE: Usa OrdineId invece di Id
                var ordineEsistente = await _context.Ordine
                    .AnyAsync(o => o.OrdineId == request.OrdineId);

                if (!ordineEsistente)
                    return SafeNotFound<StripePaymentResponseDTO>("Ordine");

                var result = await _stripeService.CreatePaymentIntentAsync(request);

                // ✅ Audit trail
                LogAuditTrail("CREATE_PAYMENT_INTENT", "StripePayment", result.PaymentIntentId);
                LogSecurityEvent("PaymentIntentCreated", new
                {
                    PaymentIntentId = result.PaymentIntentId,
                    OrdineId = request.OrdineId,
                    Amount = request.Amount,
                    Currency = request.Currency,
                    User = User.Identity?.Name ?? "Anonymous"
                });

                _logger.LogInformation("PaymentIntent creato con successo per ordine {OrdineId}", request.OrdineId);

                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Argomento non valido per ordine {OrdineId}", request.OrdineId);
                return SafeBadRequest<StripePaymentResponseDTO>(ex.Message);
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Errore database durante creazione pagamento per ordine {OrdineId}", request.OrdineId);
                return SafeInternalError<StripePaymentResponseDTO>("Errore di sistema durante la creazione del pagamento");
            }
            catch (ApplicationException ex)
            {
                _logger.LogError(ex, "Errore applicazione durante creazione PaymentIntent per ordine {OrdineId}", request.OrdineId);
                return SafeBadRequest<StripePaymentResponseDTO>(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore interno durante creazione PaymentIntent per ordine {OrdineId}", request.OrdineId);
                return SafeInternalError<StripePaymentResponseDTO>("Errore durante la creazione del pagamento");
            }
        }

        /// <summary>
        /// Conferma un pagamento completato
        /// </summary>
        [HttpPost("confirm-payment")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        //[Authorize(Roles = "admin,sistema")] // ✅ COMMENTATO PER TEST
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
                        User = User.Identity?.Name ?? "Anonymous"
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
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Errore database durante conferma pagamento {PaymentIntentId}", request.PaymentIntentId);
                return SafeInternalError("Errore di sistema durante la conferma del pagamento");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante conferma pagamento {PaymentIntentId}", request.PaymentIntentId);
                return SafeInternalError("Errore durante la conferma del pagamento");
            }
        }

        /// <summary>
        /// Gestisce i webhook da Stripe
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

                // ✅ Controllo signature header
                var signatureHeader = Request.Headers["Stripe-Signature"].FirstOrDefault();
                if (string.IsNullOrEmpty(signatureHeader))
                {
                    _logger.LogWarning("Webhook Stripe chiamato senza signature header");
                    return SafeBadRequest("Missing Stripe-Signature header");
                }

                _logger.LogInformation("Webhook ricevuto da Stripe");

                var result = await _stripeService.HandleWebhookAsync(json, signatureHeader);

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
                _logger.LogError(ex, "Errore durante elaborazione webhook Stripe");
                return SafeBadRequest(
                    _environment.IsDevelopment()
                        ? $"Errore webhook: {ex.Message}"
                        : "Webhook non valido"
                );
            }
        }

        /// <summary>
        /// Effettua un rimborso
        /// </summary>
        [HttpPost("refund")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        //[Authorize(Roles = "admin")] // ✅ COMMENTATO PER TEST
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
                        User = User.Identity?.Name ?? "Anonymous"
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
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Errore database durante rimborso {PaymentIntentId}", request.PaymentIntentId);
                return SafeInternalError("Errore di sistema durante il rimborso");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante rimborso {PaymentIntentId}", request.PaymentIntentId);
                return SafeInternalError("Errore durante il rimborso");
            }
        }

        /// <summary>
        /// Verifica lo stato di un pagamento
        /// </summary>
        [HttpGet("payment-status/{paymentIntentId}")]
        [ProducesResponseType(typeof(PaymentStatusResponseDTO), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        [AllowAnonymous] // ✅ PER TEST CON SWAGGER
        public ActionResult<PaymentStatusResponseDTO> GetPaymentStatus(string paymentIntentId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(paymentIntentId))
                    return SafeBadRequest<PaymentStatusResponseDTO>("ID pagamento non valido");

                _logger.LogInformation("Richiesta stato pagamento per {PaymentIntentId}", paymentIntentId);

                // ✅ CORREZIONE: Metodo sincrono per evitare warning CS1998
                var statusInfo = new PaymentStatusResponseDTO
                {
                    PaymentIntentId = paymentIntentId,
                    Status = "unknown",
                    Message = _environment.IsDevelopment()
                        ? "Implementa la logica di recupero stato dal database"
                        : "Stato pagamento non disponibile",
                    LastUpdated = DateTime.UtcNow
                };

                // ✅ Audit trail per consultazione stato
                LogAuditTrail("CHECK_PAYMENT_STATUS", "StripePayment", paymentIntentId);

                return Ok(statusInfo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante recupero stato per {PaymentIntentId}", paymentIntentId);
                return SafeInternalError<PaymentStatusResponseDTO>("Errore durante il recupero dello stato pagamento");
            }
        }

        /// <summary>
        /// Simula un pagamento per testing (solo in sviluppo)
        /// </summary>
        [HttpPost("simulate-payment")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(403)]
        //[Authorize(Roles = "admin,developer")] // ✅ COMMENTATO PER TEST
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

                // ✅ CORREZIONE: Usa OrdineId invece di Id
                var ordineEsistente = await _context.Ordine
                    .AnyAsync(o => o.OrdineId == request.OrdineId);

                if (!ordineEsistente)
                    return SafeNotFound("Ordine");

                // Crea una richiesta di pagamento simulata
                var paymentRequest = new StripePaymentRequestDTO
                {
                    OrdineId = request.OrdineId,
                    Amount = request.Amount,
                    Currency = request.Currency,
                    Description = request.Description ?? $"Ordine simulato #{request.OrdineId}",
                    CustomerEmail = request.CustomerEmail ?? "test@example.com"
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
                    User = User.Identity?.Name ?? "Anonymous"
                });

                return Ok(new
                {
                    success = true,
                    paymentIntent = result,
                    message = "Pagamento simulato con successo",
                    autoConfirmed = request.AutoConfirm
                });
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Errore database durante simulazione pagamento ordine {OrdineId}", request.OrdineId);
                return SafeInternalError("Errore di sistema durante la simulazione del pagamento");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante simulazione pagamento ordine {OrdineId}", request.OrdineId);
                return SafeInternalError("Errore durante la simulazione del pagamento");
            }
        }
    }
}