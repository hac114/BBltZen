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
            _sessioniQrRepository = new SessioniQrRepository(_context);
            SetupTestData();
        }

        private void SetupTestData()
        {
            // Pulisci e setup dati di test
            _context.SessioniQr.RemoveRange(_context.SessioniQr);
            _context.Tavolo.RemoveRange(_context.Tavolo);
            _context.Cliente.RemoveRange(_context.Cliente);
            _context.SaveChanges();

            // Crea tavoli di test
            if (!_context.Tavolo.Any())
            {
                _context.Tavolo.AddRange(
                    new Tavolo { TavoloId = 1, Numero = 1, Disponibile = true, QrCode = "T1_QR", Zona = "Interno" },
                    new Tavolo { TavoloId = 2, Numero = 2, Disponibile = true, QrCode = "T2_QR", Zona = "Esterno" }
                );
            }

            // Crea clienti di test
            if (!_context.Cliente.Any())
            {
                _context.Cliente.AddRange(
                    new Cliente { ClienteId = 1, TavoloId = 1 },
                    new Cliente { ClienteId = 2, TavoloId = 2 }
                );
            }

            _context.SaveChanges();
        }

        [Fact]
        public async Task AddAsync_Should_Add_SessioneQr()
        {
            // Arrange
            var sessioneDto = new SessioniQrDTO
            {
                TavoloId = 1,
                ClienteId = 1,
                CodiceSessione = "T1_20241017_123456",
                Stato = "Attiva",
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
            Assert.Equal(1, result.TavoloId);
            Assert.Equal(1, result.ClienteId);
            Assert.Equal("T1_20241017_123456", result.CodiceSessione);
            Assert.Equal("Attiva", result.Stato);
            Assert.False(result.Utilizzato);
            Assert.NotNull(result.DataCreazione);
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Correct_SessioneQr()
        {
            // Arrange
            var sessioneDto = new SessioniQrDTO
            {
                TavoloId = 1,
                ClienteId = 1,
                CodiceSessione = "T1_20241017_123457",
                Stato = "Attiva",
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
            Assert.Equal(1, result.TavoloId);
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
                TavoloId = 1,
                ClienteId = 1,
                CodiceSessione = "T1_20241017_123458",
                Stato = "Attiva",
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
        public async Task GetByCodiceSessioneAsync_Should_Return_Correct_SessioneQr()
        {
            // Arrange
            var sessioneDto = new SessioniQrDTO
            {
                TavoloId = 1,
                ClienteId = 1,
                CodiceSessione = "T1_20241017_123459",
                Stato = "Attiva",
                QrCode = "QR_CODE_001",
                DataScadenza = DateTime.Now.AddHours(1),
                Utilizzato = false
            };
            await _sessioniQrRepository.AddAsync(sessioneDto);

            // Act
            var result = await _sessioniQrRepository.GetByCodiceSessioneAsync("T1_20241017_123459");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("T1_20241017_123459", result.CodiceSessione);
            Assert.Equal(sessioneDto.SessioneId, result.SessioneId);
        }

        [Fact]
        public async Task GetByTavoloIdAsync_Should_Return_All_Sessioni_For_Tavolo()
        {
            // Arrange
            var sessioniTavolo1 = new List<SessioniQrDTO>
            {
                new SessioniQrDTO { TavoloId = 1, ClienteId = 1, CodiceSessione = "T1_001", Stato = "Attiva", QrCode = "QR_T1_001", DataScadenza = DateTime.Now.AddHours(1), Utilizzato = false },
                new SessioniQrDTO { TavoloId = 1, ClienteId = 1, CodiceSessione = "T1_002", Stato = "Attiva", QrCode = "QR_T1_002", DataScadenza = DateTime.Now.AddHours(2), Utilizzato = true }
            };

            var sessioniTavolo2 = new List<SessioniQrDTO>
            {
                new SessioniQrDTO { TavoloId = 2, ClienteId = 2, CodiceSessione = "T2_001", Stato = "Attiva", QrCode = "QR_T2_001", DataScadenza = DateTime.Now.AddHours(1), Utilizzato = false }
            };

            foreach (var sessione in sessioniTavolo1.Concat(sessioniTavolo2))
            {
                await _sessioniQrRepository.AddAsync(sessione);
            }

            // Act
            var result = await _sessioniQrRepository.GetByTavoloIdAsync(1);

            // Assert
            Assert.Equal(2, result.Count());
            Assert.All(result, s => Assert.Equal(1, s.TavoloId));
        }

        [Fact]
        public async Task GetByClienteIdAsync_Should_Return_All_Sessioni_For_Cliente()
        {
            // Arrange
            var sessioniCliente1 = new List<SessioniQrDTO>
            {
                new SessioniQrDTO { TavoloId = 1, ClienteId = 1, CodiceSessione = "T1_CL1_001", Stato = "Attiva", QrCode = "QR_CLIENTE1_001", DataScadenza = DateTime.Now.AddHours(1), Utilizzato = false },
                new SessioniQrDTO { TavoloId = 1, ClienteId = 1, CodiceSessione = "T1_CL1_002", Stato = "Attiva", QrCode = "QR_CLIENTE1_002", DataScadenza = DateTime.Now.AddHours(2), Utilizzato = true }
            };

            var sessioniCliente2 = new List<SessioniQrDTO>
            {
                new SessioniQrDTO { TavoloId = 2, ClienteId = 2, CodiceSessione = "T2_CL2_001", Stato = "Attiva", QrCode = "QR_CLIENTE2_001", DataScadenza = DateTime.Now.AddHours(1), Utilizzato = false }
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
                new SessioniQrDTO { TavoloId = 1, ClienteId = 1, CodiceSessione = "T1_NON_UTIL_001", Stato = "Attiva", QrCode = "QR_NON_UTIL_001", DataScadenza = DateTime.Now.AddHours(1), Utilizzato = false },
                new SessioniQrDTO { TavoloId = 1, ClienteId = 1, CodiceSessione = "T1_UTIL_001", Stato = "Utilizzata", QrCode = "QR_UTIL_001", DataScadenza = DateTime.Now.AddHours(1), Utilizzato = true },
                new SessioniQrDTO { TavoloId = 2, ClienteId = 2, CodiceSessione = "T2_NON_UTIL_002", Stato = "Attiva", QrCode = "QR_NON_UTIL_002", DataScadenza = DateTime.Now.AddHours(2), Utilizzato = null },
                new SessioniQrDTO { TavoloId = 2, ClienteId = 2, CodiceSessione = "T2_UTIL_002", Stato = "Utilizzata", QrCode = "QR_UTIL_002", DataScadenza = DateTime.Now.AddHours(2), Utilizzato = true }
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
                new SessioniQrDTO { TavoloId = 1, ClienteId = 1, CodiceSessione = "T1_SCADUTO_001", Stato = "Scaduta", QrCode = "QR_SCADUTO_001", DataScadenza = DateTime.Now.AddMinutes(-10), Utilizzato = false },
                new SessioniQrDTO { TavoloId = 1, ClienteId = 1, CodiceSessione = "T1_VALIDO_001", Stato = "Attiva", QrCode = "QR_VALIDO_001", DataScadenza = DateTime.Now.AddHours(1), Utilizzato = false },
                new SessioniQrDTO { TavoloId = 2, ClienteId = 2, CodiceSessione = "T2_SCADUTO_002", Stato = "Scaduta", QrCode = "QR_SCADUTO_002", DataScadenza = DateTime.Now.AddMinutes(-5), Utilizzato = true }
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
                TavoloId = 1,
                ClienteId = 1,
                CodiceSessione = "T1_ORIGINAL",
                Stato = "Attiva",
                QrCode = "QR_ORIGINAL",
                DataScadenza = DateTime.Now.AddHours(1),
                Utilizzato = false
            };
            await _sessioniQrRepository.AddAsync(sessioneDto);

            var updateDto = new SessioniQrDTO
            {
                SessioneId = sessioneDto.SessioneId,
                TavoloId = 2,
                ClienteId = 2,
                CodiceSessione = "T2_AGGIORNATO",
                Stato = "Utilizzata",
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
            Assert.Equal(2, updated.TavoloId);
            Assert.Equal(2, updated.ClienteId);
            Assert.Equal("T2_AGGIORNATO", updated.CodiceSessione);
            Assert.Equal("Utilizzata", updated.Stato);
            Assert.True(updated.Utilizzato);
            Assert.NotNull(updated.DataUtilizzo);
        }

        [Fact]
        public async Task GeneraSessioneQrAsync_Should_Create_Session_With_QR_Code()
        {
            // Arrange
            var frontendUrl = "https://bbzen.it";

            // Act
            var result = await _sessioniQrRepository.GeneraSessioneQrAsync(1, frontendUrl);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.TavoloId);
            Assert.Equal("Attiva", result.Stato);
            Assert.False(result.Utilizzato);
            Assert.NotNull(result.CodiceSessione);
            Assert.StartsWith("T1_", result.CodiceSessione);
            Assert.NotNull(result.QrCode);

            // ✅ VERIFICA SOLO CHE IL QR CODE SIA STATO GENERATO (base64 valido)
            Assert.StartsWith("iVBOR", result.QrCode); // PNG base64 inizia con iVBOR
            Assert.True(result.QrCode.Length > 100); // Base64 di almeno 100 caratteri
        }

        [Fact]
        public async Task GeneraSessioneQrAsync_Should_Throw_For_NonExisting_Tavolo()
        {
            // Arrange
            var frontendUrl = "https://bbzen.it";

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _sessioniQrRepository.GeneraSessioneQrAsync(999, frontendUrl));
        }

        [Fact]
        public async Task DeleteAsync_Should_Remove_SessioneQr()
        {
            // Arrange
            var sessioneDto = new SessioniQrDTO
            {
                TavoloId = 1,
                ClienteId = 1,
                CodiceSessione = "T1_TO_DELETE",
                Stato = "Attiva",
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
        public async Task ExistsAsync_Should_Return_True_For_Existing_Sessione()
        {
            // Arrange
            var sessioneDto = new SessioniQrDTO
            {
                TavoloId = 1,
                ClienteId = 1,
                CodiceSessione = "T1_EXISTS",
                Stato = "Attiva",
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
        public async Task GetAllAsync_Should_Return_All_Sessioni()
        {
            // Arrange
            var sessioni = new List<SessioniQrDTO>
            {
                new SessioniQrDTO { TavoloId = 1, ClienteId = 1, CodiceSessione = "T1_ALL_001", Stato = "Attiva", QrCode = "QR_ALL_001", DataScadenza = DateTime.Now.AddHours(1) },
                new SessioniQrDTO { TavoloId = 2, ClienteId = 2, CodiceSessione = "T2_ALL_002", Stato = "Attiva", QrCode = "QR_ALL_002", DataScadenza = DateTime.Now.AddHours(2) }
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