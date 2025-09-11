using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTO
{
    public class OrdineBackupDTO
    {
        public int OrdineId { get; set; }
        public int ClienteId { get; set; }
        public DateTime DataCreazione { get; set; }
        public DateTime DataAggiornamento { get; set; }
        public int? StatoOrdineId { get; set; }
        public int? StatoPagamentoId { get; set; }
        public decimal Totale { get; set; }
        public int? Priorita { get; set; }
    }
}
