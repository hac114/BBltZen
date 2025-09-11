using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTO
{
    public class VwDashboardSintesiDTO
    {
        public string Stato { get; set; } = null!;
        public int? Quantita { get; set; }
        public int? MaxMinuti { get; set; }
        public int? InRitardoCritico { get; set; }
    }
}
