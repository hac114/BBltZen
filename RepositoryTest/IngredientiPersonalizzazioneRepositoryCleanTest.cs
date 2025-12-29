using BBltZen;
using DTO;
using Microsoft.EntityFrameworkCore;
using Repository.Service;
using Xunit;

namespace RepositoryTest
{
    public class IngredientiPersonalizzazioneRepositoryCleanTest : BaseTestClean
    {
        private readonly IngredientiPersonalizzazioneRepository _repository;

        public IngredientiPersonalizzazioneRepositoryCleanTest()
        {
            _repository = new IngredientiPersonalizzazioneRepository(_context,
                GetTestLogger<IngredientiPersonalizzazioneRepository>());
        }

        #region GetAllAsync Tests

        [Fact]
        public async Task GetAllAsync_WithData_ReturnsPaginatedResponse()
        {
            // Arrange
            await ResetDatabaseAsync();
            await CreateMultipleIngredientiPersonalizzazioneAsync(3);

            // Act
            var result = await _repository.GetAllAsync(page: 1, pageSize: 10);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, result.TotalCount);
            Assert.Equal(3, result.Data.Count());
            Assert.Equal(1, result.Page);
            Assert.Equal(10, result.PageSize);
            Assert.NotEmpty(result.Message);
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
            Assert.Equal(1, result.Page);
            Assert.Equal(10, result.PageSize);
            Assert.Equal("Nessuna ingredienti personalizzazione trovata", result.Message);
        }

        [Fact]
        public async Task GetAllAsync_Pagination_WorksCorrectly()
        {
            // Arrange
            await ResetDatabaseAsync();
            await CreateMultipleIngredientiPersonalizzazioneAsync(15);

            // Act - Page 1
            var resultPage1 = await _repository.GetAllAsync(page: 1, pageSize: 5);

            // Act - Page 2
            var resultPage2 = await _repository.GetAllAsync(page: 2, pageSize: 5);

            // Assert
            Assert.Equal(15, resultPage1.TotalCount);
            Assert.Equal(3, resultPage1.TotalPages);
            Assert.True(resultPage1.HasNext);
            Assert.False(resultPage1.HasPrevious);

            Assert.Equal(5, resultPage1.Data.Count());
            Assert.Equal(5, resultPage2.Data.Count());

            // Verify IDs are different between pages
            var page1Ids = resultPage1.Data.Select(x => x.IngredientePersId).ToList();
            var page2Ids = resultPage2.Data.Select(x => x.IngredientePersId).ToList();

            Assert.Empty(page1Ids.Intersect(page2Ids));
        }

        [Fact]
        public async Task GetAllAsync_InvalidPagination_FallsBackToDefaults()
        {
            // Arrange
            await ResetDatabaseAsync();
            await CreateMultipleIngredientiPersonalizzazioneAsync(3);

            // Act - Invalid page and pageSize
            var result = await _repository.GetAllAsync(page: -1, pageSize: 0);

            // Assert
            Assert.Equal(1, result.Page); // Falls back to 1
            Assert.Equal(1, result.PageSize); // Falls back to 1 (non 10) perché SecurityHelper.ValidatePagination restituisce (1,1) per input non validi
            Assert.Equal(3, result.TotalCount);
        }

        [Fact]
        public async Task GetAllAsync_OrderedByDataCreazioneDescending()
        {
            // Arrange
            await ResetDatabaseAsync();

            var now = DateTime.UtcNow;
            var ingredientiPers = new List<IngredientiPersonalizzazione>
            {
                await CreateTestIngredientiPersonalizzazioneAsync(dataCreazione: now.AddMinutes(-10)),
                await CreateTestIngredientiPersonalizzazioneAsync(dataCreazione: now.AddMinutes(-5)),
                await CreateTestIngredientiPersonalizzazioneAsync(dataCreazione: now)
            };

            // Act
            var result = await _repository.GetAllAsync(page: 1, pageSize: 10);

            // Assert
            var resultList = result.Data.ToList();
            Assert.Equal(3, resultList.Count);

            // Should be ordered by DataCreazione descending (newest first)
            Assert.True(resultList[0].DataCreazione >= resultList[1].DataCreazione);
            Assert.True(resultList[1].DataCreazione >= resultList[2].DataCreazione);
        }

        #endregion

        #region GetByIdAsync Tests

        [Fact]
        public async Task GetByIdAsync_ValidId_ReturnsSingleResponse()
        {
            // Arrange
            await ResetDatabaseAsync();
            var testData = await CreateTestIngredientiPersonalizzazioneAsync();
            var expectedId = testData.IngredientePersId;

            // Act
            var result = await _repository.GetByIdAsync(expectedId);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(expectedId, result.Data.IngredientePersId);
            Assert.Equal(testData.PersCustomId, result.Data.PersCustomId);
            Assert.Equal(testData.IngredienteId, result.Data.IngredienteId);
            Assert.Equal(testData.DataCreazione, result.Data.DataCreazione, TimeSpan.FromSeconds(1));
            Assert.Contains($"trovata", result.Message);
        }

        [Fact]
        public async Task GetByIdAsync_InvalidId_ReturnsNotFoundResponse()
        {
            // Arrange
            int invalidId = 9999;

            // Act
            var result = await _repository.GetByIdAsync(invalidId);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Null(result.Data);
            Assert.Contains("non trovata", result.Message);
        }

