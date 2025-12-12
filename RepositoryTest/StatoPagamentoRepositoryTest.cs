using Database.Models;
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
    public class StatoPagamentoRepositoryTest : BaseTest
    {
        private readonly StatoPagamentoRepository _repository;
        private readonly new BubbleTeaContext _context;

        public StatoPagamentoRepositoryTest()
        {
            var options = new DbContextOptionsBuilder<BubbleTeaContext>()
                .UseInMemoryDatabase(databaseName: $"DolceTest_{Guid.NewGuid()}")
                .Options;

            _context = new BubbleTeaContext(options);
            _repository = new StatoPagamentoRepository(_context);

            InitializeTestData();
        }

        private void InitializeTestData()
        {
            _context.Database.EnsureDeleted();
            _context.Database.EnsureCreated();

            var statiPagamento = new List<StatoPagamento>
            {
                new StatoPagamento
                {
                    StatoPagamentoId = 1,
                    StatoPagamento1 = "Pendente"
                },
                new StatoPagamento
                {
                    StatoPagamentoId = 2,
                    StatoPagamento1 = "Completato"
                },
                new StatoPagamento
                {
                    StatoPagamentoId = 3,
                    StatoPagamento1 = "Fallito"
                },
                new StatoPagamento
                {
                    StatoPagamentoId = 4,
                    StatoPagamento1 = "Rimborsato"
                },
                new StatoPagamento
                {
                    StatoPagamentoId = 5,
                    StatoPagamento1 = "Non Richiesto"
                }
            };

            _context.StatoPagamento.AddRange(statiPagamento);
            _context.SaveChanges();
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnAllStatiPagamento()
        {
            // Act
            var result = await _repository.GetAllAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(5, result.Count());
        }

        [Fact]
        public async Task GetByIdAsync_WithValidId_ShouldReturnStatoPagamento()
        {
            // Act
            var result = await _repository.GetByIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.StatoPagamentoId);
            Assert.Equal("Pendente", result.StatoPagamento1);
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
        public async Task GetByNomeAsync_ShouldReturnStatoPagamento()
        {
            // Act
            var result = await _repository.GetByNomeAsync("Completato");

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.StatoPagamentoId);
            Assert.Equal("Completato", result.StatoPagamento1);
        }

        [Fact]
        public async Task GetByNomeAsync_WithInvalidNome_ShouldReturnNull()
        {
            // Act
            var result = await _repository.GetByNomeAsync("Stato Inesistente");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task AddAsync_ShouldAddNewStatoPagamento()
        {
            // Arrange
            var newStatoPagamento = new StatoPagamentoDTO
            {
                StatoPagamento1 = "Annullato"
            };

            // Act
            await _repository.AddAsync(newStatoPagamento);

            // Assert
            Assert.True(newStatoPagamento.StatoPagamentoId > 0);
            var result = await _repository.GetByIdAsync(newStatoPagamento.StatoPagamentoId);
            Assert.NotNull(result);
            Assert.Equal("Annullato", result.StatoPagamento1);
        }

        [Fact]
        public async Task UpdateAsync_ShouldUpdateExistingStatoPagamento()
        {
            // Arrange
            var updateDto = new StatoPagamentoDTO
            {
                StatoPagamentoId = 1,
                StatoPagamento1 = "In Attesa Modificato"
            };

            // Act
            await _repository.UpdateAsync(updateDto);

            // Assert
            var result = await _repository.GetByIdAsync(1);
            Assert.NotNull(result);
            Assert.Equal("In Attesa Modificato", result.StatoPagamento1);
        }

        [Fact]
        public async Task DeleteAsync_ShouldRemoveStatoPagamento()
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
        public async Task UpdateAsync_WithNonExistingStatoPagamento_ShouldThrowException()
        {
            // Arrange
            var updateDto = new StatoPagamentoDTO
            {
                StatoPagamentoId = 999,
                StatoPagamento1 = "Test"
            };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _repository.UpdateAsync(updateDto));
        }
    }
}
