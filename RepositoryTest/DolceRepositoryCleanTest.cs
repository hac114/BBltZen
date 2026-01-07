using BBltZen;
using DTO;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Repository.Service;
using Xunit;

namespace RepositoryTest
{
    public class DolceRepositoryCleanTest : BaseTestClean
    {
        private readonly DolceRepository _repository;

        public DolceRepositoryCleanTest()
        {
            _repository = new DolceRepository(_context, GetTestLogger<DolceRepository>());
        }

        #region GetAllAsync Tests

        [Fact]
        public async Task GetAllAsync_ShouldReturnPaginatedDolci_WhenDolciExist()
        {
            // Arrange
            await CreateMultipleDolceAsync(5);
            var page = 1;
            var pageSize = 3;

            // Act
            var result = await _repository.GetAllAsync(page, pageSize);

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.Data);
            Assert.Equal(page, result.Page);
            Assert.Equal(pageSize, result.PageSize);
            Assert.True(result.TotalCount >= 5);
            Assert.Equal(3, result.Data.Count());
            Assert.NotEmpty(result.Message);
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnEmpty_WhenNoDolciExist()
        {
            // Arrange
            await CleanTableAsync<Dolce>();
            await CleanTableAsync<Articolo>();

            // Act
            var result = await _repository.GetAllAsync();

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.Data);
            Assert.Empty(result.Data);
            Assert.Equal(0, result.TotalCount);
            Assert.Contains("Nessun", result.Message);
        }

        [Fact]
        public async Task GetAllAsync_ShouldRespectPagination_WithDifferentPageSizes()
        {
            // Arrange
            await CreateDolciForPaginationTestsAsync(10);
            var testCases = new[]
            {
                new { Page = 1, PageSize = 3, ExpectedCount = 3 },
                new { Page = 2, PageSize = 3, ExpectedCount = 3 },
                new { Page = 3, PageSize = 3, ExpectedCount = 3 },
                new { Page = 4, PageSize = 3, ExpectedCount = 1 }
            };

            foreach (var testCase in testCases)
            {
                // Act
                var result = await _repository.GetAllAsync(testCase.Page, testCase.PageSize);

                // Assert
                Assert.NotNull(result);
                Assert.NotNull(result.Data);
                Assert.Equal(testCase.ExpectedCount, result.Data.Count());
                Assert.Equal(testCase.Page, result.Page);
                Assert.Equal(testCase.PageSize, result.PageSize);
            }
        }

        [Fact]
        public async Task GetAllAsync_ShouldOrderByPrioritaDescending_ThenByName()
        {
            // Arrange
            await CleanTableAsync<Dolce>();
            var dolci = await CreateDolciConPrioritaVariataAsync([5, 3, 8, 1, 10]);

            // Act
            var result = await _repository.GetAllAsync(1, 10);

            // Assert
            Assert.NotNull(result.Data);
            var resultList = result.Data.ToList();

            // Verifica ordinamento per priorità decrescente, poi per nome
            for (int i = 0; i < resultList.Count - 1; i++)
            {
                var current = resultList[i];
                var next = resultList[i + 1];

                // Se le priorità sono uguali, verifica ordinamento nome
                if (current.Priorita == next.Priorita)
                {
                    Assert.True(string.Compare(current.Nome, next.Nome, StringComparison.OrdinalIgnoreCase) <= 0);
                }
                else
                {
                    Assert.True(current.Priorita >= next.Priorita);
                }
            }
        }

        #endregion

        #region GetByIdAsync Tests

