using System;
using System.Collections.Generic;

namespace BBltZen;

public partial class VwStatisticheOrdiniAvanzate
{
    public int OrdineId { get; set; }

    public string StatoOrdine { get; set; } = null!;

    public int? MinutiInStato { get; set; }

    public int? SogliaAttenzione { get; set; }

    public int? SogliaCritico { get; set; }

    public string LivelloAllerta { get; set; } = null!;

    public string MessaggioAllerta { get; set; } = null!;
}
