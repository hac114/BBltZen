using System;
using System.Collections.Generic;

namespace DTO
{
    public class CacheStatisticheDTO
    {
        public int StatisticheCacheId { get; set; }
        public string TipoStatistica { get; set; } = string.Empty;
        public string DatiCache { get; set; } = string.Empty;
        public DateTime DataAggiornamento { get; set; }
        public DateTime ScadenzaCache { get; set; }
        public int DimensioneBytes { get; set; }
        public int Hits { get; set; }
        public int Misses { get; set; }
        public decimal HitRate { get; set; }
    }

    public class CacheConfigDTO
    {
        public string Chiave { get; set; } = string.Empty;
        public TimeSpan Durata { get; set; }
        public int Priorita { get; set; } = 1;
        public long DimensioneMassimaBytes { get; set; } = 1024 * 1024; // 1MB default
    }

    public class CacheEntryDTO
    {
        public string Chiave { get; set; } = string.Empty;
        public object Valore { get; set; } = new();
        public DateTime DataCreazione { get; set; }
        public DateTime Scadenza { get; set; }
        public TimeSpan Durata { get; set; }
        public int DimensioneBytes { get; set; }
        public int Hits { get; set; }
        public DateTime UltimoAccesso { get; set; }
        public string Tipo { get; set; } = string.Empty;
    }

    public class CacheInfoDTO
    {
        public int TotaleEntry { get; set; }
        public long DimensioneTotaleBytes { get; set; }
        public int EntryScadute { get; set; }
        public int HitsTotali { get; set; }
        public int MissesTotali { get; set; }
        public decimal HitRatePercentuale { get; set; }
        public DateTime UltimaPulizia { get; set; }
        public List<string> ChiaviAttive { get; set; } = new();
        public Dictionary<string, int> StatistichePerTipo { get; set; } = new();
    }

    public class CacheOperationResultDTO
    {
        public bool Successo { get; set; }
        public string Messaggio { get; set; } = string.Empty;
        public string Chiave { get; set; } = string.Empty;
        public DateTime? Scadenza { get; set; }
        public TimeSpan DurataCache { get; set; }
        public int DimensioneBytes { get; set; }
    }

    public class CacheBulkOperationDTO
    {
        public List<string> Chiavi { get; set; } = new();
        public string Operazione { get; set; } = string.Empty; // "GET", "REMOVE", "CLEAR"
        public Dictionary<string, object>? Valori { get; set; }
        public TimeSpan? Durata { get; set; }
    }

    public class CacheBulkResultDTO
    {
        public int OperazioniCompletate { get; set; }
        public int OperazioniFallite { get; set; }
        public List<string> ChiaviProcessate { get; set; } = new();
        public Dictionary<string, object> Risultati { get; set; } = new();
        public TimeSpan TempoEsecuzione { get; set; }
    }

    public class CacheCleanupDTO
    {
        public int EntryRimosse { get; set; }
        public long BytesLiberati { get; set; }
        public int EntryScadute { get; set; }
        public TimeSpan TempoEsecuzione { get; set; }
        public DateTime DataPulizia { get; set; }
    }

    public class CachePerformanceDTO
    {
        public decimal HitRate { get; set; }
        public decimal MissRate { get; set; }
        public decimal MemoriaUtilizzataPercentuale { get; set; }
        public long MemoriaTotaleBytes { get; set; }
        public long MemoriaLiberaBytes { get; set; }
        public int EntryAttive { get; set; }
        public TimeSpan TempoMedioAccesso { get; set; }
        public DateTime DataRaccolta { get; set; }
    }
}