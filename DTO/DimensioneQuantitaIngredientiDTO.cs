using System.ComponentModel.DataAnnotations;

public class DimensioneQuantitaIngredientiDTO
{
    [Range(1, int.MaxValue, ErrorMessage = "DimensioneId è obbligatorio")]
    public int DimensioneId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "PersonalizzazioneIngredienteId è obbligatorio")]
    public int PersonalizzazioneIngredienteId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "DimensioneBicchiereId è obbligatorio")]
    public int DimensioneBicchiereId { get; set; }

    [Range(0.1, 3.0, ErrorMessage = "Il moltiplicatore deve essere tra 0.1 e 3")]
    public decimal Moltiplicatore { get; set; }
}