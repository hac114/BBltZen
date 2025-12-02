using System.ComponentModel.DataAnnotations;

namespace DTO
{
    public class TaxRatesFrontendDTO
    {
        public decimal Aliquota { get; set; }
        public string Descrizione { get; set; } = null!;
        public string AliquotaFormattata { get; set; } = null!; // "22.00%"
    }
}