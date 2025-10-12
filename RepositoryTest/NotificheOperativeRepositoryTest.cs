using Database;
using DTO;
using Microsoft.EntityFrameworkCore;
using Repository.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace RepositoryTest
{
    public class NotificheOperativeRepositoryTest : BaseTest
    {
        private readonly NotificheOperativeRepository _repository;
        private readonly BubbleTeaContext _context;

        public NotificheOperativeRepositoryTest()
        {
            var options = new DbContextOptionsBuilder<BubbleTeaContext>()
                .UseInMemoryDatabase(databaseName: $"NotificheOperativeTest_{Guid.NewGuid()}")
                .Options;

            _context = new BubbleTeaContext(options);
            _repository = new NotificheOperativeRepository(_context);

            InitializeTestData();
        }

        private void InitializeTestData()
        {
            _context.Database.EnsureDeleted();
            _context.Database.EnsureCreated();

            var notifiche = new List<NotificheOperative>
            {
                new NotificheOperative
                {
                    NotificaId = 1,
                    DataCreazione = DateTime.Now.AddHours(-3),
                    OrdiniCoinvolti = "1,2,3",
                    Messaggio = "Ritardo nella preparazione degli ordini",
                    Stato = "Pendente",
                    DataGestione = null,
                    UtenteGestione = null,
                    Priorita = 1
                },
                new NotificheOperative
                {
                    NotificaId = 2,
                    DataCreazione = DateTime.Now.AddHours(-2),
                    OrdiniCoinvolti = "4",
                    Messaggio = "Ingrediente esaurito: Tapioca",
                    Stato = "Pendente",
                    DataGestione = null,
                    UtenteGestione = null,
                    Priorita = 2
                },
                new NotificheOperative
                {
                    NotificaId = 3,
                    DataCreazione = DateTime.Now.AddHours(-1),
                    OrdiniCoinvolti = "5,6",
                    Messaggio = "Problema pagamento ordini",
                    Stato = "Gestita",
                    DataGestione = DateTime.Now.AddMinutes(-30),
                    UtenteGestione = "admin",
                    Priorita = 1
                },
                new NotificheOperative
                {
                    NotificaId = 4,
                    DataCreazione = DateTime.Now.AddMinutes(-30),
                    OrdiniCoinvolti = "7",
                    Messaggio = "Ordine speciale richiede attenzione",
                    Stato = "Pendente",
                    DataGestione = null,
                    UtenteGestione = null,
                    Priorita = 3
                }
            };

            _context.NotificheOperative.AddRange(notifiche);
            _context.SaveChanges();
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnAllNotifiche()
        {
            // Act
            var result = await _repository.GetAllAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(4, result.Count());
        }

        [Fact]
        public async Task GetByIdAsync_WithValidId_ShouldReturnNotifica()
        {
            // Act
            var result = await _repository.GetByIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.NotificaId);
            Assert.Equal("Ritardo nella preparazione degli ordini", result.Messaggio);
            Assert.Equal("Pendente", result.Stato);
            Assert.Equal(1, result.Priorita);
        }

        [Fact]
        public async Task GetByIdAsync_WithInvalidId_ShouldReturnNull()
        {
            // Act
            var result = await _repository.GetByIdAsync(999);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetByStatoAsync_ShouldReturnFilteredNotifiche()
        {
            // Act
            var result = await _repository.GetByStatoAsync("Pendente");

            // Assert
            var resultList = result.ToList();
            Assert.Equal(3, resultList.Count);
            Assert.All(resultList, n => Assert.Equal("Pendente", n.Stato));
        }

        [Fact]
        public async Task GetByPrioritaAsync_ShouldReturnFilteredNotifiche()
        {
            // Act
            var result = await _repository.GetByPrioritaAsync(1);

            // Assert
            var resultList = result.ToList();
            Assert.Equal(2, resultList.Count);
            Assert.All(resultList, n => Assert.Equal(1, n.Priorita));
        }

        [Fact]
        public async Task GetPendentiAsync_ShouldReturnOnlyPendingNotifiche()
        {
            // Act
            var result = await _repository.GetPendentiAsync();

            // Assert
            var resultList = result.ToList();
            Assert.Equal(3, resultList.Count);
            Assert.All(resultList, n => Assert.Equal("Pendente", n.Stato));
            // Verifica ordinamento per priorità
            Assert.Equal(1, resultList[0].Priorita); // Priorità 1
            Assert.Equal(2, resultList[1].Priorita); // Priorità 2
            Assert.Equal(3, resultList[2].Priorita); // Priorità 3
        }

        [Fact]
        public async Task GetByPeriodoAsync_ShouldReturnNotificheInPeriod()
        {
            // Arrange
            var dataInizio = DateTime.Now.AddHours(-4);
            var dataFine = DateTime.Now.AddHours(-1.5);

            // Act
            var result = await _repository.GetByPeriodoAsync(dataInizio, dataFine);

            // Assert
            var resultList = result.ToList();
            Assert.Equal(2, resultList.Count);
            Assert.All(resultList, n =>
            {
                Assert.True(n.DataCreazione >= dataInizio);
                Assert.True(n.DataCreazione <= dataFine);
            });
        }

        [Fact]
        public async Task AddAsync_ShouldAddNewNotifica()
        {
            // Arrange
            var newNotifica = new NotificheOperativeDTO
            {
                OrdiniCoinvolti = "8,9",
                Messaggio = "Nuova notifica di test",
                Stato = "Pendente",
                Priorita = 2,
                UtenteGestione = null
            };

            // Act
            await _repository.AddAsync(newNotifica);

            // Assert
            Assert.True(newNotifica.NotificaId > 0);
            var result = await _repository.GetByIdAsync(newNotifica.NotificaId);
            Assert.NotNull(result);
            Assert.Equal("Nuova notifica di test", result.Messaggio);
            Assert.Equal("Pendente", result.Stato);
            Assert.Equal(2, result.Priorita);
            Assert.NotNull(result.DataCreazione);
        }

        [Fact]
        public async Task UpdateAsync_ShouldUpdateExistingNotifica()
        {
            // Arrange
            var updateDto = new NotificheOperativeDTO
            {
                NotificaId = 1,
                OrdiniCoinvolti = "1,2,3,4",
                Messaggio = "Ritardo nella preparazione - AGGIORNATO",
                Stato = "Gestita",
                Priorita = 1,
                UtenteGestione = "testuser"
            };

            // Act
            await _repository.UpdateAsync(updateDto);

            // Assert
            var result = await _repository.GetByIdAsync(1);
            Assert.NotNull(result);
            Assert.Equal("Ritardo nella preparazione - AGGIORNATO", result.Messaggio);
            Assert.Equal("Gestita", result.Stato);
            Assert.Equal("testuser", result.UtenteGestione);
            Assert.NotNull(result.DataGestione);
        }

        [Fact]
        public async Task DeleteAsync_ShouldRemoveNotifica()
        {
            // Act
            await _repository.DeleteAsync(1);

            // Assert
            var result = await _repository.GetByIdAsync(1);
            Assert.Null(result);
        }

        [Fact]
        public async Task ExistsAsync_WithExistingId_ShouldReturnTrue()
        {
            // Act
            var result = await _repository.ExistsAsync(1);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task ExistsAsync_WithNonExistingId_ShouldReturnFalse()
        {
            // Act
            var result = await _repository.ExistsAsync(999);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task GetNumeroNotifichePendentiAsync_ShouldReturnCorrectCount()
        {
            // Act
            var result = await _repository.GetNumeroNotifichePendentiAsync();

            // Assert
            Assert.Equal(3, result);
        }

        [Fact]
        public async Task UpdateAsync_WithNonExistingNotifica_ShouldThrowException()
        {
            // Arrange
            var updateDto = new NotificheOperativeDTO
            {
                NotificaId = 999,
                OrdiniCoinvolti = "test",
                Messaggio = "Test",
                Stato = "Pendente",
                Priorita = 1
            };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _repository.UpdateAsync(updateDto));
        }
    }
}