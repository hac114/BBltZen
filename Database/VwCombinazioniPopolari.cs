using System;
using System.Collections.Generic;

namespace Database;

public partial class VwCombinazioniPopolari
{
    public string? Combinazione { get; set; }

    public int? NumeroIngredienti { get; set; }

    public int? VolteOrdinate { get; set; }

    public DateTime? PrimaDataOrdine { get; set; }

    public DateTime? UltimaDataOrdine { get; set; }

    public int? GiorniAttivita { get; set; }

    public decimal? OrdiniPerGiorno { get; set; }
}
