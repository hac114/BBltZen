using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTO
{
    public class ConfigSoglieTempiDTO
    {
        public int SogliaId { get; set; }
        public int StatoOrdineId { get; set; }
        public int SogliaAttenzione { get; set; }
        public int SogliaCritico { get; set; }
        public DateTime? DataAggiornamento { get; set; }
        public string? UtenteAggiornamento { get; set; }
    }
}
