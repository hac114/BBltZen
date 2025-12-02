using Database;
using DTO;
using Microsoft.EntityFrameworkCore;
using Repository.Service;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace RepositoryTest
{
    public class TaxRatesRepositoryTest : BaseTest
    {
        private readonly TaxRatesRepository _repository;

        public TaxRatesRepositoryTest()
        {
            _repository = new TaxRatesRepository(_context);
            InitializeTestData();
        }

        private void InitializeTestData()
        {
            _context.Database.EnsureDeleted();
            _context.Database.EnsureCreated();

            var taxRates = new List<TaxRates>
            {
                new() {
                    TaxRateId = 1,
                    Aliquota = 22.00m,
                    Descrizione = "IVA Standard",
                    DataCreazione = DateTime.Now.AddDays(-10),
                    DataAggiornamento = DateTime.Now.AddDays(-1)
                },
                new() {
                    TaxRateId = 2,
                    Aliquota = 10.00m,
                    Descrizione = "IVA Ridotta",
                    DataCreazione = DateTime.Now.AddDays(-5),
                    DataAggiornamento = DateTime.Now
                },
                new() {
                    TaxRateId = 3,
                    Aliquota = 4.00m,
                    Descrizione = "IVA Minima",
                    DataCreazione = DateTime.Now.AddDays(-3),
                    DataAggiornamento = DateTime.Now.AddDays(-1)
                }
            };

            _context.TaxRates.AddRange(taxRates);
            _context.SaveChanges();
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnPaginatedTaxRates()
        {
            // Act
            var result = await _repository.GetAllAsync(page: 1, pageSize: 10);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<PaginatedResponseDTO<TaxRatesDTO>>(result);
            Assert.Equal(3, result.TotalCount);
            Assert.Equal(1, result.Page);
            Assert.Equal(10, result.PageSize);
            Assert.True(result.Data.Any());
        }

        [Fact]
        public async Task GetByIdAsync_WithValidId_ShouldReturnTaxRate()
        {
            // Act
            var result = await _repository.GetByIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.TaxRateId);
            Assert.Equal(22.00m, result.Aliquota);
            Assert.Equal("IVA Standard", result.Descrizione);
        }

        [Fact]
        public async Task GetByIdAsync_WithNullId_ShouldReturnNull()
        {
            // Act
            var result = await _repository.GetByIdAsync(null);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetByAliquotaAsync_WithAliquota_ShouldReturnPaginatedResult()
        {
            // Act
            var result = await _repository.GetByAliquotaAsync(10.00m, page: 1, pageSize: 10);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<PaginatedResponseDTO<TaxRatesDTO>>(result);
            Assert.Equal(1, result.TotalCount);
            Assert.Equal(10.00m, result.Data.First().Aliquota);
        }

        [Fact]
        public async Task GetByAliquotaAsync_WithNullAliquota_ShouldReturnAllPaginated()
        {
            // Act
            var result = await _repository.GetByAliquotaAsync(null, page: 1, pageSize: 10);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, result.TotalCount);
            Assert.Equal(3, result.Data.Count());
        }

        [Fact]
        public async Task AddAsync_ShouldAddNewTaxRate()
        {
            // Arrange
            var newTaxRate = new TaxRatesDTO
            {
                Aliquota = 5.00m,
                Descrizione = "IVA Speciale"
            };

            // Act
            var result = await _repository.AddAsync(newTaxRate);

            // Assert
            Assert.True(result.TaxRateId > 0);
            var savedTaxRate = await _repository.GetByIdAsync(result.TaxRateId);
            Assert.NotNull(savedTaxRate);
            Assert.Equal(5.00m, savedTaxRate.Aliquota);
            Assert.Equal("IVA Speciale", savedTaxRate.Descrizione);
            Assert.NotEqual(default(DateTime), savedTaxRate.DataCreazione);
        }

        [Fact]
        public async Task AddAsync_WithDuplicateAliquotaDescrizione_ShouldThrowException()
        {
            // Arrange - Prova ad aggiungere aliquota con stessa combinazione
            var duplicateTaxRate = new TaxRatesDTO
            {
                Aliquota = 22.00m,
                Descrizione = "IVA Standard"
            };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _repository.AddAsync(duplicateTaxRate));
        }

        [Fact]
        public async Task UpdateAsync_ShouldUpdateExistingTaxRate()
        {
            // Arrange
            var updateDto = new TaxRatesDTO
            {
                TaxRateId = 1,
                Aliquota = 25.00m,
                Descrizione = "IVA Standard Aggiornata"
            };

            // Act
            await _repository.UpdateAsync(updateDto);

            // Assert
            var result = await _repository.GetByIdAsync(1);
            Assert.NotNull(result);
            Assert.Equal(25.00m, result.Aliquota);
            Assert.Equal("IVA Standard Aggiornata", result.Descrizione);
        }

        [Fact]
        public async Task DeleteAsync_ShouldRemoveTaxRate()
        {
            // Arrange
            var newTaxRate = new TaxRatesDTO
            {
                Aliquota = 15.00m,
                Descrizione = "IVA Test"
            };
            var addedTaxRate = await _repository.AddAsync(newTaxRate);

            // Act
            await _repository.DeleteAsync(addedTaxRate.TaxRateId);

            // Assert
            var result = await _repository.GetByIdAsync(addedTaxRate.TaxRateId);
            Assert.Null(result); // ✅ SILENT FAIL
        }

        [Fact]
        public async Task DeleteAsync_ShouldNotThrow_ForNonExistingId()
        {
            // Act & Assert - ✅ SILENT FAIL, NO EXCEPTION
            var exception = await Record.ExceptionAsync(() =>
                _repository.DeleteAsync(999)
            );

            Assert.Null(exception);
        }

        [Fact]
        public async Task ExistsAsync_WithExistingId_ShouldReturnTrue()
        {
            // Act
            var result = await _repository.ExistsAsync(1);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task ExistsByAliquotaDescrizioneAsync_WithExistingCombination_ShouldReturnTrue()
        {
            // Act
            var result = await _repository.ExistsByAliquotaDescrizioneAsync(22.00m, "IVA Standard");

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task ExistsByAliquotaDescrizioneAsync_WithExcludeId_ShouldReturnFalse()
        {
            // Act
            var result = await _repository.ExistsByAliquotaDescrizioneAsync(22.00m, "IVA Standard", 1);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task HasDependenciesAsync_WithOrderItems_ShouldReturnTrue()
        {
            // Arrange - Crea dipendenza
            _context.OrderItem.Add(new OrderItem
            {
                OrderItemId = 999,
                TaxRateId = 1,
                OrdineId = 1,
                ArticoloId = 1,
                Quantita = 1,
                PrezzoUnitario = 10.00m,
                TipoArticolo = "BS"
            });
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.HasDependenciesAsync(1);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task GetAllPerFrontendAsync_ShouldReturnPaginatedFrontendDTOs()
        {
            // Act
            var result = await _repository.GetAllPerFrontendAsync(page: 1, pageSize: 10);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<PaginatedResponseDTO<TaxRatesFrontendDTO>>(result);
            Assert.Equal(3, result.TotalCount);
            Assert.Equal("22.00%", result.Data.First(t => t.Aliquota == 22.00m).AliquotaFormattata);
        }

        [Fact]
        public async Task GetByAliquotaPerFrontendAsync_ShouldReturnPaginatedFrontendResult()
        {
            // Act
            var result = await _repository.GetByAliquotaPerFrontendAsync(10.00m, page: 1, pageSize: 10);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.TotalCount);
            Assert.Equal("10.00%", result.Data.First().AliquotaFormattata);
        }
    }
}