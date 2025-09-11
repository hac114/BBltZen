using System;
using System.Collections.Generic;

namespace Database;

public partial class LogAttivita
{
    public int LogId { get; set; }

    public string TipoAttivita { get; set; } = null!;

    public string Descrizione { get; set; } = null!;

    public DateTime DataEsecuzione { get; set; }

    public string? Dettagli { get; set; }
}
