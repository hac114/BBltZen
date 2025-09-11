using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTO
{
    public class VwOrdiniSospesiDTO
    {
        public int OrdineId { get; set; }
        public DateTime DataCreazione { get; set; }
        public int? MinutiSospeso { get; set; }
        public int ClienteId { get; set; }
        public decimal Totale { get; set; }
        public string LivelloAllerta { get; set; } = null!;
    }
}
