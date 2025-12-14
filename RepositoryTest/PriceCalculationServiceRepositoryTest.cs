using BBltZen;
using DTO;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
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

            // ✅ INIZIALIZZA DATI COMPLETI prima di creare i repository
            InitializeCompleteTestData();

            // ✅ CREA REPOSITORY CON LO STESSO CONTEXT
            _priceCalculationService = CreatePriceCalculationService(_context);
        }

        private void InitializeCompleteTestData()
        {
            // ✅ PULISCI E RICREA TUTTI I DATI
            _context.Database.EnsureDeleted();
            _context.Database.EnsureCreated();

            var now = DateTime.UtcNow; // ✅ UTC PER COERENZA

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
                new Articolo { ArticoloId = 1, Tipo = "BS", DataCreazione = now, DataAggiornamento = now },
                new Articolo { ArticoloId = 2, Tipo = "BS", DataCreazione = now, DataAggiornamento = now },
                new Articolo { ArticoloId = 3, Tipo = "D", DataCreazione = now, DataAggiornamento = now }
            );

            _context.BevandaStandard.AddRange(
                new BevandaStandard { ArticoloId = 1, PersonalizzazioneId = 1, DimensioneBicchiereId = 1, Prezzo = 4.50m, Disponibile = true, DataCreazione = now, DataAggiornamento = now },
                new BevandaStandard { ArticoloId = 2, PersonalizzazioneId = 1, DimensioneBicchiereId = 2, Prezzo = 6.00m, Disponibile = true, DataCreazione = now, DataAggiornamento = now }
            );

            // Dolci
            _context.Dolce.AddRange(
                new Dolce { ArticoloId = 3, Nome = "Tiramisu", Prezzo = 5.50m, Disponibile = true, DataCreazione = now, DataAggiornamento = now }
            );

            _context.SaveChanges();
        }

        private IPriceCalculationServiceRepository CreatePriceCalculationService(BubbleTeaContext context)
        {
            return new PriceCalculationServiceRepository(
                _memoryCache,
                _mockLogger.Object,
                new BevandaStandardRepository(context),
                new BevandaCustomRepository(context),
                new DolceRepository(context),
                new PersonalizzazioneCustomRepository(context),
                new IngredienteRepository(context),
                new IngredientiPersonalizzazioneRepository(context),
                new DimensioneBicchiereRepository(
                    context,
                    NullLogger<DimensioneBicchiereRepository>.Instance // ✅ Aggiungi questo
                ),
                new TaxRatesRepository(context, NullLogger<TaxRatesRepository>.Instance)
            );
        }

        [Fact]
        public async Task CalculateBevandaStandardPrice_WithValidId_ReturnsPrice()
        {
            // Act
            var result = await _priceCalculationService.CalculateBevandaStandardPrice(1);

            // Assert
            Assert.Equal(4.50m, result);
        }

        [Fact]
        public async Task CalculateBevandaStandardPrice_WithExistingData_ReturnsPrice()
        {
            // Act
            var result = await _priceCalculationService.CalculateBevandaStandardPrice(2);

            // Assert
            Assert.Equal(6.00m, result);
        }

        [Fact]
        public async Task CalculateDolcePrice_WithValidId_ReturnsPrice()
        {
            // Act
            var result = await _priceCalculationService.CalculateDolcePrice(3);

            // Assert
            Assert.Equal(5.50m, result);
        }

        [Fact]
        public async Task CalculateTaxAmount_WithValidInput_ReturnsCorrectTax()
        {
            // Act
            var result = await _priceCalculationService.CalculateTaxAmount(10.00m, 1);

            // Assert
            Assert.Equal(1.80m, result);
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
            var now = DateTime.UtcNow;

            var personalizzazioneCustom = new PersonalizzazioneCustom
            {
                PersCustomId = 1,
                Nome = "Test Custom",
                GradoDolcezza = 3,
                DimensioneBicchiereId = 1,
                DataCreazione = now
            };
            _context.PersonalizzazioneCustom.Add(personalizzazioneCustom);

            var ingredientePersonalizzazione = new IngredientiPersonalizzazione
            {
                IngredientePersId = 1,
                PersCustomId = 1,
                IngredienteId = 1
            };
            _context.IngredientiPersonalizzazione.Add(ingredientePersonalizzazione);

            await _context.SaveChangesAsync();

            // Act
            var result = await _priceCalculationService.CalculateBevandaCustomPrice(1);

            // Assert - Calcolo: PrezzoBase (3.50) + Ingrediente (1.00 * 1.00) = 4.50
            Assert.Equal(4.50m, result);
        }

        [Fact]
        public async Task CalculateImponibile_WithValidInput_ReturnsCorrectImponibile()
        {
            // Act
            var result = await _priceCalculationService.CalculateImponibile(12.20m, 1, 1);

            // Assert
            Assert.Equal(10.00m, result);
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

        [Fact]
        public async Task CalculateDolcePrice_WithInvalidId_ThrowsException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _priceCalculationService.CalculateDolcePrice(999));
        }

        [Fact]
        public async Task ClearCache_ShouldClearMemoryCache()
        {
            // Act & Assert
            await _priceCalculationService.ClearCache();
        }

        //[Fact]
        //public async Task PreloadCache_ShouldPreloadDataWithoutErrors()
        //{
        //    // Act & Assert
        //    await _priceCalculationService.PreloadCache();
        //}

        // ✅ MANTENIAMO SOLO QUESTO TEST ISOLATO COME BACKUP
        [Fact]
        public async Task CalculateBevandaStandardPrice_IsolatedTest_ShouldWork()
        {
            var options = new DbContextOptionsBuilder<BubbleTeaContext>()
                .UseInMemoryDatabase(databaseName: $"Test_Isolated_{Guid.NewGuid()}")
                .Options;

            using var isolatedContext = new BubbleTeaContext(options);
            isolatedContext.Database.EnsureCreated();

            var articolo = new Articolo { ArticoloId = 1, Tipo = "BS" };
            var bevanda = new BevandaStandard { ArticoloId = 1, Prezzo = 3.50m };

            isolatedContext.Articolo.Add(articolo);
            isolatedContext.BevandaStandard.Add(bevanda);
            await isolatedContext.SaveChangesAsync();

            var isolatedService = CreatePriceCalculationService(isolatedContext);

            var result = await isolatedService.CalculateBevandaStandardPrice(1);
            Assert.Equal(3.50m, result);
        }

        [Fact]
        public async Task CalculateOrderItemPrice_WithValidOrderItem_ReturnsCalculation()
        {
            // Arrange
            var orderItem = new OrderItem
            {
                OrderItemId = 1,
                ArticoloId = 1, // Bevanda Standard
                TipoArticolo = "BS",
                Quantita = 2,
                TaxRateId = 1
            };

            // Act
            var result = await _priceCalculationService.CalculateOrderItemPrice(orderItem);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(4.50m, result.PrezzoBase); // Prezzo singolo
            Assert.Equal(9.00m, result.TotaleIvato); // 4.50 * 2
            Assert.Equal(22.00m, result.TaxRate);
            Assert.Equal(1, result.TaxRateId);
        }

        [Fact]
        public async Task ValidateTaxRate_WithValidId_ReturnsTrue()
        {
            // Act
            var result = await _priceCalculationService.ValidateTaxRate(1);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task ValidateTaxRate_WithInvalidId_ReturnsFalse()
        {
            // Act
            var result = await _priceCalculationService.ValidateTaxRate(999); // ID inesistente

            // Assert
            Assert.False(result); // ✅ ORA DOVREBBE RESTITUIRE FALSE
        }

        [Fact]
        public async Task CalculateBatchPricesAsync_WithValidIds_ReturnsBatchResults()
        {
            // Arrange
            var request = new BatchCalculationRequestDTO
            {
                BevandeStandardIds = new List<int> { 1, 2 },
                DolciIds = new List<int> { 3 }
            };

            // Act
            var result = await _priceCalculationService.CalculateBatchPricesAsync(request);

            // Assert
            Assert.NotNull(result);

            // ✅ VERIFICHE ESPLICITE PER OGNI PROPRIETÀ
            Assert.NotNull(result.BevandeStandardPrezzi);
            Assert.NotNull(result.DolciPrezzi);
            Assert.NotNull(result.BevandeCustomPrezzi);
            Assert.NotNull(result.Errori);

            // ✅ VERIFICHE COUNT
            Assert.Equal(2, result.BevandeStandardPrezzi.Count);
            Assert.Single(result.DolciPrezzi); // ✅ SINGLE PER 1 ELEMENTO
            Assert.Empty(result.BevandeCustomPrezzi);
            Assert.Empty(result.Errori);

            // ✅ VERIFICHE VALORI
            Assert.Equal(4.50m, result.BevandeStandardPrezzi[1]);
            Assert.Equal(6.00m, result.BevandeStandardPrezzi[2]);
            Assert.Equal(5.50m, result.DolciPrezzi[3]);
        }

        [Fact]
        public async Task CalculateBatchPricesAsync_WithInvalidIds_ReturnsErrors()
        {
            // Arrange
            var request = new BatchCalculationRequestDTO
            {
                BevandeStandardIds = new List<int> { 999 }, // ID inesistente
                DolciIds = new List<int> { 888 } // ID inesistente
            };

            // Act
            var result = await _priceCalculationService.CalculateBatchPricesAsync(request);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result.BevandeStandardPrezzi);
            Assert.Empty(result.DolciPrezzi);
            Assert.NotEmpty(result.Errori); // Dovrebbe contenere errori
            Assert.True(result.Errori.Count >= 2);
        }

        [Fact]
        public async Task GetTaxRate_WithInvalidId_ReturnsDefault()
        {
            // Act
            var result = await _priceCalculationService.GetTaxRate(999);

            // Assert
            Assert.Equal(22.00m, result); // Default IVA standard
        }
    }
}