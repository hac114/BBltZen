using System;
using System.Collections.Generic;

namespace Database;

public partial class IvaBackup
{
    public int IvaId { get; set; }

    public decimal Aliquota { get; set; }

    public string Descrizione { get; set; } = null!;

    public string? CodiceCategoria { get; set; }

    public DateOnly? DataInizioValidita { get; set; }

    public DateOnly? DataFineValidita { get; set; }
}
