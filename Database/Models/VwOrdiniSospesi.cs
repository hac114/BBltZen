using System;
using System.Collections.Generic;

namespace BBltZen;

public partial class VwOrdiniSospesi
{
    public int OrdineId { get; set; }

    public DateTime DataCreazione { get; set; }

    public int? MinutiSospeso { get; set; }

    public int ClienteId { get; set; }

    public decimal Totale { get; set; }

    public string LivelloAllerta { get; set; } = null!;
}
