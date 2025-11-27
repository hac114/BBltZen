using System.ComponentModel.DataAnnotations;

namespace DTO
{
    public class CategoriaIngredienteFrontendDTO
    {
        [Required(ErrorMessage = "Il nome della categoria è obbligatorio")]
        [StringLength(50, ErrorMessage = "La categoria non può superare 50 caratteri")]
        public string Categoria { get; set; } = null!;
    }
}