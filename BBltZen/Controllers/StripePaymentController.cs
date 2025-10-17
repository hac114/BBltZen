using Microsoft.AspNetCore.Mvc;
using DTO;
using Repository.Interface;
using Microsoft.AspNetCore.Authorization;

namespace BBltZen.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StripePaymentController : ControllerBase
    {
        private readonly IStripeServiceRepository _stripeService;
        private readonly ILogger<StripePaymentController> _logger;

        public StripePaymentController(
            IStripeServiceRepository stripeService,
            ILogger<StripePaymentController> logger)
        {
            _stripeService = stripeService;
            _logger = logger;
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
        public async Task<IActionResult> CreatePaymentIntent([FromBody] StripePaymentRequestDTO request)
        {
            try
            {
                _logger.LogInformation("Creazione PaymentIntent per ordine {OrdineId}", request.OrdineId);

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var result = await _stripeService.CreatePaymentIntentAsync(request);

                _logger.LogInformation("PaymentIntent creato con successo per ordine {OrdineId}", request.OrdineId);

                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Ordine non trovato: {OrdineId}", request.OrdineId);
                return BadRequest(new { error = ex.Message });
            }
            catch (ApplicationException ex)
            {
                _logger.LogError(ex, "Errore durante la creazione del PaymentIntent per ordine {OrdineId}", request.OrdineId);
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore interno durante la creazione del PaymentIntent per ordine {OrdineId}", request.OrdineId);
                return StatusCode(500, new { error = "Errore interno del server" });
            }
        }

        /// <summary>
        /// Conferma un pagamento completato
        /// </summary>
        /// <param name="paymentIntentId">ID del PaymentIntent di Stripe</param>
        /// <returns>Esito della conferma</returns>
        [HttpPost("confirm-payment")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> ConfirmPayment([FromBody] ConfirmPaymentRequestDTO request)
        {
            try
            {
                _logger.LogInformation("Conferma pagamento per PaymentIntent {PaymentIntentId}", request.PaymentIntentId);

                var result = await _stripeService.ConfirmPaymentAsync(request.PaymentIntentId);

                if (result)
                {
                    _logger.LogInformation("Pagamento confermato con successo per {PaymentIntentId}", request.PaymentIntentId);
                    return Ok(new { success = true, message = "Pagamento confermato con successo" });
                }
                else
                {
                    _logger.LogWarning("Pagamento non confermato per {PaymentIntentId}", request.PaymentIntentId);
                    return BadRequest(new { error = "Impossibile confermare il pagamento" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante la conferma del pagamento {PaymentIntentId}", request.PaymentIntentId);
                return StatusCode(500, new { error = "Errore interno del server" });
            }
        }

        /// <summary>
        /// Gestisce i webhook da Stripe (per pagamenti completati, falliti, etc.)
        /// </summary>
        [HttpPost("webhook")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> HandleWebhook()
        {
            try
            {
                var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
                var signature = Request.Headers["Stripe-Signature"];

                _logger.LogInformation("Webhook ricevuto da Stripe");

                var result = await _stripeService.HandleWebhookAsync(json, signature);

                if (result)
                {
                    return Ok();
                }
                else
                {
                    return BadRequest(new { error = "Webhook non processato" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante l'elaborazione del webhook Stripe");
                return BadRequest(new { error = "Webhook non valido" });
            }
        }

        /// <summary>
        /// Effettua un rimborso
        /// </summary>
        [HttpPost("refund")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> RefundPayment([FromBody] RefundPaymentRequestDTO request)
        {
            try
            {
                _logger.LogInformation("Richiesta rimborso per PaymentIntent {PaymentIntentId}", request.PaymentIntentId);

                var result = await _stripeService.RefundPaymentAsync(request.PaymentIntentId);

                if (result)
                {
                    _logger.LogInformation("Rimborso effettuato con successo per {PaymentIntentId}", request.PaymentIntentId);
                    return Ok(new { success = true, message = "Rimborso effettuato con successo" });
                }
                else
                {
                    _logger.LogWarning("Rimborso fallito per {PaymentIntentId}", request.PaymentIntentId);
                    return BadRequest(new { error = "Impossibile processare il rimborso" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante il rimborso per {PaymentIntentId}", request.PaymentIntentId);
                return StatusCode(500, new { error = "Errore interno del server" });
            }
        }

        /// <summary>
        /// Verifica lo stato di un pagamento
        /// </summary>
        [HttpGet("payment-status/{paymentIntentId}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetPaymentStatus(string paymentIntentId)
        {
            try
            {
                // Questo è un endpoint di esempio - potresti voler implementare
                // una logica più sofisticata per recuperare lo stato dal database
                _logger.LogInformation("Richiesta stato pagamento per {PaymentIntentId}", paymentIntentId);

                // Per ora restituiamo un placeholder
                return Ok(new
                {
                    paymentIntentId = paymentIntentId,
                    status = "unknown",
                    message = "Implementa la logica di recupero stato"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante il recupero dello stato per {PaymentIntentId}", paymentIntentId);
                return StatusCode(500, new { error = "Errore interno del server" });
            }
        }
    }

    // DTO aggiuntivi per il controller
    public class ConfirmPaymentRequestDTO
    {
        public string PaymentIntentId { get; set; }
    }

    public class RefundPaymentRequestDTO
    {
        public string PaymentIntentId { get; set; }
        public string Reason { get; set; } = "requested_by_customer";
    }
}