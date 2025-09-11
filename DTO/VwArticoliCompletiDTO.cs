using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTO
{
    public class VwArticoliCompletiDTO
    {
        public int ArticoloId { get; set; }
        public string TipoArticolo { get; set; } = null!;
        public DateTime? DataCreazione { get; set; }
        public DateTime? DataAggiornamento { get; set; }
        public string? NomeArticolo { get; set; }
        public decimal? PrezzoBase { get; set; }
        public decimal? AliquotaIva { get; set; }
        public int? Disponibile { get; set; }
        public string Categoria { get; set; } = null!;
    }
}
