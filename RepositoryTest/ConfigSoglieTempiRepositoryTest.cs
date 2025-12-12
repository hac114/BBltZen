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
    public class ConfigSoglieTempiRepositoryTest : BaseTest
    {
        private readonly ConfigSoglieTempiRepository _repository;        

        public ConfigSoglieTempiRepositoryTest()
        {            
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
                StatoOrdineId = 4, // ✅ CAMBIATO: usa stato ordine non esistente
                SogliaAttenzione = 8,
                SogliaCritico = 25,
                UtenteAggiornamento = "testuser"
            };

            // Act
            var result = await _repository.AddAsync(newConfig); // ✅ USA IL RISULTATO

            // Assert
            Assert.True(result.SogliaId > 0); // ✅ VERIFICA ID GENERATO
            Assert.Equal(4, result.StatoOrdineId);
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
        public async Task UpdateAsync_WithNonExistingId_ShouldNotThrow()
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

            // Act & Assert - ✅ NO EXCEPTION (silent fail pattern)
            var exception = await Record.ExceptionAsync(() => _repository.UpdateAsync(updateDto));
            Assert.Null(exception);
        }

        [Fact]
        public async Task AddAsync_ShouldReturnDtoWithGeneratedId()
        {
            // Arrange
            var newConfig = new ConfigSoglieTempiDTO
            {
                StatoOrdineId = 4, // Nuovo stato ordine
                SogliaAttenzione = 8,
                SogliaCritico = 25,
                UtenteAggiornamento = "testuser"
            };

            // Act
            var result = await _repository.AddAsync(newConfig);

            // Assert
            Assert.True(result.SogliaId > 0);
            Assert.Equal(4, result.StatoOrdineId);
            Assert.Equal(8, result.SogliaAttenzione);
            Assert.Equal(25, result.SogliaCritico);
        }

        [Fact]
        public async Task AddAsync_WithDuplicateStatoOrdine_ShouldThrowArgumentException()
        {
            // Arrange
            var newConfig = new ConfigSoglieTempiDTO
            {
                StatoOrdineId = 1, // Già esistente
                SogliaAttenzione = 8,
                SogliaCritico = 25,
                UtenteAggiornamento = "testuser"
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => _repository.AddAsync(newConfig));
            Assert.Contains("Esiste già una configurazione", exception.Message);
        }

        [Fact]
        public async Task AddAsync_WithInvalidSoglie_ShouldThrowArgumentException()
        {
            // Arrange
            var newConfig = new ConfigSoglieTempiDTO
            {
                StatoOrdineId = 4,
                SogliaAttenzione = 30, // Maggiore di critico - INVALIDO
                SogliaCritico = 25,
                UtenteAggiornamento = "testuser"
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => _repository.AddAsync(newConfig));
            Assert.Contains("La soglia critica deve essere maggiore", exception.Message); // ✅ MESSAGGIO CAMBIATO
        }

        [Fact]
        public async Task UpdateAsync_WithNonExistingId_ShouldSilentFail()
        {
            // Arrange
            var updateDto = new ConfigSoglieTempiDTO
            {
                SogliaId = 999,
                StatoOrdineId = 1,
                SogliaAttenzione = 12,
                SogliaCritico = 35,
                UtenteAggiornamento = "test"
            };

            // Act & Assert - ✅ NO EXCEPTION (silent fail)
            var exception = await Record.ExceptionAsync(() => _repository.UpdateAsync(updateDto));
            Assert.Null(exception);
        }

        [Fact]
        public async Task UpdateAsync_WithDuplicateStatoOrdine_ShouldThrowArgumentException()
        {
            // Arrange
            var updateDto = new ConfigSoglieTempiDTO
            {
                SogliaId = 1, // Config esistente per stato ordine 1
                StatoOrdineId = 2, // Cambia a stato ordine 2 che è già usato da sogliaId 2
                SogliaAttenzione = 12,
                SogliaCritico = 35,
                UtenteAggiornamento = "test"
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => _repository.UpdateAsync(updateDto));
            Assert.Contains("Esiste già un'altra configurazione", exception.Message); // ✅ MESSAGGIO CAMBIATO
        }

        [Fact]
        public async Task GetSoglieByStatiOrdineAsync_ShouldReturnDictionary()
        {
            // Arrange
            var statiOrdineIds = new List<int> { 1, 2 };

            // Act
            var result = await _repository.GetSoglieByStatiOrdineAsync(statiOrdineIds);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Contains(1, result.Keys);
            Assert.Contains(2, result.Keys);
            Assert.Equal(10, result[1].SogliaAttenzione);
            Assert.Equal(15, result[2].SogliaAttenzione);
        }

        [Fact]
        public async Task GetSoglieByStatiOrdineAsync_WithNonExistingStati_ShouldReturnEmptyDictionary()
        {
            // Arrange
            var statiOrdineIds = new List<int> { 999, 1000 };

            // Act
            var result = await _repository.GetSoglieByStatiOrdineAsync(statiOrdineIds);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task ValidateSoglieAsync_WithValidSoglie_ShouldReturnTrue()
        {
            // Act
            var result = await _repository.ValidateSoglieAsync(10, 30);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task ValidateSoglieAsync_WithInvalidSoglie_ShouldReturnFalse()
        {
            // Act
            var result = await _repository.ValidateSoglieAsync(30, 10); // Critico < Attenzione

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task ValidateSoglieAsync_WithNegativeSoglie_ShouldReturnFalse()
        {
            // Act
            var result = await _repository.ValidateSoglieAsync(-5, 10);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task ValidateConfigSoglieAsync_WithValidConfig_ShouldReturnValid()
        {
            // Arrange
            var validConfig = new ConfigSoglieTempiDTO
            {
                StatoOrdineId = 4,
                SogliaAttenzione = 10,
                SogliaCritico = 30,
                UtenteAggiornamento = "testuser"
            };

            // Act
            var result = await _repository.ValidateConfigSoglieAsync(validConfig);

            // Assert
            Assert.True(result.IsValid);
            Assert.Null(result.ErrorMessage);
        }

        [Fact]
        public async Task ValidateConfigSoglieAsync_WithInvalidSoglie_ShouldReturnError()
        {
            // Arrange
            var invalidConfig = new ConfigSoglieTempiDTO
            {
                StatoOrdineId = 4,
                SogliaAttenzione = 30, // INVALIDO: maggiore di critico
                SogliaCritico = 25,
                UtenteAggiornamento = "testuser"
            };

            // Act
            var result = await _repository.ValidateConfigSoglieAsync(invalidConfig);

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains("La soglia critica deve essere maggiore", result.ErrorMessage);
        }

        [Fact]
        public async Task ValidateConfigSoglieAsync_WithDuplicateStatoOrdine_ShouldReturnError()
        {
            // Arrange
            var duplicateConfig = new ConfigSoglieTempiDTO
            {
                StatoOrdineId = 1, // Già esistente
                SogliaAttenzione = 10,
                SogliaCritico = 30,
                UtenteAggiornamento = "testuser"
            };

            // Act
            var result = await _repository.ValidateConfigSoglieAsync(duplicateConfig);

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains("Esiste già una configurazione", result.ErrorMessage);
        }
    }
}