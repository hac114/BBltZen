using System.ComponentModel.DataAnnotations;

namespace DTO
{
    public class StripePaymentRequestDTO
    {
        [Required]
        public int OrdineId { get; set; }

        [Required]
        [Range(50, 9999999)] // Minimo 0.50€ in centesimi
        public long Amount { get; set; } // Importo in centesimi (es: 1000 = 10.00€)

        [Required]
        [StringLength(3)]
        public string Currency { get; set; } = "eur";

        [StringLength(500)]
        public string Description { get; set; }

        public string CustomerEmail { get; set; }
    }

    public class StripePaymentResponseDTO
    {
        public string ClientSecret { get; set; }
        public string PaymentIntentId { get; set; }
        public string Status { get; set; }
        public long Amount { get; set; }
        public string Currency { get; set; }
    }

    public class StripeWebhookDTO
    {
        public string Type { get; set; }
        public StripeWebhookDataDTO Data { get; set; }
    }

    public class StripeWebhookDataDTO
    {
        public StripeWebhookObjectDTO Object { get; set; }
    }

    public class StripeWebhookObjectDTO
    {
        public string Id { get; set; }
        public string Status { get; set; }
        public string Customer { get; set; }
        public long Amount { get; set; }
    }
}