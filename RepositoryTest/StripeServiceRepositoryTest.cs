using BBltZen;
using DTO;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Repository.Interface;
using Repository.Service;
using Xunit;

namespace RepositoryTest
{
    public class StripeServiceRepositoryTest : BaseTest
    {
        private readonly IStripeServiceRepository _stripeService;  // ✅ USA INTERFACCIA
        private readonly Mock<ILogger<StripeServiceRepository>> _loggerMock;
        private readonly Mock<IOptions<StripeSettingsDTO>> _stripeSettingsMock;  // ✅ USA DTO

        public StripeServiceRepositoryTest()
        {
            // Mock per ILogger
            _loggerMock = new Mock<ILogger<StripeServiceRepository>>();

            // Mock per StripeSettingsDTO (usa placeholder per testing)
            _stripeSettingsMock = new Mock<IOptions<StripeSettingsDTO>>();
            _stripeSettingsMock.Setup(x => x.Value)
                .Returns(new StripeSettingsDTO
                {
                    SecretKey = "REPLACE_WITH_STRIPE_SECRET_KEY", // ✅ USA DTO
                    PublishableKey = "REPLACE_WITH_STRIPE_PUBLISHABLE_KEY",
                    WebhookSecret = "REPLACE_WITH_STRIPE_WEBHOOK_SECRET"
                });

            // ✅ INIZIALIZZA CON INTERFACCIA
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
            _context.Ordine.RemoveRange(_context.Ordine);
            _context.SaveChanges();

            // Crea ordine di test
            if (!_context.Ordine.Any())
            {
                _context.Ordine.Add(new Ordine
                {
                    OrdineId = 1,
                    DataCreazione = DateTime.UtcNow, // ✅ UTC PER COERENZA
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

            // Act
            var result = await _stripeService.CreatePaymentIntentAsync(request);

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.ClientSecret);
            Assert.NotNull(result.PaymentIntentId);
            Assert.StartsWith("pi_mock_", result.PaymentIntentId); // ✅ USA MODALITÀ MOCK
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
            Assert.False(result); // ✅ SILENT FAIL VERIFICATO
        }

        [Fact]
        public void GetStatoPagamentoId_WithValidStato_ReturnsCorrectId()
        {
            // ✅ CORREZIONE: METODO ORA È SINCRONO
            // Act - Test di TUTTI gli stati
            var inAttesaId = GetStatoPagamentoIdInternal("in_attesa");
            var pagatoId = GetStatoPagamentoIdInternal("pagato");
            var fallitoId = GetStatoPagamentoIdInternal("fallito");
            var rimborsatoId = GetStatoPagamentoIdInternal("rimborsato");

            // Assert
            Assert.Equal(1, inAttesaId);
            Assert.Equal(2, pagatoId);
            Assert.Equal(3, fallitoId);
            Assert.Equal(4, rimborsatoId);
        }

        [Fact]
        public void GetStatoPagamentoId_WithInvalidStato_ReturnsDefaultId()
        {
            // Act
            var result = GetStatoPagamentoIdInternal("stato_invalido");

            // Assert
            Assert.Equal(1, result); // Default a "In_Attesa"
        }

        [Fact]
        public void GetStatoPagamentoId_WithNullStato_ReturnsDefaultId()
        {
            // Act
            var result = GetStatoPagamentoIdInternal(null!); // ✅ USA NULL-FORGIVING OPERATOR

            // Assert
            Assert.Equal(1, result); // Default a "In_Attesa"
        }

        [Fact]
        public async Task HandleWebhookAsync_WithMockKeys_ReturnsTrue()
        {
            // Arrange
            var json = "{}";
            var signature = "mock_signature";

            // Act
            var result = await _stripeService.HandleWebhookAsync(json, signature);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task RefundPaymentAsync_WithMockKeys_ReturnsTrue()
        {
            // Arrange
            var mockPaymentIntentId = "pi_mock_123456";

            // Act
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
            await Assert.ThrowsAsync<ArgumentException>(() =>  // ✅ CORRETTO: ArgumentException
                _stripeService.CreatePaymentIntentAsync(request));
        }

        [Fact]
        public async Task HandleWebhookAsync_WithEmptyParameters_ReturnsFalse()
        {
            // Arrange
            var emptyJson = "";
            var emptySignature = "";

            // Act
            var result = await _stripeService.HandleWebhookAsync(emptyJson, emptySignature);

            // Assert
            Assert.False(result); // ✅ SILENT FAIL VERIFICATO
        }

        [Fact]
        public async Task RefundPaymentAsync_WithEmptyPaymentIntentId_ReturnsFalse()
        {
            // Arrange
            var emptyPaymentIntentId = "";

            // Act
            var result = await _stripeService.RefundPaymentAsync(emptyPaymentIntentId);

            // Assert
            Assert.False(result); // ✅ SILENT FAIL VERIFICATO
        }

        // ✅ HELPER CORRETTO CON GESTIONE NULL SICURA
        private int GetStatoPagamentoIdInternal(string? parameter)
        {
            var method = typeof(StripeServiceRepository).GetMethod(
                "GetStatoPagamentoId",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            if (method == null)
                throw new InvalidOperationException("Metodo GetStatoPagamentoId non trovato");

            try
            {
                var result = method.Invoke(_stripeService, new object?[] { parameter });

                // ✅ GESTIONE ESPLICITA DEL POSSIBILE NULL
                if (result == null)
                    throw new InvalidOperationException("Il metodo ha restituito null");

                return (int)result;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Errore durante l'invocazione del metodo: {ex.Message}", ex);
            }
        }
    }
}