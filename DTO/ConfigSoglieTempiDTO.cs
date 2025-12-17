using System.ComponentModel.DataAnnotations;

namespace DTO
{
    public class ConfigSoglieTempiDTO : IValidatableObject
    {
        public int SogliaId { get; set; }

        [Required(ErrorMessage = "L'ID stato ordine è obbligatorio")]
        [Range(1, int.MaxValue, ErrorMessage = "L'ID stato ordine deve essere maggiore di 0")]
        public int StatoOrdineId { get; set; }

        // ✅ Proprietà di navigazione per lo stato ordine completo
        public StatoOrdineDTO? StatoOrdine { get; set; }

        [Required(ErrorMessage = "La soglia attenzione è obbligatoria")]
        [Range(0, 1000, ErrorMessage = "La soglia attenzione deve essere tra 0 e 1000 minuti")]
        public int SogliaAttenzione { get; set; }

        [Required(ErrorMessage = "La soglia critico è obbligatoria")]
        [Range(0, 1000, ErrorMessage = "La soglia critico deve essere tra 0 e 1000 minuti")]
        public int SogliaCritico { get; set; }

        public DateTime? DataAggiornamento { get; set; }

        [StringLength(100, ErrorMessage = "L'utente aggiornamento non può superare 100 caratteri")]
        public string? UtenteAggiornamento { get; set; }

        // ✅ Validazione business: SogliaCritico deve essere > SogliaAttenzione
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (SogliaCritico <= SogliaAttenzione)
            {
                yield return new ValidationResult(
                    "La soglia critica deve essere maggiore della soglia di attenzione",
                    [nameof(SogliaCritico), nameof(SogliaAttenzione)]);
            }
        }
    }
}