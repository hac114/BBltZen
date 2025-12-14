using System;
using System.Collections.Generic;

namespace BBltZen;

public partial class CategoriaIngrediente
{
    public int CategoriaId { get; set; }

    public string Categoria { get; set; } = null!;

    public virtual ICollection<Ingrediente> Ingrediente { get; set; } = new List<Ingrediente>();
}
