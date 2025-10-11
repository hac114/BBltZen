namespace DTO
{
    public class PersonalizzazioneIngredienteDTO
    {
        public int PersonalizzazioneIngredienteId { get; set; }
        public int PersonalizzazioneId { get; set; }
        public int IngredienteId { get; set; }
        public decimal Quantita { get; set; }
        public int UnitaMisuraId { get; set; }
    }
}