using DTO;
using Repository.Interface;
using Repository.Service;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace RepositoryTest
{
    public class TavoloRepositoryTest : BaseTest
    {
        private readonly ITavoloRepository _tavoloRepository;

        public TavoloRepositoryTest()
        {
            // ✅ CREA IL REPOSITORY SPECIFICO USANDO IL CONTEXT EREDITATO
            _tavoloRepository = new TavoloRepository(_context);
        }

        [Fact]
        public async Task AddAsync_Should_Add_Tavolo()
        {
            // Arrange
            var tavoloDto = new TavoloDTO
            {
                Numero = 1,
                Zona = "Terrazza",
                Disponibile = true
            };

            // Act
            await _tavoloRepository.AddAsync(tavoloDto);

            // Assert
            var result = await _tavoloRepository.GetByIdAsync(tavoloDto.TavoloId);
            Assert.NotNull(result);
            Assert.Equal(1, result.Numero);
            Assert.Equal("Terrazza", result.Zona);
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Correct_Tavolo()
        {
            // Arrange
            var tavoloDto = new TavoloDTO
            {
                Numero = 2,
                Zona = "Interno",
                Disponibile = true
            };
            await _tavoloRepository.AddAsync(tavoloDto);

            // Act
            var result = await _tavoloRepository.GetByIdAsync(tavoloDto.TavoloId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(tavoloDto.TavoloId, result.TavoloId);
            Assert.Equal("Interno", result.Zona);
        }

        [Fact]
        public async Task GetByNumeroAsync_Should_Return_Correct_Tavolo()
        {
            // Arrange
            var tavoloDto = new TavoloDTO
            {
                Numero = 3,
                Zona = "Terrazza",
                Disponibile = true
            };
            await _tavoloRepository.AddAsync(tavoloDto);

            // Act
            var result = await _tavoloRepository.GetByNumeroAsync(3);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, result.Numero);
            Assert.Equal("Terrazza", result.Zona);
        }

        [Fact]
        public async Task GetDisponibiliAsync_Should_Return_Only_Available_Tables()
        {
            // Arrange
            var tavoli = new List<TavoloDTO>
            {
                new TavoloDTO { Numero = 4, Zona = "Interno", Disponibile = true },
                new TavoloDTO { Numero = 5, Zona = "Terrazza", Disponibile = false },
                new TavoloDTO { Numero = 6, Zona = "Interno", Disponibile = true }
            };

            foreach (var tavolo in tavoli)
            {
                await _tavoloRepository.AddAsync(tavolo);
            }

            // Act
            var result = await _tavoloRepository.GetDisponibiliAsync();

            // Assert
            Assert.Equal(2, result.Count());
            Assert.All(result, t => Assert.True(t.Disponibile));
        }

        [Fact]
        public async Task UpdateAsync_Should_Update_Tavolo_Correctly()
        {
            // Arrange
            var tavoloDto = new TavoloDTO
            {
                Numero = 7,
                Zona = "Interno",
                Disponibile = true
            };
            await _tavoloRepository.AddAsync(tavoloDto);

            var updateDto = new TavoloDTO
            {
                TavoloId = tavoloDto.TavoloId,
                Numero = 77,
                Zona = "Terrazza",
                Disponibile = false
            };

            // Act
            await _tavoloRepository.UpdateAsync(updateDto);

            // Assert
            var updated = await _tavoloRepository.GetByIdAsync(tavoloDto.TavoloId);
            Assert.NotNull(updated);
            Assert.Equal(77, updated.Numero);
            Assert.Equal("Terrazza", updated.Zona);
            Assert.False(updated.Disponibile);
        }

        [Fact]
        public async Task DeleteAsync_Should_Remove_Tavolo()
        {
            // Arrange
            var tavoloDto = new TavoloDTO
            {
                Numero = 8,
                Zona = "Interno",
                Disponibile = true
            };
            await _tavoloRepository.AddAsync(tavoloDto);

            // Act
            await _tavoloRepository.DeleteAsync(tavoloDto.TavoloId);

            // Assert
            var deleted = await _tavoloRepository.GetByIdAsync(tavoloDto.TavoloId);
            Assert.Null(deleted);
        }

        [Fact]
        public async Task NumeroExistsAsync_Should_Check_Number_Uniqueness()
        {
            // Arrange
            var tavoloDto = new TavoloDTO
            {
                Numero = 9,
                Zona = "Interno",
                Disponibile = true
            };
            await _tavoloRepository.AddAsync(tavoloDto);

            // Act & Assert
            var exists = await _tavoloRepository.NumeroExistsAsync(9);
            Assert.True(exists);

            var notExists = await _tavoloRepository.NumeroExistsAsync(99);
            Assert.False(notExists);
        }
    }
}