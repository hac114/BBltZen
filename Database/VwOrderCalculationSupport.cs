using System;
using System.Collections.Generic;

namespace Database;

public partial class VwOrderCalculationSupport
{
    public int OrderItemId { get; set; }

    public int OrdineId { get; set; }

    public int ArticoloId { get; set; }

    public string TipoArticolo { get; set; } = null!;

    public int Quantita { get; set; }

    public decimal? PrezzoBase { get; set; }

    public decimal? TaxRate { get; set; }
}
