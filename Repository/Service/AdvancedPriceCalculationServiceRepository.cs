using BBltZen;
using DTO;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Repository.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Repository.Service
{
    public class AdvancedPriceCalculationServiceRepository : IAdvancedPriceCalculationServiceRepository
    {
        private readonly BubbleTeaContext _context;
        private readonly IMemoryCache _memoryCache;
        private readonly ILogger<AdvancedPriceCalculationServiceRepository> _logger;
        private readonly IPriceCalculationServiceRepository _basicPriceService;

        // Cache keys
        private const string TAX_RATES_CACHE_KEY = "TaxRates";
        private const string DIMENSIONI_CACHE_KEY = "DimensioniBicchieri";
        private const string INGREDIENTI_CACHE_KEY = "Ingredienti";

        public AdvancedPriceCalculationServiceRepository(
            BubbleTeaContext context,
            IMemoryCache memoryCache,
            ILogger<AdvancedPriceCalculationServiceRepository> logger,
            IPriceCalculationServiceRepository basicPriceService)
        {
            _context = context;
            _memoryCache = memoryCache;
            _logger = logger;
            _basicPriceService = basicPriceService;
        }

        public async Task<decimal> CalculateBevandaStandardPriceAsync(int articoloId)
        {
            // Delegato al servizio base esistente
            return await _basicPriceService.CalculateBevandaStandardPrice(articoloId);
        }

        public async Task<decimal> CalculateBevandaCustomPriceAsync(int personalizzazioneCustomId)
        {
            try
            {
                // Carica i dati necessari
                var personalizzazione = await _context.PersonalizzazioneCustom
                    .FirstOrDefaultAsync(pc => pc.PersCustomId == personalizzazioneCustomId);

                if (personalizzazione == null)
                    throw new ArgumentException($"PersonalizzazioneCustom con ID {personalizzazioneCustomId} non trovata");

                var dimensione = await _context.DimensioneBicchiere
                    .FirstOrDefaultAsync(db => db.DimensioneBicchiereId == personalizzazione.DimensioneBicchiereId);

                if (dimensione == null)
                    throw new ArgumentException($"Dimensione bicchiere non trovata per personalizzazione {personalizzazioneCustomId}");

                // Carica ingredienti
                var ingredientiPersonalizzazione = await _context.IngredientiPersonalizzazione
                    .Where(ip => ip.PersCustomId == personalizzazioneCustomId)
                    .ToListAsync();

                var ingredientiIds = ingredientiPersonalizzazione.Select(ip => ip.IngredienteId).ToList();
                var ingredienti = await _context.Ingrediente
                    .Where(i => ingredientiIds.Contains(i.IngredienteId) && i.Disponibile)
                    .ToDictionaryAsync(i => i.IngredienteId);

                // Calcola prezzo base + ingredienti
                decimal prezzoIngredienti = 0;

                foreach (var ip in ingredientiPersonalizzazione)
                {
                    if (ingredienti.TryGetValue(ip.IngredienteId, out var ingrediente))
                    {
                        prezzoIngredienti += ingrediente.PrezzoAggiunto * dimensione.Moltiplicatore;
                    }
                }

                decimal prezzoTotale = dimensione.PrezzoBase + prezzoIngredienti;

                _logger.LogInformation($"Calcolato prezzo bevanda custom {personalizzazioneCustomId}: {prezzoTotale}");
                return Math.Round(prezzoTotale, 2);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Errore nel calcolo prezzo bevanda custom {personalizzazioneCustomId}");
                throw;
            }
        }

        public async Task<decimal> CalculateDolcePriceAsync(int articoloId)
        {
            // Delegato al servizio base esistente
            return await _basicPriceService.CalculateDolcePrice(articoloId);
        }

        public async Task<decimal> CalculateTaxAmountAsync(decimal importo, int taxRateId)
        {
            var aliquota = await GetTaxRateAsync(taxRateId);
            var imponibile = importo / (1 + (aliquota / 100));
            var iva = importo - imponibile;

            return Math.Round(iva, 2);
        }

        public async Task<decimal> CalculateImponibileAsync(decimal importoIvato, int taxRateId)
        {
            var aliquota = await GetTaxRateAsync(taxRateId);
            var imponibile = importoIvato / (1 + (aliquota / 100));

            return Math.Round(imponibile, 2);
        }

        public async Task<decimal> GetTaxRateAsync(int taxRateId)
        {
            try
            {
                if (taxRateId <= 0)
                    return 22.00m;

                // ✅ CORREZIONE: CHIAVE CACHE UNICA PER QUESTO REPOSITORY
                const string ADVANCED_TAX_RATES_CACHE_KEY = "AdvancedTaxRates";

                if (!_memoryCache.TryGetValue(ADVANCED_TAX_RATES_CACHE_KEY, out Dictionary<int, decimal>? taxRates) || taxRates == null)
                {
                    taxRates = await _context.TaxRates
                        .ToDictionaryAsync(tr => tr.TaxRateId, tr => tr.Aliquota);

                    _memoryCache.Set(ADVANCED_TAX_RATES_CACHE_KEY, taxRates, TimeSpan.FromMinutes(30));
                }

                return taxRates.TryGetValue(taxRateId, out var aliquota) ? aliquota : 22.00m;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Errore nel recupero tax rate {TaxRateId}, usando default", taxRateId);
                return 22.00m;
            }
        }

        public async Task<PriceCalculationResultDTO> CalculateCompletePriceAsync(PriceCalculationRequestDTO request)
        {
            try
            {
                // ✅ VALIDAZIONE INPUT CRITICA
                if (request == null)
                    throw new ArgumentNullException(nameof(request));

                if (request.ArticoloId <= 0)
                    throw new ArgumentException("ID articolo non valido", nameof(request.ArticoloId));

                if (string.IsNullOrWhiteSpace(request.TipoArticolo))
                    throw new ArgumentException("Tipo articolo obbligatorio", nameof(request.TipoArticolo));

                if (request.Quantita <= 0)
                    throw new ArgumentException("Quantità deve essere maggiore di zero", nameof(request.Quantita));

                decimal prezzoBase = 0;

                // Calcola prezzo base in base al tipo articolo
                switch (request.TipoArticolo.ToUpper())
                {
                    case "BS":
                        prezzoBase = await CalculateBevandaStandardPriceAsync(request.ArticoloId);
                        break;
                    case "BC":
                        if (!request.PersonalizzazioneCustomId.HasValue)
                            throw new ArgumentException("PersonalizzazioneCustomId richiesto per bevande custom");
                        prezzoBase = await CalculateBevandaCustomPriceAsync(request.PersonalizzazioneCustomId.Value);
                        break;
                    case "D":
                        prezzoBase = await CalculateDolcePriceAsync(request.ArticoloId);
                        break;
                    default:
                        throw new ArgumentException($"Tipo articolo non supportato: {request.TipoArticolo}");
                }

                // Usa prezzo fisso se specificato
                if (request.PrezzoFisso.HasValue)
                {
                    prezzoBase = request.PrezzoFisso.Value;
                }

                // Calcoli fiscali
                var aliquotaIva = await GetTaxRateAsync(request.TaxRateId);
                var prezzoUnitario = prezzoBase;
                var totaleIvato = prezzoUnitario * request.Quantita;
                var imponibile = await CalculateImponibileAsync(totaleIvato, request.TaxRateId);
                var ivaAmount = await CalculateTaxAmountAsync(totaleIvato, request.TaxRateId);

                return new PriceCalculationResultDTO
                {
                    ArticoloId = request.ArticoloId,
                    TipoArticolo = request.TipoArticolo,
                    PrezzoBase = Math.Round(prezzoBase, 2),
                    PrezzoUnitario = Math.Round(prezzoUnitario, 2),
                    Imponibile = Math.Round(imponibile, 2),
                    IvaAmount = Math.Round(ivaAmount, 2),
                    TotaleIvato = Math.Round(totaleIvato, 2),
                    AliquotaIva = aliquotaIva,
                    Quantita = request.Quantita,
                    DataCalcolo = DateTime.UtcNow // ✅ UTC
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore nel calcolo completo prezzo per articolo {ArticoloId}", request?.ArticoloId);
                throw;
            }
        }

        public async Task<CustomBeverageCalculationDTO> CalculateDetailedCustomBeveragePriceAsync(int personalizzazioneCustomId)
        {
            var personalizzazione = await _context.PersonalizzazioneCustom
                .FirstOrDefaultAsync(pc => pc.PersCustomId == personalizzazioneCustomId);

            if (personalizzazione == null)
                throw new ArgumentException($"PersonalizzazioneCustom non trovata: {personalizzazioneCustomId}");

            var dimensione = await _context.DimensioneBicchiere
                .FirstOrDefaultAsync(db => db.DimensioneBicchiereId == personalizzazione.DimensioneBicchiereId);

            if (dimensione == null)
                throw new ArgumentException($"Dimensione bicchiere non trovata");

            // Carica ingredienti con dettagli
            var ingredientiPersonalizzazione = await _context.IngredientiPersonalizzazione
                .Where(ip => ip.PersCustomId == personalizzazioneCustomId)
                .ToListAsync();

            var ingredientiIds = ingredientiPersonalizzazione.Select(ip => ip.IngredienteId).ToList();
            var ingredienti = await _context.Ingrediente
                .Where(i => ingredientiIds.Contains(i.IngredienteId))
                .ToDictionaryAsync(i => i.IngredienteId);

            // Calcola dettagli ingredienti
            var ingredientiCalcolo = new List<IngredienteCalcoloDTO>();
            decimal prezzoIngredienti = 0;

            foreach (var ip in ingredientiPersonalizzazione)
            {
                if (ingredienti.TryGetValue(ip.IngredienteId, out var ingrediente))
                {
                    var prezzoCalcolato = ingrediente.PrezzoAggiunto * dimensione.Moltiplicatore;
                    prezzoIngredienti += prezzoCalcolato;

                    ingredientiCalcolo.Add(new IngredienteCalcoloDTO
                    {
                        IngredienteId = ingrediente.IngredienteId,
                        NomeIngrediente = ingrediente.Ingrediente1,
                        PrezzoAggiunto = ingrediente.PrezzoAggiunto,
                        Quantita = 1, // Quantità fissa poiché non è presente nel database
                        UnitaMisura = "porzione", // Poiché non c'è quantità, usiamo "porzione"
                        PrezzoCalcolato = prezzoCalcolato
                    });
                }
            }

            decimal prezzoTotale = dimensione.PrezzoBase + prezzoIngredienti;

            return new CustomBeverageCalculationDTO
            {
                PersonalizzazioneCustomId = personalizzazioneCustomId,
                NomePersonalizzazione = personalizzazione.Nome ?? "Personalizzazione",
                DimensioneBicchiereId = dimensione.DimensioneBicchiereId,
                PrezzoBaseDimensione = dimensione.PrezzoBase,
                MoltiplicatoreDimensione = dimensione.Moltiplicatore,
                Ingredienti = ingredientiCalcolo,
                PrezzoIngredienti = prezzoIngredienti,
                PrezzoTotale = Math.Round(prezzoTotale, 2)
            };
        }

        public async Task<OrderCalculationSummaryDTO> CalculateCompleteOrderAsync(int ordineId)
        {
            var ordine = await _context.Ordine
                .FirstOrDefaultAsync(o => o.OrdineId == ordineId);

            if (ordine == null)
                throw new ArgumentException($"Ordine non trovato: {ordineId}");

            var orderItems = await _context.OrderItem
                .Where(oi => oi.OrdineId == ordineId)
                .ToListAsync();

            var itemsCalcolo = new List<OrderItemCalculationDTO>();
            decimal totaleImponibile = 0;
            decimal totaleIva = 0;
            decimal totaleOrdine = 0;

            foreach (var item in orderItems)
            {
                var request = new PriceCalculationRequestDTO
                {
                    ArticoloId = item.ArticoloId,
                    TipoArticolo = item.TipoArticolo,
                    Quantita = item.Quantita,
                    TaxRateId = item.TaxRateId,
                    PrezzoFisso = item.PrezzoUnitario > 0 ? item.PrezzoUnitario : (decimal?)null
                };

                var calcolo = await CalculateCompletePriceAsync(request);

                itemsCalcolo.Add(new OrderItemCalculationDTO
                {
                    OrderItemId = item.OrderItemId,
                    ArticoloId = item.ArticoloId,
                    TipoArticolo = item.TipoArticolo,
                    Quantita = item.Quantita,
                    PrezzoUnitario = calcolo.PrezzoUnitario,
                    Imponibile = calcolo.Imponibile,
                    IvaAmount = calcolo.IvaAmount,
                    TotaleIvato = calcolo.TotaleIvato,
                    AliquotaIva = calcolo.AliquotaIva
                });

                totaleImponibile += calcolo.Imponibile;
                totaleIva += calcolo.IvaAmount;
                totaleOrdine += calcolo.TotaleIvato;
            }

            return new OrderCalculationSummaryDTO
            {
                OrdineId = ordineId,
                TotaleImponibile = Math.Round(totaleImponibile, 2),
                TotaleIva = Math.Round(totaleIva, 2),
                TotaleOrdine = Math.Round(totaleOrdine, 2),
                Items = itemsCalcolo
            };
        }

        public async Task<List<PriceCalculationResultDTO>> CalculateBatchPricesAsync(List<PriceCalculationRequestDTO> requests)
        {
            var results = new List<PriceCalculationResultDTO>();

            if (requests == null || !requests.Any())
                return results;

            // ✅ CALCOLO PARALLELO SICURO
            var tasks = requests.Select(async request =>
            {
                try
                {
                    var result = await CalculateCompletePriceAsync(request);
                    return (Success: true, Result: result, Error: (string?)null);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Errore nel calcolo batch per articolo {ArticoloId}", request.ArticoloId);
                    return (Success: false, Result: (PriceCalculationResultDTO?)null, Error: ex.Message);
                }
            });

            var taskResults = await Task.WhenAll(tasks);

            foreach (var taskResult in taskResults)
            {
                if (taskResult.Success && taskResult.Result != null)
                {
                    results.Add(taskResult.Result);
                }
            }

            return results;
        }

        public async Task<Dictionary<int, decimal>> CalculateOrderItemsTotalAsync(List<int> orderItemIds)
        {
            var results = new Dictionary<int, decimal>();

            foreach (var orderItemId in orderItemIds)
            {
                var orderItem = await _context.OrderItem
                    .FirstOrDefaultAsync(oi => oi.OrderItemId == orderItemId);

                if (orderItem != null)
                {
                    var request = new PriceCalculationRequestDTO
                    {
                        ArticoloId = orderItem.ArticoloId,
                        TipoArticolo = orderItem.TipoArticolo,
                        Quantita = orderItem.Quantita,
                        TaxRateId = orderItem.TaxRateId,
                        PrezzoFisso = orderItem.PrezzoUnitario > 0 ? orderItem.PrezzoUnitario : (decimal?)null
                    };

                    var calcolo = await CalculateCompletePriceAsync(request);
                    results[orderItemId] = calcolo.TotaleIvato;
                }
            }

            return results;
        }

        public async Task<bool> ValidatePriceCalculationAsync(int articoloId, string tipoArticolo, decimal prezzoCalcolato)
        {
            try
            {
                decimal prezzoAtteso = 0;

                switch (tipoArticolo.ToUpper())
                {
                    case "BS":
                        prezzoAtteso = await CalculateBevandaStandardPriceAsync(articoloId);
                        break;
                    case "D":
                        prezzoAtteso = await CalculateDolcePriceAsync(articoloId);
                        break;
                    default:
                        return true; // Per BC la validazione è più complessa
                }

                // Tolleranza del 5% per arrotondamenti
                var tolleranza = prezzoAtteso * 0.05m;
                return Math.Abs(prezzoCalcolato - prezzoAtteso) <= tolleranza;
            }
            catch
            {
                return false;
            }
        }

        public async Task<decimal> ApplyDiscountAsync(decimal prezzo, decimal percentualeSconto)
        {
            if (percentualeSconto < 0 || percentualeSconto > 100)
                throw new ArgumentException("Percentuale sconto deve essere tra 0 e 100");

            var sconto = prezzo * (percentualeSconto / 100);
            return await Task.FromResult(Math.Round(prezzo - sconto, 2)); // ✅ AWAIT CON TASK.FROMRESULT
        }

        public async Task<decimal> CalculateShippingCostAsync(decimal subtotal, string metodoSpedizione)
        {
            var costo = metodoSpedizione?.ToLower() switch
            {
                "express" => 5.00m,
                "priority" => 3.50m,
                "standard" => 2.00m,
                _ => 2.50m
            };

            return await Task.FromResult(costo); // ✅ AWAIT CON TASK.FROMRESULT
        }

        public async Task PreloadCalculationCacheAsync()
        {
            try
            {
                // ✅ CHIAVI CACHE UNICHE PER QUESTO REPOSITORY
                const string ADVANCED_TAX_RATES_CACHE_KEY = "AdvancedTaxRates";
                const string ADVANCED_DIMENSIONI_CACHE_KEY = "AdvancedDimensioniBicchieri";
                const string ADVANCED_INGREDIENTI_CACHE_KEY = "AdvancedIngredienti";

                // Precarica dati frequentemente usati
                var taxRates = await _context.TaxRates
                    .ToDictionaryAsync(tr => tr.TaxRateId, tr => tr.Aliquota);
                _memoryCache.Set(ADVANCED_TAX_RATES_CACHE_KEY, taxRates, TimeSpan.FromMinutes(30));

                var dimensioni = await _context.DimensioneBicchiere
                    .ToDictionaryAsync(db => db.DimensioneBicchiereId);
                _memoryCache.Set(ADVANCED_DIMENSIONI_CACHE_KEY, dimensioni, TimeSpan.FromMinutes(60));

                var ingredienti = await _context.Ingrediente
                    .Where(i => i.Disponibile)
                    .ToDictionaryAsync(i => i.IngredienteId);
                _memoryCache.Set(ADVANCED_INGREDIENTI_CACHE_KEY, ingredienti, TimeSpan.FromMinutes(60));

                _logger.LogInformation("Cache calcoli avanzati precaricata con successo");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore nel precaricamento cache calcoli avanzati");
            }
        }

        public async Task ClearCalculationCacheAsync()
        {
            _memoryCache.Remove(TAX_RATES_CACHE_KEY);
            _memoryCache.Remove(DIMENSIONI_CACHE_KEY);
            _memoryCache.Remove(INGREDIENTI_CACHE_KEY);

            _logger.LogInformation("Cache calcoli prezzi pulita");

            await Task.CompletedTask;
        }

        public async Task<bool> IsCacheValidAsync()
        {
            return await Task.FromResult(
                _memoryCache.TryGetValue(TAX_RATES_CACHE_KEY, out _) &&
                _memoryCache.TryGetValue(DIMENSIONI_CACHE_KEY, out _)
            );
        }
    }
}