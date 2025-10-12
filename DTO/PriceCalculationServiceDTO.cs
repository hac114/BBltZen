using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTO
{
    public class PriceCalculationServiceDTO
    {
        public decimal PrezzoBase { get; set; }
        public decimal Imponibile { get; set; }
        public decimal IvaAmount { get; set; }
        public decimal TotaleIvato { get; set; }
        public int TaxRateId { get; set; }
        public decimal TaxRate { get; set; }
        public string? CalcoloDettaglio { get; set; }
    }
}
