using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTO
{
    public class SessioniQrDTO
    {
        public Guid SessioneId { get; set; }
        public int ClienteId { get; set; }
        public string QrCode { get; set; } = null!;
        public DateTime? DataCreazione { get; set; }
        public DateTime DataScadenza { get; set; }
        public bool? Utilizzato { get; set; }
        public DateTime? DataUtilizzo { get; set; }
    }
}
