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
        public required string Currency { get; set; } = "eur"; // ✅ REQUIRED

        [StringLength(500)]
        public string? Description { get; set; } // ✅ NULLABLE

        [EmailAddress]
        [StringLength(255)]
        public string? CustomerEmail { get; set; } // ✅ NULLABLE
    }

    public class StripePaymentResponseDTO
    {
        [Required]
        [StringLength(500)]
        public required string ClientSecret { get; set; } // ✅ REQUIRED

        [Required]
        [StringLength(100)]
        public required string PaymentIntentId { get; set; } // ✅ REQUIRED

        [Required]
        [StringLength(50)]
        public required string Status { get; set; } // ✅ REQUIRED

        [Range(0, 9999999)]
        public long Amount { get; set; }

        [Required]
        [StringLength(3)]
        public required string Currency { get; set; } // ✅ REQUIRED
    }

    public class StripeWebhookDTO
    {
        [Required]
        [StringLength(100)]
        public required string Type { get; set; } // ✅ REQUIRED

        [Required]
        public required StripeWebhookDataDTO Data { get; set; } // ✅ REQUIRED
    }

    public class StripeWebhookDataDTO
    {
        [Required]
        public required StripeWebhookObjectDTO Object { get; set; } // ✅ REQUIRED
    }

    public class StripeWebhookObjectDTO
    {
        [Required]
        [StringLength(100)]
        public required string Id { get; set; } // ✅ REQUIRED

        [Required]
        [StringLength(50)]
        public required string Status { get; set; } // ✅ REQUIRED

        [StringLength(100)]
        public string? Customer { get; set; } // ✅ NULLABLE

        [Range(0, 9999999)]
        public long Amount { get; set; }
    }

    // ✅ DTO AGGIUNTIVI CHE ERANO NEL CONTROLLER
    public class ConfirmPaymentRequestDTO
    {
        [Required]
        public required string PaymentIntentId { get; set; } // ✅ REQUIRED
    }

    public class RefundPaymentRequestDTO
    {
        [Required]
        public required string PaymentIntentId { get; set; } // ✅ REQUIRED

        public string Reason { get; set; } = "requested_by_customer";
    }

    public class SimulatePaymentRequestDTO
    {
        [Required]
        public int OrdineId { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public long Amount { get; set; }

        public string Currency { get; set; } = "eur";

        public string? Description { get; set; } // ✅ NULLABLE

        [EmailAddress]
        public string? CustomerEmail { get; set; } // ✅ NULLABLE

        public bool AutoConfirm { get; set; } = true;
    }

    // ✅ DTO PER LO STATO PAGAMENTO (se necessario)
    public class PaymentStatusResponseDTO
    {
        public required string PaymentIntentId { get; set; } // ✅ REQUIRED
        public required string Status { get; set; } // ✅ REQUIRED
        public string? Message { get; set; } // ✅ NULLABLE
        public DateTime? LastUpdated { get; set; } // ✅ NULLABLE
    }
}