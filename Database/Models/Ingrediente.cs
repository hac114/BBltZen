using System;
using System.Collections.Generic;

namespace BBltZen;

public partial class Ingrediente
{
    public int IngredienteId { get; set; }

    public string Ingrediente1 { get; set; } = null!;

    public int CategoriaId { get; set; }

    public decimal PrezzoAggiunto { get; set; }

    public bool Disponibile { get; set; }

    public DateTime DataInserimento { get; set; }

    public DateTime DataAggiornamento { get; set; }

    public virtual CategoriaIngrediente Categoria { get; set; } = null!;

    public virtual ICollection<IngredientiPersonalizzazione> IngredientiPersonalizzazione { get; set; } = new List<IngredientiPersonalizzazione>();

    public virtual ICollection<PersonalizzazioneIngrediente> PersonalizzazioneIngrediente { get; set; } = new List<PersonalizzazioneIngrediente>();
}
