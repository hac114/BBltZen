using Database;
using DTO;
using DTO.Monitoring;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace BBltZen.IntegrationTests
{
    public class SistemaCacheControllerTest : IClassFixture<WebApplicationFactory<Program>>, IDisposable
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;
        private readonly BubbleTeaContext _context;

        public SistemaCacheControllerTest(WebApplicationFactory<Program> factory)
        {
            _factory = factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    // Configura services per testing
                });
            });

            _client = _factory.CreateClient();

            // Crea scope per accedere al database
            var scope = _factory.Services.CreateScope();
            _context = scope.ServiceProvider.GetRequiredService<BubbleTeaContext>();
        }

        [Fact]
        public async Task GetCacheMetrics_ShouldReturnOkWithValidMetrics()
        {
            // Act
            var response = await _client.GetAsync("/api/SistemaCache/metrics");

            // Assert
            response.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var content = await response.Content.ReadAsStringAsync();
            var metrics = JsonSerializer.Deserialize<CacheMetricsDTO>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            Assert.NotNull(metrics);
            Assert.True(metrics.HitRate >= 0);
            Assert.True(metrics.MissRate >= 0);
            Assert.True(metrics.HitRatePercentuale >= 0 && metrics.HitRatePercentuale <= 100);
            Assert.NotNull(metrics.MemoriaUtilizzataFormattata);
            Assert.True(metrics.UltimaEsecuzione <= DateTime.UtcNow.AddMinutes(5)); // Ragionevole
        }

        [Fact]
        public async Task GetBackgroundServiceStatus_ShouldReturnOkWithValidStatus()
        {
            // Act
            var response = await _client.GetAsync("/api/SistemaCache/background-service/status");

            // Assert
            response.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var content = await response.Content.ReadAsStringAsync();
            var status = JsonSerializer.Deserialize<BackgroundServiceStatusDTO>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            Assert.NotNull(status);
            Assert.Equal("CacheBackgroundService", status.ServiceName);
            Assert.NotNull(status.Status);
            Assert.True(status.LastExecution <= DateTime.UtcNow.AddMinutes(5));
            Assert.True(status.Uptime >= TimeSpan.Zero);
            Assert.True(status.ExecutionCount >= 0);
            Assert.True(status.ErrorCount >= 0);
        }

        [Fact]
        public async Task GetAsync_WithExistingKey_ShouldReturnValue()
        {
            // Arrange
            var testKey = "test_key_controller";
            var testValue = new { data = "test_value" };

            var setResponse = await _client.PostAsync($"/api/SistemaCache/set/{testKey}",
                new StringContent(JsonSerializer.Serialize(new { Valore = testValue, Durata = "00:30:00" }),
                Encoding.UTF8, "application/json"));
            setResponse.EnsureSuccessStatusCode();

            // Act
            var response = await _client.GetAsync($"/api/SistemaCache/get/{testKey}");

            // Assert
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            Assert.Contains("test_value", content);
        }

        [Fact]
        public async Task SetAsync_WithValidData_ShouldReturnSuccess()
        {
            // Arrange
            var testKey = "new_key_controller";
            var request = new
            {
                Valore = new { message = "Hello World" },
                Durata = "00:10:00"
            };

            // Act
            var response = await _client.PostAsync($"/api/SistemaCache/set/{testKey}",
                new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json"));

            // Assert
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            Assert.Contains("Successo", content);
            Assert.Contains("Cache impostata con successo", content);
        }

        [Fact]
        public async Task RemoveAsync_WithExistingKey_ShouldReturnSuccess()
        {
            // Arrange
            var testKey = "key_to_remove_controller";
            var setRequest = new { Valore = "value", Durata = "00:10:00" };

            await _client.PostAsync($"/api/SistemaCache/set/{testKey}",
                new StringContent(JsonSerializer.Serialize(setRequest), Encoding.UTF8, "application/json"));

            // Act
            var response = await _client.DeleteAsync($"/api/SistemaCache/remove/{testKey}");

            // Assert
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            Assert.Contains("Successo", content);
        }

        [Fact]
        public async Task ExistsAsync_WithExistingKey_ShouldReturnTrue()
        {
            // Arrange
            var testKey = "exists_key_controller";
            var setRequest = new { Valore = "value", Durata = "00:10:00" };

            await _client.PostAsync($"/api/SistemaCache/set/{testKey}",
                new StringContent(JsonSerializer.Serialize(setRequest), Encoding.UTF8, "application/json"));

            // Act
            var response = await _client.GetAsync($"/api/SistemaCache/exists/{testKey}");

            // Assert
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            Assert.Equal("true", content.Trim('"'));
        }

        [Fact]
        public async Task GetCacheInfo_ShouldReturnValidInfo()
        {
            // Act
            var response = await _client.GetAsync("/api/SistemaCache/info");

            // Assert
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            var info = JsonSerializer.Deserialize<CacheInfoDTO>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            Assert.NotNull(info);
            Assert.True(info.HitsTotali >= 0);
            Assert.True(info.MissesTotali >= 0);
            Assert.InRange(info.HitRatePercentuale, 0, 100);
        }

        [Fact]
        public async Task GetPerformanceStats_ShouldReturnValidStats()
        {
            // Act
            var response = await _client.GetAsync("/api/SistemaCache/performance");

            // Assert
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            var stats = JsonSerializer.Deserialize<CachePerformanceDTO>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            Assert.NotNull(stats);
            Assert.InRange(stats.HitRate, 0, 100);
            Assert.InRange(stats.MissRate, 0, 100);
            Assert.True(stats.DataRaccolta <= DateTime.UtcNow.AddMinutes(5));
        }

        [Fact]
        public async Task HealthCheck_ShouldReturnHealthyStatus()
        {
            // Act
            var response = await _client.GetAsync("/api/SistemaCache/health");

            // Assert
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            Assert.Contains("Status", content);
            Assert.Contains("Healthy", content);
        }

        [Fact]
        public async Task GetStatisticheCarrelloRealtime_ShouldReturnStatistiche()
        {
            // Act
            var response = await _client.GetAsync("/api/SistemaCache/carrello/realtime");

            // Assert
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            var statistiche = JsonSerializer.Deserialize<StatisticheCarrelloDTO>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            Assert.NotNull(statistiche);
            Assert.True(statistiche.TotaleOrdini >= 0);
            Assert.True(statistiche.DataRiferimento <= DateTime.UtcNow.AddDays(1));
        }

        [Fact]
        public async Task RefreshStatisticheCarrello_ShouldReturnSuccess()
        {
            // Act
            var response = await _client.PostAsync("/api/SistemaCache/carrello/refresh", null);

            // Assert
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            Assert.Contains("Successo", content);
        }

        public void Dispose()
        {
            _client?.Dispose();
            _context?.Dispose();
        }
    }
}