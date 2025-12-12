using System;
using System.Collections.Generic;

namespace Database.Models;

public partial class VwDashboardStatistiche
{
    public string TipoStatistica { get; set; } = null!;

    public string Periodo { get; set; } = null!;

    public DateOnly? DataRiferimento { get; set; }

    public string? TotaleOrdini { get; set; }

    public string? OrdiniAnnullati { get; set; }

    public string? OrdiniConsegnati { get; set; }

    public string? TempoMedioMinuti { get; set; }

    public string? FatturatoTotale { get; set; }

    public int? GiorniMesiPassati { get; set; }
}
