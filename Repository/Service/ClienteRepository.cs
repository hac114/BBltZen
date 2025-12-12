using Database;
using Database.Models;
using DTO;
using Microsoft.EntityFrameworkCore;
using Repository.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Repository.Service
{
    public class ClienteRepository : IClienteRepository
    {
        private readonly BubbleTeaContext _context;

        public ClienteRepository(BubbleTeaContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ClienteDTO>> GetAllAsync()
        {
            return await _context.Cliente
                .Select(c => new ClienteDTO
                {
                    ClienteId = c.ClienteId,
                    TavoloId = c.TavoloId,
                    DataCreazione = c.DataCreazione,
                    DataAggiornamento = c.DataAggiornamento
                })
                .ToListAsync();
        }

        public async Task<ClienteDTO?> GetByIdAsync(int id)
        {
            var cliente = await _context.Cliente
                .FirstOrDefaultAsync(c => c.ClienteId == id);

            if (cliente == null) return null;

            return new ClienteDTO
            {
                ClienteId = cliente.ClienteId,
                TavoloId = cliente.TavoloId,
                DataCreazione = cliente.DataCreazione,
                DataAggiornamento = cliente.DataAggiornamento
            };
        }

        public async Task<ClienteDTO?> GetByTavoloIdAsync(int tavoloId)
        {
            var cliente = await _context.Cliente
                .FirstOrDefaultAsync(c => c.TavoloId == tavoloId);

            if (cliente == null) return null;

            return new ClienteDTO
            {
                ClienteId = cliente.ClienteId,
                TavoloId = cliente.TavoloId,
                DataCreazione = cliente.DataCreazione,
                DataAggiornamento = cliente.DataAggiornamento
            };
        }

        public async Task<ClienteDTO> AddAsync(ClienteDTO clienteDto) // ✅ CAMBIATO: ritorna ClienteDTO
        {
            // ✅ Validazione input
            if (clienteDto == null)
                throw new ArgumentNullException(nameof(clienteDto));

            // ✅ Controllo se il tavolo è già occupato
            if (await ExistsByTavoloIdAsync(clienteDto.TavoloId))
                throw new ArgumentException($"Il tavolo {clienteDto.TavoloId} è già associato a un altro cliente");

            var cliente = new Cliente
            {
                TavoloId = clienteDto.TavoloId,
                DataCreazione = DateTime.Now,
                DataAggiornamento = DateTime.Now
            };

            _context.Cliente.Add(cliente);
            await _context.SaveChangesAsync();

            // ✅ Aggiorna DTO con ID generato dal database
            clienteDto.ClienteId = cliente.ClienteId;
            clienteDto.DataCreazione = cliente.DataCreazione;
            clienteDto.DataAggiornamento = cliente.DataAggiornamento;

            return clienteDto; // ✅ IMPORTANTE: ritorna il DTO aggiornato
        }

        public async Task UpdateAsync(ClienteDTO clienteDto)
        {
            var cliente = await _context.Cliente.FindAsync(clienteDto.ClienteId);
            if (cliente == null)
                throw new ArgumentException("Cliente not found");

            cliente.TavoloId = clienteDto.TavoloId;
            cliente.DataAggiornamento = DateTime.Now;

            await _context.SaveChangesAsync();

            clienteDto.DataAggiornamento = cliente.DataAggiornamento;
        }

        public async Task DeleteAsync(int id)
        {
            var cliente = await _context.Cliente.FindAsync(id);
            if (cliente != null)
            {
                _context.Cliente.Remove(cliente);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Cliente.AnyAsync(c => c.ClienteId == id);
        }

        public async Task<bool> ExistsByTavoloIdAsync(int tavoloId)
        {
            return await _context.Cliente.AnyAsync(c => c.TavoloId == tavoloId);
        }
    }
}