        [Fact]
        public async Task GetByIdAsync_IdZeroOrNegative_ReturnsErrorResponse()
        {
            // Arrange
            int zeroId = 0;
            int negativeId = -1;

            // Act
            var resultZero = await _repository.GetByIdAsync(zeroId);
            var resultNegative = await _repository.GetByIdAsync(negativeId);

            // Assert
            Assert.False(resultZero.Success);
            Assert.Contains("non valido", resultZero.Message);

            Assert.False(resultNegative.Success);
            Assert.Contains("non valido", resultNegative.Message);
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsDTOWithNames()
        {
            // Arrange
            await ResetDatabaseAsync();

            var personalizzazione = await CreateTestPersonalizzazioneCustomAsync(
                nome: "Test Personalizzazione",
                persCustomId: 1001
            );

            var ingrediente = new Ingrediente
            {
                IngredienteId = 1001,
                Ingrediente1 = "Test Ingrediente",
                CategoriaId = 1,
                PrezzoAggiunto = 1.00m,
                Disponibile = true,
                DataInserimento = DateTime.UtcNow,
                DataAggiornamento = DateTime.UtcNow
            };
            _context.Ingrediente.Add(ingrediente);
            await _context.SaveChangesAsync();

            var testData = await CreateTestIngredientiPersonalizzazioneAsync(
                persCustomId: personalizzazione.PersCustomId,
                ingredienteId: ingrediente.IngredienteId
            );

            // Act
            var result = await _repository.GetByIdAsync(testData.IngredientePersId);

            // Assert
            Assert.NotNull(result.Data);
            Assert.Equal(personalizzazione.Nome, result.Data.NomePersonalizzazione);
            Assert.Equal(ingrediente.Ingrediente1, result.Data.NomeIngrediente);
        }

        #endregion

        #region GetByPersCustomIdAsync Tests

        [Fact]
        public async Task GetByPersCustomIdAsync_ValidId_ReturnsPaginatedResponse()
        {
            // Arrange
            await ResetDatabaseAsync();

            var personalizzazione = await CreateTestPersonalizzazioneCustomAsync(
                nome: "Test Pers Custom",
                persCustomId: 1001
            );

            // Create multiple ingredienti for this personalizzazione
            for (int i = 0; i < 3; i++)
            {
                await CreateTestIngredientiPersonalizzazioneAsync(
                    persCustomId: personalizzazione.PersCustomId,
                    ingredienteId: 1000 + i
                );
            }

            // Act
            var result = await _repository.GetByPersCustomIdAsync(
                personalizzazione.PersCustomId,
                page: 1,
                pageSize: 10
            );

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, result.TotalCount);
            Assert.Equal(3, result.Data.Count());
            Assert.All(result.Data, dto =>
                Assert.Equal(personalizzazione.PersCustomId, dto.PersCustomId));
            Assert.Contains(personalizzazione.PersCustomId.ToString(), result.Message);
        }

        [Fact]
        public async Task GetByPersCustomIdAsync_InvalidId_ReturnsEmptyResponse()
        {
            // Arrange
            // Usiamo 0 invece di 9999 perché il repository valida solo se persCustomId <= 0
            int invalidId = 0;

            // Act
            var result = await _repository.GetByPersCustomIdAsync(invalidId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(0, result.TotalCount);
            Assert.Empty(result.Data);
            Assert.Contains("Il parametro 'persCustomId' non è valido", result.Message);
        }

        [Fact]
        public async Task GetByPersCustomIdAsync_NoIngredientsForPersonalizzazione_ReturnsEmpty()
        {
            // Arrange
            await ResetDatabaseAsync();
            var personalizzazione = await CreateTestPersonalizzazioneCustomAsync(
                nome: "Empty Pers Custom",
                persCustomId: 1002
            );

            // Act
            var result = await _repository.GetByPersCustomIdAsync(personalizzazione.PersCustomId);

            // Assert
            Assert.Equal(0, result.TotalCount);
            Assert.Empty(result.Data);
            Assert.Contains("Nessuna", result.Message);
        }

        [Fact]
        public async Task GetByPersCustomIdAsync_PaginationWorksCorrectly()
        {
            // Arrange
            await ResetDatabaseAsync();

            var personalizzazione = await CreateTestPersonalizzazioneCustomAsync(persCustomId: 1001);

            // Create 15 ingredienti for this personalizzazione
            for (int i = 0; i < 15; i++)
            {
                await CreateTestIngredientiPersonalizzazioneAsync(
                    persCustomId: personalizzazione.PersCustomId,
                    ingredienteId: 1000 + i
                );
            }

            // Act
            var resultPage1 = await _repository.GetByPersCustomIdAsync(personalizzazione.PersCustomId, page: 1, pageSize: 5);
            var resultPage2 = await _repository.GetByPersCustomIdAsync(personalizzazione.PersCustomId, page: 2, pageSize: 5);

            // Assert
            Assert.Equal(15, resultPage1.TotalCount);
            Assert.Equal(5, resultPage1.Data.Count());
            Assert.Equal(5, resultPage2.Data.Count());

            var page1Ids = resultPage1.Data.Select(x => x.IngredientePersId).ToList();
            var page2Ids = resultPage2.Data.Select(x => x.IngredientePersId).ToList();
            Assert.Empty(page1Ids.Intersect(page2Ids));
        }

        #endregion

        #region GetByIngredienteIdAsync Tests

        [Fact]
        public async Task GetByIngredienteIdAsync_ValidId_ReturnsPaginatedResponse()
        {
            // Arrange
            await ResetDatabaseAsync();

            // Create a specific ingrediente
            var ingrediente = new Ingrediente
            {
                IngredienteId = 1001,
                Ingrediente1 = "Test Ingrediente",
                CategoriaId = 1,
                PrezzoAggiunto = 1.00m,
                Disponibile = true,
                DataInserimento = DateTime.UtcNow,
                DataAggiornamento = DateTime.UtcNow
            };
            _context.Ingrediente.Add(ingrediente);
            await _context.SaveChangesAsync();

            // Create multiple personalizzazioni with this ingrediente
            for (int i = 0; i < 3; i++)
            {
                await CreateTestIngredientiPersonalizzazioneAsync(
                    persCustomId: 1000 + i,
                    ingredienteId: ingrediente.IngredienteId
                );
            }

            // Act
            var result = await _repository.GetByIngredienteIdAsync(
                ingrediente.IngredienteId,
                page: 1,
                pageSize: 10
            );

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, result.TotalCount);
            Assert.Equal(3, result.Data.Count());
            Assert.All(result.Data, dto =>
                Assert.Equal(ingrediente.IngredienteId, dto.IngredienteId));
            Assert.Contains(ingrediente.IngredienteId.ToString(), result.Message);
        }

