using System;
using System.Collections.Generic;

namespace BBltZen;

public partial class VwDashboardAmministrativa
{
    public int StatoOrdineId { get; set; }

    public string StatoOrdine { get; set; } = null!;

    public int? TotaleOrdini { get; set; }

    public decimal TempoMedioMinuti { get; set; }

    public int TempoMassimo { get; set; }

    public double Mediana { get; set; }

    public double Percentile90 { get; set; }

    public int SogliaAttenzione { get; set; }

    public int SogliaCritico { get; set; }

    public int LivelloPriorita { get; set; }

    public string TooltipStatistiche { get; set; } = null!;
}
