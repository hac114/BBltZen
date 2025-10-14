using Database;
using DTO;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;
using Repository.Interface;
using Repository.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace RepositoryTest
{
    public class AdvancedPriceCalculationServiceRepositoryTest : BaseTest
    {
        private readonly IAdvancedPriceCalculationServiceRepository _advancedPriceService;
        private readonly IPriceCalculationServiceRepository _basicPriceService;
        private readonly Mock<ILogger<AdvancedPriceCalculationServiceRepository>> _mockLogger;
        private readonly IMemoryCache _memoryCache;

        public AdvancedPriceCalculationServiceRepositoryTest()
        {
            _mockLogger = new Mock<ILogger<AdvancedPriceCalculationServiceRepository>>();
            _memoryCache = new MemoryCache(new MemoryCacheOptions());

            // Inizializza dati di test
            InitializeTestData();

            // Crea servizio base
            _basicPriceService = new PriceCalculationServiceRepository(
                _memoryCache,
                Mock.Of<ILogger<PriceCalculationServiceRepository>>(),
                new BevandaStandardRepository(_context),
                new BevandaCustomRepository(_context),
                new DolceRepository(_context),
                new PersonalizzazioneCustomRepository(_context),
                new IngredienteRepository(_context),
                new IngredientiPersonalizzazioneRepository(_context),
                new DimensioneBicchiereRepository(_context),
                new TaxRatesRepository(_context)
            );

            // Crea servizio avanzato
            _advancedPriceService = new AdvancedPriceCalculationServiceRepository(
                _context,
                _memoryCache,
                _mockLogger.Object,
                _basicPriceService
            );
        }

        private void InitializeTestData()
        {
            // Pulisci e ricrea database
            _context.Database.EnsureDeleted();
            _context.Database.EnsureCreated();

            // Tax Rates
            _context.TaxRates.AddRange(
                new TaxRates { TaxRateId = 1, Aliquota = 22.00m, Descrizione = "IVA Standard" },
                new TaxRates { TaxRateId = 2, Aliquota = 10.00m, Descrizione = "IVA Ridotta" }
            );

            // Dimensioni Bicchieri
            _context.DimensioneBicchiere.AddRange(
                new DimensioneBicchiere { DimensioneBicchiereId = 1, Sigla = "M", Descrizione = "medium", Capienza = 500, PrezzoBase = 3.50m, Moltiplicatore = 1.00m },
                new DimensioneBicchiere { DimensioneBicchiereId = 2, Sigla = "L", Descrizione = "large", Capienza = 700, PrezzoBase = 5.00m, Moltiplicatore = 1.30m }
            );

            // Ingredienti
            _context.Ingrediente.AddRange(
                new Ingrediente { IngredienteId = 1, Ingrediente1 = "Tea Nero Premium", CategoriaId = 1, PrezzoAggiunto = 1.00m, Disponibile = true },
                new Ingrediente { IngredienteId = 2, Ingrediente1 = "Latte Condensato", CategoriaId = 2, PrezzoAggiunto = 0.50m, Disponibile = true }
            );

            // Articoli e Bevande Standard
            _context.Articolo.AddRange(
                new Articolo { ArticoloId = 1, Tipo = "BS", DataCreazione = DateTime.Now, DataAggiornamento = DateTime.Now },
                new Articolo { ArticoloId = 2, Tipo = "D", DataCreazione = DateTime.Now, DataAggiornamento = DateTime.Now }
            );

            _context.BevandaStandard.AddRange(
                new BevandaStandard { ArticoloId = 1, PersonalizzazioneId = 1, DimensioneBicchiereId = 1, Prezzo = 4.50m, Disponibile = true, DataCreazione = DateTime.Now, DataAggiornamento = DateTime.Now }
            );

            _context.Dolce.AddRange(
                new Dolce { ArticoloId = 2, Nome = "Tiramisu", Prezzo = 5.50m, Disponibile = true, DataCreazione = DateTime.Now, DataAggiornamento = DateTime.Now }
            );

            // Personalizzazione Custom per test
            _context.PersonalizzazioneCustom.AddRange(
                new PersonalizzazioneCustom { PersCustomId = 1, Nome = "Test Custom", GradoDolcezza = 3, DimensioneBicchiereId = 1, DataCreazione = DateTime.Now, DataAggiornamento = DateTime.Now }
            );

            _context.IngredientiPersonalizzazione.AddRange(
                new IngredientiPersonalizzazione { IngredientePersId = 1, PersCustomId = 1, IngredienteId = 1, DataCreazione = DateTime.Now }
            );

            _context.SaveChanges();
        }

        [Fact]
        public async Task CalculateBevandaCustomPriceAsync_WithValidId_ReturnsCorrectPrice()
        {
            // Act
            var result = await _advancedPriceService.CalculateBevandaCustomPriceAsync(1);

            // Assert - Prezzo base 3.50 + ingrediente 1.00 = 4.50
            Assert.Equal(4.50m, result);
        }

        [Fact]
        public async Task CalculateBevandaCustomPriceAsync_WithLargeSize_ReturnsCorrectPriceWithMultiplier()
        {
            // Arrange
            var personalizzazioneLarge = new PersonalizzazioneCustom
            {
                PersCustomId = 2,
                Nome = "Test Large",
                GradoDolcezza = 3,
                DimensioneBicchiereId = 2, // Large
                DataCreazione = DateTime.Now,
                DataAggiornamento = DateTime.Now
            };
            _context.PersonalizzazioneCustom.Add(personalizzazioneLarge);

            var ingredientePers = new IngredientiPersonalizzazione
            {
                IngredientePersId = 2,
                PersCustomId = 2,
                IngredienteId = 1, // Tea Nero Premium - 1.00€
                DataCreazione = DateTime.Now
            };
            _context.IngredientiPersonalizzazione.Add(ingredientePers);
            await _context.SaveChangesAsync();

            // Act
            var result = await _advancedPriceService.CalculateBevandaCustomPriceAsync(2);

            // Assert - Prezzo base 5.00 + (ingrediente 1.00 × 1.3) = 6.30
            Assert.Equal(6.30m, result);
        }

        [Fact]
        public async Task CalculateCompletePriceAsync_WithBevandaStandard_ReturnsCompleteCalculation()
        {
            // Arrange
            var request = new PriceCalculationRequestDTO
            {
                ArticoloId = 1,
                TipoArticolo = "BS",
                Quantita = 2,
                TaxRateId = 1
            };

            // Act
            var result = await _advancedPriceService.CalculateCompletePriceAsync(request);

            // Assert
            Assert.Equal(1, result.ArticoloId);
            Assert.Equal("BS", result.TipoArticolo);
            Assert.Equal(4.50m, result.PrezzoBase);
            Assert.Equal(4.50m, result.PrezzoUnitario);
            Assert.Equal(9.00m, result.TotaleIvato);
            Assert.Equal(22.00m, result.AliquotaIva);
            Assert.True(result.Imponibile > 0);
            Assert.True(result.IvaAmount > 0);
        }

        [Fact]
        public async Task CalculateTaxAmountAsync_WithValidInput_ReturnsCorrectTax()
        {
            // Act
            var result = await _advancedPriceService.CalculateTaxAmountAsync(12.20m, 1); // 12.20€ con IVA 22%

            // Assert - 12.20 - (12.20 / 1.22) = 2.20
            Assert.Equal(2.20m, result);
        }

        [Fact]
        public async Task CalculateImponibileAsync_WithValidInput_ReturnsCorrectImponibile()
        {
            // Act
            var result = await _advancedPriceService.CalculateImponibileAsync(12.20m, 1); // 12.20€ con IVA 22%

            // Assert - 12.20 / 1.22 = 10.00
            Assert.Equal(10.00m, result);
        }

        [Fact]
        public async Task GetTaxRateAsync_WithValidId_ReturnsTaxRate()
        {
            // Act
            var result = await _advancedPriceService.GetTaxRateAsync(1);

            // Assert
            Assert.Equal(22.00m, result);
        }

        [Fact]
        public async Task CalculateDetailedCustomBeveragePriceAsync_WithValidId_ReturnsDetailedCalculation()
        {
            // Act
            var result = await _advancedPriceService.CalculateDetailedCustomBeveragePriceAsync(1);

            // Assert
            Assert.Equal(1, result.PersonalizzazioneCustomId);
            Assert.Equal("Test Custom", result.NomePersonalizzazione);
            Assert.Equal(1, result.DimensioneBicchiereId);
            Assert.Equal(3.50m, result.PrezzoBaseDimensione);
            Assert.Equal(1.00m, result.MoltiplicatoreDimensione);
            Assert.Equal(1.00m, result.PrezzoIngredienti);
            Assert.Equal(4.50m, result.PrezzoTotale);
            Assert.Single(result.Ingredienti);
        }

        [Fact]
        public async Task CalculateBatchPricesAsync_WithMultipleRequests_ReturnsAllCalculations()
        {
            // Arrange
            var requests = new List<PriceCalculationRequestDTO>
            {
                new() { ArticoloId = 1, TipoArticolo = "BS", Quantita = 1, TaxRateId = 1 },
                new() { ArticoloId = 2, TipoArticolo = "D", Quantita = 1, TaxRateId = 1 }
            };

            // Act
            var results = await _advancedPriceService.CalculateBatchPricesAsync(requests);

            // Assert
            Assert.Equal(2, results.Count);
            Assert.Contains(results, r => r.TipoArticolo == "BS");
            Assert.Contains(results, r => r.TipoArticolo == "D");
        }

        [Fact]
        public async Task ApplyDiscountAsync_WithValidDiscount_ReturnsDiscountedPrice()
        {
            // Act
            var result = await _advancedPriceService.ApplyDiscountAsync(10.00m, 20); // 20% di sconto

            // Assert
            Assert.Equal(8.00m, result);
        }

        [Fact]
        public async Task ValidatePriceCalculationAsync_WithCorrectPrice_ReturnsTrue()
        {
            // Act
            var result = await _advancedPriceService.ValidatePriceCalculationAsync(1, "BS", 4.50m);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task ValidatePriceCalculationAsync_WithIncorrectPrice_ReturnsFalse()
        {
            // Act
            var result = await _advancedPriceService.ValidatePriceCalculationAsync(1, "BS", 10.00m);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task PreloadCalculationCacheAsync_ShouldLoadCacheWithoutErrors()
        {
            // Act & Assert - Non dovrebbe lanciare eccezioni
            await _advancedPriceService.PreloadCalculationCacheAsync();
        }

        [Fact]
        public async Task ClearCalculationCacheAsync_ShouldClearCacheWithoutErrors()
        {
            // Act & Assert - Non dovrebbe lanciare eccezioni
            await _advancedPriceService.ClearCalculationCacheAsync();
        }

        [Fact]
        public async Task CalculateBevandaCustomPriceAsync_WithInvalidId_ThrowsException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _advancedPriceService.CalculateBevandaCustomPriceAsync(999));
        }

        [Fact]
        public async Task CalculateCompletePriceAsync_WithInvalidTipoArticolo_ThrowsException()
        {
            // Arrange
            var request = new PriceCalculationRequestDTO
            {
                ArticoloId = 1,
                TipoArticolo = "INVALID",
                Quantita = 1,
                TaxRateId = 1
            };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _advancedPriceService.CalculateCompletePriceAsync(request));
        }
    }
}