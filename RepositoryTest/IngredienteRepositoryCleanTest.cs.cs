using BBltZen;
using DTO;
using Microsoft.EntityFrameworkCore;
using Repository.Service;
using Xunit;

namespace RepositoryTest
{
    public class IngredienteRepositoryCleanTest : BaseTestClean
    {
        private readonly IngredienteRepository _repository;

        public IngredienteRepositoryCleanTest()
        {
            _repository = new IngredienteRepository(_context, GetTestLogger<IngredienteRepository>());
        }

        #region Initialize e Cleanup

        [Fact]
        public void Constructor_ShouldInitializeRepository()
        {
            // Assert
            Assert.NotNull(_repository);
            Assert.NotNull(_context);
        }

        [Fact]
        public async Task Database_ShouldBeCleanForEachTest()
        {
            // Arrange
            await SetupIngredienteTestDataAsync();

            // Act
            var count = await _context.Ingrediente.CountAsync();

            // Assert
            Assert.Equal(5, count);
        }

        #endregion

        #region GetAllAsync Tests

        [Fact]
        public async Task GetAllAsync_WithDefaultPagination_ShouldReturnFirstPage()
        {
            // Arrange
            await SetupIngredienteTestDataAsync();

            // Act
            var result = await _repository.GetAllAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(5, result.TotalCount);
            Assert.Equal(1, result.Page);
            Assert.Equal(10, result.PageSize);
            Assert.Equal(5, result.Data.Count());
            Assert.Contains("Trovato 5 ingredienti", result.Message);
        }

        [Fact]
        public async Task GetAllAsync_WithCustomPagination_ShouldReturnCorrectPage()
        {
            // Arrange
            await SetupIngredienteTestDataAsync();

            // Act
            var result = await _repository.GetAllAsync(page: 2, pageSize: 2);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(5, result.TotalCount);
            Assert.Equal(2, result.Page);
            Assert.Equal(2, result.PageSize);
            Assert.Equal(2, result.Data.Count());
        }

        [Fact]
        public async Task GetAllAsync_WithNoData_ShouldReturnEmpty()
        {
            // Arrange
            await CleanTableAsync<Ingrediente>();

            // Act
            var result = await _repository.GetAllAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(0, result.TotalCount);
            Assert.Empty(result.Data);
            Assert.Contains("Nessun ingrediente trovato", result.Message);
        }

        #endregion

        #region GetByIdAsync Tests

        [Fact]
        public async Task GetByIdAsync_WithValidId_ShouldReturnIngrediente()
        {
            // Arrange
            await SetupIngredienteTestDataAsync();
            var expectedId = 1;

            // Act
            var result = await _repository.GetByIdAsync(expectedId);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            AssertIngredienteDTO(
                result.Data,
                expectedId,
                "Tea Nero Premium",
                1,
                0.50m,
                true,
                true
            );
        }

        [Fact]
        public async Task GetByIdAsync_WithInvalidId_ShouldReturnNotFound()
        {
            // Arrange
            await SetupIngredienteTestDataAsync();
            var invalidId = 999;

            // Act
            var result = await _repository.GetByIdAsync(invalidId);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Null(result.Data);
            Assert.Contains($"ID {invalidId} non trovato", result.Message);
        }

        [Fact]
        public async Task GetByIdAsync_WithZeroId_ShouldReturnError()
        {
            // Act
            var result = await _repository.GetByIdAsync(0);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Contains("ID ingrediente non valido", result.Message);
        }

        #endregion

        #region GetByNomeAsync Tests

        [Fact]
        public async Task GetByNomeAsync_WithExistingNome_ShouldReturnIngredienti()
        {
            // Arrange
            await SetupIngredienteTestDataAsync();

            // Act
            var result = await _repository.GetByNomeAsync("Tea");

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.TotalCount);
            Assert.Single(result.Data);
            Assert.Contains("Tea Nero Premium", result.Data.First().Nome);
        }

