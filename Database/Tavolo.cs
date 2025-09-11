using System;
using System.Collections.Generic;

namespace Database;

public partial class Tavolo
{
    public int TavoloId { get; set; }

    public string QrCode { get; set; } = null!;

    public bool Disponibile { get; set; }

    public int Numero { get; set; }

    public string? Zona { get; set; }

    public virtual ICollection<Cliente> Cliente { get; set; } = new List<Cliente>();
}
