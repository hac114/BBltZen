using System.ComponentModel.DataAnnotations;

namespace DTO
{
    public class TaxRatesFrontendDTO
    {
        [Range(0, 100, ErrorMessage = "L'aliquota deve essere tra 0 e 100")]
        public decimal Aliquota { get; set; }

        [StringLength(100, ErrorMessage = "La descrizione non può superare 100 caratteri")]
        public string Descrizione { get; set; } = null!;

        public string AliquotaFormattata { get; set; } = string.Empty; // "22%"
    }
}