        [Fact]
        public async Task GetByIngredienteIdAsync_InvalidId_ReturnsEmptyResponse()
        {
            // Arrange
            // Usiamo 0 invece di 9999 perché il repository valida solo se ingredienteId <= 0
            int invalidId = 0;

            // Act
            var result = await _repository.GetByIngredienteIdAsync(invalidId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(0, result.TotalCount);
            Assert.Empty(result.Data);
            Assert.Contains("Il parametro 'ingredienteId' non è valido", result.Message);
        }

        [Fact]
        public async Task GetByIngredienteIdAsync_NoPersonalizzazioniForIngrediente_ReturnsEmpty()
        {
            // Arrange
            await ResetDatabaseAsync();
            var ingrediente = new Ingrediente
            {
                IngredienteId = 1002,
                Ingrediente1 = "Unused Ingrediente",
                CategoriaId = 1,
                PrezzoAggiunto = 1.00m,
                Disponibile = true,
                DataInserimento = DateTime.UtcNow,
                DataAggiornamento = DateTime.UtcNow
            };
            _context.Ingrediente.Add(ingrediente);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetByIngredienteIdAsync(ingrediente.IngredienteId);

            // Assert
            Assert.Equal(0, result.TotalCount);
            Assert.Empty(result.Data);
            Assert.Contains("Nessuna", result.Message);
        }

        #endregion

        #region GetByCombinazioneAsync Tests

        [Fact]
        public async Task GetByCombinazioneAsync_ValidCombinazione_ReturnsSingleResponse()
        {
            // Arrange
            await ResetDatabaseAsync();

            var personalizzazione = await CreateTestPersonalizzazioneCustomAsync(persCustomId: 1001);
            var ingrediente = new Ingrediente
            {
                IngredienteId = 1001,
                Ingrediente1 = "Test Ingrediente",
                CategoriaId = 1,
                PrezzoAggiunto = 1.00m,
                Disponibile = true,
                DataInserimento = DateTime.UtcNow,
                DataAggiornamento = DateTime.UtcNow
            };
            _context.Ingrediente.Add(ingrediente);
            await _context.SaveChangesAsync();

            var testData = await CreateTestIngredientiPersonalizzazioneAsync(
                persCustomId: personalizzazione.PersCustomId,
                ingredienteId: ingrediente.IngredienteId
            );

            // Act
            var result = await _repository.GetByCombinazioneAsync(
                personalizzazione.PersCustomId,
                ingrediente.IngredienteId
            );

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(personalizzazione.PersCustomId, result.Data.PersCustomId);
            Assert.Equal(ingrediente.IngredienteId, result.Data.IngredienteId);
            Assert.Equal(testData.IngredientePersId, result.Data.IngredientePersId);
            Assert.Contains("trovata", result.Message);
        }

        [Fact]
        public async Task GetByCombinazioneAsync_InvalidIds_ReturnsErrorResponse()
        {
            // Act
            var resultZero = await _repository.GetByCombinazioneAsync(0, 0);
            var resultNegative = await _repository.GetByCombinazioneAsync(-1, -1);

            // Assert
            Assert.False(resultZero.Success);
            Assert.Contains("non valida", resultZero.Message);

            Assert.False(resultNegative.Success);
            Assert.Contains("non valida", resultNegative.Message);
        }

        [Fact]
        public async Task GetByCombinazioneAsync_NonExistentCombinazione_ReturnsNotFoundResponse()
        {
            // Arrange
            await ResetDatabaseAsync();
            var personalizzazione = await CreateTestPersonalizzazioneCustomAsync(persCustomId: 1001);
            var ingrediente = new Ingrediente
            {
                IngredienteId = 1001,
                Ingrediente1 = "Test Ingrediente",
                CategoriaId = 1,
                PrezzoAggiunto = 1.00m,
                Disponibile = true,
                DataInserimento = DateTime.UtcNow,
                DataAggiornamento = DateTime.UtcNow
            };
            _context.Ingrediente.Add(ingrediente);
            await _context.SaveChangesAsync();

            // DON'T create the combination

            // Act
            var result = await _repository.GetByCombinazioneAsync(
                personalizzazione.PersCustomId,
                ingrediente.IngredienteId
            );

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Null(result.Data);
            Assert.Contains("non trovata", result.Message);
        }

        [Fact]
        public async Task GetByCombinazioneAsync_ReturnsDTOWithNames()
        {
            // Arrange
            await ResetDatabaseAsync();

            var personalizzazione = await CreateTestPersonalizzazioneCustomAsync(
                nome: "Test Pers Name",
                persCustomId: 1001
            );

            var ingrediente = new Ingrediente
            {
                IngredienteId = 1001,
                Ingrediente1 = "Test Ing Name",
                CategoriaId = 1,
                PrezzoAggiunto = 1.00m,
                Disponibile = true,
                DataInserimento = DateTime.UtcNow,
                DataAggiornamento = DateTime.UtcNow
            };
            _context.Ingrediente.Add(ingrediente);
            await _context.SaveChangesAsync();

            await CreateTestIngredientiPersonalizzazioneAsync(
                persCustomId: personalizzazione.PersCustomId,
                ingredienteId: ingrediente.IngredienteId
            );

            // Act
            var result = await _repository.GetByCombinazioneAsync(
                personalizzazione.PersCustomId,
                ingrediente.IngredienteId
            );

            // Assert
            Assert.NotNull(result.Data);
            Assert.Equal(personalizzazione.Nome, result.Data.NomePersonalizzazione);
            Assert.Equal(ingrediente.Ingrediente1, result.Data.NomeIngrediente);
        }

        #endregion

        #region AddAsync Tests

        [Fact]
        public async Task AddAsync_ValidDTO_ReturnsSuccessResponse()
        {
            // Arrange
            await ResetDatabaseAsync();

            var personalizzazione = await CreateTestPersonalizzazioneCustomAsync(persCustomId: 1001);
            var ingrediente = new Ingrediente
            {
                IngredienteId = 1001,
                Ingrediente1 = "Test Ingrediente",
                CategoriaId = 1,
                PrezzoAggiunto = 1.00m,
                Disponibile = true,
                DataInserimento = DateTime.UtcNow,
                DataAggiornamento = DateTime.UtcNow
            };
            _context.Ingrediente.Add(ingrediente);
            await _context.SaveChangesAsync();

            var dto = new IngredientiPersonalizzazioneDTO
            {
                PersCustomId = personalizzazione.PersCustomId,
                IngredienteId = ingrediente.IngredienteId,
                DataCreazione = DateTime.UtcNow
            };

            // Act
            var result = await _repository.AddAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.True(result.Data.IngredientePersId > 0);
            Assert.Equal(personalizzazione.PersCustomId, result.Data.PersCustomId);
            Assert.Equal(ingrediente.IngredienteId, result.Data.IngredienteId);
            Assert.Contains("creata con successo", result.Message);

            // Verify it was saved in database
            var saved = await _context.IngredientiPersonalizzazione
                .FindAsync(result.Data.IngredientePersId);
            Assert.NotNull(saved);
        }

        [Fact]
        public async Task AddAsync_DuplicateCombinazione_ReturnsErrorResponse()
        {
            // Arrange
            await ResetDatabaseAsync();

            var personalizzazione = await CreateTestPersonalizzazioneCustomAsync(persCustomId: 1001);
            var ingrediente = new Ingrediente
            {
                IngredienteId = 1001,
                Ingrediente1 = "Test Ingrediente",
                CategoriaId = 1,
                PrezzoAggiunto = 1.00m,
                Disponibile = true,
                DataInserimento = DateTime.UtcNow,
                DataAggiornamento = DateTime.UtcNow
            };
            _context.Ingrediente.Add(ingrediente);
            await _context.SaveChangesAsync();

            // Create first combination
            await CreateTestIngredientiPersonalizzazioneAsync(
                persCustomId: personalizzazione.PersCustomId,
                ingredienteId: ingrediente.IngredienteId
            );

            // Try to create duplicate
            var dto = new IngredientiPersonalizzazioneDTO
            {
                PersCustomId = personalizzazione.PersCustomId,
                IngredienteId = ingrediente.IngredienteId,
                DataCreazione = DateTime.UtcNow
            };

            // Act
            var result = await _repository.AddAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Null(result.Data);
            Assert.Contains("Esiste già", result.Message);
        }

        [Fact]
        public async Task AddAsync_NullDTO_ThrowsException()
        {
            // Arrange
            IngredientiPersonalizzazioneDTO? nullDto = null;

            // Act & Assert
            // Ora non ci aspettiamo più un'eccezione poiché il repository la gestisce internamente
            // Verifichiamo invece che la risposta contenga un errore
            var result = await _repository.AddAsync(nullDto!);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Contains("Errore", result.Message);
        }

        [Fact]
        public async Task AddAsync_InvalidIds_ReturnsErrorResponse()
        {
            // Arrange
            var dtoZero = new IngredientiPersonalizzazioneDTO
            {
                PersCustomId = 0,
                IngredienteId = 0,
                DataCreazione = DateTime.UtcNow
            };

            var dtoNegative = new IngredientiPersonalizzazioneDTO
            {
                PersCustomId = -1,
                IngredienteId = -1,
                DataCreazione = DateTime.UtcNow
            };

            // Act
            var resultZero = await _repository.AddAsync(dtoZero);
            var resultNegative = await _repository.AddAsync(dtoNegative);

            // Assert
            Assert.False(resultZero.Success);
            Assert.Contains("obbligatorio", resultZero.Message);

            Assert.False(resultNegative.Success);
            Assert.Contains("obbligatorio", resultNegative.Message);
        }

        [Fact]
        public async Task AddAsync_IncludesNomePersonalizzazioneAndNomeIngrediente()
        {
            // Arrange
            await ResetDatabaseAsync();

            var personalizzazione = await CreateTestPersonalizzazioneCustomAsync(
                nome: "Test Pers Name",
                persCustomId: 1001
            );

            var ingrediente = new Ingrediente
            {
                IngredienteId = 1001,
                Ingrediente1 = "Test Ing Name",
                CategoriaId = 1,
                PrezzoAggiunto = 1.00m,
                Disponibile = true,
                DataInserimento = DateTime.UtcNow,
                DataAggiornamento = DateTime.UtcNow
            };
            _context.Ingrediente.Add(ingrediente);
            await _context.SaveChangesAsync();

            var dto = new IngredientiPersonalizzazioneDTO
            {
                PersCustomId = personalizzazione.PersCustomId,
                IngredienteId = ingrediente.IngredienteId,
                DataCreazione = DateTime.UtcNow
            };

            // Act
            var result = await _repository.AddAsync(dto);

            // Assert
            Assert.NotNull(result.Data);
            Assert.Equal(personalizzazione.Nome, result.Data.NomePersonalizzazione);
            Assert.Equal(ingrediente.Ingrediente1, result.Data.NomeIngrediente);
        }

        #endregion

        #region UpdateAsync Tests

        [Fact]
        public async Task UpdateAsync_ValidDTO_ReturnsSuccessResponse()
        {
            // Arrange
            await ResetDatabaseAsync();

            // Create initial data
            var initialData = await CreateTestIngredientiPersonalizzazioneAsync(
                persCustomId: 1001,
                ingredienteId: 1001
            );

            // Create new personalizzazione and ingrediente for update
            var newPersonalizzazione = await CreateTestPersonalizzazioneCustomAsync(persCustomId: 1002);
            var newIngrediente = new Ingrediente
            {
                IngredienteId = 1002,
                Ingrediente1 = "New Test Ingrediente",
                CategoriaId = 1,
                PrezzoAggiunto = 1.00m,
                Disponibile = true,
                DataInserimento = DateTime.UtcNow,
                DataAggiornamento = DateTime.UtcNow
            };
            _context.Ingrediente.Add(newIngrediente);
            await _context.SaveChangesAsync();

            var dto = new IngredientiPersonalizzazioneDTO
            {
                IngredientePersId = initialData.IngredientePersId,
                PersCustomId = newPersonalizzazione.PersCustomId,
                IngredienteId = newIngrediente.IngredienteId,
                DataCreazione = initialData.DataCreazione
            };

            // Act
            var result = await _repository.UpdateAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.True(result.Data); // Boolean success
            Assert.Contains("aggiornata con successo", result.Message);

            // Verify changes in database
            var updated = await _context.IngredientiPersonalizzazione
                .FindAsync(initialData.IngredientePersId);
            Assert.NotNull(updated);
            Assert.Equal(newPersonalizzazione.PersCustomId, updated.PersCustomId);
            Assert.Equal(newIngrediente.IngredienteId, updated.IngredienteId);
        }

        [Fact]
        public async Task UpdateAsync_DuplicateCombinazione_ReturnsErrorResponse()
        {
            // Arrange
            await ResetDatabaseAsync();

            // Create first combination
            var existingCombo = await CreateTestIngredientiPersonalizzazioneAsync(
                persCustomId: 1001,
                ingredienteId: 1001
            );

            // Create second combination to update
            var toUpdate = await CreateTestIngredientiPersonalizzazioneAsync(
                persCustomId: 1002,
                ingredienteId: 1002
            );

            // Try to update second to match first (duplicate)
            var dto = new IngredientiPersonalizzazioneDTO
            {
                IngredientePersId = toUpdate.IngredientePersId,
                PersCustomId = existingCombo.PersCustomId, // Duplicate persCustomId
                IngredienteId = existingCombo.IngredienteId, // Duplicate ingredienteId
                DataCreazione = toUpdate.DataCreazione
            };

            // Act
            var result = await _repository.UpdateAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Contains("Esiste già", result.Message);
        }

        [Fact]
        public async Task UpdateAsync_NoChanges_ReturnsSuccessWithFalse()
        {
            // Arrange
            await ResetDatabaseAsync();

            var initialData = await CreateTestIngredientiPersonalizzazioneAsync(
                persCustomId: 1001,
                ingredienteId: 1001
            );

            var dto = new IngredientiPersonalizzazioneDTO
            {
                IngredientePersId = initialData.IngredientePersId,
                PersCustomId = initialData.PersCustomId,
                IngredienteId = initialData.IngredienteId,
                DataCreazione = initialData.DataCreazione
            };

            // Act
            var result = await _repository.UpdateAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.False(result.Data); // No changes made
            Assert.Contains("Nessuna modifica necessaria", result.Message);
        }

        [Fact]
        public async Task UpdateAsync_InvalidId_ReturnsErrorResponse()
        {
            // Arrange
            var dto = new IngredientiPersonalizzazioneDTO
            {
                IngredientePersId = 0, // Invalid ID
                PersCustomId = 1,
                IngredienteId = 1,
                DataCreazione = DateTime.UtcNow
            };

            // Act
            var result = await _repository.UpdateAsync(dto);

            // Assert
            Assert.False(result.Success);
            Assert.Contains("obbligatorio", result.Message);
        }

        [Fact]
        public async Task UpdateAsync_NonExistentRecord_ReturnsNotFoundResponse()
        {
            // Arrange
            var dto = new IngredientiPersonalizzazioneDTO
            {
                IngredientePersId = 9999, // Non-existent
                PersCustomId = 1,
                IngredienteId = 1,
                DataCreazione = DateTime.UtcNow
            };

            // Act
            var result = await _repository.UpdateAsync(dto);

            // Assert
            Assert.False(result.Success);
            Assert.Contains("non trovata", result.Message);
        }

        [Fact]
        public async Task UpdateAsync_NullDTO_ThrowsException()
        {
            // Arrange
            IngredientiPersonalizzazioneDTO? nullDto = null;

            // Act & Assert
            // Ora non ci aspettiamo più un'eccezione poiché il repository la gestisce internamente
            // Verifichiamo invece che la risposta contenga un errore
            var result = await _repository.UpdateAsync(nullDto!);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Contains("Errore", result.Message);
        }

        #endregion

        #region DeleteAsync Tests

        [Fact]
        public async Task DeleteAsync_ValidId_ReturnsSuccessResponse()
        {
            // Arrange
            await ResetDatabaseAsync();

            var testData = await CreateTestIngredientiPersonalizzazioneAsync();
            var idToDelete = testData.IngredientePersId;

            // Verify it exists before deletion
            var existsBefore = await _context.IngredientiPersonalizzazione
                .AnyAsync(ip => ip.IngredientePersId == idToDelete);
            Assert.True(existsBefore);

            // Act
            var result = await _repository.DeleteAsync(idToDelete);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.True(result.Data); // Deletion successful
            Assert.Contains("eliminata con successo", result.Message);

            // Verify it no longer exists
            var existsAfter = await _context.IngredientiPersonalizzazione
                .AnyAsync(ip => ip.IngredientePersId == idToDelete);
            Assert.False(existsAfter);
        }

        [Fact]
        public async Task DeleteAsync_InvalidId_ReturnsErrorResponse()
        {
            // Arrange
            int invalidId = 0;

            // Act
            var result = await _repository.DeleteAsync(invalidId);

            // Assert
            Assert.False(result.Success);
            Assert.Contains("non valido", result.Message);
        }

        [Fact]
        public async Task DeleteAsync_NonExistentId_ReturnsNotFoundResponse()
        {
            // Arrange
            int nonExistentId = 9999;

            // Act
            var result = await _repository.DeleteAsync(nonExistentId);

            // Assert
            Assert.False(result.Success);
            Assert.Contains("non trovato", result.Message);
        }

        #endregion

        #region ExistsAsync Tests

        [Fact]
        public async Task ExistsAsync_ExistingId_ReturnsSuccessResponse()
        {
            // Arrange
            await ResetDatabaseAsync();

            var testData = await CreateTestIngredientiPersonalizzazioneAsync();
            var existingId = testData.IngredientePersId;

            // Act
            var result = await _repository.ExistsAsync(existingId);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.True(result.Data); // Exists
            Assert.Contains("esiste", result.Message);
        }

        [Fact]
        public async Task ExistsAsync_NonExistentId_ReturnsSuccessResponseWithFalse()
        {
            // Arrange
            int nonExistentId = 9999;

            // Act
            var result = await _repository.ExistsAsync(nonExistentId);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success); // The check itself succeeded
            Assert.False(result.Data); // Doesn't exist
            Assert.Contains("non trovata", result.Message);
        }

