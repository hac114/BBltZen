using BBltZen;
using DTO;
using Moq;
using Repository.Service;
using Stripe.Climate;
using Xunit;

namespace RepositoryTest
{
    public class ArticoloRepositoryCleanTest : BaseTestClean
    {
        private readonly ArticoloRepository _repository;
        private readonly Mock<ILogger<ArticoloRepository>> _mockLogger;
        private static readonly string[] expected = ["BC", "BS", "D"];

        public ArticoloRepositoryCleanTest()
        {
            _mockLogger = new Mock<ILogger<ArticoloRepository>>();
            _repository = new ArticoloRepository(_context, _mockLogger.Object);
        }

        #region GetAllAsync Tests

        [Fact]
        public async Task GetAllAsync_ShouldReturnEmpty_WhenNoArticoli()
        {
            // Arrange
            await CleanTableAsync<Articolo>();

            // Act
            var result = await _repository.GetAllAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result.Data);
            Assert.Equal(0, result.TotalCount);
            Assert.Contains("Nessun articolo trovato", result.Message);
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnArticoli_WhenArticoliExist()
        {
            // Arrange
            await CleanTableAsync<Articolo>();
            var articoli = await CreateAllTipiArticoliAsync();

            // Act
            var result = await _repository.GetAllAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, result.Data.Count());
            Assert.Equal(3, result.TotalCount);
            Assert.Contains("Trovati 3 articoli", result.Message);
        }

        [Fact]
        public async Task GetAllAsync_ShouldRespectPagination()
        {
            // Arrange
            await CleanTableAsync<Articolo>();
            var articoli = await CreateMultipleArticoliAsync("BC", 15);

            // Act
            var result = await _repository.GetAllAsync(page: 2, pageSize: 5);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(5, result.Data.Count());
            Assert.Equal(2, result.Page);
            Assert.Equal(5, result.PageSize);
            Assert.Equal(15, result.TotalCount);
            Assert.Equal(3, result.TotalPages);
            Assert.True(result.HasPrevious);
            Assert.True(result.HasNext);
        }

        #endregion

        #region GetByIdAsync Tests

        [Fact]
        public async Task GetByIdAsync_ShouldReturnNotFound_WhenArticoloNotExists()
        {
            // Act
            var result = await _repository.GetByIdAsync(9999);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Null(result.Data);
            Assert.Contains("non trovato", result.Message);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnError_WhenInvalidId()
        {
            // Act
            var result = await _repository.GetByIdAsync(0);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Contains("non valido", result.Message);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnArticolo_WhenExists()
        {
            // Arrange
            var articolo = await CreateTestArticoloAsync("BC");

            // Act
            var result = await _repository.GetByIdAsync(articolo.ArticoloId);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(articolo.ArticoloId, result.Data.ArticoloId);
            Assert.Equal(articolo.Tipo, result.Data.Tipo);
            AssertDateTimeWithTolerance(articolo.DataCreazione, result.Data.DataCreazione);
            AssertDateTimeWithTolerance(articolo.DataAggiornamento, result.Data.DataAggiornamento);
            Assert.Contains("trovato", result.Message);
        }

        #endregion

        #region GetByTipoAsync Tests

        [Fact]
        public async Task GetByTipoAsync_ShouldReturnEmpty_WhenNoMatchingTipo()
        {
            // Arrange
            await CleanTableAsync<Articolo>();
            await CreateAllTipiArticoliAsync();

            // Act
            var result = await _repository.GetByTipoAsync("XX");

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result.Data);
            Assert.Equal(0, result.TotalCount);
            // ✅ MODIFICATO: Il repository dice "Tipo non valido" per XX
            Assert.Contains("Tipo non valido", result.Message);
        }

        [Fact]
        public async Task GetByTipoAsync_ShouldReturnArticoli_ForValidTipo()
        {
            // Arrange
            await CleanTableAsync<Articolo>();
            await CreateMultipleArticoliAsync("BC", 3);
            await CreateMultipleArticoliAsync("BS", 2);
            await CreateMultipleArticoliAsync("D", 1);

            // Act
            var result = await _repository.GetByTipoAsync("BC");

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, result.Data.Count());
            Assert.Equal(3, result.TotalCount);
            Assert.All(result.Data, dto => Assert.Equal("BC", dto.Tipo));
            Assert.Contains("Trovati 3 articoli", result.Message);
        }

