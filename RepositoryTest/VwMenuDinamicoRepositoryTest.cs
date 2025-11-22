using Database;
using DTO;
using Microsoft.EntityFrameworkCore;
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
    public class VwMenuDinamicoRepositoryTest : BaseTest
    {
        private readonly Mock<ILogger<VwMenuDinamicoRepository>> _loggerMock;
        private readonly IVwMenuDinamicoRepository _repository;

        public VwMenuDinamicoRepositoryTest()
        {
            _loggerMock = new Mock<ILogger<VwMenuDinamicoRepository>>();
            _repository = new VwMenuDinamicoRepository(_context, _loggerMock.Object);
        }

        [Fact]
        public async Task GetMenuCompletoAsync_ReturnsList()
        {
            // Act
            var result = await _repository.GetMenuCompletoAsync();

            // Assert
            Assert.NotNull(result);
            Assert.IsType<List<VwMenuDinamicoDTO>>(result);
        }

        [Fact]
        public async Task GetPrimoPianoAsync_ReturnsList()
        {
            // Act
            var result = await _repository.GetPrimoPianoAsync(6);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<List<VwMenuDinamicoDTO>>(result);
        }

        [Fact]
        public async Task GetBevandeDisponibiliAsync_ReturnsList()
        {
            // Act
            var result = await _repository.GetBevandeDisponibiliAsync();

            // Assert
            Assert.NotNull(result);
            Assert.IsType<List<VwMenuDinamicoDTO>>(result);
        }

        [Fact]
        public async Task GetBevandePerCategoriaAsync_ReturnsList()
        {
            // Act
            var result = await _repository.GetBevandePerCategoriaAsync("BS");

            // Assert
            Assert.NotNull(result);
            Assert.IsType<List<VwMenuDinamicoDTO>>(result);
        }

        [Fact]
        public async Task GetBevandePerPrioritaAsync_ReturnsList()
        {
            // Act
            var result = await _repository.GetBevandePerPrioritaAsync(1, 5);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<List<VwMenuDinamicoDTO>>(result);
        }

        [Fact]
        public async Task GetBevandeConScontoAsync_ReturnsList()
        {
            // Act
            var result = await _repository.GetBevandeConScontoAsync();

            // Assert
            Assert.NotNull(result);
            Assert.IsType<List<VwMenuDinamicoDTO>>(result);
        }

        [Fact]
        public async Task GetBevandaByIdAsync_WithNonExistingId_ReturnsNull()
        {
            // Act
            var result = await _repository.GetBevandaByIdAsync(999, "BS");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetCategorieDisponibiliAsync_ReturnsList()
        {
            // Act
            var result = await _repository.GetCategorieDisponibiliAsync();

            // Assert
            Assert.NotNull(result);
            Assert.IsType<List<string>>(result);
        }

        [Fact]
        public async Task SearchBevandeAsync_WithNonExistingTerm_ReturnsEmpty()
        {
            // Act
            var result = await _repository.SearchBevandeAsync("NonEsistente");

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetCountBevandeDisponibiliAsync_ReturnsNumber()
        {
            // Act
            var result = await _repository.GetCountBevandeDisponibiliAsync();

            // Assert
            Assert.IsType<int>(result);
            Assert.True(result >= 0);
        }

        [Fact]
        public void Repository_CanBeConstructed()
        {
            // Assert
            Assert.NotNull(_repository);
        }
        
        [Fact]
        public async Task ExistsAsync_WithNonExistingId_ReturnsFalse()
        {
            // Act
            var result = await _repository.ExistsAsync(999, "BS");

            // Assert
            Assert.False(result);
        }        

        [Fact]
        public async Task SearchBevandeAsync_WithEmptyTerm_ReturnsEmpty()
        {
            // Act - SOLO stringhe vuote/whitespace (null non permesso dal metodo)
            var resultEmpty = await _repository.SearchBevandeAsync("");
            var resultWhitespace = await _repository.SearchBevandeAsync("   ");

            // Assert
            Assert.NotNull(resultEmpty);
            Assert.NotNull(resultWhitespace);
            Assert.Empty(resultEmpty);
            Assert.Empty(resultWhitespace);
        }        
    }
}