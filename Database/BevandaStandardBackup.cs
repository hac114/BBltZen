using System;
using System.Collections.Generic;

namespace Database;

public partial class BevandaStandardBackup
{
    public int ArticoloId { get; set; }

    public int PersonalizzazioneId { get; set; }

    public int DimensioneBicchiereId { get; set; }

    public decimal Prezzo { get; set; }

    public string? Descrizione { get; set; }

    public string? ImmagineUrl { get; set; }

    public bool Disponibile { get; set; }

    public bool SempreDisponibile { get; set; }

    public int Priorita { get; set; }

    public DateTime DataCreazione { get; set; }

    public DateTime DataAggiornamento { get; set; }
}
