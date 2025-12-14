using BBltZen;
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

        public NotificheOperativeRepositoryTest()
        {
            // ✅ USA IL CONTEXT EREDITATO DA BaseTest
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
            var result = await _repository.AddAsync(newNotifica); // ✅ CORREGGI: assegna risultato

            // Assert
            Assert.True(result.NotificaId > 0); // ✅ VERIFICA sul risultato
            Assert.Equal("Nuova notifica di test", result.Messaggio);
            Assert.Equal("Pendente", result.Stato);
            Assert.Equal(2, result.Priorita);
            
            // ✅ VERIFICA CHE LA DATA SIA STATA IMPOSTATA
            Assert.NotEqual(default(DateTime), result.DataCreazione);
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

            // ✅ VERIFICA CHE LA DATA GESTIONE SIA STATA IMPOSTATA
            Assert.NotEqual(default(DateTime), result.DataGestione);
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
        public async Task UpdateAsync_WithNonExistingNotifica_ShouldNotThrow()
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

            // Act & Assert - ✅ CORREGGI: Non dovrebbe lanciare eccezione
            var exception = await Record.ExceptionAsync(() => _repository.UpdateAsync(updateDto));
            Assert.Null(exception);
        }

        [Fact]
        public async Task AddAsync_ShouldSetCorrectTimestamps()
        {
            // Arrange
            var newNotifica = new NotificheOperativeDTO
            {
                OrdiniCoinvolti = "10",
                Messaggio = "Test timestamp",
                Stato = "Pendente",
                Priorita = 1
            };

            // Act
            var result = await _repository.AddAsync(newNotifica);

            // Assert            
            // ✅ VERIFICA DIRETTAMENTE IL RANGE
            Assert.InRange(result.DataCreazione, DateTime.Now.AddMinutes(-1), DateTime.Now.AddMinutes(1));
        }

        [Fact]
        public async Task AddAsync_ShouldIncludeTipoNotifica()
        {
            // Arrange
            var newNotifica = new NotificheOperativeDTO
            {
                OrdiniCoinvolti = "11",
                Messaggio = "Test tipo notifica",
                Stato = "Pendente",
                Priorita = 1,
                TipoNotifica = "urgente"
            };

            // Act
            var result = await _repository.AddAsync(newNotifica);

            // Assert
            Assert.Equal("urgente", result.TipoNotifica);
            var retrieved = await _repository.GetByIdAsync(result.NotificaId);
            Assert.NotNull(retrieved);
            Assert.Equal("urgente", retrieved.TipoNotifica);
        }

        [Fact]
        public async Task GetByTipoNotificaAsync_ShouldReturnFilteredNotifiche()
        {
            // Arrange - Aggiungi notifica con tipo specifico
            var notificaUrgente = new NotificheOperativeDTO
            {
                OrdiniCoinvolti = "12",
                Messaggio = "Notifica urgente",
                Stato = "Pendente",
                Priorita = 1,
                TipoNotifica = "urgente"
            };
            await _repository.AddAsync(notificaUrgente);

            // Act
            var result = await _repository.GetByTipoNotificaAsync("urgente");

            // Assert
            var resultList = result.ToList();
            Assert.Single(resultList);
            Assert.All(resultList, n => Assert.Equal("urgente", n.TipoNotifica));
        }

        [Fact]
        public async Task GetStatisticheNotificheAsync_ShouldReturnCorrectStatistics()
        {
            // Act
            var result = await _repository.GetStatisticheNotificheAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, result["Pendente"]);  // 3 notifiche pendenti
            Assert.Equal(1, result["Gestita"]);   // 1 notifica gestita
        }

        [Fact]
        public async Task GetNumeroNotificheByStatoAsync_ShouldReturnCorrectCount()
        {
            // Act
            var pendenti = await _repository.GetNumeroNotificheByStatoAsync("Pendente");
            var gestite = await _repository.GetNumeroNotificheByStatoAsync("Gestita");

            // Assert
            Assert.Equal(3, pendenti);
            Assert.Equal(1, gestite);
        }

        [Fact]
        public async Task UpdateAsync_ShouldSetDataGestioneWhenStatoGestita()
        {
            // Arrange
            var updateDto = new NotificheOperativeDTO
            {
                NotificaId = 2,
                OrdiniCoinvolti = "4",
                Messaggio = "Ingrediente esaurito: Tapioca",
                Stato = "Gestita",
                Priorita = 2,
                UtenteGestione = "operator"
            };

            // Act
            await _repository.UpdateAsync(updateDto);

            // Assert
            var result = await _repository.GetByIdAsync(2);
            Assert.NotNull(result);
            Assert.Equal("Gestita", result.Stato);
            Assert.NotNull(result.DataGestione);
            Assert.InRange(result.DataGestione.Value, DateTime.Now.AddMinutes(-1), DateTime.Now.AddMinutes(1));
        }
    }
}