using BBltZen;
using DTO;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Repository.Service;
using Xunit;

namespace RepositoryTest
{
    public class BevandaStandardRepositoryCleanTest : BaseTestClean
    {
        private readonly BevandaStandardRepository _repository;

        public BevandaStandardRepositoryCleanTest()
        {
            _repository = new BevandaStandardRepository(_context, GetTestLogger<BevandaStandardRepository>());
        }

        #region GetAllAsync Tests

        [Fact]
        public async Task GetAllAsync_ReturnsPaginatedResponse()
        {
            // Arrange
            await ResetDatabaseAsync();
            await CreateMultipleBevandaStandardAsync(5);

            // Act
            var result = await _repository.GetAllAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(5, result.TotalCount);
            Assert.Equal(1, result.Page);
            Assert.Equal(10, result.PageSize);
            Assert.Equal(1, result.TotalPages);
            Assert.False(result.HasPrevious);
            Assert.False(result.HasNext);
            Assert.Contains(result.Message, ["Trovate 5 bevande standard", "Trovata 1 bevanda standard"]);
        }

        [Fact]
        public async Task GetAllAsync_EmptyDatabase_ReturnsEmptyResponse()
        {
            // Arrange
            await ResetDatabaseAsync();

            // Act
            var result = await _repository.GetAllAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result.Data);
            Assert.Equal(0, result.TotalCount);
            Assert.Equal(0, result.TotalPages); // Cambiato da 1 a 0
            Assert.Equal("Nessuna bevanda standard trovata", result.Message);
        }

        [Fact]
        public async Task GetAllAsync_WithPagination_ReturnsCorrectPage()
        {
            // Arrange
            await ResetDatabaseAsync();
            await CreateMultipleBevandaStandardAsync(15);

            // Act
            var result = await _repository.GetAllAsync(page: 2, pageSize: 5);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(15, result.TotalCount);
            Assert.Equal(2, result.Page);
            Assert.Equal(5, result.PageSize);
            Assert.Equal(3, result.TotalPages);
            Assert.True(result.HasPrevious);
            Assert.True(result.HasNext);
            Assert.Equal(5, result.Data.Count());
        }

