using System;
using System.Collections.Generic;

namespace Database;

public partial class DimensioneQuantitaIngredienti
{
    public int DimensioneId { get; set; }

    public int PersonalizzazioneIngredienteId { get; set; }

    public int DimensioneBicchiereId { get; set; }

    public decimal Moltiplicatore { get; set; }

    public virtual DimensioneBicchiere DimensioneBicchiere { get; set; } = null!;

    public virtual PersonalizzazioneIngrediente PersonalizzazioneIngrediente { get; set; } = null!;
}
