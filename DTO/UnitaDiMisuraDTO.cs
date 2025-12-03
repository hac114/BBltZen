using System.ComponentModel.DataAnnotations;

namespace DTO
{
    public class UnitaDiMisuraDTO
    {
        public int UnitaMisuraId { get; set; }

        [Required(ErrorMessage = "La sigla è obbligatoria")]
        [StringLength(10, ErrorMessage = "La sigla non può superare 10 caratteri")] // ✅ AGGIUSTATO: 10 caratteri come DB
        [RegularExpression(@"^[A-Z]+$", ErrorMessage = "La sigla deve contenere solo lettere maiuscole")]
        public string Sigla { get; set; } = null!;

        [Required(ErrorMessage = "La descrizione è obbligatoria")]
        [StringLength(50, ErrorMessage = "La descrizione non può superare 50 caratteri")] // ✅ AGGIUSTATO: 50 caratteri come DB
        public string Descrizione { get; set; } = null!;
    }
}