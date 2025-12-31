using System.ComponentModel.DataAnnotations;

namespace DTO
{
    public class BevandaStandardCardDTO
    {
        public int ArticoloId { get; set; }

        [StringLength(100)]
        public string Nome { get; set; } = null!;

        [StringLength(500)]
        public string? Descrizione { get; set; }

        [StringLength(500)]
        public string? ImmagineUrl { get; set; }

        public bool Disponibile { get; set; }
        public bool SempreDisponibile { get; set; }
        public int Priorita { get; set; }

        // ✅ NUOVO: Prezzi calcolati per ogni dimensione
        public List<PrezzoDimensioneDTO> PrezziPerDimensioni { get; set; } = [];

        // ✅ NUOVO: Lista ingredienti per la descrizione
        public List<string> Ingredienti { get; set; } = [];
    }

    public class PrezzoDimensioneDTO
    {
        public int DimensioneBicchiereId { get; set; }
        public string Sigla { get; set; } = null!; // "M", "L"
        public string Descrizione { get; set; } = null!; // "Medio 500ml", "Large 700ml"
        public decimal PrezzoNetto { get; set; }
        public decimal PrezzoIva { get; set; }
        public decimal PrezzoTotale { get; set; }
        public decimal AliquotaIva { get; set; }
    }
}