using Database.Models;
using DTO;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Repository.Interface;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace BBltZen.BackgroundServices
{
    public class CacheBackgroundService : BackgroundService
    {
        private readonly ILogger<CacheBackgroundService> _logger;
        private readonly IServiceProvider _serviceProvider;

        // ✅ METRICHE DI TRACKING
        private static int _executionCount = 0;
        private static DateTime _lastExecution = DateTime.MinValue;
        private static DateTime _serviceStartTime = DateTime.UtcNow;
        private static string? _lastError = null;
        private static int _errorCount = 0;

        public CacheBackgroundService(
            ILogger<CacheBackgroundService> logger,
            IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Cache Background Service avviato");
            _serviceStartTime = DateTime.UtcNow;

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await DoWorkAsync();
                    _executionCount++;
                    _lastExecution = DateTime.UtcNow;
                    _logger.LogInformation("✅ Background job #{ExecutionCount} completato", _executionCount);

                    await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    _errorCount++;
                    _lastError = ex.Message;
                    _logger.LogError(ex, "❌ Errore durante l'esecuzione del background service");
                    await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
                }
            }

            _logger.LogInformation("Cache Background Service fermato");
        }

        private async Task<StatisticheCarrelloDTO> CalcolaStatisticheCarrelloRealiAsync()
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<BubbleTeaContext>();

            var now = DateTime.UtcNow;
            var oggi = now.Date;

            // 📊 QUERY PER STATISTICHE FONDAMENTALI
            var totaleOrdini = await context.Ordine.CountAsync();
            var totaleProdottiVenduti = await context.OrderItem.SumAsync(oi => oi.Quantita);
            var fatturatoTotale = await context.Ordine.SumAsync(o => o.Totale);
            var valoreMedioOrdine = totaleOrdini > 0 ? fatturatoTotale / totaleOrdini : 0;
            var prodottiPerOrdineMedio = totaleOrdini > 0 ? (decimal)totaleProdottiVenduti / totaleOrdini : 0;

            // 🎯 QUERY PER TASSO CONVERSIONE
            var ordiniCompletati = await context.Ordine
                .CountAsync(o => o.StatoOrdineId == 4); // "consegnato"

            var carrelliAbbandonati = await context.Ordine
                .CountAsync(o => o.StatoOrdineId == 9); // "in_carrello"

            var tassoConversioneCarrello = ordiniCompletati + carrelliAbbandonati > 0
                ? (decimal)ordiniCompletati / (ordiniCompletati + carrelliAbbandonati) * 100
                : 0;

            // 📦 QUERY PER DISTRIBUZIONE PRODOTTI
            var distribuzionePerTipologia = await context.OrderItem
                .GroupBy(oi => oi.TipoArticolo)
                .Select(g => new
                {
                    TipoArticolo = g.Key,
                    TotaleVendite = g.Count(),
                    QuantitaTotale = g.Sum(x => x.Quantita),
                    RicavoTotale = g.Sum(x => x.Imponibile)
                })
                .ToListAsync();

            // Convertiamo dopo la query
            var distribuzioneDTO = distribuzionePerTipologia.Select(g => new DistribuzioneProdottoDTO
            {
                TipoArticolo = g.TipoArticolo,
                Descrizione = g.TipoArticolo == "BS" ? "Bevanda Standard" :
                              g.TipoArticolo == "BC" ? "Bevanda Custom" :
                              g.TipoArticolo == "D" ? "Dolce" : "Altro",
                TotaleVendite = g.TotaleVendite,
                QuantitaTotale = g.QuantitaTotale,
                RicavoTotale = g.RicavoTotale,
                PercentualeVendite = totaleOrdini > 0 ? (decimal)g.TotaleVendite / totaleOrdini * 100 : 0
            }).ToList();

            // 🏆 QUERY PER PRODOTTI PIÙ VENDUTI
            var prodottiPiuVenduti = await context.OrderItem
                .GroupBy(oi => new { oi.ArticoloId, oi.TipoArticolo })
                .Select(g => new ProdottoTopDTO
                {
                    ArticoloId = g.Key.ArticoloId,
                    TipoArticolo = g.Key.TipoArticolo,
                    NomeProdotto = g.Key.TipoArticolo + " - ID: " + g.Key.ArticoloId,
                    QuantitaVenduta = g.Sum(x => x.Quantita),
                    RicavoTotale = g.Sum(x => x.Imponibile)
                })
                .OrderByDescending(p => p.QuantitaVenduta)
                .Take(10)
                .ToListAsync();

            // ⏰ QUERY PER FASCIA ORARIA PIÙ ATTIVA
            var fasciaOrariaPiuAttiva = await context.Ordine
                .GroupBy(o => EF.Functions.DateDiffHour(oggi, o.DataCreazione))
                .Select(g => new { Ora = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .FirstOrDefaultAsync();

            var fasciaOraria = fasciaOrariaPiuAttiva != null
                ? $"{fasciaOrariaPiuAttiva.Ora:00}:00-{fasciaOrariaPiuAttiva.Ora + 1:00}:00"
                : "N/A";

            // 📈 STATISTICHE DI OGGI
            var ordiniOggi = await context.Ordine
                .CountAsync(o => o.DataCreazione.Date == oggi);

            var fatturatoOggi = await context.Ordine
                .Where(o => o.DataCreazione.Date == oggi)
                .SumAsync(o => o.Totale);

            return new StatisticheCarrelloDTO
            {
                TotaleOrdini = totaleOrdini,
                TotaleProdottiVenduti = totaleProdottiVenduti,
                FatturatoTotale = fatturatoTotale,
                ValoreMedioOrdine = valoreMedioOrdine,
                ProdottiPerOrdineMedio = prodottiPerOrdineMedio,

                TassoConversioneBozza = 0, // Da implementare se serve
                TassoConversioneCarrello = tassoConversioneCarrello,
                CarrelliAbbandonati = carrelliAbbandonati,

                DistribuzionePerTipologia = distribuzioneDTO,
                ProdottiPiuVenduti = prodottiPiuVenduti,

                FasciaOrariaPiuAttiva = fasciaOraria,
                OrdiniOggi = ordiniOggi,
                FatturatoOggi = fatturatoOggi,

                DataRiferimento = oggi,
                DataAggiornamento = now
            };
        }

        // Miglioriamo il DoWorkAsync per avere intervalli diversi
        private async Task DoWorkAsync()
        {
            using var scope = _serviceProvider.CreateScope();

            var sistemaCache = scope.ServiceProvider.GetRequiredService<ISistemaCacheRepository>();
            var statisticheCache = scope.ServiceProvider.GetRequiredService<IStatisticheCacheRepository>();

            var now = DateTime.UtcNow;

            // 🔄 OGNI 5 MINUTI - Cache memoria realtime
            await sistemaCache.RefreshStatisticheCarrelloAsync();
            _logger.LogInformation("🔄 Cache realtime aggiornata");

            // 📊 OGNI 30 MINUTI - Cache persistente (solo ai minuti 0, 30)
            if (now.Minute is 0 or 30)
            {
                try
                {
                    // Calcola statistiche carrello dal database
                    var statisticheReali = await CalcolaStatisticheCarrelloRealiAsync();

                    // Salva in cache persistente
                    await statisticheCache.SalvaStatisticheCarrelloAsync("Oggi", statisticheReali);

                    _logger.LogInformation("📊 Statistiche carrello calcolate e salvate");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "❌ Errore nel calcolo statistiche carrello");
                }
            }

            // 🗑️ OGNI ORA - Pulizia cache (solo al minuto 0)
            if (now.Minute == 0)
            {
                await sistemaCache.CleanupExpiredAsync();
                _logger.LogInformation("🗑️ Cache pulita");
            }
        }

        // ✅ METODI PER IL MONITORING (accessibili dal controller)
        public static int GetExecutionCount() => _executionCount;
        public static DateTime GetLastExecution() => _lastExecution;
        public static DateTime GetServiceStartTime() => _serviceStartTime;
        public static string? GetLastError() => _lastError;
        public static int GetErrorCount() => _errorCount;
        public static TimeSpan GetUptime() => DateTime.UtcNow - _serviceStartTime;
    }
}