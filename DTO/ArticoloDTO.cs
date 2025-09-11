using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTO
{
    public class ArticoloDTO
    {
        public int ArticoloId { get; set; }
        public string Tipo { get; set; }
        public DateTime? DataCreazione { get; set; }
        public DateTime? DataAggiornamento { get; set; }
    }
}
