using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTO
{
    public class PriceCalculationServiceDTO
    {
        [Range(0, 10000, ErrorMessage = "Il prezzo base deve essere tra 0 e 10000")]
        public decimal PrezzoBase { get; set; }

        [Range(0, 10000, ErrorMessage = "L'imponibile deve essere tra 0 e 10000")]
        public decimal Imponibile { get; set; }

        [Range(0, 2200, ErrorMessage = "L'IVA amount deve essere tra 0 e 2200")]
        public decimal IvaAmount { get; set; }

        [Range(0, 12000, ErrorMessage = "Il totale ivato deve essere tra 0 e 12000")]
        public decimal TotaleIvato { get; set; }

        public int TaxRateId { get; set; }

        [Range(0, 100, ErrorMessage = "Il tax rate deve essere tra 0 e 100")]
        public decimal TaxRate { get; set; }

        [StringLength(1000, ErrorMessage = "Il calcolo dettaglio non può superare 1000 caratteri")]
        public string? CalcoloDettaglio { get; set; }
    }
}