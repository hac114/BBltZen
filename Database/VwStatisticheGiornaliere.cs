using System;
using System.Collections.Generic;

namespace Database;

public partial class VwStatisticheGiornaliere
{
    public DateOnly? Data { get; set; }

    public int? TotaleOrdini { get; set; }

    public int? OrdiniAnnullati { get; set; }

    public int? OrdiniConsegnati { get; set; }

    public int? TempoMedioCompletamentoMinuti { get; set; }
}
