using System;
using System.Collections.Generic;

namespace BBltZen;

public partial class ConfigSoglieTempi
{
    public int SogliaId { get; set; }

    public int StatoOrdineId { get; set; }

    public int SogliaAttenzione { get; set; }

    public int SogliaCritico { get; set; }

    public DateTime? DataAggiornamento { get; set; }

    public string? UtenteAggiornamento { get; set; }

    public virtual StatoOrdine StatoOrdine { get; set; } = null!;
}
