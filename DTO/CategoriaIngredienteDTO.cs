using System.ComponentModel.DataAnnotations;

namespace DTO
{
    public class CategoriaIngredienteDTO
    {
        public int CategoriaId { get; set; }

        [StringLength(50, ErrorMessage = "La categoria non può superare 50 caratteri")]
        public required string Categoria { get; set; }
    }    
}