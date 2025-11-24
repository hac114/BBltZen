namespace DTO.Monitoring
{
    public class CacheMetricsDTO
    {
        public DateTime Timestamp { get; set; }

        // 📈 EFFICIENZA CACHE
        public decimal HitRate { get; set; }
        public decimal MissRate { get; set; }
        public decimal HitRatePercentuale { get; set; }

        // 📊 UTILIZZO MEMORIA
        public long MemoriaUtilizzataBytes { get; set; }
        public string MemoriaUtilizzataFormattata { get; set; } = string.Empty;
        public int EntryAttive { get; set; }
        public int EntryScadute { get; set; }

        // ⚡ PERFORMANCE
        public int RequestsTotali { get; set; }
        public int RequestsUltimaOra { get; set; }
        public TimeSpan TempoMedioRisposta { get; set; }

        // 🔧 STATO SERVICE
        public string StatoBackgroundService { get; set; } = "Unknown";
        public DateTime UltimaEsecuzione { get; set; }
        public TimeSpan IntervalloEsecuzione { get; set; }

        // 📈 STATISTICHE CARRELLO
        public int StatisticheCarrelloInCache { get; set; }
        public DateTime? UltimoAggiornamentoStatistiche { get; set; }
    }

    public class BackgroundServiceStatusDTO
    {
        public string ServiceName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime LastExecution { get; set; }
        public TimeSpan Uptime { get; set; }
        public int ExecutionCount { get; set; }
        public int ErrorCount { get; set; }
        public string? LastError { get; set; }
    }
}