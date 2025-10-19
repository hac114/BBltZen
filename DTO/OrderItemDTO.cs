using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTO
{
    public class OrderItemDTO
    {
        public int OrderItemId { get; set; }
        public int OrdineId { get; set; }
        public int ArticoloId { get; set; }

        [Range(1, 500, ErrorMessage = "La quantità deve essere tra 1 e 500")]
        public int Quantita { get; set; }

        [Range(0.01, 100, ErrorMessage = "Il prezzo unitario deve essere tra 0.01 e 100")]
        public decimal PrezzoUnitario { get; set; }

        [Range(0, 100, ErrorMessage = "Lo sconto applicato deve essere tra 0 e 100")]
        public decimal ScontoApplicato { get; set; }

        [Range(0.01, 10000, ErrorMessage = "L'imponibile deve essere tra 0.01 e 10000")]
        public decimal Imponibile { get; set; }

        public DateTime DataCreazione { get; set; }
        public DateTime DataAggiornamento { get; set; }

        [StringLength(2, ErrorMessage = "Il tipo articolo non può superare 2 caratteri")]
        public string TipoArticolo { get; set; } = null!;

        [Range(0.01, 12000, ErrorMessage = "Il totale ivato deve essere tra 0.01 e 12000")]
        public decimal? TotaleIvato { get; set; }

        public int TaxRateId { get; set; }
    }
}