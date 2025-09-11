using System;
using System.Collections.Generic;

namespace Database;

public partial class Cliente
{
    public int ClienteId { get; set; }

    public int TavoloId { get; set; }

    public string SessioneId { get; set; } = null!;

    public DateTime DataCreazione { get; set; }

    public DateTime DataAggiornamento { get; set; }

    public virtual ICollection<LogAccessi> LogAccessi { get; set; } = new List<LogAccessi>();

    public virtual ICollection<Ordine> Ordine { get; set; } = new List<Ordine>();

    public virtual ICollection<PreferitiCliente> PreferitiCliente { get; set; } = new List<PreferitiCliente>();

    public virtual ICollection<SessioniQr> SessioniQr { get; set; } = new List<SessioniQr>();

    public virtual Tavolo Tavolo { get; set; } = null!;

    public virtual ICollection<Utenti> Utenti { get; set; } = new List<Utenti>();
}
