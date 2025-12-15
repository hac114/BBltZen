using BBltZen;
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
    public class StatoPagamentoRepositoryCleanTest : BaseTestClean
    {
        private readonly StatoPagamentoRepository _repository;

        public StatoPagamentoRepositoryCleanTest()
        {
            _repository = new StatoPagamentoRepository(_context, GetTestLogger<StatoPagamentoRepository>());
        }

        #region GetAllAsync Tests

        [Fact]
        public async Task GetAllAsync_ShouldReturnPaginatedResponse()
        {
            // Arrange
            await SetupStatoPagamentoTestDataAsync();

            // Act
            var result = await _repository.GetAllAsync(page: 1, pageSize: 3);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(5, result.TotalCount);
            Assert.Equal(3, result.Data.Count());
            Assert.Equal(1, result.Page);
            Assert.Equal(3, result.PageSize);
        }

        [Fact]
        public async Task GetAllAsync_WithPageSizeLargerThanTotal_ShouldReturnAllItems()
        {
            // Arrange
            await SetupStatoPagamentoTestDataAsync();

            // Act
            var result = await _repository.GetAllAsync(page: 1, pageSize: 20);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(5, result.TotalCount);
            Assert.Equal(5, result.Data.Count());
        }

        [Fact]
        public async Task GetAllAsync_WithSecondPage_ShouldReturnCorrectItems()
        {
            // Arrange
            await SetupStatoPagamentoTestDataAsync();

            // Act
            var result = await _repository.GetAllAsync(page: 2, pageSize: 3);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(5, result.TotalCount);
            Assert.Equal(2, result.Data.Count());
            Assert.Equal(2, result.Page);
        }

        [Fact]
        public async Task GetAllAsync_WithInvalidPage_ShouldUseSafeValues()
        {
            // Arrange
            await SetupStatoPagamentoTestDataAsync();

            // Act
            var result = await _repository.GetAllAsync(page: 0, pageSize: 0);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Page);
            Assert.True(result.PageSize > 0);
        }

        [Fact]
        public async Task GetAllAsync_WithEmptyTable_ShouldReturnEmptyList()
        {
            // Arrange
            await CleanTableAsync<StatoPagamento>();

            // Act
            var result = await _repository.GetAllAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(0, result.TotalCount);
            Assert.Empty(result.Data);
            Assert.Contains("Nessuno stato pagamento trovato", result.Message);
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnOrderedResults()
        {
            // Arrange
            await CleanTableAsync<StatoPagamento>();
            // Creiamo stati in ordine sparso
            await CreateTestStatoPagamentoAsync("fallito");
            await CreateTestStatoPagamentoAsync("non richiesto");
            await CreateTestStatoPagamentoAsync("pendente");

            // Act
            var result = await _repository.GetAllAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, result.TotalCount);
            var items = result.Data.ToList();
            // Secondo comparatore: non richiesto (1), pendente (2), fallito (4)
            Assert.Equal("non richiesto", items[0].StatoPagamento1);
            Assert.Equal("pendente", items[1].StatoPagamento1);
            Assert.Equal("fallito", items[2].StatoPagamento1);
        }

        #endregion

        #region GetByIdAsync Tests

        [Fact]
        public async Task GetByIdAsync_WithValidId_ShouldReturnStatoPagamento()
        {
            // Arrange
            await SetupStatoPagamentoTestDataAsync();
            int existingId = 1;

            // Act
            var result = await _repository.GetByIdAsync(existingId);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(existingId, result.Data.StatoPagamentoId);
            Assert.Equal("non richiesto", result.Data.StatoPagamento1);
        }

        [Fact]
        public async Task GetByIdAsync_WithNonExistingId_ShouldReturnNotFound()
        {
            // Arrange
            await SetupStatoPagamentoTestDataAsync();
            int nonExistingId = 999;

            // Act
            var result = await _repository.GetByIdAsync(nonExistingId);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Equal($"Stato pagamento con ID {nonExistingId} non trovato", result.Message);
        }

        [Fact]
        public async Task GetByIdAsync_WithZeroId_ShouldReturnError()
        {
            // Arrange
            int zeroId = 0;

            // Act
            var result = await _repository.GetByIdAsync(zeroId);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Equal("ID stato pagamento non valido", result.Message);
        }

        [Fact]
        public async Task GetByIdAsync_WithNegativeId_ShouldReturnError()
        {
            // Arrange
            int negativeId = -1;

            // Act
            var result = await _repository.GetByIdAsync(negativeId);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Equal("ID stato pagamento non valido", result.Message);
        }

        #endregion

        #region GetByNomeAsync Tests

        [Fact]
        public async Task GetByNomeAsync_WithValidNome_ShouldReturnStatoPagamento()
        {
            // Arrange
            await SetupStatoPagamentoTestDataAsync();
            string existingNome = "non richiesto";

            // Act
            var result = await _repository.GetByNomeAsync(existingNome);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(existingNome, result.Data.StatoPagamento1);
            Assert.Equal(1, result.Data.StatoPagamentoId);
        }

        [Fact]
        public async Task GetByNomeAsync_WithCaseInsensitiveNome_ShouldReturnStatoPagamento()
        {
            // Arrange
            await SetupStatoPagamentoTestDataAsync();

            // Act
            var result = await _repository.GetByNomeAsync("NON RICHIESTO");

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal("non richiesto", result.Data.StatoPagamento1);
        }

        [Fact]
        public async Task GetByNomeAsync_WithNonExistingNome_ShouldReturnNotFound()
        {
            // Arrange
            await SetupStatoPagamentoTestDataAsync();
            string nonExistingNome = "inesistente";

            // Act
            var result = await _repository.GetByNomeAsync(nonExistingNome);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Equal($"Nessuno stato pagamento trovato con nome '{nonExistingNome}'", result.Message);
        }

        [Fact]
        public async Task GetByNomeAsync_WithNullNome_ShouldReturnError()
        {
            // Arrange
            string? nullNome = null;

            // Act
            var result = await _repository.GetByNomeAsync(nullNome!);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Equal("Il parametro 'nomeStatoPagamento' è obbligatorio", result.Message);
        }

        [Fact]
        public async Task GetByNomeAsync_WithEmptyNome_ShouldReturnError()
        {
            // Arrange
            string emptyNome = "";

            // Act
            var result = await _repository.GetByNomeAsync(emptyNome);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Equal("Il parametro 'nomeStatoPagamento' è obbligatorio", result.Message);
        }

        [Fact]
        public async Task GetByNomeAsync_WithInvalidInput_ShouldReturnError()
        {
            // Arrange
            string invalidInput = new string('a', 101); // Troppo lungo

            // Act
            var result = await _repository.GetByNomeAsync(invalidInput);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Contains("non validi", result.Message);
        }

        #endregion

        #region AddAsync Tests

        [Fact]
        public async Task AddAsync_WithValidDto_ShouldCreateNewStatoPagamento()
        {
            // Arrange
            await CleanTableAsync<StatoPagamento>();
            var dto = new StatoPagamentoDTO
            {
                StatoPagamento1 = "nuovo_stato"
            };

            // Act
            var result = await _repository.AddAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal("nuovo_stato", result.Data.StatoPagamento1);
            Assert.True(result.Data.StatoPagamentoId > 0);
            Assert.Contains("creato con successo", result.Message);

            // Verify in database
            var fromDb = await _context.StatoPagamento.FindAsync(result.Data.StatoPagamentoId);
            Assert.NotNull(fromDb);
            Assert.Equal("nuovo_stato", fromDb.StatoPagamento1);
        }

        [Fact]
        public async Task AddAsync_WithNullDto_ShouldThrowException()
        {
            // Arrange
            StatoPagamentoDTO nullDto = null!;

            // Act
            var result = await _repository.AddAsync(nullDto);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Equal("Errore interno durante la creazione dello stato pagamento", result.Message);
        }

        [Fact]
        public async Task AddAsync_WithEmptyNome_ShouldReturnError()
        {
            // Arrange
            var dto = new StatoPagamentoDTO
            {
                StatoPagamento1 = ""
            };

            // Act
            var result = await _repository.AddAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Equal("Nome stato pagamento obbligatorio", result.Message);
        }

        [Fact]
        public async Task AddAsync_WithExistingNome_ShouldReturnError()
        {
            // Arrange
            await SetupStatoPagamentoTestDataAsync();
            var dto = new StatoPagamentoDTO
            {
                StatoPagamento1 = "non richiesto" // Esiste già nel seed
            };

            // Act
            var result = await _repository.AddAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Contains("Esiste già", result.Message);
        }

        [Fact]
        public async Task AddAsync_WithInvalidInput_ShouldReturnError()
        {
            // Arrange
            var dto = new StatoPagamentoDTO
            {
                StatoPagamento1 = new string('a', 101) // Troppo lungo
            };

            // Act
            var result = await _repository.AddAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Contains("non valido", result.Message);
        }

        [Fact]
        public async Task AddAsync_WithTrimmedNome_ShouldTrimSpaces()
        {
            // Arrange
            await CleanTableAsync<StatoPagamento>();
            var dto = new StatoPagamentoDTO
            {
                StatoPagamento1 = "  stato_con_spazi  "
            };

            // Act
            var result = await _repository.AddAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal("stato_con_spazi", result.Data.StatoPagamento1);
        }

        #endregion

        #region UpdateAsync Tests

        [Fact]
        public async Task UpdateAsync_WithValidDto_ShouldUpdateStatoPagamento()
        {
            // Arrange
            await SetupStatoPagamentoTestDataAsync();
            var stato = await _context.StatoPagamento.FirstAsync(s => s.StatoPagamentoId == 1);
            var originalName = stato.StatoPagamento1;

            var dto = new StatoPagamentoDTO
            {
                StatoPagamentoId = 1,
                StatoPagamento1 = "non richiesto modificato"
            };

            // Act
            var result = await _repository.UpdateAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.True(result.Data); // bool result

            // Verify changes
            var updated = await _context.StatoPagamento.FindAsync(1);
            Assert.NotNull(updated);
            Assert.Equal("non richiesto modificato", updated.StatoPagamento1);
            Assert.NotEqual(originalName, updated.StatoPagamento1);
        }

        [Fact]
        public async Task UpdateAsync_WithNoChanges_ShouldReturnFalseWithMessage()
        {
            // Arrange
            await SetupStatoPagamentoTestDataAsync();
            var stato = await _context.StatoPagamento.FirstAsync(s => s.StatoPagamentoId == 1);

            var dto = new StatoPagamentoDTO
            {
                StatoPagamentoId = 1,
                StatoPagamento1 = "non richiesto" // Stesso nome
            };

            // Act
            var result = await _repository.UpdateAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.False(result.Data); // Nessuna modifica
            Assert.Contains("Nessuna modifica necessaria", result.Message);
        }

        [Fact]
        public async Task UpdateAsync_WithNonExistingId_ShouldReturnNotFound()
        {
            // Arrange
            await SetupStatoPagamentoTestDataAsync();
            var dto = new StatoPagamentoDTO
            {
                StatoPagamentoId = 999,
                StatoPagamento1 = "inesistente"
            };

            // Act
            var result = await _repository.UpdateAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Contains("non trovato", result.Message);
        }

        [Fact]
        public async Task UpdateAsync_WithNullDto_ShouldReturnError()
        {
            // Arrange
            StatoPagamentoDTO nullDto = null!;

            // Act
            var result = await _repository.UpdateAsync(nullDto);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Equal("Errore interno durante l'aggiornamento dello stato pagamento", result.Message);
        }

        [Fact]
        public async Task UpdateAsync_WithEmptyNome_ShouldReturnError()
        {
            // Arrange
            await SetupStatoPagamentoTestDataAsync();
            var dto = new StatoPagamentoDTO
            {
                StatoPagamentoId = 1,
                StatoPagamento1 = ""
            };

            // Act
            var result = await _repository.UpdateAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Equal("Nome stato pagamento obbligatorio", result.Message);
        }

        [Fact]
        public async Task UpdateAsync_WithDuplicateNome_ShouldReturnError()
        {
            // Arrange
            await SetupStatoPagamentoTestDataAsync();
            // Stato 1 = "non richiesto", Stato 2 = "pendente"
            var dto = new StatoPagamentoDTO
            {
                StatoPagamentoId = 1, // non richiesto
                StatoPagamento1 = "pendente" // Già usato da stato 2
            };

            // Act
            var result = await _repository.UpdateAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Contains("Esiste già", result.Message);
        }

        [Fact]
        public async Task UpdateAsync_WithInvalidInput_ShouldReturnError()
        {
            // Arrange
            await SetupStatoPagamentoTestDataAsync();
            var dto = new StatoPagamentoDTO
            {
                StatoPagamentoId = 1,
                StatoPagamento1 = new string('a', 101) // Troppo lungo
            };

            // Act
            var result = await _repository.UpdateAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Contains("non valido", result.Message);
        }

        #endregion

        #region DeleteAsync Tests

        [Fact]
        public async Task DeleteAsync_WithValidId_ShouldDeleteStatoPagamento()
        {
            // Arrange
            await SetupStatoPagamentoTestDataAsync();
            int idToDelete = 1;

            // Act
            var result = await _repository.DeleteAsync(idToDelete);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.True(result.Data);

            // Verify deletion
            var deleted = await _context.StatoPagamento.FindAsync(idToDelete);
            Assert.Null(deleted);
        }

        [Fact]
        public async Task DeleteAsync_WithNonExistingId_ShouldReturnNotFound()
        {
            // Arrange
            await SetupStatoPagamentoTestDataAsync();
            int nonExistingId = 999;

            // Act
            var result = await _repository.DeleteAsync(nonExistingId);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Contains("non trovato", result.Message);
        }

        [Fact]
        public async Task DeleteAsync_WithZeroId_ShouldReturnError()
        {
            // Arrange
            int zeroId = 0;

            // Act
            var result = await _repository.DeleteAsync(zeroId);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Equal("ID stato pagamento non valido", result.Message);
        }

        [Fact]
        public async Task DeleteAsync_WithNegativeId_ShouldReturnError()
        {
            // Arrange
            int negativeId = -1;

            // Act
            var result = await _repository.DeleteAsync(negativeId);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Equal("ID stato pagamento non valido", result.Message);
        }

        [Fact]
        public async Task DeleteAsync_WithDependencies_ShouldReturnError()
        {
            // Arrange
            await SetupStatoPagamentoTestDataAsync();

            // Usa uno StatoOrdine esistente dal seed (il seed crea 8 stati ordine)
            // Ad esempio, lo stato ordine con ID 1 (bozza) esiste già
            var statoOrdineEsistente = await _context.StatoOrdine.FindAsync(1);
            // Assicurati che esista (dovrebbe dal seed)
            Assert.NotNull(statoOrdineEsistente);

            // Crea una dipendenza per lo stato pagamento 1 (non richiesto)
            var ordine = new Ordine
            {
                StatoPagamentoId = 1,
                StatoOrdineId = statoOrdineEsistente.StatoOrdineId, // Usa l'ID esistente
                Totale = 10.00m,
                DataCreazione = DateTime.UtcNow,
                DataAggiornamento = DateTime.UtcNow,
                ClienteId = 1,
                Priorita = 1,
                SessioneId = null
            };

            _context.Ordine.Add(ordine);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.DeleteAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Contains("dipendenze", result.Message);
        }

        #endregion

        #region ExistsAsync Tests

        [Fact]
        public async Task ExistsAsync_WithExistingId_ShouldReturnTrue()
        {
            // Arrange
            await SetupStatoPagamentoTestDataAsync();
            int existingId = 1;

            // Act
            var result = await _repository.ExistsAsync(existingId);

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
            await SetupStatoPagamentoTestDataAsync();
            int nonExistingId = 999;

            // Act
            var result = await _repository.ExistsAsync(nonExistingId);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.False(result.Data);
            Assert.Contains("non trovato", result.Message);
        }

        [Fact]
        public async Task ExistsAsync_WithZeroId_ShouldReturnError()
        {
            // Arrange
            int zeroId = 0;

            // Act
            var result = await _repository.ExistsAsync(zeroId);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Equal("ID stato pagamento non valido", result.Message);
        }

        #endregion

        #region ExistsByNomeAsync Tests

        [Fact]
        public async Task ExistsByNomeAsync_WithExistingNome_ShouldReturnTrue()
        {
            // Arrange
            await SetupStatoPagamentoTestDataAsync();
            string existingNome = "non richiesto";

            // Act
            var result = await _repository.ExistsByNomeAsync(existingNome);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.True(result.Data);
            Assert.Contains("esiste", result.Message);
        }

        [Fact]
        public async Task ExistsByNomeAsync_WithCaseInsensitiveNome_ShouldReturnTrue()
        {
            // Arrange
            await SetupStatoPagamentoTestDataAsync();

            // Act
            var result = await _repository.ExistsByNomeAsync("NON RICHIESTO");

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.True(result.Data);
        }

        [Fact]
        public async Task ExistsByNomeAsync_WithNonExistingNome_ShouldReturnFalse()
        {
            // Arrange
            await SetupStatoPagamentoTestDataAsync();
            string nonExistingNome = "inesistente";

            // Act
            var result = await _repository.ExistsByNomeAsync(nonExistingNome);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.False(result.Data);
            Assert.Contains("non trovato", result.Message);
        }

        [Fact]
        public async Task ExistsByNomeAsync_WithNullNome_ShouldReturnError()
        {
            // Arrange
            string? nullNome = null;

            // Act
            var result = await _repository.ExistsByNomeAsync(nullNome!);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Equal("Il nome dello stato pagamento è obbligatorio", result.Message);
        }

        [Fact]
        public async Task ExistsByNomeAsync_WithEmptyNome_ShouldReturnError()
        {
            // Arrange
            string emptyNome = "";

            // Act
            var result = await _repository.ExistsByNomeAsync(emptyNome);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Equal("Il nome dello stato pagamento è obbligatorio", result.Message);
        }

        [Fact]
        public async Task ExistsByNomeAsync_WithInvalidInput_ShouldReturnError()
        {
            // Arrange
            string invalidInput = new string('a', 101);

            // Act
            var result = await _repository.ExistsByNomeAsync(invalidInput);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Contains("non validi", result.Message);
        }

        #endregion

        #region Integration Tests

        [Fact]
        public async Task FullCRUD_Workflow_ShouldWorkCorrectly()
        {
            // Arrange - Clean slate
            await CleanTableAsync<StatoPagamento>();

            // 1. Add
            var addDto = new StatoPagamentoDTO
            {
                StatoPagamento1 = "test_workflow"
            };
            var addResult = await _repository.AddAsync(addDto);
            Assert.True(addResult.Success);
            Assert.NotNull(addResult.Data);
            int newId = addResult.Data.StatoPagamentoId;

            // 2. GetById to verify
            var getResult = await _repository.GetByIdAsync(newId);
            Assert.True(getResult.Success);
            Assert.NotNull(getResult.Data);
            Assert.Equal("test_workflow", getResult.Data.StatoPagamento1);

            // 3. Update
            var updateDto = new StatoPagamentoDTO
            {
                StatoPagamentoId = newId,
                StatoPagamento1 = "test_workflow_updated"
            };
            var updateResult = await _repository.UpdateAsync(updateDto);
            Assert.True(updateResult.Success);

            // 4. Verify update
            var verifyResult = await _repository.GetByIdAsync(newId);
            Assert.True(verifyResult.Success);
            Assert.NotNull(verifyResult.Data);
            Assert.Equal("test_workflow_updated", verifyResult.Data.StatoPagamento1);

            // 5. Delete
            var deleteResult = await _repository.DeleteAsync(newId);
            Assert.True(deleteResult.Success);

            // 6. Verify deletion
            var finalCheck = await _repository.GetByIdAsync(newId);
            Assert.False(finalCheck.Success);
        }

        [Fact]
        public async Task GetAllAsync_WithDifferentData_ShouldRespectComparerOrder()
        {
            // Arrange
            await CleanTableAsync<StatoPagamento>();
            // Inserisci in ordine diverso da quello del comparatore
            await CreateTestStatoPagamentoAsync("rimborsato");
            await CreateTestStatoPagamentoAsync("pendente");
            await CreateTestStatoPagamentoAsync("fallito");

            // Act
            var result = await _repository.GetAllAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, result.TotalCount);
            // Ordine secondo comparatore: pendente (2), fallito (4), rimborsato (5)
            var items = result.Data.ToList();
            Assert.Equal("pendente", items[0].StatoPagamento1);
            Assert.Equal("fallito", items[1].StatoPagamento1);
            Assert.Equal("rimborsato", items[2].StatoPagamento1);
        }

        #endregion

        #region Edge Cases

        [Fact]
        public async Task AddAsync_WithVeryLongButValidNome_ShouldWork()
        {
            // Arrange
            await CleanTableAsync<StatoPagamento>();
            var longName = new string('a', 100); // Lunghezza massima consentita
            var dto = new StatoPagamentoDTO
            {
                StatoPagamento1 = longName
            };

            // Act
            var result = await _repository.AddAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(longName, result.Data.StatoPagamento1);
        }

        [Fact]
        public async Task DeleteAsync_LastStatoPagamento_ShouldWork()
        {
            // Arrange
            await CleanTableAsync<StatoPagamento>();
            // Crea solo uno stato
            var stato = new StatoPagamento { StatoPagamento1 = "unico" };
            _context.StatoPagamento.Add(stato);
            await _context.SaveChangesAsync();
            int id = stato.StatoPagamentoId;

            // Act
            var result = await _repository.DeleteAsync(id);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Empty(_context.StatoPagamento);
        }

        [Fact]
        public async Task GetByNomeAsync_WithSpecialCharacters_ShouldNormalize()
        {
            // Arrange
            await CleanTableAsync<StatoPagamento>();
            var stato = new StatoPagamento
            {
                StatoPagamento1 = "stato-con_trattini",
            };
            _context.StatoPagamento.Add(stato);
            await _context.SaveChangesAsync();

            // Act - Cerca con spazi e maiuscole
            var result = await _repository.GetByNomeAsync("  STATO-CON_TRATTINI  ");

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal("stato-con_trattini", result.Data.StatoPagamento1);
        }

        #endregion
    }
}