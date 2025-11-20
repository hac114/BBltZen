using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DTO
{
    public class CacheStatisticheDTO
    {
        public int StatisticheCacheId { get; set; }

        [Required(ErrorMessage = "Il tipo statistica è obbligatorio")]
        [StringLength(50, ErrorMessage = "Il tipo statistica non può superare 50 caratteri")]
        public string TipoStatistica { get; set; } = string.Empty;

        [Required(ErrorMessage = "I dati cache sono obbligatori")]
        [StringLength(4000, ErrorMessage = "I dati cache non possono superare 4000 caratteri")]
        public string DatiCache { get; set; } = string.Empty;

        public DateTime DataAggiornamento { get; set; }
        public DateTime ScadenzaCache { get; set; }

        [Range(0, long.MaxValue, ErrorMessage = "La dimensione deve essere positiva")]
        public int DimensioneBytes { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Gli hits devono essere positivi")]
        public int Hits { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "I misses devono essere positivi")]
        public int Misses { get; set; }

        [Range(0, 100, ErrorMessage = "L'hit rate deve essere tra 0 e 100")]
        public decimal HitRate { get; set; }
    }

    public class CacheConfigDTO
    {
        [Required(ErrorMessage = "La chiave è obbligatoria")]
        [StringLength(200, ErrorMessage = "La chiave non può superare 200 caratteri")]
        public string Chiave { get; set; } = string.Empty;

        [Required(ErrorMessage = "La durata è obbligatoria")]
        public TimeSpan Durata { get; set; }

        [Range(1, 10, ErrorMessage = "La priorità deve essere tra 1 e 10")]
        public int Priorita { get; set; } = 1;

        [Range(1024, long.MaxValue, ErrorMessage = "La dimensione massima deve essere almeno 1KB")]
        public long DimensioneMassimaBytes { get; set; } = 1024 * 1024;
    }

    public class CacheEntryDTO
    {
        [StringLength(200)]
        public string Chiave { get; set; } = string.Empty;

        public object Valore { get; set; } = new();
        public DateTime DataCreazione { get; set; }
        public DateTime Scadenza { get; set; }
        public TimeSpan Durata { get; set; }

        [Range(0, long.MaxValue)]
        public int DimensioneBytes { get; set; }

        [Range(0, int.MaxValue)]
        public int Hits { get; set; }

        public DateTime UltimoAccesso { get; set; }

        [StringLength(50)]
        public string Tipo { get; set; } = string.Empty;
    }

    public class CacheInfoDTO
    {
        [Range(0, int.MaxValue)]
        public int TotaleEntry { get; set; }

        [Range(0, long.MaxValue)]
        public long DimensioneTotaleBytes { get; set; }

        [Range(0, int.MaxValue)]
        public int EntryScadute { get; set; }

        [Range(0, int.MaxValue)]
        public int HitsTotali { get; set; }

        [Range(0, int.MaxValue)]
        public int MissesTotali { get; set; }

        [Range(0, 100)]
        public decimal HitRatePercentuale { get; set; }

        public DateTime UltimaPulizia { get; set; }

        public List<string> ChiaviAttive { get; set; } = new();
        public Dictionary<string, int> StatistichePerTipo { get; set; } = new();
    }

    public class CacheOperationResultDTO
    {
        public bool Successo { get; set; }

        [StringLength(500)]
        public string Messaggio { get; set; } = string.Empty;

        [StringLength(200)]
        public string Chiave { get; set; } = string.Empty;

        public DateTime? Scadenza { get; set; }
        public TimeSpan DurataCache { get; set; }

        [Range(0, long.MaxValue)]
        public int DimensioneBytes { get; set; }
    }

    public class CacheBulkOperationDTO
    {
        public List<string> Chiavi { get; set; } = new();

        [StringLength(20)]
        public string Operazione { get; set; } = string.Empty;

        public Dictionary<string, object>? Valori { get; set; }
        public TimeSpan? Durata { get; set; }
    }

    public class CacheBulkResultDTO
    {
        [Range(0, int.MaxValue)]
        public int OperazioniCompletate { get; set; }

        [Range(0, int.MaxValue)]
        public int OperazioniFallite { get; set; }

        public List<string> ChiaviProcessate { get; set; } = new();
        public Dictionary<string, object> Risultati { get; set; } = new();
        public TimeSpan TempoEsecuzione { get; set; }
    }

    public class CacheCleanupDTO
    {
        [Range(0, int.MaxValue)]
        public int EntryRimosse { get; set; }

        [Range(0, long.MaxValue)]
        public long BytesLiberati { get; set; }

        [Range(0, int.MaxValue)]
        public int EntryScadute { get; set; }

        public TimeSpan TempoEsecuzione { get; set; }
        public DateTime DataPulizia { get; set; }
    }

    public class CachePerformanceDTO
    {
        [Range(0, 100)]
        public decimal HitRate { get; set; }

        [Range(0, 100)]
        public decimal MissRate { get; set; }

        [Range(0, 100)]
        public decimal MemoriaUtilizzataPercentuale { get; set; }

        [Range(0, long.MaxValue)]
        public long MemoriaTotaleBytes { get; set; }

        [Range(0, long.MaxValue)]
        public long MemoriaLiberaBytes { get; set; }

        [Range(0, int.MaxValue)]
        public int EntryAttive { get; set; }

        public TimeSpan TempoMedioAccesso { get; set; }
        public DateTime DataRaccolta { get; set; }
    }

    // ✅ DTO AGGIUNTI DAL CONTROLLER
    public class CacheSetRequest
    {
        public object Valore { get; set; } = new();
        public TimeSpan? Durata { get; set; }
    }

    public class BulkSetRequest
    {
        public Dictionary<string, object> Valori { get; set; } = new();
        public TimeSpan? Durata { get; set; }
    }
}