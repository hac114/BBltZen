using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTO
{
    public class StatisticheCacheDTO
    {
        public int Id { get; set; }
        public string TipoStatistica { get; set; } = null!;
        public string Periodo { get; set; } = null!;
        public string Metriche { get; set; } = null!;
        public DateTime? DataAggiornamento { get; set; }
    }
}
