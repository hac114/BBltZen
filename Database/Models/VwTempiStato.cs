using System;
using System.Collections.Generic;

namespace Database.Models;

public partial class VwTempiStato
{
    public string StatoOrdine { get; set; } = null!;

    public int? NumeroOrdini { get; set; }

    public int? TempoMedioMinuti { get; set; }

    public int? TempoMassimoMinuti { get; set; }

    public int? TempoMinimoMinuti { get; set; }
}
