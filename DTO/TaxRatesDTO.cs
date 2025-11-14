using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTO
{
    public class TaxRatesDTO
    {
        public int TaxRateId { get; set; }

        [Range(0, 100, ErrorMessage = "L'aliquota deve essere tra 0 e 100")]
        public decimal Aliquota { get; set; }

        [StringLength(100, ErrorMessage = "La descrizione non può superare 100 caratteri")]
        public string Descrizione { get; set; } = null!;

        public DateTime DataCreazione { get; set; }
        public DateTime DataAggiornamento { get; set; }
    }
}