using Database;
using DTO;
using Microsoft.EntityFrameworkCore;
using Repository.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Repository.Service
{
    public class PreferitiClienteRepository : IPreferitiClienteRepository
    {
        private readonly BubbleTeaContext _context;

        public PreferitiClienteRepository(BubbleTeaContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<PreferitiClienteDTO>> GetAllAsync()
        {
            return await _context.PreferitiCliente
                .Include(p => p.Cliente)
                .Include(p => p.Bevanda)
                .Select(p => new PreferitiClienteDTO
                {
                    PreferitoId = p.PreferitoId,
                    ClienteId = p.ClienteId,
                    BevandaId = p.BevandaId,
                    DataAggiunta = p.DataAggiunta
                })
                .ToListAsync();
        }

        public async Task<PreferitiClienteDTO?> GetByIdAsync(int id)
        {
            var preferito = await _context.PreferitiCliente
                .Include(p => p.Cliente)
                .Include(p => p.Bevanda)
                .FirstOrDefaultAsync(p => p.PreferitoId == id);

            if (preferito == null) return null;

            return new PreferitiClienteDTO
            {
                PreferitoId = preferito.PreferitoId,
                ClienteId = preferito.ClienteId,
                BevandaId = preferito.BevandaId,
                DataAggiunta = preferito.DataAggiunta
            };
        }

        public async Task<IEnumerable<PreferitiClienteDTO>> GetByClienteIdAsync(int clienteId)
        {
            return await _context.PreferitiCliente
                .Where(p => p.ClienteId == clienteId)
                .Include(p => p.Bevanda)
                .Select(p => new PreferitiClienteDTO
                {
                    PreferitoId = p.PreferitoId,
                    ClienteId = p.ClienteId,
                    BevandaId = p.BevandaId,
                    DataAggiunta = p.DataAggiunta
                })
                .ToListAsync();
        }

        public async Task<IEnumerable<PreferitiClienteDTO>> GetByBevandaIdAsync(int bevandaId)
        {
            return await _context.PreferitiCliente
                .Where(p => p.BevandaId == bevandaId)
                .Include(p => p.Cliente)
                .Select(p => new PreferitiClienteDTO
                {
                    PreferitoId = p.PreferitoId,
                    ClienteId = p.ClienteId,
                    BevandaId = p.BevandaId,
                    DataAggiunta = p.DataAggiunta
                })
                .ToListAsync();
        }

        public async Task<PreferitiClienteDTO?> GetByClienteAndBevandaAsync(int clienteId, int bevandaId)
        {
            var preferito = await _context.PreferitiCliente
                .Include(p => p.Cliente)
                .Include(p => p.Bevanda)
                .FirstOrDefaultAsync(p => p.ClienteId == clienteId && p.BevandaId == bevandaId);

            if (preferito == null) return null;

            return new PreferitiClienteDTO
            {
                PreferitoId = preferito.PreferitoId,
                ClienteId = preferito.ClienteId,
                BevandaId = preferito.BevandaId,
                DataAggiunta = preferito.DataAggiunta
            };
        }

        public async Task AddAsync(PreferitiClienteDTO preferitoDto)
        {
            var preferito = new PreferitiCliente
            {
                ClienteId = preferitoDto.ClienteId,
                BevandaId = preferitoDto.BevandaId,
                DataAggiunta = DateTime.Now
            };

            _context.PreferitiCliente.Add(preferito);
            await _context.SaveChangesAsync();

            preferitoDto.PreferitoId = preferito.PreferitoId;
            preferitoDto.DataAggiunta = preferito.DataAggiunta;
        }

        public async Task UpdateAsync(PreferitiClienteDTO preferitoDto)
        {
            var preferito = await _context.PreferitiCliente.FindAsync(preferitoDto.PreferitoId);
            if (preferito == null)
                throw new ArgumentException("Preferito not found");

            preferito.ClienteId = preferitoDto.ClienteId;
            preferito.BevandaId = preferitoDto.BevandaId;
            preferito.DataAggiunta = preferitoDto.DataAggiunta;

            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var preferito = await _context.PreferitiCliente.FindAsync(id);
            if (preferito != null)
            {
                _context.PreferitiCliente.Remove(preferito);
                await _context.SaveChangesAsync();
            }
        }

        public async Task DeleteByClienteAndBevandaAsync(int clienteId, int bevandaId)
        {
            var preferito = await _context.PreferitiCliente
                .FirstOrDefaultAsync(p => p.ClienteId == clienteId && p.BevandaId == bevandaId);

            if (preferito != null)
            {
                _context.PreferitiCliente.Remove(preferito);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.PreferitiCliente.AnyAsync(p => p.PreferitoId == id);
        }

        public async Task<bool> ExistsByClienteAndBevandaAsync(int clienteId, int bevandaId)
        {
            return await _context.PreferitiCliente
                .AnyAsync(p => p.ClienteId == clienteId && p.BevandaId == bevandaId);
        }

        public async Task<int> GetCountByClienteAsync(int clienteId)
        {
            return await _context.PreferitiCliente
                .Where(p => p.ClienteId == clienteId)
                .CountAsync();
        }
    }
}
