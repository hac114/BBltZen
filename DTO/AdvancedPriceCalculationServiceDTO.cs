using System;
using System.Collections.Generic;

namespace DTO
{
    public class PriceCalculationRequestDTO
    {
        public int ArticoloId { get; set; }
        public string TipoArticolo { get; set; } = string.Empty; // "BS", "BC", "D"
        public int? PersonalizzazioneCustomId { get; set; }
        public int Quantita { get; set; } = 1;
        public int TaxRateId { get; set; } = 1;
        public decimal? PrezzoFisso { get; set; }
    }

    public class PriceCalculationResultDTO
    {
        public int ArticoloId { get; set; }
        public string TipoArticolo { get; set; } = string.Empty;
        public decimal PrezzoBase { get; set; }
        public decimal PrezzoUnitario { get; set; }
        public decimal Imponibile { get; set; }
        public decimal IvaAmount { get; set; }
        public decimal TotaleIvato { get; set; }
        public decimal AliquotaIva { get; set; }
        public int Quantita { get; set; }
        public decimal? ScontoApplicato { get; set; }
        public DateTime DataCalcolo { get; set; } = DateTime.Now;
    }

    public class CustomBeverageCalculationDTO
    {
        public int PersonalizzazioneCustomId { get; set; }
        public string NomePersonalizzazione { get; set; } = string.Empty;
        public int DimensioneBicchiereId { get; set; }
        public decimal PrezzoBaseDimensione { get; set; }
        public decimal MoltiplicatoreDimensione { get; set; }
        public List<IngredienteCalcoloDTO> Ingredienti { get; set; } = new();
        public decimal PrezzoTotale { get; set; }
        public decimal PrezzoIngredienti { get; set; }
    }

    public class IngredienteCalcoloDTO
    {
        public int IngredienteId { get; set; }
        public string NomeIngrediente { get; set; } = string.Empty;
        public decimal PrezzoAggiunto { get; set; }
        public decimal Quantita { get; set; }
        public string UnitaMisura { get; set; } = string.Empty;
        public decimal PrezzoCalcolato { get; set; }
    }

    public class OrderCalculationSummaryDTO
    {
        public int OrdineId { get; set; }
        public decimal TotaleImponibile { get; set; }
        public decimal TotaleIva { get; set; }
        public decimal TotaleOrdine { get; set; }
        public List<OrderItemCalculationDTO> Items { get; set; } = new();
        public DateTime DataCalcolo { get; set; } = DateTime.Now;
    }

    public class OrderItemCalculationDTO
    {
        public int OrderItemId { get; set; }
        public int ArticoloId { get; set; }
        public string TipoArticolo { get; set; } = string.Empty;
        public int Quantita { get; set; }
        public decimal PrezzoUnitario { get; set; }
        public decimal Imponibile { get; set; }
        public decimal IvaAmount { get; set; }
        public decimal TotaleIvato { get; set; }
        public decimal AliquotaIva { get; set; }
    }

    public class TaxCalculationDTO
    {
        public int TaxRateId { get; set; }
        public decimal Aliquota { get; set; }
        public string Descrizione { get; set; } = string.Empty;
        public decimal ImportoImponibile { get; set; }
        public decimal ImportoIva { get; set; }
        public decimal ImportoTotale { get; set; }
    }
}