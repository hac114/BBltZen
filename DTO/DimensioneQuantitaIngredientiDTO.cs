using System.ComponentModel.DataAnnotations;

namespace DTO
{
    public class DimensioneQuantitaIngredientiDTO
    {
        public int DimensioneId { get; set; }
        public int PersonalizzazioneIngredienteId { get; set; }
        public int DimensioneBicchiereId { get; set; }

        [Range(0.1, 3, ErrorMessage = "Il moltiplicatore deve essere tra 0.1 e 3")]
        public decimal Moltiplicatore { get; set; }
        
    }
}
