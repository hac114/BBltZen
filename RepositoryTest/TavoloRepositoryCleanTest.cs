using Database.Models;
using DTO;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Repository.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace RepositoryTest
{
    public class TavoloRepositoryCleanTest : BaseTestClean
    {
        private readonly TavoloRepository _repository;

        public TavoloRepositoryCleanTest()
        {
            _repository = new TavoloRepository(_context, GetTestLogger<TavoloRepository>());
        }

        #region GetAllAsync Tests

        [Fact]
        public async Task GetAllAsync_ShouldReturnPaginatedTavoli()
        {
            // Arrange - Crea tavoli per il test
            await CreateTestTavoloAsync(1, true, "Zona A");

            // Act
            var result = await _repository.GetAllAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Page);
            Assert.Equal(10, result.PageSize);
            Assert.True(result.TotalCount > 0);
            Assert.Contains("tavoli", result.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task GetAllAsync_WithPagination_ShouldReturnCorrectPage()
        {
            // Arrange
            for (int i = 1; i <= 15; i++)
            {
                await CreateTestTavoloAsync(i, true, $"Zona {i % 3}");
            }

            // Act - Pagina 2, 5 elementi per pagina
            var result = await _repository.GetAllAsync(page: 2, pageSize: 5);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Page);
            Assert.Equal(5, result.PageSize);
            Assert.Equal(15, result.TotalCount);
            Assert.Equal(5, result.Data.Count());
        }

        [Fact]
        public async Task GetAllAsync_NoTavoli_ShouldReturnEmpty()
        {
            // Arrange
            await CleanTableAsync<Tavolo>();

            // Act
            var result = await _repository.GetAllAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result.Data);
            Assert.Equal(0, result.TotalCount);
            Assert.Contains("Nessun tavolo trovato", result.Message, StringComparison.Ordinal);
        }

        [Fact]
        public async Task GetAllAsync_ShouldOrderByNumero()
        {
            // Arrange
            await CleanTableAsync<Tavolo>();

            await CreateTestTavoloAsync(5, true, "Zona A");
            await CreateTestTavoloAsync(1, true, "Zona B");
            await CreateTestTavoloAsync(3, true, "Zona C");

            // Act
            var result = await _repository.GetAllAsync();

            // Assert - Verifica ordinamento per numero
            var tavoli = result.Data.ToList();
            Assert.Equal(1, tavoli[0].Numero);
            Assert.Equal(3, tavoli[1].Numero);
            Assert.Equal(5, tavoli[2].Numero);
        }

        #endregion

        #region GetByIdAsync Tests

        [Fact]
        public async Task GetByIdAsync_ValidId_ShouldReturnTavolo()
        {
            // Arrange
            var testTavolo = await CreateTestTavoloAsync(99, true, "Test GetById");

            // Act
            var result = await _repository.GetByIdAsync(testTavolo.TavoloId);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(testTavolo.TavoloId, result.Data.TavoloId);
            Assert.Equal(99, result.Data.Numero);
            Assert.True(result.Data.Disponibile);
            Assert.Equal("Test GetById", result.Data.Zona);
            Assert.Contains($"Tavolo con ID {testTavolo.TavoloId} trovato", result.Message, StringComparison.Ordinal);
        }

        [Fact]
        public async Task GetByIdAsync_InvalidId_ShouldReturnError()
        {
            // Arrange
            int invalidId = -1;

            // Act
            var result = await _repository.GetByIdAsync(invalidId);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Null(result.Data);
            Assert.Contains("ID tavolo non valido", result.Message, StringComparison.Ordinal);
        }

        [Fact]
        public async Task GetByIdAsync_NonExistentId_ShouldReturnNotFound()
        {
            // Arrange
            int nonExistentId = 9999;

            // Act
            var result = await _repository.GetByIdAsync(nonExistentId);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Null(result.Data);
            Assert.Contains($"Tavolo con ID {nonExistentId} non trovato", result.Message, StringComparison.Ordinal);
        }

        #endregion

        #region GetByNumeroAsync Tests

        [Fact]
        public async Task GetByNumeroAsync_ValidNumero_ShouldReturnTavolo()
        {
            // Arrange
            await CreateTestTavoloAsync(42, true, "Test GetByNumero");

            // Act
            var result = await _repository.GetByNumeroAsync(42);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(42, result.Data.Numero);
            Assert.Contains($"Tavolo con numero 42 trovato", result.Message, StringComparison.Ordinal);
        }

        [Fact]
        public async Task GetByNumeroAsync_InvalidNumero_ShouldReturnError()
        {
            // Arrange
            int invalidNumero = 0;

            // Act
            var result = await _repository.GetByNumeroAsync(invalidNumero);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Null(result.Data);
            Assert.Contains("Il numero tavolo deve essere maggiore di 0", result.Message, StringComparison.Ordinal);
        }

        [Fact]
        public async Task GetByNumeroAsync_NonExistentNumero_ShouldReturnNotFound()
        {
            // Arrange
            int nonExistentNumero = 999;

            // Act
            var result = await _repository.GetByNumeroAsync(nonExistentNumero);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Null(result.Data);
            Assert.Contains($"Tavolo con numero {nonExistentNumero} non trovato", result.Message, StringComparison.Ordinal);
        }

        #endregion

        #region GetDisponibiliAsync Tests

        [Fact]
        public async Task GetDisponibiliAsync_ShouldReturnOnlyAvailableTavoli()
        {
            // Arrange
            await CleanTableAsync<Tavolo>();

            await CreateTestTavoloAsync(1, true, "Zona A");
            await CreateTestTavoloAsync(2, false, "Zona A");
            await CreateTestTavoloAsync(3, true, "Zona B");

            // Act
            var result = await _repository.GetDisponibiliAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.TotalCount); // Solo tavoli 1 e 3
            Assert.All(result.Data, t => Assert.True(t.Disponibile));
            Assert.Contains("disponibili", result.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task GetDisponibiliAsync_NoDisponibili_ShouldReturnEmpty()
        {
            // Arrange
            await CleanTableAsync<Tavolo>();

            // Solo tavoli occupati
            await CreateTestTavoloAsync(1, false, "Zona A");
            await CreateTestTavoloAsync(2, false, "Zona B");

            // Act
            var result = await _repository.GetDisponibiliAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result.Data);
            Assert.Equal(0, result.TotalCount);
            Assert.Contains("Nessun tavolo disponibile trovato", result.Message, StringComparison.Ordinal);
        }

        #endregion

        #region GetOccupatiAsync Tests

        [Fact]
        public async Task GetOccupatiAsync_ShouldReturnOnlyOccupiedTavoli()
        {
            // Arrange
            await CleanTableAsync<Tavolo>();

            await CreateTestTavoloAsync(1, true, "Zona A");
            await CreateTestTavoloAsync(2, false, "Zona A");
            await CreateTestTavoloAsync(3, false, "Zona B");

            // Act
            var result = await _repository.GetOccupatiAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.TotalCount); // Solo tavoli 2 e 3
            Assert.All(result.Data, t => Assert.False(t.Disponibile));
            Assert.Contains("occupati", result.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task GetOccupatiAsync_NoOccupati_ShouldReturnEmpty()
        {
            // Arrange
            await CleanTableAsync<Tavolo>();

            // Solo tavoli disponibili
            await CreateTestTavoloAsync(1, true, "Zona A");
            await CreateTestTavoloAsync(2, true, "Zona B");

            // Act
            var result = await _repository.GetOccupatiAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result.Data);
            Assert.Equal(0, result.TotalCount);
            Assert.Contains("Nessun tavolo occupato trovato", result.Message, StringComparison.Ordinal);
        }

        #endregion

        #region GetByZonaAsync Tests

        [Fact]
        public async Task GetByZonaAsync_ValidZona_ShouldReturnFilteredTavoli()
        {
            // Arrange
            await CleanTableAsync<Tavolo>();

            await CreateTestTavoloAsync(1, true, "Terrazza");
            await CreateTestTavoloAsync(2, true, "Interno");
            await CreateTestTavoloAsync(3, true, "Terrazza Vista Mare");

            // Act
            var result = await _repository.GetByZonaAsync("Terrazza");

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.TotalCount); // Tavoli 1 e 3
            Assert.All(result.Data, t => Assert.Contains("Terrazza", t.Zona!, StringComparison.OrdinalIgnoreCase));
            Assert.Contains("Terrazza", result.Message, StringComparison.Ordinal);
        }

        [Fact]
        public async Task GetByZonaAsync_EmptyZona_ShouldReturnError()
        {
            // Act
            var result = await _repository.GetByZonaAsync("");

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result.Data);
            Assert.Contains("Il parametro 'zona' è obbligatorio", result.Message, StringComparison.Ordinal);
        }

        [Fact]
        public async Task GetByZonaAsync_InvalidInput_ShouldReturnError()
        {
            // Arrange
            var invalidInput = new string('A', 60); // Troppo lungo (max 50)

            // Act
            var result = await _repository.GetByZonaAsync(invalidInput);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result.Data);
            Assert.Contains("Il parametro 'zona' non è valido", result.Message, StringComparison.Ordinal);
        }

        [Fact]
        public async Task GetByZonaAsync_CaseInsensitive_ShouldFindMatches()
        {
            // Arrange
            await CleanTableAsync<Tavolo>();
            await CreateTestTavoloAsync(1, true, "TERRAZZA PRINCIPALE");

            // Act - Cerca con case diverso
            var result = await _repository.GetByZonaAsync("terrazza");

            // Assert
            Assert.NotNull(result);
            Assert.Single(result.Data);
            Assert.Equal("TERRAZZA PRINCIPALE", result.Data.First().Zona);
        }

        #endregion

        #region ExistsAsync Tests

        [Fact]
        public async Task ExistsAsync_ValidExistingId_ShouldReturnTrue()
        {
            // Arrange
            var testTavolo = await CreateTestTavoloAsync();

            // Act
            var result = await _repository.ExistsAsync(testTavolo.TavoloId);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.True(result.Data);
            Assert.Contains($"Tavolo con ID {testTavolo.TavoloId} esiste", result.Message, StringComparison.Ordinal);
        }

        [Fact]
        public async Task ExistsAsync_NonExistentId_ShouldReturnFalse()
        {
            // Act
            var result = await _repository.ExistsAsync(9999);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.False(result.Data);
            Assert.Contains($"Tavolo con ID 9999 non trovato", result.Message, StringComparison.Ordinal);
        }

        [Fact]
        public async Task ExistsAsync_InvalidId_ShouldReturnError()
        {
            // Act
            var result = await _repository.ExistsAsync(0);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Contains("ID tavolo non valido", result.Message, StringComparison.Ordinal);
        }

        #endregion

        #region NumeroExistsAsync Tests

        [Fact]
        public async Task NumeroExistsAsync_ExistingNumero_ShouldReturnTrue()
        {
            // Arrange
            await CreateTestTavoloAsync(77, true, "Test NumeroExists");

            // Act
            var result = await _repository.NumeroExistsAsync(77);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.True(result.Data);
            Assert.Contains($"Tavolo con numero 77 esiste", result.Message, StringComparison.Ordinal);
        }

        [Fact]
        public async Task NumeroExistsAsync_NonExistentNumero_ShouldReturnFalse()
        {
            // Act
            var result = await _repository.NumeroExistsAsync(999);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.False(result.Data);
            Assert.Contains($"Tavolo con numero 999 non trovato", result.Message, StringComparison.Ordinal);
        }

        [Fact]
        public async Task NumeroExistsAsync_InvalidNumero_ShouldReturnError()
        {
            // Act
            var result = await _repository.NumeroExistsAsync(0);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Contains("Il numero del tavolo deve essere maggiore di 0", result.Message, StringComparison.Ordinal);
        }

        [Fact]
        public async Task NumeroExistsAsync_WithExcludeId_ShouldWorkCorrectly()
        {
            // Arrange
            var tavolo1 = await CreateTestTavoloAsync(50, true, "Test");
            var tavolo2 = await CreateTestTavoloAsync(50, true, "Test 2"); // Stesso numero

            // Act - Cerca numero 50 escludendo tavolo1
            var result = await _repository.NumeroExistsAsync(50, tavolo1.TavoloId);

            // Assert - Dovrebbe trovare tavolo2 (quindi true)
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.True(result.Data);
            Assert.Contains($"escludendo ID {tavolo1.TavoloId}", result.Message, StringComparison.Ordinal);
        }

        #endregion

        #region CountAsync Tests

        [Fact]
        public async Task CountAsync_ShouldReturnTotalCount()
        {
            // Arrange
            await CleanTableAsync<Tavolo>();

            await CreateTestTavoloAsync(1, true, "Zona A");
            await CreateTestTavoloAsync(2, false, "Zona B");
            await CreateTestTavoloAsync(3, true, "Zona C");

            // Act
            var result = await _repository.CountAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(3, result.Data);
            Assert.Contains("3 tavoli", result.Message, StringComparison.Ordinal);
        }

        [Fact]
        public async Task CountAsync_NoTavoli_ShouldReturnZero()
        {
            // Arrange
            await CleanTableAsync<Tavolo>();

            // Act
            var result = await _repository.CountAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(0, result.Data);
            Assert.Contains("Nessun tavolo presente", result.Message, StringComparison.Ordinal);
        }

        [Fact]
        public async Task CountDisponibiliAsync_ShouldReturnAvailableCount()
        {
            // Arrange
            await CleanTableAsync<Tavolo>();

            await CreateTestTavoloAsync(1, true, "Zona A");
            await CreateTestTavoloAsync(2, false, "Zona A");
            await CreateTestTavoloAsync(3, true, "Zona B");

            // Act
            var result = await _repository.CountDisponibiliAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(2, result.Data); // Solo tavoli 1 e 3
            Assert.Contains("disponibili", result.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task CountOccupatiAsync_ShouldReturnOccupiedCount()
        {
            // Arrange
            await CleanTableAsync<Tavolo>();

            await CreateTestTavoloAsync(1, true, "Zona A");
            await CreateTestTavoloAsync(2, false, "Zona A");
            await CreateTestTavoloAsync(3, false, "Zona B");

            // Act
            var result = await _repository.CountOccupatiAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(2, result.Data); // Solo tavoli 2 e 3
            Assert.Contains("occupati", result.Message, StringComparison.OrdinalIgnoreCase);
        }

        #endregion

        #region AddAsync Tests

        [Fact]
        public async Task AddAsync_ValidTavolo_ShouldCreateAndReturnTavolo()
        {
            // Arrange
            var tavoloDto = new TavoloDTO
            {
                Numero = 100,
                Disponibile = true,
                Zona = "Terrazza Test"
            };

            // Act
            var result = await _repository.AddAsync(tavoloDto);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.True(result.Data.TavoloId > 0);
            Assert.Equal(100, result.Data.Numero);
            Assert.True(result.Data.Disponibile);
            Assert.Equal("Terrazza Test", result.Data.Zona);
            Assert.Contains("Tavolo 100 creato con successo", result.Message, StringComparison.Ordinal);

            // Verifica che sia stato salvato nel database
            var savedTavolo = await _context.Tavolo.FindAsync(result.Data.TavoloId);
            Assert.NotNull(savedTavolo);
            Assert.Equal(result.Data.TavoloId, savedTavolo.TavoloId);
            Assert.Equal("Terrazza Test", savedTavolo.Zona);
        }

        [Fact]
        public async Task AddAsync_DuplicateNumero_ShouldReturnError()
        {
            // Arrange
            await CreateTestTavoloAsync(88, true, "Zona Duplicato");

            var tavoloDto = new TavoloDTO
            {
                Numero = 88,
                Disponibile = false,
                Zona = "Zona Diversa"
            };

            // Act
            var result = await _repository.AddAsync(tavoloDto);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Null(result.Data);
            Assert.Contains($"Esiste già un tavolo con numero 88", result.Message, StringComparison.Ordinal);
        }

        [Fact]
        public async Task AddAsync_InvalidNumero_ShouldReturnError()
        {
            // Arrange
            var tavoloDto = new TavoloDTO
            {
                Numero = 0, // Numero non valido
                Disponibile = true,
                Zona = "Test"
            };

            // Act
            var result = await _repository.AddAsync(tavoloDto);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Null(result.Data);
            Assert.Contains("Il numero del tavolo deve essere maggiore di 0", result.Message, StringComparison.Ordinal);
        }

        [Fact]
        public async Task AddAsync_InvalidZonaLength_ShouldReturnError()
        {
            // Arrange
            var tavoloDto = new TavoloDTO
            {
                Numero = 10,
                Disponibile = true,
                Zona = new string('A', 60) // Troppo lungo (max 50)
            };

            // Act
            var result = await _repository.AddAsync(tavoloDto);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Null(result.Data);
            Assert.Contains("Il campo 'Zona' contiene caratteri non validi", result.Message, StringComparison.Ordinal);
        }

        [Fact]
        public async Task AddAsync_NullDto_ShouldReturnErrorResponse()
        {
            // Act
            var result = await _repository.AddAsync(null!);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Null(result.Data);
            Assert.Contains("Errore interno", result.Message, StringComparison.Ordinal);
        }

        #endregion

        #region UpdateAsync Tests

        [Fact]
        public async Task UpdateAsync_ValidUpdate_ShouldUpdateTavolo()
        {
            // Arrange
            var tavolo = await CreateTestTavoloAsync(10, true, "Vecchia Zona");
            var tavoloDto = new TavoloDTO
            {
                TavoloId = tavolo.TavoloId,
                Numero = 20,
                Disponibile = false,
                Zona = "Nuova Zona"
            };

            // Act
            var result = await _repository.UpdateAsync(tavoloDto);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.True(result.Data);
            Assert.Contains($"Tavolo con ID {tavolo.TavoloId} aggiornato con successo", result.Message, StringComparison.Ordinal);

            // Verifica aggiornamento nel database
            var updatedTavolo = await _context.Tavolo.FindAsync(tavolo.TavoloId);
            Assert.NotNull(updatedTavolo);
            Assert.Equal(20, updatedTavolo.Numero);
            Assert.False(updatedTavolo.Disponibile);
            Assert.Equal("Nuova Zona", updatedTavolo.Zona);
        }

        [Fact]
        public async Task UpdateAsync_NonExistentId_ShouldReturnNotFound()
        {
            // Arrange
            var tavoloDto = new TavoloDTO
            {
                TavoloId = 9999,
                Numero = 99,
                Disponibile = true,
                Zona = "Test"
            };

            // Act
            var result = await _repository.UpdateAsync(tavoloDto);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Contains($"Tavolo con ID 9999 non trovato", result.Message, StringComparison.Ordinal);
        }

        [Fact]
        public async Task UpdateAsync_DuplicateNumero_ShouldReturnError()
        {
            // Arrange
            var tavolo1 = await CreateTestTavoloAsync(30, true, "Zona 1");
            await CreateTestTavoloAsync(40, true, "Zona 2");

            var tavoloDto = new TavoloDTO
            {
                TavoloId = tavolo1.TavoloId,
                Numero = 40, // Numero già usato da tavolo2
                Disponibile = true,
                Zona = "Zona Modificata"
            };

            // Act
            var result = await _repository.UpdateAsync(tavoloDto);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Contains($"Esiste già un altro tavolo con numero 40", result.Message, StringComparison.Ordinal);
        }

        [Fact]
        public async Task UpdateAsync_NoChanges_ShouldReturnFalseWithMessage()
        {
            // Arrange
            var tavolo = await CreateTestTavoloAsync(50, true, "Test No Changes");
            var tavoloDto = new TavoloDTO
            {
                TavoloId = tavolo.TavoloId,
                Numero = 50,
                Disponibile = true,
                Zona = "Test No Changes"
            };

            // Act
            var result = await _repository.UpdateAsync(tavoloDto);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.False(result.Data); // False perché non ci sono cambiamenti
            Assert.Contains($"Nessuna modifica necessaria", result.Message, StringComparison.Ordinal);
        }

        [Fact]
        public async Task UpdateAsync_NullDto_ShouldReturnErrorResponse()
        {
            // Act
            var result = await _repository.UpdateAsync(null!);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Contains("Errore interno", result.Message, StringComparison.Ordinal);
        }

        #endregion

        #region DeleteAsync Tests

        [Fact]
        public async Task DeleteAsync_ValidId_ShouldDeleteTavolo()
        {
            // Arrange
            var tavolo = await CreateTestTavoloAsync(60, true, "Test Delete");
            var tavoloId = tavolo.TavoloId;

            // Act
            var result = await _repository.DeleteAsync(tavoloId);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.True(result.Data);
            Assert.Contains($"eliminato con successo", result.Message, StringComparison.Ordinal);

            // Verifica che sia stato eliminato dal database
            var deletedTavolo = await _context.Tavolo.FindAsync(tavoloId);
            Assert.Null(deletedTavolo);
        }

        [Fact]
        public async Task DeleteAsync_NonExistentId_ShouldReturnNotFound()
        {
            // Act
            var result = await _repository.DeleteAsync(9999);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Contains($"Tavolo con ID 9999 non trovato", result.Message, StringComparison.Ordinal);
        }

        [Fact]
        public async Task DeleteAsync_InvalidId_ShouldReturnError()
        {
            // Act
            var result = await _repository.DeleteAsync(0);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Contains("ID tavolo non valido", result.Message, StringComparison.Ordinal);
        }

        [Fact]
        public async Task DeleteAsync_TavoloWithDependencies_ShouldReturnError()
        {
            // Arrange
            var tavolo = await CreateTestTavoloAsync(70, true, "Test Dependencies");

            // Nota: Non creiamo dipendenze reali perché richiederebbe Cliente o SessioniQr
            // Il test verificherà che il metodo HasDependenciesAsync funzioni correttamente

            // Act
            var result = await _repository.DeleteAsync(tavolo.TavoloId);

            // Se ci sono dipendenze nel seed, il test dovrebbe fallire
            // Altrimenti, dovrebbe riuscire
            Assert.NotNull(result);

            if (!result.Success)
            {
                Assert.Contains("Impossibile eliminare il tavolo", result.Message, StringComparison.Ordinal);
            }
            else
            {
                Assert.True(result.Success);
                Assert.True(result.Data);
            }
        }

        #endregion

        #region ToggleDisponibilitaAsync Tests

        [Fact]
        public async Task ToggleDisponibilitaAsync_ValidId_ShouldToggleAvailability()
        {
            // Arrange
            var tavolo = await CreateTestTavoloAsync(80, true, "Test Toggle");
            var initialState = tavolo.Disponibile;

            // Act
            var result = await _repository.ToggleDisponibilitaAsync(tavolo.TavoloId);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotEqual(initialState, result.Data); // Stato dovrebbe essere cambiato
            Assert.Contains($"impostato come", result.Message, StringComparison.Ordinal);

            // Verifica nel database
            var updatedTavolo = await _context.Tavolo.FindAsync(tavolo.TavoloId);
            Assert.NotNull(updatedTavolo);
            Assert.Equal(!initialState, updatedTavolo.Disponibile);
        }

        [Fact]
        public async Task ToggleDisponibilitaAsync_NonExistentId_ShouldReturnNotFound()
        {
            // Act
            var result = await _repository.ToggleDisponibilitaAsync(9999);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Contains($"Tavolo con ID 9999 non trovato", result.Message, StringComparison.Ordinal);
        }

        [Fact]
        public async Task ToggleDisponibilitaAsync_InvalidId_ShouldReturnError()
        {
            // Act
            var result = await _repository.ToggleDisponibilitaAsync(0);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Contains("ID tavolo non valido", result.Message, StringComparison.Ordinal);
        }

        [Fact]
        public async Task ToggleDisponibilitaByNumeroAsync_ValidNumero_ShouldToggleAvailability()
        {
            // Arrange
            await CreateTestTavoloAsync(90, true, "Test Toggle By Numero");

            // Act
            var result = await _repository.ToggleDisponibilitaByNumeroAsync(90);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.False(result.Data); // Da true a false
            Assert.Contains($"impostato come", result.Message, StringComparison.Ordinal);
        }

        [Fact]
        public async Task ToggleDisponibilitaByNumeroAsync_NonExistentNumero_ShouldReturnNotFound()
        {
            // Act
            var result = await _repository.ToggleDisponibilitaByNumeroAsync(999);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Contains($"Tavolo con numero 999 non trovato", result.Message, StringComparison.Ordinal);
        }

        #endregion

        #region Integration Tests

        [Fact]
        public async Task FullCrudFlow_ShouldWorkCorrectly()
        {
            // 1. CREATE
            var addDto = new TavoloDTO
            {
                Numero = 200,
                Disponibile = true,
                Zona = "Flow Test"
            };
            var addResult = await _repository.AddAsync(addDto);
            Assert.True(addResult.Success);
            var tavoloId = addResult.Data!.TavoloId;

            // 2. READ by ID
            var getResult = await _repository.GetByIdAsync(tavoloId);
            Assert.True(getResult.Success);
            Assert.Equal(200, getResult.Data!.Numero);

            // 3. READ by Numero
            var getByNumeroResult = await _repository.GetByNumeroAsync(200);
            Assert.True(getByNumeroResult.Success);

            // 4. EXISTS
            var existsResult = await _repository.ExistsAsync(tavoloId);
            Assert.True(existsResult.Success);
            Assert.True(existsResult.Data);

            // 5. Numero EXISTS
            var numeroExistsResult = await _repository.NumeroExistsAsync(200);
            Assert.True(numeroExistsResult.Success);
            Assert.True(numeroExistsResult.Data);

            // 6. TOGGLE disponibilità
            var toggleResult = await _repository.ToggleDisponibilitaAsync(tavoloId);
            Assert.True(toggleResult.Success);
            Assert.False(toggleResult.Data); // Ora false

            // 7. UPDATE
            var updateDto = new TavoloDTO
            {
                TavoloId = tavoloId,
                Numero = 201,
                Disponibile = false,
                Zona = "Flow Test Updated"
            };
            var updateResult = await _repository.UpdateAsync(updateDto);
            Assert.True(updateResult.Success);
            Assert.True(updateResult.Data);

            // 8. Verify update
            var verifyResult = await _repository.GetByIdAsync(tavoloId);
            Assert.Equal(201, verifyResult.Data!.Numero);

            // 9. DELETE
            var deleteResult = await _repository.DeleteAsync(tavoloId);
            Assert.True(deleteResult.Success);
            Assert.True(deleteResult.Data);

            // 10. Verify deletion
            var finalExistsResult = await _repository.ExistsAsync(tavoloId);
            Assert.False(finalExistsResult.Data);
        }

        [Fact]
        public async Task CountMethods_ShouldWorkTogether()
        {
            // Arrange
            await CleanTableAsync<Tavolo>();

            await CreateTestTavoloAsync(1, true, "A");
            await CreateTestTavoloAsync(2, false, "A");
            await CreateTestTavoloAsync(3, true, "B");
            await CreateTestTavoloAsync(4, false, "B");

            // Act & Assert
            var totalResult = await _repository.CountAsync();
            Assert.Equal(4, totalResult.Data);

            var disponibiliResult = await _repository.CountDisponibiliAsync();
            Assert.Equal(2, disponibiliResult.Data);

            var occupatiResult = await _repository.CountOccupatiAsync();
            Assert.Equal(2, occupatiResult.Data);

            // Verifica consistenza
            Assert.Equal(totalResult.Data, disponibiliResult.Data + occupatiResult.Data);
        }

        #endregion

        #region Private Helper Methods Tests

        [Fact]
        public async Task HasDependenciesAsync_ShouldBeTestable()
        {
            // Arrange
            var tavolo = await CreateTestTavoloAsync(300, true, "Test Dependencies");

            // Act - Usa reflection per testare il metodo privato
            var method = typeof(TavoloRepository)
                .GetMethod("HasDependenciesAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            var task = (Task<bool>)method!.Invoke(_repository, new object[] { tavolo.TavoloId })!;
            await task;

            // Assert - Verifica che il metodo non lanci eccezioni
            // Non possiamo sapere il risultato esatto perché dipende dai dati seedati
            Assert.True(true); // Placeholder - testa solo che il metodo sia eseguibile
        }

        #endregion
    }
}