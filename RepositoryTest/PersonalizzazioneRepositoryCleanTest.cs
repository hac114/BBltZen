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
    public class PersonalizzazioneRepositoryCleanTest : BaseTestClean
    {
        private readonly PersonalizzazioneRepository _repository;

        public PersonalizzazioneRepositoryCleanTest()
        {
            _repository = new PersonalizzazioneRepository(_context,
                GetTestLogger<PersonalizzazioneRepository>());
        }

        #region GetAllAsync Tests

        [Fact]
        public async Task GetAllAsync_ShouldReturnPaginatedResults()
        {
            // Arrange
            // Le personalizzazioni sono già seedate nel BaseTestClean (5 record)

            // Act
            var result = await _repository.GetAllAsync(page: 1, pageSize: 3);

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.Data);
            Assert.Equal(5, result.TotalCount);
            Assert.Equal(3, result.Data.Count());
            Assert.Equal(1, result.Page);
            Assert.Equal(3, result.PageSize);
            Assert.Contains("Trovate 5 personalizzazioni", result.Message);
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnEmpty_WhenNoPersonalizzazioni()
        {
            // Arrange
            // Rimuovi tutte le personalizzazioni
            var allPersonalizzazioni = await _context.Personalizzazione.ToListAsync();
            _context.Personalizzazione.RemoveRange(allPersonalizzazioni);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetAllAsync();

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.Data);
            Assert.Equal(0, result.TotalCount);
            Assert.Empty(result.Data);
            Assert.Contains("Nessuna personalizzazione trovata", result.Message);
        }

        [Fact]
        public async Task GetAllAsync_ShouldHandleInvalidPagination()
        {
            // Arrange
            var invalidPage = 0;
            var invalidPageSize = 0;

            // Act
            var result = await _repository.GetAllAsync(page: invalidPage, pageSize: invalidPageSize);

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.Data);
            // CORREZIONE: SecurityHelper.ValidatePagination corregge a page=1 e pageSize=1 (non 10!)
            // pageSize=0 viene clampato a 1, non a 10
            Assert.Equal(1, result.Page);
            Assert.Equal(1, result.PageSize);  // Modificato da 10 a 1
            Assert.Equal(5, result.TotalCount); // Le 5 del seed
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnSecondPage()
        {
            // Arrange
            // Crea più personalizzazioni per testare la paginazione
            await CreateMultiplePersonalizzazioniAsync(8); // +8 = 13 totali (5 seed + 8 nuove)

            // Act
            var result = await _repository.GetAllAsync(page: 2, pageSize: 5);

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.Data);
            Assert.Equal(13, result.TotalCount);
            Assert.Equal(5, result.Data.Count()); // Seconda pagina con 5 record
            Assert.Equal(2, result.Page);
            Assert.Equal(5, result.PageSize);
            Assert.True(result.HasPrevious);
            Assert.True(result.HasNext);
        }

        #endregion

        #region GetByIdAsync Tests

        [Fact]
        public async Task GetByIdAsync_ShouldReturnPersonalizzazione_WhenExists()
        {
            // Arrange
            var personalizzazione = await CreateTestPersonalizzazioneAsync("TEST_GETBYID", "Descrizione test");

            // Act
            var result = await _repository.GetByIdAsync(personalizzazione.PersonalizzazioneId);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(personalizzazione.PersonalizzazioneId, result.Data.PersonalizzazioneId);
            Assert.Equal("TEST_GETBYID", result.Data.Nome);
            Assert.Equal("Descrizione test", result.Data.Descrizione);
            Assert.Contains("trovata", result.Message);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnNotFound_WhenNotExists()
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
        public async Task GetByIdAsync_ShouldReturnError_WhenInvalidId()
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

        [Fact]
        public async Task GetByIdAsync_ShouldReturnError_WhenNegativeId()
        {
            // Arrange
            var negativeId = -1;

            // Act
            var result = await _repository.GetByIdAsync(negativeId);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Null(result.Data);
            Assert.Contains("non valido", result.Message);
        }

        #endregion

        #region GetByNomeAsync Tests

        [Fact]
        public async Task GetByNomeAsync_ShouldReturnPersonalizzazioni_WhenNameStartsWith()
        {
            // Arrange
            await CreateTestPersonalizzazioneAsync("EXTRA_TEST", "Descrizione 1");
            await CreateTestPersonalizzazioneAsync("EXTRA_SPECIAL", "Descrizione 2");
            await CreateTestPersonalizzazioneAsync("ALTRO", "Descrizione 3");

            // Act - Cerca "EXTRA" (case insensitive)
            var result = await _repository.GetByNomeAsync("extra", page: 1, pageSize: 10);

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.Data);
            Assert.Equal(2, result.TotalCount); // "EXTRA_TEST" e "EXTRA_SPECIAL"
            Assert.All(result.Data, p => Assert.StartsWith("EXTRA", p.Nome));
            Assert.Contains("Trovate 2 personalizzazioni", result.Message);
        }

        [Fact]
        public async Task GetByNomeAsync_ShouldReturnEmpty_WhenNoMatches()
        {
            // Arrange
            var searchTerm = "NONEXISTENT";

            // Act
            var result = await _repository.GetByNomeAsync(searchTerm);

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.Data);
            Assert.Equal(0, result.TotalCount);
            Assert.Empty(result.Data);
            Assert.Contains($"Nessuna personalizzazione trovata con nome che inizia con '{searchTerm}'", result.Message);
        }

        [Fact]
        public async Task GetByNomeAsync_ShouldHandleInvalidInput()
        {
            // Arrange
            var dangerousInput = "<script>alert('xss')</script>";

            // Act
            var result = await _repository.GetByNomeAsync(dangerousInput);

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.Data);
            Assert.Equal(0, result.TotalCount);
            Assert.Empty(result.Data);
            Assert.Contains("contiene caratteri non validi", result.Message);
        }

        [Fact]
        public async Task GetByNomeAsync_ShouldHandleEmptyAfterNormalization()
        {
            // Arrange
            var onlySpaces = "   ";

            // Act
            var result = await _repository.GetByNomeAsync(onlySpaces);

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.Data);
            Assert.Equal(0, result.TotalCount);
            Assert.Empty(result.Data);
            Assert.Contains("obbligatorio", result.Message);
        }

        [Fact]
        public async Task GetByNomeAsync_ShouldNormalizeToUppercase()
        {
            // Arrange
            await CreateTestPersonalizzazioneAsync("MAIUSCOLO", "Test");

            // Act - Cerca con minuscolo
            var result = await _repository.GetByNomeAsync("maiuscolo");

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.Data);
            Assert.Equal(1, result.TotalCount);
            Assert.Contains("MAIUSCOLO", result.Data.First().Nome);
        }

        #endregion

        #region AddAsync Tests

        [Fact]
        public async Task AddAsync_ShouldCreatePersonalizzazione_ForValidData()
        {
            // Arrange
            var dto = new PersonalizzazioneDTO
            {
                Nome = "Nuova Personalizzazione",
                Descrizione = "Descrizione dettagliata"
            };

            // Act
            var result = await _repository.AddAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.True(result.Data.PersonalizzazioneId > 0);            
            Assert.Equal("Descrizione dettagliata", result.Data.Descrizione);
            Assert.True(result.Data.DtCreazione > DateTime.MinValue);
            Assert.Contains("creata con successo", result.Message);

            // Verifica nel database
            var saved = await _context.Personalizzazione
                .FindAsync(result.Data.PersonalizzazioneId);
            Assert.NotNull(saved);
            Assert.Equal("Nuova Personalizzazione", saved.Nome);
        }

        [Fact]
        public async Task AddAsync_ShouldReturnError_WhenNomeIsEmpty()
        {
            // Arrange
            var dto = new PersonalizzazioneDTO
            {
                Nome = "",
                Descrizione = "Descrizione valida"
            };

            // Act
            var result = await _repository.AddAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Null(result.Data);
            Assert.Contains("obbligatorio", result.Message);
        }

        [Fact]
        public async Task AddAsync_ShouldReturnError_WhenDescrizioneIsEmpty()
        {
            // Arrange
            var dto = new PersonalizzazioneDTO
            {
                Nome = "Test Nome",
                Descrizione = ""
            };

            // Act
            var result = await _repository.AddAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Null(result.Data);
            Assert.Contains("obbligatorio", result.Message);
        }

        [Fact]
        public async Task AddAsync_ShouldReturnError_WhenNomeIsInvalid()
        {
            // Arrange
            var dto = new PersonalizzazioneDTO
            {
                Nome = "<script>alert('xss')</script>",
                Descrizione = "Descrizione valida"
            };

            // Act
            var result = await _repository.AddAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Null(result.Data);
            Assert.Contains("non valido", result.Message);
        }

        [Fact]
        public async Task AddAsync_ShouldReturnError_WhenDescrizioneIsInvalid()
        {
            // Arrange
            var dto = new PersonalizzazioneDTO
            {
                Nome = "Nome valido",
                Descrizione = new string('a', 501) // Supera i 500 caratteri
            };

            // Act
            var result = await _repository.AddAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Null(result.Data);
            Assert.Contains("non valido", result.Message);
        }

        [Fact]
        public async Task AddAsync_ShouldReturnError_WhenDuplicateNome()
        {
            // Arrange
            await CreateTestPersonalizzazioneAsync("DUPLICATO", "Prima");

            var dto = new PersonalizzazioneDTO
            {
                Nome = "duplicato", // Stesso nome, case diverso
                Descrizione = "Seconda"
            };

            // Act
            var result = await _repository.AddAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Null(result.Data);
            Assert.Contains("Esiste già", result.Message);
        }        

        #endregion

        #region UpdateAsync Tests

        [Fact]
        public async Task UpdateAsync_ShouldUpdatePersonalizzazione_ForValidData()
        {
            // Arrange
            var personalizzazione = await CreateTestPersonalizzazioneAsync("VECCHIO NOME", "Vecchia descrizione");

            var dto = new PersonalizzazioneDTO
            {
                PersonalizzazioneId = personalizzazione.PersonalizzazioneId,
                Nome = "Nuovo Nome",
                Descrizione = "Nuova descrizione"
            };

            // Act
            var result = await _repository.UpdateAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            // CORREZIONE: Rimuovi Assert.NotNull per bool, usa direttamente Assert.True
            Assert.True(result.Data); // bool true 
            Assert.Contains("aggiornata con successo", result.Message);

            // Verifica nel database
            var updated = await _context.Personalizzazione
                .FindAsync(personalizzazione.PersonalizzazioneId);
            Assert.NotNull(updated);            
            Assert.Equal("Nuova descrizione", updated.Descrizione);
        }

        [Fact]
        public async Task UpdateAsync_ShouldReturnNotFound_WhenNotExists()
        {
            // Arrange
            var dto = new PersonalizzazioneDTO
            {
                PersonalizzazioneId = 9999,
                Nome = "Test",
                Descrizione = "Test"
            };

            // Act
            var result = await _repository.UpdateAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.False(result.Data); // bool false
            Assert.Contains("non trovata", result.Message);
        }

        [Fact]
        public async Task UpdateAsync_ShouldReturnError_WhenDuplicateNome()
        {
            // Arrange
            var prima = await CreateTestPersonalizzazioneAsync("PRIMA", "Descrizione 1");
            var seconda = await CreateTestPersonalizzazioneAsync("SECONDA", "Descrizione 2");

            var dto = new PersonalizzazioneDTO
            {
                PersonalizzazioneId = seconda.PersonalizzazioneId,
                Nome = "prima", // Cerca di cambiare il nome della seconda in "PRIMA"
                Descrizione = "Descrizione modificata"
            };

            // Act
            var result = await _repository.UpdateAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.False(result.Data);
            Assert.Contains("Esiste già", result.Message);

            // Verifica che la seconda non sia cambiata
            var stillSeconda = await _context.Personalizzazione
                .FindAsync(seconda.PersonalizzazioneId);

            // CORREZIONE: Aggiungi controllo null per evitare warning
            Assert.NotNull(stillSeconda);
            Assert.Equal("SECONDA", stillSeconda.Nome);
            Assert.Equal("Descrizione 2", stillSeconda.Descrizione); // Verifica anche la descrizione non cambiata
        }

        [Fact]
        public async Task UpdateAsync_ShouldReturnNoChanges_WhenSameData()
        {
            // Arrange
            var personalizzazione = await CreateTestPersonalizzazioneAsync("STESSO", "Stessa descrizione");

            var dto = new PersonalizzazioneDTO
            {
                PersonalizzazioneId = personalizzazione.PersonalizzazioneId,
                Nome = "stesso", // Stesso nome, case diverso
                Descrizione = "Stessa descrizione"
            };

            // Act
            var result = await _repository.UpdateAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.False(result.Data); // bool false (nessuna modifica)
            Assert.Contains("Nessuna modifica necessaria", result.Message);
        }

        [Fact]
        public async Task UpdateAsync_ShouldUpdateOnlyNome()
        {
            // Arrange
            var personalizzazione = await CreateTestPersonalizzazioneAsync("ORIGINALE", "Descrizione originale");

            var dto = new PersonalizzazioneDTO
            {
                PersonalizzazioneId = personalizzazione.PersonalizzazioneId,
                Nome = "MODIFICATO",
                Descrizione = "Descrizione originale" // Stessa descrizione
            };

            // Act
            var result = await _repository.UpdateAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.True(result.Data); // bool true (c'è stata modifica)

            var updated = await _context.Personalizzazione
                .FindAsync(personalizzazione.PersonalizzazioneId);

            // CORREZIONE: Aggiungi controllo null per evitare warning
            Assert.NotNull(updated);
            Assert.Equal("MODIFICATO", updated.Nome);
            Assert.Equal("Descrizione originale", updated.Descrizione);
        }

        [Fact]
        public async Task UpdateAsync_ShouldUpdateOnlyDescrizione()
        {
            // Arrange
            var personalizzazione = await CreateTestPersonalizzazioneAsync("ORIGINALE", "Vecchia descrizione");

            var dto = new PersonalizzazioneDTO
            {
                PersonalizzazioneId = personalizzazione.PersonalizzazioneId,
                Nome = "ORIGINALE", // Stesso nome
                Descrizione = "Nuova descrizione"
            };

            // Act
            var result = await _repository.UpdateAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.True(result.Data); // bool true (c'è stata modifica)

            var updated = await _context.Personalizzazione
                .FindAsync(personalizzazione.PersonalizzazioneId);

            // CORREZIONE: Aggiungi controllo null per evitare warning
            Assert.NotNull(updated);
            Assert.Equal("ORIGINALE", updated.Nome);
            Assert.Equal("Nuova descrizione", updated.Descrizione);
        }

        #endregion

        #region DeleteAsync Tests

        [Fact]
        public async Task DeleteAsync_ShouldDeletePersonalizzazione_WhenNoDependencies()
        {
            // Arrange
            var personalizzazione = await CreateTestPersonalizzazioneAsync("DA ELIMINARE", "Descrizione");

            // Act
            var result = await _repository.DeleteAsync(personalizzazione.PersonalizzazioneId);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.True(result.Data); // bool true
            Assert.Contains("eliminata con successo", result.Message);

            // Verifica che sia stato eliminato
            var deleted = await _context.Personalizzazione
                .FindAsync(personalizzazione.PersonalizzazioneId);
            Assert.Null(deleted);
        }

        [Fact]
        public async Task DeleteAsync_ShouldReturnNotFound_WhenNotExists()
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
        public async Task DeleteAsync_ShouldReturnError_WhenInvalidId()
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
        public async Task ExistsAsync_ShouldReturnTrue_WhenExists()
        {
            // Arrange
            var personalizzazione = await CreateTestPersonalizzazioneAsync("EXISTS_TEST", "Descrizione");

            // Act
            var result = await _repository.ExistsAsync(personalizzazione.PersonalizzazioneId);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.True(result.Data); // bool true
            Assert.Contains("esiste", result.Message);
        }

        [Fact]
        public async Task ExistsAsync_ShouldReturnFalse_WhenNotExists()
        {
            // Arrange
            var nonExistentId = 9999;

            // Act
            var result = await _repository.ExistsAsync(nonExistentId);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.False(result.Data); // bool false
            Assert.Contains("non trovata", result.Message);
        }

        [Fact]
        public async Task ExistsAsync_ShouldReturnError_WhenInvalidId()
        {
            // Arrange
            var invalidId = 0;

            // Act
            var result = await _repository.ExistsAsync(invalidId);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.False(result.Data);
            Assert.Contains("non valido", result.Message);
        }

        #endregion

        #region ExistsByNomeAsync Tests

        [Fact]
        public async Task ExistsByNomeAsync_ShouldReturnTrue_WhenExists()
        {
            // Arrange
            await CreateTestPersonalizzazioneAsync("CERCA_ME", "Descrizione");

            // Act - Cerca case insensitive
            var result = await _repository.ExistsByNomeAsync("cerca_me");

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.True(result.Data); // bool true
            Assert.Contains("esiste", result.Message);
        }

        [Fact]
        public async Task ExistsByNomeAsync_ShouldReturnFalse_WhenNotExists()
        {
            // Arrange
            var nome = "NON_EXISTE";

            // Act
            var result = await _repository.ExistsByNomeAsync(nome);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.False(result.Data); // bool false
            Assert.Contains("non trovata", result.Message);
        }

        [Fact]
        public async Task ExistsByNomeAsync_ShouldReturnError_WhenEmptyNome()
        {
            // Arrange
            var emptyNome = "";

            // Act
            var result = await _repository.ExistsByNomeAsync(emptyNome);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.False(result.Data);
            Assert.Contains("obbligatorio", result.Message);
        }

        [Fact]
        public async Task ExistsByNomeAsync_ShouldReturnError_WhenInvalidInput()
        {
            // Arrange
            var dangerousInput = "<script>malicious</script>";

            // Act
            var result = await _repository.ExistsByNomeAsync(dangerousInput);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.False(result.Data);
            Assert.Contains("contiene caratteri non validi", result.Message);
        }

        [Fact]
        public async Task ExistsByNomeAsync_ShouldNormalizeToUppercase()
        {
            // Arrange
            await CreateTestPersonalizzazioneAsync("MAIUSCOLO", "Test");

            // Act - Cerca con minuscolo e spazi
            var result = await _repository.ExistsByNomeAsync("  maiuscolo  ");

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.True(result.Data);
            Assert.Contains("MAIUSCOLO", result.Message);
        }

        #endregion
    }
}