using System.ComponentModel.DataAnnotations;

namespace DTO
{
    public class PersonalizzazioneIngredienteDTO
    {
        public int PersonalizzazioneIngredienteId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "PersonalizzazioneId è obbligatorio")]
        public int PersonalizzazioneId { get; set; }

        public string? NomePersonalizzazione { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "IngredienteId è obbligatorio")]
        public int IngredienteId { get; set; }

        public string? NomeIngrediente { get; set; }

        [Range(0.001, 999.99, ErrorMessage = "La quantità deve essere tra 0.001 e 999.99")]
        public decimal Quantita { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "UnitaMisuraId è obbligatorio")]
        public int UnitaMisuraId { get; set; }

        public string? UnitaMisura { get; set; }
    }

}
