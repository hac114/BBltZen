using System;
using System.Collections.Generic;

namespace BBltZen;

public partial class PersonalizzazioneIngrediente
{
    public int PersonalizzazioneIngredienteId { get; set; }

    public int PersonalizzazioneId { get; set; }

    public int IngredienteId { get; set; }

    public decimal Quantita { get; set; }

    public int UnitaMisuraId { get; set; }

    public virtual ICollection<DimensioneQuantitaIngredienti> DimensioneQuantitaIngredienti { get; set; } = new List<DimensioneQuantitaIngredienti>();

    public virtual Ingrediente Ingrediente { get; set; } = null!;

    public virtual Personalizzazione Personalizzazione { get; set; } = null!;

    public virtual UnitaDiMisura UnitaMisura { get; set; } = null!;
}
