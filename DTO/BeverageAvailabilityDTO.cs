using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DTO
{
    public class BeverageAvailabilityDTO
    {
        [Range(1, int.MaxValue, ErrorMessage = "ID articolo non valido")]
        public int ArticoloId { get; set; }

        [Required(ErrorMessage = "Tipo articolo obbligatorio")]
        [StringLength(2, ErrorMessage = "Tipo articolo deve essere di 2 caratteri")]
        [RegularExpression("^(BS|BC|D)$", ErrorMessage = "Tipo articolo non valido")]
        public string TipoArticolo { get; set; } = null!;

        [Required(ErrorMessage = "Nome obbligatorio")]
        [StringLength(100, ErrorMessage = "Nome non può superare 100 caratteri")]
        public string Nome { get; set; } = null!;

        public bool Disponibile { get; set; }

        [StringLength(500, ErrorMessage = "Motivo non può superare 500 caratteri")]
        public string? MotivoNonDisponibile { get; set; }

        public List<IngredienteMancanteDTO> IngredientiMancanti { get; set; } = new();

        public DateTime DataVerifica { get; set; } = DateTime.UtcNow; // ✅ Valore di default
    }

    public class IngredienteMancanteDTO
    {
        public int IngredienteId { get; set; }

        [StringLength(100)]
        public string NomeIngrediente { get; set; } = null!;

        [StringLength(50)]
        public string Categoria { get; set; } = null!;

        public bool Critico { get; set; }
    }

    public class AvailabilityUpdateDTO
    {
        public int ArticoloId { get; set; }

        [StringLength(2)]
        public string TipoArticolo { get; set; } = null!;

        public bool NuovoStatoDisponibilita { get; set; }

        [StringLength(500)]
        public string? Motivo { get; set; }

        public DateTime DataAggiornamento { get; set; }
    }

    public class MenuAvailabilityDTO
    {
        [Range(0, int.MaxValue)]
        public int TotalBevande { get; set; }

        [Range(0, int.MaxValue)]
        public int BevandeDisponibili { get; set; }

        [Range(0, int.MaxValue)]
        public int BevandeNonDisponibili { get; set; }

        public List<BeverageAvailabilityDTO> PrimoPianoDisponibile { get; set; } = new();
        public List<BeverageAvailabilityDTO> SostitutiPrimoPiano { get; set; } = new();

        public DateTime DataAggiornamento { get; set; }
    }

    public class ForceAvailabilityRequest
    {
        [Required(ErrorMessage = "Lo stato di disponibilità è obbligatorio")]
        public bool Disponibile { get; set; }

        [StringLength(500, ErrorMessage = "Il motivo non può superare 500 caratteri")]
        public string? Motivo { get; set; }
    }
}