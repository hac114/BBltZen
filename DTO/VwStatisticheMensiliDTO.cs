using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTO
{
    public class VwStatisticheMensiliDTO
    {
        public int? Anno { get; set; }
        public int? Mese { get; set; }
        public int? TotaleOrdini { get; set; }
        public int? OrdiniAnnullati { get; set; }
        public decimal? FatturatoTotale { get; set; }
        public int? TempoMedioCompletamentoMinuti { get; set; }
    }
}
