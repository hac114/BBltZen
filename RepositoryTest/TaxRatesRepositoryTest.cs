using Database;
using DTO;
using Microsoft.EntityFrameworkCore;
using Repository.Service;
using System;
using System.Collections.Generic;
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
                new TaxRates
                {
                    TaxRateId = 1,
                    Aliquota = 22.00m,
                    Descrizione = "IVA Standard",
                    DataCreazione = DateTime.Now.AddDays(-10),
                    DataAggiornamento = DateTime.Now.AddDays(-1)
                },
                new TaxRates
                {
                    TaxRateId = 2,
                    Aliquota = 10.00m,
                    Descrizione = "IVA Ridotta",
                    DataCreazione = DateTime.Now.AddDays(-5),
                    DataAggiornamento = DateTime.Now
                },
                new TaxRates
                {
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
        public async Task GetAllAsync_ShouldReturnAllTaxRates()
        {
            // Act
            var result = await _repository.GetAllAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, result.Count());
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
        public async Task GetByIdAsync_WithInvalidId_ShouldReturnNull()
        {
            // Act
            var result = await _repository.GetByIdAsync(999);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetByAliquotaAsync_ShouldReturnTaxRate()
        {
            // Act
            var result = await _repository.GetByAliquotaAsync(10.00m);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.TaxRateId);
            Assert.Equal(10.00m, result.Aliquota);
            Assert.Equal("IVA Ridotta", result.Descrizione);
        }

        [Fact]
        public async Task GetByAliquotaAsync_WithInvalidAliquota_ShouldReturnNull()
        {
            // Act
            var result = await _repository.GetByAliquotaAsync(99.99m);

            // Assert
            Assert.Null(result);
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
            var result = await _repository.AddAsync(newTaxRate); // ✅ CORREGGI: assegna risultato

            // Assert
            Assert.True(result.TaxRateId > 0); // ✅ USA result
            var savedTaxRate = await _repository.GetByIdAsync(result.TaxRateId); // ✅ USA result
            Assert.NotNull(savedTaxRate);
            Assert.Equal(5.00m, savedTaxRate.Aliquota);
            Assert.Equal("IVA Speciale", savedTaxRate.Descrizione);
            Assert.NotNull(savedTaxRate.DataCreazione);
            Assert.NotNull(savedTaxRate.DataAggiornamento);
        }

        [Fact]
        public async Task UpdateAsync_ShouldUpdateExistingTaxRate()
        {
            // Arrange
            var updateDto = new TaxRatesDTO
            {
                TaxRateId = 1,
                Aliquota = 25.00m, // Aliquota modificata
                Descrizione = "IVA Standard Aggiornata" // Descrizione modificata
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
            // Act
            await _repository.DeleteAsync(1);

            // Assert
            var result = await _repository.GetByIdAsync(1);
            Assert.Null(result);
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
        public async Task ExistsAsync_WithNonExistingId_ShouldReturnFalse()
        {
            // Act
            var result = await _repository.ExistsAsync(999);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task ExistsByAliquotaAsync_WithExistingAliquota_ShouldReturnTrue()
        {
            // Act
            var result = await _repository.ExistsByAliquotaAsync(22.00m);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task ExistsByAliquotaAsync_WithNonExistingAliquota_ShouldReturnFalse()
        {
            // Act
            var result = await _repository.ExistsByAliquotaAsync(99.99m);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task ExistsByAliquotaAsync_WithExcludeId_ShouldReturnCorrectResult()
        {
            // Act
            var result = await _repository.ExistsByAliquotaAsync(22.00m, 1);

            // Assert
            Assert.False(result); // Non dovrebbero esserci altre aliquote al 22% oltre all'ID 1
        }

        [Fact]
        public async Task UpdateAsync_WithNonExistingTaxRate_ShouldThrowException()
        {
            // Arrange
            var updateDto = new TaxRatesDTO
            {
                TaxRateId = 999,
                Aliquota = 25.00m,
                Descrizione = "Test"
            };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _repository.UpdateAsync(updateDto));
        }
    }
}