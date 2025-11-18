using System.ComponentModel.DataAnnotations;

namespace DTO
{
    public class SoglieValidationRequestDTO
    {
        [Required(ErrorMessage = "La soglia attenzione è obbligatoria")]
        [Range(0, 1000, ErrorMessage = "La soglia attenzione deve essere tra 0 e 1000 minuti")]
        public int SogliaAttenzione { get; set; }

        [Required(ErrorMessage = "La soglia critico è obbligatoria")]
        [Range(0, 1000, ErrorMessage = "La soglia critico deve essere tra 0 e 1000 minuti")]
        public int SogliaCritico { get; set; }
    }
}