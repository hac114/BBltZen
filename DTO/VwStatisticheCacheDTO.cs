using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTO
{
    public class VwStatisticheCacheDTO
    {
        public int Id { get; set; }
        public string TipoStatistica { get; set; } = null!;
        public string Periodo { get; set; } = null!;
        public DateOnly? DataRiferimento { get; set; }
        public string? TotaleOrdini { get; set; }
        public string? OrdiniAnnullati { get; set; }
        public string? OrdiniConsegnati { get; set; }
        public string? TempoMedioMinuti { get; set; }
        public string? FatturatoTotale { get; set; }
        public DateTime? DataAggiornamento { get; set; }
    }
}
