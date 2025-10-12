// In RepositoryTest/PriceCalculationServiceRepositoryTest.cs
using Database;
using DTO;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;
using Repository.Interface;
using Repository.Service;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace RepositoryTest
{
    public class PriceCalculationServiceRepositoryTest : BaseTest
    {
        private readonly IPriceCalculationServiceRepository _priceCalculationService;
        private readonly Mock<ILogger<PriceCalculationServiceRepository>> _mockLogger;
        private readonly IMemoryCache _memoryCache;

        public PriceCalculationServiceRepositoryTest()
        {
            _mockLogger = new Mock<ILogger<PriceCalculationServiceRepository>>();
            _memoryCache = new MemoryCache(new MemoryCacheOptions());

            // Crea i repository necessari per il servizio
            var bevandaStandardRepo = new BevandaStandardRepository(_context);
            var bevandaCustomRepo = new BevandaCustomRepository(_context);
            var dolceRepo = new DolceRepository(_context);
            var personalizzazioneCustomRepo = new PersonalizzazioneCustomRepository(_context);
            var ingredienteRepo = new IngredienteRepository(_context);
            var ingredientiPersonalizzazioneRepo = new IngredientiPersonalizzazioneRepository(_context);
            var dimensioneBicchiereRepo = new DimensioneBicchiereRepository(_context);
            var taxRatesRepo = new TaxRatesRepository(_context);

            _priceCalculationService = new PriceCalculationServiceRepository(
                _memoryCache,
                _mockLogger.Object,
                bevandaStandardRepo,
                bevandaCustomRepo,
                dolceRepo,
                personalizzazioneCustomRepo,
                ingredienteRepo,
                ingredientiPersonalizzazioneRepo,
                dimensioneBicchiereRepo,
                taxRatesRepo
            );

            InitializeTestData();
        }

        private void InitializeTestData()
        {
            // ✅ INIZIALIZZA DATI DI TEST
            if (!_context.TaxRates.Any())
            {
                _context.TaxRates.AddRange(
                    new TaxRates { TaxRateId = 1, Aliquota = 22.00m, Descrizione = "IVA Standard" },
                    new TaxRates { TaxRateId = 2, Aliquota = 10.00m, Descrizione = "IVA Ridotta" }
                );
            }

            if (!_context.DimensioneBicchiere.Any())
            {
                _context.DimensioneBicchiere.AddRange(
                    new DimensioneBicchiere { DimensioneBicchiereId = 1, Sigla = "M", Descrizione = "medium", Capienza = 500, PrezzoBase = 3.50m, Moltiplicatore = 1.00m },
                    new DimensioneBicchiere { DimensioneBicchiereId = 2, Sigla = "L", Descrizione = "large", Capienza = 700, PrezzoBase = 5.00m, Moltiplicatore = 1.30m }
                );
            }

            if (!_context.Ingrediente.Any())
            {
                _context.Ingrediente.AddRange(
                    // ✅ USA LA ENTITY DEL DATABASE - verifica il nome esatto della proprietà
                    new Ingrediente { IngredienteId = 1, Ingrediente1 = "Tea Nero Premium", CategoriaId = 1, PrezzoAggiunto = 1.00m, Disponibile = true },
                    new Ingrediente { IngredienteId = 2, Ingrediente1 = "Latte Condensato", CategoriaId = 2, PrezzoAggiunto = 0.50m, Disponibile = true }
                );
            }

            _context.SaveChanges();
        }

        [Fact]
        public async Task CalculateBevandaStandardPrice_WithValidId_ReturnsPrice()
        {
            // Arrange - Usa un ArticoloId che ESISTE già nei dati di test
            var existingBevanda = _context.BevandaStandard.First();

            // Act - Passa l'ArticoloId che sappiamo esistere
            var result = await _priceCalculationService.CalculateBevandaStandardPrice(existingBevanda.ArticoloId);

            // Assert
            Assert.Equal(existingBevanda.Prezzo, result);
        }

        [Fact]
        public async Task CalculateTaxAmount_WithValidInput_ReturnsCorrectTax()
        {
            // Act
            var result = await _priceCalculationService.CalculateTaxAmount(10.00m, 1); // 10€ con IVA 22%

            // Assert
            Assert.Equal(1.80m, result); // 10 - (10 / 1.22) = 1.80
        }

        [Fact]
        public async Task GetTaxRate_WithValidId_ReturnsTaxRate()
        {
            // Act
            var result = await _priceCalculationService.GetTaxRate(1);

            // Assert
            Assert.Equal(22.00m, result);
        }

        [Fact]
        public async Task CalculateBevandaCustomPrice_WithValidInput_ReturnsCorrectPrice()
        {
            // Arrange
            var personalizzazioneCustom = new PersonalizzazioneCustom
            {
                PersCustomId = 1,
                Nome = "Test Custom",
                GradoDolcezza = 3,
                DimensioneBicchiereId = 1 // Medium - prezzo base 3.50
            };
            _context.PersonalizzazioneCustom.Add(personalizzazioneCustom);

            var ingredientePersonalizzazione = new IngredientiPersonalizzazione
            {
                IngredientePersId = 1,
                PersCustomId = 1,
                IngredienteId = 1 // Tea Nero Premium - 1.00€
            };
            _context.IngredientiPersonalizzazione.Add(ingredientePersonalizzazione);

            await _context.SaveChangesAsync();

            // Act
            var result = await _priceCalculationService.CalculateBevandaCustomPrice(1);

            // Assert - Prezzo base 3.50 + ingrediente 1.00 = 4.50
            Assert.Equal(4.50m, result);
        }

        [Fact]
        public async Task CalculateBevandaCustomPrice_WithLargeSize_ReturnsCorrectPriceWithMultiplier()
        {
            // Arrange
            var personalizzazioneCustom = new PersonalizzazioneCustom
            {
                PersCustomId = 2,
                Nome = "Test Custom Large",
                GradoDolcezza = 3,
                DimensioneBicchiereId = 2 // Large - prezzo base 5.00, moltiplicatore 1.3
            };
            _context.PersonalizzazioneCustom.Add(personalizzazioneCustom);

            var ingredientePersonalizzazione = new IngredientiPersonalizzazione
            {
                IngredientePersId = 2,
                PersCustomId = 2,
                IngredienteId = 1 // Tea Nero Premium - 1.00€ × 1.3 = 1.30€
            };
            _context.IngredientiPersonalizzazione.Add(ingredientePersonalizzazione);

            await _context.SaveChangesAsync();

            // Act
            var result = await _priceCalculationService.CalculateBevandaCustomPrice(2);

            // Assert - Prezzo base 5.00 + (ingrediente 1.00 × 1.3) = 6.30
            Assert.Equal(6.30m, result);
        }

        [Fact]
        public async Task CalculateImponibile_WithValidInput_ReturnsCorrectImponibile()
        {
            // Act
            var result = await _priceCalculationService.CalculateImponibile(12.20m, 1, 1); // 12.20€ con IVA 22%

            // Assert - 12.20 / 1.22 = 10.00
            Assert.Equal(10.00m, result);
        }

        [Fact]
        public async Task ClearCache_ShouldClearMemoryCache()
        {
            // Act & Assert - Non dovrebbe lanciare eccezioni
            await _priceCalculationService.ClearCache();
        }

        [Fact]
        public async Task CalculateBevandaStandardPrice_WithInvalidId_ThrowsException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _priceCalculationService.CalculateBevandaStandardPrice(999));
        }

        [Fact]
        public async Task CalculateBevandaCustomPrice_WithInvalidId_ThrowsException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _priceCalculationService.CalculateBevandaCustomPrice(999));
        }
    }
}