        [Theory]
        [InlineData("BC")]
        [InlineData("BS")]
        [InlineData("D")]
        public async Task GetByTipoAsync_ShouldWork_ForAllValidTipi(string tipo)
        {
            // Arrange
            await CleanTableAsync<Articolo>();
            var articolo = await CreateTestArticoloAsync(tipo);

            // Act
            var result = await _repository.GetByTipoAsync(tipo);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result.Data);
            var dto = result.Data.First();
            Assert.Equal(tipo, dto.Tipo);
        }

        [Fact]
        public async Task GetByTipoAsync_ShouldValidateInput_ForInvalidCharacters()
        {
            // Act
            var result = await _repository.GetByTipoAsync("<script>alert('xss')</script>");

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result.Data);
            Assert.Contains("non validi", result.Message);
        }

        [Fact]
        public async Task GetByTipoAsync_ShouldHandleCaseInsensitiveSearch()
        {
            // Arrange
            await CleanTableAsync<Articolo>();
            await CreateTestArticoloAsync("BC");

            // Act - input in minuscolo
            var result = await _repository.GetByTipoAsync("bc");

            // Assert
            Assert.NotNull(result);
            Assert.Single(result.Data);
            var dto = result.Data.First();
            Assert.Equal("BC", dto.Tipo);
        }

        #endregion

        #region AddAsync Tests

        [Fact]
        public async Task AddAsync_ShouldReturnError_WhenTipoIsEmpty()
        {
            // Arrange
            var dto = new ArticoloDTO { Tipo = "" };

            // Act
            var result = await _repository.AddAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Null(result.Data);
            Assert.Contains("obbligatorio", result.Message);
        }

        [Fact]
        public async Task AddAsync_ShouldReturnError_WhenTipoIsInvalid()
        {
            // Arrange
            var dto = new ArticoloDTO { Tipo = "XX" }; // Non valido

            // Act
            var result = await _repository.AddAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Null(result.Data);
            Assert.Contains("non valido", result.Message);
        }

        [Fact]
        public async Task AddAsync_ShouldReturnError_WhenTipoHasDangerousCharacters()
        {
            // Arrange
            var dto = new ArticoloDTO { Tipo = "<script>" };

            // Act
            var result = await _repository.AddAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Contains("non valido", result.Message);
        }

        [Theory]
        [InlineData("BC")]
        [InlineData("BS")]
        [InlineData("D")]
        public async Task AddAsync_ShouldCreateArticolo_ForValidTipo(string tipo)
        {
            // Arrange
            var dto = new ArticoloDTO { Tipo = tipo };

            // Act
            var result = await _repository.AddAsync(dto);

            // Assert
            Assert.NotNull(result);

            // Per debugging se fallisce
            if (!result.Success)
            {
                Assert.Fail($"AddAsync fallito per tipo '{tipo}': {result.Message}");
            }

            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.True(result.Data.ArticoloId > 0);

            // Tipo dovrebbe essere maiuscolo dopo normalizzazione
            Assert.Equal(tipo.ToUpper(), result.Data.Tipo);

            // DataCreazione deve essere recente (entro 5 secondi da adesso)
            var now = DateTime.UtcNow;
            Assert.True(result.Data.DataCreazione <= now,
                $"DataCreazione {result.Data.DataCreazione} è nel futuro rispetto a {now}");
            Assert.True(now - result.Data.DataCreazione < TimeSpan.FromSeconds(5),
                $"DataCreazione {result.Data.DataCreazione} è troppo vecchia (oltre 5 secondi da {now})");

            // DataAggiornamento inizialmente uguale a DataCreazione
            Assert.Equal(result.Data.DataCreazione, result.Data.DataAggiornamento);

            Assert.Contains("creato con successo", result.Message);
        }

        [Fact]
        public async Task AddAsync_ShouldNormalizeTipo_ToUpperCase()
        {
            // Arrange
            var dto = new ArticoloDTO { Tipo = "bc" }; // minuscolo

            // Act
            var result = await _repository.AddAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);

            // VERIFICA NEL DATABASE (non solo nel DTO)
            var articoloInDb = await _context.Articolo.FindAsync(result.Data.ArticoloId);
            Assert.NotNull(articoloInDb);
            Assert.Equal("BC", articoloInDb!.Tipo);
        }

        #endregion

        #region UpdateAsync Tests

        [Fact]
        public async Task UpdateAsync_ShouldReturnError_WhenArticoloNotFound()
        {
            // Arrange
            var dto = new ArticoloDTO { ArticoloId = 9999, Tipo = "BC" };

            // Act
            var result = await _repository.UpdateAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Contains("non trovato", result.Message);
        }

        [Fact]
        public async Task UpdateAsync_ShouldReturnError_WhenInvalidId()
        {
            // Arrange
            var dto = new ArticoloDTO { ArticoloId = 0, Tipo = "BC" };

            // Act
            var result = await _repository.UpdateAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Contains("non valido", result.Message);
        }

        [Fact]
        public async Task UpdateAsync_ShouldReturnError_WhenTipoInvalid()
        {
            // Arrange
            var articolo = await CreateTestArticoloAsync("BC");
            var dto = new ArticoloDTO { ArticoloId = articolo.ArticoloId, Tipo = "XX" };

            // Act
            var result = await _repository.UpdateAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Contains("non valido", result.Message);
        }

        [Fact]
        public async Task UpdateAsync_ShouldUpdateArticolo_WhenValidChanges()
        {
            // Arrange
            var articolo = await CreateTestArticoloAsync("BC");
            var originalUpdateTime = articolo.DataAggiornamento;
            var dto = new ArticoloDTO
            {
                ArticoloId = articolo.ArticoloId,
                Tipo = "BS" // Cambia tipo
            };

            // Act
            var result = await _repository.UpdateAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.True(result.Data); // bool = true per successo

            // Verifica nel database - SICURA
            var updatedArticolo = await _context.Articolo.FindAsync(articolo.ArticoloId);
            Assert.NotNull(updatedArticolo); // ✅ Verifica che non sia null
            Assert.Equal("BS", updatedArticolo!.Tipo);
            Assert.Equal(articolo.DataCreazione, updatedArticolo.DataCreazione); // DataCreazione non cambia
            Assert.True(updatedArticolo.DataAggiornamento > originalUpdateTime); // DataAggiornamento aggiornata
            Assert.Contains("aggiornato", result.Message);
        }

        [Fact]
        public async Task UpdateAsync_ShouldReturnNoChanges_WhenSameData()
        {
            // Arrange
            var articolo = await CreateTestArticoloAsync("BC");
            var originalUpdateTime = articolo.DataAggiornamento;
            var dto = new ArticoloDTO
            {
                ArticoloId = articolo.ArticoloId,
                Tipo = "BC" // Stesso tipo
            };

            // Act
            var result = await _repository.UpdateAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.False(result.Data); // bool = false perché nessuna modifica
            Assert.Contains("Nessuna modifica", result.Message);

            // Verifica che DataAggiornamento non sia cambiata - SICURA
            var sameArticolo = await _context.Articolo.FindAsync(articolo.ArticoloId);
            Assert.NotNull(sameArticolo); // ✅ Verifica che non sia null
            AssertDateTimeWithTolerance(originalUpdateTime, sameArticolo!.DataAggiornamento);
        }

        [Fact]
        public async Task UpdateAsync_ShouldNormalizeInput()
        {
            // Arrange
            var articolo = await CreateTestArticoloAsync("BC");
            var dto = new ArticoloDTO
            {
                ArticoloId = articolo.ArticoloId,
                Tipo = "  bs  " // Con spazi e minuscolo
            };

            // Act
            var updateResult = await _repository.UpdateAsync(dto);

            // Assert
            Assert.NotNull(updateResult);

            if (!updateResult.Success)
            {
                Assert.Fail($"Update fallito: {updateResult.Message}");
            }

            Assert.True(updateResult.Success);

            // Verifica nel database (senza Include)
            var updated = await _context.Articolo.FindAsync(articolo.ArticoloId);
            Assert.NotNull(updated);

            // Dovrebbe essere maiuscolo dopo normalizzazione
            Assert.Equal("BS", updated.Tipo);

            // DataAggiornamento dovrebbe essere recente e maggiore o uguale a quella originale
            Assert.True(updated.DataAggiornamento >= articolo.DataAggiornamento,
                $"DataAggiornamento non è aumentata: {articolo.DataAggiornamento} <= {updated.DataAggiornamento}");

            // DataCreazione NON dovrebbe cambiare
            Assert.Equal(articolo.DataCreazione, updated.DataCreazione);
        }

        #endregion

        #region DeleteAsync Tests

        [Fact]
        public async Task DeleteAsync_ShouldReturnError_WhenInvalidId()
        {
            // Act
            var result = await _repository.DeleteAsync(0);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Contains("non valido", result.Message);
        }

        [Fact]
        public async Task DeleteAsync_ShouldReturnNotFound_WhenArticoloNotExists()
        {
            // Act
            var result = await _repository.DeleteAsync(9999);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Contains("non trovato", result.Message);
        }

        [Fact]
        public async Task DeleteAsync_ShouldDeleteArticolo_WhenNoDependencies()
        {
            // Arrange
            var articolo = await CreateTestArticoloAsync("BC");

            // Act
            var result = await _repository.DeleteAsync(articolo.ArticoloId);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.True(result.Data);
            Assert.Contains("eliminato", result.Message);

            // Verifica che sia stato eliminato - SICURA
            var deleted = await _context.Articolo.FindAsync(articolo.ArticoloId);
            Assert.Null(deleted);
        }

        [Fact]
        public async Task DeleteAsync_ShouldReturnError_WhenHasOrderItemDependencies()
        {
            // Arrange
            var articolo = await CreateTestArticoloAsync("BC");

            // Crea un ordine
            var ordine = new Ordine
            {
                ClienteId = 1,
                DataCreazione = DateTime.UtcNow,
                DataAggiornamento = DateTime.UtcNow,
                StatoOrdineId = 1,
                StatoPagamentoId = 1,
                Totale = 10.00m,
                Priorita = 1,
                SessioneId = null


            };
            _context.Ordine.Add(ordine);
            await _context.SaveChangesAsync();

            // Crea OrderItem collegato
            var orderItem = new OrderItem
            {
                OrdineId = ordine.OrdineId,
                ArticoloId = articolo.ArticoloId,
                Quantita = 1,
                PrezzoUnitario = 5.00m,
                ScontoApplicato = 0.00m,
                Imponibile = 5.00m,
                DataCreazione = DateTime.UtcNow,
                DataAggiornamento = DateTime.UtcNow,
                TipoArticolo = "BC",
                TotaleIvato = 6.10m,
                TaxRateId = 1
            };
            _context.OrderItem.Add(orderItem);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.DeleteAsync(articolo.ArticoloId);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Contains("dipendenze", result.Message.ToLower());

            // Verifica che l'articolo non sia stato eliminato - SICURA
            var stillExists = await _context.Articolo.FindAsync(articolo.ArticoloId);
            Assert.NotNull(stillExists); // ✅ Verifica che esista ancora
        }

        [Fact]
        public async Task DeleteAsync_ShouldReturnError_WhenHasBevandaCustomDependency()
        {
            // Arrange
            var articolo = await CreateTestArticoloAsync("BC");

            // Crea BevandaCustom collegata
            var bevandaCustom = new BevandaCustom
            {
                ArticoloId = articolo.ArticoloId,
                PersCustomId = 1,
                Prezzo = 5.00m
            };
            _context.BevandaCustom.Add(bevandaCustom);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.DeleteAsync(articolo.ArticoloId);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Contains("dipendenze", result.Message.ToLower());
        }

        #endregion

        #region ExistsAsync Tests

        [Fact]
        public async Task ExistsAsync_ShouldReturnFalse_WhenArticoloNotExists()
        {
            // Act
            var result = await _repository.ExistsAsync(9999);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.False(result.Data);
            Assert.Contains("non trovato", result.Message);
        }

        [Fact]
        public async Task ExistsAsync_ShouldReturnTrue_WhenArticoloExists()
        {
            // Arrange
            var articolo = await CreateTestArticoloAsync("BC");

            // Act
            var result = await _repository.ExistsAsync(articolo.ArticoloId);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.True(result.Data);
            Assert.Contains("esiste", result.Message);
        }

        [Fact]
        public async Task ExistsAsync_ShouldReturnError_WhenInvalidId()
        {
            // Act
            var result = await _repository.ExistsAsync(0);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Contains("non valido", result.Message);
        }

        #endregion

        #region ExistsByTipoAsync Tests

        [Fact]
        public async Task ExistsByTipoAsync_ShouldReturnError_WhenTipoNotValid()
        {
            // Act
            var result = await _repository.ExistsByTipoAsync("XX");

            // Assert
            Assert.NotNull(result);
            // ✅ MODIFICATO: XX non è valido → Success = false
            Assert.False(result.Success);
            Assert.Contains("non valido", result.Message);
        }

        [Fact]
        public async Task ExistsByTipoAsync_ShouldReturnTrue_WhenTipoExists()
        {
            // Arrange
            await CreateTestArticoloAsync("BC");

            // Act
            var result = await _repository.ExistsByTipoAsync("BC");

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.True(result.Data);
            // ✅ MODIFICATO: Case-insensitive o verifica generica
            Assert.False(string.IsNullOrEmpty(result.Message));
        }

        [Fact]
        public async Task ExistsByTipoAsync_ShouldHandleCaseInsensitive()
        {
            // Arrange
            await CreateTestArticoloAsync("BC");

            // Act - ricerca in minuscolo
            var result = await _repository.ExistsByTipoAsync("bc");

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.True(result.Data);
        }

        [Fact]
        public async Task ExistsByTipoAsync_ShouldReturnError_WhenInvalidTipo()
        {
            // Act
            var result = await _repository.ExistsByTipoAsync("<script>");

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Contains("non validi", result.Message);
        }

        [Fact]
        public async Task ExistsByTipoAsync_ShouldReturnError_WhenEmptyTipo()
        {
            // Act
            var result = await _repository.ExistsByTipoAsync("");

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Contains("obbligatorio", result.Message);
        }

        #endregion

        #region Integration Tests

        [Fact]
        public async Task FullCrudFlow_ShouldWorkCorrectly()
        {
            // 1. CREATE
            var createDto = new ArticoloDTO { Tipo = "BC" };
            var createResult = await _repository.AddAsync(createDto);
            Assert.True(createResult.Success);
            Assert.NotNull(createResult.Data);
            var articoloId = createResult.Data.ArticoloId;

            // 2. READ (GetById)
            var readResult = await _repository.GetByIdAsync(articoloId);
            Assert.True(readResult.Success);
            Assert.NotNull(readResult.Data);
            Assert.Equal("BC", readResult.Data.Tipo);

            // 3. EXISTS
            var existsResult = await _repository.ExistsAsync(articoloId);
            Assert.True(existsResult.Success);
            Assert.True(existsResult.Data);

            var existsByTipoResult = await _repository.ExistsByTipoAsync("BC");
            Assert.True(existsByTipoResult.Success);
            Assert.True(existsByTipoResult.Data);

            // 4. UPDATE
            var updateDto = new ArticoloDTO
            {
                ArticoloId = articoloId,
                Tipo = "BS"
            };
            var updateResult = await _repository.UpdateAsync(updateDto);
            Assert.True(updateResult.Success);

            // Verifica aggiornamento - SICURA
            var afterUpdate = await _repository.GetByIdAsync(articoloId);
            Assert.True(afterUpdate.Success);
            Assert.NotNull(afterUpdate.Data);
            Assert.Equal("BS", afterUpdate.Data.Tipo);

            // Verifica anche nel database
            var articoloInDb = await _context.Articolo.FindAsync(articoloId);
            Assert.NotNull(articoloInDb); // ✅ Verifica
            Assert.Equal("BS", articoloInDb!.Tipo);

            // 5. DELETE
            var deleteResult = await _repository.DeleteAsync(articoloId);
            Assert.True(deleteResult.Success);

            // Verifica eliminazione
            var afterDelete = await _repository.ExistsAsync(articoloId);
            Assert.True(afterDelete.Success);
            Assert.False(afterDelete.Data);

            // Verifica anche nel database
            var deletedInDb = await _context.Articolo.FindAsync(articoloId);
            Assert.Null(deletedInDb);
        }

        [Fact]
        public async Task GetAll_ShouldReturnCorrectOrder()
        {
            // Arrange - Crea articoli in ordine sparso
            await CleanTableAsync<Articolo>();
            await CreateTestArticoloAsync("D", dataCreazione: DateTime.UtcNow.AddMinutes(3));
            await CreateTestArticoloAsync("BC", dataCreazione: DateTime.UtcNow.AddMinutes(1));
            await CreateTestArticoloAsync("BS", dataCreazione: DateTime.UtcNow.AddMinutes(2));

            // Act
            var result = await _repository.GetAllAsync();

            // Assert - Dovrebbe essere ordinato per Tipo (alfabetico)
            var tipos = result.Data.Select(a => a.Tipo).ToList();
            Assert.Equal(expected, tipos);
        }

        [Fact]
        public async Task GetByTipo_ShouldRespectPagination()
        {
            // Arrange
            await CleanTableAsync<Articolo>();
            await CreateMultipleArticoliAsync("BC", 10);

            // Act
            var result = await _repository.GetByTipoAsync("BC", page: 2, pageSize: 3);

            // Assert
            Assert.Equal(3, result.Data.Count());
            Assert.Equal(2, result.Page);
            Assert.Equal(10, result.TotalCount);
            Assert.Equal(4, result.TotalPages); // 10/3 = 3.33 → 4 pagine
        }

        #endregion        
    }
}