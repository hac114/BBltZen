using Database.Models;
using DTO;
using Microsoft.Extensions.Logging;
using Moq;
using Repository.Interface;
using Repository.Service;
using System.Collections.Generic;
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
        }

        [Fact]
        public void Repository_CanBeConstructed()
        {
            // Assert
            Assert.NotNull(_repository);
        }

        [Fact]
        public async Task GetAllAsync_ReturnsIEnumerable()
        {
            // Act
            var result = await _repository.GetAllAsync();

            // Assert
            Assert.NotNull(result);
            Assert.IsAssignableFrom<IEnumerable<VwArticoliCompletiDTO>>(result);
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
        public async Task ExistsAsync_WithNonExistingId_ReturnsFalse()
        {
            // Act
            var result = await _repository.ExistsAsync(999);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task GetByTipoAsync_WithValidTipo_ReturnsIEnumerable()
        {
            // Act
            var result = await _repository.GetByTipoAsync("BS");

            // Assert
            Assert.NotNull(result);
            Assert.IsAssignableFrom<IEnumerable<VwArticoliCompletiDTO>>(result);
        }

        [Fact]
        public async Task GetByCategoriaAsync_WithValidCategoria_ReturnsIEnumerable()
        {
            // Act
            var result = await _repository.GetByCategoriaAsync("Bevanda");

            // Assert
            Assert.NotNull(result);
            Assert.IsAssignableFrom<IEnumerable<VwArticoliCompletiDTO>>(result);
        }

        [Fact]
        public async Task GetDisponibiliAsync_ReturnsIEnumerable()
        {
            // Act
            var result = await _repository.GetDisponibiliAsync();

            // Assert
            Assert.NotNull(result);
            Assert.IsAssignableFrom<IEnumerable<VwArticoliCompletiDTO>>(result);
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
        public async Task SearchByNameAsync_WithEmptyString_ReturnsEmpty()
        {
            // Act
            var result = await _repository.SearchByNameAsync("");

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetByPriceRangeAsync_WithValidRange_ReturnsIEnumerable()
        {
            // Act
            var result = await _repository.GetByPriceRangeAsync(4.00m, 5.00m);

            // Assert
            Assert.NotNull(result);
            Assert.IsAssignableFrom<IEnumerable<VwArticoliCompletiDTO>>(result);
        }

        [Fact]
        public async Task GetByPriceRangeAsync_WithInvalidRange_ReturnsEmpty()
        {
            // Act
            var result = await _repository.GetByPriceRangeAsync(1000m, 2000m);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetArticoliConIvaAsync_ReturnsIEnumerable()
        {
            // Act
            var result = await _repository.GetArticoliConIvaAsync();

            // Assert
            Assert.NotNull(result);
            Assert.IsAssignableFrom<IEnumerable<VwArticoliCompletiDTO>>(result);
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
        public async Task GetCategorieAsync_ReturnsIEnumerable()
        {
            // Act
            var result = await _repository.GetCategorieAsync();

            // Assert
            Assert.NotNull(result);
            Assert.IsAssignableFrom<IEnumerable<string>>(result);
        }

        [Fact]
        public async Task GetTipiArticoloAsync_ReturnsIEnumerable()
        {
            // Act
            var result = await _repository.GetTipiArticoloAsync();

            // Assert
            Assert.NotNull(result);
            Assert.IsAssignableFrom<IEnumerable<string>>(result);
        }
    }
}