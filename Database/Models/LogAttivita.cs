using System;
using System.Collections.Generic;

namespace Database.Models;

public partial class LogAttivita
{
    public int LogId { get; set; }

    public string TipoAttivita { get; set; } = null!;

    public string Descrizione { get; set; } = null!;

    public DateTime DataEsecuzione { get; set; }

    public string? Dettagli { get; set; }

    public int? UtenteId { get; set; }

    public virtual Utenti? Utente { get; set; }
}
