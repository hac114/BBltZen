using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DTO
{
    public class OrderTotalDTO
    {
        [Range(1, int.MaxValue, ErrorMessage = "ID ordine non valido")]
        public int OrderId { get; set; }

        [Range(0, 10000, ErrorMessage = "Subtotale non valido")]
        public decimal SubTotale { get; set; }

        [Range(0, 5000, ErrorMessage = "Totale IVA non valido")]
        public decimal TotaleIVA { get; set; }

        [Range(0, 15000, ErrorMessage = "Totale generale non valido")]
        public decimal TotaleGenerale { get; set; }

        public DateTime DataCalcolo { get; set; } = DateTime.UtcNow; // ✅ Default value

        public List<OrderItemTotalDTO> Items { get; set; } = new();
    }

    public class OrderItemTotalDTO
    {
        public int OrderItemId { get; set; }
        public int ArticoloId { get; set; }

        [StringLength(2)]
        public string TipoArticolo { get; set; } = null!;

        [Range(1, 500)]
        public int Quantita { get; set; }

        [Range(0.01, 100)]
        public decimal PrezzoUnitario { get; set; }

        [Range(0, 100)]
        public decimal ScontoApplicato { get; set; }

        [Range(0.01, 10000)]
        public decimal Imponibile { get; set; }

        [Range(0.01, 12000)]
        public decimal TotaleIVATO { get; set; }

        [Range(0, 100)]
        public decimal AliquotaIVA { get; set; }
    }

    public class OrderUpdateTotalDTO
    {
        [Range(1, int.MaxValue, ErrorMessage = "ID ordine non valido")]
        public int OrderId { get; set; }

        [Range(0, 15000, ErrorMessage = "Vecchio totale non valido")]
        public decimal VecchioTotale { get; set; }

        [Range(0, 15000, ErrorMessage = "Nuovo totale non valido")]
        public decimal NuovoTotale { get; set; }

        public decimal Differenza { get; set; }

        public DateTime DataAggiornamento { get; set; } = DateTime.UtcNow; // ✅ Default value
    }
}