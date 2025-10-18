using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTO
{
    public class TavoloDTO
    {
        public int TavoloId { get; set; }
        public bool Disponibile { get; set; }
        public int Numero { get; set; }
        public string? Zona { get; set; }
    }
}