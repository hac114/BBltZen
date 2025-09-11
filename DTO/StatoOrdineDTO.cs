using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTO
{
    public class StatoOrdineDTO
    {
        public int StatoOrdineId { get; set; }
        public string StatoOrdine1 { get; set; } = null!;
        public bool Terminale { get; set; }
    }
}
