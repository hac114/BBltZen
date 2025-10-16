using System;
using System.Collections.Generic;

namespace DTO
{
    public class OrderTotalDTO
    {
        public int OrderId { get; set; }
        public decimal SubTotale { get; set; }
        public decimal TotaleIVA { get; set; }
        public decimal TotaleGenerale { get; set; }
        public DateTime DataCalcolo { get; set; }
        public List<OrderItemTotalDTO> Items { get; set; } = new List<OrderItemTotalDTO>();
    }

    public class OrderItemTotalDTO
    {
        public int OrderItemId { get; set; }
        public int ArticoloId { get; set; }
        public string TipoArticolo { get; set; } = null!;
        public int Quantita { get; set; }
        public decimal PrezzoUnitario { get; set; }
        public decimal ScontoApplicato { get; set; }
        public decimal Imponibile { get; set; }
        public decimal TotaleIVATO { get; set; }
        public decimal AliquotaIVA { get; set; }
    }

    public class OrderUpdateTotalDTO
    {
        public int OrderId { get; set; }
        public decimal VecchioTotale { get; set; }
        public decimal NuovoTotale { get; set; }
        public decimal Differenza { get; set; }
        public DateTime DataAggiornamento { get; set; }
    }
}