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
    public class StatoOrdineRepositoryTest : BaseTest
    {
        private readonly StatoOrdineRepository _repository;
        private readonly BubbleTeaContext _context;

        public StatoOrdineRepositoryTest()
        {
            var options = new DbContextOptionsBuilder<BubbleTeaContext>()
                .UseInMemoryDatabase(databaseName: $"StatoOrdineTest_{Guid.NewGuid()}")
                .Options;

            _context = new BubbleTeaContext(options);
            _repository = new StatoOrdineRepository(_context);

            InitializeTestData();
        }

        private void InitializeTestData()
        {
            _context.Database.EnsureDeleted();
            _context.Database.EnsureCreated();

            var statiOrdine = new List<StatoOrdine>
            {
                new StatoOrdine
                {
                    StatoOrdineId = 1,
                    StatoOrdine1 = "In Coda",
                    Terminale = false
                },
                new StatoOrdine
                {
                    StatoOrdineId = 2,
                    StatoOrdine1 = "In Preparazione",
                    Terminale = false
                },
                new StatoOrdine
                {
                    StatoOrdineId = 3,
                    StatoOrdine1 = "Pronta consegna",
                    Terminale = false
                },
                new StatoOrdine
                {
                    StatoOrdineId = 4,
                    StatoOrdine1 = "Consegnato",
                    Terminale = true
                },
                new StatoOrdine
                {
                    StatoOrdineId = 5,
                    StatoOrdine1 = "Sospeso",
                    Terminale = false
                },
                new StatoOrdine
                {
                    StatoOrdineId = 6,
                    StatoOrdine1 = "Annullato",
                    Terminale = true
                },
                new StatoOrdine
                {
                    StatoOrdineId = 8,
                    StatoOrdine1 = "Bozza",
                    Terminale = false
                },
                new StatoOrdine
                {
                    StatoOrdineId = 9,
                    StatoOrdine1 = "In carrello",
                    Terminale = false
                }
            };

            _context.StatoOrdine.AddRange(statiOrdine);
            _context.SaveChanges();
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnAllStatiOrdine()
        {
            // Act
            var result = await _repository.GetAllAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(8, result.Count());
        }

        [Fact]
        public async Task GetByIdAsync_WithValidId_ShouldReturnStatoOrdine()
        {
            // Act
            var result = await _repository.GetByIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.StatoOrdineId);
            Assert.Equal("In Coda", result.StatoOrdine1);
            Assert.False(result.Terminale);
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
        public async Task GetByNomeAsync_ShouldReturnStatoOrdine()
        {
            // Act
            var result = await _repository.GetByNomeAsync("In Preparazione");

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.StatoOrdineId);
            Assert.Equal("In Preparazione", result.StatoOrdine1);
            Assert.False(result.Terminale);
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
        public async Task GetStatiTerminaliAsync_ShouldReturnOnlyTerminalStates()
        {
            // Act
            var result = await _repository.GetStatiTerminaliAsync();

            // Assert
            var resultList = result.ToList();
            Assert.Equal(2, resultList.Count);
            Assert.All(resultList, s => Assert.True(s.Terminale));
        }

        [Fact]
        public async Task GetStatiNonTerminaliAsync_ShouldReturnOnlyNonTerminalStates()
        {
            // Act
            var result = await _repository.GetStatiNonTerminaliAsync();

            // Assert
            var resultList = result.ToList();
            Assert.Equal(6, resultList.Count);
            Assert.All(resultList, s => Assert.False(s.Terminale));
        }

        [Fact]
        public async Task AddAsync_ShouldAddNewStatoOrdine()
        {
            // Arrange
            var newStatoOrdine = new StatoOrdineDTO
            {
                StatoOrdine1 = "In Consegna",
                Terminale = false
            };

            // Act
            await _repository.AddAsync(newStatoOrdine);

            // Assert
            Assert.True(newStatoOrdine.StatoOrdineId > 0);
            var result = await _repository.GetByIdAsync(newStatoOrdine.StatoOrdineId);
            Assert.NotNull(result);
            Assert.Equal("In Consegna", result.StatoOrdine1);
            Assert.False(result.Terminale);
        }

        [Fact]
        public async Task UpdateAsync_ShouldUpdateExistingStatoOrdine()
        {
            // Arrange
            var updateDto = new StatoOrdineDTO
            {
                StatoOrdineId = 1,
                StatoOrdine1 = "In Attesa Modificato",
                Terminale = true
            };

            // Act
            await _repository.UpdateAsync(updateDto);

            // Assert
            var result = await _repository.GetByIdAsync(1);
            Assert.NotNull(result);
            Assert.Equal("In Attesa Modificato", result.StatoOrdine1);
            Assert.True(result.Terminale);
        }

        [Fact]
        public async Task DeleteAsync_ShouldRemoveStatoOrdine()
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
        public async Task UpdateAsync_WithNonExistingStatoOrdine_ShouldThrowException()
        {
            // Arrange
            var updateDto = new StatoOrdineDTO
            {
                StatoOrdineId = 999,
                StatoOrdine1 = "Test",
                Terminale = false
            };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _repository.UpdateAsync(updateDto));
        }
    }
}
