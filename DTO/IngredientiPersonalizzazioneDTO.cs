using System;
using System.ComponentModel.DataAnnotations;

namespace DTO
{
    public class IngredientiPersonalizzazioneDTO
    {
        public int IngredientePersId { get; set; }

        [Required(ErrorMessage = "L'ID personalizzazione è obbligatorio")]
        [Range(1, int.MaxValue, ErrorMessage = "L'ID personalizzazione deve essere positivo")]
        public int PersCustomId { get; set; }

        [Required(ErrorMessage = "L'ID ingrediente è obbligatorio")]
        [Range(1, int.MaxValue, ErrorMessage = "L'ID ingrediente deve essere positivo")]
        public int IngredienteId { get; set; }

        public DateTime DataCreazione { get; set; }
    }
}