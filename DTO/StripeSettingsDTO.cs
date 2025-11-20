using System.ComponentModel.DataAnnotations;

namespace DTO
{
    public class StripeSettingsDTO
    {
        [Required(ErrorMessage = "La chiave segreta è obbligatoria")]
        [StringLength(200, ErrorMessage = "La chiave segreta non può superare 200 caratteri")]
        public required string SecretKey { get; set; }

        [Required(ErrorMessage = "La chiave pubblicabile è obbligatoria")]
        [StringLength(200, ErrorMessage = "La chiave pubblicabile non può superare 200 caratteri")]
        public required string PublishableKey { get; set; }

        [Required(ErrorMessage = "Il segreto webhook è obbligatorio")]
        [StringLength(200, ErrorMessage = "Il segreto webhook non può superare 200 caratteri")]
        public required string WebhookSecret { get; set; }
    }
}