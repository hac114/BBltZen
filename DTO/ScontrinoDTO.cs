using System.ComponentModel.DataAnnotations;

namespace DTO
{
    public class ScontrinoDTO
    {
        public int OrdineId { get; set; }

        [Required]
        public List<string> RigheScontrino { get; set; } = new List<string>();

        public DateTime DataGenerazione { get; set; }
        public decimal TotaleOrdine { get; set; }
        public string StatoOrdine { get; set; } = string.Empty;
        public string StatoPagamento { get; set; } = string.Empty;
    }

    public class GeneraScontrinoRequestDTO
    {
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "L'ID ordine deve essere valido")]
        public int OrdineId { get; set; }
    }
}