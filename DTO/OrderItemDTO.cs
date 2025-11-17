using System;
using System.ComponentModel.DataAnnotations;

namespace DTO
{
    public class OrderItemDTO
    {
        public int OrderItemId { get; set; }

        [Required(ErrorMessage = "L'ID ordine è obbligatorio")]
        [Range(1, int.MaxValue, ErrorMessage = "L'ID ordine deve essere maggiore di 0")]
        public int OrdineId { get; set; }

        [Required(ErrorMessage = "L'ID articolo è obbligatorio")]
        [Range(1, int.MaxValue, ErrorMessage = "L'ID articolo deve essere maggiore di 0")]
        public int ArticoloId { get; set; }

        [Required(ErrorMessage = "La quantità è obbligatoria")]
        [Range(1, 500, ErrorMessage = "La quantità deve essere tra 1 e 500")]
        public int Quantita { get; set; }

        [Required(ErrorMessage = "Il prezzo unitario è obbligatorio")]
        [Range(0.01, 100, ErrorMessage = "Il prezzo unitario deve essere tra 0.01 e 100")]
        public decimal PrezzoUnitario { get; set; }

        [Required(ErrorMessage = "Lo sconto applicato è obbligatorio")]
        [Range(0, 100, ErrorMessage = "Lo sconto applicato deve essere tra 0 e 100")]
        public decimal ScontoApplicato { get; set; }

        [Required(ErrorMessage = "L'imponibile è obbligatorio")]
        [Range(0.01, 10000, ErrorMessage = "L'imponibile deve essere tra 0.01 e 10000")]
        public decimal Imponibile { get; set; }

        public DateTime DataCreazione { get; set; }
        public DateTime DataAggiornamento { get; set; }

        [Required(ErrorMessage = "Il tipo articolo è obbligatorio")]
        [StringLength(2, ErrorMessage = "Il tipo articolo non può superare 2 caratteri")]
        public string TipoArticolo { get; set; } = "BS"; // ✅ DEFAULT: BS = Bevanda Standard

        [Range(0.01, 12000, ErrorMessage = "Il totale ivato deve essere tra 0.01 e 12000")]
        public decimal? TotaleIvato { get; set; }

        [Required(ErrorMessage = "L'ID aliquota è obbligatorio")]
        [Range(1, int.MaxValue, ErrorMessage = "L'ID aliquota deve essere maggiore di 0")]
        public int TaxRateId { get; set; }
    }
}