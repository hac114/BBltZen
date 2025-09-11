using System;
using System.Collections.Generic;

namespace Database;

public partial class NotificheOperative
{
    public int NotificaId { get; set; }

    public DateTime DataCreazione { get; set; }

    public string OrdiniCoinvolti { get; set; } = null!;

    public string Messaggio { get; set; } = null!;

    public string Stato { get; set; } = null!;

    public DateTime? DataGestione { get; set; }

    public string? UtenteGestione { get; set; }

    public int? Priorita { get; set; }
}
