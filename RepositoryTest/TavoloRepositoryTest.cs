using DTO;
using Repository.Interface;
using Repository.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace RepositoryTest
{
    public class TavoloRepositoryTest : BaseTest
    {
        private readonly ITavoloRepository _tavoloRepository;

        public TavoloRepositoryTest()
        {
            // ✅ CREA IL REPOSITORY SPECIFICO USANDO IL CONTEXT EREDITATO
            _tavoloRepository = new TavoloRepository(_context);
        }

        [Fact]
        public async Task AddAsync_ShouldAddNewTavolo()
        {
            // Arrange
            var newTavolo = new TavoloDTO
            {
                Numero = 1,
                Zona = "Terrazza",
                Disponibile = true
            };

            // Act
            var result = await _tavoloRepository.AddAsync(newTavolo); // ✅ USA IL RISULTATO

            // Assert
            Assert.True(result.TavoloId > 0); // ✅ VERIFICA ID GENERATO
            Assert.Equal(1, result.Numero);
            Assert.Equal("Terrazza", result.Zona);
            Assert.True(result.Disponibile);
        }

        [Fact]
        public async Task GetByIdAsync_WithValidId_ShouldReturnTavolo()
        {
            // Arrange
            var newTavolo = new TavoloDTO
            {
                Numero = 2,
                Zona = "Interno",
                Disponibile = true
            };
            var addedTavolo = await _tavoloRepository.AddAsync(newTavolo); // ✅ USA RISULTATO

            // Act
            var result = await _tavoloRepository.GetByIdAsync(addedTavolo.TavoloId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(addedTavolo.TavoloId, result.TavoloId);
            Assert.Equal(2, result.Numero);
            Assert.Equal("Interno", result.Zona);
            Assert.True(result.Disponibile);
        }

        [Fact]
        public async Task GetByIdAsync_WithInvalidId_ShouldReturnNull()
        {
            // Act
            var result = await _tavoloRepository.GetByIdAsync(999);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetByNumeroAsync_WithValidNumero_ShouldReturnTavolo()
        {
            // Arrange
            var newTavolo = new TavoloDTO
            {
                Numero = 3,
                Zona = "Terrazza",
                Disponibile = true
            };
            await _tavoloRepository.AddAsync(newTavolo);

            // Act
            var result = await _tavoloRepository.GetByNumeroAsync(3);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, result.Numero);
            Assert.Equal("Terrazza", result.Zona);
            Assert.True(result.Disponibile);
        }

        [Fact]
        public async Task GetByNumeroAsync_WithInvalidNumero_ShouldReturnNull()
        {
            // Act
            var result = await _tavoloRepository.GetByNumeroAsync(999);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetDisponibiliAsync_ShouldReturnOnlyAvailableTables()
        {
            // Arrange
            var tavoli = new List<TavoloDTO>
            {
                new TavoloDTO { Numero = 4, Zona = "Interno", Disponibile = true },
                new TavoloDTO { Numero = 5, Zona = "Terrazza", Disponibile = false },
                new TavoloDTO { Numero = 6, Zona = "Interno", Disponibile = true }
            };

            foreach (var tavolo in tavoli)
            {
                await _tavoloRepository.AddAsync(tavolo);
            }

            // Act
            var result = await _tavoloRepository.GetDisponibiliAsync();

            // Assert
            var resultList = result.ToList();
            Assert.Equal(2, resultList.Count);
            Assert.All(resultList, t => Assert.True(t.Disponibile));
        }

        [Fact]
        public async Task GetByZonaAsync_ShouldReturnFilteredResults()
        {
            // Arrange
            var tavoli = new List<TavoloDTO>
            {
                new TavoloDTO { Numero = 7, Zona = "Terrazza", Disponibile = true },
                new TavoloDTO { Numero = 8, Zona = "Interno", Disponibile = true },
                new TavoloDTO { Numero = 9, Zona = "Terrazza", Disponibile = false }
            };

            foreach (var tavolo in tavoli)
            {
                await _tavoloRepository.AddAsync(tavolo);
            }

            // Act
            var result = await _tavoloRepository.GetByZonaAsync("Terrazza");

            // Assert
            var resultList = result.ToList();
            Assert.Equal(2, resultList.Count);
            Assert.All(resultList, t => Assert.Equal("Terrazza", t.Zona));
        }

        [Fact]
        public async Task UpdateAsync_ShouldUpdateExistingTavolo()
        {
            // Arrange
            var newTavolo = new TavoloDTO
            {
                Numero = 10,
                Zona = "Interno",
                Disponibile = true
            };
            var addedTavolo = await _tavoloRepository.AddAsync(newTavolo);

            var updateDto = new TavoloDTO
            {
                TavoloId = addedTavolo.TavoloId,
                Numero = 20,
                Zona = "Terrazza",
                Disponibile = false
            };

            // Act
            await _tavoloRepository.UpdateAsync(updateDto);

            // Assert
            var result = await _tavoloRepository.GetByIdAsync(addedTavolo.TavoloId);
            Assert.NotNull(result);
            Assert.Equal(20, result.Numero);
            Assert.Equal("Terrazza", result.Zona);
            Assert.False(result.Disponibile);
        }

        [Fact]
        public async Task UpdateAsync_ShouldNotThrow_ForNonExistingId()
        {
            // Arrange
            var updateDto = new TavoloDTO
            {
                TavoloId = 999,
                Numero = 99,
                Zona = "Interno",
                Disponibile = true
            };

            // Act & Assert - ✅ SILENT FAIL, NO EXCEPTION
            var exception = await Record.ExceptionAsync(() =>
                _tavoloRepository.UpdateAsync(updateDto)
            );

            Assert.Null(exception);
        }

        [Fact]
        public async Task DeleteAsync_ShouldRemoveTavolo()
        {
            // Arrange
            var newTavolo = new TavoloDTO
            {
                Numero = 11,
                Zona = "Interno",
                Disponibile = true
            };
            var addedTavolo = await _tavoloRepository.AddAsync(newTavolo);

            // Act
            await _tavoloRepository.DeleteAsync(addedTavolo.TavoloId);

            // Assert
            var result = await _tavoloRepository.GetByIdAsync(addedTavolo.TavoloId);
            Assert.Null(result);
        }

        [Fact]
        public async Task DeleteAsync_ShouldNotThrow_ForNonExistingId()
        {
            // Act & Assert - ✅ SILENT FAIL, NO EXCEPTION
            var exception = await Record.ExceptionAsync(() =>
                _tavoloRepository.DeleteAsync(999)
            );

            Assert.Null(exception);
        }

        [Fact]
        public async Task ExistsAsync_WithExistingId_ShouldReturnTrue()
        {
            // Arrange
            var newTavolo = new TavoloDTO
            {
                Numero = 12,
                Zona = "Interno",
                Disponibile = true
            };
            var addedTavolo = await _tavoloRepository.AddAsync(newTavolo);

            // Act
            var result = await _tavoloRepository.ExistsAsync(addedTavolo.TavoloId);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task ExistsAsync_WithNonExistingId_ShouldReturnFalse()
        {
            // Act
            var result = await _tavoloRepository.ExistsAsync(999);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task NumeroExistsAsync_WithExistingNumero_ShouldReturnTrue()
        {
            // Arrange
            var newTavolo = new TavoloDTO
            {
                Numero = 13,
                Zona = "Interno",
                Disponibile = true
            };
            await _tavoloRepository.AddAsync(newTavolo);

            // Act
            var result = await _tavoloRepository.NumeroExistsAsync(13);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task NumeroExistsAsync_WithNonExistingNumero_ShouldReturnFalse()
        {
            // Act
            var result = await _tavoloRepository.NumeroExistsAsync(999);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task NumeroExistsAsync_WithExcludeId_ShouldExcludeCurrentRecord()
        {
            // Arrange
            var newTavolo = new TavoloDTO
            {
                Numero = 14,
                Zona = "Interno",
                Disponibile = true
            };
            var addedTavolo = await _tavoloRepository.AddAsync(newTavolo);

            // Act - Cerca numero 14 escludendo il record corrente
            var result = await _tavoloRepository.NumeroExistsAsync(14, addedTavolo.TavoloId);

            // Assert - Non dovrebbe trovare altri record con numero 14
            Assert.False(result);
        }

        [Fact]
        public async Task AddAsync_ShouldThrow_ForDuplicateNumero()
        {
            // Arrange
            var tavolo1 = new TavoloDTO
            {
                Numero = 15,
                Zona = "Interno",
                Disponibile = true
            };
            await _tavoloRepository.AddAsync(tavolo1);

            var tavolo2 = new TavoloDTO
            {
                Numero = 15, // ✅ STESSO NUMERO
                Zona = "Terrazza",
                Disponibile = false
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
                _tavoloRepository.AddAsync(tavolo2)
            );

            Assert.Contains("esiste già", exception.Message.ToLower());
        }

        [Fact]
        public async Task UpdateAsync_ShouldThrow_ForDuplicateNumero()
        {
            // Arrange - Crea due tavoli con numeri diversi
            var tavolo1 = new TavoloDTO
            {
                Numero = 16,
                Zona = "Interno",
                Disponibile = true
            };
            var addedTavolo1 = await _tavoloRepository.AddAsync(tavolo1);

            var tavolo2 = new TavoloDTO
            {
                Numero = 17,
                Zona = "Terrazza",
                Disponibile = true
            };
            var addedTavolo2 = await _tavoloRepository.AddAsync(tavolo2);

            // Act & Assert - Prova a fare l'update del secondo tavolo con lo stesso numero del primo
            var updateDto = new TavoloDTO
            {
                TavoloId = addedTavolo2.TavoloId,
                Numero = 16, // ✅ STESSO NUMERO DEL PRIMO TAVOLO
                Zona = "Terrazza",
                Disponibile = true
            };

            var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
                _tavoloRepository.UpdateAsync(updateDto)
            );

            Assert.Contains("esiste già", exception.Message.ToLower());
        }
    }
}