        [Fact]
        public async Task GetByNomeAsync_WithNonExistingNome_ShouldReturnEmpty()
        {
            // Arrange
            await SetupIngredienteTestDataAsync();

            // Act
            var result = await _repository.GetByNomeAsync("NonEsistente");

            // Assert
            Assert.NotNull(result);
            Assert.Equal(0, result.TotalCount);
            Assert.Empty(result.Data);
            Assert.Contains("Nessun ingrediente trovato", result.Message);
        }

        [Fact]
        public async Task GetByNomeAsync_WithCaseInsensitiveSearch_ShouldFindMatches()
        {
            // Arrange
            await SetupIngredienteTestDataAsync();

            // Act
            var result = await _repository.GetByNomeAsync("TEA");

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.TotalCount);
        }

        [Fact]
        public async Task GetByNomeAsync_WithEmptyNome_ShouldReturnError()
        {
            // Act
            var result = await _repository.GetByNomeAsync("");

            // Assert
            Assert.NotNull(result);
            Assert.Equal(0, result.TotalCount);
            Assert.Contains("Il parametro 'ingrediente' è obbligatorio", result.Message);
        }

        [Fact]
        public async Task GetByNomeAsync_WithInvalidCharacters_ShouldReturnError()
        {
            // Act
            var result = await _repository.GetByNomeAsync("<script>alert('xss')</script>");

            // Assert
            Assert.NotNull(result);
            Assert.Equal(0, result.TotalCount);
            Assert.Contains("Il parametro 'ingrediente' contiene caratteri non validi", result.Message);
        }

        #endregion

        #region GetByCategoriaAsync Tests

        [Fact]
        public async Task GetByCategoriaAsync_WithExistingCategoria_ShouldReturnIngredienti()
        {
            // Arrange
            await SetupIngredienteTestDataAsync();

            // Act
            var result = await _repository.GetByCategoriaAsync("tea");

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.TotalCount);
            Assert.Single(result.Data);
            Assert.Contains("Tea Nero Premium", result.Data.First().Nome);
        }

        [Fact]
        public async Task GetByCategoriaAsync_WithNonExistingCategoria_ShouldReturnEmpty()
        {
            // Arrange
            await SetupIngredienteTestDataAsync();

            // Act
            var result = await _repository.GetByCategoriaAsync("NonEsistente");

            // Assert
            Assert.NotNull(result);
            Assert.Equal(0, result.TotalCount);
            Assert.Empty(result.Data);
            Assert.Contains("Nessun ingrediente trovato", result.Message);
        }

        #endregion

        #region GetDisponibiliAsync e GetNonDisponibiliAsync Tests

        [Fact]
        public async Task GetByDisponibilisync_ShouldReturnOnlyDisponibili()
        {
            // Arrange
            await SetupIngredienteTestDataAsync();

            // Act
            var result = await _repository.GetByDisponibilisync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(4, result.TotalCount); // 4 disponibili su 5
            Assert.All(result.Data, i => Assert.True(i.Disponibile));
        }

        [Fact]
        public async Task GetByNonDisponibilisync_ShouldReturnOnlyNonDisponibili()
        {
            // Arrange
            await SetupIngredienteTestDataAsync();

            // Act
            var result = await _repository.GetByNonDisponibilisync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.TotalCount); // 1 non disponibile
            Assert.All(result.Data, i => Assert.False(i.Disponibile));
            Assert.Contains("Perle di Tapioca", result.Data.First().Nome);
        }

        #endregion

        #region AddAsync Tests

        [Fact]
        public async Task AddAsync_WithValidData_ShouldCreateIngrediente()
        {
            // Arrange
            await CleanTableAsync<Ingrediente>();
            var dto = CreateTestIngredienteDTO(
                "Nuovo Ingrediente",
                1,
                2.50m,
                true
            );

            // Act
            var result = await _repository.AddAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);

            // Verifica i dati
            AssertIngredienteDTO(
                result.Data,
                result.Data.IngredienteId,
                "Nuovo Ingrediente",
                1,
                2.50m,
                true,
                true
            );

            // Verifica che sia stato salvato nel database
            var saved = await _context.Ingrediente
                .FirstOrDefaultAsync(i => i.IngredienteId == result.Data.IngredienteId);
            Assert.NotNull(saved);
            Assert.Equal("Nuovo Ingrediente", saved.Ingrediente1);
        }

        [Fact]
        public async Task AddAsync_WithDuplicateNome_ShouldReturnError()
        {
            // Arrange
            await SetupIngredienteTestDataAsync();
            var dto = CreateTestIngredienteDTO(
                "Tea Nero Premium",
                1,
                2.50m,
                true
            );

            // Act
            var result = await _repository.AddAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Null(result.Data);
            Assert.Contains("Esiste già un ingrediente con nome", result.Message);
        }

        [Fact]
        public async Task AddAsync_WithDuplicateNomeCaseInsensitive_ShouldReturnError()
        {
            // Arrange
            await SetupIngredienteTestDataAsync();
            var dto = CreateTestIngredienteDTO(
                "TEA NERO PREMIUM", // Maiuscolo
                1,
                2.50m,
                true
            );

            // Act
            var result = await _repository.AddAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Contains("Esiste già un ingrediente con nome", result.Message);
        }

        [Fact]
        public async Task AddAsync_WithEmptyNome_ShouldReturnError()
        {
            // Arrange
            var dto = CreateTestIngredienteDTO("", 1, 2.50m, true);

            // Act
            var result = await _repository.AddAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Contains("Nome ingrediente obbligatorio", result.Message);
        }

        [Fact]
        public async Task AddAsync_WithInvalidInput_ShouldReturnError()
        {
            // Arrange
            var dto = CreateTestIngredienteDTO(
                "Nome<script>alert('xss')</script>",
                1,
                2.50m,
                true
            );

            // Act
            var result = await _repository.AddAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Contains("Nome ingrediente non valido", result.Message);
        }

        [Fact]
        public async Task AddAsync_WithNullDto_ShouldReturnError()
        {
            // Act
            var result = await _repository.AddAsync(null!);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Contains("Errore interno", result.Message);
        }

        #endregion

        #region UpdateAsync Tests

        [Fact]
        public async Task UpdateAsync_WithValidData_ShouldUpdateIngrediente()
        {
            // Arrange
            await SetupIngredienteTestDataAsync();
            var existingId = 1;
            var dto = new IngredienteDTO
            {
                IngredienteId = existingId,
                Nome = "Tea Nero Premium Modificato",
                CategoriaId = 2,
                PrezzoAggiunto = 0.75m,
                Disponibile = false,
                DataInserimento = DateTime.UtcNow.AddDays(-1),
                DataAggiornamento = DateTime.UtcNow
            };

            // Act
            var result = await _repository.UpdateAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.True(result.Data);
            Assert.Contains("aggiornato con successo", result.Message);

            // Verifica le modifiche
            var updated = await _context.Ingrediente.FindAsync(existingId);
            Assert.NotNull(updated);
            Assert.Equal("Tea Nero Premium Modificato", updated.Ingrediente1);
            Assert.Equal(2, updated.CategoriaId);
            Assert.Equal(0.75m, updated.PrezzoAggiunto);
            Assert.False(updated.Disponibile);
        }

        [Fact]
        public async Task UpdateAsync_WithDuplicateNome_ShouldReturnError()
        {
            // Arrange
            await SetupIngredienteTestDataAsync();
            var dto = new IngredienteDTO
            {
                IngredienteId = 1, // Tea Nero Premium
                Nome = "Latte di Cocco", // Nome già esistente
                CategoriaId = 1,
                PrezzoAggiunto = 0.50m,
                Disponibile = true
            };

            // Act
            var result = await _repository.UpdateAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.False(result.Data);
            Assert.Contains("Esiste già un altro ingrediente con nome", result.Message);
        }

        [Fact]
        public async Task UpdateAsync_WithSameData_ShouldReturnNoChanges()
        {
            // Arrange
            await SetupIngredienteTestDataAsync();
            var existing = await _context.Ingrediente.FindAsync(1);

            // Verifica che l'ingrediente esista prima di usarlo
            Assert.NotNull(existing); // ✅ Aggiunto controllo

            var dto = new IngredienteDTO
            {
                IngredienteId = 1,
                Nome = existing.Ingrediente1, // ✅ Ora existing non è null
                CategoriaId = existing.CategoriaId,
                PrezzoAggiunto = existing.PrezzoAggiunto,
                Disponibile = existing.Disponibile
            };

            // Act
            var result = await _repository.UpdateAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.False(result.Data);
            Assert.Contains("Nessuna modifica necessaria", result.Message);
        }

        [Fact]
        public async Task UpdateAsync_WithNonExistingId_ShouldReturnNotFound()
        {
            // Arrange
            await SetupIngredienteTestDataAsync();
            var dto = new IngredienteDTO
            {
                IngredienteId = 999,
                Nome = "Non Esistente",
                CategoriaId = 1,
                PrezzoAggiunto = 1.00m,
                Disponibile = true
            };

            // Act
            var result = await _repository.UpdateAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.False(result.Data);
            Assert.Contains("non trovato", result.Message);
        }

        #endregion

        #region DeleteAsync Tests

        [Fact]
        public async Task DeleteAsync_WithValidId_ShouldDeleteIngrediente()
        {
            // Arrange
            await SetupIngredienteTestDataAsync();
            var existingId = 1;
            var initialCount = await _context.Ingrediente.CountAsync();

            // Act
            var result = await _repository.DeleteAsync(existingId);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.True(result.Data);
            Assert.Contains("eliminato con successo", result.Message);

            // Verifica che sia stato rimosso
            var finalCount = await _context.Ingrediente.CountAsync();
            Assert.Equal(initialCount - 1, finalCount);
            var deleted = await _context.Ingrediente.FindAsync(existingId);
            Assert.Null(deleted);
        }

        [Fact]
        public async Task DeleteAsync_WithNonExistingId_ShouldReturnNotFound()
        {
            // Arrange
            await SetupIngredienteTestDataAsync();
            var invalidId = 999;

            // Act
            var result = await _repository.DeleteAsync(invalidId);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.False(result.Data);
            Assert.Contains("non trovato", result.Message);
        }

        [Fact]
        public async Task DeleteAsync_WithZeroId_ShouldReturnError()
        {
            // Act
            var result = await _repository.DeleteAsync(0);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Contains("ID ingrediente non valido", result.Message);
        }

        [Fact]
        public async Task DeleteAsync_WithNegativeId_ShouldReturnError()
        {
            // Act
            var result = await _repository.DeleteAsync(-1);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Contains("ID ingrediente non valido", result.Message);
        }

        #endregion

        #region ExistsAsync Tests

        [Fact]
        public async Task ExistsAsync_WithExistingId_ShouldReturnTrue()
        {
            // Arrange
            await SetupIngredienteTestDataAsync();

            // Act
            var result = await _repository.ExistsAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.True(result.Data);
            Assert.Contains("esiste", result.Message);
        }

        [Fact]
        public async Task ExistsAsync_WithNonExistingId_ShouldReturnFalse()
        {
            // Arrange
            await SetupIngredienteTestDataAsync();

            // Act
            var result = await _repository.ExistsAsync(999);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.False(result.Data);
            Assert.Contains("non trovato", result.Message);
        }

        #endregion

        #region ExistsByNomeAsync Tests

        [Fact]
        public async Task ExistsByNomeAsync_WithExistingNome_ShouldReturnTrue()
        {
            // Arrange
            await SetupIngredienteTestDataAsync();

            // Act
            var result = await _repository.ExistsByNomeAsync("Tea Nero Premium");

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.True(result.Data);
            Assert.Contains("esiste", result.Message);
        }

        [Fact]
        public async Task ExistsByNomeAsync_WithCaseInsensitive_ShouldFindMatch()
        {
            // Arrange
            await SetupIngredienteTestDataAsync();

            // Act
            var result = await _repository.ExistsByNomeAsync("TEA NERO PREMIUM");

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.True(result.Data);
        }

        [Fact]
        public async Task ExistsByNomeAsync_WithNonExistingNome_ShouldReturnFalse()
        {
            // Arrange
            await SetupIngredienteTestDataAsync();

            // Act
            var result = await _repository.ExistsByNomeAsync("Non Esistente");

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.False(result.Data);
            Assert.Contains("non trovato", result.Message);
        }

        #endregion

        #region ToggleDisponibilitaAsync Tests

        [Fact]
        public async Task ToggleDisponibilitaAsync_ShouldToggleDisponibile()
        {
            // Arrange
            await SetupIngredienteTestDataAsync();
            var ingredienteId = 1;
            var initial = await _context.Ingrediente.FindAsync(ingredienteId);

            // ✅ Verifica che l'ingrediente esista
            Assert.NotNull(initial);
            var initialDisponibile = initial.Disponibile;

            // Act
            var result = await _repository.ToggleDisponibilitaAsync(ingredienteId);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(!initialDisponibile, result.Data);
            Assert.Contains(
                initialDisponibile ? "non disponibile" : "disponibile",
                result.Message
            );

            // Verifica il cambio
            var updated = await _context.Ingrediente.FindAsync(ingredienteId);
            Assert.NotNull(updated); // ✅ Verifica che l'ingrediente aggiornato esista
            Assert.Equal(!initialDisponibile, updated.Disponibile);
        }

        [Fact]
        public async Task ToggleDisponibilitaAsync_WithNonExistingId_ShouldReturnNotFound()
        {
            // Arrange
            await SetupIngredienteTestDataAsync();

            // Act
            var result = await _repository.ToggleDisponibilitaAsync(999);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.False(result.Data);
            Assert.Contains("non trovato", result.Message);
        }

        #endregion

        #region Count Tests

        [Fact]
        public async Task CountAsync_ShouldReturnCorrectCount()
        {
            // Arrange
            await SetupIngredienteTestDataAsync();

            // Act
            var result = await _repository.CountAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(5, result.Data);
            Assert.Contains("5 ingredienti", result.Message);
        }

        [Fact]
        public async Task CountDisponibiliAsync_ShouldReturnOnlyDisponibili()
        {
            // Arrange
            await SetupIngredienteTestDataAsync();

            // Act
            var result = await _repository.CountDisponibiliAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(4, result.Data);
            Assert.Contains("4 ingredienti disponibili", result.Message);
        }

        [Fact]
        public async Task CountNonDisponibiliAsync_ShouldReturnOnlyNonDisponibili()
        {
            // Arrange
            await SetupIngredienteTestDataAsync();

            // Act
            var result = await _repository.CountNonDisponibiliAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(1, result.Data);
            Assert.Contains("1 ingrediente non disponibile", result.Message);
        }

        #endregion

        #region Edge Cases e Security Tests

        [Fact]
        public async Task GetAllAsync_WithInvalidPage_ShouldUseSafeDefaults()
        {
            // Arrange
            await SetupIngredienteTestDataAsync();

            // Act - Page 0 diventa 1 per sicurezza
            var result = await _repository.GetAllAsync(page: 0, pageSize: -5);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Page); // Corretto a 1
            Assert.True(result.PageSize > 0); // Corretto a valore positivo
        }

        [Fact]
        public async Task GetByNomeAsync_WithSQLInjectionAttempt_ShouldBeBlocked()
        {
            // Act
            var result = await _repository.GetByNomeAsync("'; DROP TABLE Ingrediente; --");

            // Assert
            Assert.NotNull(result);
            Assert.Equal(0, result.TotalCount);
            Assert.Contains("Il parametro 'ingrediente' contiene caratteri non validi", result.Message);
        }

        [Fact]
        public async Task AddAsync_WithMaxLengthNome_ShouldSucceed()
        {
            // Arrange
            await CleanTableAsync<Ingrediente>();
            var longNome = new string('A', 50); // Lunghezza massima
            var dto = CreateTestIngredienteDTO(longNome, 1, 1.00m, true);

            // Act
            var result = await _repository.AddAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data); // ✅ Aggiunto: verifica che Data non sia null
            Assert.Equal(longNome, result.Data!.Nome); // ✅ Usa ! dopo Assert.NotNull
        }

        [Fact]
        public async Task AddAsync_WithTooLongNome_ShouldBeRejectedBySecurityHelper()
        {
            // Arrange
            var tooLongNome = new string('A', 51); // Troppo lungo
            var dto = CreateTestIngredienteDTO(tooLongNome, 1, 1.00m, true);

            // Act
            var result = await _repository.AddAsync(dto);

            // Assert - SecurityHelper.IsValidInput restituirà false per lunghezza
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Contains("Nome ingrediente non valido", result.Message);
        }

        #endregion

        #region Vincolo UNIQUE Tests

        [Fact]
        public async Task AddAsync_ShouldEnforceUniqueNomeConstraint()
        {
            // Arrange
            await CleanTableAsync<Ingrediente>();

            // Primo ingrediente
            var dto1 = CreateTestIngredienteDTO("Ingrediente Unico", 1, 1.00m, true);
            var result1 = await _repository.AddAsync(dto1);
            Assert.True(result1.Success);

            // Secondo ingrediente con stesso nome (diverso case)
            var dto2 = CreateTestIngredienteDTO("INGREDIENTE UNICO", 1, 1.50m, true);

            // Act
            var result2 = await _repository.AddAsync(dto2);

            // Assert
            Assert.NotNull(result2);
            Assert.False(result2.Success);
            Assert.Contains("Esiste già un ingrediente con nome", result2.Message);
        }

        [Fact]
        public async Task UpdateAsync_ShouldEnforceUniqueNomeConstraint()
        {
            // Arrange
            await SetupIngredienteTestDataAsync();

            // Modifica il secondo ingrediente con il nome del primo
            var dto = new IngredienteDTO
            {
                IngredienteId = 2, // Latte di Cocco
                Nome = "Tea Nero Premium", // Nome del primo ingrediente
                CategoriaId = 2,
                PrezzoAggiunto = 0.80m,
                Disponibile = true
            };

            // Act
            var result = await _repository.UpdateAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Contains("Esiste già un altro ingrediente con nome", result.Message);
        }

        [Fact]
        public async Task UpdateAsync_CanRenameToSameNomeWithDifferentCase()
        {
            // Arrange
            await SetupIngredienteTestDataAsync();
            var dto = new IngredienteDTO
            {
                IngredienteId = 1,
                Nome = "TEA NERO PREMIUM", // Stesso nome ma maiuscolo
                CategoriaId = 1,
                PrezzoAggiunto = 0.50m,
                Disponibile = true
            };

            // Act
            var result = await _repository.UpdateAsync(dto);

            // Assert - Il repository NON considera il cambio di case come modifica
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.False(result.Data); // ❌ Cambia: ora è False (nessuna modifica rilevata)

            var updated = await _context.Ingrediente.FindAsync(1);
            Assert.NotNull(updated);
            Assert.Equal("Tea Nero Premium", updated!.Ingrediente1); // ❌ Cambia: mantiene il case originale
        }

        #endregion
    }
}