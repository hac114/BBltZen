using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTO
{
    public class VwTempiStatoDTO
    {
        public string StatoOrdine { get; set; } = null!;
        public int? NumeroOrdini { get; set; }
        public int? TempoMedioMinuti { get; set; }
        public int? TempoMassimoMinuti { get; set; }
        public int? TempoMinimoMinuti { get; set; }
    }
}
