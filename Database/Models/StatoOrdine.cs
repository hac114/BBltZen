using System;
using System.Collections.Generic;

namespace BBltZen;

public partial class StatoOrdine
{
    public int StatoOrdineId { get; set; }

    public string StatoOrdine1 { get; set; } = null!;

    public bool Terminale { get; set; }

    public virtual ConfigSoglieTempi? ConfigSoglieTempi { get; set; }

    public virtual ICollection<Ordine> Ordine { get; set; } = new List<Ordine>();

    public virtual ICollection<StatoStoricoOrdine> StatoStoricoOrdine { get; set; } = new List<StatoStoricoOrdine>();
}
