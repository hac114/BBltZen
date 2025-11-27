using Database;
using DTO;
using Microsoft.EntityFrameworkCore;
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

        // ✅ NUOVI TEST PER METODI FRONTEND
        [Fact]
        public async Task GetAllPerFrontendAsync_ShouldReturnFrontendDTOs()
        {
            // Arrange
            var tavoli = new List<TavoloDTO>
            {
                new TavoloDTO { Numero = 21, Zona = "Interno", Disponibile = true },
                new TavoloDTO { Numero = 22, Zona = "Terrazza", Disponibile = false }
            };

            foreach (var tavolo in tavoli)
            {
                await _tavoloRepository.AddAsync(tavolo);
            }

            // Act
            var result = await _tavoloRepository.GetAllPerFrontendAsync();

            // Assert
            var resultList = result.ToList();
            Assert.Equal(2, resultList.Count);

            // ✅ VERIFICA FORMATTAZIONE FRONTEND
            var tavolo1 = resultList.First(t => t.Numero == 21);
            Assert.Equal("SI", tavolo1.Disponibile);
            Assert.Equal("INTERNO", tavolo1.Zona);

            var tavolo2 = resultList.First(t => t.Numero == 22);
            Assert.Equal("NO", tavolo2.Disponibile);
            Assert.Equal("TERRAZZA", tavolo2.Zona);

            // ✅ VERIFICA CHE NON CI SIA L'ID
            Assert.All(resultList, t => Assert.Null(t.GetType().GetProperty("TavoloId")));
        }

        [Fact]
        public async Task GetDisponibiliPerFrontendAsync_ShouldReturnOnlyAvailableTablesFormatted()
        {
            // Arrange
            var tavoli = new List<TavoloDTO>
            {
                new TavoloDTO { Numero = 23, Zona = "Interno", Disponibile = true },
                new TavoloDTO { Numero = 24, Zona = "Terrazza", Disponibile = false },
                new TavoloDTO { Numero = 25, Zona = "Interno", Disponibile = true }
            };

            foreach (var tavolo in tavoli)
            {
                await _tavoloRepository.AddAsync(tavolo);
            }

            // Act
            var result = await _tavoloRepository.GetDisponibiliPerFrontendAsync();

            // Assert
            var resultList = result.ToList();
            Assert.Equal(2, resultList.Count);
            Assert.All(resultList, t => Assert.Equal("SI", t.Disponibile));
            Assert.All(resultList, t => Assert.NotNull(t.Zona));
        }

        [Fact]
        public async Task GetByZonaPerFrontendAsync_ShouldReturnFilteredAndFormattedResults()
        {
            // Arrange
            var tavoli = new List<TavoloDTO>
            {
                new TavoloDTO { Numero = 26, Zona = "Terrazza", Disponibile = true },
                new TavoloDTO { Numero = 27, Zona = "Interno", Disponibile = true },
                new TavoloDTO { Numero = 28, Zona = "terrazza", Disponibile = false } // ✅ lowercase
            };

            foreach (var tavolo in tavoli)
            {
                await _tavoloRepository.AddAsync(tavolo);
            }

            // Act - Cerca con case insensitive
            var result = await _tavoloRepository.GetByZonaPerFrontendAsync("Terrazza");

            // Assert
            var resultList = result.ToList();
            Assert.Equal(2, resultList.Count);
            Assert.All(resultList, t => Assert.Equal("TERRAZZA", t.Zona)); // ✅ Tutti in maiuscolo
        }

        [Fact]
        public async Task GetByNumeroPerFrontendAsync_WithValidNumero_ShouldReturnFormattedTavolo()
        {
            // Arrange
            var newTavolo = new TavoloDTO
            {
                Numero = 29,
                Zona = "Interno",
                Disponibile = true
            };
            await _tavoloRepository.AddAsync(newTavolo);

            // Act
            var result = await _tavoloRepository.GetByNumeroPerFrontendAsync(29);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(29, result.Numero);
            Assert.Equal("SI", result.Disponibile);
            Assert.Equal("INTERNO", result.Zona);
            // ✅ VERIFICA CHE NON CI SIA L'ID
            Assert.Null(result.GetType().GetProperty("TavoloId")?.GetValue(result));
        }

        [Fact]
        public async Task GetByNumeroPerFrontendAsync_WithInvalidNumero_ShouldReturnNull()
        {
            // Act
            var result = await _tavoloRepository.GetByNumeroPerFrontendAsync(999);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task ToggleDisponibilitaAsync_ShouldToggleAvailability()
        {
            // Arrange
            var newTavolo = new TavoloDTO
            {
                Numero = 32,
                Zona = "Interno",
                Disponibile = true
            };
            var addedTavolo = await _tavoloRepository.AddAsync(newTavolo);

            // Act - Toggle da true a false
            var result1 = await _tavoloRepository.ToggleDisponibilitaAsync(addedTavolo.TavoloId);

            // Assert - Prima toggle
            Assert.False(result1);
            var tavoloAfterFirstToggle = await _tavoloRepository.GetByIdAsync(addedTavolo.TavoloId);
            Assert.NotNull(tavoloAfterFirstToggle);
            Assert.False(tavoloAfterFirstToggle.Disponibile);

            // Act - Toggle da false a true
            var result2 = await _tavoloRepository.ToggleDisponibilitaAsync(addedTavolo.TavoloId);

            // Assert - Seconda toggle
            Assert.True(result2);
            var tavoloAfterSecondToggle = await _tavoloRepository.GetByIdAsync(addedTavolo.TavoloId);
            Assert.NotNull(tavoloAfterSecondToggle);
            Assert.True(tavoloAfterSecondToggle.Disponibile);
        }

        [Fact]
        public async Task ToggleDisponibilitaAsync_WithInvalidId_ShouldReturnFalse()
        {
            // Act
            var result = await _tavoloRepository.ToggleDisponibilitaAsync(999);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task ToggleDisponibilitaByNumeroAsync_ShouldToggleAvailability()
        {
            // Arrange
            var newTavolo = new TavoloDTO
            {
                Numero = 33,
                Zona = "Interno",
                Disponibile = true
            };
            await _tavoloRepository.AddAsync(newTavolo);

            // Act - Toggle da true a false
            var result1 = await _tavoloRepository.ToggleDisponibilitaByNumeroAsync(33);

            // Assert - Prima toggle
            Assert.False(result1);
            var tavoloAfterFirstToggle = await _tavoloRepository.GetByNumeroAsync(33);
            Assert.NotNull(tavoloAfterFirstToggle);
            Assert.False(tavoloAfterFirstToggle.Disponibile);

            // Act - Toggle da false a true
            var result2 = await _tavoloRepository.ToggleDisponibilitaByNumeroAsync(33);

            // Assert - Seconda toggle
            Assert.True(result2);
            var tavoloAfterSecondToggle = await _tavoloRepository.GetByNumeroAsync(33);
            Assert.NotNull(tavoloAfterSecondToggle);
            Assert.True(tavoloAfterSecondToggle.Disponibile);
        }

        [Fact]
        public async Task ToggleDisponibilitaByNumeroAsync_WithInvalidNumero_ShouldReturnFalse()
        {
            // Act
            var result = await _tavoloRepository.ToggleDisponibilitaByNumeroAsync(999);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task DeleteAsync_WithDependencies_ShouldThrowException()
        {
            // Arrange - Crea un tavolo con dipendenze
            var newTavolo = new TavoloDTO
            {
                Numero = 34,
                Zona = "Interno",
                Disponibile = true
            };
            var addedTavolo = await _tavoloRepository.AddAsync(newTavolo);

            // Crea un cliente associato al tavolo
            _context.Cliente.Add(new Cliente
            {
                ClienteId = 2,
                TavoloId = addedTavolo.TavoloId,
                DataCreazione = DateTime.UtcNow
            });
            await _context.SaveChangesAsync();

            // Act & Assert - ✅ CATTURA ENTRAMBE LE POSSIBILI ECCEZIONI
            var exception = await Record.ExceptionAsync(() =>
                _tavoloRepository.DeleteAsync(addedTavolo.TavoloId)
            );

            // ✅ VERIFICA CHE SIA STATO LANCIATO UN ERRORE
            Assert.NotNull(exception);
            Assert.True(exception is InvalidOperationException || exception is DbUpdateException);

            // ✅ VERIFICA CHE IL TAVOLO NON SIA STATO ELIMINATO
            var result = await _tavoloRepository.GetByIdAsync(addedTavolo.TavoloId);
            Assert.NotNull(result);
        }

        [Fact]
        public async Task DeleteAsync_WithNoDependencies_ShouldSucceed()
        {
            // Arrange - Crea un tavolo senza dipendenze
            var newTavolo = new TavoloDTO
            {
                Numero = 35,
                Zona = "Interno",
                Disponibile = true
            };
            var addedTavolo = await _tavoloRepository.AddAsync(newTavolo);

            // Act & Assert - Non dovrebbe lanciare eccezioni
            var exception = await Record.ExceptionAsync(() =>
                _tavoloRepository.DeleteAsync(addedTavolo.TavoloId)
            );

            Assert.Null(exception);

            // Verifica che il tavolo sia stato eliminato
            var result = await _tavoloRepository.GetByIdAsync(addedTavolo.TavoloId);
            Assert.Null(result);
        }       
    }
}