using System;
using System.Collections.Generic;

namespace BBltZen;

public partial class DimensioneBicchiere
{
    public int DimensioneBicchiereId { get; set; }

    public string Sigla { get; set; } = null!;

    public string Descrizione { get; set; } = null!;

    public decimal Capienza { get; set; }

    public int UnitaMisuraId { get; set; }

    public decimal PrezzoBase { get; set; }

    public decimal Moltiplicatore { get; set; }

    public virtual ICollection<BevandaStandard> BevandaStandard { get; set; } = new List<BevandaStandard>();

    public virtual ICollection<DimensioneQuantitaIngredienti> DimensioneQuantitaIngredienti { get; set; } = new List<DimensioneQuantitaIngredienti>();

    public virtual ICollection<PersonalizzazioneCustom> PersonalizzazioneCustom { get; set; } = new List<PersonalizzazioneCustom>();

    public virtual ICollection<PreferitiCliente> PreferitiCliente { get; set; } = new List<PreferitiCliente>();

    public virtual UnitaDiMisura UnitaMisura { get; set; } = null!;
}
