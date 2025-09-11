using System;
using System.Collections.Generic;

namespace Database;

public partial class Utenti
{
    public int UtenteId { get; set; }

    public int? ClienteId { get; set; }

    public string Email { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public string TipoUtente { get; set; } = null!;

    public DateTime? DataCreazione { get; set; }

    public DateTime? DataAggiornamento { get; set; }

    public DateTime? UltimoAccesso { get; set; }

    public bool? Attivo { get; set; }

    public virtual Cliente? Cliente { get; set; }

    public virtual ICollection<LogAccessi> LogAccessi { get; set; } = new List<LogAccessi>();
}
