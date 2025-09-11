using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTO
{
    public class PreferitiClienteDTO
    {
        public int PreferitoId { get; set; }
        public int ClienteId { get; set; }
        public int BevandaId { get; set; }
        public DateTime? DataAggiunta { get; set; }
    }
}
