using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTO
{
    public class ClienteDTO
    {
        public int ClienteId { get; set; }
        public int TavoloId { get; set; }        
        public DateTime DataCreazione { get; set; }
        public DateTime DataAggiornamento { get; set; }
        
    }
}
