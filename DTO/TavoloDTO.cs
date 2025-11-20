using System.ComponentModel.DataAnnotations;

namespace DTO
{
    public class TavoloDTO
    {
        public int TavoloId { get; set; }

        [Required(ErrorMessage = "La disponibilità è obbligatoria")]
        public bool Disponibile { get; set; } = true;

        [Required(ErrorMessage = "Il numero tavolo è obbligatorio")]
        [Range(1, 100, ErrorMessage = "Il numero tavolo deve essere tra 1 e 100")]
        public int Numero { get; set; }

        [StringLength(50, ErrorMessage = "La zona non può superare 50 caratteri")]
        [RegularExpression(@"^[A-Za-z\s]*$", ErrorMessage = "La zona può contenere solo lettere e spazi")]
        public string? Zona { get; set; }
    }
}