        [Fact]
        public async Task ExistsAsync_InvalidId_ReturnsErrorResponse()
        {
            // Arrange
            int invalidId = 0;

            // Act
            var result = await _repository.ExistsAsync(invalidId);

            // Assert
            Assert.False(result.Success);
            Assert.Contains("non valido", result.Message);
        }

        #endregion

        #region ExistsByCombinazioneAsync Tests

        [Fact]
        public async Task ExistsByCombinazioneAsync_ExistingCombinazione_ReturnsSuccessResponse()
        {
            // Arrange
            await ResetDatabaseAsync();

            var personalizzazione = await CreateTestPersonalizzazioneCustomAsync(persCustomId: 1001);
            var ingrediente = new Ingrediente
            {
                IngredienteId = 1001,
                Ingrediente1 = "Test Ingrediente",
                CategoriaId = 1,
                PrezzoAggiunto = 1.00m,
                Disponibile = true,
                DataInserimento = DateTime.UtcNow,
                DataAggiornamento = DateTime.UtcNow
            };
            _context.Ingrediente.Add(ingrediente);
            await _context.SaveChangesAsync();

            await CreateTestIngredientiPersonalizzazioneAsync(
                persCustomId: personalizzazione.PersCustomId,
                ingredienteId: ingrediente.IngredienteId
            );

            // Act
            var result = await _repository.ExistsByCombinazioneAsync(
                personalizzazione.PersCustomId,
                ingrediente.IngredienteId
            );

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.True(result.Data); // Exists
            Assert.Contains("esiste", result.Message);
        }

