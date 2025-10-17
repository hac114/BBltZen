using System;
using System.Collections.Generic;

namespace Database;

public partial class SessioniQr
{
    public Guid SessioneId { get; set; }

    public int TavoloId { get; set; }           // ✅ CAMBIATO: ClienteId → TavoloId

    public int? ClienteId { get; set; }         // ✅ MODIFICATO: ora nullable

    public string CodiceSessione { get; set; } = null!;  // ✅ NUOVO

    public string Stato { get; set; } = "Attiva";  // ✅ NUOVO

    public string QrCode { get; set; } = null!;

    public DateTime? DataCreazione { get; set; }

    public DateTime DataScadenza { get; set; }

    public bool? Utilizzato { get; set; }

    public DateTime? DataUtilizzo { get; set; }    
}