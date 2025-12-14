using BBltZen;
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
    public class ClienteRepositoryTest : BaseTest
    {
        private readonly IClienteRepository _clienteRepository;

        public ClienteRepositoryTest()
        {
            _clienteRepository = new ClienteRepository(_context);
        }

        [Fact]
        public async Task AddAsync_Should_Add_Cliente()
        {
            // Arrange
            var tavolo = new Tavolo
            {
                Numero = 1,
                Zona = "Terrazza",
                Disponibile = true
            };
            _context.Tavolo.Add(tavolo);
            await _context.SaveChangesAsync();

            var clienteDto = new ClienteDTO
            {
                TavoloId = tavolo.TavoloId
                // ✅ RIMOSSO: DataCreazione e DataAggiornamento (vengono settati dal repository)
            };

            // Act
            var result = await _clienteRepository.AddAsync(clienteDto); // ✅ CAMBIATO: assegna il risultato

            // Assert
            Assert.NotNull(result);
            Assert.True(result.ClienteId > 0); // ✅ VERIFICA ID generato
            Assert.Equal(tavolo.TavoloId, result.TavoloId);
        }

        [Fact]
        public async Task GetAllAsync_Should_Return_All_Clienti()
        {
            // Arrange
            var tavoli = new List<Tavolo>
            {
                new Tavolo { Numero = 1, Zona = "Terrazza", Disponibile = true },
                new Tavolo { Numero = 2, Zona = "Interno", Disponibile = true }
            };
            _context.Tavolo.AddRange(tavoli);
            await _context.SaveChangesAsync();

            var clienti = new List<Cliente>
            {
                new Cliente { TavoloId = tavoli[0].TavoloId },
                new Cliente { TavoloId = tavoli[1].TavoloId }
            };
            _context.Cliente.AddRange(clienti);
            await _context.SaveChangesAsync();

            // Act
            var result = await _clienteRepository.GetAllAsync();

            // Assert
            Assert.Equal(2, result.Count());
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Correct_Cliente()
        {
            // Arrange
            var tavolo = new Tavolo
            {
                Numero = 1,
                Zona = "Terrazza",
                Disponibile = true
            };
            _context.Tavolo.Add(tavolo);
            await _context.SaveChangesAsync();

            var cliente = new Cliente { TavoloId = tavolo.TavoloId };
            _context.Cliente.Add(cliente);
            await _context.SaveChangesAsync();

            // Act
            var result = await _clienteRepository.GetByIdAsync(cliente.ClienteId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(cliente.ClienteId, result.ClienteId);
        }

        [Fact]
        public async Task GetByTavoloIdAsync_Should_Return_Correct_Cliente()
        {
            // Arrange
            var tavolo = new Tavolo
            {
                Numero = 1,
                Zona = "Interno",
                Disponibile = true
            };
            _context.Tavolo.Add(tavolo);
            await _context.SaveChangesAsync();

            var cliente = new Cliente { TavoloId = tavolo.TavoloId };
            _context.Cliente.Add(cliente);
            await _context.SaveChangesAsync();

            // Act
            var result = await _clienteRepository.GetByTavoloIdAsync(tavolo.TavoloId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(tavolo.TavoloId, result.TavoloId);
        }

        [Fact]
        public async Task UpdateAsync_Should_Update_Cliente()
        {
            // Arrange
            var tavolo1 = new Tavolo { Numero = 1, Zona = "Terrazza", Disponibile = true };
            var tavolo2 = new Tavolo { Numero = 2, Zona = "Interno", Disponibile = true };
            _context.Tavolo.AddRange(tavolo1, tavolo2);
            await _context.SaveChangesAsync();

            var cliente = new Cliente { TavoloId = tavolo1.TavoloId };
            _context.Cliente.Add(cliente);
            await _context.SaveChangesAsync();

            var updateDto = new ClienteDTO
            {
                ClienteId = cliente.ClienteId,
                TavoloId = tavolo2.TavoloId
                // ✅ RIMOSSO: DataAggiornamento (viene settato dal repository)
            };

            // Act
            await _clienteRepository.UpdateAsync(updateDto);

            // Assert
            var updated = await _clienteRepository.GetByIdAsync(cliente.ClienteId);
            Assert.NotNull(updated);
            Assert.Equal(tavolo2.TavoloId, updated.TavoloId);
            // ✅ POTREMMO AGGIUNGERE: verifica che DataAggiornamento sia stato aggiornato
        }

        [Fact]
        public async Task DeleteAsync_Should_Remove_Cliente()
        {
            // Arrange
            var tavolo = new Tavolo
            {
                Numero = 1,
                Zona = "Terrazza",
                Disponibile = true
            };
            _context.Tavolo.Add(tavolo);
            await _context.SaveChangesAsync();

            var cliente = new Cliente { TavoloId = tavolo.TavoloId };
            _context.Cliente.Add(cliente);
            await _context.SaveChangesAsync();

            // Act
            await _clienteRepository.DeleteAsync(cliente.ClienteId);

            // Assert
            var deleted = await _clienteRepository.GetByIdAsync(cliente.ClienteId);
            Assert.Null(deleted);
        }

        [Fact]
        public async Task AddAsync_Should_Set_Correct_Timestamps()
        {
            // Arrange
            var tavolo = new Tavolo { Numero = 1, Zona = "Terrazza", Disponibile = true };
            _context.Tavolo.Add(tavolo);
            await _context.SaveChangesAsync();

            var clienteDto = new ClienteDTO { TavoloId = tavolo.TavoloId };

            // Act
            var result = await _clienteRepository.AddAsync(clienteDto);

            // Assert - ✅ USA ToString("yyyy-MM-dd HH:mm:ss") per confronto senza millisecondi
            Assert.Equal(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                         result.DataCreazione.ToString("yyyy-MM-dd HH:mm:ss"));
            Assert.Equal(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                         result.DataAggiornamento.ToString("yyyy-MM-dd HH:mm:ss"));
            Assert.Equal(result.DataCreazione.ToString("yyyy-MM-dd HH:mm:ss"),
                         result.DataAggiornamento.ToString("yyyy-MM-dd HH:mm:ss"));
        }

        [Fact]
        public async Task AddAsync_Should_Set_Correct_Timestamps_WithTolerance()
        {
            // Arrange
            var tavolo = new Tavolo { Numero = 1, Zona = "Terrazza", Disponibile = true };
            _context.Tavolo.Add(tavolo);
            await _context.SaveChangesAsync();

            var clienteDto = new ClienteDTO { TavoloId = tavolo.TavoloId };

            // Act
            var result = await _clienteRepository.AddAsync(clienteDto);

            // Assert - ✅ CONFRONTO CON TOLLERANZA DI 1 SECONDO
            var timeTolerance = TimeSpan.FromSeconds(1);

            Assert.True((DateTime.Now - result.DataCreazione).Duration() <= timeTolerance);
            Assert.True((DateTime.Now - result.DataAggiornamento).Duration() <= timeTolerance);
            Assert.Equal(result.DataCreazione.ToString("yyyy-MM-dd HH:mm:ss"),
                         result.DataAggiornamento.ToString("yyyy-MM-dd HH:mm:ss"));
        }

        [Fact]
        public async Task UpdateAsync_Should_Update_DataAggiornamento()
        {
            // Arrange
            var tavolo1 = new Tavolo { Numero = 1, Zona = "Terrazza", Disponibile = true };
            var tavolo2 = new Tavolo { Numero = 2, Zona = "Interno", Disponibile = true };
            _context.Tavolo.AddRange(tavolo1, tavolo2);
            await _context.SaveChangesAsync();

            var cliente = new Cliente { TavoloId = tavolo1.TavoloId };
            _context.Cliente.Add(cliente);
            await _context.SaveChangesAsync();

            var originalUpdateTime = cliente.DataAggiornamento;

            var updateDto = new ClienteDTO
            {
                ClienteId = cliente.ClienteId,
                TavoloId = tavolo2.TavoloId
            };

            // Act - ✅ ATTENDI 1ms per essere sicuro del cambiamento
            await Task.Delay(1);
            await _clienteRepository.UpdateAsync(updateDto);

            // Assert - ✅ USA Compare() invece di operatori diretti
            var updated = await _clienteRepository.GetByIdAsync(cliente.ClienteId);
            Assert.NotNull(updated);
            Assert.True(DateTime.Compare(updated.DataAggiornamento, originalUpdateTime) > 0);
        }

        [Fact]
        public async Task AddAsync_With_Occupied_Tavolo_Should_Throw_Exception()
        {
            // Arrange
            var tavolo = new Tavolo { Numero = 1, Zona = "Terrazza", Disponibile = true };
            _context.Tavolo.Add(tavolo);
            await _context.SaveChangesAsync();

            // Crea primo cliente per il tavolo
            var primoCliente = new Cliente { TavoloId = tavolo.TavoloId };
            _context.Cliente.Add(primoCliente);
            await _context.SaveChangesAsync();

            var secondoClienteDto = new ClienteDTO { TavoloId = tavolo.TavoloId };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _clienteRepository.AddAsync(secondoClienteDto));
        }

        [Fact]
        public async Task GetByIdAsync_With_InvalidId_Should_Return_Null()
        {
            // Act
            var result = await _clienteRepository.GetByIdAsync(999);

            // Assert
            Assert.Null(result);
        }
    }
}