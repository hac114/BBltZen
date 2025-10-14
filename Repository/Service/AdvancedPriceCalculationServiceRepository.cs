using Database;
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
            // Usa cache per performance
            if (!_memoryCache.TryGetValue(TAX_RATES_CACHE_KEY, out Dictionary<int, decimal> taxRates))
            {
                taxRates = await _context.TaxRates
                    .ToDictionaryAsync(tr => tr.TaxRateId, tr => tr.Aliquota);

                _memoryCache.Set(TAX_RATES_CACHE_KEY, taxRates, TimeSpan.FromMinutes(30));
            }

            return taxRates.TryGetValue(taxRateId, out var aliquota) ? aliquota : 22.00m; // Default IVA standard
        }

        public async Task<PriceCalculationResultDTO> CalculateCompletePriceAsync(PriceCalculationRequestDTO request)
        {
            try
            {
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
                    PrezzoBase = prezzoBase,
                    PrezzoUnitario = prezzoUnitario,
                    Imponibile = imponibile,
                    IvaAmount = ivaAmount,
                    TotaleIvato = totaleIvato,
                    AliquotaIva = aliquotaIva,
                    Quantita = request.Quantita
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Errore nel calcolo completo prezzo per articolo {request.ArticoloId}");
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

            foreach (var request in requests)
            {
                try
                {
                    var result = await CalculateCompletePriceAsync(request);
                    results.Add(result);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Errore nel calcolo batch per articolo {request.ArticoloId}");
                    // Continua con gli altri calcoli
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
            return Math.Round(prezzo - sconto, 2);
        }

        public async Task<decimal> CalculateShippingCostAsync(decimal subtotal, string metodoSpedizione)
        {
            // Logica semplificata per costi spedizione
            return metodoSpedizione?.ToLower() switch
            {
                "express" => 5.00m,
                "priority" => 3.50m,
                "standard" => 2.00m,
                _ => 2.50m // Default
            };
        }

        public async Task PreloadCalculationCacheAsync()
        {
            try
            {
                // Precarica dati frequentemente usati
                var taxRates = await _context.TaxRates
                    .ToDictionaryAsync(tr => tr.TaxRateId, tr => tr.Aliquota);
                _memoryCache.Set(TAX_RATES_CACHE_KEY, taxRates, TimeSpan.FromMinutes(30));

                var dimensioni = await _context.DimensioneBicchiere
                    .ToDictionaryAsync(db => db.DimensioneBicchiereId);
                _memoryCache.Set(DIMENSIONI_CACHE_KEY, dimensioni, TimeSpan.FromMinutes(60));

                var ingredienti = await _context.Ingrediente
                    .Where(i => i.Disponibile)
                    .ToDictionaryAsync(i => i.IngredienteId);
                _memoryCache.Set(INGREDIENTI_CACHE_KEY, ingredienti, TimeSpan.FromMinutes(60));

                _logger.LogInformation("Cache calcoli prezzi precaricata con successo");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore nel precaricamento cache calcoli prezzi");
            }
        }

        public async Task ClearCalculationCacheAsync()
        {
            _memoryCache.Remove(TAX_RATES_CACHE_KEY);
            _memoryCache.Remove(DIMENSIONI_CACHE_KEY);
            _memoryCache.Remove(INGREDIENTI_CACHE_KEY);

            _logger.LogInformation("Cache calcoli prezzi pulita");
        }

        public async Task<bool> IsCacheValidAsync()
        {
            return _memoryCache.TryGetValue(TAX_RATES_CACHE_KEY, out _) &&
                   _memoryCache.TryGetValue(DIMENSIONI_CACHE_KEY, out _);
        }
    }
}