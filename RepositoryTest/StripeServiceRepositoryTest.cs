using Database;
using DTO;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Repository.Interface;
using Repository.Service;
using Xunit;

namespace RepositoryTest
{
    public class StripeServiceRepositoryTest : BaseTest, IDisposable
    {
        private readonly StripeServiceRepository _stripeService;
        private readonly Mock<ILogger<StripeServiceRepository>> _loggerMock;
        private readonly Mock<IOptions<StripeSettings>> _stripeSettingsMock;

        public StripeServiceRepositoryTest()
        {
            // Mock per ILogger
            _loggerMock = new Mock<ILogger<StripeServiceRepository>>();

            // Mock per StripeSettings (usa placeholder per testing)
            _stripeSettingsMock = new Mock<IOptions<StripeSettings>>();
            _stripeSettingsMock.Setup(x => x.Value)
                .Returns(new StripeSettings
                {
                    SecretKey = "REPLACE_WITH_STRIPE_SECRET_KEY", // ✅ Usa placeholder
                    PublishableKey = "REPLACE_WITH_STRIPE_PUBLISHABLE_KEY",
                    WebhookSecret = "REPLACE_WITH_STRIPE_WEBHOOK_SECRET"
                });

            // Inizializza il servizio
            _stripeService = new StripeServiceRepository(
                _context,
                _stripeSettingsMock.Object,
                _loggerMock.Object
            );

            // Inizializza dati di test
            InitializeTestData();
        }

        private void InitializeTestData()
        {
            // Pulisci e ricrea dati di test
            _context.Ordine.RemoveRange(_context.Ordine); // ✅ Corretto: Ordini (plurale)
            _context.StatoPagamento.RemoveRange(_context.StatoPagamento);
            _context.SaveChanges();

            // Crea stati pagamento di test (già fatto in BaseTest, ma ridondante per sicurezza)
            if (!_context.StatoPagamento.Any())
            {
                _context.StatoPagamento.AddRange(
                    new StatoPagamento { StatoPagamentoId = 1, StatoPagamento1 = "In_Attesa" },
                    new StatoPagamento { StatoPagamentoId = 2, StatoPagamento1 = "Pagato" },
                    new StatoPagamento { StatoPagamentoId = 3, StatoPagamento1 = "Fallito" },
                    new StatoPagamento { StatoPagamentoId = 4, StatoPagamento1 = "Rimborsato" }
                );
            }

            // Crea ordine di test
            if (!_context.Ordine.Any()) // ✅ Corretto: Ordini (plurale)
            {
                _context.Ordine.Add(new Ordine // ✅ Corretto: Ordini (plurale)
                {
                    OrdineId = 1,
                    DataCreazione = DateTime.Now,
                    StatoPagamentoId = 1, // In Attesa
                    Totale = 15.50m
                });
            }

            _context.SaveChanges();
        }

        [Fact]
        public void Constructor_WithValidParameters_InitializesCorrectly()
        {
            // Arrange & Act già fatte nel costruttore

            // Assert
            Assert.NotNull(_stripeService);
        }

        [Fact]
        public async Task CreatePaymentIntentAsync_WithInvalidOrderId_ThrowsException()
        {
            // Arrange
            var request = new StripePaymentRequestDTO
            {
                OrdineId = 999, // Ordine inesistente
                Amount = 1500, // 15.00€ in centesimi
                Currency = "eur",
                Description = "Test payment"
            };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _stripeService.CreatePaymentIntentAsync(request));
        }

        [Fact]
        public async Task CreatePaymentIntentAsync_WithValidRequest_ReturnsMockResponse()
        {
            // Arrange
            var request = new StripePaymentRequestDTO
            {
                OrdineId = 1,
                Amount = 1550, // 15.50€ in centesimi
                Currency = "eur",
                Description = "Test payment for order 1",
                CustomerEmail = "test@example.com"
            };

            // Act - Ora dovrebbe funzionare con la chiave mock
            var result = await _stripeService.CreatePaymentIntentAsync(request);

            // Assert - Verifica che restituisca una risposta mock
            Assert.NotNull(result);
            Assert.NotNull(result.ClientSecret);
            Assert.NotNull(result.PaymentIntentId);
            Assert.StartsWith("pi_mock_", result.PaymentIntentId); // ✅ Dovrebbe usare la modalità mock
            Assert.Equal("requires_payment_method", result.Status);
            Assert.Equal(request.Amount, result.Amount);
            Assert.Equal(request.Currency, result.Currency);
        }

