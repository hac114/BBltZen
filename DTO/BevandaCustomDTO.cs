using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTO
{
    public class BevandaCustomDTO
    {
        public int BevandaCustomId { get; set; }
        public int ArticoloId { get; set; }
        public int PersCustomId { get; set; }
        public decimal Prezzo { get; set; }
        public DateTime DataCreazione { get; set; }
        public DateTime DataAggiornamento { get; set; }
    }
}
