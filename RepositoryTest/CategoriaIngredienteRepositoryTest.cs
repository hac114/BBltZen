using DTO;
using Repository.Interface;
using Repository.Service;
using Database;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace RepositoryTest
{
    public class CategoriaIngredienteRepositoryTest : BaseTest
    {
        private readonly ICategoriaIngredienteRepository _categoriaIngredienteRepository;

        public CategoriaIngredienteRepositoryTest()
        {
            // ✅ SEMPLICE: BaseTest già fornisce _context (AppDbContext) inizializzato
            _categoriaIngredienteRepository = new CategoriaIngredienteRepository(_context);
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
        public async Task GetAllAsync_Should_Return_All_CategorieIngrediente()
        {
            // Arrange - Pulisci le categorie esistenti
            await CleanTableAsync<CategoriaIngrediente>();

            var categorieList = new List<CategoriaIngredienteDTO>
            {
                new CategoriaIngredienteDTO { Categoria = "Tè e Infusi" },
                new CategoriaIngredienteDTO { Categoria = "Sciroppi" },
                new CategoriaIngredienteDTO { Categoria = "Topping" },
                new CategoriaIngredienteDTO { Categoria = "Latte e Creamer" }
            };

            foreach (var categoria in categorieList)
            {
                await _categoriaIngredienteRepository.AddAsync(categoria);
            }

            // Act
            var result = await _categoriaIngredienteRepository.GetAllAsync();

            // Assert
            Assert.Equal(4, result.Count());
            Assert.Contains(result, c => c.Categoria == "Tè e Infusi");
            Assert.Contains(result, c => c.Categoria == "Sciroppi");
            Assert.Contains(result, c => c.Categoria == "Topping");
            Assert.Contains(result, c => c.Categoria == "Latte e Creamer");
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
        public async Task UpdateAsync_Should_Not_Throw_For_NonExisting_Id()
        {
            // Arrange - Pulisci le categorie esistenti
            await CleanTableAsync<CategoriaIngrediente>();

            var updateDto = new CategoriaIngredienteDTO
            {
                CategoriaId = 999,
                Categoria = "Categoria Inesistente"
            };

            // Act & Assert - Non dovrebbe lanciare eccezioni
            await _categoriaIngredienteRepository.UpdateAsync(updateDto);
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
        public async Task DeleteAsync_Should_Not_Throw_For_NonExisting_Id()
        {
            // Act & Assert - Non dovrebbe lanciare eccezioni
            await _categoriaIngredienteRepository.DeleteAsync(999);
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
        public async Task GetAllAsync_Should_Return_Empty_List_When_No_Data()
        {
            // Arrange - Pulisci tutte le categorie esistenti
            await CleanTableAsync<CategoriaIngrediente>();

            // Act
            var result = await _categoriaIngredienteRepository.GetAllAsync();

            // Assert
            Assert.Empty(result);
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
        public async Task AddAsync_WithDuplicateName_ShouldThrowArgumentException()
        {
            // Arrange
            await CleanTableAsync<CategoriaIngrediente>();

            var categoria1 = new CategoriaIngredienteDTO { Categoria = "Duplicato" };
            await _categoriaIngredienteRepository.AddAsync(categoria1);

            var categoria2 = new CategoriaIngredienteDTO { Categoria = "Duplicato" };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
                _categoriaIngredienteRepository.AddAsync(categoria2));
            Assert.Contains("Esiste già una categoria", exception.Message);
        }

        [Fact]
        public async Task UpdateAsync_WithDuplicateName_ShouldThrowArgumentException()
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

            // Act & Assert - ✅ ORA il repository gestisce il controllo
            var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
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

        [Fact]
        public async Task UpdateAsync_WithNonExistingId_ShouldSilentFail()
        {
            // Arrange
            var updateDto = new CategoriaIngredienteDTO
            {
                CategoriaId = 999,
                Categoria = "Non Esiste"
            };

            // Act & Assert - ✅ NO EXCEPTION (silent fail pattern)
            var exception = await Record.ExceptionAsync(() =>
                _categoriaIngredienteRepository.UpdateAsync(updateDto));
            Assert.Null(exception);
        }

        // ✅ NUOVI TEST PER METODI FRONTEND

        [Fact]
        public async Task GetAllPerFrontendAsync_ShouldReturnFrontendDTOs_WithoutIds()
        {
            // Arrange
            await CleanTableAsync<CategoriaIngrediente>();

            var categorieList = new List<CategoriaIngredienteDTO>
            {
                new CategoriaIngredienteDTO { Categoria = "Tè e Infusi" },
                new CategoriaIngredienteDTO { Categoria = "Sciroppi" },
                new CategoriaIngredienteDTO { Categoria = "Topping" }
            };

            foreach (var categoria in categorieList)
            {
                await _categoriaIngredienteRepository.AddAsync(categoria);
            }

            // Act
            var result = await _categoriaIngredienteRepository.GetAllPerFrontendAsync();

            // Assert
            var resultList = result.ToList();
            Assert.Equal(3, resultList.Count);

            // ✅ VERIFICA CHE NON CI SIANO ID
            Assert.All(resultList, c => Assert.Equal(0, GetIdIfExists(c)));

            // ✅ VERIFICA I DATI
            var teUnita = resultList.First(c => c.Categoria == "Tè e Infusi");
            Assert.Equal("Tè e Infusi", teUnita.Categoria);

            var sciroppiUnita = resultList.First(c => c.Categoria == "Sciroppi");
            Assert.Equal("Sciroppi", sciroppiUnita.Categoria);
        }

        [Fact]
        public async Task GetAllPerFrontendAsync_ShouldReturnEmptyList_WhenNoData()
        {
            // Arrange
            await CleanTableAsync<CategoriaIngrediente>();

            // Act
            var result = await _categoriaIngredienteRepository.GetAllPerFrontendAsync();

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetByNomePerFrontendAsync_WithValidNome_ShouldReturnFrontendDTO()
        {
            // Arrange
            await CleanTableAsync<CategoriaIngrediente>();

            var categoriaDto = new CategoriaIngredienteDTO
            {
                Categoria = "Latte e Creamer"
            };
            await _categoriaIngredienteRepository.AddAsync(categoriaDto);

            // Act
            var result = await _categoriaIngredienteRepository.GetByNomePerFrontendAsync("Latte e Creamer");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Latte e Creamer", result.Categoria);

            // ✅ VERIFICA CHE NON CI SIA ID
            Assert.Equal(0, GetIdIfExists(result));
        }

        [Fact]
        public async Task GetByNomePerFrontendAsync_WithInvalidNome_ShouldReturnNull()
        {
            // Act
            var result = await _categoriaIngredienteRepository.GetByNomePerFrontendAsync("INVALID");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetByNomePerFrontendAsync_ShouldBeCaseSensitive()
        {
            // Arrange
            await CleanTableAsync<CategoriaIngrediente>();

            var categoriaDto = new CategoriaIngredienteDTO
            {
                Categoria = "TOPPING", // Maiuscolo
            };
            await _categoriaIngredienteRepository.AddAsync(categoriaDto);

            // Act & Assert - Dovrebbe trovare solo con nome esatto
            var resultUpper = await _categoriaIngredienteRepository.GetByNomePerFrontendAsync("TOPPING");
            Assert.NotNull(resultUpper);

            var resultLower = await _categoriaIngredienteRepository.GetByNomePerFrontendAsync("topping");
            Assert.Null(resultLower); // ❌ Case sensitive
        }

        // ✅ METODO HELPER PER VERIFICARE ASSENZA ID
        private int GetIdIfExists(object dto)
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
    }
}