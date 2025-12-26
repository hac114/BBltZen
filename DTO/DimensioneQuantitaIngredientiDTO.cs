using System.ComponentModel.DataAnnotations;

namespace DTO
{
    public class DimensioneQuantitaIngredientiDTO
    {
        [Range(1, int.MaxValue, ErrorMessage = "DimensioneId è obbligatorio")]
        public int DimensioneId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "PersonalizzazioneIngredienteId è obbligatorio")]
        public int PersonalizzazioneIngredienteId { get; set; }

        public string? NomePersonalizzazione { get; set; }  //aggiunto

        [Range(1, int.MaxValue, ErrorMessage = "DimensioneBicchiereId è obbligatorio")]
        public int DimensioneBicchiereId { get; set; }
        public string? DescrizioneBicchiere { get; set; }  //aggiunto

        [Range(0.1, 10.0, ErrorMessage = "Il moltiplicatore deve essere tra 0.1 e 10")]
        public decimal Moltiplicatore { get; set; }
    }
}
