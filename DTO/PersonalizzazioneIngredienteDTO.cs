using System.ComponentModel.DataAnnotations;

namespace DTO
{
    public class PersonalizzazioneIngredienteDTO
    {
        public int PersonalizzazioneIngredienteId { get; set; }
        public int PersonalizzazioneId { get; set; }
        public int IngredienteId { get; set; }

        [Range(0.01, 1000, ErrorMessage = "La quantità deve essere tra 0.01 e 1000")]
        public decimal Quantita { get; set; }
        public int UnitaMisuraId { get; set; }
    }
}