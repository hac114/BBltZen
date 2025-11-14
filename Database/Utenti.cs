using System;
using System.Collections.Generic;

namespace Database;

public partial class Utenti
{
    public int UtenteId { get; set; }

    public int? ClienteId { get; set; }

    public string? Email { get; set; }

    public string? PasswordHash { get; set; }

    public string TipoUtente { get; set; } = null!;

    public DateTime? DataCreazione { get; set; }

    public DateTime? DataAggiornamento { get; set; }

    public DateTime? UltimoAccesso { get; set; }

    public bool? Attivo { get; set; }

    public string? Nome { get; set; }

    public string? Cognome { get; set; }

    public string? Telefono { get; set; }

    public Guid? SessioneGuest { get; set; }

    public virtual Cliente? Cliente { get; set; }

    public virtual ICollection<LogAccessi> LogAccessi { get; set; } = new List<LogAccessi>();

    public virtual ICollection<LogAttivita> LogAttivita { get; set; } = new List<LogAttivita>();
}
