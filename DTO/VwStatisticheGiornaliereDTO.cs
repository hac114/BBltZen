using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTO
{
    public class VwStatisticheGiornaliereDTO
    {
        public DateOnly? Data { get; set; }
        public int? TotaleOrdini { get; set; }
        public int? OrdiniAnnullati { get; set; }
        public int? OrdiniConsegnati { get; set; }
        public int? TempoMedioCompletamentoMinuti { get; set; }
    }
}
