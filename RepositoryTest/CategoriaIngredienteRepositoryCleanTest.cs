using BBltZen;
using DTO;
using Repository.Service;
using Xunit;

namespace RepositoryTest
{
    public class CategoriaIngredienteRepositoryCleanTest : BaseTestClean
    {
        private readonly CategoriaIngredienteRepository _repository;

        public CategoriaIngredienteRepositoryCleanTest()
        {
            _repository = new CategoriaIngredienteRepository(_context, GetTestLogger<CategoriaIngredienteRepository>());
        }

        #region GetAllAsync Tests

        [Fact]
        public async Task GetAllAsync_ShouldReturnPaginatedCategorie()
        {
            // Act
            var result = await _repository.GetAllAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Page);
            Assert.Equal(10, result.PageSize);
            Assert.True(result.TotalCount >= 6); // 6 categorie seedate nel BaseTestClean
            Assert.Contains("categorie di ingrediente", result.Message);
        }

        [Fact]
        public async Task GetAllAsync_WithPagination_ShouldReturnCorrectPage()
        {
            // Arrange
            await CreateMultipleCategorieAsync(10); // Aggiungi altre categorie

            // Act - Pagina 2, 5 elementi per pagina
            var result = await _repository.GetAllAsync(page: 2, pageSize: 5);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Page);
            Assert.Equal(5, result.PageSize);
            Assert.True(result.Data.Count() <= 5);
        }

        [Fact]
        public async Task GetAllAsync_NoCategorie_ShouldReturnEmpty()
        {
            // Arrange
            await CleanTableAsync<CategoriaIngrediente>();

            // Act
            var result = await _repository.GetAllAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result.Data);
            Assert.Equal(0, result.TotalCount);
            Assert.Contains("Nessuna categoria di ingrediente trovata", result.Message);
        }

        [Fact]
        public async Task GetAllAsync_ShouldOrderByCategoria()
        {
            // Arrange
            await CleanTableAsync<CategoriaIngrediente>();

            await CreateTestCategoriaIngredienteAsync("Zeta");
            await CreateTestCategoriaIngredienteAsync("Alfa");
            await CreateTestCategoriaIngredienteAsync("Beta");

            // Act
            var result = await _repository.GetAllAsync();

            // Assert - Verifica ordinamento alfabetico
            var categorie = result.Data.ToList();
            Assert.Equal("Alfa", categorie[0].Categoria);
            Assert.Equal("Beta", categorie[1].Categoria);
            Assert.Equal("Zeta", categorie[2].Categoria);
        }

        #endregion

        #region GetByIdAsync Tests

        [Fact]
        public async Task GetByIdAsync_ValidId_ShouldReturnCategoria()
        {
            // Arrange
            var testCategoria = await CreateTestCategoriaIngredienteAsync("TestGetById");

            // Act
            var result = await _repository.GetByIdAsync(testCategoria.CategoriaId);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(testCategoria.CategoriaId, result.Data.CategoriaId);
            Assert.Equal("TestGetById", result.Data.Categoria);
            Assert.Contains($"Categoria ingrediente con ID {testCategoria.CategoriaId} trovata", result.Message);
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
            Assert.Contains("ID categoria non valido", result.Message);
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
            Assert.Contains($"Categoria ingrediente con ID {nonExistentId} non trovata", result.Message);
        }

        #endregion

        #region GetByNomeAsync Tests

        [Fact]
        public async Task GetByNomeAsync_ValidNome_ShouldReturnFilteredCategorie()
        {
            // Arrange
            await CleanTableAsync<CategoriaIngrediente>();

            await CreateTestCategoriaIngredienteAsync("TestCategoria1");
            await CreateTestCategoriaIngredienteAsync("TestCategoria2");
            await CreateTestCategoriaIngredienteAsync("AltraCategoria");

            // Act
            var result = await _repository.GetByNomeAsync("Test");

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.TotalCount);
            Assert.True(result.Data.All(c => c.Categoria!.StartsWith("Test", StringComparison.OrdinalIgnoreCase)));
            Assert.Contains("Test", result.Message);
        }

        [Fact]
        public async Task GetByNomeAsync_EmptyNome_ShouldReturnError()
        {
            // Act
            var result = await _repository.GetByNomeAsync("");

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result.Data);
            Assert.Contains("Il parametro 'categoria' è obbligatorio", result.Message);
        }

        [Fact]
        public async Task GetByNomeAsync_InvalidInput_ShouldReturnError()
        {
            // Arrange
            var invalidInput = new string('A', 60); // Troppo lungo

            // Act
            var result = await _repository.GetByNomeAsync(invalidInput);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result.Data);
            Assert.Contains("Il parametro 'categoria' contiene caratteri non validi", result.Message);
        }

        [Fact]
        public async Task GetByNomeAsync_CaseInsensitive_ShouldFindMatches()
        {
            // Arrange
            await CleanTableAsync<CategoriaIngrediente>();
            await CreateTestCategoriaIngredienteAsync("TESTCATEGORIA");

            // Act - Cerca con case diverso
            var result = await _repository.GetByNomeAsync("test");

            // Assert
            Assert.NotNull(result);
            Assert.Single(result.Data);
            Assert.Equal("TESTCATEGORIA", result.Data.First().Categoria);
        }

        [Fact]
        public async Task GetByNomeAsync_WithPagination_ShouldReturnCorrectPage()
        {
            // Arrange
            await CleanTableAsync<CategoriaIngrediente>();

            for (int i = 0; i < 15; i++)
            {
                await CreateTestCategoriaIngredienteAsync($"Test{i}");
            }

            // Act
            var result = await _repository.GetByNomeAsync("Test", page: 2, pageSize: 5);

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
            var testCategoria = await CreateTestCategoriaIngredienteAsync();

            // Act
            var result = await _repository.ExistsAsync(testCategoria.CategoriaId);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.True(result.Data);
            Assert.Contains($"Categoria ingrediente con ID {testCategoria.CategoriaId} esiste", result.Message);
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
            Assert.Contains($"Categoria ingrediente con ID 9999 non trovata", result.Message);
        }

        [Fact]
        public async Task ExistsAsync_InvalidId_ShouldReturnError()
        {
            // Act
            var result = await _repository.ExistsAsync(0);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Contains("ID categoria non valido", result.Message);
        }

        #endregion

        #region ExistsByNomeAsync Tests

        [Fact]
        public async Task ExistsByNomeAsync_ExistingNome_ShouldReturnTrue()
        {
            // Arrange
            await CreateTestCategoriaIngredienteAsync("TestExists");

            // Act
            var result = await _repository.ExistsByNomeAsync("TestExists");

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.True(result.Data);
            Assert.Contains($"Categoria ingrediente con nome 'TestExists' esiste", result.Message);
        }

        [Fact]
        public async Task ExistsByNomeAsync_NonExistentNome_ShouldReturnFalse()
        {
            // Act
            var result = await _repository.ExistsByNomeAsync("NomeInesistente");

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.False(result.Data);
            Assert.Contains($"Categoria ingrediente con nome 'NomeInesistente' non trovata", result.Message);
        }

        [Fact]
        public async Task ExistsByNomeAsync_EmptyNome_ShouldReturnError()
        {
            // Act
            var result = await _repository.ExistsByNomeAsync("");

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Contains("Il nome della categoria è obbligatorio", result.Message);
        }

        [Fact]
        public async Task ExistsByNomeAsync_InvalidInput_ShouldReturnError()
        {
            // Arrange
            var invalidInput = new string('A', 60); // Troppo lungo

            // Act
            var result = await _repository.ExistsByNomeAsync(invalidInput);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Contains("Il nome della categoria contiene caratteri non validi", result.Message);
        }

        [Fact]
        public async Task ExistsByNomeAsync_CaseInsensitive_ShouldFindMatches()
        {
            // Arrange
            await CreateTestCategoriaIngredienteAsync("TESTUPPERCASE");

            // Act - Cerca con case diverso
            var result = await _repository.ExistsByNomeAsync("testuppercase");

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.True(result.Data);
        }

        #endregion

        #region AddAsync Tests

        [Fact]
        public async Task AddAsync_ValidCategoria_ShouldCreateAndReturnCategoria()
        {
            // Arrange
            var categoriaDto = new CategoriaIngredienteDTO
            {
                Categoria = "NuovaCategoriaTest"
            };

            // Act
            var result = await _repository.AddAsync(categoriaDto);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.True(result.Data.CategoriaId > 0);
            Assert.Equal("NuovaCategoriaTest", result.Data.Categoria);
            Assert.Contains("NuovaCategoriaTest", result.Message);
            Assert.Contains("creata con successo", result.Message);

            // Verifica che sia stato salvato nel database
            var savedCategoria = await _context.CategoriaIngrediente.FindAsync(result.Data.CategoriaId);
            Assert.NotNull(savedCategoria);
            Assert.Equal(result.Data.CategoriaId, savedCategoria.CategoriaId);
        }

        [Fact]
        public async Task AddAsync_DuplicateNome_ShouldReturnError()
        {
            // Arrange
            await CreateTestCategoriaIngredienteAsync("Duplicato");

            var categoriaDto = new CategoriaIngredienteDTO
            {
                Categoria = "Duplicato"
            };

            // Act
            var result = await _repository.AddAsync(categoriaDto);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Null(result.Data);
            Assert.Contains("Esiste già una categoria con nome 'Duplicato'", result.Message);
        }

        [Fact]
        public async Task AddAsync_EmptyNome_ShouldReturnError()
        {
            // Arrange
            var categoriaDto = new CategoriaIngredienteDTO
            {
                Categoria = ""
            };

            // Act
            var result = await _repository.AddAsync(categoriaDto);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Null(result.Data);
            Assert.Contains("Nome categoria obbligatorio", result.Message);
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
            Assert.Contains("Errore interno", result.Message);
        }

        [Fact]
        public async Task AddAsync_InvalidNomeLength_ShouldReturnError()
        {
            // Arrange
            var categoriaDto = new CategoriaIngredienteDTO
            {
                Categoria = new string('A', 60) // Troppo lungo
            };

            // Act
            var result = await _repository.AddAsync(categoriaDto);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Null(result.Data);
            Assert.Contains("Nome categoria non valido", result.Message);
        }

        #endregion

        #region UpdateAsync Tests

        [Fact]
        public async Task UpdateAsync_ValidUpdate_ShouldUpdateCategoria()
        {
            // Arrange
            var categoria = await CreateTestCategoriaIngredienteAsync("VecchioNome");
            var categoriaDto = new CategoriaIngredienteDTO
            {
                CategoriaId = categoria.CategoriaId,
                Categoria = "NuovoNome"
            };

            // Act
            var result = await _repository.UpdateAsync(categoriaDto);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.True(result.Data);
            Assert.Contains($"Categoria con ID {categoria.CategoriaId} aggiornata con successo", result.Message);

            // Verifica aggiornamento nel database
            var updatedCategoria = await _context.CategoriaIngrediente.FindAsync(categoria.CategoriaId);
            Assert.NotNull(updatedCategoria);
            Assert.Equal("NuovoNome", updatedCategoria.Categoria);
        }

        [Fact]
        public async Task UpdateAsync_NonExistentId_ShouldReturnNotFound()
        {
            // Arrange
            var categoriaDto = new CategoriaIngredienteDTO
            {
                CategoriaId = 9999,
                Categoria = "Test"
            };

            // Act
            var result = await _repository.UpdateAsync(categoriaDto);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Contains($"Categoria ingrediente con ID 9999 non trovata", result.Message);
        }

        [Fact]
        public async Task UpdateAsync_DuplicateNome_ShouldReturnError()
        {
            // Arrange
            var categoria1 = await CreateTestCategoriaIngredienteAsync("Categoria1");
            var categoria2 = await CreateTestCategoriaIngredienteAsync("Categoria2");

            var categoriaDto = new CategoriaIngredienteDTO
            {
                CategoriaId = categoria1.CategoriaId,
                Categoria = "Categoria2" // Nome già usato da categoria2
            };

            // Act
            var result = await _repository.UpdateAsync(categoriaDto);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Contains("Esiste già un'altra categoria con nome 'Categoria2'", result.Message);
        }

        [Fact]
        public async Task UpdateAsync_NoChanges_ShouldReturnFalseWithMessage()
        {
            // Arrange
            var categoria = await CreateTestCategoriaIngredienteAsync("TestNoChanges");
            var categoriaDto = new CategoriaIngredienteDTO
            {
                CategoriaId = categoria.CategoriaId,
                Categoria = "TestNoChanges" // Stesso nome
            };

            // Act
            var result = await _repository.UpdateAsync(categoriaDto);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.False(result.Data); // False perché non ci sono cambiamenti
            Assert.Contains($"Nessuna modifica necessaria", result.Message);
        }

        [Fact]
        public async Task UpdateAsync_EmptyNome_ShouldReturnError()
        {
            // Arrange
            var categoria = await CreateTestCategoriaIngredienteAsync("Test");
            var categoriaDto = new CategoriaIngredienteDTO
            {
                CategoriaId = categoria.CategoriaId,
                Categoria = ""
            };

            // Act
            var result = await _repository.UpdateAsync(categoriaDto);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Contains("Nome categoria obbligatorio", result.Message);
        }

        [Fact]
        public async Task UpdateAsync_NullDto_ShouldReturnErrorResponse()
        {
            // Act
            var result = await _repository.UpdateAsync(null!);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Contains("Errore interno", result.Message);
        }

        #endregion

        #region DeleteAsync Tests

        [Fact]
        public async Task DeleteAsync_ValidId_ShouldDeleteCategoria()
        {
            // Arrange
            var categoria = await CreateTestCategoriaIngredienteAsync("TestDelete");
            var categoriaId = categoria.CategoriaId;

            // Act
            var result = await _repository.DeleteAsync(categoriaId);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.True(result.Data);
            Assert.Contains($"eliminata con successo", result.Message);

            // Verifica che sia stato eliminato dal database
            var deletedCategoria = await _context.CategoriaIngrediente.FindAsync(categoriaId);
            Assert.Null(deletedCategoria);
        }

        [Fact]
        public async Task DeleteAsync_NonExistentId_ShouldReturnNotFound()
        {
            // Act
            var result = await _repository.DeleteAsync(9999);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Contains($"Categoria ingrediente con ID 9999 non trovata", result.Message);
        }

        [Fact]
        public async Task DeleteAsync_InvalidId_ShouldReturnError()
        {
            // Act
            var result = await _repository.DeleteAsync(0);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Contains("ID categoria non valido", result.Message);
        }

        [Fact]
        public async Task DeleteAsync_CategoriaWithDependencies_ShouldReturnError()
        {
            // Arrange
            var categoria = await CreateTestCategoriaIngredienteAsync("CategoriaConDipendenti");

            // Crea un ingrediente collegato alla categoria
            var ingrediente = new Ingrediente
            {
                Ingrediente1 = "TestIngrediente",
                CategoriaId = categoria.CategoriaId,
                PrezzoAggiunto = 1.00m,
                Disponibile = true,
                DataInserimento = DateTime.UtcNow
            };
            _context.Ingrediente.Add(ingrediente);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.DeleteAsync(categoria.CategoriaId);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Contains("Impossibile eliminare la categoria perché ci sono ingredienti collegati", result.Message);

            // Verifica che la categoria non sia stata eliminata
            var categoriaEsisteAncora = await _context.CategoriaIngrediente.FindAsync(categoria.CategoriaId);
            Assert.NotNull(categoriaEsisteAncora);
        }

        #endregion

        #region Private Helper Methods Tests

        [Fact]
        public async Task HasDependenciesAsync_WithIngredients_ShouldReturnTrue()
        {
            // Arrange
            var categoria = await CreateTestCategoriaIngredienteAsync("TestDependencies");

            var ingrediente = new Ingrediente
            {
                Ingrediente1 = "IngredienteTest",
                CategoriaId = categoria.CategoriaId,
                PrezzoAggiunto = 1.50m,
                Disponibile = true,
                DataInserimento = DateTime.UtcNow
            };
            _context.Ingrediente.Add(ingrediente);
            await _context.SaveChangesAsync();

            // Act - Usa reflection per testare il metodo privato
            var method = typeof(CategoriaIngredienteRepository)
                .GetMethod("HasDependenciesAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            var task = (Task<bool>)method!.Invoke(_repository, [categoria.CategoriaId])!;
            var result = await task;

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task HasDependenciesAsync_WithoutIngredients_ShouldReturnFalse()
        {
            // Arrange
            var categoria = await CreateTestCategoriaIngredienteAsync("TestNoDependencies");

            // Act - Usa reflection per testare il metodo privato
            var method = typeof(CategoriaIngredienteRepository)
                .GetMethod("HasDependenciesAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            var task = (Task<bool>)method!.Invoke(_repository, [categoria.CategoriaId])!;
            var result = await task;

            // Assert
            Assert.False(result);
        }

        #endregion

        #region Integration Tests

        [Fact]
        public async Task FullCrudFlow_ShouldWorkCorrectly()
        {
            // 1. CREATE
            var addDto = new CategoriaIngredienteDTO { Categoria = "FlowTest" };
            var addResult = await _repository.AddAsync(addDto);
            Assert.True(addResult.Success);
            var categoriaId = addResult.Data!.CategoriaId;

            // 2. READ by ID
            var getResult = await _repository.GetByIdAsync(categoriaId);
            Assert.True(getResult.Success);
            Assert.Equal("FlowTest", getResult.Data!.Categoria);

            // 3. EXISTS
            var existsResult = await _repository.ExistsAsync(categoriaId);
            Assert.True(existsResult.Success);
            Assert.True(existsResult.Data);

            // 4. UPDATE
            var updateDto = new CategoriaIngredienteDTO
            {
                CategoriaId = categoriaId,
                Categoria = "FlowTestUpdated"
            };
            var updateResult = await _repository.UpdateAsync(updateDto);
            Assert.True(updateResult.Success);
            Assert.True(updateResult.Data);

            // 5. Verify update
            var verifyResult = await _repository.GetByIdAsync(categoriaId);
            Assert.Equal("FlowTestUpdated", verifyResult.Data!.Categoria);

            // 6. DELETE
            var deleteResult = await _repository.DeleteAsync(categoriaId);
            Assert.True(deleteResult.Success);
            Assert.True(deleteResult.Data);

            // 7. Verify deletion
            var finalExistsResult = await _repository.ExistsAsync(categoriaId);
            Assert.False(finalExistsResult.Data);
        }

        #endregion
    }
}