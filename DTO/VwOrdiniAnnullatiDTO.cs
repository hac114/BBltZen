using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTO
{
    public class VwOrdiniAnnullatiDTO
    {
        public int OrdineId { get; set; }
        public DateTime DataCreazione { get; set; }
        public DateTime DataAnnullamento { get; set; }
        public int ClienteId { get; set; }
        public decimal Totale { get; set; }
        public int? MinutiPrimaAnnullamento { get; set; }
    }
}
