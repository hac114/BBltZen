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

        // ✅ RIMOSSO Dispose() - BaseTest già lo gestisce automaticamente

        [Fact]
        public async Task AddAsync_Should_Add_CategoriaIngrediente()
        {
            // Arrange - Pulisci le categorie esistenti
            await CleanTableAsync<Database.CategoriaIngrediente>();

            var categoriaDto = new CategoriaIngredienteDTO
            {
                Categoria = "Tè e Infusi"
            };

            // Act
            await _categoriaIngredienteRepository.AddAsync(categoriaDto);

            // Assert
            var result = await _categoriaIngredienteRepository.GetByIdAsync(categoriaDto.CategoriaId);
            Assert.NotNull(result);
            Assert.Equal("Tè e Infusi", result.Categoria);
            Assert.True(result.CategoriaId > 0);
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Correct_CategoriaIngrediente()
        {
            // Arrange - Pulisci le categorie esistenti
            await CleanTableAsync<Database.CategoriaIngrediente>();

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
            await CleanTableAsync<Database.CategoriaIngrediente>();

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
            Assert.Equal(4, result.Count);
            Assert.Contains(result, c => c.Categoria == "Tè e Infusi");
            Assert.Contains(result, c => c.Categoria == "Sciroppi");
            Assert.Contains(result, c => c.Categoria == "Topping");
            Assert.Contains(result, c => c.Categoria == "Latte e Creamer");
        }

        [Fact]
        public async Task UpdateAsync_Should_Update_CategoriaIngrediente_Correctly()
        {
            // Arrange - Pulisci le categorie esistenti
            await CleanTableAsync<Database.CategoriaIngrediente>();

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
            await CleanTableAsync<Database.CategoriaIngrediente>();

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
            await CleanTableAsync<Database.CategoriaIngrediente>();

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
            // Arrange - Pulisci le categorie esistenti
            await CleanTableAsync<Database.CategoriaIngrediente>();

            var categoriaDto = new CategoriaIngredienteDTO
            {
                Categoria = "Frutta"
            };

            // Act
            await _categoriaIngredienteRepository.AddAsync(categoriaDto);

            // Assert
            Assert.True(categoriaDto.CategoriaId > 0);
        }

        [Fact]
        public async Task GetAllAsync_Should_Return_Empty_List_When_No_Data()
        {
            // Arrange - Pulisci tutte le categorie esistenti
            await CleanTableAsync<Database.CategoriaIngrediente>();

            // Act
            var result = await _categoriaIngredienteRepository.GetAllAsync();

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task Multiple_Add_Should_Generate_Different_Ids()
        {
            // Arrange - Pulisci le categorie esistenti
            await CleanTableAsync<Database.CategoriaIngrediente>();

            var categoria1 = new CategoriaIngredienteDTO { Categoria = "Categoria 1" };
            var categoria2 = new CategoriaIngredienteDTO { Categoria = "Categoria 2" };

            // Act
            await _categoriaIngredienteRepository.AddAsync(categoria1);
            await _categoriaIngredienteRepository.AddAsync(categoria2);

            // Assert
            Assert.NotEqual(categoria1.CategoriaId, categoria2.CategoriaId);
            Assert.True(categoria1.CategoriaId > 0);
            Assert.True(categoria2.CategoriaId > 0);
        }

        [Fact]
        public async Task Update_Should_Not_Affect_Other_Categorie()
        {
            // Arrange - Pulisci le categorie esistenti
            await CleanTableAsync<Database.CategoriaIngrediente>();

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
    }
}