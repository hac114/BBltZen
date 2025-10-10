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
    public class IngredienteRepositoryTest : IDisposable
    {
        private readonly IIngredienteRepository _ingredienteRepository;
        private readonly BubbleTeaContext _context;

        public IngredienteRepositoryTest()
        {
            var options = new DbContextOptionsBuilder<BubbleTeaContext>()
                .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
                .Options;

            _context = new BubbleTeaContext(options);
            _context.Database.EnsureCreated();
            _ingredienteRepository = new IngredienteRepository(_context);
        }

        public void Dispose()
        {
            _context?.Database?.EnsureDeleted();
            _context?.Dispose();
        }

        [Fact]
        public async Task AddAsync_Should_Add_Ingrediente()
        {
            // Arrange
            var ingredienteDto = new IngredienteDTO
            {
                Nome = "Tapioca",
                CategoriaId = 1,
                PrezzoAggiunto = 0.50m,
                Disponibile = true  // 👈 ASSICURATI che sia true
            };

            // Act
            await _ingredienteRepository.AddAsync(ingredienteDto);

            // Assert - Verifica direttamente dal database per evitare problemi di filtro
            var result = await _context.Ingrediente.FindAsync(ingredienteDto.IngredienteId);
            Assert.NotNull(result);
            Assert.Equal("Tapioca", result.Ingrediente1);
            Assert.Equal(1, result.CategoriaId);
            Assert.True(result.Disponibile);
        }        

        [Fact]
        public async Task GetByIdAsync_Should_Return_Correct_Ingrediente()
        {
            // Arrange
            var ingredienteDto = new IngredienteDTO
            {
                Nome = "Sciroppo di vaniglia",
                CategoriaId = 2,
                PrezzoAggiunto = 0.30m,
                Disponibile = true  // 👈 ASSICURATI che sia true
            };
            await _ingredienteRepository.AddAsync(ingredienteDto);

            // Act
            var result = await _ingredienteRepository.GetByIdAsync(ingredienteDto.IngredienteId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(ingredienteDto.IngredienteId, result.IngredienteId);
            Assert.Equal("Sciroppo di vaniglia", result.Nome);
            Assert.True(result.Disponibile);  // 👈 AGGIUNGI questa verifica
        }

        [Fact]
        public async Task GetAllAsync_Should_Return_Only_Available_Ingredienti()
        {
            // Arrange
            var ingredienti = new List<IngredienteDTO>
            {
                new IngredienteDTO { Nome = "Tè verde", CategoriaId = 1, PrezzoAggiunto = 0.50m, Disponibile = true },
                new IngredienteDTO { Nome = "Tè nero", CategoriaId = 1, PrezzoAggiunto = 0.50m, Disponibile = false },
                new IngredienteDTO { Nome = "Latte", CategoriaId = 2, PrezzoAggiunto = 0.30m, Disponibile = true }
            };

            foreach (var ingrediente in ingredienti)
            {
                await _ingredienteRepository.AddAsync(ingrediente);
            }

            // Act
            var result = await _ingredienteRepository.GetAllAsync();

            // Assert
            Assert.Equal(2, result.Count());
            Assert.All(result, i => Assert.True(i.Disponibile));
        }

        [Fact]
        public async Task GetByCategoriaAsync_Should_Return_All_Ingredienti_Of_Category()
        {
            // Arrange
            var categoriaId = 1;
            var ingredienti = new List<IngredienteDTO>
            {
                new IngredienteDTO { Nome = "Tè verde", CategoriaId = categoriaId, PrezzoAggiunto = 0.50m, Disponibile = true },
                new IngredienteDTO { Nome = "Tè nero", CategoriaId = categoriaId, PrezzoAggiunto = 0.50m, Disponibile = false },
                new IngredienteDTO { Nome = "Latte", CategoriaId = 2, PrezzoAggiunto = 0.30m, Disponibile = true }
            };

            foreach (var ingrediente in ingredienti)
            {
                await _ingredienteRepository.AddAsync(ingrediente);
            }

            // Act
            var result = await _ingredienteRepository.GetByCategoriaAsync(categoriaId);

            // Assert
            Assert.Equal(2, result.Count());
            Assert.All(result, i => Assert.Equal(categoriaId, i.CategoriaId));
        }

        [Fact]
        public async Task GetDisponibiliAsync_Should_Return_Only_Available_Ingredienti()
        {
            // Arrange
            var ingredienti = new List<IngredienteDTO>
            {
                new IngredienteDTO { Nome = "Tè verde", CategoriaId = 1, PrezzoAggiunto = 0.50m, Disponibile = true },
                new IngredienteDTO { Nome = "Tè nero", CategoriaId = 1, PrezzoAggiunto = 0.50m, Disponibile = false },
                new IngredienteDTO { Nome = "Latte", CategoriaId = 2, PrezzoAggiunto = 0.30m, Disponibile = true }
            };

            foreach (var ingrediente in ingredienti)
            {
                await _ingredienteRepository.AddAsync(ingrediente);
            }

            // Act
            var result = await _ingredienteRepository.GetDisponibiliAsync();

            // Assert
            Assert.Equal(2, result.Count());
            Assert.All(result, i => Assert.True(i.Disponibile));
        }

        [Fact]
        public async Task UpdateAsync_Should_Update_Ingrediente_Correctly()
        {
            // Arrange
            var ingredienteDto = new IngredienteDTO
            {
                Nome = "Tè verde",
                CategoriaId = 1,
                PrezzoAggiunto = 0.50m,
                Disponibile = true
            };
            await _ingredienteRepository.AddAsync(ingredienteDto);

            var updateDto = new IngredienteDTO
            {
                IngredienteId = ingredienteDto.IngredienteId,
                Nome = "Tè verde premium",
                CategoriaId = 2,
                PrezzoAggiunto = 0.80m,
                Disponibile = false  // 👈 Imposta a false per testare il soft-delete
            };

            // Act
            await _ingredienteRepository.UpdateAsync(updateDto);

            // Assert - Dopo l'update a Disponibile=false, GetByIdAsync dovrebbe restituire null
            var updated = await _ingredienteRepository.GetByIdAsync(ingredienteDto.IngredienteId);
            Assert.Null(updated);

            // Verifica che l'ingrediente sia ancora nel database ma con Disponibile=false
            var stillInDb = await _context.Ingrediente.FindAsync(ingredienteDto.IngredienteId);
            Assert.NotNull(stillInDb);
            Assert.False(stillInDb.Disponibile);
            Assert.Equal("Tè verde premium", stillInDb.Ingrediente1);
        }

        [Fact]
        public async Task DeleteAsync_Should_SoftDelete_Ingrediente()
        {
            // Arrange
            var ingredienteDto = new IngredienteDTO
            {
                Nome = "Tè matcha",
                CategoriaId = 1,
                PrezzoAggiunto = 1.00m,
                Disponibile = true
            };
            await _ingredienteRepository.AddAsync(ingredienteDto);

            // Act
            await _ingredienteRepository.DeleteAsync(ingredienteDto.IngredienteId);

            // Assert - SOFT DELETE: non è più visibile nelle query normali
            var deleted = await _ingredienteRepository.GetByIdAsync(ingredienteDto.IngredienteId);
            Assert.Null(deleted);

            // Ma dovrebbe essere ancora nel database (verifica con query diretta)
            var stillInDb = await _context.Ingrediente.FindAsync(ingredienteDto.IngredienteId);
            Assert.NotNull(stillInDb);
            Assert.False(stillInDb.Disponibile);
        }

        [Fact]
        public async Task ExistsAsync_Should_Return_True_For_Existing_Ingrediente()
        {
            // Arrange
            var ingredienteDto = new IngredienteDTO
            {
                Nome = "Sciroppo di cocco",
                CategoriaId = 2,
                PrezzoAggiunto = 0.40m,
                Disponibile = true
            };
            await _ingredienteRepository.AddAsync(ingredienteDto);

            // Act
            var exists = await _ingredienteRepository.ExistsAsync(ingredienteDto.IngredienteId);

            // Assert
            Assert.True(exists);
        }

        [Fact]
        public async Task ExistsAsync_Should_Return_False_For_NonExisting_Ingrediente()
        {
            // Act
            var exists = await _ingredienteRepository.ExistsAsync(999);

            // Assert
            Assert.False(exists);
        }
    }
}