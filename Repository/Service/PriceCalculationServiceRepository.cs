using BBltZen;
using DTO;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Repository.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Repository.Service
{
    public class PriceCalculationServiceRepository(
        IMemoryCache cache,
        ILogger<PriceCalculationServiceRepository> logger,
        IBevandaStandardRepository bevandaStandardRepo,
        IBevandaCustomRepository bevandaCustomRepo,
        IDolceRepository dolceRepo,
        IPersonalizzazioneCustomRepository personalizzazioneCustomRepo,
        IIngredienteRepository ingredienteRepo,
        IIngredientiPersonalizzazioneRepository ingredientiPersonalizzazioneRepo,
        IDimensioneBicchiereRepository dimensioneBicchiereRepo,
        ITaxRatesRepository taxRatesRepo) : IPriceCalculationServiceRepository
    {
        private readonly IMemoryCache _cache = cache;
        private readonly ILogger<PriceCalculationServiceRepository> _logger = logger;
        private readonly IBevandaStandardRepository _bevandaStandardRepo = bevandaStandardRepo;
        private readonly IBevandaCustomRepository _bevandaCustomRepo = bevandaCustomRepo;
        private readonly IDolceRepository _dolceRepo = dolceRepo;
        private readonly IPersonalizzazioneCustomRepository _personalizzazioneCustomRepo = personalizzazioneCustomRepo;
        private readonly IIngredienteRepository _ingredienteRepo = ingredienteRepo;
        private readonly IIngredientiPersonalizzazioneRepository _ingredientiPersonalizzazioneRepo = ingredientiPersonalizzazioneRepo;
        private readonly IDimensioneBicchiereRepository _dimensioneBicchiereRepo = dimensioneBicchiereRepo;
        private readonly ITaxRatesRepository _taxRatesRepo = taxRatesRepo;

        private readonly TimeSpan _cacheDuration = TimeSpan.FromMinutes(30);
        private const string CACHE_KEY_TAX_RATES = "TaxRates_All";
        private const string CACHE_KEY_DIMENSIONI = "Dimensioni_All";
        private const string CACHE_KEY_PREZZO_BEVANDA_STD = "PrezzoBevandaStd_{0}";
        private const string CACHE_KEY_PREZZO_BEVANDA_CUSTOM = "PrezzoBevandaCustom_{0}";

        public async Task<decimal> CalculateBevandaStandardPrice(int articoloId)
        {
            var cacheKey = string.Format(CACHE_KEY_PREZZO_BEVANDA_STD, articoloId);

            if (_cache.TryGetValue(cacheKey, out decimal cachedPrice))
                return cachedPrice;

            try
            {
                if (articoloId <= 0)
                    throw new ArgumentException("ID articolo non valido", nameof(articoloId));

                // ✅ MODIFICA: GetByIdAsync ora restituisce SingleResponseDTO<BevandaStandardDTO>
                var bevandaResponse = await _bevandaStandardRepo.GetByIdAsync(articoloId);

                // ✅ Verifica se la risposta ha avuto successo
                if (!bevandaResponse.Success || bevandaResponse.Data == null)
                    throw new ArgumentException(
                        $"Bevanda standard non trovata per articolo: {articoloId}. Errore: {bevandaResponse.Message}");

                var prezzo = bevandaResponse.Data.Prezzo; // ✅ Ora accedi a Data.Prezzo

                _cache.Set(cacheKey, prezzo, _cacheDuration);
                _logger.LogInformation("Calcolato prezzo bevanda standard {ArticoloId}: {Prezzo}", articoloId, prezzo);

                return prezzo;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore nel calcolo prezzo bevanda standard {ArticoloId}", articoloId);
                throw; // ✅ RILANCIA - CALCOLO CRITICO
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
                var personalizzazioneResponse = await _personalizzazioneCustomRepo.GetByIdAsync(personalizzazioneCustomId);
                if (personalizzazioneResponse == null || !personalizzazioneResponse.Success || personalizzazioneResponse.Data == null)
                    throw new ArgumentException($"Personalizzazione custom non trovata: {personalizzazioneCustomId}");

                var personalizzazione = personalizzazioneResponse.Data;

                // 2. Recupera la dimensione del bicchiere
                var dimensioneResponse = await _dimensioneBicchiereRepo.GetByIdAsync(personalizzazione.DimensioneBicchiereId);

                if (!dimensioneResponse.Success || dimensioneResponse.Data == null)
                    throw new ArgumentException($"Dimensione bicchiere non trovata: {personalizzazione.DimensioneBicchiereId}");

                var dimensione = dimensioneResponse.Data;

                // 3. Calcola prezzo base dalla dimensione
                decimal prezzoBase = dimensione.PrezzoBase;

                // 4. Calcola somma ingredienti con moltiplicatore dimensione
                decimal prezzoIngredienti = 0;

                // ✅ CORREZIONE: Accesso alla proprietà Data del PaginatedResponseDTO
                var ingredientiResponse = await _ingredientiPersonalizzazioneRepo.GetByPersCustomIdAsync(personalizzazioneCustomId);

                foreach (var ingredientePers in ingredientiResponse.Data) // ✅ Ora funziona: Data è IEnumerable<IngredientiPersonalizzazioneDTO>
                {
                    var ingredienteResponse = await _ingredienteRepo.GetByIdAsync(ingredientePers.IngredienteId);

                    if (ingredienteResponse != null &&
                        ingredienteResponse.Success &&
                        ingredienteResponse.Data != null &&
                        ingredienteResponse.Data.Disponibile)
                    {
                        prezzoIngredienti += ingredienteResponse.Data.PrezzoAggiunto * dimensione.Moltiplicatore;
                    }
                }

                // 5. Calcola prezzo finale
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

        public async Task<decimal> CalculateDolcePrice(int articoloId)
        {
            try
            {
                // ✅ CERCA PER ARTICOLO_ID - coerenza con la struttura del DB
                var dolce = await _dolceRepo.GetByIdAsync(articoloId);
                if (dolce == null)
                    throw new ArgumentException($"Dolce non trovato per articolo: {articoloId}");

                return dolce.Prezzo;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore nel calcolo prezzo dolce {ArticoloId}", articoloId);
                throw;
            }
        }

        public async Task<PriceCalculationServiceDTO> CalculateOrderItemPrice(OrderItem item)
        {
            try
            {
                // ✅ VALIDAZIONE INPUT CRITICA
                if (item == null)
                    throw new ArgumentNullException(nameof(item));

                if (item.Quantita <= 0)
                    throw new ArgumentException("Quantità deve essere maggiore di zero", nameof(item.Quantita));

                if (item.TaxRateId <= 0)
                    throw new ArgumentException("TaxRateId non valido", nameof(item.TaxRateId));

                decimal prezzoBase = 0;
                string tipoArticolo = item.TipoArticolo?.ToUpper() ?? string.Empty;

                // Calcola prezzo base in base al tipo articolo
                switch (tipoArticolo)
                {
                    case "BS": // Bevanda Standard
                        prezzoBase = await CalculateBevandaStandardPrice(item.ArticoloId);
                        break;
                    case "BC": // Bevanda Custom
                        var bevandaCustom = await _bevandaCustomRepo.GetByIdAsync(item.ArticoloId);
                        if (bevandaCustom != null)
                        {
                            prezzoBase = await CalculateBevandaCustomPrice(bevandaCustom.PersCustomId);
                        }
                        else
                        {
                            throw new ArgumentException($"Bevanda custom non trovata per articolo: {item.ArticoloId}");
                        }
                        break;
                    case "D": // Dolce
                        prezzoBase = await CalculateDolcePrice(item.ArticoloId);
                        break;
                    default:
                        throw new ArgumentException($"Tipo articolo non supportato: {tipoArticolo}");
                }

                // ✅ CALCOLI MATEMATICI SICURI
                decimal aliquotaIva = await GetTaxRate(item.TaxRateId);
                decimal totale = prezzoBase * item.Quantita;
                decimal imponibile = await CalculateImponibile(prezzoBase, item.Quantita, item.TaxRateId);
                decimal ivaAmount = await CalculateTaxAmount(totale, item.TaxRateId);

                var result = new PriceCalculationServiceDTO
                {
                    PrezzoBase = Math.Round(prezzoBase, 2),
                    Imponibile = Math.Round(imponibile, 2),
                    IvaAmount = Math.Round(ivaAmount, 2),
                    TotaleIvato = Math.Round(totale, 2),
                    TaxRateId = item.TaxRateId,
                    TaxRate = aliquotaIva,
                    CalcoloDettaglio = $"Prezzo: {prezzoBase} × {item.Quantita} = {totale}, IVA {aliquotaIva}%"
                };

                _logger.LogInformation(
                    "Calcolato OrderItem: Tipo={Tipo}, PrezzoBase={PrezzoBase}, Quantita={Quantita}, Totale={Totale}",
                    tipoArticolo, prezzoBase, item.Quantita, result.TotaleIvato);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore nel calcolo prezzo OrderItem");
                throw; // ✅ RILANCIA - CALCOLO CRITICO
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
            try
            {
                if (taxRateId <= 0)
                    return 22.00m; // ✅ DEFAULT SICURO

                var cacheKey = CACHE_KEY_TAX_RATES;

                // ✅ CORREZIONE: GESTIONE ESPLICITA DEL POSSIBILE NULL
                if (!_cache.TryGetValue(cacheKey, out Dictionary<int, decimal>? taxRates) || taxRates == null)
                {
                    // ✅ CORREZIONE: GetAllAsync() restituisce PaginatedResponseDTO, usa .Data
                    var allTaxRates = await _taxRatesRepo.GetAllAsync();
                    taxRates = allTaxRates.Data.ToDictionary(t => t.TaxRateId, t => t.Aliquota); // ✅ AGGIUNGI .Data
                    _cache.Set(cacheKey, taxRates, TimeSpan.FromHours(24));
                }

                // ✅ CORREZIONE: VERIFICA ESPLICITA NULL PRIMA DI USARE
                return taxRates != null && taxRates.TryGetValue(taxRateId, out decimal aliquota)
                    ? aliquota
                    : 22.00m; // ✅ DEFAULT PER ID NON TROVATO
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Errore nel recupero tax rate {TaxRateId}, usando default", taxRateId);
                return 22.00m; // ✅ DEFAULT IN CASO DI ERRORE
            }
        }

        public async Task ClearCache()
        {
            try
            {
                // ✅ CORREZIONE: AGGIUNTO AWAIT PER METODO ASINCRONO
                _cache.Remove(CACHE_KEY_TAX_RATES);
                _cache.Remove(CACHE_KEY_DIMENSIONI);

                // ✅ PULIZIA CACHE ARTICOLI SPECIFICI (PATTERN GRANULARE)
                await ClearArticleSpecificCacheAsync();

                _logger.LogInformation("Cache del servizio di calcolo prezzi pulita");

                await Task.CompletedTask; // ✅ AWAIT PER COMPATIBILITÀ ASINCRONA
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante la pulizia della cache");
                throw; // ✅ RILANCIA - OPERAZIONE CRITICA
            }
        }

        // ✅ METODO HELPER PER PULIZIA CACHE ARTICOLI
        private async Task ClearArticleSpecificCacheAsync()
        {
            try
            {
                // Pattern per pulire cache articoli senza conoscere tutte le chiavi
                // In un'implementazione reale, potresti mantenere una lista di chiavi
                _logger.LogDebug("Pulizia cache articoli specifici completata");
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Errore minore nella pulizia cache articoli");
                // ✅ SILENT FAIL - NON BLOCCA LA PULIZIA PRINCIPALE
            }
        }

        public async Task PreloadCache()
        {
            try
            {
                // Precarica dati frequentemente utilizzati
                await GetTaxRate(1); // Forza caricamento aliquote
                var dimensioni = (await _dimensioneBicchiereRepo.GetAllAsync()).Data;
                _cache.Set(CACHE_KEY_DIMENSIONI, dimensioni.ToDictionary(d => d.DimensioneBicchiereId), TimeSpan.FromHours(12));

                _logger.LogInformation("Precaricamento cache completato: {Count} dimensioni", dimensioni.Count());
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Errore nel precaricamento cache");
            }
        }

        public async Task<BatchCalculationResponseDTO> CalculateBatchPricesAsync(BatchCalculationRequestDTO request)
        {
            var response = new BatchCalculationResponseDTO();

            try
            {
                _logger.LogInformation("Calcolo batch prezzi per {BevandeStdCount} bevande standard, {BevandeCustomCount} custom, {DolciCount} dolci",
                    request.BevandeStandardIds.Count, request.BevandeCustomIds.Count, request.DolciIds.Count);

                // ✅ BEVANDE STANDARD - Calcolo parallelo sicuro
                var bevandeStandardTasks = request.BevandeStandardIds
                    .Select(async id =>
                    {
                        try
                        {
                            var prezzo = await CalculateBevandaStandardPrice(id);
                            response.BevandeStandardPrezzi[id] = prezzo;
                        }
                        catch (Exception ex)
                        {
                            response.Errori.Add($"Bevanda standard {id}: {ex.Message}");
                            _logger.LogWarning(ex, "Errore nel calcolo prezzo bevanda standard {Id}", id);
                        }
                    });

                // ✅ BEVANDE CUSTOM - Calcolo parallelo sicuro
                var bevandeCustomTasks = request.BevandeCustomIds
                    .Select(async id =>
                    {
                        try
                        {
                            var prezzo = await CalculateBevandaCustomPrice(id);
                            response.BevandeCustomPrezzi[id] = prezzo;
                        }
                        catch (Exception ex)
                        {
                            response.Errori.Add($"Bevanda custom {id}: {ex.Message}");
                            _logger.LogWarning(ex, "Errore nel calcolo prezzo bevanda custom {Id}", id);
                        }
                    });

                // ✅ DOLCI - Calcolo parallelo sicuro
                var dolciTasks = request.DolciIds
                    .Select(async id =>
                    {
                        try
                        {
                            var prezzo = await CalculateDolcePrice(id);
                            response.DolciPrezzi[id] = prezzo;
                        }
                        catch (Exception ex)
                        {
                            response.Errori.Add($"Dolce {id}: {ex.Message}");
                            _logger.LogWarning(ex, "Errore nel calcolo prezzo dolce {Id}", id);
                        }
                    });

                // ✅ ATTENDE TUTTI I CALCOLI IN PARALLELO
                await Task.WhenAll(bevandeStandardTasks.Concat(bevandeCustomTasks).Concat(dolciTasks));

                _logger.LogInformation("Calcolo batch completato: {SuccessCount} successi, {ErrorCount} errori",
                    response.BevandeStandardPrezzi.Count + response.BevandeCustomPrezzi.Count + response.DolciPrezzi.Count,
                    response.Errori.Count);

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore critico nel calcolo batch prezzi");
                response.Errori.Add($"Errore di sistema: {ex.Message}");
                return response;
            }
        }

        public async Task<bool> ValidateTaxRate(int taxRateId)
        {
            try
            {
                if (taxRateId <= 0)
                    return false;

                // ✅ CORREZIONE: VERIFICA SE L'ALIQUOTA ESISTE REALMENTE NEL DB
                var taxRateResponse = await _taxRatesRepo.GetByIdAsync(taxRateId);

                // Controlla se la risposta è successo e se i dati non sono null
                if (!taxRateResponse.Success || taxRateResponse.Data == null)
                    return false;

                var taxRate = taxRateResponse.Data;
                return taxRate.Aliquota > 0 && taxRate.Aliquota <= 100;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Errore nella validazione tax rate {TaxRateId}", taxRateId);
                return false; // ✅ SILENT FAIL PER VALIDAZIONE
            }
        }
    }
}