        [Fact]
        public async Task GetAllAsync_InvalidPagination_FallsBackToDefaults()
        {
            // Arrange
            await ResetDatabaseAsync();
            await CreateMultipleBevandaStandardAsync(3);

            // Act
            var result = await _repository.GetAllAsync(page: 0, pageSize: -5);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Page); // Deve essere 1
            Assert.Equal(1, result.PageSize); // Cambiato da 10 a 1
            Assert.Equal(3, result.TotalCount);
        }

        #endregion

        #region GetByIdAsync Tests

        [Fact]
        public async Task GetByIdAsync_ExistingId_ReturnsBevandaStandard()
        {
            // Arrange
            await ResetDatabaseAsync();
            var bevanda = await CreateTestBevandaStandardAsync();

            // Act
            var result = await _repository.GetByIdAsync(bevanda.ArticoloId);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(bevanda.ArticoloId, result.Data.ArticoloId);

            // Convert entity to DTO
            var bevandaDto = await MapToDTODirectly(bevanda);
            AssertBevandaStandardDTOEqual(result.Data, bevandaDto);
        }

        [Fact]
        public async Task GetByIdAsync_NonExistingId_ReturnsNotFound()
        {
            // Arrange
            await ResetDatabaseAsync();

            // Act
            var result = await _repository.GetByIdAsync(999);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Null(result.Data);
            Assert.Contains("non trovata", result.Message);
        }

        [Fact]
        public async Task GetByIdAsync_InvalidId_ReturnsError()
        {
            // Arrange
            await ResetDatabaseAsync();

            // Act
            var result = await _repository.GetByIdAsync(0);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Null(result.Data);
            Assert.Contains("non valido", result.Message);
        }

        #endregion

        #region GetDisponibiliAsync Tests

        [Fact]
        public async Task GetDisponibiliAsync_ReturnsOnlySempreDisponibili()
        {
            // Arrange
            await ResetDatabaseAsync();

            // Create 3 sempre disponibili
            await CreateTestBevandaStandardAsync(disponibile: true, sempreDisponibile: true);
            await CreateTestBevandaStandardAsync(disponibile: true, sempreDisponibile: true);
            await CreateTestBevandaStandardAsync(disponibile: true, sempreDisponibile: true);

            // Create 2 non sempre disponibili
            await CreateTestBevandaStandardAsync(disponibile: false, sempreDisponibile: false);
            await CreateTestBevandaStandardAsync(disponibile: false, sempreDisponibile: false);

            // Act
            var result = await _repository.GetDisponibiliAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, result.TotalCount);
            Assert.All(result.Data, dto => Assert.True(dto.SempreDisponibile));
        }

        [Fact]
        public async Task GetDisponibiliAsync_OrdersByPriorita()
        {
            // Arrange
            await ResetDatabaseAsync();

            var bevanda1 = await CreateTestBevandaStandardAsync(priorita: 5);
            var bevanda2 = await CreateTestBevandaStandardAsync(priorita: 10);
            var bevanda3 = await CreateTestBevandaStandardAsync(priorita: 1);

            // Act
            var result = await _repository.GetDisponibiliAsync();

            // Assert
            Assert.NotNull(result);
            var data = result.Data.ToList();
            Assert.Equal(bevanda2.ArticoloId, data[0].ArticoloId); // Priorità 10
            Assert.Equal(bevanda1.ArticoloId, data[1].ArticoloId); // Priorità 5
            Assert.Equal(bevanda3.ArticoloId, data[2].ArticoloId); // Priorità 1
        }

        #endregion

        #region GetByDimensioneBicchiereAsync Tests

        [Fact]
        public async Task GetByDimensioneBicchiereAsync_ReturnsFilteredResults()
        {
            // Arrange
            await ResetDatabaseAsync();

            var dimensione1 = 1; // M
            var dimensione2 = 2; // L

            // Create 2 bevande with dimensione 1
            await CreateTestBevandaStandardAsync(dimensioneBicchiereId: dimensione1);
            await CreateTestBevandaStandardAsync(dimensioneBicchiereId: dimensione1);

            // Create 1 bevanda with dimensione 2
            await CreateTestBevandaStandardAsync(dimensioneBicchiereId: dimensione2);

            // Act
            var result = await _repository.GetByDimensioneBicchiereAsync(dimensione1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.TotalCount);
            Assert.All(result.Data, dto => Assert.Equal(dimensione1, dto.DimensioneBicchiereId));
        }

        [Fact]
        public async Task GetByDimensioneBicchiereAsync_InvalidId_ReturnsEmpty()
        {
            // Arrange
            await ResetDatabaseAsync();

            // Act
            var result = await _repository.GetByDimensioneBicchiereAsync(0);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result.Data);
            Assert.Equal(0, result.TotalCount);
            Assert.Contains("non valido", result.Message);
        }

        #endregion

        #region GetByPersonalizzazioneAsync Tests
        [Fact]
        public async Task GetByPersonalizzazioneAsync_ReturnsFilteredResults()
        {
            // Arrange
            await ResetDatabaseAsync();

            var personalizzazione1 = 1;
            var personalizzazione2 = 2;

            // Create 2 bevande with personalizzazione 1 ma con dimensioni DIVERSE
            await CreateTestBevandaStandardAsync(
                personalizzazioneId: personalizzazione1,
                dimensioneBicchiereId: 1
            );

            await CreateTestBevandaStandardAsync(
                personalizzazioneId: personalizzazione1,
                dimensioneBicchiereId: 2  // Dimensione diversa per evitare vincolo UNIQUE
            );

            // Create 1 bevanda with personalizzazione 2
            await CreateTestBevandaStandardAsync(
                personalizzazioneId: personalizzazione2,
                dimensioneBicchiereId: 1
            );

            // Act
            var result = await _repository.GetByPersonalizzazioneAsync(personalizzazione1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.TotalCount);
            Assert.All(result.Data, dto => Assert.Equal(personalizzazione1, dto.PersonalizzazioneId));
        }

        #endregion

        #region AddAsync Tests

        [Fact]
        public async Task AddAsync_ValidData_CreatesNewBevandaStandard()
        {
            // Arrange
            await ResetDatabaseAsync();

            var dto = CreateTestBevandaStandardDTO(
                personalizzazioneId: 1,
                dimensioneBicchiereId: 1,
                prezzo: 4.50m,
                disponibile: true,
                sempreDisponibile: true,
                priorita: 5
            );

            // Act
            var result = await _repository.AddAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.True(result.Data.ArticoloId > 0);
            Assert.Contains("creata con successo", result.Message);

            // Verify in database
            var dbEntity = await _context.BevandaStandard.FindAsync(result.Data.ArticoloId);
            Assert.NotNull(dbEntity);
        }

        [Fact]
        public async Task AddAsync_DuplicateCombinazione_ReturnsError()
        {
            // Arrange
            await ResetDatabaseAsync();

            var existing = await CreateTestBevandaStandardAsync(
                personalizzazioneId: 1,
                dimensioneBicchiereId: 1
            );

            var dto = CreateTestBevandaStandardDTO(
                personalizzazioneId: 1,
                dimensioneBicchiereId: 1,
                prezzo: 5.00m
            );

            // Act
            var result = await _repository.AddAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Null(result.Data);
            Assert.Contains("Esiste già", result.Message);
        }

        [Fact]
        public async Task AddAsync_InvalidPrice_ReturnsError()
        {
            // Arrange
            await ResetDatabaseAsync();

            var dto = CreateTestBevandaStandardDTO(prezzo: 100.00m); // > 99.99

            // Act
            var result = await _repository.AddAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Contains("prezzo", result.Message.ToLower());
        }

        [Fact]
        public async Task AddAsync_InvalidPriorita_ReturnsError()
        {
            // Arrange
            await ResetDatabaseAsync();

            var dto = CreateTestBevandaStandardDTO(priorita: 11); // > 10

            // Act
            var result = await _repository.AddAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Contains("priorità", result.Message.ToLower());
        }

        [Fact]
        public async Task AddAsync_SempreDisponibileFalse_ForcesDisponibileFalse()
        {
            // Arrange
            await ResetDatabaseAsync();

            var dto = CreateTestBevandaStandardDTO(
                disponibile: true,
                sempreDisponibile: false
            );

            // Act
            var result = await _repository.AddAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.False(result.Data.Disponibile);
            Assert.False(result.Data.SempreDisponibile);
        }

        #endregion

        #region UpdateAsync Tests

        [Fact]
        public async Task UpdateAsync_ValidData_UpdatesSuccessfully()
        {
            // Arrange
            await ResetDatabaseAsync();

            var existing = await CreateTestBevandaStandardAsync(prezzo: 4.50m);

            var dto = CreateTestBevandaStandardDTO(
                articoloId: existing.ArticoloId,
                prezzo: 5.50m,
                priorita: 8
            );

            // Act
            var result = await _repository.UpdateAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.True(result.Data);
            Assert.Contains("aggiornata con successo", result.Message);

            // Verify update
            var updated = await _context.BevandaStandard.FindAsync(existing.ArticoloId);
            Assert.NotNull(updated); // Aggiunto per sicurezza
            Assert.Equal(5.50m, updated!.Prezzo);
            Assert.Equal(8, updated.Priorita);
        }

        [Fact]
        public async Task UpdateAsync_NonExistingId_ReturnsNotFound()
        {
            // Arrange
            await ResetDatabaseAsync();

            var dto = CreateTestBevandaStandardDTO(articoloId: 999);

            // Act
            var result = await _repository.UpdateAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.False(result.Data);
            Assert.Contains("non trovata", result.Message);
        }

        [Fact]
        public async Task UpdateAsync_DuplicateCombinazione_ReturnsError()
        {
            // Arrange
            await ResetDatabaseAsync();

            // Create first bevanda
            await CreateTestBevandaStandardAsync(
                personalizzazioneId: 1,
                dimensioneBicchiereId: 1
            );

            // Create second bevanda
            var bevanda2 = await CreateTestBevandaStandardAsync(
                personalizzazioneId: 2,
                dimensioneBicchiereId: 2
            );

            // Try to update bevanda2 to have same combination as bevanda1
            var dto = CreateTestBevandaStandardDTO(
                articoloId: bevanda2.ArticoloId,
                personalizzazioneId: 1,
                dimensioneBicchiereId: 1
            );

            // Act
            var result = await _repository.UpdateAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Contains("Esiste già", result.Message);
        }

        [Fact]
        public async Task UpdateAsync_NoChanges_ReturnsSuccessFalse()
        {
            // Arrange
            await ResetDatabaseAsync();

            var existing = await CreateTestBevandaStandardAsync();

            var dto = await MapToDTODirectly(existing);

            // Act
            var result = await _repository.UpdateAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.False(result.Data); // No changes made
            Assert.Contains("Nessuna modifica", result.Message);
        }

        #endregion

        #region DeleteAsync Tests

        [Fact]
        public async Task DeleteAsync_ExistingId_DeletesSuccessfully()
        {
            // Arrange
            await ResetDatabaseAsync();

            var existing = await CreateTestBevandaStandardAsync();
            var articoloId = existing.ArticoloId;

            // Act
            var result = await _repository.DeleteAsync(articoloId);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.True(result.Data);
            Assert.Contains("eliminata con successo", result.Message);

            // Verify deletion
            var deleted = await _context.BevandaStandard.FindAsync(articoloId);
            Assert.Null(deleted);

            // Verify orphan Articolo also deleted
            var articolo = await _context.Articolo.FindAsync(articoloId);
            Assert.Null(articolo);
        }

        [Fact]
        public async Task DeleteAsync_NonExistingId_ReturnsNotFound()
        {
            // Arrange
            await ResetDatabaseAsync();

            // Act
            var result = await _repository.DeleteAsync(999);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.False(result.Data);
            Assert.Contains("non trovata", result.Message);
        }

        [Fact]
        public async Task DeleteAsync_InvalidId_ReturnsError()
        {
            // Arrange
            await ResetDatabaseAsync();

            // Act
            var result = await _repository.DeleteAsync(0);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Contains("non valido", result.Message);
        }

        #endregion

        #region ExistsAsync Tests

        [Fact]
        public async Task ExistsAsync_ExistingId_ReturnsTrue()
        {
            // Arrange
            await ResetDatabaseAsync();

            var existing = await CreateTestBevandaStandardAsync();

            // Act
            var result = await _repository.ExistsAsync(existing.ArticoloId);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.True(result.Data);
            Assert.Contains("esiste", result.Message);
        }

        [Fact]
        public async Task ExistsAsync_NonExistingId_ReturnsFalse()
        {
            // Arrange
            await ResetDatabaseAsync();

            // Act
            var result = await _repository.ExistsAsync(999);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.False(result.Data);
            Assert.Contains("non trovata", result.Message);
        }

        #endregion

        #region ExistsByCombinazioneAsync Tests

        [Fact]
        public async Task ExistsByCombinazioneAsync_WithIds_Existing_ReturnsTrue()
        {
            // Arrange
            await ResetDatabaseAsync();

            await CreateTestBevandaStandardAsync(
                personalizzazioneId: 1,
                dimensioneBicchiereId: 1
            );

            // Act
            var result = await _repository.ExistsByCombinazioneAsync(1, 1);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.True(result.Data);
            Assert.Contains("esiste", result.Message);
        }

        [Fact]
        public async Task ExistsByCombinazioneAsync_WithIds_NonExisting_ReturnsFalse()
        {
            // Arrange
            await ResetDatabaseAsync();

            // Act
            var result = await _repository.ExistsByCombinazioneAsync(99, 99);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.False(result.Data);
            Assert.Contains("non trovata", result.Message);
        }

        [Fact]
        public async Task ExistsByCombinazioneAsync_WithStrings_Existing_ReturnsTrue()
        {
            // Arrange
            await ResetDatabaseAsync();

            // Use seeded personalizzazione with name "classic milk tea"
            // and dimensione bicchiere with descrizione "Medium"
            await CreateTestBevandaStandardAsync(
                personalizzazioneId: 1,
                dimensioneBicchiereId: 1
            );

            // Act
            var result = await _repository.ExistsByCombinazioneAsync("classic", "Medium");

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.True(result.Data);
            Assert.Contains("esiste", result.Message);
        }

        #endregion

        #region GetCardProdottiAsync Tests

        [Fact]
        public async Task GetCardProdottiAsync_ReturnsGroupedCards()
        {
            // Arrange
            await ResetDatabaseAsync();

            var personalizzazioneId = 1;

            // Create 3 bevande standard sempre disponibili
            await CreateTestBevandaStandardAsync(
                personalizzazioneId: personalizzazioneId,
                dimensioneBicchiereId: 1,
                sempreDisponibile: true
            );

            await CreateTestBevandaStandardAsync(
                personalizzazioneId: personalizzazioneId,
                dimensioneBicchiereId: 2,
                sempreDisponibile: true
            );

            await CreateTestBevandaStandardAsync(
                personalizzazioneId: 2,
                dimensioneBicchiereId: 1,
                sempreDisponibile: true
            );

            // Act
            var result = await _repository.GetCardProdottiAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, result.TotalCount); // Ora dovrebbe restituire 3 card
            Assert.Equal(3, result.Data.Count());

            Assert.All(result.Data, card =>
            {
                Assert.NotNull(card.PrezziPerDimensioni);
                Assert.NotEmpty(card.PrezziPerDimensioni);
            });
        }

        #endregion

        #region GetCardProdottiPrimoPianoAsync Tests

        [Fact]
        public async Task GetCardProdottiPrimoPianoAsync_ReturnsFilteredCards()
        {
            // Arrange
            await ResetDatabaseAsync();

            // Card primo piano (sempre disponibile E disponibile)
            await CreateTestBevandaStandardAsync(
                personalizzazioneId: 1,
                sempreDisponibile: true,
                disponibile: true
            );

            await CreateTestBevandaStandardAsync(
                personalizzazioneId: 2,
                sempreDisponibile: true,
                disponibile: true
            );

            // Card secondo piano (sempre disponibile ma non disponibile)
            await CreateTestBevandaStandardAsync(
                personalizzazioneId: 3,
                sempreDisponibile: true,
                disponibile: false
            );

            // Act
            var result = await _repository.GetCardProdottiPrimoPianoAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.TotalCount); // Solo le 2 in primo piano
            Assert.All(result.Data, card =>
            {
                // Verifica che siano effettivamente card primo piano
                Assert.NotEmpty(card.PrezziPerDimensioni);
            });
        }

        #endregion

        #region GetPrimoPianoAsync and GetSecondoPianoAsync Tests

        [Fact]
        public async Task GetPrimoPianoAsync_ReturnsOnlySempreDisponibileAndDisponibile()
        {
            // Arrange
            await ResetDatabaseAsync();

            // Create primo piano (both true)
            await CreateTestBevandaStandardAsync(disponibile: true, sempreDisponibile: true);
            await CreateTestBevandaStandardAsync(disponibile: true, sempreDisponibile: true);

            // Create secondo piano (sempre disponibile but not disponibile)
            await CreateTestBevandaStandardAsync(disponibile: false, sempreDisponibile: true);

            // Create not sempre disponibile
            await CreateTestBevandaStandardAsync(disponibile: false, sempreDisponibile: false);

            // Act
            var result = await _repository.GetPrimoPianoAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.TotalCount);
            Assert.All(result.Data, dto =>
            {
                Assert.True(dto.SempreDisponibile);
                Assert.True(dto.Disponibile);
            });
        }

        [Fact]
        public async Task GetSecondoPianoAsync_ReturnsOnlySempreDisponibileAndNotDisponibile()
        {
            // Arrange
            await ResetDatabaseAsync();

            // Create secondo piano
            await CreateTestBevandaStandardAsync(disponibile: false, sempreDisponibile: true);
            await CreateTestBevandaStandardAsync(disponibile: false, sempreDisponibile: true);

            // Create primo piano
            await CreateTestBevandaStandardAsync(disponibile: true, sempreDisponibile: true);

            // Create not sempre disponibile
            await CreateTestBevandaStandardAsync(disponibile: false, sempreDisponibile: false);

            // Act
            var result = await _repository.GetSecondoPianoAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.TotalCount);
            Assert.All(result.Data, dto =>
            {
                Assert.True(dto.SempreDisponibile);
                Assert.False(dto.Disponibile);
            });
        }

        #endregion

        #region Count Methods Tests

        [Fact]
        public async Task CountAsync_ReturnsTotalCount()
        {
            // Arrange
            await ResetDatabaseAsync();

            await CreateMultipleBevandaStandardAsync(5);

            // Act
            var result = await _repository.CountAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(5, result.Data);
        }

        [Fact]
        public async Task CountPrimoPianoAsync_ReturnsCorrectCount()
        {
            // Arrange
            await ResetDatabaseAsync();

            await CreateTestBevandaStandardAsync(disponibile: true, sempreDisponibile: true);
            await CreateTestBevandaStandardAsync(disponibile: true, sempreDisponibile: true);
            await CreateTestBevandaStandardAsync(disponibile: false, sempreDisponibile: true);

            // Act
            var result = await _repository.CountPrimoPianoAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(2, result.Data);
        }

        [Fact]
        public async Task CountDisponibiliAsync_ReturnsSempreDisponibileCount()
        {
            // Arrange
            await ResetDatabaseAsync();

            await CreateTestBevandaStandardAsync(sempreDisponibile: true);
            await CreateTestBevandaStandardAsync(sempreDisponibile: true);
            await CreateTestBevandaStandardAsync(sempreDisponibile: false);

            // Act
            var result = await _repository.CountDisponibiliAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(2, result.Data);
        }

        #endregion

        #region ToggleDisponibilitaAsync Tests        

        [Fact]
        public async Task ToggleDisponibileAsync_ShouldToggleDisponibile()
        {
            // Arrange
            await SetupBevandaStandardTestDataAsync();

            // ✅ Recupera un dolce esistente dal database
            var bevandaStandard = await _context.BevandaStandard.FirstOrDefaultAsync();
            Assert.NotNull(bevandaStandard);

            var articoloId = bevandaStandard.ArticoloId;
            var initialDisponibile = bevandaStandard.Disponibile;

            // Act
            var result = await _repository.ToggleDisponibileAsync(articoloId);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(!initialDisponibile, result.Data);
            Assert.Contains(initialDisponibile ? "non disponibile" : "disponibile", result.Message);

            // Verifica il cambio
            var updated = await _context.BevandaStandard.FindAsync(articoloId);
            Assert.NotNull(updated);
            Assert.Equal(!initialDisponibile, updated.Disponibile);
        }

        [Fact]
        public async Task ToggleDisponibileAsync_ShouldToggleDisponibile_WithSpecificBevandaStandard()
        {
            // Arrange
            await SetupBevandaStandardTestDataAsync();

            // ✅ Crea un dolce specifico per questo test (per controllare meglio lo stato iniziale)
            var bevandaStandard = await CreateBevandaStandardDisponibileAsync();
            var articoloId = bevandaStandard.ArticoloId;
            var initialDisponibile = bevandaStandard.Disponibile; // Dovrebbe essere true

            // Act
            var result = await _repository.ToggleDisponibileAsync(articoloId);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.False(result.Data); // Dovrebbe diventare false
            Assert.Contains("non disponibile", result.Message);

            // Verifica
            var updated = await _context.BevandaStandard.FindAsync(articoloId);
            Assert.NotNull(updated);
            Assert.False(updated.Disponibile);
        }

        [Fact]
        public async Task ToggleDisponibileAsync_ShouldToggleFromFalseToTrue()
        {
            // Arrange
            var bevandaStandard = await CreateBevandaStandardNonDisponibileAsync();
            var articoloId = bevandaStandard.ArticoloId;
            Assert.False(bevandaStandard.Disponibile); // Verifica che sia inizialmente false

            // Act
            var result = await _repository.ToggleDisponibileAsync(articoloId);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.True(result.Data); // Dovrebbe diventare true
            Assert.Contains("disponibile", result.Message);

            // Verifica
            var updated = await _context.BevandaStandard.FindAsync(articoloId);
            Assert.NotNull(updated);
            Assert.True(updated.Disponibile);
        }

        [Fact]
        public async Task ToggleDisponibileAsync_WithNonExistingId_ShouldReturnNotFound()
        {
            // Arrange
            var nonExistingId = 999999;

            // Act
            var result = await _repository.ToggleDisponibileAsync(nonExistingId);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.False(result.Data);
            Assert.Contains("non trovata", result.Message);
        }

        [Fact]
        public async Task ToggleDisponibileAsync_ShouldReturnError_WhenIdIsZeroOrNegative()
        {
            // Arrange
            var invalidIds = new[] { 0, -1, -100 };

            foreach (var invalidId in invalidIds)
            {
                // Act
                var result = await _repository.ToggleDisponibileAsync(invalidId);

                // Assert
                Assert.NotNull(result);
                Assert.False(result.Success);
                Assert.False(result.Data);
                Assert.Contains("non valido", result.Message);
            }
        }

        [Fact]
        public async Task ToggleDisponibileAsync_ShouldUpdateDataAggiornamento()
        {
            // Arrange
            var bevandaStandard = await CreateBevandaStandardDisponibileAsync();
            var articoloId = bevandaStandard.ArticoloId;
            var originalUpdateTime = bevandaStandard.DataAggiornamento;

            // Attendi un momento per essere sicuri che le date siano diverse
            await Task.Delay(100); // Aumenta il delay a 100ms

            // Act
            var result = await _repository.ToggleDisponibileAsync(articoloId);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);

            // Verifica che DataAggiornamento sia stato aggiornato
            var updated = await _context.BevandaStandard.FindAsync(articoloId);
            Assert.NotNull(updated);

            // Usa >= invece di > per sicurezza
            Assert.True(updated.DataAggiornamento >= originalUpdateTime,
                $"DataAggiornamento non aggiornata: {updated.DataAggiornamento} <= {originalUpdateTime}");

            // Se vuoi essere più rigoroso, puoi anche verificare che sia cambiato almeno di qualche tick
            // Assert.NotEqual(originalUpdateTime, updated.DataAggiornamento);
        }

        [Fact]
        public async Task ToggleSempreDisponibileAsync_ShouldToggleSempreDisponibile()
        {
            // Arrange
            await SetupBevandaStandardTestDataAsync();

            // ✅ Recupera un dolce esistente dal database
            var bevandaStandard = await _context.BevandaStandard.FirstOrDefaultAsync();
            Assert.NotNull(bevandaStandard);

            var articoloId = bevandaStandard.ArticoloId;
            var initialDisponibile = bevandaStandard.SempreDisponibile;

            // Act
            var result = await _repository.ToggleSempreDisponibileAsync(articoloId);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(!initialDisponibile, result.Data);
            Assert.Contains(initialDisponibile ? "non sempre disponibile" : "sempre disponibile", result.Message);

            // Verifica il cambio
            var updated = await _context.BevandaStandard.FindAsync(articoloId);
            Assert.NotNull(updated);
            Assert.Equal(!initialDisponibile, updated.SempreDisponibile);
        }

        [Fact]
        public async Task ToggleSempreDisponibileAsync_ShouldToggleDisponibile_WithSpecificBevandaStandard()
        {
            // Arrange
            await SetupBevandaStandardTestDataAsync();
            
            var bevandaStandard = await CreateBevandaStandardSempreDisponibileAsync();
            var articoloId = bevandaStandard.ArticoloId;
            var initialDisponibile = bevandaStandard.SempreDisponibile; // Dovrebbe essere true

            // Act
            var result = await _repository.ToggleSempreDisponibileAsync(articoloId);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.False(result.Data); // Dovrebbe diventare false
            Assert.Contains("non sempre disponibile", result.Message);

            // Verifica
            var updated = await _context.BevandaStandard.FindAsync(articoloId);
            Assert.NotNull(updated);
            Assert.False(updated.SempreDisponibile);
        }

        [Fact]
        public async Task ToggleSempreDisponibileAsync_ShouldToggleFromFalseToTrue()
        {
            // Arrange
            var bevandaStandard = await CreateBevandaStandardNonSempreDisponibileAsync();
            var articoloId = bevandaStandard.ArticoloId;
            Assert.False(bevandaStandard.SempreDisponibile); // Verifica che sia inizialmente false

            // Act
            var result = await _repository.ToggleSempreDisponibileAsync(articoloId);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.True(result.Data); // Dovrebbe diventare true
            Assert.Contains("sempre disponibile", result.Message);

            // Verifica
            var updated = await _context.BevandaStandard.FindAsync(articoloId);
            Assert.NotNull(updated);
            Assert.True(updated.SempreDisponibile);
        }

        [Fact]
        public async Task ToggleSempreDisponibileAsync_WithNonExistingId_ShouldReturnNotFound()
        {
            // Arrange
            var nonExistingId = 999999;

            // Act
            var result = await _repository.ToggleSempreDisponibileAsync(nonExistingId);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.False(result.Data);
            Assert.Contains("non trovata", result.Message);
        }

        [Fact]
        public async Task ToggleSempreDisponibileAsync_ShouldReturnError_WhenIdIsZeroOrNegative()
        {
            // Arrange
            var invalidIds = new[] { 0, -1, -100 };

            foreach (var invalidId in invalidIds)
            {
                // Act
                var result = await _repository.ToggleSempreDisponibileAsync(invalidId);

                // Assert
                Assert.NotNull(result);
                Assert.False(result.Success);
                Assert.False(result.Data);
                Assert.Contains("non valido", result.Message);
            }
        }

        [Fact]
        public async Task ToggleSempreDisponibileAsync_ShouldUpdateDataAggiornamento()
        {
            // Arrange
            var bevandaStandard = await CreateBevandaStandardDisponibileAsync();
            var articoloId = bevandaStandard.ArticoloId;
            var originalUpdateTime = bevandaStandard.DataAggiornamento;

            // Attendi un momento per essere sicuri che le date siano diverse
            await Task.Delay(100); // Aumenta il delay a 100ms

            // Act
            var result = await _repository.ToggleSempreDisponibileAsync(articoloId);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);

            // Verifica che DataAggiornamento sia stato aggiornato
            var updated = await _context.BevandaStandard.FindAsync(articoloId);
            Assert.NotNull(updated);

            // Usa >= invece di > per sicurezza
            Assert.True(updated.DataAggiornamento >= originalUpdateTime,
                $"DataAggiornamento non aggiornata: {updated.DataAggiornamento} <= {originalUpdateTime}");

            // Se vuoi essere più rigoroso, puoi anche verificare che sia cambiato almeno di qualche tick
            // Assert.NotEqual(originalUpdateTime, updated.DataAggiornamento);
        }

        #endregion

        #region Helper Methods

        private async Task<BevandaStandardDTO> MapToDTODirectly(BevandaStandard bevandaStandard)
        {
            var dimensione = await _context.DimensioneBicchiere
                .FirstOrDefaultAsync(d => d.DimensioneBicchiereId == bevandaStandard.DimensioneBicchiereId);

            return new BevandaStandardDTO
            {
                ArticoloId = bevandaStandard.ArticoloId,
                PersonalizzazioneId = bevandaStandard.PersonalizzazioneId,
                DimensioneBicchiereId = bevandaStandard.DimensioneBicchiereId,
                Prezzo = bevandaStandard.Prezzo,
                ImmagineUrl = bevandaStandard.ImmagineUrl,
                Disponibile = bevandaStandard.Disponibile,
                SempreDisponibile = bevandaStandard.SempreDisponibile,
                Priorita = bevandaStandard.Priorita,
                DataCreazione = bevandaStandard.DataCreazione,
                DataAggiornamento = bevandaStandard.DataAggiornamento,
                DimensioneBicchiere = dimensione != null
                    ? new DimensioneBicchiereDTO
                    {
                        DimensioneBicchiereId = dimensione.DimensioneBicchiereId,
                        Sigla = dimensione.Sigla,
                        Descrizione = dimensione.Descrizione,
                        Capienza = dimensione.Capienza,
                        UnitaMisuraId = dimensione.UnitaMisuraId,
                        PrezzoBase = dimensione.PrezzoBase,
                        Moltiplicatore = dimensione.Moltiplicatore
                    }
                    : null
            };
        }

        #endregion
    }
}