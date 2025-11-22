using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DTO
{
    public class PriceCalculationRequestDTO
    {
        public int ArticoloId { get; set; }

        [Required(ErrorMessage = "Il tipo articolo è obbligatorio")]
        [StringLength(10, ErrorMessage = "Il tipo articolo non può superare 10 caratteri")]
        public required string TipoArticolo { get; set; }

        public int? PersonalizzazioneCustomId { get; set; }

        [Range(1, 100, ErrorMessage = "La quantità deve essere tra 1 e 100")]
        public int Quantita { get; set; } = 1;

        [Range(1, int.MaxValue, ErrorMessage = "L'ID aliquota IVA deve essere valido")]
        public int TaxRateId { get; set; } = 1;

        [Range(0.01, 1000, ErrorMessage = "Il prezzo fisso deve essere tra 0.01 e 1000")]
        public decimal? PrezzoFisso { get; set; }
    }

    public class PriceCalculationResultDTO
    {
        public int ArticoloId { get; set; }

        [StringLength(10)]
        public required string TipoArticolo { get; set; }

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

        public DateTime DataCalcolo { get; set; } = DateTime.UtcNow;
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

        [StringLength(10)]
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

        public DateTime DataCalcolo { get; set; } = DateTime.UtcNow;
    }

    public class OrderItemCalculationDTO
    {
        public int OrderItemId { get; set; }
        public int ArticoloId { get; set; }

        [StringLength(10)]
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

    // ✅ DTO PER IL CONTROLLER
    public class DiscountRequestDTO
    {
        [Required(ErrorMessage = "Il prezzo è obbligatorio")]
        [Range(0.01, 10000, ErrorMessage = "Il prezzo deve essere tra 0.01 e 10000")]
        public decimal Prezzo { get; set; }

        [Required(ErrorMessage = "La percentuale sconto è obbligatoria")]
        [Range(0, 100, ErrorMessage = "La percentuale sconto deve essere tra 0 e 100")]
        public decimal PercentualeSconto { get; set; }
    }

    public class ValidationRequestDTO
    {
        [Required(ErrorMessage = "L'ID articolo è obbligatorio")]
        public int ArticoloId { get; set; }

        [Required(ErrorMessage = "Il tipo articolo è obbligatorio")]
        [StringLength(10, ErrorMessage = "Il tipo articolo non può superare 10 caratteri")]
        public string TipoArticolo { get; set; } = string.Empty;

        [Required(ErrorMessage = "Il prezzo calcolato è obbligatorio")]
        [Range(0.01, 10000, ErrorMessage = "Il prezzo calcolato deve essere tra 0.01 e 10000")]
        public decimal PrezzoCalcolato { get; set; }
    }

    public class ShippingCostRequestDTO
    {
        [Required(ErrorMessage = "Il subtotale è obbligatorio")]
        [Range(0.01, 100000, ErrorMessage = "Il subtotale deve essere tra 0.01 e 100000")]
        public decimal Subtotal { get; set; }

        [Required(ErrorMessage = "Il metodo di spedizione è obbligatorio")]
        [StringLength(20, ErrorMessage = "Il metodo di spedizione non può superare 20 caratteri")]
        public string MetodoSpedizione { get; set; } = string.Empty;
    }

    public class AdvancedBatchCalculationRequestDTO
    {
        public List<int> BevandeStandardIds { get; set; } = new List<int>();
        public List<int> BevandeCustomIds { get; set; } = new List<int>();
        public List<int> DolciIds { get; set; } = new List<int>();
    }

    public class AdvancedBatchCalculationResponseDTO
    {
        public Dictionary<int, decimal> BevandeStandardPrezzi { get; set; } = new Dictionary<int, decimal>();
        public Dictionary<int, decimal> BevandeCustomPrezzi { get; set; } = new Dictionary<int, decimal>();
        public Dictionary<int, decimal> DolciPrezzi { get; set; } = new Dictionary<int, decimal>();
        public List<string> Errori { get; set; } = new List<string>();
    }
}