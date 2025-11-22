using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTO
{
    public class PriceCalculationServiceDTO
    {
        [Range(0, 10000, ErrorMessage = "Il prezzo base deve essere tra 0 e 10000")]
        public decimal PrezzoBase { get; set; }

        [Range(0, 10000, ErrorMessage = "L'imponibile deve essere tra 0 e 10000")]
        public decimal Imponibile { get; set; }

        [Range(0, 2200, ErrorMessage = "L'IVA amount deve essere tra 0 e 2200")]
        public decimal IvaAmount { get; set; }

        [Range(0, 12000, ErrorMessage = "Il totale ivato deve essere tra 0 e 12000")]
        public decimal TotaleIvato { get; set; }

        public int TaxRateId { get; set; }

        [Range(0, 100, ErrorMessage = "Il tax rate deve essere tra 0 e 100")]
        public decimal TaxRate { get; set; }

        [StringLength(1000, ErrorMessage = "Il calcolo dettaglio non può superare 1000 caratteri")]
        public string? CalcoloDettaglio { get; set; }
    }

    // ✅ DTO PER LE RICHIESTE DI CALCOLO
    public class TaxCalculationRequestDTO
    {
        [Required(ErrorMessage = "L'imponibile è obbligatorio")]
        [Range(0.01, 100000, ErrorMessage = "L'imponibile deve essere tra 0.01 e 100000")]
        public decimal Imponibile { get; set; }

        [Required(ErrorMessage = "L'ID aliquota IVA è obbligatorio")]
        [Range(1, int.MaxValue, ErrorMessage = "L'ID aliquota IVA deve essere valido")] // ✅ MIGLIORATO RANGE
        public int TaxRateId { get; set; }
    }

    public class ImponibileCalculationRequestDTO
    {
        [Required(ErrorMessage = "Il prezzo è obbligatorio")]
        [Range(0.01, 10000, ErrorMessage = "Il prezzo deve essere tra 0.01 e 10000")]
        public decimal Prezzo { get; set; }

        [Required(ErrorMessage = "La quantità è obbligatoria")]
        [Range(1, 1000, ErrorMessage = "La quantità deve essere tra 1 e 1000")]
        public int Quantita { get; set; }

        [Required(ErrorMessage = "L'ID aliquota IVA è obbligatorio")]
        [Range(1, 10, ErrorMessage = "L'ID aliquota IVA deve essere valido")]
        public int TaxRateId { get; set; }
    }

    public class BatchCalculationRequestDTO
    {
        public List<int> BevandeStandardIds { get; set; } = new List<int>();
        public List<int> BevandeCustomIds { get; set; } = new List<int>();
        public List<int> DolciIds { get; set; } = new List<int>();
    }

    public class BatchCalculationResponseDTO
    {
        public Dictionary<int, decimal> BevandeStandardPrezzi { get; set; } = new Dictionary<int, decimal>();
        public Dictionary<int, decimal> BevandeCustomPrezzi { get; set; } = new Dictionary<int, decimal>();
        public Dictionary<int, decimal> DolciPrezzi { get; set; } = new Dictionary<int, decimal>();
        public List<string> Errori { get; set; } = new List<string>();
    }
}