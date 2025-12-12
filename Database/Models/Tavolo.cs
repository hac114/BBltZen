using System;
using System.Collections.Generic;

namespace Database.Models;

public partial class Tavolo
{
    public int TavoloId { get; set; }

    public bool Disponibile { get; set; }

    public int Numero { get; set; }

    public string? Zona { get; set; }

    public virtual ICollection<Cliente> Cliente { get; set; } = new List<Cliente>();

    public virtual ICollection<SessioniQr> SessioniQr { get; set; } = new List<SessioniQr>();
}
