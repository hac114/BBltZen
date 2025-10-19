using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DTO
{
    public class PriceCalculationRequestDTO
    {
        public int ArticoloId { get; set; }

        [StringLength(10)]
        public string TipoArticolo { get; set; } = string.Empty;

        public int? PersonalizzazioneCustomId { get; set; }

        [Range(1, 100)]
        public int Quantita { get; set; } = 1;

        public int TaxRateId { get; set; } = 1;

        [Range(0.01, 1000)]
        public decimal? PrezzoFisso { get; set; }
    }

    public class PriceCalculationResultDTO
    {
        public int ArticoloId { get; set; }

        [StringLength(10)]
        public string TipoArticolo { get; set; } = string.Empty;

        [Range(0, 1000)]
        public decimal PrezzoBase { get; set; }

        [Range(0.01, 1000)]
        public decimal PrezzoUnitario { get; set; }

        [Range(0.01, 10000)]
        public decimal Imponibile { get; set; }

        [Range(0, 5000)]
        public decimal IvaAmount { get; set; }

        [Range(0.01, 15000)]
        public decimal TotaleIvato { get; set; }

        [Range(0, 100)]
        public decimal AliquotaIva { get; set; }

        [Range(1, 100)]
        public int Quantita { get; set; }

        [Range(0, 100)]
        public decimal? ScontoApplicato { get; set; }

        public DateTime DataCalcolo { get; set; } = DateTime.Now;
    }

    public class CustomBeverageCalculationDTO
    {
        public int PersonalizzazioneCustomId { get; set; }

        [StringLength(100)]
        public string NomePersonalizzazione { get; set; } = string.Empty;

        public int DimensioneBicchiereId { get; set; }

        [Range(0.01, 100)]
        public decimal PrezzoBaseDimensione { get; set; }

        [Range(0.1, 10)]
        public decimal MoltiplicatoreDimensione { get; set; }

        public List<IngredienteCalcoloDTO> Ingredienti { get; set; } = new();

        [Range(0.01, 1000)]
        public decimal PrezzoTotale { get; set; }

        [Range(0, 500)]
        public decimal PrezzoIngredienti { get; set; }
    }

    public class IngredienteCalcoloDTO
    {
        public int IngredienteId { get; set; }

        [StringLength(100)]
        public string NomeIngrediente { get; set; } = string.Empty;

        [Range(0, 20)]
        public decimal PrezzoAggiunto { get; set; }

        [Range(0.01, 1000)]
        public decimal Quantita { get; set; }

        [StringLength(2)]
        public string UnitaMisura { get; set; } = string.Empty;

        [Range(0, 200)]
        public decimal PrezzoCalcolato { get; set; }
    }

    public class OrderCalculationSummaryDTO
    {
        public int OrdineId { get; set; }

        [Range(0.01, 100000)]
        public decimal TotaleImponibile { get; set; }

        [Range(0, 5000)]
        public decimal TotaleIva { get; set; }

        [Range(0.01, 150000)]
        public decimal TotaleOrdine { get; set; }

        public List<OrderItemCalculationDTO> Items { get; set; } = new();

        public DateTime DataCalcolo { get; set; } = DateTime.Now;
    }

    public class OrderItemCalculationDTO
    {
        public int OrderItemId { get; set; }
        public int ArticoloId { get; set; }

        [StringLength(2)]
        public string TipoArticolo { get; set; } = string.Empty;

        [Range(1, 100)]
        public int Quantita { get; set; }

        [Range(0.01, 1000)]
        public decimal PrezzoUnitario { get; set; }

        [Range(0.01, 10000)]
        public decimal Imponibile { get; set; }

        [Range(0, 5000)]
        public decimal IvaAmount { get; set; }

        [Range(0.01, 15000)]
        public decimal TotaleIvato { get; set; }

        [Range(0, 100)]
        public decimal AliquotaIva { get; set; }
    }

    public class TaxCalculationDTO
    {
        public int TaxRateId { get; set; }

        [Range(0, 100)]
        public decimal Aliquota { get; set; }

        [StringLength(100)]
        public string Descrizione { get; set; } = string.Empty;

        [Range(0.01, 10000)]
        public decimal ImportoImponibile { get; set; }

        [Range(0, 5000)]
        public decimal ImportoIva { get; set; }

        [Range(0.01, 15000)]
        public decimal ImportoTotale { get; set; }
    }
}