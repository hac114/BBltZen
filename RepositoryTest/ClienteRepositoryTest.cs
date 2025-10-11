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
            await CleanTableAsync<Database.Tavolo>();
            await CleanTableAsync<Database.Cliente>();

            // Crea un tavolo prima
            var tavolo = new Database.Tavolo
            {
                Numero = 1,
                Zona = "Terrazza",
                QrCode = "QR001",
                Disponibile = true
            };
            _context.Tavolo.Add(tavolo);
            await _context.SaveChangesAsync();

            var clienteDto = new ClienteDTO
            {
                TavoloId = tavolo.TavoloId
            };

            // Act
            await _clienteRepository.AddAsync(clienteDto);

            // Assert
            var result = await _clienteRepository.GetByIdAsync(clienteDto.ClienteId);
            Assert.NotNull(result);
            Assert.Equal(tavolo.TavoloId, result.TavoloId);
            Assert.True(result.ClienteId > 0);
            Assert.NotNull(result.DataCreazione);
            Assert.NotNull(result.DataAggiornamento);
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Correct_Cliente()
        {
            // Arrange
            await CleanTableAsync<Database.Tavolo>();
            await CleanTableAsync<Database.Cliente>();

            var tavolo = new Database.Tavolo
            {
                Numero = 2,
                Zona = "Interno",
                QrCode = "QR002",
                Disponibile = true
            };
            _context.Tavolo.Add(tavolo);
            await _context.SaveChangesAsync();

            var clienteDto = new ClienteDTO
            {
                TavoloId = tavolo.TavoloId
            };
            await _clienteRepository.AddAsync(clienteDto);

            // Act
            var result = await _clienteRepository.GetByIdAsync(clienteDto.ClienteId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(clienteDto.ClienteId, result.ClienteId);
            Assert.Equal(tavolo.TavoloId, result.TavoloId);
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_For_NonExisting_Id()
        {
            // Act
            var result = await _clienteRepository.GetByIdAsync(999);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetByTavoloIdAsync_Should_Return_Correct_Cliente()
        {
            // Arrange
            await CleanTableAsync<Database.Tavolo>();
            await CleanTableAsync<Database.Cliente>();

            var tavolo = new Database.Tavolo
            {
                Numero = 3,
                Zona = "Terrazza",
                QrCode = "QR003",
                Disponibile = true
            };
            _context.Tavolo.Add(tavolo);
            await _context.SaveChangesAsync();

            var clienteDto = new ClienteDTO
            {
                TavoloId = tavolo.TavoloId
            };
            await _clienteRepository.AddAsync(clienteDto);

            // Act
            var result = await _clienteRepository.GetByTavoloIdAsync(tavolo.TavoloId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(tavolo.TavoloId, result.TavoloId);
            Assert.Equal(clienteDto.ClienteId, result.ClienteId);
        }

        [Fact]
        public async Task GetByTavoloIdAsync_Should_Return_Null_For_NonExisting_TavoloId()
        {
            // Act
            var result = await _clienteRepository.GetByTavoloIdAsync(999);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetAllAsync_Should_Return_All_Clienti()
        {
            // Arrange
            await CleanTableAsync<Database.Tavolo>();
            await CleanTableAsync<Database.Cliente>();

            var tavoli = new List<Database.Tavolo>
            {
                new Database.Tavolo { Numero = 1, Zona = "Terrazza", QrCode = "QR001", Disponibile = true },
                new Database.Tavolo { Numero = 2, Zona = "Interno", QrCode = "QR002", Disponibile = true }
            };
            _context.Tavolo.AddRange(tavoli);
            await _context.SaveChangesAsync();

            var clienti = new List<ClienteDTO>
            {
                new ClienteDTO { TavoloId = tavoli[0].TavoloId },
                new ClienteDTO { TavoloId = tavoli[1].TavoloId }
            };

            foreach (var cliente in clienti)
            {
                await _clienteRepository.AddAsync(cliente);
            }

            // Act
            var result = await _clienteRepository.GetAllAsync();

            // Assert
            Assert.Equal(2, result.Count());
        }

        [Fact]
        public async Task UpdateAsync_Should_Update_Cliente_Correctly()
        {
            // Arrange
            await CleanTableAsync<Database.Tavolo>();
            await CleanTableAsync<Database.Cliente>();

            var tavolo1 = new Database.Tavolo { Numero = 1, Zona = "Terrazza", QrCode = "QR001", Disponibile = true };
            var tavolo2 = new Database.Tavolo { Numero = 2, Zona = "Interno", QrCode = "QR002", Disponibile = true };
            _context.Tavolo.AddRange(tavolo1, tavolo2);
            await _context.SaveChangesAsync();

            var clienteDto = new ClienteDTO
            {
                TavoloId = tavolo1.TavoloId
            };
            await _clienteRepository.AddAsync(clienteDto);

            var updateDto = new ClienteDTO
            {
                ClienteId = clienteDto.ClienteId,
                TavoloId = tavolo2.TavoloId
            };

            // Act
            await _clienteRepository.UpdateAsync(updateDto);

            // Assert
            var updated = await _clienteRepository.GetByIdAsync(clienteDto.ClienteId);
            Assert.NotNull(updated);
            Assert.Equal(tavolo2.TavoloId, updated.TavoloId);
        }

        [Fact]
        public async Task UpdateAsync_Should_Throw_For_NonExisting_Id()
        {
            // Arrange
            var updateDto = new ClienteDTO
            {
                ClienteId = 999,
                TavoloId = 1
            };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _clienteRepository.UpdateAsync(updateDto));
        }

        [Fact]
        public async Task DeleteAsync_Should_Remove_Cliente()
        {
            // Arrange
            await CleanTableAsync<Database.Tavolo>();
            await CleanTableAsync<Database.Cliente>();

            var tavolo = new Database.Tavolo { Numero = 1, Zona = "Interno", QrCode = "QR001", Disponibile = true };
            _context.Tavolo.Add(tavolo);
            await _context.SaveChangesAsync();

            var clienteDto = new ClienteDTO
            {
                TavoloId = tavolo.TavoloId
            };
            await _clienteRepository.AddAsync(clienteDto);

            // Act
            await _clienteRepository.DeleteAsync(clienteDto.ClienteId);

            // Assert
            var deleted = await _clienteRepository.GetByIdAsync(clienteDto.ClienteId);
            Assert.Null(deleted);
        }

        [Fact]
        public async Task DeleteAsync_Should_Not_Throw_For_NonExisting_Id()
        {
            // Act & Assert - Non dovrebbe lanciare eccezioni
            await _clienteRepository.DeleteAsync(999);
        }

        [Fact]
        public async Task ExistsAsync_Should_Return_True_For_Existing_Cliente()
        {
            // Arrange
            await CleanTableAsync<Database.Tavolo>();
            await CleanTableAsync<Database.Cliente>();

            var tavolo = new Database.Tavolo { Numero = 1, Zona = "Terrazza", QrCode = "QR001", Disponibile = true };
            _context.Tavolo.Add(tavolo);
            await _context.SaveChangesAsync();

            var clienteDto = new ClienteDTO
            {
                TavoloId = tavolo.TavoloId
            };
            await _clienteRepository.AddAsync(clienteDto);

            // Act
            var exists = await _clienteRepository.ExistsAsync(clienteDto.ClienteId);

            // Assert
            Assert.True(exists);
        }

        [Fact]
        public async Task ExistsAsync_Should_Return_False_For_NonExisting_Cliente()
        {
            // Act
            var exists = await _clienteRepository.ExistsAsync(999);

            // Assert
            Assert.False(exists);
        }

        [Fact]
        public async Task ExistsByTavoloIdAsync_Should_Return_True_For_Existing_TavoloId()
        {
            // Arrange
            await CleanTableAsync<Database.Tavolo>();
            await CleanTableAsync<Database.Cliente>();

            var tavolo = new Database.Tavolo { Numero = 1, Zona = "Terrazza", QrCode = "QR001", Disponibile = true };
            _context.Tavolo.Add(tavolo);
            await _context.SaveChangesAsync();

            var clienteDto = new ClienteDTO
            {
                TavoloId = tavolo.TavoloId
            };
            await _clienteRepository.AddAsync(clienteDto);

            // Act
            var exists = await _clienteRepository.ExistsByTavoloIdAsync(tavolo.TavoloId);

            // Assert
            Assert.True(exists);
        }

        [Fact]
        public async Task ExistsByTavoloIdAsync_Should_Return_False_For_NonExisting_TavoloId()
        {
            // Act
            var exists = await _clienteRepository.ExistsByTavoloIdAsync(999);

            // Assert
            Assert.False(exists);
        }

        [Fact]
        public async Task AddAsync_Should_Assign_Generated_Id_And_Timestamps()
        {
            // Arrange
            await CleanTableAsync<Database.Tavolo>();
            await CleanTableAsync<Database.Cliente>();

            var tavolo = new Database.Tavolo { Numero = 1, Zona = "Interno", QrCode = "QR001", Disponibile = true };
            _context.Tavolo.Add(tavolo);
            await _context.SaveChangesAsync();

            var clienteDto = new ClienteDTO
            {
                TavoloId = tavolo.TavoloId
            };

            // Act
            await _clienteRepository.AddAsync(clienteDto);

            // Assert
            Assert.True(clienteDto.ClienteId > 0);
            Assert.NotNull(clienteDto.DataCreazione);
            Assert.NotNull(clienteDto.DataAggiornamento);
        }
    }
}