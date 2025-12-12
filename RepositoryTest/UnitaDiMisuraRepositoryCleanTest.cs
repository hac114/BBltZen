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
    public class UnitaDiMisuraRepositoryCleanTest : BaseTestClean
    {
        private readonly UnitaDiMisuraRepository _repository;

        public UnitaDiMisuraRepositoryCleanTest()
        {
            _repository = new UnitaDiMisuraRepository(_context, GetTestLogger<UnitaDiMisuraRepository>());
        }

        #region GetAllAsync Tests

        [Fact]
        public async Task GetAllAsync_ShouldReturnPaginatedUnitaDiMisura()
        {
            // Act
            var result = await _repository.GetAllAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Page);
            Assert.Equal(10, result.PageSize);
            Assert.True(result.TotalCount >= 3); // 3 unità seedate nel BaseTestClean (ML, GR, PZ)
            Assert.Contains("unità di misura", result.Message);
        }

        [Fact]
        public async Task GetAllAsync_WithPagination_ShouldReturnCorrectPage()
        {
            // Arrange
            for (int i = 0; i < 10; i++)
            {
                await CreateTestUnitaDiMisuraAsync($"TEST{i}", $"Test Unità {i}");
            }

            // Act - Pagina 2, 5 elementi per pagina
            var result = await _repository.GetAllAsync(page: 2, pageSize: 5);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Page);
            Assert.Equal(5, result.PageSize);
            Assert.True(result.Data.Count() <= 5);
        }

        [Fact]
        public async Task GetAllAsync_NoUnita_ShouldReturnEmpty()
        {
            // Arrange
            await CleanTableAsync<UnitaDiMisura>();

            // Act
            var result = await _repository.GetAllAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result.Data);
            Assert.Equal(0, result.TotalCount);
            Assert.Contains("Nessuna unità di misura trovata", result.Message);
        }

        [Fact]
        public async Task GetAllAsync_ShouldOrderBySigla()
        {
            // Arrange
            await CleanTableAsync<UnitaDiMisura>();

            await CreateTestUnitaDiMisuraAsync("Z", "Zeta");
            await CreateTestUnitaDiMisuraAsync("A", "Alfa");
            await CreateTestUnitaDiMisuraAsync("B", "Beta");

            // Act
            var result = await _repository.GetAllAsync();

            // Assert - Verifica ordinamento per sigla
            var unita = result.Data.ToList();
            Assert.Equal("A", unita[0].Sigla);
            Assert.Equal("B", unita[1].Sigla);
            Assert.Equal("Z", unita[2].Sigla);
        }

        #endregion

        #region GetByIdAsync Tests

        [Fact]
        public async Task GetByIdAsync_ValidId_ShouldReturnUnita()
        {
            // Arrange
            var testUnita = await CreateTestUnitaDiMisuraAsync("TEST", "Test GetById");

            // Act
            var result = await _repository.GetByIdAsync(testUnita.UnitaMisuraId);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(testUnita.UnitaMisuraId, result.Data.UnitaMisuraId);
            Assert.Equal("TEST", result.Data.Sigla);
            Assert.Equal("Test GetById", result.Data.Descrizione);
            Assert.Contains($"Unità di misura con ID {testUnita.UnitaMisuraId} trovata", result.Message);
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
            Assert.Contains("ID unità di misura non valido", result.Message);
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
            Assert.Contains($"Unità di misura con ID {nonExistentId} non trovata", result.Message);
        }

        #endregion

        #region GetBySiglaAsync Tests

        [Fact]
        public async Task GetBySiglaAsync_ValidSigla_ShouldReturnFilteredUnita()
        {
            // Arrange
            await CleanTableAsync<UnitaDiMisura>();

            await CreateTestUnitaDiMisuraAsync("ML", "Millilitri");
            await CreateTestUnitaDiMisuraAsync("MM", "Millimetri");
            await CreateTestUnitaDiMisuraAsync("CM", "Centimetri");

            // Act
            var result = await _repository.GetBySiglaAsync("M");

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.TotalCount); // ML e MM
            Assert.True(result.Data.All(u => u.Sigla!.StartsWith("M", StringComparison.OrdinalIgnoreCase)));
            Assert.Contains("sigla che inizia con 'M'", result.Message);
        }

        [Fact]
        public async Task GetBySiglaAsync_EmptySigla_ShouldReturnError()
        {
            // Act
            var result = await _repository.GetBySiglaAsync("");

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result.Data);
            Assert.Contains("Il parametro 'sigla' è obbligatorio", result.Message);
        }

        [Fact]
        public async Task GetBySiglaAsync_InvalidInput_ShouldReturnError()
        {
            // Arrange
            var invalidInput = new string('A', 15); // Troppo lungo (max 10)

            // Act
            var result = await _repository.GetBySiglaAsync(invalidInput);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result.Data);
            Assert.Contains("Il parametro 'sigla' contiene caratteri non validi", result.Message);
        }

        [Fact]
        public async Task GetBySiglaAsync_CaseInsensitive_ShouldFindMatches()
        {
            // Arrange
            await CleanTableAsync<UnitaDiMisura>();
            await CreateTestUnitaDiMisuraAsync("TEST", "Test Unit");

            // Act - Cerca con case diverso
            var result = await _repository.GetBySiglaAsync("test");

            // Assert
            Assert.NotNull(result);
            Assert.Single(result.Data);
            Assert.Equal("TEST", result.Data.First().Sigla);
        }

        [Fact]
        public async Task GetBySiglaAsync_WithPagination_ShouldReturnCorrectPage()
        {
            // Arrange
            await CleanTableAsync<UnitaDiMisura>();

            for (int i = 0; i < 15; i++)
            {
                await CreateTestUnitaDiMisuraAsync($"T{i}", $"Test {i}");
            }

            // Act
            var result = await _repository.GetBySiglaAsync("T", page: 2, pageSize: 5);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Page);
            Assert.Equal(5, result.PageSize);
            Assert.Equal(15, result.TotalCount);
            Assert.Equal(5, result.Data.Count());
        }

        #endregion

        #region GetByDescrizioneAsync Tests

        [Fact]
        public async Task GetByDescrizioneAsync_ValidDescrizione_ShouldReturnFilteredUnita()
        {
            // Arrange
            await CleanTableAsync<UnitaDiMisura>();

            await CreateTestUnitaDiMisuraAsync("ML", "Millilitri liquidi");
            await CreateTestUnitaDiMisuraAsync("GR", "Grammi solidi");
            await CreateTestUnitaDiMisuraAsync("MM", "Millimetri lineari");

            // Act
            var result = await _repository.GetByDescrizioneAsync("Milli");

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.TotalCount); // Millilitri e Millimetri
            Assert.True(result.Data.All(u => u.Descrizione!.StartsWith("Milli", StringComparison.OrdinalIgnoreCase)));
        }

        [Fact]
        public async Task GetByDescrizioneAsync_EmptyDescrizione_ShouldReturnError()
        {
            // Act
            var result = await _repository.GetByDescrizioneAsync("");

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result.Data);
            Assert.Contains("Il parametro 'descrizione' è obbligatorio", result.Message);
        }

        [Fact]
        public async Task GetByDescrizioneAsync_InvalidInput_ShouldReturnError()
        {
            // Arrange
            var invalidInput = new string('A', 60); // Troppo lungo (max 50)

            // Act
            var result = await _repository.GetByDescrizioneAsync(invalidInput);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result.Data);
            Assert.Contains("Il parametro 'descrizione' contiene caratteri non validi", result.Message);
        }

        #endregion

        #region ExistsAsync Tests

        [Fact]
        public async Task ExistsAsync_ValidExistingId_ShouldReturnTrue()
        {
            // Arrange
            var testUnita = await CreateTestUnitaDiMisuraAsync();

            // Act
            var result = await _repository.ExistsAsync(testUnita.UnitaMisuraId);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.True(result.Data);
            Assert.Contains($"Unità di misura con ID {testUnita.UnitaMisuraId} esiste", result.Message);
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
            Assert.Contains($"Unità di misura con ID 9999 non trovata", result.Message);
        }

        [Fact]
        public async Task ExistsAsync_InvalidId_ShouldReturnError()
        {
            // Act
            var result = await _repository.ExistsAsync(0);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Contains("ID unità di misura non valido", result.Message);
        }

        #endregion

        #region SiglaExistsAsync Tests

        [Fact]
        public async Task SiglaExistsAsync_ExistingSigla_ShouldReturnTrue()
        {
            // Arrange
            await CreateTestUnitaDiMisuraAsync("TEST", "Test SiglaExists");

            // Act
            var result = await _repository.SiglaExistsAsync("TEST");

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.True(result.Data);
            Assert.Contains($"Unità di misura con sigla 'TEST' esiste", result.Message);
        }

        [Fact]
        public async Task SiglaExistsAsync_NonExistentSigla_ShouldReturnFalse()
        {
            // Act
            var result = await _repository.SiglaExistsAsync("INVALID");

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.False(result.Data);
            Assert.Contains($"Unità di misura con sigla 'INVALID' non trovata", result.Message);
        }

        [Fact]
        public async Task SiglaExistsAsync_EmptySigla_ShouldReturnError()
        {
            // Act
            var result = await _repository.SiglaExistsAsync("");

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Contains("La sigla è obbligatoria", result.Message);
        }

        [Fact]
        public async Task SiglaExistsAsync_InvalidInput_ShouldReturnError()
        {
            // Arrange
            var invalidInput = new string('A', 15); // Troppo lungo

            // Act
            var result = await _repository.SiglaExistsAsync(invalidInput);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Contains("La sigla contiene caratteri non validi", result.Message);
        }

        [Fact]
        public async Task SiglaExistsAsync_CaseInsensitive_ShouldFindMatches()
        {
            // Arrange
            await CreateTestUnitaDiMisuraAsync("UPPER", "Test Uppercase");

            // Act - Cerca con case diverso
            var result = await _repository.SiglaExistsAsync("upper");

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.True(result.Data);
        }

        #endregion

        #region DescrizioneExistsAsync Tests

        [Fact]
        public async Task DescrizioneExistsAsync_ExistingDescrizione_ShouldReturnTrue()
        {
            // Arrange
            await CreateTestUnitaDiMisuraAsync("TEST", "Descrizione Test");

            // Act
            var result = await _repository.DescrizioneExistsAsync("Descrizione Test");

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.True(result.Data);
            Assert.Contains($"Unità di misura con descrizione 'Descrizione Test' esiste", result.Message);
        }

        [Fact]
        public async Task DescrizioneExistsAsync_NonExistentDescrizione_ShouldReturnFalse()
        {
            // Act
            var result = await _repository.DescrizioneExistsAsync("Descrizione Inesistente");

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.False(result.Data);
            Assert.Contains($"Unità di misura con descrizione 'Descrizione Inesistente' non trovata", result.Message);
        }

        [Fact]
        public async Task DescrizioneExistsAsync_EmptyDescrizione_ShouldReturnError()
        {
            // Act
            var result = await _repository.DescrizioneExistsAsync("");

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Contains("La descrizione è obbligatoria", result.Message);
        }

        [Fact]
        public async Task DescrizioneExistsAsync_InvalidInput_ShouldReturnError()
        {
            // Arrange
            var invalidInput = new string('A', 60); // Troppo lungo

            // Act
            var result = await _repository.DescrizioneExistsAsync(invalidInput);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Contains("La descrizione contiene caratteri non validi", result.Message);
        }

        #endregion

        #region AddAsync Tests

        [Fact]
        public async Task AddAsync_ValidUnita_ShouldCreateAndReturnUnita()
        {
            // Arrange
            var unitaDto = new UnitaDiMisuraDTO
            {
                Sigla = "NEW",
                Descrizione = "Nuova Unità di Test"
            };

            // Act
            var result = await _repository.AddAsync(unitaDto);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.True(result.Data.UnitaMisuraId > 0);
            Assert.Equal("NEW", result.Data.Sigla);
            Assert.Equal("Nuova Unità di Test", result.Data.Descrizione);
            Assert.Contains("NEW", result.Message);
            Assert.Contains("creata con successo", result.Message);

            // Verifica che sia stato salvato nel database
            var savedUnita = await _context.UnitaDiMisura.FindAsync(result.Data.UnitaMisuraId);
            Assert.NotNull(savedUnita);
            Assert.Equal(result.Data.UnitaMisuraId, savedUnita.UnitaMisuraId);
            Assert.Equal("NEW", savedUnita.Sigla);
        }

        [Fact]
        public async Task AddAsync_DuplicateSigla_ShouldReturnError()
        {
            // Arrange
            await CreateTestUnitaDiMisuraAsync("DUP", "Duplicato Sigla");

            var unitaDto = new UnitaDiMisuraDTO
            {
                Sigla = "DUP",
                Descrizione = "Descrizione Diversa"
            };

            // Act
            var result = await _repository.AddAsync(unitaDto);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Null(result.Data);
            Assert.Contains("Esiste già un'unità di misura con sigla 'DUP'", result.Message);
        }

        [Fact]
        public async Task AddAsync_DuplicateDescrizione_ShouldReturnError()
        {
            // Arrange
            await CreateTestUnitaDiMisuraAsync("SIG1", "Descrizione Duplicata");

            var unitaDto = new UnitaDiMisuraDTO
            {
                Sigla = "SIG2",
                Descrizione = "Descrizione Duplicata"
            };

            // Act
            var result = await _repository.AddAsync(unitaDto);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Null(result.Data);
            Assert.Contains("Esiste già un'unità di misura con descrizione 'Descrizione Duplicata'", result.Message);
        }

        [Fact]
        public async Task AddAsync_EmptySigla_ShouldReturnError()
        {
            // Arrange
            var unitaDto = new UnitaDiMisuraDTO
            {
                Sigla = "",
                Descrizione = "Descrizione Test"
            };

            // Act
            var result = await _repository.AddAsync(unitaDto);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Null(result.Data);
            Assert.Contains("Sigla obbligatoria", result.Message);
        }

        [Fact]
        public async Task AddAsync_EmptyDescrizione_ShouldReturnError()
        {
            // Arrange
            var unitaDto = new UnitaDiMisuraDTO
            {
                Sigla = "TEST",
                Descrizione = ""
            };

            // Act
            var result = await _repository.AddAsync(unitaDto);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Null(result.Data);
            Assert.Contains("Descrizione obbligatoria", result.Message);
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
            Assert.Contains("Errore interno", result.Message);
        }

        [Fact]
        public async Task AddAsync_InvalidSiglaLength_ShouldReturnError()
        {
            // Arrange
            var unitaDto = new UnitaDiMisuraDTO
            {
                Sigla = new string('A', 15), // Troppo lungo (max 10)
                Descrizione = "Test"
            };

            // Act
            var result = await _repository.AddAsync(unitaDto);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Null(result.Data);
            Assert.Contains("Sigla non valida", result.Message);
        }

        #endregion

        #region UpdateAsync Tests

        [Fact]
        public async Task UpdateAsync_ValidUpdate_ShouldUpdateUnita()
        {
            // Arrange
            var unita = await CreateTestUnitaDiMisuraAsync("OLD", "Vecchia Descrizione");
            var unitaDto = new UnitaDiMisuraDTO
            {
                UnitaMisuraId = unita.UnitaMisuraId,
                Sigla = "NEW",
                Descrizione = "Nuova Descrizione"
            };

            // Act
            var result = await _repository.UpdateAsync(unitaDto);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.True(result.Data);
            Assert.Contains($"Unità di misura con ID {unita.UnitaMisuraId} aggiornata con successo", result.Message);

            // Verifica aggiornamento nel database
            var updatedUnita = await _context.UnitaDiMisura.FindAsync(unita.UnitaMisuraId);
            Assert.NotNull(updatedUnita);
            Assert.Equal("NEW", updatedUnita.Sigla);
            Assert.Equal("Nuova Descrizione", updatedUnita.Descrizione);
        }

        [Fact]
        public async Task UpdateAsync_NonExistentId_ShouldReturnNotFound()
        {
            // Arrange
            var unitaDto = new UnitaDiMisuraDTO
            {
                UnitaMisuraId = 9999,
                Sigla = "TEST",
                Descrizione = "Test"
            };

            // Act
            var result = await _repository.UpdateAsync(unitaDto);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Contains($"Unità di misura con ID 9999 non trovata", result.Message);
        }

        [Fact]
        public async Task UpdateAsync_DuplicateSigla_ShouldReturnError()
        {
            // Arrange
            var unita1 = await CreateTestUnitaDiMisuraAsync("SIG1", "Descrizione 1");
            var unita2 = await CreateTestUnitaDiMisuraAsync("SIG2", "Descrizione 2");

            var unitaDto = new UnitaDiMisuraDTO
            {
                UnitaMisuraId = unita1.UnitaMisuraId,
                Sigla = "SIG2", // Sigla già usata da unita2
                Descrizione = "Descrizione 1 Modificata"
            };

            // Act
            var result = await _repository.UpdateAsync(unitaDto);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Contains("Esiste già un'altra unità di misura con sigla 'SIG2'", result.Message);
        }

        [Fact]
        public async Task UpdateAsync_DuplicateDescrizione_ShouldReturnError()
        {
            // Arrange
            var unita1 = await CreateTestUnitaDiMisuraAsync("SIG1", "Descrizione 1");
            var unita2 = await CreateTestUnitaDiMisuraAsync("SIG2", "Descrizione 2");

            var unitaDto = new UnitaDiMisuraDTO
            {
                UnitaMisuraId = unita1.UnitaMisuraId,
                Sigla = "SIG1",
                Descrizione = "Descrizione 2" // Descrizione già usata da unita2
            };

            // Act
            var result = await _repository.UpdateAsync(unitaDto);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Contains("Esiste già un'altra unità di misura con descrizione 'Descrizione 2'", result.Message);
        }

        [Fact]
        public async Task UpdateAsync_NoChanges_ShouldReturnFalseWithMessage()
        {
            // Arrange
            var unita = await CreateTestUnitaDiMisuraAsync("TEST", "Test No Changes");
            var unitaDto = new UnitaDiMisuraDTO
            {
                UnitaMisuraId = unita.UnitaMisuraId,
                Sigla = "TEST",
                Descrizione = "Test No Changes"
            };

            // Act
            var result = await _repository.UpdateAsync(unitaDto);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.False(result.Data); // False perché non ci sono cambiamenti
            Assert.Contains($"Nessuna modifica necessaria", result.Message);
        }

        [Fact]
        public async Task UpdateAsync_NullDto_ShouldReturnErrorResponse()
        {
            // Act
            var result = await _repository.UpdateAsync(null!);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Contains("Errore interno", result.Message);
        }

        #endregion

        #region DeleteAsync Tests

        [Fact]
        public async Task DeleteAsync_ValidId_ShouldDeleteUnita()
        {
            // Arrange
            var unita = await CreateTestUnitaDiMisuraAsync("TEST", "Test Delete");
            var unitaId = unita.UnitaMisuraId;

            // Act
            var result = await _repository.DeleteAsync(unitaId);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.True(result.Data);
            Assert.Contains($"eliminata con successo", result.Message);

            // Verifica che sia stato eliminato dal database
            var deletedUnita = await _context.UnitaDiMisura.FindAsync(unitaId);
            Assert.Null(deletedUnita);
        }

        [Fact]
        public async Task DeleteAsync_NonExistentId_ShouldReturnNotFound()
        {
            // Act
            var result = await _repository.DeleteAsync(9999);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Contains($"Unità di misura con ID 9999 non trovata", result.Message);
        }

        [Fact]
        public async Task DeleteAsync_InvalidId_ShouldReturnError()
        {
            // Act
            var result = await _repository.DeleteAsync(0);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Contains("ID unità di misura non valido", result.Message);
        }

        [Fact]
        public async Task DeleteAsync_UnitaWithDependencies_ShouldReturnError()
        {
            // Arrange
            var unita = await CreateTestUnitaDiMisuraAsync("TEST", "Test Dependencies");

            // Crea un DimensioneBicchiere collegato all'unità
            var dimensione = new DimensioneBicchiere
            {
                Sigla = "M",
                Descrizione = "Medium",
                Capienza = 500.00m,
                UnitaMisuraId = unita.UnitaMisuraId,
                PrezzoBase = 3.50m,
                Moltiplicatore = 1.00m
            };
            _context.DimensioneBicchiere.Add(dimensione);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.DeleteAsync(unita.UnitaMisuraId);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Contains("Impossibile eliminare l'unità di misura perché ci sono dipendenze attive", result.Message);

            // Verifica che l'unità non sia stata eliminata
            var unitaEsisteAncora = await _context.UnitaDiMisura.FindAsync(unita.UnitaMisuraId);
            Assert.NotNull(unitaEsisteAncora);
        }

        #endregion

        #region Private Helper Methods Tests

        [Fact]
        public async Task HasDependenciesAsync_WithDependencies_ShouldReturnTrue()
        {
            // Arrange
            var unita = await CreateTestUnitaDiMisuraAsync("TEST", "Test Dependencies");

            // Crea una dipendenza
            var dimensione = new DimensioneBicchiere
            {
                Sigla = "T",
                Descrizione = "Test",
                Capienza = 100.00m,
                UnitaMisuraId = unita.UnitaMisuraId,
                PrezzoBase = 1.00m,
                Moltiplicatore = 1.00m
            };
            _context.DimensioneBicchiere.Add(dimensione);
            await _context.SaveChangesAsync();

            // Act - Usa reflection per testare il metodo privato
            var method = typeof(UnitaDiMisuraRepository)
                .GetMethod("HasDependenciesAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            var task = (Task<bool>)method!.Invoke(_repository, [unita.UnitaMisuraId])!;
            var result = await task;

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task HasDependenciesAsync_WithoutDependencies_ShouldReturnFalse()
        {
            // Arrange
            var unita = await CreateTestUnitaDiMisuraAsync("TEST", "Test No Dependencies");

            // Act - Usa reflection per testare il metodo privato
            var method = typeof(UnitaDiMisuraRepository)
                .GetMethod("HasDependenciesAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            var task = (Task<bool>)method!.Invoke(_repository, [unita.UnitaMisuraId])!;
            var result = await task;

            // Assert
            Assert.False(result);
        }

        #endregion

        #region Integration Tests

        [Fact]
        public async Task FullCrudFlow_ShouldWorkCorrectly()
        {
            // 1. CREATE
            var addDto = new UnitaDiMisuraDTO
            {
                Sigla = "FLOW",
                Descrizione = "Flow Test Unit"
            };
            var addResult = await _repository.AddAsync(addDto);
            Assert.True(addResult.Success);
            var unitaId = addResult.Data!.UnitaMisuraId;

            // 2. READ by ID
            var getResult = await _repository.GetByIdAsync(unitaId);
            Assert.True(getResult.Success);
            Assert.Equal("FLOW", getResult.Data!.Sigla);

            // 3. EXISTS
            var existsResult = await _repository.ExistsAsync(unitaId);
            Assert.True(existsResult.Success);
            Assert.True(existsResult.Data);

            // 4. Sigla EXISTS
            var siglaExistsResult = await _repository.SiglaExistsAsync("FLOW");
            Assert.True(siglaExistsResult.Success);
            Assert.True(siglaExistsResult.Data);

            // 5. UPDATE
            var updateDto = new UnitaDiMisuraDTO
            {
                UnitaMisuraId = unitaId,
                Sigla = "FLOW2",
                Descrizione = "Flow Test Unit Updated"
            };
            var updateResult = await _repository.UpdateAsync(updateDto);
            Assert.True(updateResult.Success);
            Assert.True(updateResult.Data);

            // 6. Verify update
            var verifyResult = await _repository.GetByIdAsync(unitaId);
            Assert.Equal("FLOW2", verifyResult.Data!.Sigla);

            // 7. DELETE
            var deleteResult = await _repository.DeleteAsync(unitaId);
            Assert.True(deleteResult.Success);
            Assert.True(deleteResult.Data);

            // 8. Verify deletion
            var finalExistsResult = await _repository.ExistsAsync(unitaId);
            Assert.False(finalExistsResult.Data);
        }

        #endregion
    }
}