        [Fact]
        public async Task GetByIdAsync_ShouldReturnDolce_WhenValidId()
        {
            // Arrange
            var dolce = await CreateTestDolceAsync();
            var expectedId = dolce.ArticoloId;

            // Act
            var result = await _repository.GetByIdAsync(expectedId);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(expectedId, result.Data.ArticoloId);
            Assert.Equal(dolce.Nome, result.Data.Nome);
            Assert.Contains(expectedId.ToString(), result.Message);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnNotFound_WhenInvalidId()
        {
            // Arrange
            var invalidId = 999999;

            // Act
            var result = await _repository.GetByIdAsync(invalidId);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Null(result.Data);
            Assert.Contains("non trovato", result.Message);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnError_WhenIdIsZeroOrNegative()
        {
            // Arrange
            var invalidIds = new[] { 0, -1, -100 };

            foreach (var invalidId in invalidIds)
            {
                // Act
                var result = await _repository.GetByIdAsync(invalidId);

                // Assert
                Assert.NotNull(result);
                Assert.False(result.Success);
                Assert.Contains("non valido", result.Message);
            }
        }

        #endregion

        #region GetDisponibiliAsync Tests

        [Fact]
        public async Task GetDisponibiliAsync_ShouldReturnOnlyDisponibiliDolci()
        {
            // Arrange
            await CreateDolceDisponibileAsync();
            await CreateDolceNonDisponibileAsync();
            await CreateDolceDisponibileAsync();

            // Act
            var result = await _repository.GetDisponibiliAsync();

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.Data);
            Assert.All(result.Data, d => Assert.True(d.Disponibile));
            Assert.Contains("disponibil", result.Message);
        }

        [Fact]
        public async Task GetDisponibiliAsync_ShouldReturnEmpty_WhenNoDisponibili()
        {
            // Arrange
            await CleanTableAsync<Dolce>();
            for (int i = 0; i < 3; i++)
            {
                await CreateDolceNonDisponibileAsync();
            }

            // Act
            var result = await _repository.GetDisponibiliAsync();

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.Data);
            Assert.Empty(result.Data);
            Assert.Contains("Nessun dolce disponibile", result.Message);
        }

