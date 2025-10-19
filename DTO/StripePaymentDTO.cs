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

        [EmailAddress]
        [StringLength(255)]
        public string CustomerEmail { get; set; }
    }

    public class StripePaymentResponseDTO
    {
        [Required]
        [StringLength(500)]
        public string ClientSecret { get; set; }

        [Required]
        [StringLength(100)]
        public string PaymentIntentId { get; set; }

        [Required]
        [StringLength(50)]
        public string Status { get; set; }

        [Range(0, 9999999)]
        public long Amount { get; set; }

        [Required]
        [StringLength(3)]
        public string Currency { get; set; }
    }

    public class StripeWebhookDTO
    {
        [Required]
        [StringLength(100)]
        public string Type { get; set; }

        [Required]
        public StripeWebhookDataDTO Data { get; set; }
    }

    public class StripeWebhookDataDTO
    {
        [Required]
        public StripeWebhookObjectDTO Object { get; set; }
    }

    public class StripeWebhookObjectDTO
    {
        [Required]
        [StringLength(100)]
        public string Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Status { get; set; }

        [StringLength(100)]
        public string Customer { get; set; }

        [Range(0, 9999999)]
        public long Amount { get; set; }
    }
}