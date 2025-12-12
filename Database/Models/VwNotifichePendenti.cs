using System;
using System.Collections.Generic;

namespace Database.Models;

public partial class VwNotifichePendenti
{
    public int NotificaId { get; set; }

    public DateTime DataCreazione { get; set; }

    public string OrdiniCoinvolti { get; set; } = null!;

    public string Messaggio { get; set; } = null!;

    public int? Priorita { get; set; }

    public int? MinutiDaCreazione { get; set; }
}
