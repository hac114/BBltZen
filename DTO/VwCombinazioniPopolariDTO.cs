using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTO
{
    public class VwCombinazioniPopolariDTO
    {
        public string? Combinazione { get; set; }
        public int? NumeroIngredienti { get; set; }
        public int? VolteOrdinate { get; set; }
        public DateTime? PrimaDataOrdine { get; set; }
        public DateTime? UltimaDataOrdine { get; set; }
        public int? GiorniAttivita { get; set; }
        public decimal? OrdiniPerGiorno { get; set; }
    }
}
