using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTO
{
    public class OrdineDTO
    {
        public int OrdineId { get; set; }
        public int ClienteId { get; set; }
        public DateTime DataCreazione { get; set; }
        public DateTime DataAggiornamento { get; set; }
        public int? StatoOrdineId { get; set; }
        public int? StatoPagamentoId { get; set; }

        [Range(0.01, 100000, ErrorMessage = "Il totale deve essere tra 0.01 e 100000")]
        public decimal Totale { get; set; }

        [Range(1, 10, ErrorMessage = "La priorità deve essere tra 1 e 10")]
        public int? Priorita { get; set; }
    }
}