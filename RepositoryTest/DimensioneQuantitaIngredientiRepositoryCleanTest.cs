using BBltZen;
using DTO;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Repository.Service;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace RepositoryTest
{
    public class DimensioneQuantitaIngredientiRepositoryCleanTest : BaseTestClean
    {
        private readonly DimensioneQuantitaIngredientiRepository _repository;

        public DimensioneQuantitaIngredientiRepositoryCleanTest()
        {
            _repository = new DimensioneQuantitaIngredientiRepository(_context,
                GetTestLogger<DimensioneQuantitaIngredientiRepository>());
        }

        #region GetAllAsync Tests

        [Fact]
        public async Task GetAllAsync_ShouldReturnPaginatedResults()
        {
            // Arrange
            var testItems = await CreateMultipleDimensioneQuantitaIngredientiAsync(3);

            // Act
            var result = await _repository.GetAllAsync(page: 1, pageSize: 10);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, result.TotalCount);
            Assert.Equal(1, result.Page);
            Assert.Equal(10, result.PageSize);
            Assert.NotEmpty(result.Data);
            Assert.Equal(3, result.Data.Count());
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnEmptyWhenNoData()
        {
            // Act
            var result = await _repository.GetAllAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(0, result.TotalCount);
            Assert.Empty(result.Data);
            Assert.Contains("Nessuna", result.Message);
        }

        [Fact]
        public async Task GetAllAsync_ShouldHandleInvalidPagination()
        {
            // Arrange
            await CreateMultipleDimensioneQuantitaIngredientiAsync(5);

            // Act - Pagina non valida (0 diventa 1 per sicurezza)
            var result = await _repository.GetAllAsync(page: 0, pageSize: -5);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(5, result.TotalCount);
            Assert.Equal(1, result.Page); // Correzione a 1
            Assert.Equal(1, result.PageSize); // Correzione a 1 (minimo)
        }

        #endregion

        #region GetByIdAsync Tests

        [Fact]
        public async Task GetByIdAsync_ShouldReturnItem()
        {
            // Arrange
            var testItem = await CreateTestDimensioneQuantitaIngredientiAsync(
                personalizzazioneIngredienteId: 1001,
                dimensioneBicchiereId: 1,
                moltiplicatore: 2.5m);

            // Act
            var result = await _repository.GetByIdAsync(testItem.DimensioneId);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(testItem.DimensioneId, result.Data.DimensioneId);
            Assert.Equal(testItem.Moltiplicatore, result.Data.Moltiplicatore);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnNotFoundForInvalidId()
        {
            // Act
            var result = await _repository.GetByIdAsync(-1);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Null(result.Data);
            Assert.Contains("non valido", result.Message);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnNotFoundForNonExistentId()
        {
            // Act
            var result = await _repository.GetByIdAsync(99999);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Null(result.Data);
            Assert.Contains("non trovata", result.Message);
        }

        #endregion

        #region GetByBicchiereIdAsync Tests

        [Fact]
        public async Task GetByBicchiereIdAsync_ShouldReturnFilteredResults()
        {
            // Arrange
            var bicchiereId = 1;
            await CreateTestDimensioneQuantitaIngredientiAsync(
                personalizzazioneIngredienteId: 1001,
                dimensioneBicchiereId: bicchiereId);
            await CreateTestDimensioneQuantitaIngredientiAsync(
                personalizzazioneIngredienteId: 1002,
                dimensioneBicchiereId: bicchiereId);
            // Altro bicchiere
            await CreateTestDimensioneQuantitaIngredientiAsync(
                personalizzazioneIngredienteId: 1003,
                dimensioneBicchiereId: 2);

            // Act
            var result = await _repository.GetByBicchiereIdAsync(bicchiereId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.TotalCount);
            Assert.All(result.Data, d => Assert.Equal(bicchiereId, d.DimensioneBicchiereId));
        }

        [Fact]
        public async Task GetByBicchiereIdAsync_ShouldReturnEmptyForInvalidBicchiereId()
        {
            // Act
            var result = await _repository.GetByBicchiereIdAsync(-1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(0, result.TotalCount);
            Assert.Empty(result.Data);
            Assert.Contains("non è valido", result.Message);
        }

        #endregion

        #region GetByPersonalizzazioneIngredienteIdAsync Tests

        [Fact]
        public async Task GetByPersonalizzazioneIngredienteIdAsync_ShouldReturnFilteredResults()
        {
            // Arrange
            var personalizzazioneId = 1001;
            await CreateTestDimensioneQuantitaIngredientiAsync(
                personalizzazioneIngredienteId: personalizzazioneId,
                dimensioneBicchiereId: 1);
            await CreateTestDimensioneQuantitaIngredientiAsync(
                personalizzazioneIngredienteId: personalizzazioneId,
                dimensioneBicchiereId: 2);
            // Altra personalizzazione
            await CreateTestDimensioneQuantitaIngredientiAsync(
                personalizzazioneIngredienteId: 1002,
                dimensioneBicchiereId: 1);

            // Act
            var result = await _repository.GetByPersonalizzazioneIngredienteIdAsync(personalizzazioneId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.TotalCount);
            Assert.All(result.Data, d => Assert.Equal(personalizzazioneId, d.PersonalizzazioneIngredienteId));
        }

        [Fact]
        public async Task GetByPersonalizzazioneIngredienteIdAsync_ShouldReturnEmptyForInvalidId()
        {
            // Act
            var result = await _repository.GetByPersonalizzazioneIngredienteIdAsync(-1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(0, result.TotalCount);
            Assert.Empty(result.Data);
            Assert.Contains("non è valido", result.Message);
        }

        #endregion

        #region GetByBicchiereDescrizioneAsync Tests

        [Fact]
        public async Task GetByBicchiereDescrizioneAsync_ShouldReturnFilteredResults()
        {
            // Arrange - Assumendo che il seed abbia "Medium" come descrizione per ID 1
            await CreateTestDimensioneQuantitaIngredientiAsync(
                personalizzazioneIngredienteId: 1001,
                dimensioneBicchiereId: 1); // Medium nel seed

            // Act - Cerca per parte della descrizione
            var result = await _repository.GetByBicchiereDescrizioneAsync("Med", page: 1, pageSize: 10);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.TotalCount >= 1);
            Assert.NotEmpty(result.Data);
        }

        [Fact]
        public async Task GetByBicchiereDescrizioneAsync_ShouldHandleInvalidInput()
        {
            // Act - Input potenzialmente pericoloso
            var result = await _repository.GetByBicchiereDescrizioneAsync("<script>alert('xss')</script>");

            // Assert
            Assert.NotNull(result);
            Assert.Equal(0, result.TotalCount);
            Assert.Empty(result.Data);
            Assert.Contains("contiene caratteri non validi", result.Message);
        }

        [Fact]
        public async Task GetByBicchiereDescrizioneAsync_ShouldHandleEmptySearch()
        {
            // Act
            var result = await _repository.GetByBicchiereDescrizioneAsync("   ");

            // Assert
            Assert.NotNull(result);
            Assert.Equal(0, result.TotalCount);
            Assert.Empty(result.Data);
            Assert.Contains("è obbligatorio", result.Message);
        }

        #endregion

        #region AddAsync Tests

        [Fact]
        public async Task AddAsync_ShouldCreateNewRecord()
        {
            // Arrange
            var dto = CreateTestDimensioneQuantitaIngredientiDTO(
                personalizzazioneIngredienteId: 1001,
                dimensioneBicchiereId: 1,
                moltiplicatore: 1.5m);

            // Act
            var result = await _repository.AddAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.NotEqual(0, result.Data.DimensioneId); // ID generato
            Assert.Equal(dto.PersonalizzazioneIngredienteId, result.Data.PersonalizzazioneIngredienteId);
            Assert.Equal(dto.DimensioneBicchiereId, result.Data.DimensioneBicchiereId);
            Assert.Equal(dto.Moltiplicatore, result.Data.Moltiplicatore);
            Assert.Contains("creata con successo", result.Message);
        }

        [Fact]
        public async Task AddAsync_ShouldFailWhenDuplicateCombination()
        {
            // Arrange
            await CreateTestDimensioneQuantitaIngredientiAsync(
                personalizzazioneIngredienteId: 1001,
                dimensioneBicchiereId: 1,
                moltiplicatore: 1.0m);

            var duplicateDto = CreateTestDimensioneQuantitaIngredientiDTO(
                personalizzazioneIngredienteId: 1001,
                dimensioneBicchiereId: 1,
                moltiplicatore: 2.0m); // Stessa combinazione, moltiplicatore diverso

            // Act
            var result = await _repository.AddAsync(duplicateDto);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Null(result.Data);
            Assert.Contains("Esiste già", result.Message);
        }

        [Fact]
        public async Task AddAsync_ShouldFailForInvalidMoltiplicatore()
        {
            // Arrange - Moltiplicatore fuori range
            var dto = CreateTestDimensioneQuantitaIngredientiDTO(
                personalizzazioneIngredienteId: 1001,
                dimensioneBicchiereId: 1,
                moltiplicatore: 15.0m); // > 10

            // Act
            var result = await _repository.AddAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Null(result.Data);
            Assert.Contains("Il moltiplicatore deve essere", result.Message);
        }

        [Fact]
        public async Task AddAsync_ShouldFailForZeroMoltiplicatore()
        {
            // Arrange
            var dto = CreateTestDimensioneQuantitaIngredientiDTO(
                personalizzazioneIngredienteId: 1001,
                dimensioneBicchiereId: 1,
                moltiplicatore: 0m); // <= 0

            // Act
            var result = await _repository.AddAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Null(result.Data);
            Assert.Contains("Il moltiplicatore deve essere", result.Message);
        }

        [Fact]
        public async Task AddAsync_ShouldFailForInvalidInput()
        {
            // Arrange
            var dto = CreateTestDimensioneQuantitaIngredientiDTO(
                personalizzazioneIngredienteId: 0, // Non valido
                dimensioneBicchiereId: 1,
                moltiplicatore: 1.5m);

            // Act
            var result = await _repository.AddAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Null(result.Data);
            Assert.Contains("obbligatorio", result.Message);
        }

        [Fact]
        public async Task AddAsync_ShouldFailForNullDto()
        {
            // Act
            var result = await _repository.AddAsync(null!);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Null(result.Data);
            Assert.Contains("Errore interno", result.Message);
        }

        #endregion

        #region UpdateAsync Tests

        [Fact]
        public async Task UpdateAsync_ShouldUpdateExistingRecord()
        {
            // Arrange
            var existing = await CreateTestDimensioneQuantitaIngredientiAsync(
                personalizzazioneIngredienteId: 1001,
                dimensioneBicchiereId: 1,
                moltiplicatore: 1.0m);

            var updateDto = CreateTestDimensioneQuantitaIngredientiDTO(
                personalizzazioneIngredienteId: 1001,
                dimensioneBicchiereId: 2, // Cambiato
                moltiplicatore: 2.5m);   // Cambiato
            updateDto.DimensioneId = existing.DimensioneId;

            // Act
            var result = await _repository.UpdateAsync(updateDto);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.True(result.Data); // bool true
            Assert.Contains("aggiornata con successo", result.Message);

            // Verifica aggiornamento nel DB
            var updated = await _context.DimensioneQuantitaIngredienti
                .FirstOrDefaultAsync(d => d.DimensioneId == existing.DimensioneId);
            Assert.NotNull(updated);
            Assert.Equal(2, updated.DimensioneBicchiereId);
            Assert.Equal(2.5m, updated.Moltiplicatore);
        }

        [Fact]
        public async Task UpdateAsync_ShouldFailForNonExistentId()
        {
            // Arrange
            var dto = CreateTestDimensioneQuantitaIngredientiDTO(
                personalizzazioneIngredienteId: 1001,
                dimensioneBicchiereId: 1,
                moltiplicatore: 1.5m);
            dto.DimensioneId = 99999; // ID inesistente

            // Act
            var result = await _repository.UpdateAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.False(result.Data); // bool false
            Assert.Contains("non trovata", result.Message);
        }

        [Fact]
        public async Task UpdateAsync_ShouldFailWhenCreatingDuplicate()
        {
            // Arrange
            // Record esistente con combinazione A
            await CreateTestDimensioneQuantitaIngredientiAsync(
                personalizzazioneIngredienteId: 1001,
                dimensioneBicchiereId: 1,
                moltiplicatore: 1.0m);

            // Record da aggiornare
            var toUpdate = await CreateTestDimensioneQuantitaIngredientiAsync(
                personalizzazioneIngredienteId: 1002,
                dimensioneBicchiereId: 2,
                moltiplicatore: 1.5m);

            // DTO che cerca di aggiornare il secondo record alla stessa combinazione del primo
            var updateDto = CreateTestDimensioneQuantitaIngredientiDTO(
                personalizzazioneIngredienteId: 1001, // Stesso del primo
                dimensioneBicchiereId: 1,             // Stesso del primo
                moltiplicatore: 2.0m);
            updateDto.DimensioneId = toUpdate.DimensioneId;

            // Act
            var result = await _repository.UpdateAsync(updateDto);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Contains("Esiste già", result.Message);
        }

        [Fact]
        public async Task UpdateAsync_ShouldReturnNoChangesWhenSameData()
        {
            // Arrange
            var existing = await CreateTestDimensioneQuantitaIngredientiAsync(
                personalizzazioneIngredienteId: 1001,
                dimensioneBicchiereId: 1,
                moltiplicatore: 1.5m);

            var updateDto = CreateTestDimensioneQuantitaIngredientiDTO(
                personalizzazioneIngredienteId: existing.PersonalizzazioneIngredienteId,
                dimensioneBicchiereId: existing.DimensioneBicchiereId,
                moltiplicatore: existing.Moltiplicatore);
            updateDto.DimensioneId = existing.DimensioneId;

            // Act
            var result = await _repository.UpdateAsync(updateDto);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.False(result.Data); // bool false perché nessuna modifica
            Assert.Contains("Nessuna modifica necessaria", result.Message);
        }

        #endregion

        #region DeleteAsync Tests

        [Fact]
        public async Task DeleteAsync_ShouldDeleteExistingRecord()
        {
            // Arrange
            var existing = await CreateTestDimensioneQuantitaIngredientiAsync(
                personalizzazioneIngredienteId: 1001,
                dimensioneBicchiereId: 1);

            // Act
            var result = await _repository.DeleteAsync(existing.DimensioneId);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.True(result.Data); // bool true
            Assert.Contains("eliminata con successo", result.Message);

            // Verifica che sia stato rimosso
            var deleted = await _context.DimensioneQuantitaIngredienti
                .FirstOrDefaultAsync(d => d.DimensioneId == existing.DimensioneId);
            Assert.Null(deleted);
        }

        [Fact]
        public async Task DeleteAsync_ShouldFailForInvalidId()
        {
            // Act
            var result = await _repository.DeleteAsync(-1);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.False(result.Data);
            Assert.Contains("non valido", result.Message);
        }

        [Fact]
        public async Task DeleteAsync_ShouldFailForNonExistentId()
        {
            // Act
            var result = await _repository.DeleteAsync(99999);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.False(result.Data);
            Assert.Contains("non trovata", result.Message);
        }

        #endregion

        #region ExistsAsync Tests

        [Fact]
        public async Task ExistsAsync_ShouldReturnTrueForExistingRecord()
        {
            // Arrange
            var existing = await CreateTestDimensioneQuantitaIngredientiAsync();

            // Act
            var result = await _repository.ExistsAsync(existing.DimensioneId);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.True(result.Data); // bool true
            Assert.Contains("esiste", result.Message);
        }

        [Fact]
        public async Task ExistsAsync_ShouldReturnFalseForNonExistentRecord()
        {
            // Act
            var result = await _repository.ExistsAsync(99999);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.False(result.Data); // bool false
            Assert.Contains("non trovata", result.Message);
        }

        [Fact]
        public async Task ExistsAsync_ShouldFailForInvalidId()
        {
            // Act
            var result = await _repository.ExistsAsync(-1);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.False(result.Data);
            Assert.Contains("non valido", result.Message);
        }

        #endregion

        #region ExistsByCombinazioneAsync Tests

        [Fact]
        public async Task ExistsByCombinazioneAsync_ShouldReturnTrueForExistingCombination()
        {
            // Arrange
            await CreateTestDimensioneQuantitaIngredientiAsync(
                personalizzazioneIngredienteId: 1001,
                dimensioneBicchiereId: 1);

            // Act
            var result = await _repository.ExistsByCombinazioneAsync(1001, 1);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.True(result.Data); // bool true
            Assert.Contains("esiste", result.Message);
        }

        [Fact]
        public async Task ExistsByCombinazioneAsync_ShouldReturnFalseForNonExistentCombination()
        {
            // Act
            var result = await _repository.ExistsByCombinazioneAsync(99999, 99999);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.False(result.Data); // bool false
            Assert.Contains("non trovata", result.Message);
        }

        [Fact]
        public async Task ExistsByCombinazioneAsync_ShouldFailForInvalidIds()
        {
            // Act
            var result = await _repository.ExistsByCombinazioneAsync(-1, -1);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.False(result.Data);
            Assert.Contains("non valido", result.Message);
        }

        #endregion

        #region CountAsync Tests

        [Fact]
        public async Task CountAsync_ShouldReturnCorrectCount()
        {
            // Arrange
            await CreateMultipleDimensioneQuantitaIngredientiAsync(3);

            // Act
            var result = await _repository.CountAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(3, result.Data);
            Assert.Contains("3", result.Message);
        }

        [Fact]
        public async Task CountAsync_ShouldReturnZeroWhenEmpty()
        {
            // Act
            var result = await _repository.CountAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(0, result.Data);
            Assert.Contains("Nessuna", result.Message);
        }

        #endregion

        #region GetCountByPersonalizzazioneIngredientiAsync Tests

        [Fact]
        public async Task GetCountByPersonalizzazioneIngredientiAsync_ShouldReturnCorrectCount()
        {
            // Arrange
            var personalizzazioneId = 1001;
            await CreateTestDimensioneQuantitaIngredientiAsync(
                personalizzazioneIngredienteId: personalizzazioneId,
                dimensioneBicchiereId: 1);
            await CreateTestDimensioneQuantitaIngredientiAsync(
                personalizzazioneIngredienteId: personalizzazioneId,
                dimensioneBicchiereId: 2);

            // Altra personalizzazione (non dovrebbe essere contata)
            await CreateTestDimensioneQuantitaIngredientiAsync(
                personalizzazioneIngredienteId: 1002,
                dimensioneBicchiereId: 1);

            // Act
            var result = await _repository.GetCountByPersonalizzazioneIngredientiAsync(personalizzazioneId);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(2, result.Data);
            Assert.Contains("2", result.Message);
        }

        [Fact]
        public async Task GetCountByPersonalizzazioneIngredientiAsync_ShouldReturnZeroForNonExistent()
        {
            // Act
            var result = await _repository.GetCountByPersonalizzazioneIngredientiAsync(99999);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(0, result.Data);
            Assert.Contains("Nessun bicchiere", result.Message);
        }

        [Fact]
        public async Task GetCountByPersonalizzazioneIngredientiAsync_ShouldFailForInvalidId()
        {
            // Act
            var result = await _repository.GetCountByPersonalizzazioneIngredientiAsync(-1);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Equal(0, result.Data);
            Assert.Contains("non valido", result.Message);
        }

        #endregion        
    }
}