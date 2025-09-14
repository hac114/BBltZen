using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTO
{
    public class TaxRatesDTO
    {
    public int TaxRateId { get; set; }
    public decimal Aliquota { get; set; }
    public string Descrizione { get; set; } = null!;
    public DateTime? DataCreazione { get; set; }
    public DateTime? DataAggiornamento { get; set; }
    }
}
