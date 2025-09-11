using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTO
{
    public class StatoStoricoOrdineDTO
    {
        public int StatoStoricoOrdineId { get; set; }
        public int OrdineId { get; set; }
        public int StatoOrdineId { get; set; }
        public DateTime Inizio { get; set; }
        public DateTime? Fine { get; set; }
    }
}
