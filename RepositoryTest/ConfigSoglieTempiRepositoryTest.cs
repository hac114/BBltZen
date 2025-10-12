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
    public class ConfigSoglieTempiRepositoryTest : BaseTest
    {
        private readonly ConfigSoglieTempiRepository _repository;
        private readonly BubbleTeaContext _context;

        public ConfigSoglieTempiRepositoryTest()
        {
            var options = new DbContextOptionsBuilder<BubbleTeaContext>()
                .UseInMemoryDatabase(databaseName: $"ConfigSoglieTempiTest_{Guid.NewGuid()}")
                .Options;

            _context = new BubbleTeaContext(options);
            _repository = new ConfigSoglieTempiRepository(_context);

            InitializeTestData();
        }

        private void InitializeTestData()
        {
            _context.Database.EnsureDeleted();
            _context.Database.EnsureCreated();

            // Crea dati necessari per i test
            var statiOrdine = new List<StatoOrdine>
            {
                new StatoOrdine
                {
                    StatoOrdineId = 1,
                    StatoOrdine1 = "In Attesa",
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
                    StatoOrdine1 = "Pronto",
                    Terminale = false
                }
            };

            var configSoglieTempi = new List<ConfigSoglieTempi>
            {
                new ConfigSoglieTempi
                {
                    SogliaId = 1,
                    StatoOrdineId = 1,
                    SogliaAttenzione = 10,
                    SogliaCritico = 30,
                    DataAggiornamento = DateTime.Now.AddDays(-1),
                    UtenteAggiornamento = "admin"
                },
                new ConfigSoglieTempi
                {
                    SogliaId = 2,
                    StatoOrdineId = 2,
                    SogliaAttenzione = 15,
                    SogliaCritico = 45,
                    DataAggiornamento = DateTime.Now.AddHours(-2),
                    UtenteAggiornamento = "manager"
                },
                new ConfigSoglieTempi
                {
                    SogliaId = 3,
                    StatoOrdineId = 3,
                    SogliaAttenzione = 5,
                    SogliaCritico = 15,
                    DataAggiornamento = DateTime.Now,
                    UtenteAggiornamento = "admin"
                }
            };

            _context.StatoOrdine.AddRange(statiOrdine);
            _context.ConfigSoglieTempi.AddRange(configSoglieTempi);
            _context.SaveChanges();
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnAllConfigSoglieTempi()
        {
            // Act
            var result = await _repository.GetAllAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, result.Count());
        }

        [Fact]
        public async Task GetByIdAsync_WithValidId_ShouldReturnConfigSoglieTempi()
        {
            // Act
            var result = await _repository.GetByIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.SogliaId);
            Assert.Equal(1, result.StatoOrdineId);
            Assert.Equal(10, result.SogliaAttenzione);
            Assert.Equal(30, result.SogliaCritico);
            Assert.Equal("admin", result.UtenteAggiornamento);
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
        public async Task GetByStatoOrdineIdAsync_ShouldReturnConfigSoglieTempi()
        {
            // Act
            var result = await _repository.GetByStatoOrdineIdAsync(2);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.SogliaId);
            Assert.Equal(2, result.StatoOrdineId);
            Assert.Equal(15, result.SogliaAttenzione);
            Assert.Equal(45, result.SogliaCritico);
        }

        [Fact]
        public async Task GetByStatoOrdineIdAsync_WithInvalidStatoOrdineId_ShouldReturnNull()
        {
            // Act
            var result = await _repository.GetByStatoOrdineIdAsync(999);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task AddAsync_ShouldAddNewConfigSoglieTempi()
        {
            // Arrange
            var newConfig = new ConfigSoglieTempiDTO
            {
                StatoOrdineId = 1, // Nota: questo creerà un duplicato per testare la validazione
                SogliaAttenzione = 8,
                SogliaCritico = 25,
                UtenteAggiornamento = "testuser"
            };

            // Act
            await _repository.AddAsync(newConfig);

            // Assert
            Assert.True(newConfig.SogliaId > 0);
            var result = await _repository.GetByIdAsync(newConfig.SogliaId);
            Assert.NotNull(result);
            Assert.Equal(1, result.StatoOrdineId);
            Assert.Equal(8, result.SogliaAttenzione);
            Assert.Equal(25, result.SogliaCritico);
            Assert.Equal("testuser", result.UtenteAggiornamento);
            Assert.NotNull(result.DataAggiornamento);
        }

        [Fact]
        public async Task UpdateAsync_ShouldUpdateExistingConfigSoglieTempi()
        {
            // Arrange
            var updateDto = new ConfigSoglieTempiDTO
            {
                SogliaId = 1,
                StatoOrdineId = 1,
                SogliaAttenzione = 12, // Modificato
                SogliaCritico = 35, // Modificato
                UtenteAggiornamento = "updateduser"
            };

            // Act
            await _repository.UpdateAsync(updateDto);

            // Assert
            var result = await _repository.GetByIdAsync(1);
            Assert.NotNull(result);
            Assert.Equal(12, result.SogliaAttenzione);
            Assert.Equal(35, result.SogliaCritico);
            Assert.Equal("updateduser", result.UtenteAggiornamento);
        }

        [Fact]
        public async Task DeleteAsync_ShouldRemoveConfigSoglieTempi()
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
        public async Task ExistsByStatoOrdineIdAsync_WithExistingStatoOrdineId_ShouldReturnTrue()
        {
            // Act
            var result = await _repository.ExistsByStatoOrdineIdAsync(1);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task ExistsByStatoOrdineIdAsync_WithNonExistingStatoOrdineId_ShouldReturnFalse()
        {
            // Act
            var result = await _repository.ExistsByStatoOrdineIdAsync(999);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task ExistsByStatoOrdineIdAsync_WithExcludeId_ShouldReturnCorrectResult()
        {
            // Act
            var result = await _repository.ExistsByStatoOrdineIdAsync(1, 1);

            // Assert
            Assert.False(result); // Non dovrebbero esserci altre configurazioni per stato ordine 1 oltre all'ID 1
        }

        [Fact]
        public async Task UpdateAsync_WithNonExistingConfigSoglieTempi_ShouldThrowException()
        {
            // Arrange
            var updateDto = new ConfigSoglieTempiDTO
            {
                SogliaId = 999,
                StatoOrdineId = 1,
                SogliaAttenzione = 10,
                SogliaCritico = 30,
                UtenteAggiornamento = "test"
            };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _repository.UpdateAsync(updateDto));
        }
    }
}