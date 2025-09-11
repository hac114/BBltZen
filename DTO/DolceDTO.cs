using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTO
{
    public class DolceDTO
    {
        public int ArticoloId { get; set; }
        public string Nome { get; set; } = null!;
        public decimal Prezzo { get; set; }
        public string? Descrizione { get; set; }
        public string? ImmagineUrl { get; set; }
        public bool Disponibile { get; set; }
        public int Priorita { get; set; }
        public DateTime DataCreazione { get; set; }
        public DateTime DataAggiornamento { get; set; }
    }
}
