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
                QrCode = "QR001",
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
                QrCode = "QR002",
                Disponibile = true
            };
            await _tavoloRepository.AddAsync(tavoloDto);

            // Act
            var result = await _tavoloRepository.GetByIdAsync(tavoloDto.TavoloId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(tavoloDto.TavoloId, result.TavoloId);
            Assert.Equal("QR002", result.QrCode);
        }

        [Fact]
        public async Task GetByQrCodeAsync_Should_Return_Correct_Tavolo()
        {
            // Arrange
            var tavoloDto = new TavoloDTO
            {
                Numero = 3,
                Zona = "Terrazza",
                QrCode = "QR003",
                Disponibile = true
            };
            await _tavoloRepository.AddAsync(tavoloDto);

            // Act
            var result = await _tavoloRepository.GetByQrCodeAsync("QR003");

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
                new TavoloDTO { Numero = 4, Zona = "Interno", QrCode = "QR004", Disponibile = true },
                new TavoloDTO { Numero = 5, Zona = "Terrazza", QrCode = "QR005", Disponibile = false },
                new TavoloDTO { Numero = 6, Zona = "Interno", QrCode = "QR006", Disponibile = true }
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
                QrCode = "QR007",
                Disponibile = true
            };
            await _tavoloRepository.AddAsync(tavoloDto);

            var updateDto = new TavoloDTO
            {
                TavoloId = tavoloDto.TavoloId,
                Numero = 77,
                Zona = "Terrazza",
                QrCode = "QR0077",
                Disponibile = false
            };

            // Act
            await _tavoloRepository.UpdateAsync(updateDto);

            // Assert
            var updated = await _tavoloRepository.GetByIdAsync(tavoloDto.TavoloId);
            Assert.NotNull(updated);
            Assert.Equal(77, updated.Numero);
            Assert.Equal("Terrazza", updated.Zona);
            Assert.Equal("QR0077", updated.QrCode);
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
                QrCode = "QR008",
                Disponibile = true
            };
            await _tavoloRepository.AddAsync(tavoloDto);

            // Act
            await _tavoloRepository.DeleteAsync(tavoloDto.TavoloId);

            // Assert
            var deleted = await _tavoloRepository.GetByIdAsync(tavoloDto.TavoloId);
            Assert.Null(deleted);
        }
    }
}