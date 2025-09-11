using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTO
{
    public class LogAttivitaDTO
    {
        public int LogId { get; set; }
        public string TipoAttivita { get; set; } = null!;
        public string Descrizione { get; set; } = null!;
        public DateTime DataEsecuzione { get; set; }
        public string? Dettagli { get; set; }
    }
}
