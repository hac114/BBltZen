using Database;
using DTO;
using Microsoft.Extensions.Logging;
using Moq;
using Repository.Service;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace RepositoryTest
{
    public class OperationalNotificationServiceRepositoryTest : BaseTest
    {
        private readonly OperationalNotificationServiceRepository _service;
        private readonly Mock<ILogger<OperationalNotificationServiceRepository>> _loggerMock;

        public OperationalNotificationServiceRepositoryTest()
        {
            _loggerMock = new Mock<ILogger<OperationalNotificationServiceRepository>>();
            _service = new OperationalNotificationServiceRepository(_context, _loggerMock.Object);
        }

        [Fact]
        public async Task NotifyLowStockAsync_WithUnavailableIngredients_CreatesNotifications()
        {
            // Arrange
            var categoria = CreateTestCategoriaIngrediente();
            var ingrediente = CreateTestIngrediente(categoria.CategoriaId, false);
            var personalizzazione = CreateTestPersonalizzazione();
            CreateTestPersonalizzazioneIngrediente(personalizzazione.PersonalizzazioneId, ingrediente.IngredienteId, 1);

            // Act
            var result = await _service.NotifyLowStockAsync();

            // Assert
            Assert.Single(result);
            Assert.Equal(ingrediente.IngredienteId, result.First().IngredienteId); // ✅ CAMBIA: [0] → First()
            Assert.Equal(1, result.First().BevandeAffette); // ✅ CAMBIA: [0] → First()

            var notifiche = await _service.GetUnreadNotificationsAsync();
            Assert.NotEmpty(notifiche);
        }

        [Fact]
        public async Task CreateNotificationAsync_CreatesValidNotification()
        {
            // Arrange
            var tipo = "TEST";
            var titolo = "Test Notification";
            var messaggio = "This is a test notification";
            var priorita = "Alta";

            // Act
            var result = await _service.CreateNotificationAsync(tipo, titolo, messaggio, priorita);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(tipo, result.TipoNotifica);
            Assert.Equal(titolo, result.Titolo);
            Assert.Equal(messaggio, result.Messaggio);
            Assert.Equal(priorita, result.Priorita);
            Assert.False(result.Letta);
        }

        [Fact]
        public async Task MarkNotificationAsReadAsync_MarksNotificationAsRead()
        {
            // Arrange
            var notifica = await _service.CreateNotificationAsync("TEST", "Test", "Message", "Media");

            // Act
            var result = await _service.MarkNotificationAsReadAsync(notifica.NotificationId);

            // Assert
            Assert.True(result);

            var notificheNonLette = await _service.GetUnreadNotificationsAsync();
            Assert.DoesNotContain(notificheNonLette, n => n.NotificationId == notifica.NotificationId);
        }

        [Fact]
        public async Task GetNotificationSummaryAsync_ReturnsCorrectCounts()
        {
            // Arrange
            await _service.CreateNotificationAsync("TEST1", "Test1", "Message1", "Alta");
            await _service.CreateNotificationAsync("TEST2", "Test2", "Message2", "Media");
            await _service.CreateNotificationAsync("TEST3", "Test3", "Message3", "Bassa");

            // Act
            var summary = await _service.GetNotificationSummaryAsync();

            // Assert
            Assert.True(summary.TotalNotifiche >= 3);
            Assert.True(summary.NotificheNonLette >= 3);
            Assert.True(summary.NotificheAltaPriorita >= 1);
            Assert.NotNull(summary.UltimeNotifiche);
            Assert.True(summary.UltimeNotifiche.Count <= 5);
        }

        [Fact]
        public async Task NotifyNewOrderAsync_CreatesOrderNotification()
        {
            // Act
            await _service.NotifyNewOrderAsync(123);

            // Assert
            var notifiche = await _service.GetUnreadNotificationsAsync();
            var notificaCreata = notifiche.FirstOrDefault(n => n.Messaggio.Contains("123"));
            Assert.NotNull(notificaCreata);
            Assert.Contains("123", notificaCreata.Messaggio);
        }

        [Fact]
        public async Task NotifyPaymentIssueAsync_CreatesHighPriorityNotification()
        {
            // Act
            await _service.NotifyPaymentIssueAsync(456);

            // Assert
            var notifiche = await _service.GetUnreadNotificationsAsync();
            var notificaCreata = notifiche.FirstOrDefault(n => n.Messaggio.Contains("456"));
            Assert.NotNull(notificaCreata);
            Assert.Equal("Alta", notificaCreata.Priorita);
            Assert.Contains("456", notificaCreata.Messaggio);
        }

        [Fact]
        public async Task GetPendingNotificationsCountAsync_ReturnsCorrectCount()
        {
            // Arrange
            await _service.CreateNotificationAsync("TEST1", "Test1", "Message1", "Alta");
            await _service.CreateNotificationAsync("TEST2", "Test2", "Message2", "Media");

            // Act
            var count = await _service.GetPendingNotificationsCountAsync();

            // Assert
            Assert.True(count >= 2);
        }

        [Fact]
        public async Task ExistsAsync_WithExistingNotification_ReturnsTrue()
        {
            // Arrange
            var notifica = await _service.CreateNotificationAsync("TEST", "Test", "Message", "Media");

            // Act
            var result = await _service.ExistsAsync(notifica.NotificationId);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task ExistsAsync_WithNonExistingNotification_ReturnsFalse()
        {
            // Act
            var result = await _service.ExistsAsync(999);

            // Assert
            Assert.False(result);
        }

        // Helper methods corretti
        private CategoriaIngrediente CreateTestCategoriaIngrediente()
        {
            var categoria = new CategoriaIngrediente
            {
                Categoria = $"Test Categoria {Guid.NewGuid()}"
            };
            _context.CategoriaIngrediente.Add(categoria);
            _context.SaveChanges();
            return categoria;
        }

        private Ingrediente CreateTestIngrediente(int categoriaId, bool disponibile)
        {
            var ingrediente = new Ingrediente
            {
                Ingrediente1 = $"Test Ingrediente {Guid.NewGuid()}",
                CategoriaId = categoriaId,
                PrezzoAggiunto = 1.00m,
                Disponibile = disponibile,
                DataInserimento = DateTime.Now,
                DataAggiornamento = DateTime.Now
            };
            _context.Ingrediente.Add(ingrediente);
            _context.SaveChanges();
            return ingrediente;
        }

        private Personalizzazione CreateTestPersonalizzazione()
        {
            var personalizzazione = new Personalizzazione
            {
                Nome = "Test Personalizzazione",
                Descrizione = "Descrizione test",
                DtCreazione = DateTime.Now
            };
            _context.Personalizzazione.Add(personalizzazione);
            _context.SaveChanges();
            return personalizzazione;
        }

        private PersonalizzazioneIngrediente CreateTestPersonalizzazioneIngrediente(int personalizzazioneId, int ingredienteId, decimal quantita)
        {
            // Crea un'unità di misura se non esiste
            if (!_context.UnitaDiMisura.Any())
            {
                var unitaMisura = new UnitaDiMisura
                {
                    Sigla = "g",
                    Descrizione = "grammi"
                };
                _context.UnitaDiMisura.Add(unitaMisura);
                _context.SaveChanges();
            }

            var unitaMisuraId = _context.UnitaDiMisura.First().UnitaMisuraId;

            var pi = new PersonalizzazioneIngrediente
            {
                PersonalizzazioneId = personalizzazioneId,
                IngredienteId = ingredienteId,
                Quantita = quantita,
                UnitaMisuraId = unitaMisuraId
            };
            _context.PersonalizzazioneIngrediente.Add(pi);
            _context.SaveChanges();
            return pi;
        }
    }
}