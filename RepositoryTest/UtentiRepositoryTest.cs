using BBltZen;
using DTO;
using Microsoft.EntityFrameworkCore;
using Repository.Service;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace RepositoryTest
{
    public class UtentiRepositoryTest : BaseTest
    {
        private readonly UtentiRepository _repository;

        public UtentiRepositoryTest()
        {
            _repository = new UtentiRepository(_context);
        }

        [Fact]
        public async Task AddAsync_Should_Add_Utente()
        {
            // Arrange
            var utenteDto = new UtentiDTO
            {
                Email = "test@example.com",
                PasswordHash = "hashed_password"
                // ✅ RIMOSSO: TipoUtente, DataCreazione, Attivo (vengono settati dal repository)
            };

            // Act
            var result = await _repository.AddAsync(utenteDto); // ✅ CAMBIATO: assegna risultato

            // Assert
            Assert.NotNull(result);
            Assert.True(result.UtenteId > 0); // ✅ VERIFICA ID generato
            Assert.Equal("test@example.com", result.Email);
            Assert.Equal("cliente", result.TipoUtente); // ✅ VERIFICA DEFAULT
            Assert.True(result.Attivo); // ✅ VERIFICA DEFAULT
            Assert.NotNull(result.DataCreazione); // ✅ VERIFICA timestamp
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Utente()
        {
            // Arrange
            var utenteDto = new UtentiDTO
            {
                Email = "getbyid@example.com",
                PasswordHash = "hashed_password"
                // ✅ RIMOSSO: TipoUtente, Attivo (vengono settati dal repository)
            };
            var addedUtente = await _repository.AddAsync(utenteDto); // ✅ CAMBIATO: assegna risultato

            // Act
            var result = await _repository.GetByIdAsync(addedUtente.UtenteId); // ✅ USA ID dal risultato

            // Assert
            Assert.NotNull(result);
            Assert.Equal(addedUtente.UtenteId, result.UtenteId);
            Assert.Equal("getbyid@example.com", result.Email);
            Assert.Equal("cliente", result.TipoUtente); // ✅ VERIFICA DEFAULT
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_For_Invalid_Id()
        {
            // Act
            var result = await _repository.GetByIdAsync(999);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetByEmailAsync_Should_Return_Utente()
        {
            // Arrange
            var utenteDto = new UtentiDTO
            {
                Email = "unique@example.com",
                PasswordHash = "hashed_password"
            };
            var addedUtente = await _repository.AddAsync(utenteDto); // ✅ CAMBIATO: assegna risultato

            // Act
            var result = await _repository.GetByEmailAsync("unique@example.com");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("unique@example.com", result.Email);
            Assert.Equal(addedUtente.UtenteId, result.UtenteId); // ✅ VERIFICA ID
        }

        [Fact]
        public async Task GetByEmailAsync_Should_Return_Null_For_Invalid_Email()
        {
            // Act
            var result = await _repository.GetByEmailAsync("nonexistent@example.com");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetByTipoUtenteAsync_Should_Return_Filtered_Utenti()
        {
            // Arrange
            var utente1 = new UtentiDTO { Email = "cliente1@test.com", PasswordHash = "hash1", TipoUtente = "cliente", Attivo = true };
            var utente2 = new UtentiDTO { Email = "admin@test.com", PasswordHash = "hash2", TipoUtente = "admin", Attivo = true };
            var utente3 = new UtentiDTO { Email = "cliente2@test.com", PasswordHash = "hash3", TipoUtente = "cliente", Attivo = true };

            await _repository.AddAsync(utente1);
            await _repository.AddAsync(utente2);
            await _repository.AddAsync(utente3);

            // Act
            var clienti = await _repository.GetByTipoUtenteAsync("cliente");

            // Assert
            Assert.Equal(2, clienti.Count());
            Assert.All(clienti, u => Assert.Equal("cliente", u.TipoUtente));
        }

        [Fact]
        public async Task GetAttiviAsync_Should_Return_Only_Active_Utenti()
        {
            // Arrange
            var utente1 = new UtentiDTO { Email = "active1@test.com", PasswordHash = "hash1", TipoUtente = "cliente", Attivo = true };
            var utente2 = new UtentiDTO { Email = "inactive@test.com", PasswordHash = "hash2", TipoUtente = "cliente", Attivo = false };
            var utente3 = new UtentiDTO { Email = "active2@test.com", PasswordHash = "hash3", TipoUtente = "cliente", Attivo = true };

            await _repository.AddAsync(utente1);
            await _repository.AddAsync(utente2);
            await _repository.AddAsync(utente3);

            // Act
            var attivi = await _repository.GetAttiviAsync();

            // Assert
            Assert.Equal(2, attivi.Count());
            Assert.All(attivi, u => Assert.True(u.Attivo));
        }

        [Fact]
        public async Task UpdateAsync_Should_Update_Utente()
        {
            // Arrange
            var utenteDto = new UtentiDTO
            {
                Email = "original@example.com",
                PasswordHash = "original_hash"
            };
            var addedUtente = await _repository.AddAsync(utenteDto); // ✅ CAMBIATO: assegna risultato

            // Act - Update
            var updateDto = new UtentiDTO
            {
                UtenteId = addedUtente.UtenteId, // ✅ USA ID dal risultato
                Email = "updated@example.com",
                PasswordHash = "updated_hash",
                TipoUtente = "admin",
                Attivo = false
            };

            await _repository.UpdateAsync(updateDto);

            // Assert
            var updated = await _repository.GetByIdAsync(addedUtente.UtenteId);
            Assert.NotNull(updated);
            Assert.Equal("updated@example.com", updated.Email);
            Assert.Equal("updated_hash", updated.PasswordHash);
            Assert.Equal("admin", updated.TipoUtente);
            Assert.False(updated.Attivo);
        }

        [Fact]
        public async Task UpdateAsync_Should_Throw_For_Invalid_Id()
        {
            // Arrange
            var invalidUtente = new UtentiDTO
            {
                UtenteId = 999,
                Email = "invalid@example.com",
                PasswordHash = "hash",
                TipoUtente = "cliente"
            };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _repository.UpdateAsync(invalidUtente));
        }

        [Fact]
        public async Task DeleteAsync_Should_Remove_Utente()
        {
            // Arrange
            var utenteDto = new UtentiDTO
            {
                Email = "todelete@example.com",
                PasswordHash = "hash",
                TipoUtente = "cliente",
                Attivo = true
            };
            await _repository.AddAsync(utenteDto);
            var utenteId = utenteDto.UtenteId;

            // Verify exists
            Assert.True(await _repository.ExistsAsync(utenteId));

            // Act
            await _repository.DeleteAsync(utenteId);

            // Assert
            Assert.False(await _repository.ExistsAsync(utenteId));
        }

        [Fact]
        public async Task DeleteAsync_Should_Do_Nothing_For_Invalid_Id()
        {
            // Act & Assert (should not throw)
            await _repository.DeleteAsync(999);
        }

        [Fact]
        public async Task ExistsAsync_Should_Return_Correct_Value()
        {
            // Arrange
            var utenteDto = new UtentiDTO
            {
                Email = "exists@example.com",
                PasswordHash = "hash",
                TipoUtente = "cliente",
                Attivo = true
            };
            await _repository.AddAsync(utenteDto);
            var utenteId = utenteDto.UtenteId;

            // Act & Assert
            Assert.True(await _repository.ExistsAsync(utenteId));
            Assert.False(await _repository.ExistsAsync(999));
        }

        [Fact]
        public async Task EmailExistsAsync_Should_Return_Correct_Value()
        {
            // Arrange
            var utenteDto = new UtentiDTO
            {
                Email = "uniqueemail@example.com",
                PasswordHash = "hash",
                TipoUtente = "cliente",
                Attivo = true
            };
            await _repository.AddAsync(utenteDto);

            // Act & Assert
            Assert.True(await _repository.EmailExistsAsync("uniqueemail@example.com"));
            Assert.False(await _repository.EmailExistsAsync("nonexistent@example.com"));
        }

        [Fact]
        public async Task GetAllAsync_Should_Return_All_Utenti()
        {
            // Arrange
            var utente1 = new UtentiDTO { Email = "test1@example.com", PasswordHash = "hash1", TipoUtente = "cliente", Attivo = true };
            var utente2 = new UtentiDTO { Email = "test2@example.com", PasswordHash = "hash2", TipoUtente = "admin", Attivo = false };

            await _repository.AddAsync(utente1);
            await _repository.AddAsync(utente2);

            // Act
            var allUtenti = await _repository.GetAllAsync();

            // Assert
            Assert.Equal(2, allUtenti.Count());
        }

        [Fact]
        public async Task AddAsync_Should_Set_Default_Values()
        {
            // Arrange
            var utenteDto = new UtentiDTO
            {
                Email = "default@example.com",
                PasswordHash = "hashed_password"
                // ✅ INTENZIONALMENTE NON SETTO: TipoUtente, Attivo
            };

            // Act
            var result = await _repository.AddAsync(utenteDto);

            // Assert - ✅ VERIFICA VALORI DI DEFAULT
            Assert.Equal("cliente", result.TipoUtente);
            Assert.True(result.Attivo);

            // ✅ CORRETTO: VERIFICA PRIMA CHE NON SIANO NULL
            Assert.NotNull(result.DataCreazione);
            Assert.NotNull(result.DataAggiornamento);

            // ✅ USA GetValueOrDefault() PER EVITARE WARNING
            var dataCreazione = result.DataCreazione.GetValueOrDefault();
            var dataAggiornamento = result.DataAggiornamento.GetValueOrDefault();

            var timeTolerance = TimeSpan.FromSeconds(2);
            Assert.True((DateTime.Now - dataCreazione).Duration() <= timeTolerance,
                $"DataCreazione fuori tolleranza. Now: {DateTime.Now}, Creazione: {dataCreazione}");

            Assert.True((DateTime.Now - dataAggiornamento).Duration() <= timeTolerance,
                $"DataAggiornamento fuori tolleranza. Now: {DateTime.Now}, Aggiornamento: {dataAggiornamento}");

            // ✅ VERIFICA CHE LE DATE SIANO APPROSSIMATIVAMENTE UGUALI
            Assert.True((dataCreazione - dataAggiornamento).Duration() <= TimeSpan.FromMilliseconds(100),
                "DataCreazione e DataAggiornamento dovrebbero essere quasi uguali");
        }

        [Fact]
        public async Task AddAsync_Should_Set_Correct_Timestamps()
        {
            // Arrange
            var utenteDto = new UtentiDTO
            {
                Email = "timestamp@example.com",
                PasswordHash = "hashed_password"
            };

            // Act
            var result = await _repository.AddAsync(utenteDto);

            // Assert - ✅ USA ToString per evitare problemi di millisecondi
            Assert.Equal(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                         result.DataCreazione?.ToString("yyyy-MM-dd HH:mm:ss"));
            Assert.Equal(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                         result.DataAggiornamento?.ToString("yyyy-MM-dd HH:mm:ss"));
        }

        [Fact]
        public async Task UpdateAsync_Should_Update_DataAggiornamento()
        {
            // Arrange
            var utenteDto = new UtentiDTO
            {
                Email = "update@example.com",
                PasswordHash = "original_hash"
            };
            var addedUtente = await _repository.AddAsync(utenteDto);

            // ✅ USA GetValueOrDefault() per evitare warning
            var originalUpdateTime = addedUtente.DataAggiornamento.GetValueOrDefault();

            var updateDto = new UtentiDTO
            {
                UtenteId = addedUtente.UtenteId,
                Email = "updated@example.com",
                PasswordHash = "updated_hash",
                TipoUtente = "admin"
            };

            // Act
            await Task.Delay(1);
            await _repository.UpdateAsync(updateDto);

            // Assert
            var updated = await _repository.GetByIdAsync(addedUtente.UtenteId);
            Assert.NotNull(updated);

            // ✅ USA GetValueOrDefault() per evitare warning
            var newUpdateTime = updated.DataAggiornamento.GetValueOrDefault();
            Assert.True(newUpdateTime > originalUpdateTime);
        }

        [Fact]
        public async Task GetByEmailAsync_With_InvalidEmail_Should_Return_Null()
        {
            // Act
            var result = await _repository.GetByEmailAsync("nonexistent@example.com");

            // Assert
            Assert.Null(result);
        }
    }
}