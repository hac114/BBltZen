using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DTO
{
    public class OrderTotalDTO
    {
        public int OrderId { get; set; }

        [Range(0, 10000)]
        public decimal SubTotale { get; set; }

        [Range(0, 5000)]
        public decimal TotaleIVA { get; set; }

        [Range(0, 15000)]
        public decimal TotaleGenerale { get; set; }

        public DateTime DataCalcolo { get; set; }

        public List<OrderItemTotalDTO> Items { get; set; } = new List<OrderItemTotalDTO>();
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
        public int OrderId { get; set; }

        [Range(0, 15000)]
        public decimal VecchioTotale { get; set; }

        [Range(0, 15000)]
        public decimal NuovoTotale { get; set; }

        public decimal Differenza { get; set; }

        public DateTime DataAggiornamento { get; set; }
    }
}