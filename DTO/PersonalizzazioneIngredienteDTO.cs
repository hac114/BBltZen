using System.ComponentModel.DataAnnotations;

public class PersonalizzazioneIngredienteDTO
{
    public int PersonalizzazioneIngredienteId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "PersonalizzazioneId è obbligatorio")]
    public int PersonalizzazioneId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "IngredienteId è obbligatorio")]
    public int IngredienteId { get; set; }

    [Range(0.001, 1000, ErrorMessage = "La quantità deve essere tra 0.001 e 1000")]
    public decimal Quantita { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "UnitaMisuraId è obbligatorio")]
    public int UnitaMisuraId { get; set; }
}