using Database;
using DTO;
using Repository.Interface;
using Repository.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace RepositoryTest
{
    public class SessioniQrRepositoryTest : BaseTest
    {
        private readonly ISessioniQrRepository _sessioniQrRepository;

        public SessioniQrRepositoryTest()
        {
            // ✅ USA IL REPOSITORY GIA' CREATO IN BaseTest O CREANE UNO SPECIFICO
            _sessioniQrRepository = new SessioniQrRepository(_context);

            // Setup: aggiungi alcuni clienti per i test
            SetupClienti();
        }

        private void SetupClienti()
        {
            // Aggiungi clienti di test se non esistono
            if (!_context.Cliente.Any())
            {
                _context.Cliente.AddRange(
                    new Cliente { TavoloId = 1 },
                    new Cliente { TavoloId = 2 }
                );
                _context.SaveChanges();
            }
        }

        [Fact]
        public async Task AddAsync_Should_Add_SessioneQr()
        {
            // Arrange
            var sessioneDto = new SessioniQrDTO
            {
                ClienteId = 1,
                QrCode = "QR_TEST_001",
                DataScadenza = DateTime.Now.AddHours(1),
                Utilizzato = false
            };

            // Act
            await _sessioniQrRepository.AddAsync(sessioneDto);

            // Assert
            var result = await _sessioniQrRepository.GetByIdAsync(sessioneDto.SessioneId);
            Assert.NotNull(result);
            Assert.Equal("QR_TEST_001", result.QrCode);
            Assert.Equal(1, result.ClienteId);
            Assert.False(result.Utilizzato);
            Assert.NotNull(result.DataCreazione);
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Correct_SessioneQr()
        {
            // Arrange
            var sessioneDto = new SessioniQrDTO
            {
                ClienteId = 1,
                QrCode = "QR_TEST_002",
                DataScadenza = DateTime.Now.AddHours(2),
                Utilizzato = false
            };
            await _sessioniQrRepository.AddAsync(sessioneDto);

            // Act
            var result = await _sessioniQrRepository.GetByIdAsync(sessioneDto.SessioneId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(sessioneDto.SessioneId, result.SessioneId);
            Assert.Equal("QR_TEST_002", result.QrCode);
            Assert.Equal(1, result.ClienteId);
            Assert.False(result.Utilizzato);
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_For_NonExisting_Id()
        {
            // Act
            var result = await _sessioniQrRepository.GetByIdAsync(Guid.NewGuid());

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetByQrCodeAsync_Should_Return_Correct_SessioneQr()
        {
            // Arrange
            var sessioneDto = new SessioniQrDTO
            {
                ClienteId = 1,
                QrCode = "QR_UNIQUE_001",
                DataScadenza = DateTime.Now.AddHours(1),
                Utilizzato = false
            };
            await _sessioniQrRepository.AddAsync(sessioneDto);

            // Act
            var result = await _sessioniQrRepository.GetByQrCodeAsync("QR_UNIQUE_001");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("QR_UNIQUE_001", result.QrCode);
            Assert.Equal(sessioneDto.SessioneId, result.SessioneId);
        }

        [Fact]
        public async Task GetByQrCodeAsync_Should_Return_Null_For_NonExisting_QrCode()
        {
            // Act
            var result = await _sessioniQrRepository.GetByQrCodeAsync("QR_NON_EXISTENT");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetByClienteIdAsync_Should_Return_All_Sessioni_For_Cliente()
        {
            // Arrange
            var sessioniCliente1 = new List<SessioniQrDTO>
            {
                new SessioniQrDTO { ClienteId = 1, QrCode = "QR_CLIENTE1_001", DataScadenza = DateTime.Now.AddHours(1), Utilizzato = false },
                new SessioniQrDTO { ClienteId = 1, QrCode = "QR_CLIENTE1_002", DataScadenza = DateTime.Now.AddHours(2), Utilizzato = true }
            };

            var sessioniCliente2 = new List<SessioniQrDTO>
            {
                new SessioniQrDTO { ClienteId = 2, QrCode = "QR_CLIENTE2_001", DataScadenza = DateTime.Now.AddHours(1), Utilizzato = false }
            };

            foreach (var sessione in sessioniCliente1.Concat(sessioniCliente2))
            {
                await _sessioniQrRepository.AddAsync(sessione);
            }

            // Act
            var result = await _sessioniQrRepository.GetByClienteIdAsync(1);

            // Assert
            Assert.Equal(2, result.Count());
            Assert.All(result, s => Assert.Equal(1, s.ClienteId));
        }

        [Fact]
        public async Task GetNonutilizzateAsync_Should_Return_Only_NonUtilizzate_Sessioni()
        {
            // Arrange
            var sessioni = new List<SessioniQrDTO>
            {
                new SessioniQrDTO { ClienteId = 1, QrCode = "QR_NON_UTIL_001", DataScadenza = DateTime.Now.AddHours(1), Utilizzato = false },
                new SessioniQrDTO { ClienteId = 1, QrCode = "QR_UTIL_001", DataScadenza = DateTime.Now.AddHours(1), Utilizzato = true },
                new SessioniQrDTO { ClienteId = 2, QrCode = "QR_NON_UTIL_002", DataScadenza = DateTime.Now.AddHours(2), Utilizzato = null },
                new SessioniQrDTO { ClienteId = 2, QrCode = "QR_UTIL_002", DataScadenza = DateTime.Now.AddHours(2), Utilizzato = true }
            };

            foreach (var sessione in sessioni)
            {
                await _sessioniQrRepository.AddAsync(sessione);
            }

            // Act
            var result = await _sessioniQrRepository.GetNonutilizzateAsync();

            // Assert
            Assert.Equal(2, result.Count());
            Assert.All(result, s => Assert.True(s.Utilizzato == false || s.Utilizzato == null));
        }

        [Fact]
        public async Task GetScaduteAsync_Should_Return_Only_Scadute_Sessioni()
        {
            // Arrange
            var sessioni = new List<SessioniQrDTO>
            {
                new SessioniQrDTO { ClienteId = 1, QrCode = "QR_SCADUTO_001", DataScadenza = DateTime.Now.AddMinutes(-10), Utilizzato = false },
                new SessioniQrDTO { ClienteId = 1, QrCode = "QR_VALIDO_001", DataScadenza = DateTime.Now.AddHours(1), Utilizzato = false },
                new SessioniQrDTO { ClienteId = 2, QrCode = "QR_SCADUTO_002", DataScadenza = DateTime.Now.AddMinutes(-5), Utilizzato = true }
            };

            foreach (var sessione in sessioni)
            {
                await _sessioniQrRepository.AddAsync(sessione);
            }

            // Act
            var result = await _sessioniQrRepository.GetScaduteAsync();

            // Assert
            Assert.Equal(2, result.Count());
            Assert.All(result, s => Assert.True(s.DataScadenza <= DateTime.Now));
        }

        [Fact]
        public async Task UpdateAsync_Should_Update_SessioneQr_Correctly()
        {
            // Arrange
            var sessioneDto = new SessioniQrDTO
            {
                ClienteId = 1,
                QrCode = "QR_ORIGINAL",
                DataScadenza = DateTime.Now.AddHours(1),
                Utilizzato = false
            };
            await _sessioniQrRepository.AddAsync(sessioneDto);

            var updateDto = new SessioniQrDTO
            {
                SessioneId = sessioneDto.SessioneId,
                ClienteId = 2,
                QrCode = "QR_AGGIORNATO",
                DataScadenza = DateTime.Now.AddHours(3),
                Utilizzato = true,
                DataUtilizzo = DateTime.Now
            };

            // Act
            await _sessioniQrRepository.UpdateAsync(updateDto);

            // Assert
            var updated = await _sessioniQrRepository.GetByIdAsync(sessioneDto.SessioneId);
            Assert.NotNull(updated);
            Assert.Equal("QR_AGGIORNATO", updated.QrCode);
            Assert.Equal(2, updated.ClienteId);
            Assert.True(updated.Utilizzato);
            Assert.NotNull(updated.DataUtilizzo);
        }

        [Fact]
        public async Task UpdateAsync_Should_Throw_For_NonExisting_Id()
        {
            // Arrange
            var updateDto = new SessioniQrDTO
            {
                SessioneId = Guid.NewGuid(),
                ClienteId = 1,
                QrCode = "QR_INESISTENTE",
                DataScadenza = DateTime.Now.AddHours(1)
            };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _sessioniQrRepository.UpdateAsync(updateDto));
        }

        [Fact]
        public async Task DeleteAsync_Should_Remove_SessioneQr()
        {
            // Arrange
            var sessioneDto = new SessioniQrDTO
            {
                ClienteId = 1,
                QrCode = "QR_TO_DELETE",
                DataScadenza = DateTime.Now.AddHours(1),
                Utilizzato = false
            };
            await _sessioniQrRepository.AddAsync(sessioneDto);

            // Act
            await _sessioniQrRepository.DeleteAsync(sessioneDto.SessioneId);

            // Assert
            var deleted = await _sessioniQrRepository.GetByIdAsync(sessioneDto.SessioneId);
            Assert.Null(deleted);
        }

        [Fact]
        public async Task DeleteAsync_Should_Not_Throw_For_NonExisting_Id()
        {
            // Act & Assert - Non dovrebbe lanciare eccezioni
            await _sessioniQrRepository.DeleteAsync(Guid.NewGuid());
        }

        [Fact]
        public async Task ExistsAsync_Should_Return_True_For_Existing_Sessione()
        {
            // Arrange
            var sessioneDto = new SessioniQrDTO
            {
                ClienteId = 1,
                QrCode = "QR_EXISTS",
                DataScadenza = DateTime.Now.AddHours(1)
            };
            await _sessioniQrRepository.AddAsync(sessioneDto);

            // Act
            var exists = await _sessioniQrRepository.ExistsAsync(sessioneDto.SessioneId);

            // Assert
            Assert.True(exists);
        }

        [Fact]
        public async Task ExistsAsync_Should_Return_False_For_NonExisting_Sessione()
        {
            // Act
            var exists = await _sessioniQrRepository.ExistsAsync(Guid.NewGuid());

            // Assert
            Assert.False(exists);
        }

        [Fact]
        public async Task AddAsync_Should_Assign_Generated_Guid_And_DataCreazione()
        {
            // Arrange
            var sessioneDto = new SessioniQrDTO
            {
                ClienteId = 1,
                QrCode = "QR_GENERATED",
                DataScadenza = DateTime.Now.AddHours(1)
            };

            // Act
            await _sessioniQrRepository.AddAsync(sessioneDto);

            // Assert
            Assert.NotEqual(Guid.Empty, sessioneDto.SessioneId);
            Assert.NotNull(sessioneDto.DataCreazione);
        }

        [Fact]
        public async Task GetAllAsync_Should_Return_All_Sessioni()
        {
            // Arrange
            var sessioni = new List<SessioniQrDTO>
            {
                new SessioniQrDTO { ClienteId = 1, QrCode = "QR_ALL_001", DataScadenza = DateTime.Now.AddHours(1) },
                new SessioniQrDTO { ClienteId = 2, QrCode = "QR_ALL_002", DataScadenza = DateTime.Now.AddHours(2) }
            };

            foreach (var sessione in sessioni)
            {
                await _sessioniQrRepository.AddAsync(sessione);
            }

            // Act
            var result = await _sessioniQrRepository.GetAllAsync();

            // Assert
            Assert.Equal(2, result.Count());
        }
    }
}