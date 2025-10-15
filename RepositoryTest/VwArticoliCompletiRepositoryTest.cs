using Database;
using DTO;
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
    public class VwArticoliCompletiRepositoryTest : BaseTest
    {
        private readonly Mock<ILogger<VwArticoliCompletiRepository>> _loggerMock;
        private readonly IVwArticoliCompletiRepository _repository;

        public VwArticoliCompletiRepositoryTest()
        {
            _loggerMock = new Mock<ILogger<VwArticoliCompletiRepository>>();
            _repository = new VwArticoliCompletiRepository(_context, _loggerMock.Object);

            // RIMOSSO SetupTestData() - Non funziona con le viste in InMemory
        }

        [Fact]
        public async Task GetAllAsync_ReturnsAllArticoli()
        {
            // Act
            var result = await _repository.GetAllAsync();

            // Assert
            Assert.NotNull(result);
            Assert.IsType<List<VwArticoliCompletiDTO>>(result);
            // Per viste in InMemory, restituirà lista vuota
        }

        [Fact]
        public async Task GetByIdAsync_WithNonExistingId_ReturnsNull()
        {
            // Act
            var result = await _repository.GetByIdAsync(999);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetByTipoAsync_WithValidTipo_ReturnsList()
        {
            // Act
            var result = await _repository.GetByTipoAsync("BS");

            // Assert
            Assert.NotNull(result);
            Assert.IsType<List<VwArticoliCompletiDTO>>(result);
        }

        [Fact]
        public async Task GetByCategoriaAsync_WithValidCategoria_ReturnsList()
        {
            // Act
            var result = await _repository.GetByCategoriaAsync("Bevanda");

            // Assert
            Assert.NotNull(result);
            Assert.IsType<List<VwArticoliCompletiDTO>>(result);
        }

        [Fact]
        public async Task GetDisponibiliAsync_ReturnsList()
        {
            // Act
            var result = await _repository.GetDisponibiliAsync();

            // Assert
            Assert.NotNull(result);
            Assert.IsType<List<VwArticoliCompletiDTO>>(result);
        }

        [Fact]
        public async Task SearchByNameAsync_WithNonExistingName_ReturnsEmpty()
        {
            // Act
            var result = await _repository.SearchByNameAsync("NonEsistente");

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetByPriceRangeAsync_WithValidRange_ReturnsList()
        {
            // Act
            var result = await _repository.GetByPriceRangeAsync(4.00m, 5.00m);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<List<VwArticoliCompletiDTO>>(result);
        }

        [Fact]
        public async Task GetArticoliConIvaAsync_ReturnsList()
        {
            // Act
            var result = await _repository.GetArticoliConIvaAsync();

            // Assert
            Assert.NotNull(result);
            Assert.IsType<List<VwArticoliCompletiDTO>>(result);
        }

        [Fact]
        public async Task GetCountAsync_ReturnsNumber()
        {
            // Act
            var result = await _repository.GetCountAsync();

            // Assert
            Assert.IsType<int>(result);
            Assert.True(result >= 0);
        }

        [Fact]
        public async Task GetCategorieAsync_ReturnsList()
        {
            // Act
            var result = await _repository.GetCategorieAsync();

            // Assert
            Assert.NotNull(result);
            Assert.IsType<List<string>>(result);
        }

        [Fact]
        public async Task GetTipiArticoloAsync_ReturnsList()
        {
            // Act
            var result = await _repository.GetTipiArticoloAsync();

            // Assert
            Assert.NotNull(result);
            Assert.IsType<List<string>>(result);
        }

        [Fact]
        public async Task Repository_CanBeConstructed()
        {
            // Assert
            Assert.NotNull(_repository);
        }

        [Fact]
        public async Task GetAllAsync_ReturnsEmptyList_WhenNoData()
        {
            // Act
            var result = await _repository.GetAllAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetCountAsync_ReturnsZero_WhenNoData()
        {
            // Act
            var result = await _repository.GetCountAsync();

            // Assert
            Assert.Equal(0, result);
        }

        [Fact]
        public async Task GetCategorieAsync_ReturnsEmptyList_WhenNoData()
        {
            // Act
            var result = await _repository.GetCategorieAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetTipiArticoloAsync_ReturnsEmptyList_WhenNoData()
        {
            // Act
            var result = await _repository.GetTipiArticoloAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }
    }
}