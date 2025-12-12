using System;
using System.Collections.Generic;

namespace Database.Models;

public partial class Cliente
{
    public int ClienteId { get; set; }

    public int TavoloId { get; set; }

    public DateTime DataCreazione { get; set; }

    public DateTime DataAggiornamento { get; set; }

    public virtual ICollection<LogAccessi> LogAccessi { get; set; } = new List<LogAccessi>();

    public virtual ICollection<Ordine> Ordine { get; set; } = new List<Ordine>();

    public virtual ICollection<PreferitiCliente> PreferitiCliente { get; set; } = new List<PreferitiCliente>();

    public virtual Tavolo Tavolo { get; set; } = null!;

    public virtual ICollection<Utenti> Utenti { get; set; } = new List<Utenti>();
}
