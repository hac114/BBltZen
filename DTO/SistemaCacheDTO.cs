using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DTO
{
    public class CacheStatisticheDTO
    {
        public int StatisticheCacheId { get; set; }

        [StringLength(50)]
        public string TipoStatistica { get; set; } = string.Empty;

        [StringLength(4000)]
        public string DatiCache { get; set; } = string.Empty;

        public DateTime DataAggiornamento { get; set; }
        public DateTime ScadenzaCache { get; set; }

        [Range(0, long.MaxValue)]
        public int DimensioneBytes { get; set; }

        [Range(0, int.MaxValue)]
        public int Hits { get; set; }

        [Range(0, int.MaxValue)]
        public int Misses { get; set; }

        [Range(0, 100)]
        public decimal HitRate { get; set; }
    }

    public class CacheConfigDTO
    {
        [StringLength(200)]
        public string Chiave { get; set; } = string.Empty;

        public TimeSpan Durata { get; set; }

        [Range(1, 10)]
        public int Priorita { get; set; } = 1;

        [Range(1024, long.MaxValue)]
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
}