using System.ComponentModel.DataAnnotations;
namespace DTO
{
    public class TavoloDTO
    {
        // ✅ ID SEMPRE PRESENTE (per tutte le operazioni)
        public int TavoloId { get; set; }

        // ✅ PROPRIETÀ BASE
        [Required(ErrorMessage = "Il numero tavolo è obbligatorio")]
        [Range(1, 100, ErrorMessage = "Il numero tavolo deve essere tra 1 e 100")]
        public int Numero { get; set; }

        [Required(ErrorMessage = "La disponibilità è obbligatoria")]
        public bool Disponibile { get; set; } = true;

        [StringLength(50, ErrorMessage = "La zona non può superare 50 caratteri")]
        [RegularExpression(@"^[A-Za-z\s]*$", ErrorMessage = "La zona può contenere solo lettere e spazi")]
        public string? Zona { get; set; }

        // ✅ EVENTUALI CAMPI "INTELLIGENTI" DAL FRONTEND
        // Esempio: se FrontendDTO aveva campi calcolati/derivati
        //public string? DisponibileTestuale => Disponibile ? "SI" : "NO";
        //public string? Stato => Disponibile ? "Libero" : "Occupato";
        //public string? DescrizioneCompleta => $"Tavolo {Numero} - {Zona}";
    }
}