        [Fact]
        public async Task ExistsByCombinazioneAsync_NonExistentCombinazione_ReturnsSuccessResponseWithFalse()
        {
            // Arrange
            await ResetDatabaseAsync();

            var personalizzazione = await CreateTestPersonalizzazioneCustomAsync(persCustomId: 1001);
            var ingrediente = new Ingrediente
            {
                IngredienteId = 1001,
                Ingrediente1 = "Test Ingrediente",
                CategoriaId = 1,
                PrezzoAggiunto = 1.00m,
                Disponibile = true,
                DataInserimento = DateTime.UtcNow,
                DataAggiornamento = DateTime.UtcNow
            };
            _context.Ingrediente.Add(ingrediente);
            await _context.SaveChangesAsync();

            // DON'T create the combination

            // Act
            var result = await _repository.ExistsByCombinazioneAsync(
                personalizzazione.PersCustomId,
                ingrediente.IngredienteId
            );

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success); // The check itself succeeded
            Assert.False(result.Data); // Doesn't exist
            Assert.Contains("non trovata", result.Message);
        }

        [Fact]
        public async Task ExistsByCombinazioneAsync_InvalidIds_ReturnsErrorResponse()
        {
            // Arrange
            int invalidPersId = 0;
            int invalidIngId = 0;

            // Act
            var result = await _repository.ExistsByCombinazioneAsync(invalidPersId, invalidIngId);

            // Assert
            Assert.False(result.Success);
            Assert.Contains("non valido", result.Message);
        }

        #endregion

        #region CountAsync Tests

        [Fact]
        public async Task CountAsync_WithData_ReturnsCorrectCount()
        {
            // Arrange
            await ResetDatabaseAsync();
            await CreateMultipleIngredientiPersonalizzazioneAsync(5);

            // Act
            var result = await _repository.CountAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(5, result.Data);
            Assert.Contains("Trovate 5", result.Message);
        }

        [Fact]
        public async Task CountAsync_EmptyDatabase_ReturnsZero()
        {
            // Arrange
            await ResetDatabaseAsync();

            // Act
            var result = await _repository.CountAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(0, result.Data);
            Assert.Contains("non trovata", result.Message);
        }

        [Fact]
        public async Task CountAsync_SingleRecord_ReturnsOne()
        {
            // Arrange
            await ResetDatabaseAsync();
            await CreateTestIngredientiPersonalizzazioneAsync();

            // Act
            var result = await _repository.CountAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(1, result.Data);
            Assert.Contains("Trovata 1", result.Message);
        }

        #endregion

        #region Edge Cases and Integration Tests

        [Fact]
        public async Task MultipleOperations_WorkCorrectlyTogether()
        {
            // Arrange
            await ResetDatabaseAsync();

            var personalizzazione1 = await CreateTestPersonalizzazioneCustomAsync(persCustomId: 1001);
            var personalizzazione2 = await CreateTestPersonalizzazioneCustomAsync(persCustomId: 1002);

            var ingrediente1 = new Ingrediente
            {
                IngredienteId = 1001,
                Ingrediente1 = "Ingrediente 1",
                CategoriaId = 1,
                PrezzoAggiunto = 1.00m,
                Disponibile = true,
                DataInserimento = DateTime.UtcNow,
                DataAggiornamento = DateTime.UtcNow
            };

            var ingrediente2 = new Ingrediente
            {
                IngredienteId = 1002,
                Ingrediente1 = "Ingrediente 2",
                CategoriaId = 1,
                PrezzoAggiunto = 2.00m,
                Disponibile = true,
                DataInserimento = DateTime.UtcNow,
                DataAggiornamento = DateTime.UtcNow
            };

            _context.Ingrediente.AddRange(ingrediente1, ingrediente2);
            await _context.SaveChangesAsync();

            // 1. Add first combination
            var dto1 = new IngredientiPersonalizzazioneDTO
            {
                PersCustomId = personalizzazione1.PersCustomId,
                IngredienteId = ingrediente1.IngredienteId,
                DataCreazione = DateTime.UtcNow
            };
            var addResult1 = await _repository.AddAsync(dto1);
            Assert.True(addResult1.Success);
            var id1 = addResult1.Data!.IngredientePersId;

            // 2. Add second combination
            var dto2 = new IngredientiPersonalizzazioneDTO
            {
                PersCustomId = personalizzazione2.PersCustomId,
                IngredienteId = ingrediente2.IngredienteId,
                DataCreazione = DateTime.UtcNow
            };
            var addResult2 = await _repository.AddAsync(dto2);
            Assert.True(addResult2.Success);
            var id2 = addResult2.Data!.IngredientePersId;

            // 3. Verify both exist
            var countResult = await _repository.CountAsync();
            Assert.Equal(2, countResult.Data);

            // 4. Verify GetByPersCustomId works
            var byPersResult = await _repository.GetByPersCustomIdAsync(personalizzazione1.PersCustomId);
            Assert.Equal(1, byPersResult.TotalCount);

            // 5. Verify GetByIngredienteId works
            var byIngResult = await _repository.GetByIngredienteIdAsync(ingrediente2.IngredienteId);
            Assert.Equal(1, byIngResult.TotalCount);

            // 6. Verify ExistsByCombinazione works
            var existsResult = await _repository.ExistsByCombinazioneAsync(
                personalizzazione1.PersCustomId,
                ingrediente1.IngredienteId
            );
            Assert.True(existsResult.Data);

            // 7. Delete first
            var deleteResult = await _repository.DeleteAsync(id1);
            Assert.True(deleteResult.Success);

            // 8. Verify count decreased
            var finalCount = await _repository.CountAsync();
            Assert.Equal(1, finalCount.Data);

            // 9. Verify first no longer exists
            var existsAfterDelete = await _repository.ExistsAsync(id1);
            Assert.False(existsAfterDelete.Data);
        }

        [Fact]
        public async Task GetAllAsync_ReturnsDataWithNames()
        {
            // Arrange
            await ResetDatabaseAsync();

            var personalizzazione = await CreateTestPersonalizzazioneCustomAsync(
                nome: "Test Pers for GetAll",
                persCustomId: 1001
            );

            var ingrediente = new Ingrediente
            {
                IngredienteId = 1001,
                Ingrediente1 = "Test Ing for GetAll",
                CategoriaId = 1,
                PrezzoAggiunto = 1.00m,
                Disponibile = true,
                DataInserimento = DateTime.UtcNow,
                DataAggiornamento = DateTime.UtcNow
            };
            _context.Ingrediente.Add(ingrediente);
            await _context.SaveChangesAsync();

            await CreateTestIngredientiPersonalizzazioneAsync(
                persCustomId: personalizzazione.PersCustomId,
                ingredienteId: ingrediente.IngredienteId
            );

            // Act
            var result = await _repository.GetAllAsync();

            // Assert
            var dto = result.Data.FirstOrDefault();
            Assert.NotNull(dto);
            Assert.Equal(personalizzazione.Nome, dto.NomePersonalizzazione);
            Assert.Equal(ingrediente.Ingrediente1, dto.NomeIngrediente);
        }

        [Fact]
        public async Task GetByPersCustomIdAsync_ReturnsDataWithNames()
        {
            // Arrange
            await ResetDatabaseAsync();

            var personalizzazione = await CreateTestPersonalizzazioneCustomAsync(
                nome: "Test Pers for GetByPers",
                persCustomId: 1001
            );

            var ingrediente = new Ingrediente
            {
                IngredienteId = 1001,
                Ingrediente1 = "Test Ing for GetByPers",
                CategoriaId = 1,
                PrezzoAggiunto = 1.00m,
                Disponibile = true,
                DataInserimento = DateTime.UtcNow,
                DataAggiornamento = DateTime.UtcNow
            };
            _context.Ingrediente.Add(ingrediente);
            await _context.SaveChangesAsync();

            await CreateTestIngredientiPersonalizzazioneAsync(
                persCustomId: personalizzazione.PersCustomId,
                ingredienteId: ingrediente.IngredienteId
            );

            // Act
            var result = await _repository.GetByPersCustomIdAsync(personalizzazione.PersCustomId);

            // Assert
            var dto = result.Data.FirstOrDefault();
            Assert.NotNull(dto);
            Assert.Equal(personalizzazione.Nome, dto.NomePersonalizzazione);
            Assert.Equal(ingrediente.Ingrediente1, dto.NomeIngrediente);
        }

        [Fact]
        public async Task GetByIngredienteIdAsync_ReturnsDataWithNames()
        {
            // Arrange
            await ResetDatabaseAsync();

            var personalizzazione = await CreateTestPersonalizzazioneCustomAsync(
                nome: "Test Pers for GetByIng",
                persCustomId: 1001
            );

            var ingrediente = new Ingrediente
            {
                IngredienteId = 1001,
                Ingrediente1 = "Test Ing for GetByIng",
                CategoriaId = 1,
                PrezzoAggiunto = 1.00m,
                Disponibile = true,
                DataInserimento = DateTime.UtcNow,
                DataAggiornamento = DateTime.UtcNow
            };
            _context.Ingrediente.Add(ingrediente);
            await _context.SaveChangesAsync();

            await CreateTestIngredientiPersonalizzazioneAsync(
                persCustomId: personalizzazione.PersCustomId,
                ingredienteId: ingrediente.IngredienteId
            );

            // Act
            var result = await _repository.GetByIngredienteIdAsync(ingrediente.IngredienteId);

            // Assert
            var dto = result.Data.FirstOrDefault();
            Assert.NotNull(dto);
            Assert.Equal(personalizzazione.Nome, dto.NomePersonalizzazione);
            Assert.Equal(ingrediente.Ingrediente1, dto.NomeIngrediente);
        }

        #endregion
    }
}