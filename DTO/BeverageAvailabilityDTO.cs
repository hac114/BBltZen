namespace DTO
{
    public class BeverageAvailabilityDTO
    {
        public int ArticoloId { get; set; }
        public string TipoArticolo { get; set; } = null!;
        public string Nome { get; set; } = null!;
        public bool Disponibile { get; set; }
        public string? MotivoNonDisponibile { get; set; }
        public List<IngredienteMancanteDTO> IngredientiMancanti { get; set; } = new();
        public DateTime DataVerifica { get; set; }
    }

    public class IngredienteMancanteDTO
    {
        public int IngredienteId { get; set; }
        public string NomeIngrediente { get; set; } = null!;
        public string Categoria { get; set; } = null!;
        public bool Critico { get; set; }
    }

    public class AvailabilityUpdateDTO
    {
        public int ArticoloId { get; set; }
        public string TipoArticolo { get; set; } = null!;
        public bool NuovoStatoDisponibilita { get; set; }
        public string? Motivo { get; set; }
        public DateTime DataAggiornamento { get; set; }
    }

    public class MenuAvailabilityDTO
    {
        public int TotalBevande { get; set; }
        public int BevandeDisponibili { get; set; }
        public int BevandeNonDisponibili { get; set; }
        public List<BeverageAvailabilityDTO> PrimoPianoDisponibile { get; set; } = new();
        public List<BeverageAvailabilityDTO> SostitutiPrimoPiano { get; set; } = new();
        public DateTime DataAggiornamento { get; set; }
    }
}