using DTO;
using Repository.Interface;
using Assert = Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace RepositoryTest
{
    [TestClass]
    public class TavoloRepositoryTest : BaseTest
    {
        private ITavoloRepository _tavoloRepository;

        [TestInitialize]
        public void Initialize()
        {
            _tavoloRepository = MockTavoloRepository; // Usa direttamente il mock
        }

        [TestMethod]
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
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Numero);
            Assert.AreEqual("Terrazza", result.Zona);
        }

        [TestMethod]
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
            Assert.IsNotNull(result);
            Assert.AreEqual(tavoloDto.TavoloId, result.TavoloId);
            Assert.AreEqual("QR002", result.QrCode);
        }

        [TestMethod]
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
            Assert.IsNotNull(result);
            Assert.AreEqual(3, result.Numero);
            Assert.AreEqual("Terrazza", result.Zona);
        }

        [TestMethod]
        public async Task GetDisponibiliAsync_Should_Return_Only_Available_Tables()
        {
            // Arrange
            var tavoli = new List<TavoloDTO>
            {
                new() { Numero = 4, Zona = "Interno", QrCode = "QR004", Disponibile = true },
                new() { Numero = 5, Zona = "Terrazza", QrCode = "QR005", Disponibile = false },
                new() { Numero = 6, Zona = "Interno", QrCode = "QR006", Disponibile = true }
            };

            foreach (var tavolo in tavoli)
            {
                await _tavoloRepository.AddAsync(tavolo);
            }

            // Act
            var result = await _tavoloRepository.GetDisponibiliAsync();

            // Assert
            Assert.AreEqual(2, result.Count());
            Assert.IsTrue(result.All(t => t.Disponibile));
        }

        [TestMethod]
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
            Assert.IsNotNull(updated);
            Assert.AreEqual(77, updated.Numero);
            Assert.AreEqual("Terrazza", updated.Zona);
            Assert.AreEqual("QR0077", updated.QrCode);
            Assert.IsFalse(updated.Disponibile);
        }

        [TestMethod]
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
            Assert.IsNull(deleted);
        }
    }
}