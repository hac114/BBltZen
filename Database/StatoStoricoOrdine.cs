using System;
using System.Collections.Generic;

namespace Database;

public partial class StatoStoricoOrdine
{
    public int StatoStoricoOrdineId { get; set; }

    public int OrdineId { get; set; }

    public int StatoOrdineId { get; set; }

    public DateTime Inizio { get; set; }

    public DateTime? Fine { get; set; }

    public virtual Ordine Ordine { get; set; } = null!;

    public virtual StatoOrdine StatoOrdine { get; set; } = null!;
}
