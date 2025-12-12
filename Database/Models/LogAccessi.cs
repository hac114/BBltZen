using System;
using System.Collections.Generic;

namespace Database.Models;

public partial class LogAccessi
{
    public int LogId { get; set; }

    public int? UtenteId { get; set; }

    public int? ClienteId { get; set; }

    public string TipoAccesso { get; set; } = null!;

    public string Esito { get; set; } = null!;

    public string? IpAddress { get; set; }

    public string? UserAgent { get; set; }

    public DateTime? DataCreazione { get; set; }

    public string? Dettagli { get; set; }

    public virtual Cliente? Cliente { get; set; }

    public virtual Utenti? Utente { get; set; }
}