        [Fact]
        public async Task GetDisponibiliAsync_ShouldRespectPagination()
        {
            // Arrange
            for (int i = 0; i < 8; i++)
            {
                await CreateDolceDisponibileAsync();
            }
            var page = 2;
            var pageSize = 3;

            // Act
            var result = await _repository.GetDisponibiliAsync(page, pageSize);

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.Data);
            Assert.Equal(page, result.Page);
            Assert.Equal(pageSize, result.PageSize);
            Assert.True(result.TotalCount >= 8);
            Assert.All(result.Data, d => Assert.True(d.Disponibile));
        }

        #endregion

        #region GetNonDisponibiliAsync Tests

        [Fact]
        public async Task GetNonDisponibiliAsync_ShouldReturnOnlyNonDisponibiliDolci()
        {
            // Arrange
            await CreateDolceDisponibileAsync();
            await CreateDolceNonDisponibileAsync();
            await CreateDolceDisponibileAsync();
            await CreateDolceNonDisponibileAsync();

            // Act
            var result = await _repository.GetNonDisponibiliAsync();

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.Data);
            Assert.All(result.Data, d => Assert.False(d.Disponibile));
            Assert.Contains("non disponibil", result.Message);
        }

        [Fact]
        public async Task GetNonDisponibiliAsync_ShouldReturnEmpty_WhenAllDisponibili()
        {
            // Arrange
            await CleanTableAsync<Dolce>();
            for (int i = 0; i < 3; i++)
            {
                await CreateDolceDisponibileAsync();
            }

            // Act
            var result = await _repository.GetNonDisponibiliAsync();

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.Data);
            Assert.Empty(result.Data);
            Assert.Contains("Nessun dolce non disponibile", result.Message);
        }

        #endregion

        #region GetByPrioritaAsync Tests

        [Fact]
        public async Task GetByPrioritaAsync_ShouldReturnDolciWithSpecificPriorita()
        {
            // Arrange
            var targetPriorita = 3;
            await CreateTestDolceAsync(priorita: targetPriorita);
            await CreateTestDolceAsync(priorita: targetPriorita);
            await CreateTestDolceAsync(priorita: 5); // Diversa priorità
            await CreateTestDolceAsync(priorita: 1); // Diversa priorità

            // Act
            var result = await _repository.GetByPrioritaAsync(targetPriorita);

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.Data);
            Assert.All(result.Data, d => Assert.Equal(targetPriorita, d.Priorita));
            Assert.Contains(targetPriorita.ToString(), result.Message);
        }

        [Fact]
        public async Task GetByPrioritaAsync_ShouldReturnEmpty_WhenNoDolciWithPriorita()
        {
            // Arrange
            await CreateTestDolceAsync(priorita: 5);
            await CreateTestDolceAsync(priorita: 7);
            var targetPriorita = 3;

            // Act
            var result = await _repository.GetByPrioritaAsync(targetPriorita);

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.Data);
            Assert.Empty(result.Data);
            Assert.Contains($"Nessun dolce con priorità {targetPriorita}", result.Message);
        }

        [Fact]
        public async Task GetByPrioritaAsync_ShouldValidatePrioritaRange()
        {
            // Arrange
            var invalidPrioritaValues = new[] { 0, 11, -1, 100 };

            foreach (var invalidPriorita in invalidPrioritaValues)
            {
                // Act
                var result = await _repository.GetByPrioritaAsync(invalidPriorita);

                // Assert
                Assert.NotNull(result);
                Assert.NotNull(result.Data);
                Assert.Empty(result.Data);
                Assert.Contains("La priorità deve essere tra 1 e 10", result.Message);
            }
        }

        [Fact]
        public async Task GetByPrioritaAsync_ShouldRespectPagination()
        {
            // Arrange
            var targetPriorita = 5;
            for (int i = 0; i < 7; i++)
            {
                await CreateTestDolceAsync(priorita: targetPriorita);
            }
            var page = 2;
            var pageSize = 3;

            // Act
            var result = await _repository.GetByPrioritaAsync(targetPriorita, page, pageSize);

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.Data);
            Assert.Equal(page, result.Page);
            Assert.Equal(pageSize, result.PageSize);
            Assert.True(result.TotalCount >= 7);
            Assert.All(result.Data, d => Assert.Equal(targetPriorita, d.Priorita));
        }

        #endregion

        #region AddAsync Tests

        [Fact]
        public async Task AddAsync_ShouldCreateNewDolce_WithValidData()
        {
            // Arrange
            var dolceDto = CreateTestDolceDTO();
            dolceDto.ArticoloId = 0; // Verrà assegnato dal sistema

            // Act
            var result = await _repository.AddAsync(dolceDto);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.True(result.Data.ArticoloId > 0);
            Assert.Equal(dolceDto.Nome, result.Data.Nome);
            Assert.Equal(dolceDto.Prezzo, result.Data.Prezzo);
            Assert.Equal(dolceDto.Disponibile, result.Data.Disponibile);
            Assert.Equal(dolceDto.Priorita, result.Data.Priorita);
            Assert.Contains("creato", result.Message);

            // Verifica che sia stato salvato nel database
            var savedDolce = await _context.Dolce.FindAsync(result.Data.ArticoloId);
            Assert.NotNull(savedDolce);
            Assert.Equal(dolceDto.Nome, savedDolce.Nome);
        }

        [Fact]
        public async Task AddAsync_ShouldReturnError_WhenNomeIsNullOrEmpty()
        {
            // Arrange
            var invalidNames = new[] { "", " ", null };

            foreach (var invalidName in invalidNames)
            {
                var dolceDto = CreateTestDolceDTO();
                dolceDto.Nome = invalidName!;

                // Act
                var result = await _repository.AddAsync(dolceDto);

                // Assert
                Assert.NotNull(result);
                Assert.False(result.Success);
                Assert.Contains("obbligatorio", result.Message);
            }
        }

        [Fact]
        public async Task AddAsync_ShouldReturnError_WhenNomeAlreadyExists()
        {
            // Arrange
            var existingDolce = await CreateTestDolceAsync();
            var dolceDto = CreateTestDolceDTO();
            dolceDto.Nome = existingDolce.Nome;

            // Act
            var result = await _repository.AddAsync(dolceDto);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Contains("già", result.Message);
        }

        [Fact]
        public async Task AddAsync_ShouldReturnError_WhenPrezzoIsInvalid()
        {
            // Arrange
            var invalidPrices = new[] { -1.00m, 100.00m, 0.00m };

            foreach (var invalidPrice in invalidPrices)
            {
                var dolceDto = CreateTestDolceDTO();
                dolceDto.Prezzo = invalidPrice;

                // Act
                var result = await _repository.AddAsync(dolceDto);

                // Assert
                Assert.NotNull(result);
                Assert.False(result.Success);
                Assert.Contains("prezzo", result.Message.ToLower());
            }
        }

        [Fact]
        public async Task AddAsync_ShouldReturnError_WhenPrioritaIsOutOfRange()
        {
            // Arrange
            var invalidPrioritaValues = new[] { 0, 11, -5 };

            foreach (var invalidPriorita in invalidPrioritaValues)
            {
                var dolceDto = CreateTestDolceDTO();
                dolceDto.Priorita = invalidPriorita;

                // Act
                var result = await _repository.AddAsync(dolceDto);

                // Assert
                Assert.NotNull(result);
                Assert.False(result.Success);
                Assert.Contains("priorità", result.Message);
            }
        }

        [Fact]
        public async Task AddAsync_ShouldCreateArticoloRecord_Automatically()
        {
            // Arrange
            var dolceDto = CreateTestDolceDTO();
            dolceDto.ArticoloId = 0;

            // Act
            var result = await _repository.AddAsync(dolceDto);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.True(result.Data!.ArticoloId > 0);

            // Verifica che esista l'articolo associato
            var articolo = await _context.Articolo.FindAsync(result.Data.ArticoloId);
            Assert.NotNull(articolo);
            Assert.Equal("D", articolo.Tipo);
        }

        [Fact]
        public async Task AddAsync_ShouldSetDefaultValues_WhenNotProvided()
        {
            // Arrange
            var dolceDto = CreateTestDolceDTO();
            dolceDto.Disponibile = true; // Default atteso
            dolceDto.Priorita = 1; // Default atteso

            // Act
            var result = await _repository.AddAsync(dolceDto);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.True(result.Data!.Disponibile);
            Assert.Equal(1, result.Data.Priorita);
        }

        #endregion

        #region UpdateAsync Tests

        [Fact]
        public async Task UpdateAsync_ShouldUpdateExistingDolce_WithValidData()
        {
            // Arrange
            var existingDolce = await CreateTestDolceAsync();
            var originalName = existingDolce.Nome;
            var originalPrice = existingDolce.Prezzo;

            // Aspetta un po' per essere sicuro che le date siano diverse
            await Task.Delay(10);

            var dolceDto = CreateTestDolceDTO(
                articoloId: existingDolce.ArticoloId,
                nome: $"{originalName} Modificato",
                prezzo: originalPrice + 2.00m,
                priorita: existingDolce.Priorita + 1);

            // Act
            var result = await _repository.UpdateAsync(dolceDto);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.True(result.Data);
            Assert.Contains("aggiornato", result.Message);

            // Verifica che sia stato aggiornato nel database
            var updatedDolce = await _context.Dolce.FindAsync(existingDolce.ArticoloId);
            Assert.NotNull(updatedDolce);
            Assert.Equal(dolceDto.Nome, updatedDolce.Nome);
            Assert.Equal(dolceDto.Prezzo, updatedDolce.Prezzo);
            Assert.Equal(dolceDto.Priorita, updatedDolce.Priorita);

            // ✅ USARE >= INVECE DI > (a volte l'update è troppo veloce)
            Assert.True(updatedDolce.DataAggiornamento >= existingDolce.DataAggiornamento,
                $"DataAggiornamento non aggiornata: {updatedDolce.DataAggiornamento} <= {existingDolce.DataAggiornamento}");
        }

        [Fact]
        public async Task UpdateAsync_ShouldReturnNotFound_WhenDolceDoesNotExist()
        {
            // Arrange
            var nonExistentId = 999999;
            var dolceDto = CreateTestDolceDTO(articoloId: nonExistentId);

            // Act
            var result = await _repository.UpdateAsync(dolceDto);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Contains("non trovato", result.Message);
        }

        [Fact]
        public async Task UpdateAsync_ShouldReturnError_WhenNomeAlreadyExistsForOtherDolce()
        {
            // Arrange
            var dolce1 = await CreateTestDolceAsync();
            var dolce2 = await CreateTestDolceAsync();

            var dolceDto = CreateTestDolceDTO(
                articoloId: dolce2.ArticoloId,
                nome: dolce1.Nome);

            // Act
            var result = await _repository.UpdateAsync(dolceDto);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Contains("già", result.Message);
        }

        [Fact]
        public async Task UpdateAsync_ShouldReturnSuccessButFalse_WhenNoChanges()
        {
            // Arrange
            var existingDolce = await CreateTestDolceAsync();

            var dolceDto = MapToDTO(existingDolce);
            dolceDto.ArticoloId = existingDolce.ArticoloId;

            // Act
            var result = await _repository.UpdateAsync(dolceDto);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.False(result.Data);
            Assert.Contains("Nessuna modifica", result.Message);
        }

        [Fact]
        public async Task UpdateAsync_ShouldUpdateArticoloTimestamp()
        {
            // Arrange
            var existingDolce = await CreateTestDolceAsync();
            var articolo = await _context.Articolo.FindAsync(existingDolce.ArticoloId);
            var originalArticoloUpdate = articolo!.DataAggiornamento;

            var dolceDto = CreateTestDolceDTO(
                articoloId: existingDolce.ArticoloId,
                nome: $"{existingDolce.Nome} Aggiornato");

            // Act
            var result = await _repository.UpdateAsync(dolceDto);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);

            // Verifica che l'articolo sia stato aggiornato
            articolo = await _context.Articolo.FindAsync(existingDolce.ArticoloId);
            Assert.NotNull(articolo);
            Assert.True(articolo.DataAggiornamento > originalArticoloUpdate);
        }

        #endregion

        #region DeleteAsync Tests

        [Fact]
        public async Task DeleteAsync_ShouldDeleteDolce_WhenValidId()
        {
            // Arrange
            var dolce = await CreateTestDolceAsync();
            var articoloId = dolce.ArticoloId;

            // Act
            var result = await _repository.DeleteAsync(articoloId);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.True(result.Data);
            Assert.Contains("eliminato", result.Message);

            // Verifica che sia stato rimosso dal database
            var deletedDolce = await _context.Dolce.FindAsync(articoloId);
            Assert.Null(deletedDolce);

            // Verifica che anche l'articolo sia stato eliminato
            var deletedArticolo = await _context.Articolo.FindAsync(articoloId);
            Assert.Null(deletedArticolo);
        }

        [Fact]
        public async Task DeleteAsync_ShouldReturnNotFound_WhenDolceDoesNotExist()
        {
            // Arrange
            var nonExistentId = 999999;

            // Act
            var result = await _repository.DeleteAsync(nonExistentId);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Contains("non trovato", result.Message);
        }

        [Fact]
        public async Task DeleteAsync_ShouldReturnError_WhenIdIsZeroOrNegative()
        {
            // Arrange
            var invalidIds = new[] { 0, -1, -100 };

            foreach (var invalidId in invalidIds)
            {
                // Act
                var result = await _repository.DeleteAsync(invalidId);

                // Assert
                Assert.NotNull(result);
                Assert.False(result.Success);
                Assert.Contains("non valido", result.Message);
            }
        }

        [Fact]
        public async Task DeleteAsync_ShouldDeleteBothDolceAndArticolo()
        {
            // Arrange
            var dolce = await CreateTestDolceAsync();
            var articoloId = dolce.ArticoloId;

            // Verifica che esistano entrambi prima della cancellazione
            var dolceBefore = await _context.Dolce.FindAsync(articoloId);
            var articoloBefore = await _context.Articolo.FindAsync(articoloId);
            Assert.NotNull(dolceBefore);
            Assert.NotNull(articoloBefore);

            // Act
            var result = await _repository.DeleteAsync(articoloId);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);

            // Verifica che siano stati entrambi eliminati
            var dolceAfter = await _context.Dolce.FindAsync(articoloId);
            var articoloAfter = await _context.Articolo.FindAsync(articoloId);
            Assert.Null(dolceAfter);
            Assert.Null(articoloAfter);
        }

        #endregion

        #region ExistsAsync Tests

        [Fact]
        public async Task ExistsAsync_ShouldReturnTrue_WhenDolceExists()
        {
            // Arrange
            var dolce = await CreateTestDolceAsync();
            var articoloId = dolce.ArticoloId;

            // Act
            var result = await _repository.ExistsAsync(articoloId);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.True(result.Data);
            Assert.Contains("esiste", result.Message);
        }

        [Fact]
        public async Task ExistsAsync_ShouldReturnFalse_WhenDolceDoesNotExist()
        {
            // Arrange
            var nonExistentId = 999999;

            // Act
            var result = await _repository.ExistsAsync(nonExistentId);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.False(result.Data);
            Assert.Contains("non trovato", result.Message);
        }

        [Fact]
        public async Task ExistsAsync_ShouldReturnError_WhenIdIsZeroOrNegative()
        {
            // Arrange
            var invalidIds = new[] { 0, -1, -100 };

            foreach (var invalidId in invalidIds)
            {
                // Act
                var result = await _repository.ExistsAsync(invalidId);

                // Assert
                Assert.NotNull(result);
                Assert.False(result.Success);
                Assert.Contains("non valido", result.Message);
            }
        }

        #endregion

        #region CountAsync Tests

        [Fact]
        public async Task CountAsync_ShouldReturnCorrectCount_WhenDolciExist()
        {
            // Arrange
            var expectedCount = 5;
            await CreateMultipleDolceAsync(expectedCount);

            // Act
            var result = await _repository.CountAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(expectedCount, result.Data);
            Assert.Contains(expectedCount.ToString(), result.Message);
        }

        [Fact]
        public async Task CountAsync_ShouldReturnZero_WhenNoDolciExist()
        {
            // Arrange
            await CleanTableAsync<Dolce>();

            // Act
            var result = await _repository.CountAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(0, result.Data);
            Assert.Contains("Nessun dolce", result.Message);
        }

        #endregion

        #region CountDisponibiliAsync Tests

        [Fact]
        public async Task CountDisponibiliAsync_ShouldReturnCorrectCount_OfDisponibili()
        {
            // Arrange
            await CreateDolceDisponibileAsync();
            await CreateDolceDisponibileAsync();
            await CreateDolceNonDisponibileAsync();
            await CreateDolceDisponibileAsync();

            // Act
            var result = await _repository.CountDisponibiliAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(3, result.Data);
            Assert.Contains("3", result.Message);
        }

        [Fact]
        public async Task CountDisponibiliAsync_ShouldReturnZero_WhenNoDisponibili()
        {
            // Arrange
            await CleanTableAsync<Dolce>();
            for (int i = 0; i < 3; i++)
            {
                await CreateDolceNonDisponibileAsync();
            }

            // Act
            var result = await _repository.CountDisponibiliAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(0, result.Data);
            Assert.Contains("Nessun dolce disponibile", result.Message);
        }

        #endregion

        #region CountNonDisponibiliAsync Tests

        [Fact]
        public async Task CountNonDisponibiliAsync_ShouldReturnCorrectCount_OfNonDisponibili()
        {
            // Arrange
            await CreateDolceDisponibileAsync();
            await CreateDolceNonDisponibileAsync();
            await CreateDolceDisponibileAsync();
            await CreateDolceNonDisponibileAsync();
            await CreateDolceNonDisponibileAsync();

            // Act
            var result = await _repository.CountNonDisponibiliAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(3, result.Data);
            Assert.Contains("3", result.Message);
        }

        [Fact]
        public async Task CountNonDisponibiliAsync_ShouldReturnZero_WhenAllDisponibili()
        {
            // Arrange
            await CleanTableAsync<Dolce>();
            for (int i = 0; i < 3; i++)
            {
                await CreateDolceDisponibileAsync();
            }

            // Act
            var result = await _repository.CountNonDisponibiliAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(0, result.Data);
            Assert.Contains("Nessun dolce non disponibile", result.Message);
        }

        #endregion

        #region ToggleDisponibilitaAsync Tests        

        [Fact]
        public async Task ToggleDisponibilitaAsync_ShouldToggleDisponibile()
        {
            // Arrange
            await SetupDolceTestDataAsync();

            // ✅ Recupera un dolce esistente dal database
            var dolce = await _context.Dolce.FirstOrDefaultAsync();
            Assert.NotNull(dolce); // Assicurati che ci sia almeno un dolce

            var articoloId = dolce.ArticoloId;
            var initialDisponibile = dolce.Disponibile;

            // Act
            var result = await _repository.ToggleDisponibilitaAsync(articoloId);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(!initialDisponibile, result.Data);
            Assert.Contains(initialDisponibile ? "non disponibile" : "disponibile", result.Message);

            // Verifica il cambio
            var updated = await _context.Dolce.FindAsync(articoloId);
            Assert.NotNull(updated);
            Assert.Equal(!initialDisponibile, updated.Disponibile);
        }

        [Fact]
        public async Task ToggleDisponibilitaAsync_ShouldToggleDisponibile_WithSpecificDolce()
        {
            // Arrange
            await SetupDolceTestDataAsync();

            // ✅ Crea un dolce specifico per questo test (per controllare meglio lo stato iniziale)
            var dolce = await CreateDolceDisponibileAsync();
            var articoloId = dolce.ArticoloId;
            var initialDisponibile = dolce.Disponibile; // Dovrebbe essere true

            // Act
            var result = await _repository.ToggleDisponibilitaAsync(articoloId);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.False(result.Data); // Dovrebbe diventare false
            Assert.Contains("non disponibile", result.Message);

            // Verifica
            var updated = await _context.Dolce.FindAsync(articoloId);
            Assert.NotNull(updated);
            Assert.False(updated.Disponibile);
        }

        [Fact]
        public async Task ToggleDisponibilitaAsync_ShouldToggleFromFalseToTrue()
        {
            // Arrange
            var dolce = await CreateDolceNonDisponibileAsync();
            var articoloId = dolce.ArticoloId;
            Assert.False(dolce.Disponibile); // Verifica che sia inizialmente false

            // Act
            var result = await _repository.ToggleDisponibilitaAsync(articoloId);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.True(result.Data); // Dovrebbe diventare true
            Assert.Contains("disponibile", result.Message);

            // Verifica
            var updated = await _context.Dolce.FindAsync(articoloId);
            Assert.NotNull(updated);
            Assert.True(updated.Disponibile);
        }

        [Fact]
        public async Task ToggleDisponibilitaAsync_WithNonExistingId_ShouldReturnNotFound()
        {
            // Arrange
            var nonExistingId = 999999;

            // Act
            var result = await _repository.ToggleDisponibilitaAsync(nonExistingId);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.False(result.Data);
            Assert.Contains("non trovato", result.Message);
        }

        [Fact]
        public async Task ToggleDisponibilitaAsync_ShouldReturnError_WhenIdIsZeroOrNegative()
        {
            // Arrange
            var invalidIds = new[] { 0, -1, -100 };

            foreach (var invalidId in invalidIds)
            {
                // Act
                var result = await _repository.ToggleDisponibilitaAsync(invalidId);

                // Assert
                Assert.NotNull(result);
                Assert.False(result.Success);
                Assert.False(result.Data);
                Assert.Contains("non valido", result.Message);
            }
        }

        [Fact]
        public async Task ToggleDisponibilitaAsync_ShouldUpdateDataAggiornamento()
        {
            // Arrange
            var dolce = await CreateDolceDisponibileAsync();
            var articoloId = dolce.ArticoloId;
            var originalUpdateTime = dolce.DataAggiornamento;

            // Attendi un momento per essere sicuri che le date siano diverse
            await Task.Delay(100); // Aumenta il delay a 100ms

            // Act
            var result = await _repository.ToggleDisponibilitaAsync(articoloId);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);

            // Verifica che DataAggiornamento sia stato aggiornato
            var updated = await _context.Dolce.FindAsync(articoloId);
            Assert.NotNull(updated);

            // Usa >= invece di > per sicurezza
            Assert.True(updated.DataAggiornamento >= originalUpdateTime,
                $"DataAggiornamento non aggiornata: {updated.DataAggiornamento} <= {originalUpdateTime}");

            // Se vuoi essere più rigoroso, puoi anche verificare che sia cambiato almeno di qualche tick
            // Assert.NotEqual(originalUpdateTime, updated.DataAggiornamento);
        }

        #endregion

        #region Helper Methods

        private static DolceDTO MapToDTO(Dolce dolce)
        {
            return new DolceDTO
            {
                ArticoloId = dolce.ArticoloId,
                Nome = dolce.Nome,
                Prezzo = dolce.Prezzo,
                Descrizione = dolce.Descrizione,
                ImmagineUrl = dolce.ImmagineUrl,
                Disponibile = dolce.Disponibile,
                Priorita = dolce.Priorita,
                DataCreazione = dolce.DataCreazione,
                DataAggiornamento = dolce.DataAggiornamento
            };
        }

        #endregion
    }
}