using System;
using System.Collections.Generic;

namespace Database.Models;

public partial class StatoOrdine
{
    public int StatoOrdineId { get; set; }

    public string StatoOrdine1 { get; set; } = null!;

    public bool Terminale { get; set; }

    public virtual ICollection<ConfigSoglieTempi> ConfigSoglieTempi { get; set; } = new List<ConfigSoglieTempi>();

    public virtual ICollection<Ordine> Ordine { get; set; } = new List<Ordine>();

    public virtual ICollection<StatoStoricoOrdine> StatoStoricoOrdine { get; set; } = new List<StatoStoricoOrdine>();
}
