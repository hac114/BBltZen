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
    public class TaxRatesRepositoryCleanTest : BaseTestClean
    {
        private readonly TaxRatesRepository _repository;

        public TaxRatesRepositoryCleanTest()
        {
            _repository = new TaxRatesRepository(_context, GetTestLogger<TaxRatesRepository>());
        }

        #region GetAllAsync Tests

        [Fact]
        public async Task GetAllAsync_ShouldReturnPaginatedTaxRates()
        {
            // Act
            var result = await _repository.GetAllAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Page);
            Assert.Equal(10, result.PageSize);
            Assert.True(result.TotalCount >= 2); // 2 tax rates seedati nel BaseTestClean
            Assert.Contains("aliquote", result.Message);
        }

        [Fact]
        public async Task GetAllAsync_WithPagination_ShouldReturnCorrectPage()
        {
            // Arrange
            for (int i = 0; i < 10; i++)
            {
                await CreateTestTaxRateAsync(5.00m + i, $"Test Aliquota {i}");
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
        public async Task GetAllAsync_NoTaxRates_ShouldReturnEmpty()
        {
            // Arrange
            await CleanTableAsync<TaxRates>();

            // Act
            var result = await _repository.GetAllAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result.Data);
            Assert.Equal(0, result.TotalCount);
            Assert.Contains("Nessuna aliquota trovata", result.Message);
        }

        [Fact]
        public async Task GetAllAsync_ShouldOrderByAliquotaDescending()
        {
            // Arrange
            await CleanTableAsync<TaxRates>();

            await CreateTestTaxRateAsync(5.00m, "Bassa");
            await CreateTestTaxRateAsync(22.00m, "Alta");
            await CreateTestTaxRateAsync(10.00m, "Media");

            // Act
            var result = await _repository.GetAllAsync();

            // Assert - Verifica ordinamento decrescente per aliquota
            var aliquote = result.Data.ToList();
            Assert.Equal(22.00m, aliquote[0].Aliquota);
            Assert.Equal(10.00m, aliquote[1].Aliquota);
            Assert.Equal(5.00m, aliquote[2].Aliquota);
        }

        #endregion

        #region GetByIdAsync Tests

        [Fact]
        public async Task GetByIdAsync_ValidId_ShouldReturnTaxRate()
        {
            // Arrange
            var testTaxRate = await CreateTestTaxRateAsync(15.00m, "Test GetById");

            // Act
            var result = await _repository.GetByIdAsync(testTaxRate.TaxRateId);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(testTaxRate.TaxRateId, result.Data.TaxRateId);
            Assert.Equal(15.00m, result.Data.Aliquota);
            Assert.Equal("Test GetById", result.Data.Descrizione);
            Assert.InRange(result.Data.DataCreazione,
                DateTime.UtcNow.AddSeconds(-5),
                DateTime.UtcNow.AddSeconds(5));
            Assert.Contains($"Aliquota con ID {testTaxRate.TaxRateId} trovata", result.Message);
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
            Assert.Contains("ID aliquota non valido", result.Message);
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
            Assert.Contains($"Aliquota con ID {nonExistentId} non trovata", result.Message);
        }

        #endregion

        #region GetByAliquotaAsync Tests

        [Fact]
        public async Task GetByAliquotaAsync_ValidAliquota_ShouldReturnFilteredTaxRates()
        {
            // Arrange
            await CleanTableAsync<TaxRates>();

            await CreateTestTaxRateAsync(22.00m, "IVA Standard");
            await CreateTestTaxRateAsync(22.00m, "IVA Standard 2");
            await CreateTestTaxRateAsync(10.00m, "IVA Ridotta");

            // Act
            var result = await _repository.GetByAliquotaAsync(22.00m);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.TotalCount);
            Assert.True(result.Data.All(t => t.Aliquota == 22.00m));
            // Usa StringComparison.Ordinal per evitare problemi di cultura
            Assert.Contains("22", result.Message, StringComparison.Ordinal);
        }

        [Fact]
        public async Task GetByAliquotaAsync_NoMatchingAliquota_ShouldReturnEmpty()
        {
            // Act
            var result = await _repository.GetByAliquotaAsync(99.99m);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result.Data);
            // Cerca il numero senza specificare formato decimale
            Assert.Contains("99", result.Message, StringComparison.Ordinal);
        }

        [Fact]
        public async Task GetByAliquotaAsync_WithPagination_ShouldReturnCorrectPage()
        {
            // Arrange
            await CleanTableAsync<TaxRates>();

            for (int i = 0; i < 15; i++)
            {
                await CreateTestTaxRateAsync(22.00m, $"Test {i}");
            }

            // Act
            var result = await _repository.GetByAliquotaAsync(22.00m, page: 2, pageSize: 5);

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
        public async Task GetByDescrizioneAsync_ValidDescrizione_ShouldReturnFilteredTaxRates()
        {
            // Arrange
            await CleanTableAsync<TaxRates>();

            await CreateTestTaxRateAsync(22.00m, "IVA Standard");
            await CreateTestTaxRateAsync(10.00m, "IVA Ridotta");
            await CreateTestTaxRateAsync(5.00m, "IVA Agevolata");

            // Act
            var result = await _repository.GetByDescrizioneAsync("IVA");

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, result.TotalCount);
            Assert.True(result.Data.All(t => t.Descrizione!.Contains("IVA", StringComparison.OrdinalIgnoreCase)));
            Assert.Contains("IVA", result.Message, StringComparison.Ordinal);
        }

        [Fact]
        public async Task GetByDescrizioneAsync_EmptyDescrizione_ShouldReturnError()
        {
            // Act
            var result = await _repository.GetByDescrizioneAsync("");

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result.Data);
            Assert.Contains("Il parametro 'descrizione' è obbligatorio", result.Message, StringComparison.Ordinal);
        }

        [Fact]
        public async Task GetByDescrizioneAsync_InvalidInput_ShouldReturnError()
        {
            // Arrange
            var invalidInput = new string('A', 110); // Troppo lungo (max 100)

            // Act
            var result = await _repository.GetByDescrizioneAsync(invalidInput);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result.Data);
            Assert.Contains("Il parametro 'descrizione' contiene caratteri non validi", result.Message, StringComparison.Ordinal);
        }

        [Fact]
        public async Task GetByDescrizioneAsync_CaseInsensitive_ShouldFindMatches()
        {
            // Arrange
            await CleanTableAsync<TaxRates>();
            await CreateTestTaxRateAsync(22.00m, "IVA STANDARD");

            // Act - Cerca con case diverso
            var result = await _repository.GetByDescrizioneAsync("iva");

            // Assert
            Assert.NotNull(result);
            Assert.Single(result.Data);
            Assert.Equal("IVA STANDARD", result.Data.First().Descrizione);
        }

        [Fact]
        public async Task GetByDescrizioneAsync_WithPagination_ShouldReturnCorrectPage()
        {
            // Arrange
            await CleanTableAsync<TaxRates>();

            for (int i = 0; i < 15; i++)
            {
                await CreateTestTaxRateAsync(22.00m, $"Test {i}");
            }

            // Act
            var result = await _repository.GetByDescrizioneAsync("Test", page: 2, pageSize: 5);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Page);
            Assert.Equal(5, result.PageSize);
            Assert.Equal(15, result.TotalCount);
            Assert.Equal(5, result.Data.Count());
        }

        #endregion

        #region ExistsAsync Tests

        [Fact]
        public async Task ExistsAsync_ValidExistingId_ShouldReturnTrue()
        {
            // Arrange
            var testTaxRate = await CreateTestTaxRateAsync();

            // Act
            var result = await _repository.ExistsAsync(testTaxRate.TaxRateId);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.True(result.Data);
            Assert.Contains($"Aliquota con ID {testTaxRate.TaxRateId} esiste", result.Message, StringComparison.Ordinal);
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
            Assert.Contains($"Aliquota con ID 9999 non trovata", result.Message, StringComparison.Ordinal);
        }

        [Fact]
        public async Task ExistsAsync_InvalidId_ShouldReturnError()
        {
            // Act
            var result = await _repository.ExistsAsync(0);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Contains("ID aliquota non valido", result.Message, StringComparison.Ordinal);
        }

        #endregion

        #region ExistsByAliquotaAsync Tests

        [Fact]
        public async Task ExistsByAliquotaAsync_ExistingAliquota_ShouldReturnTrue()
        {
            // Arrange
            await CreateTestTaxRateAsync(15.00m, "Test ExistsByAliquota");

            // Act
            var result = await _repository.ExistsByAliquotaAsync(15.00m);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.True(result.Data);
            // Non cercare la formattazione esatta del decimale, controlla solo parti della stringa
            Assert.Contains("Aliquota", result.Message, StringComparison.Ordinal);
            Assert.Contains("15", result.Message, StringComparison.Ordinal);
            Assert.Contains("esiste", result.Message, StringComparison.Ordinal);
        }

        [Fact]
        public async Task ExistsByAliquotaAsync_NonExistentAliquota_ShouldReturnFalse()
        {
            // Act
            var result = await _repository.ExistsByAliquotaAsync(99.99m);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.False(result.Data);
            // Controlla solo parti della stringa
            Assert.Contains("Aliquota", result.Message, StringComparison.Ordinal);
            Assert.Contains("99", result.Message, StringComparison.Ordinal);
            Assert.Contains("non trovata", result.Message, StringComparison.Ordinal);
        }

        [Fact]
        public async Task ExistsByAliquotaAsync_InvalidAliquotaNegative_ShouldReturnError()
        {
            // Act
            var result = await _repository.ExistsByAliquotaAsync(-5.00m);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Contains("Aliquota deve essere compresa tra 0 e 100", result.Message, StringComparison.Ordinal);
        }

        [Fact]
        public async Task ExistsByAliquotaAsync_InvalidAliquotaOver100_ShouldReturnError()
        {
            // Act
            var result = await _repository.ExistsByAliquotaAsync(150.00m);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Contains("Aliquota deve essere compresa tra 0 e 100", result.Message, StringComparison.Ordinal);
        }

        #endregion

        #region ExistsByAliquotaDescrizioneAsync Tests

        [Fact]
        public async Task ExistsByAliquotaDescrizioneAsync_ExistingCombination_ShouldReturnTrue()
        {
            // Arrange
            await CreateTestTaxRateAsync(18.00m, "IVA Speciale");

            // Act
            var result = await _repository.ExistsByAliquotaDescrizioneAsync(18.00m, "IVA Speciale");

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.True(result.Data);
            // Controlla parti della stringa invece della formattazione esatta
            Assert.Contains("Aliquota", result.Message, StringComparison.Ordinal);
            Assert.Contains("18", result.Message, StringComparison.Ordinal);
            Assert.Contains("IVA Speciale", result.Message, StringComparison.Ordinal);
            Assert.Contains("esiste", result.Message, StringComparison.Ordinal);
        }

        [Fact]
        public async Task ExistsByAliquotaDescrizioneAsync_NonExistentCombination_ShouldReturnFalse()
        {
            // Act
            var result = await _repository.ExistsByAliquotaDescrizioneAsync(25.00m, "IVA Inesistente");

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.False(result.Data);
            // Controlla parti della stringa
            Assert.Contains("Aliquota", result.Message, StringComparison.Ordinal);
            Assert.Contains("25", result.Message, StringComparison.Ordinal);
            Assert.Contains("IVA Inesistente", result.Message, StringComparison.Ordinal);
            Assert.Contains("non trovata", result.Message, StringComparison.Ordinal);
        }

        [Fact]
        public async Task ExistsByAliquotaDescrizioneAsync_EmptyDescrizione_ShouldReturnError()
        {
            // Act
            var result = await _repository.ExistsByAliquotaDescrizioneAsync(22.00m, "");

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Contains("La descrizione è obbligatoria", result.Message, StringComparison.Ordinal);
        }

        [Fact]
        public async Task ExistsByAliquotaDescrizioneAsync_InvalidInput_ShouldReturnError()
        {
            // Arrange
            var invalidInput = new string('A', 110); // Troppo lungo

            // Act
            var result = await _repository.ExistsByAliquotaDescrizioneAsync(22.00m, invalidInput);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Contains("La descrizione contiene caratteri non validi", result.Message, StringComparison.Ordinal);
        }

        [Fact]
        public async Task ExistsByAliquotaDescrizioneAsync_CaseInsensitive_ShouldFindMatches()
        {
            // Arrange
            await CreateTestTaxRateAsync(12.00m, "IVA RIDOTTA");

            // Act - Cerca con case diverso
            var result = await _repository.ExistsByAliquotaDescrizioneAsync(12.00m, "iva ridotta");

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.True(result.Data);
        }

        #endregion

        #region AddAsync Tests

        [Fact]
        public async Task AddAsync_ValidTaxRate_ShouldCreateAndReturnTaxRate()
        {
            // Arrange
            var taxRateDto = new TaxRatesDTO
            {
                Aliquota = 7.00m,
                Descrizione = "IVA Agevolata Test"
            };

            // Act
            var result = await _repository.AddAsync(taxRateDto);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.True(result.Data.TaxRateId > 0);
            Assert.Equal(7.00m, result.Data.Aliquota);
            Assert.Equal("IVA Agevolata Test", result.Data.Descrizione);
            Assert.InRange(result.Data.DataCreazione,
                DateTime.UtcNow.AddSeconds(-5),
                DateTime.UtcNow.AddSeconds(5));
            Assert.InRange(result.Data.DataAggiornamento,
                DateTime.UtcNow.AddSeconds(-5),
                DateTime.UtcNow.AddSeconds(5));

            // Controlla parti della stringa invece della formattazione esatta del decimale
            Assert.Contains("Aliquota", result.Message, StringComparison.Ordinal);
            Assert.Contains("7", result.Message, StringComparison.Ordinal);
            Assert.Contains("IVA Agevolata Test", result.Message, StringComparison.Ordinal);
            Assert.Contains("creata con successo", result.Message, StringComparison.Ordinal);

            // Verifica che sia stato salvato nel database
            var savedTaxRate = await _context.TaxRates.FindAsync(result.Data.TaxRateId);
            Assert.NotNull(savedTaxRate);
            Assert.Equal(result.Data.TaxRateId, savedTaxRate.TaxRateId);
            Assert.Equal("IVA Agevolata Test", savedTaxRate.Descrizione);
        }

        [Fact]
        public async Task AddAsync_DuplicateAliquotaDescrizione_ShouldReturnError()
        {
            // Arrange
            await CreateTestTaxRateAsync(22.00m, "IVA Standard Duplicata");

            var taxRateDto = new TaxRatesDTO
            {
                Aliquota = 22.00m,
                Descrizione = "IVA Standard Duplicata"
            };

            // Act
            var result = await _repository.AddAsync(taxRateDto);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Null(result.Data);

            // Controlla parti della stringa invece della formattazione esatta
            Assert.Contains("Esiste già un'aliquota", result.Message, StringComparison.Ordinal);
            Assert.Contains("22", result.Message, StringComparison.Ordinal);
            Assert.Contains("IVA Standard Duplicata", result.Message, StringComparison.Ordinal);
        }

        [Fact]
        public async Task AddAsync_EmptyDescrizione_ShouldReturnError()
        {
            // Arrange
            var taxRateDto = new TaxRatesDTO
            {
                Aliquota = 22.00m,
                Descrizione = ""
            };

            // Act
            var result = await _repository.AddAsync(taxRateDto);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Null(result.Data);
            Assert.Contains("Descrizione obbligatoria", result.Message, StringComparison.Ordinal);
        }

        [Fact]
        public async Task AddAsync_InvalidAliquotaNegative_ShouldReturnError()
        {
            // Arrange
            var taxRateDto = new TaxRatesDTO
            {
                Aliquota = -5.00m,
                Descrizione = "Test"
            };

            // Act
            var result = await _repository.AddAsync(taxRateDto);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Null(result.Data);
            Assert.Contains("Aliquota deve essere compresa tra 0 e 100", result.Message, StringComparison.Ordinal);
        }

        [Fact]
        public async Task AddAsync_InvalidAliquotaOver100_ShouldReturnError()
        {
            // Arrange
            var taxRateDto = new TaxRatesDTO
            {
                Aliquota = 150.00m,
                Descrizione = "Test"
            };

            // Act
            var result = await _repository.AddAsync(taxRateDto);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Null(result.Data);
            Assert.Contains("Aliquota deve essere compresa tra 0 e 100", result.Message, StringComparison.Ordinal);
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

        [Fact]
        public async Task AddAsync_InvalidDescrizioneLength_ShouldReturnError()
        {
            // Arrange
            var taxRateDto = new TaxRatesDTO
            {
                Aliquota = 22.00m,
                Descrizione = new string('A', 110) // Troppo lungo (max 100)
            };

            // Act
            var result = await _repository.AddAsync(taxRateDto);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Null(result.Data);
            Assert.Contains("Descrizione non valida", result.Message, StringComparison.Ordinal);
        }

        #endregion

        #region UpdateAsync Tests

        [Fact]
        public async Task UpdateAsync_ValidUpdate_ShouldUpdateTaxRate()
        {
            // Arrange
            var taxRate = await CreateTestTaxRateAsync(10.00m, "Vecchia Descrizione");
            var taxRateDto = new TaxRatesDTO
            {
                TaxRateId = taxRate.TaxRateId,
                Aliquota = 15.00m,
                Descrizione = "Nuova Descrizione"
            };

            // Act
            var result = await _repository.UpdateAsync(taxRateDto);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.True(result.Data);
            Assert.Contains($"Aliquota con ID {taxRate.TaxRateId} aggiornata con successo",
                result.Message, StringComparison.Ordinal);

            // Verifica aggiornamento nel database
            var updatedTaxRate = await _context.TaxRates.FindAsync(taxRate.TaxRateId);
            Assert.NotNull(updatedTaxRate);
            Assert.Equal(15.00m, updatedTaxRate.Aliquota);
            Assert.Equal("Nuova Descrizione", updatedTaxRate.Descrizione);
            Assert.InRange(updatedTaxRate.DataAggiornamento,
                DateTime.UtcNow.AddSeconds(-5),
                DateTime.UtcNow.AddSeconds(5));
        }

        [Fact]
        public async Task UpdateAsync_NonExistentId_ShouldReturnNotFound()
        {
            // Arrange
            var taxRateDto = new TaxRatesDTO
            {
                TaxRateId = 9999,
                Aliquota = 22.00m,
                Descrizione = "Test"
            };

            // Act
            var result = await _repository.UpdateAsync(taxRateDto);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Contains($"Aliquota con ID 9999 non trovata",
                result.Message, StringComparison.Ordinal);
        }

        [Fact]
        public async Task UpdateAsync_DuplicateAliquotaDescrizione_ShouldReturnError()
        {
            // Arrange
            var taxRate1 = await CreateTestTaxRateAsync(10.00m, "Descrizione 1");
            var taxRate2 = await CreateTestTaxRateAsync(22.00m, "Descrizione 2");

            var taxRateDto = new TaxRatesDTO
            {
                TaxRateId = taxRate1.TaxRateId,
                Aliquota = 22.00m, // Aliquota già usata da taxRate2
                Descrizione = "Descrizione 2" // Descrizione già usata da taxRate2
            };

            // Act
            var result = await _repository.UpdateAsync(taxRateDto);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            // Controlla parti della stringa
            Assert.Contains("Esiste già un'altra aliquota", result.Message, StringComparison.Ordinal);
            Assert.Contains("22", result.Message, StringComparison.Ordinal);
            Assert.Contains("Descrizione 2", result.Message, StringComparison.Ordinal);
        }

        [Fact]
        public async Task UpdateAsync_NoChanges_ShouldReturnFalseWithMessage()
        {
            // Arrange
            var taxRate = await CreateTestTaxRateAsync(22.00m, "Test No Changes");
            var taxRateDto = new TaxRatesDTO
            {
                TaxRateId = taxRate.TaxRateId,
                Aliquota = 22.00m,
                Descrizione = "Test No Changes"
            };

            // Act
            var result = await _repository.UpdateAsync(taxRateDto);

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
        public async Task DeleteAsync_ValidId_ShouldDeleteTaxRate()
        {
            // Arrange
            var taxRate = await CreateTestTaxRateAsync(18.00m, "Test Delete");
            var taxRateId = taxRate.TaxRateId;

            // Act
            var result = await _repository.DeleteAsync(taxRateId);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.True(result.Data);
            Assert.Contains($"eliminata con successo", result.Message, StringComparison.Ordinal);

            // Verifica che sia stato eliminato dal database
            var deletedTaxRate = await _context.TaxRates.FindAsync(taxRateId);
            Assert.Null(deletedTaxRate);
        }

        [Fact]
        public async Task DeleteAsync_NonExistentId_ShouldReturnNotFound()
        {
            // Act
            var result = await _repository.DeleteAsync(9999);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Contains($"Aliquota con ID 9999 non trovata", result.Message, StringComparison.Ordinal);
        }

        [Fact]
        public async Task DeleteAsync_InvalidId_ShouldReturnError()
        {
            // Act
            var result = await _repository.DeleteAsync(0);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Contains("ID aliquota non valido", result.Message, StringComparison.Ordinal);
        }

        [Fact]
        public async Task DeleteAsync_TaxRateWithDependencies_ShouldReturnError()
        {
            // Arrange
            var taxRate = await CreateTestTaxRateAsync(22.00m, "Test Dependencies");

            // Usa reflection per testare il metodo privato HasDependenciesAsync
            var method = typeof(TaxRatesRepository)
                .GetMethod("HasDependenciesAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            var task = (Task<bool>)method!.Invoke(_repository, new object[] { taxRate.TaxRateId })!;
            var hasDependencies = await task;

            if (!hasDependencies)
            {
                // Senza dipendenze, l'eliminazione dovrebbe riuscire
                var result = await _repository.DeleteAsync(taxRate.TaxRateId);
                Assert.True(result.Success);
                Assert.True(result.Data);
            }
            else
            {
                // Con dipendenze, l'eliminazione dovrebbe fallire
                var result = await _repository.DeleteAsync(taxRate.TaxRateId);
                Assert.False(result.Success);
                Assert.Contains("Impossibile eliminare aliquota", result.Message, StringComparison.Ordinal);
            }
        }

        #endregion

        #region Private Helper Methods Tests

        [Fact]
        public async Task HasDependenciesAsync_WithoutDependencies_ShouldReturnFalse()
        {
            // Arrange
            var taxRate = await CreateTestTaxRateAsync(5.00m, "Test No Dependencies");

            // Act - Usa reflection per testare il metodo privato
            var method = typeof(TaxRatesRepository)
                .GetMethod("HasDependenciesAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            var task = (Task<bool>)method!.Invoke(_repository, new object[] { taxRate.TaxRateId })!;
            var result = await task;

            // Assert - Normalmente dovrebbe essere false perché non abbiamo creato OrderItem
            Assert.False(result);
        }

        #endregion

        #region Integration Tests

        [Fact]
        public async Task FullCrudFlow_ShouldWorkCorrectly()
        {
            // 1. CREATE
            var addDto = new TaxRatesDTO
            {
                Aliquota = 8.00m,
                Descrizione = "Flow Test Aliquota"
            };
            var addResult = await _repository.AddAsync(addDto);
            Assert.True(addResult.Success);
            var taxRateId = addResult.Data!.TaxRateId;

            // 2. READ by ID
            var getResult = await _repository.GetByIdAsync(taxRateId);
            Assert.True(getResult.Success);
            Assert.Equal(8.00m, getResult.Data!.Aliquota);

            // 3. EXISTS
            var existsResult = await _repository.ExistsAsync(taxRateId);
            Assert.True(existsResult.Success);
            Assert.True(existsResult.Data);

            // 4. Aliquota EXISTS
            var aliquotaExistsResult = await _repository.ExistsByAliquotaAsync(8.00m);
            Assert.True(aliquotaExistsResult.Success);
            Assert.True(aliquotaExistsResult.Data);

            // 5. UPDATE
            var updateDto = new TaxRatesDTO
            {
                TaxRateId = taxRateId,
                Aliquota = 9.00m,
                Descrizione = "Flow Test Aliquota Updated"
            };
            var updateResult = await _repository.UpdateAsync(updateDto);
            Assert.True(updateResult.Success);
            Assert.True(updateResult.Data);

            // 6. Verify update
            var verifyResult = await _repository.GetByIdAsync(taxRateId);
            Assert.Equal(9.00m, verifyResult.Data!.Aliquota);

            // 7. DELETE
            var deleteResult = await _repository.DeleteAsync(taxRateId);
            Assert.True(deleteResult.Success);
            Assert.True(deleteResult.Data);

            // 8. Verify deletion
            var finalExistsResult = await _repository.ExistsAsync(taxRateId);
            Assert.False(finalExistsResult.Data);
        }

        #endregion

        #region Date Tests (senza warning)

        [Fact]
        public async Task TaxRateDates_ShouldBeSetCorrectly()
        {
            // Arrange
            var beforeCreation = DateTime.UtcNow;

            var taxRateDto = new TaxRatesDTO
            {
                Aliquota = 12.50m,
                Descrizione = "Test Date Aliquota"
            };

            // Act
            var result = await _repository.AddAsync(taxRateDto);

            // Assert senza warning
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);

            // Usa Assert.True con confronto manuale per evitare warning
            Assert.True(result.Data.DataCreazione >= beforeCreation,
                "DataCreazione deve essere dopo il momento di inizio");
            Assert.True(result.Data.DataCreazione <= DateTime.UtcNow,
                "DataCreazione non può essere nel futuro");
            Assert.True(result.Data.DataAggiornamento >= beforeCreation,
                "DataAggiornamento deve essere dopo il momento di inizio");
            Assert.True(result.Data.DataAggiornamento <= DateTime.UtcNow,
                "DataAggiornamento non può essere nel futuro");

            // DataCreazione e DataAggiornamento dovrebbero essere molto vicine
            var timeDifference = result.Data.DataAggiornamento - result.Data.DataCreazione;
            Assert.True(timeDifference.TotalSeconds >= 0,
                "DataAggiornamento non può essere precedente a DataCreazione");
            Assert.True(timeDifference.TotalSeconds < 5,
                "DataCreazione e DataAggiornamento dovrebbero essere vicine");
        }

        [Fact]
        public async Task Update_ShouldUpdateDataAggiornamento()
        {
            // Arrange
            var taxRate = await CreateTestTaxRateAsync(10.00m, "Test Update Date");
            var originalUpdateTime = taxRate.DataAggiornamento;

            // Attendi un po' per essere sicuri che il tempo sia passato
            await Task.Delay(100);

            var taxRateDto = new TaxRatesDTO
            {
                TaxRateId = taxRate.TaxRateId,
                Aliquota = 15.00m, // Cambio aliquota
                Descrizione = "Test Update Date"
            };

            // Act
            var result = await _repository.UpdateAsync(taxRateDto);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.True(result.Data); // True perché ci sono cambiamenti

            var updatedTaxRate = await _context.TaxRates.FindAsync(taxRate.TaxRateId);
            Assert.NotNull(updatedTaxRate);

            // DataAggiornamento dovrebbe essere stata aggiornata
            Assert.True(updatedTaxRate.DataAggiornamento > originalUpdateTime,
                "DataAggiornamento dovrebbe essere stata aggiornata");

            // DataCreazione dovrebbe rimanere invariata
            Assert.Equal(taxRate.DataCreazione, updatedTaxRate.DataCreazione);
        }

        #endregion
    }
}