        [Fact]
        public async Task ConfirmPaymentAsync_WithMockPaymentIntent_ReturnsTrue()
        {
            // Arrange - Crea prima un payment intent mock
            var request = new StripePaymentRequestDTO
            {
                OrdineId = 1,
                Amount = 1550,
                Currency = "eur"
            };
            var paymentResult = await _stripeService.CreatePaymentIntentAsync(request);
            var mockPaymentIntentId = paymentResult.PaymentIntentId;

            // Act
            var result = await _stripeService.ConfirmPaymentAsync(mockPaymentIntentId);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task ConfirmPaymentAsync_WithInvalidPaymentIntentId_ReturnsFalse()
        {
            // Arrange
            var invalidPaymentIntentId = "pi_invalid_123456";

            // Act
            var result = await _stripeService.ConfirmPaymentAsync(invalidPaymentIntentId);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task GetStatoPagamentoId_WithValidStato_ReturnsCorrectId()
        {
            // Act - Test di TUTTI gli stati
            var inAttesaId = await GetStatoPagamentoIdInternal("in_attesa");
            var pagatoId = await GetStatoPagamentoIdInternal("pagato");
            var fallitoId = await GetStatoPagamentoIdInternal("fallito");
            var rimborsatoId = await GetStatoPagamentoIdInternal("rimborsato");

            // Assert
            Assert.Equal(1, inAttesaId);
            Assert.Equal(2, pagatoId);
            Assert.Equal(3, fallitoId);
            Assert.Equal(4, rimborsatoId);
        }

        [Fact]
        public async Task GetStatoPagamentoId_WithInvalidStato_ReturnsDefaultId()
        {
            // Act
            var result = await GetStatoPagamentoIdInternal("stato_invalido");

            // Assert
            Assert.Equal(1, result); // Default a "In_Attesa"
        }

        [Fact]
        public async Task GetStatoPagamentoId_WithNullStato_ReturnsDefaultId()
        {
            // Act
            var result = await GetStatoPagamentoIdInternal(null);

            // Assert
            Assert.Equal(1, result); // Default a "In_Attesa"
        }

        [Fact]
        public async Task HandleWebhookAsync_WithMockKeys_ReturnsTrue()
        {
            // Arrange
            var json = "{}";
            var signature = "mock_signature";

            // Act - Con chiavi mock dovrebbe restituire true
            var result = await _stripeService.HandleWebhookAsync(json, signature);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task RefundPaymentAsync_WithMockKeys_ReturnsTrue()
        {
            // Arrange
            var mockPaymentIntentId = "pi_mock_123456";

            // Act - Con chiavi mock dovrebbe simulare il rimborso
            var result = await _stripeService.RefundPaymentAsync(mockPaymentIntentId);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task CreatePaymentIntentAsync_WithZeroAmount_ThrowsException()
        {
            // Arrange
            var request = new StripePaymentRequestDTO
            {
                OrdineId = 1,
                Amount = 0, // Importo zero non valido
                Currency = "eur"
            };

            // Act & Assert
            await Assert.ThrowsAsync<ApplicationException>(() =>
                _stripeService.CreatePaymentIntentAsync(request));
        }

        // Helper migliorato per testare metodi privati
        private async Task<int> GetStatoPagamentoIdInternal(string parameter)
        {
            var method = typeof(StripeServiceRepository).GetMethod(
                "GetStatoPagamentoId",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            if (method == null)
                throw new InvalidOperationException("Metodo GetStatoPagamentoId non trovato");

            try
            {
                var task = (Task<int>)method.Invoke(_stripeService, new object[] { parameter });
                return await task;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Errore durante l'invocazione del metodo: {ex.Message}", ex);
            }
        }

        public new void Dispose()
        {
            _context.Ordine.RemoveRange(_context.Ordine); // ✅ Corretto: Ordini (plurale)
            _context.StatoPagamento.RemoveRange(_context.StatoPagamento);
            _context.SaveChanges();
            base.Dispose();
        }
    }
}