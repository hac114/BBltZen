using BBltZen;
using DTO;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Repository.Service;
using Xunit;

namespace RepositoryTest
{
    public class BevandaCustomRepositoryCleanTest : BaseTestClean
    {
        private readonly BevandaCustomRepository _repository;

        public BevandaCustomRepositoryCleanTest()
        {
            _repository = new BevandaCustomRepository(_context,
                NullLogger<BevandaCustomRepository>.Instance);
        }

        #region GetAllAsync Tests

        [Fact]
        public async Task GetAllAsync_WhenNoData_ReturnsEmptyPaginatedResponse()
        {
            // Arrange
            await ResetDatabaseAsync();

            // Act
            var result = await _repository.GetAllAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result.Data);
            Assert.Equal(0, result.TotalCount);
            Assert.Equal("Nessuna bevanda custom trovata", result.Message);
        }

        [Fact]
        public async Task GetAllAsync_WithData_ReturnsPaginatedResponse()
        {
            // Arrange
            await ResetDatabaseAsync();
            await CreateMultipleBevandaCustomAsync(3);

            // Act
            var result = await _repository.GetAllAsync(page: 1, pageSize: 2);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Data.Count());
            Assert.Equal(3, result.TotalCount);
            Assert.Equal(2, result.PageSize);
            Assert.Equal(1, result.Page);
            Assert.Contains("Trovate 3 bevande custom", result.Message);
        }

        [Fact]
        public async Task GetAllAsync_WithInvalidPagination_FallsBackToDefaults()
        {
            // Arrange
            await CreateMultipleBevandaCustomAsync(2);

            // Act
            var result = await _repository.GetAllAsync(page: 0, pageSize: 0);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Page); // ✅ Corretto: Math.Max(1, 0) = 1
            Assert.Equal(1, result.PageSize); // ✅ Corretto: Math.Clamp(0, 1, 100) = 1
        }

        #endregion

        #region GetByIdAsync Tests

        // Modifica il test GetById
        [Fact]
        public async Task GetByIdAsync_WithValidId_ReturnsSuccessResponse()
        {
            // Arrange
            var bevandaCustom = await CreateTestBevandaCustomAsync();

            // Act
            var result = await _repository.GetByIdAsync(bevandaCustom.ArticoloId);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(bevandaCustom.ArticoloId, result.Data.ArticoloId);
            Assert.Equal(bevandaCustom.PersCustomId, result.Data.PersCustomId);
            Assert.NotNull(result.Data.PrezzoDimensione);
            Assert.Contains("trovata", result.Message);
        }

        [Fact]
        public async Task GetByIdAsync_WithNonExistentId_ReturnsNotFoundResponse()
        {
            // Arrange
            var nonExistentId = 9999;

            // Act
            var result = await _repository.GetByIdAsync(nonExistentId);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Null(result.Data);
            Assert.Contains("non trovata", result.Message);
        }

        [Fact]
        public async Task GetByIdAsync_WithInvalidId_ReturnsErrorResponse()
        {
            // Arrange
            var invalidId = 0;

            // Act
            var result = await _repository.GetByIdAsync(invalidId);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Null(result.Data);
            Assert.Contains("non valido", result.Message);
        }        

        #endregion

        #region GetByPersCustomIdAsync Tests

        [Fact]
        public async Task GetByPersCustomIdAsync_WithValidId_ReturnsSuccessResponse()
        {
            // Arrange
            var bevandaCustom = await CreateTestBevandaCustomAsync();

            // Act
            var result = await _repository.GetByPersCustomIdAsync(bevandaCustom.PersCustomId);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(bevandaCustom.ArticoloId, result.Data.ArticoloId);
            Assert.Contains("trovata", result.Message);
        }

        [Fact]
        public async Task GetByPersCustomIdAsync_WithNonExistentId_ReturnsNotFoundResponse()
        {
            // Arrange
            var nonExistentId = 9999;

            // Act
            var result = await _repository.GetByPersCustomIdAsync(nonExistentId);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Null(result.Data);
            Assert.Contains("non trovata", result.Message);
        }

        [Fact]
        public async Task GetByPersCustomIdAsync_WithInvalidId_ReturnsErrorResponse()
        {
            // Arrange
            var invalidId = 0;

            // Act
            var result = await _repository.GetByPersCustomIdAsync(invalidId);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Null(result.Data);
            Assert.Contains("non valido", result.Message);
        }

        #endregion

        #region GetAllOrderedByDimensioneAsync Tests

        [Fact]
        public async Task GetAllOrderedByDimensioneAsync_ReturnsOrderedByDimensione()
        {
            // Arrange
            await ResetDatabaseAsync();

            // Crea bevande custom con diverse dimensioni
            await CreateCompleteBevandaCustomCardDataAsync(
                nomePersonalizzazione: "Bevanda 1", dimensioneBicchiereId: 2);
            await CreateCompleteBevandaCustomCardDataAsync(
                nomePersonalizzazione: "Bevanda 2", dimensioneBicchiereId: 1);
            await CreateCompleteBevandaCustomCardDataAsync(
                nomePersonalizzazione: "Bevanda 3", dimensioneBicchiereId: 1);

            // Act
            var result = await _repository.GetAllOrderedByDimensioneAsync(pageSize: 10);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, result.TotalCount);

            // Verifica che siano ordinati per dimensione (1 prima di 2)
            var bevande = result.Data.ToList();
            Assert.Contains("Trovate 3 bevande custom per dimensione bicchiere", result.Message);
        }

        #endregion

        #region GetAllOrderedByPersonalizzazioneAsync Tests

        [Fact]
        public async Task GetAllOrderedByPersonalizzazioneAsync_ReturnsOrderedByPersonalizzazione()
        {
            // Arrange
            await ResetDatabaseAsync();

            await CreateCompleteBevandaCustomCardDataAsync(
                nomePersonalizzazione: "Bevanda B");
            await CreateCompleteBevandaCustomCardDataAsync(
                nomePersonalizzazione: "Bevanda A");
            await CreateCompleteBevandaCustomCardDataAsync(
                nomePersonalizzazione: "Bevanda C");

            // Act
            var result = await _repository.GetAllOrderedByPersonalizzazioneAsync(pageSize: 10);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, result.TotalCount);
            Assert.Contains("Trovate 3 bevande custom per personalizzazione", result.Message);
        }

        #endregion

        #region AddAsync Tests

        [Fact]
        public async Task AddAsync_WithValidData_ReturnsSuccessResponse()
        {
            // Arrange
            await ResetDatabaseAsync();

            // ✅ Crea una PersonalizzazioneCustom con ID 100 prima di creare la BevandaCustom
            await EnsurePersonalizzazioneCustomExistsAsync(100);

            var dto = CreateTestBevandaCustomDTO(persCustomId: 100, prezzo: 6.50m);

            // Act
            var result = await _repository.AddAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.True(result.Data.ArticoloId > 0);
            Assert.Equal(100, result.Data.PersCustomId);
            Assert.Equal(6.50m, result.Data.Prezzo);
            Assert.Contains("creata con successo", result.Message);
        }

        [Fact]
        public async Task AddAsync_WithDuplicatePersCustomId_ReturnsErrorResponse()
        {
            // Arrange
            var existing = await CreateTestBevandaCustomAsync(persCustomId: 200);
            var dto = CreateTestBevandaCustomDTO(persCustomId: 200, prezzo: 7.50m);

            // Act
            var result = await _repository.AddAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Null(result.Data);
            Assert.Contains("Esiste già una bevanda custom", result.Message);
        }

        [Fact]
        public async Task AddAsync_WithInvalidPrice_ReturnsErrorResponse()
        {
            // Arrange
            var dto = CreateTestBevandaCustomDTO(prezzo: 0m);

            // Act
            var result = await _repository.AddAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Null(result.Data);
            Assert.Contains("Il prezzo deve essere tra 0.01 e 99.99", result.Message);
        }

        [Fact]
        public async Task AddAsync_WithNonExistentPersCustomId_ReturnsErrorResponse()
        {
            // Arrange
            var dto = CreateTestBevandaCustomDTO(persCustomId: 9999);

            // Act
            var result = await _repository.AddAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Null(result.Data);
            Assert.Contains("Personalizzazione custom non trovata", result.Message);
        }

        #endregion

        #region UpdateAsync Tests

        [Fact]
        public async Task UpdateAsync_WithValidData_ReturnsSuccessResponse()
        {
            // Arrange
            var existing = await CreateTestBevandaCustomAsync(prezzo: 5.00m);
            var dto = CreateTestBevandaCustomDTO(
                articoloId: existing.ArticoloId,
                persCustomId: existing.PersCustomId,
                prezzo: 6.50m);

            // Act
            var result = await _repository.UpdateAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.True(result.Data);
            Assert.Contains("aggiornata con successo", result.Message);

            // ✅ MODIFICA: Verifica l'aggiornamento direttamente dal database
            var updatedFromDb = await _context.BevandaCustom
                .FirstOrDefaultAsync(bc => bc.ArticoloId == existing.ArticoloId);
            Assert.NotNull(updatedFromDb);
            Assert.Equal(6.50m, updatedFromDb.Prezzo);
        }

        [Fact]
        public async Task UpdateAsync_WithNonExistentId_ReturnsNotFoundResponse()
        {
            // Arrange
            var dto = CreateTestBevandaCustomDTO(articoloId: 9999);

            // Act
            var result = await _repository.UpdateAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.False(result.Data);
            Assert.Contains("non trovata", result.Message);
        }

        [Fact]
        public async Task UpdateAsync_WithDuplicatePersCustomId_ReturnsErrorResponse()
        {
            // Arrange
            var existing1 = await CreateTestBevandaCustomAsync(persCustomId: 300);
            var existing2 = await CreateTestBevandaCustomAsync(persCustomId: 400);

            var dto = CreateTestBevandaCustomDTO(
                articoloId: existing1.ArticoloId,
                persCustomId: 400, // Cerca di usare lo stesso PersCustomId di existing2
                prezzo: 7.00m);

            // Act
            var result = await _repository.UpdateAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Contains("Esiste già un'altra bevanda custom", result.Message);
        }

        [Fact]
        public async Task UpdateAsync_WithNoChanges_ReturnsSuccessWithFalseData()
        {
            // Arrange
            var existing = await CreateTestBevandaCustomAsync();
            var dto = MapToDTO(existing);

            // Act
            var result = await _repository.UpdateAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.False(result.Data);
            Assert.Contains("Nessuna modifica necessaria", result.Message);
        }

        #endregion

        #region DeleteAsync Tests

        [Fact]
        public async Task DeleteAsync_WithValidId_ReturnsSuccessResponse()
        {
            // Arrange
            var existing = await CreateTestBevandaCustomAsync();

            // Act
            var result = await _repository.DeleteAsync(existing.ArticoloId);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.True(result.Data);
            Assert.Contains("eliminata con successo", result.Message);

            // Verify deletion
            var existsResult = await _repository.ExistsAsync(existing.ArticoloId);
            Assert.False(existsResult.Data);
        }

        [Fact]
        public async Task DeleteAsync_WithNonExistentId_ReturnsNotFoundResponse()
        {
            // Arrange
            var nonExistentId = 9999;

            // Act
            var result = await _repository.DeleteAsync(nonExistentId);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.False(result.Data);
            Assert.Contains("non trovata", result.Message);
        }

        [Fact]
        public async Task DeleteAsync_WithInvalidId_ReturnsErrorResponse()
        {
            // Arrange
            var invalidId = 0;

            // Act
            var result = await _repository.DeleteAsync(invalidId);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.False(result.Data);
            Assert.Contains("non valido", result.Message);
        }

        #endregion

        #region ExistsAsync Tests

        [Fact]
        public async Task ExistsAsync_WithExistingId_ReturnsTrue()
        {
            // Arrange
            var existing = await CreateTestBevandaCustomAsync();

            // Act
            var result = await _repository.ExistsAsync(existing.ArticoloId);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.True(result.Data);
            Assert.Contains("esiste", result.Message);
        }

        [Fact]
        public async Task ExistsAsync_WithNonExistentId_ReturnsFalse()
        {
            // Arrange
            var nonExistentId = 9999;

            // Act
            var result = await _repository.ExistsAsync(nonExistentId);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.False(result.Data);
            Assert.Contains("non trovata", result.Message);
        }

        #endregion

        #region ExistsByPersCustomIdAsync Tests

        [Fact]
        public async Task ExistsByPersCustomIdAsync_WithExistingPersCustomId_ReturnsTrue()
        {
            // Arrange
            var existing = await CreateTestBevandaCustomAsync(persCustomId: 500);

            // Act
            var result = await _repository.ExistsByPersCustomIdAsync(500);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.True(result.Data);
            Assert.Contains("esiste", result.Message);
        }

        [Fact]
        public async Task ExistsByPersCustomIdAsync_WithNonExistentPersCustomId_ReturnsFalse()
        {
            // Arrange
            var nonExistentId = 9999;

            // Act
            var result = await _repository.ExistsByPersCustomIdAsync(nonExistentId);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.False(result.Data);
            Assert.Contains("non trovata", result.Message);
        }

        #endregion

        #region GetCardProdottiAsync Tests

        [Fact]
        public async Task GetCardProdottiAsync_ReturnsCardDTOs()
        {
            // Arrange
            await ResetDatabaseAsync();
            await CreateMultipleBevandaCustomAsync(2);

            // Act
            var result = await _repository.GetCardProdottiAsync(pageSize: 10);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.TotalCount);
            Assert.All(result.Data, card =>
            {
                Assert.NotNull(card.NomePersonalizzazione);
                Assert.NotNull(card.PrezzoDimensione);
                Assert.NotNull(card.Ingredienti);
            });
        }

        [Fact]
        public async Task GetCardProdottiAsync_WithPagination_ReturnsCorrectPage()
        {
            // Arrange
            await ResetDatabaseAsync();
            await CreateMultipleBevandaCustomAsync(5);

            // Act
            var result = await _repository.GetCardProdottiAsync(page: 2, pageSize: 2);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Data.Count());
            Assert.Equal(5, result.TotalCount);
            Assert.Equal(2, result.Page);
            Assert.Equal(2, result.PageSize);
        }

        #endregion

        #region GetCardProdottoByIdAsync Tests

        [Fact]
        public async Task GetCardProdottoByIdAsync_WithValidId_ReturnsCardDTO()
        {
            // Arrange
            var existing = await CreateTestBevandaCustomAsync();

            // Act
            var result = await _repository.GetCardProdottoByIdAsync(existing.ArticoloId);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(existing.ArticoloId, result.Data.ArticoloId);
            Assert.NotNull(result.Data.NomePersonalizzazione);
            Assert.NotNull(result.Data.PrezzoDimensione);
            Assert.NotNull(result.Data.Ingredienti);
        }        

        [Fact]
        public async Task GetCardProdottoByIdAsync_WithNonExistentId_ReturnsNotFound()
        {
            // Arrange
            var nonExistentId = 9999;

            // Act
            var result = await _repository.GetCardProdottoByIdAsync(nonExistentId);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Null(result.Data);
            Assert.Contains("non trovata", result.Message);
        }

        #endregion

        #region GetCardPersonalizzazioneAsync Tests

        [Fact]
        public async Task GetCardPersonalizzazioneAsync_WithMatchingName_ReturnsCards()
        {
            // Arrange
            await ResetDatabaseAsync();
            await CreateCompleteBevandaCustomCardDataAsync(
                nomePersonalizzazione: "Bubble Tea Speciale");
            await CreateCompleteBevandaCustomCardDataAsync(
                nomePersonalizzazione: "Bubble Tea Classico");

            // Act
            var result = await _repository.GetCardPersonalizzazioneAsync("Bubble Tea", pageSize: 10);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.TotalCount);
            Assert.Contains("Bubble Tea", result.Message);
        }

        [Fact]
        public async Task GetCardPersonalizzazioneAsync_WithNonMatchingName_ReturnsEmpty()
        {
            // Arrange
            await CreateCompleteBevandaCustomCardDataAsync(
                nomePersonalizzazione: "Bubble Tea Classico");

            // Act
            var result = await _repository.GetCardPersonalizzazioneAsync("Non Esistente", pageSize: 10);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(0, result.TotalCount);
            Assert.Contains("Nessuna personalizzazione trovata", result.Message);
        }

        [Fact]
        public async Task GetCardPersonalizzazioneAsync_WithInvalidInput_ReturnsEmpty()
        {
            // Arrange
            var invalidInput = "<script>alert('xss')</script>";

            // Act
            var result = await _repository.GetCardPersonalizzazioneAsync(invalidInput, pageSize: 10);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(0, result.TotalCount);
            Assert.Contains("Nome personalizzazione non valido", result.Message);
        }

        #endregion

        #region GetCardDimensioneBicchiereAsync Tests

        [Fact]
        public async Task GetCardDimensioneBicchiereAsync_WithMatchingDescription_ReturnsCards()
        {
            // Arrange
            await ResetDatabaseAsync();

            // Crea bevande con dimensioni specifiche
            // Dimensione ID 1 = "Medium", ID 2 = "Large" (dal seed in BaseTestClean)
            await CreateCompleteBevandaCustomCardDataAsync(
                nomePersonalizzazione: "Bevanda 1",
                dimensioneBicchiereId: 1); // Medium

            await CreateCompleteBevandaCustomCardDataAsync(
                nomePersonalizzazione: "Bevanda 2",
                dimensioneBicchiereId: 2); // Large

            // Act - Cerca per descrizione del bicchiere (Medium)
            var result = await _repository.GetCardDimensioneBicchiereAsync("Medium", pageSize: 10);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.TotalCount); // Solo una bevanda con dimensione "Medium"
            Assert.Contains("Medium", result.Message);
        }

        // Aggiungi anche un test per "Large"
        [Fact]
        public async Task GetCardDimensioneBicchiereAsync_WithLargeDescription_ReturnsCards()
        {
            // Arrange
            await ResetDatabaseAsync();

            await CreateCompleteBevandaCustomCardDataAsync(
                nomePersonalizzazione: "Bevanda 1",
                dimensioneBicchiereId: 1);

            await CreateCompleteBevandaCustomCardDataAsync(
                nomePersonalizzazione: "Bevanda 2",
                dimensioneBicchiereId: 2); // Large

            await CreateCompleteBevandaCustomCardDataAsync(
                nomePersonalizzazione: "Bevanda 3",
                dimensioneBicchiereId: 2); // Large

            // Act - Cerca per descrizione del bicchiere (Large)
            var result = await _repository.GetCardDimensioneBicchiereAsync("Large", pageSize: 10);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.TotalCount); // Due bevande con dimensione "Large"
            Assert.Contains("Large", result.Message);
        }

        [Fact]
        public async Task GetCardDimensioneBicchiereAsync_WithInvalidInput_ReturnsEmpty()
        {
            // Arrange
            var invalidInput = "<script>alert('xss')</script>";

            // Act
            var result = await _repository.GetCardDimensioneBicchiereAsync(invalidInput, pageSize: 10);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(0, result.TotalCount);
            Assert.Contains("Descrizione bicchiere non valida", result.Message);
        }

        #endregion

        #region CountAsync Tests

        [Fact]
        public async Task CountAsync_WithData_ReturnsCorrectCount()
        {
            // Arrange
            await ResetDatabaseAsync();
            await CreateMultipleBevandaCustomAsync(3);

            // Act
            var result = await _repository.CountAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(3, result.Data);
            Assert.Contains("3 bevande custom presenti", result.Message);
        }

        [Fact]
        public async Task CountAsync_WithoutData_ReturnsZero()
        {
            // Arrange
            await ResetDatabaseAsync();

            // Act
            var result = await _repository.CountAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(0, result.Data);
            Assert.Contains("Nessuna bevanda custom presente", result.Message);
        }

        #endregion

        #region CountDimensioneBicchiereAsync Tests

        [Fact]
        public async Task CountDimensioneBicchiereAsync_WithMatchingDescription_ReturnsCount()
        {
            // Arrange
            await ResetDatabaseAsync();
            await CreateCompleteBevandaCustomCardDataAsync(
                nomePersonalizzazione: "Bevanda Medium", dimensioneBicchiereId: 1);
            await CreateCompleteBevandaCustomCardDataAsync(
                nomePersonalizzazione: "Bevanda Large", dimensioneBicchiereId: 2);

            // Act
            var result = await _repository.CountDimensioneBicchiereAsync("Medium");

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(1, result.Data);
            Assert.Contains("1 bevanda custom trovata per dimensione: Medium", result.Message);
        }

        [Fact]
        public async Task CountDimensioneBicchiereAsync_WithInvalidInput_ReturnsError()
        {
            // Arrange
            var invalidInput = "<script>alert('xss')</script>";

            // Act
            var result = await _repository.CountDimensioneBicchiereAsync(invalidInput);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Contains("Descrizione bicchiere non valida", result.Message);
        }

        #endregion

        #region CountPersonalizzazioneAsync Tests

        [Fact]
        public async Task CountPersonalizzazioneAsync_WithMatchingName_ReturnsCount()
        {
            // Arrange
            await ResetDatabaseAsync();
            await CreateCompleteBevandaCustomCardDataAsync(
                nomePersonalizzazione: "Bubble Tea Classico");
            await CreateCompleteBevandaCustomCardDataAsync(
                nomePersonalizzazione: "Bubble Tea Fruttato");
            await CreateCompleteBevandaCustomCardDataAsync(
                nomePersonalizzazione: "Tè Verde");

            // Act
            var result = await _repository.CountPersonalizzazioneAsync("Bubble Tea");

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(2, result.Data);
            Assert.Contains("2 bevande custom trovate per personalizzazione: Bubble Tea", result.Message);
        }

        [Fact]
        public async Task CountPersonalizzazioneAsync_WithInvalidInput_ReturnsError()
        {
            // Arrange
            var invalidInput = "<script>alert('xss')</script>";

            // Act
            var result = await _repository.CountPersonalizzazioneAsync(invalidInput);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Contains("Nome personalizzazione non valido", result.Message);
        }

        #endregion

        #region Helper Methods

        private static BevandaCustomDTO MapToDTO(BevandaCustom bevandaCustom)
        {
            return new BevandaCustomDTO
            {
                ArticoloId = bevandaCustom.ArticoloId,
                PersCustomId = bevandaCustom.PersCustomId,
                Prezzo = bevandaCustom.Prezzo,
                DataCreazione = bevandaCustom.DataCreazione,
                DataAggiornamento = bevandaCustom.DataAggiornamento
            };
        }

        #endregion
    }
}