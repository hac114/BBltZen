using System;
using System.Collections.Generic;

namespace Database.Models;

public partial class VwStatisticheMensili
{
    public int? Anno { get; set; }

    public int? Mese { get; set; }

    public int? TotaleOrdini { get; set; }

    public int? OrdiniAnnullati { get; set; }

    public decimal? FatturatoTotale { get; set; }

    public int? TempoMedioCompletamentoMinuti { get; set; }
}
