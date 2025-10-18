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
            var tavolo = new Database.Tavolo
            {
                Numero = 1,
                Zona = "Terrazza",
                Disponibile = true
            };
            _context.Tavolo.Add(tavolo);
            await _context.SaveChangesAsync();

            var clienteDto = new ClienteDTO
            {
                TavoloId = tavolo.TavoloId,
                DataCreazione = DateTime.Now,
                DataAggiornamento = DateTime.Now
            };

            // Act
            await _clienteRepository.AddAsync(clienteDto);

            // Assert
            var result = await _clienteRepository.GetByIdAsync(clienteDto.ClienteId);
            Assert.NotNull(result);
            Assert.Equal(tavolo.TavoloId, result.TavoloId);
        }

        [Fact]
        public async Task GetAllAsync_Should_Return_All_Clienti()
        {
            // Arrange
            var tavoli = new List<Database.Tavolo>
            {
                new Database.Tavolo { Numero = 1, Zona = "Terrazza", Disponibile = true },
                new Database.Tavolo { Numero = 2, Zona = "Interno", Disponibile = true }
            };
            _context.Tavolo.AddRange(tavoli);
            await _context.SaveChangesAsync();

            var clienti = new List<Database.Cliente>
            {
                new Database.Cliente { TavoloId = tavoli[0].TavoloId },
                new Database.Cliente { TavoloId = tavoli[1].TavoloId }
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
            var tavolo = new Database.Tavolo
            {
                Numero = 1,
                Zona = "Terrazza",
                Disponibile = true
            };
            _context.Tavolo.Add(tavolo);
            await _context.SaveChangesAsync();

            var cliente = new Database.Cliente { TavoloId = tavolo.TavoloId };
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
            var tavolo = new Database.Tavolo
            {
                Numero = 1,
                Zona = "Interno",
                Disponibile = true
            };
            _context.Tavolo.Add(tavolo);
            await _context.SaveChangesAsync();

            var cliente = new Database.Cliente { TavoloId = tavolo.TavoloId };
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
            var tavolo1 = new Database.Tavolo
            {
                Numero = 1,
                Zona = "Terrazza",
                Disponibile = true
            };
            var tavolo2 = new Database.Tavolo
            {
                Numero = 2,
                Zona = "Interno",
                Disponibile = true
            };
            _context.Tavolo.AddRange(tavolo1, tavolo2);
            await _context.SaveChangesAsync();

            var cliente = new Database.Cliente { TavoloId = tavolo1.TavoloId };
            _context.Cliente.Add(cliente);
            await _context.SaveChangesAsync();

            var updateDto = new ClienteDTO
            {
                ClienteId = cliente.ClienteId,
                TavoloId = tavolo2.TavoloId,
                DataAggiornamento = DateTime.Now
            };

            // Act
            await _clienteRepository.UpdateAsync(updateDto);

            // Assert
            var updated = await _clienteRepository.GetByIdAsync(cliente.ClienteId);
            Assert.NotNull(updated);
            Assert.Equal(tavolo2.TavoloId, updated.TavoloId);
        }

        [Fact]
        public async Task DeleteAsync_Should_Remove_Cliente()
        {
            // Arrange
            var tavolo = new Database.Tavolo
            {
                Numero = 1,
                Zona = "Terrazza",
                Disponibile = true
            };
            _context.Tavolo.Add(tavolo);
            await _context.SaveChangesAsync();

            var cliente = new Database.Cliente { TavoloId = tavolo.TavoloId };
            _context.Cliente.Add(cliente);
            await _context.SaveChangesAsync();

            // Act
            await _clienteRepository.DeleteAsync(cliente.ClienteId);

            // Assert
            var deleted = await _clienteRepository.GetByIdAsync(cliente.ClienteId);
            Assert.Null(deleted);
        }

        // I test rimanenti continuano allo stesso modo, rimuovendo QrCode...
    }
}