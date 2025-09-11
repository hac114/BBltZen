using System;
using System.Collections.Generic;

namespace Database;

public partial class PreferitiCliente
{
    public int PreferitoId { get; set; }

    public int ClienteId { get; set; }

    public int BevandaId { get; set; }

    public DateTime? DataAggiunta { get; set; }

    public virtual BevandaStandard Bevanda { get; set; } = null!;

    public virtual Cliente Cliente { get; set; } = null!;
}
