using Database;
using DTO;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Repository.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Service
{
    public class PriceCalculationServiceRepository : IPriceCalculationServiceRepository
    {
        private readonly IMemoryCache _cache;
        private readonly ILogger<PriceCalculationServiceRepository> _logger;
        private readonly IBevandaStandardRepository _bevandaStandardRepo;
        private readonly IBevandaCustomRepository _bevandaCustomRepo;
        private readonly IDolceRepository _dolceRepo;
        private readonly IPersonalizzazioneCustomRepository _personalizzazioneCustomRepo;
        private readonly IIngredienteRepository _ingredienteRepo;
        private readonly IIngredientiPersonalizzazioneRepository _ingredientiPersonalizzazioneRepo;
        private readonly IDimensioneBicchiereRepository _dimensioneBicchiereRepo;
        private readonly ITaxRatesRepository _taxRatesRepo;

        private readonly TimeSpan _cacheDuration = TimeSpan.FromMinutes(30);
        private const string CACHE_KEY_TAX_RATES = "TaxRates_All";
        private const string CACHE_KEY_DIMENSIONI = "Dimensioni_All";
        private const string CACHE_KEY_PREZZO_BEVANDA_STD = "PrezzoBevandaStd_{0}";
        private const string CACHE_KEY_PREZZO_BEVANDA_CUSTOM = "PrezzoBevandaCustom_{0}";

        public PriceCalculationServiceRepository(
            IMemoryCache cache,
            ILogger<PriceCalculationServiceRepository> logger,
            IBevandaStandardRepository bevandaStandardRepo,
            IBevandaCustomRepository bevandaCustomRepo,
            IDolceRepository dolceRepo,
            IPersonalizzazioneCustomRepository personalizzazioneCustomRepo,
            IIngredienteRepository ingredienteRepo,
            IIngredientiPersonalizzazioneRepository ingredientiPersonalizzazioneRepo,
            IDimensioneBicchiereRepository dimensioneBicchiereRepo,
            ITaxRatesRepository taxRatesRepo)
        {
            _cache = cache;
            _logger = logger;
            _bevandaStandardRepo = bevandaStandardRepo;
            _bevandaCustomRepo = bevandaCustomRepo;
            _dolceRepo = dolceRepo;
            _personalizzazioneCustomRepo = personalizzazioneCustomRepo;
            _ingredienteRepo = ingredienteRepo;
            _ingredientiPersonalizzazioneRepo = ingredientiPersonalizzazioneRepo;
            _dimensioneBicchiereRepo = dimensioneBicchiereRepo;
            _taxRatesRepo = taxRatesRepo;
        }

        public async Task<decimal> CalculateBevandaStandardPrice(int bevandaStandardId)
        {
            var cacheKey = string.Format(CACHE_KEY_PREZZO_BEVANDA_STD, bevandaStandardId);

            if (_cache.TryGetValue(cacheKey, out decimal cachedPrice))
                return cachedPrice;

            try
            {
                var bevanda = await _bevandaStandardRepo.GetByIdAsync(bevandaStandardId);
                if (bevanda == null)
                    throw new ArgumentException($"Bevanda standard non trovata: {bevandaStandardId}");

                var prezzo = bevanda.Prezzo;

                _cache.Set(cacheKey, prezzo, _cacheDuration);
                _logger.LogInformation("Calcolato prezzo bevanda standard {BevandaId}: {Prezzo}", bevandaStandardId, prezzo);

                return prezzo;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore nel calcolo prezzo bevanda standard {BevandaId}", bevandaStandardId);
                throw;
            }
        }

        public async Task<decimal> CalculateBevandaCustomPrice(int personalizzazioneCustomId)
        {
            var cacheKey = string.Format(CACHE_KEY_PREZZO_BEVANDA_CUSTOM, personalizzazioneCustomId);

            if (_cache.TryGetValue(cacheKey, out decimal cachedPrice))
                return cachedPrice;

            try
            {
                // 1. Recupera la personalizzazione custom
                var personalizzazione = await _personalizzazioneCustomRepo.GetByIdAsync(personalizzazioneCustomId);
                if (personalizzazione == null)
                    throw new ArgumentException($"Personalizzazione custom non trovata: {personalizzazioneCustomId}");

                // 2. Recupera la dimensione del bicchiere
                var dimensione = await _dimensioneBicchiereRepo.GetByIdAsync(personalizzazione.DimensioneBicchiereId);
                if (dimensione == null)
                    throw new ArgumentException($"Dimensione bicchiere non trovata: {personalizzazione.DimensioneBicchiereId}");

                // 3. Calcola prezzo base dalla dimensione (replica logica DB)
                decimal prezzoBase = dimensione.PrezzoBase;

                // 4. Calcola somma ingredienti con moltiplicatore dimensione
                decimal prezzoIngredienti = 0;
                var ingredientiPersonalizzazione = await _ingredientiPersonalizzazioneRepo.GetByPersCustomIdAsync(personalizzazioneCustomId);

                foreach (var ingredientePers in ingredientiPersonalizzazione)
                {
                    var ingrediente = await _ingredienteRepo.GetByIdAsync(ingredientePers.IngredienteId);
                    if (ingrediente != null && ingrediente.Disponibile)
                    {
                        // Applica moltiplicatore dimensione (1.0 per Medium, 1.3 per Large)
                        prezzoIngredienti += ingrediente.PrezzoAggiunto * dimensione.Moltiplicatore;
                    }
                }

                // 5. Calcola prezzo finale (replica esatta logica DB)
                decimal prezzoFinale = Math.Round(prezzoBase + prezzoIngredienti, 2);

                _cache.Set(cacheKey, prezzoFinale, _cacheDuration);

                _logger.LogInformation(
                    "Calcolato prezzo bevanda custom {PersCustomId}: Base={Base}, Ingredienti={Ingredienti}, Finale={Finale}, Dimensione={Dimensione}",
                    personalizzazioneCustomId, prezzoBase, prezzoIngredienti, prezzoFinale, dimensione.Descrizione);

                return prezzoFinale;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore nel calcolo prezzo bevanda custom {PersCustomId}", personalizzazioneCustomId);
                throw;
            }
        }

        public async Task<decimal> CalculateDolcePrice(int dolceId)
        {
            try
            {
                var dolce = await _dolceRepo.GetByIdAsync(dolceId);
                if (dolce == null)
                    throw new ArgumentException($"Dolce non trovato: {dolceId}");

                return dolce.Prezzo;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore nel calcolo prezzo dolce {DolceId}", dolceId);
                throw;
            }
        }

        public async Task<PriceCalculationServiceDTO> CalculateOrderItemPrice(OrderItem item)
        {
            try
            {
                decimal prezzoBase = 0;
                string tipoArticolo = item.TipoArticolo;

                // Calcola prezzo base in base al tipo articolo
                switch (tipoArticolo.ToUpper())
                {
                    case "BS": // Bevanda Standard
                        prezzoBase = await CalculateBevandaStandardPrice(item.ArticoloId);
                        break;
                    case "BC": // Bevanda Custom
                               // Per bevanda custom, articolo_id punta a BEVANDA_CUSTOM, che ha pers_custom_id
                        var bevandaCustom = await _bevandaCustomRepo.GetByArticoloIdAsync(item.ArticoloId);
                        if (bevandaCustom != null)
                        {
                            prezzoBase = await CalculateBevandaCustomPrice(bevandaCustom.PersCustomId);
                        }
                        break;
                    case "D": // Dolce
                        prezzoBase = await CalculateDolcePrice(item.ArticoloId);
                        break;
                    default:
                        throw new ArgumentException($"Tipo articolo non supportato: {tipoArticolo}");
                }

                // Calcola imponibile e IVA (replica funzioni SQL)
                decimal aliquotaIva = await GetTaxRate(item.TaxRateId);
                decimal imponibile = await CalculateImponibile(prezzoBase, item.Quantita, item.TaxRateId);
                decimal ivaAmount = await CalculateTaxAmount(prezzoBase * item.Quantita, item.TaxRateId);
                decimal totaleIvato = prezzoBase * item.Quantita;

                var result = new PriceCalculationServiceDTO
                {
                    PrezzoBase = prezzoBase,
                    Imponibile = Math.Round(imponibile, 2),
                    IvaAmount = Math.Round(ivaAmount, 2),
                    TotaleIvato = Math.Round(totaleIvato, 2),
                    TaxRateId = item.TaxRateId,
                    TaxRate = aliquotaIva,
                    CalcoloDettaglio = $"Prezzo: {prezzoBase} × {item.Quantita} = {totaleIvato}, IVA {aliquotaIva}%"
                };

                _logger.LogInformation(
                    "Calcolato OrderItem {OrderItemId}: PrezzoBase={PrezzoBase}, Quantita={Quantita}, Imponibile={Imponibile}, IVA={Iva}, Totale={Totale}",
                    item.OrderItemId, prezzoBase, item.Quantita, result.Imponibile, result.IvaAmount, result.TotaleIvato);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore nel calcolo prezzo OrderItem {OrderItemId}", item.OrderItemId);
                throw;
            }
        }

        public async Task<decimal> CalculateTaxAmount(decimal imponibile, int taxRateId)
        {
            // Replica funzione SQL: CalcolaIVA
            decimal aliquota = await GetTaxRate(taxRateId);
            decimal totale = imponibile;
            decimal imponibileCalc = totale / (1 + (aliquota / 100));
            return Math.Round(totale - imponibileCalc, 2);
        }

        public async Task<decimal> CalculateImponibile(decimal prezzo, int quantita, int taxRateId)
        {
            // Replica funzione SQL: CalcolaImponibile
            decimal aliquota = await GetTaxRate(taxRateId);
            decimal totale = prezzo * quantita;
            return Math.Round(totale / (1 + (aliquota / 100)), 2);
        }

        public async Task<decimal> GetTaxRate(int taxRateId)
        {
            var cacheKey = CACHE_KEY_TAX_RATES;

            if (!_cache.TryGetValue(cacheKey, out Dictionary<int, decimal> taxRates))
            {
                var allTaxRates = await _taxRatesRepo.GetAllAsync();
                taxRates = allTaxRates.ToDictionary(t => t.TaxRateId, t => t.Aliquota);
                _cache.Set(cacheKey, taxRates, TimeSpan.FromHours(24)); // Cache lunga per aliquote
            }

            return taxRates.TryGetValue(taxRateId, out decimal aliquota) ? aliquota : 22.00m; // Default IVA standard
        }

        public async Task ClearCache()
        {
            // Implementa logica per pulire cache specifica
            _cache.Remove(CACHE_KEY_TAX_RATES);
            _cache.Remove(CACHE_KEY_DIMENSIONI);
            // Nota: Per cache specifiche articoli, sarebbe meglio avere un metodo più granulare
            _logger.LogInformation("Cache del servizio di calcolo prezzi pulita");
        }

        public async Task PreloadCache()
        {
            try
            {
                // Precarica dati frequentemente utilizzati
                await GetTaxRate(1); // Forza caricamento aliquote
                var dimensioni = await _dimensioneBicchiereRepo.GetAllAsync();
                _cache.Set(CACHE_KEY_DIMENSIONI, dimensioni.ToDictionary(d => d.DimensioneBicchiereId), TimeSpan.FromHours(12));

                _logger.LogInformation("Precaricamento cache completato: {Count} dimensioni", dimensioni.Count());
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Errore nel precaricamento cache");
            }
        }
    }
}
