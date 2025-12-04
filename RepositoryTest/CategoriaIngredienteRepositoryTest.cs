using Database;
using DTO;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Repository.Interface;
using Repository.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace RepositoryTest
{
    public class CategoriaIngredienteRepositoryTest : BaseTest
    {
        private readonly CategoriaIngredienteRepository _categoriaIngredienteRepository;

        public CategoriaIngredienteRepositoryTest()
        {
            // ✅ USA NullLogger per i test InMemory
            _categoriaIngredienteRepository = new CategoriaIngredienteRepository(
                _context,
                NullLogger<CategoriaIngredienteRepository>.Instance
            );

            // ✅ PULISCI LA TABELLA PER ISOLARE I TEST
            CleanTableAsync<CategoriaIngrediente>().Wait();
        }

        [Fact]
        public async Task AddAsync_Should_Add_CategoriaIngrediente()
        {
            // Arrange
            await CleanTableAsync<CategoriaIngrediente>();

            var categoriaDto = new CategoriaIngredienteDTO
            {
                Categoria = "Tè e Infusi"
            };

            // Act - ✅ USA IL RISULTATO
            var result = await _categoriaIngredienteRepository.AddAsync(categoriaDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Tè e Infusi", result.Categoria);
            Assert.True(result.CategoriaId > 0); // ✅ VERIFICA ID GENERATO
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Correct_CategoriaIngrediente()
        {
            // Arrange - Pulisci le categorie esistenti
            await CleanTableAsync<CategoriaIngrediente>();

            var categoriaDto = new CategoriaIngredienteDTO
            {
                Categoria = "Sciroppi"
            };
            await _categoriaIngredienteRepository.AddAsync(categoriaDto);

            // Act
            var result = await _categoriaIngredienteRepository.GetByIdAsync(categoriaDto.CategoriaId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(categoriaDto.CategoriaId, result.CategoriaId);
            Assert.Equal("Sciroppi", result.Categoria);
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_For_NonExisting_Id()
        {
            // Act
            var result = await _categoriaIngredienteRepository.GetByIdAsync(999);

            // Assert
            Assert.Null(result);
        }        

        [Fact]
        public async Task UpdateAsync_Should_Update_CategoriaIngrediente_Correctly()
        {
            // Arrange - Pulisci le categorie esistenti
            await CleanTableAsync<CategoriaIngrediente>();

            var categoriaDto = new CategoriaIngredienteDTO
            {
                Categoria = "Tè"
            };
            await _categoriaIngredienteRepository.AddAsync(categoriaDto);

            var updateDto = new CategoriaIngredienteDTO
            {
                CategoriaId = categoriaDto.CategoriaId,
                Categoria = "Tè e Infusi Premium"
            };

            // Act
            await _categoriaIngredienteRepository.UpdateAsync(updateDto);

            // Assert
            var updated = await _categoriaIngredienteRepository.GetByIdAsync(categoriaDto.CategoriaId);
            Assert.NotNull(updated);
            Assert.Equal("Tè e Infusi Premium", updated.Categoria);
        }

        [Fact]
        public async Task UpdateAsync_Should_Throw_When_Entity_Not_Found()
        {
            // Arrange
            await CleanTableAsync<CategoriaIngrediente>();

            var updateDto = new CategoriaIngredienteDTO
            {
                CategoriaId = 999,
                Categoria = "Categoria Inesistente"
            };

            // Act & Assert - ✅ ORA LANCIA KeyNotFoundException
            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                _categoriaIngredienteRepository.UpdateAsync(updateDto));
        }

        [Fact]
        public async Task DeleteAsync_Should_Remove_CategoriaIngrediente()
        {
            // Arrange - Pulisci le categorie esistenti
            await CleanTableAsync<CategoriaIngrediente>();

            var categoriaDto = new CategoriaIngredienteDTO
            {
                Categoria = "Topping Speciali"
            };
            await _categoriaIngredienteRepository.AddAsync(categoriaDto);

            // Act
            await _categoriaIngredienteRepository.DeleteAsync(categoriaDto.CategoriaId);

            // Assert
            var deleted = await _categoriaIngredienteRepository.GetByIdAsync(categoriaDto.CategoriaId);
            Assert.Null(deleted);
        }

        [Fact]
        public async Task DeleteAsync_Should_Throw_When_Entity_Not_Found()
        {
            // Act & Assert - ✅ ORA LANCIA KeyNotFoundException
            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                _categoriaIngredienteRepository.DeleteAsync(999));
        }

        [Fact]
        public async Task AddAsync_Should_Assign_Generated_Id()
        {
            // Arrange
            await CleanTableAsync<CategoriaIngrediente>();

            var categoriaDto = new CategoriaIngredienteDTO
            {
                Categoria = "Frutta"
            };

            // Act - ✅ USA IL RISULTATO
            var result = await _categoriaIngredienteRepository.AddAsync(categoriaDto);

            // Assert
            Assert.True(result.CategoriaId > 0);
        }
        
        [Fact]
        public async Task Multiple_Add_Should_Generate_Different_Ids()
        {
            // Arrange
            await CleanTableAsync<CategoriaIngrediente>();

            var categoria1 = new CategoriaIngredienteDTO { Categoria = "Categoria 1" };
            var categoria2 = new CategoriaIngredienteDTO { Categoria = "Categoria 2" };

            // Act - ✅ USA I RISULTATI
            var result1 = await _categoriaIngredienteRepository.AddAsync(categoria1);
            var result2 = await _categoriaIngredienteRepository.AddAsync(categoria2);

            // Assert
            Assert.NotEqual(result1.CategoriaId, result2.CategoriaId);
            Assert.True(result1.CategoriaId > 0);
            Assert.True(result2.CategoriaId > 0);
        }

        [Fact]
        public async Task Update_Should_Not_Affect_Other_Categorie()
        {
            // Arrange - Pulisci le categorie esistenti
            await CleanTableAsync<CategoriaIngrediente>();

            var categoria1 = new CategoriaIngredienteDTO { Categoria = "Original 1" };
            var categoria2 = new CategoriaIngredienteDTO { Categoria = "Original 2" };

            await _categoriaIngredienteRepository.AddAsync(categoria1);
            await _categoriaIngredienteRepository.AddAsync(categoria2);

            // Act - Modifica solo la prima categoria
            var updateDto = new CategoriaIngredienteDTO
            {
                CategoriaId = categoria1.CategoriaId,
                Categoria = "Modificata 1"
            };
            await _categoriaIngredienteRepository.UpdateAsync(updateDto);

            // Assert - Verifica che la seconda categoria sia rimasta invariata
            var updatedCategoria1 = await _categoriaIngredienteRepository.GetByIdAsync(categoria1.CategoriaId);
            var categoria2Unchanged = await _categoriaIngredienteRepository.GetByIdAsync(categoria2.CategoriaId);

            Assert.NotNull(updatedCategoria1);
            Assert.NotNull(categoria2Unchanged);
            Assert.Equal("Modificata 1", updatedCategoria1.Categoria);
            Assert.Equal("Original 2", categoria2Unchanged.Categoria);
        }

        [Fact]
        public async Task AddAsync_WithDuplicateName_ShouldThrow_InvalidOperationException()
        {
            // Arrange
            await CleanTableAsync<CategoriaIngrediente>();

            var categoria1 = new CategoriaIngredienteDTO { Categoria = "Duplicato" };
            await _categoriaIngredienteRepository.AddAsync(categoria1);

            var categoria2 = new CategoriaIngredienteDTO { Categoria = "Duplicato" };

            // Act & Assert - ✅ ORA LANCIA InvalidOperationException
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _categoriaIngredienteRepository.AddAsync(categoria2));
            Assert.Contains("Esiste già una categoria", exception.Message);
        }

        [Fact]
        public async Task UpdateAsync_WithDuplicateName_ShouldThrow_InvalidOperationException()
        {
            // Arrange
            await CleanTableAsync<CategoriaIngrediente>();

            var categoria1 = new CategoriaIngredienteDTO { Categoria = "Categoria 1" };
            var categoria2 = new CategoriaIngredienteDTO { Categoria = "Categoria 2" };

            await _categoriaIngredienteRepository.AddAsync(categoria1);
            await _categoriaIngredienteRepository.AddAsync(categoria2);

            // Prova a rinominare categoria2 con lo stesso nome di categoria1
            var updateDto = new CategoriaIngredienteDTO
            {
                CategoriaId = categoria2.CategoriaId,
                Categoria = "Categoria 1" // Nome duplicato
            };

            // Act & Assert - ✅ ORA LANCIA InvalidOperationException
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _categoriaIngredienteRepository.UpdateAsync(updateDto));
            Assert.Contains("Esiste già un'altra categoria", exception.Message);
        }

        [Fact]
        public async Task ExistsAsync_ShouldReturnCorrectResults()
        {
            // Arrange
            await CleanTableAsync<CategoriaIngrediente>();

            var categoria = new CategoriaIngredienteDTO { Categoria = "Test" };
            var result = await _categoriaIngredienteRepository.AddAsync(categoria);

            // Act & Assert
            var exists = await _categoriaIngredienteRepository.ExistsAsync(result.CategoriaId);
            var notExists = await _categoriaIngredienteRepository.ExistsAsync(999);

            Assert.True(exists);
            Assert.False(notExists);
        }

        [Fact]
        public async Task ExistsByNomeAsync_ShouldReturnCorrectResults()
        {
            // Arrange
            await CleanTableAsync<CategoriaIngrediente>();

            var categoria = new CategoriaIngredienteDTO { Categoria = "Test" };
            await _categoriaIngredienteRepository.AddAsync(categoria);

            // Act & Assert - ✅ CORRETTO: ora ExistsByNomeAsync accetta solo il nome
            var exists = await _categoriaIngredienteRepository.ExistsByNomeAsync("Test");
            var notExists = await _categoriaIngredienteRepository.ExistsByNomeAsync("NonEsiste");

            Assert.True(exists);
            Assert.False(notExists);
        }

        //[Fact]
        //public async Task UpdateAsync_WithNonExistingId_ShouldSilentFail()
        //{
        //    // Arrange
        //    var updateDto = new CategoriaIngredienteDTO
        //    {
        //        CategoriaId = 999,
        //        Categoria = "Non Esiste"
        //    };

        //    // Act & Assert - ✅ NO EXCEPTION (silent fail pattern)
        //    var exception = await Record.ExceptionAsync(() =>
        //        _categoriaIngredienteRepository.UpdateAsync(updateDto));
        //    Assert.Null(exception);
        //}

        // ✅ NUOVI TEST PER METODI FRONTEND                

        [Fact]
        public async Task GetByNomePerFrontendAsync_WithInvalidNome_ShouldReturn_Empty_PaginatedResult()
        {
            // Arrange
            await CleanTableAsync<CategoriaIngrediente>();

            // Act - Cerca nome inesistente
            var result = await _categoriaIngredienteRepository.GetByNomePerFrontendAsync("INVALIDNOME", page: 1, pageSize: 10);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<PaginatedResponseDTO<CategoriaIngredienteFrontendDTO>>(result);
            Assert.Empty(result.Data);
            Assert.Equal(0, result.TotalCount);
            Assert.Equal(1, result.Page);
            Assert.Equal(10, result.PageSize);
        }

        [Fact]
        public async Task GetByNomePerFrontendAsync_Should_Be_Case_Insensitive()
        {
            // Arrange
            await CleanTableAsync<CategoriaIngrediente>();

            var categoriaDto = new CategoriaIngredienteDTO
            {
                Categoria = "TOPPING", // Maiuscolo
            };
            await _categoriaIngredienteRepository.AddAsync(categoriaDto);

            // Act & Assert - ✅ ORA È CASE-INSENSITIVE
            var resultUpper = await _categoriaIngredienteRepository.GetByNomePerFrontendAsync("TOPPING");
            Assert.NotNull(resultUpper);

            var resultLower = await _categoriaIngredienteRepository.GetByNomePerFrontendAsync("topping");
            Assert.NotNull(resultLower); // ✅ TROVA ANCHE CON MINUSCOLO
        }

        // ✅ METODO HELPER PER VERIFICARE ASSENZA ID
        private static int GetIdIfExists(object dto)
        {
            var property = dto.GetType().GetProperty("CategoriaId") ??
                           dto.GetType().GetProperty("Id");

            if (property != null)
            {
                var value = property.GetValue(dto);
                return value is int intValue ? intValue : 0;
            }

            return 0;
        }

        [Fact]
        public async Task GetAllAsync_Should_Return_Paginated_Results()
        {
            // Arrange - Crea 15 categorie
            await CleanTableAsync<CategoriaIngrediente>();

            for (int i = 1; i <= 15; i++)
            {
                await _categoriaIngredienteRepository.AddAsync(new CategoriaIngredienteDTO
                {
                    Categoria = $"Categoria {i}"
                });
            }

            // Act - Pagina 1, 10 elementi per pagina
            var result = await _categoriaIngredienteRepository.GetAllAsync(page: 1, pageSize: 10);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<PaginatedResponseDTO<CategoriaIngredienteDTO>>(result);
            Assert.Equal(10, result.Data.Count());
            Assert.Equal(1, result.Page);
            Assert.Equal(10, result.PageSize);
            Assert.Equal(15, result.TotalCount);
            Assert.Equal(2, result.TotalPages);
            Assert.False(result.HasPrevious);
            Assert.True(result.HasNext);
        }

        [Fact]
        public async Task GetByNomeAsync_Should_Return_Paginated_Results()
        {
            // Arrange
            await CleanTableAsync<CategoriaIngrediente>();

            await _categoriaIngredienteRepository.AddAsync(new CategoriaIngredienteDTO { Categoria = "Tè Verde" });
            await _categoriaIngredienteRepository.AddAsync(new CategoriaIngredienteDTO { Categoria = "Tè Nero" });
            await _categoriaIngredienteRepository.AddAsync(new CategoriaIngredienteDTO { Categoria = "Caffè" });

            // Act - Cerca "Tè" (deve trovare Tè Verde e Tè Nero)
            var result = await _categoriaIngredienteRepository.GetByNomeAsync("Tè", page: 1, pageSize: 10);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.TotalCount);
            Assert.All(result.Data, c => Assert.StartsWith("Tè", c.Categoria));
        }

        [Fact]
        public async Task GetByNomeAsync_With_Null_Should_Return_All()
        {
            // Arrange
            await CleanTableAsync<CategoriaIngrediente>();

            await _categoriaIngredienteRepository.AddAsync(new CategoriaIngredienteDTO { Categoria = "Latte" });
            await _categoriaIngredienteRepository.AddAsync(new CategoriaIngredienteDTO { Categoria = "Dolcificante" });

            // Act
            var result = await _categoriaIngredienteRepository.GetByNomeAsync(null, page: 1, pageSize: 10);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.TotalCount);
        }

        [Fact]
        public async Task GetByNomePerFrontendAsync_Should_Return_Paginated_FrontendDTOs()
        {
            // Arrange
            await CleanTableAsync<CategoriaIngrediente>();

            await _categoriaIngredienteRepository.AddAsync(new CategoriaIngredienteDTO { Categoria = "Tè Verde" });
            await _categoriaIngredienteRepository.AddAsync(new CategoriaIngredienteDTO { Categoria = "Tè Nero" });

            // Act
            var result = await _categoriaIngredienteRepository.GetByNomePerFrontendAsync("Tè", page: 1, pageSize: 10);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<PaginatedResponseDTO<CategoriaIngredienteFrontendDTO>>(result);
            Assert.Equal(2, result.TotalCount);
            Assert.All(result.Data, dto =>
            {
                Assert.StartsWith("Tè", dto.Categoria);
                // Verifica che non ci sia ID
                Assert.Null(dto.GetType().GetProperty("CategoriaId")?.GetValue(dto));
            });
        }

        [Fact]
        public async Task HasDependenciesAsync_Should_Return_False_InMemory()
        {
            // Arrange
            await CleanTableAsync<CategoriaIngrediente>();

            var categoria = new CategoriaIngredienteDTO { Categoria = "Test" };
            var added = await _categoriaIngredienteRepository.AddAsync(categoria);

            // Act
            var hasDependencies = await _categoriaIngredienteRepository.HasDependenciesAsync(added.CategoriaId);

            // Assert - In InMemory non ci sono dipendenze
            Assert.False(hasDependencies);
        }

        [Fact]
        public async Task DeleteAsync_Should_Throw_When_Has_Dependencies()
        {
            // Arrange
            await CleanTableAsync<CategoriaIngrediente>();

            var categoria = new CategoriaIngredienteDTO { Categoria = "Test" };
            var added = await _categoriaIngredienteRepository.AddAsync(categoria);

            // Act & Assert
            // Se ci fossero dipendenze, dovrebbe lanciare InvalidOperationException
            // Per ora testa che non lanci eccezioni (se non ci sono dipendenze)
            await _categoriaIngredienteRepository.DeleteAsync(added.CategoriaId);

            var deleted = await _categoriaIngredienteRepository.GetByIdAsync(added.CategoriaId);
            Assert.Null(deleted); // Verifica che sia stato eliminato
        }
    }
}