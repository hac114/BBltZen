using Database;
using DTO;
using Microsoft.EntityFrameworkCore;
using Repository.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Service
{
    public class StatoPagamentoRepository : IStatoPagamentoRepository
    {

        private readonly BubbleTeaContext _context;
        public StatoPagamentoRepository(BubbleTeaContext context)
        {
            _context = context;
        }
        public async Task<IEnumerable<StatoPagamentoDTO>> GetAllAsync()
        {
            return await _context.StatoPagamento
                .Select(s => new StatoPagamentoDTO
                {
                    StatoPagamentoId = s.StatoPagamentoId,
                    StatoPagamento1 = s.StatoPagamento1, // Note the different property name
                    // Map all other properties from StatoPagamento entity to StatoPagamentoDTO
                })
                .ToListAsync();
        }

        public async Task<StatoPagamentoDTO> GetByIdAsync(int statoPagamentoId)
        {
            var statoPagamento = await _context.StatoPagamento.FindAsync(statoPagamentoId);
            if (statoPagamento == null) return null;

            return new StatoPagamentoDTO
            {
                StatoPagamentoId = statoPagamento.StatoPagamentoId,
                StatoPagamento1 = statoPagamento.StatoPagamento1, // Note the different property name
                // Map all other properties
            };
        }

        public async Task<StatoPagamentoDTO> GetByNomeAsync(string nomeStatoPagamento)
        {
            var statoPagamento = await _context.StatoPagamento
                .FirstOrDefaultAsync(s => s.StatoPagamento1 == nomeStatoPagamento);

            if (statoPagamento == null) return null;

            return new StatoPagamentoDTO
            {
                StatoPagamentoId = statoPagamento.StatoPagamentoId,
                StatoPagamento1 = statoPagamento.StatoPagamento1, // Note the different property name
                // Map all other properties
            };
        }

        public async Task AddAsync(StatoPagamentoDTO statoPagamentoDto)
        {
            var statoPagamento = new StatoPagamento
            {
                StatoPagamento1 = statoPagamentoDto.StatoPagamento1, // Note the different property name
                // Map all other properties from DTO to entity
            };

            await _context.StatoPagamento.AddAsync(statoPagamento);
            await _context.SaveChangesAsync();

            // Return the generated ID to the DTO
            statoPagamentoDto.StatoPagamentoId = statoPagamento.StatoPagamentoId;
        }

        public async Task UpdateAsync(StatoPagamentoDTO statoPagamentoDto)
        {
            var statoPagamento = await _context.StatoPagamento.FindAsync(statoPagamentoDto.StatoPagamentoId);
            if (statoPagamento == null)
                throw new ArgumentException("Stato pagamento not found");

            statoPagamento.StatoPagamento1 = statoPagamentoDto.StatoPagamento1; // Note the different property name
            // Update all other properties

            _context.StatoPagamento.Update(statoPagamento);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int statoPagamentoId)
        {
            var statoPagamento = await _context.StatoPagamento.FindAsync(statoPagamentoId);
            if (statoPagamento != null)
            {
                _context.StatoPagamento.Remove(statoPagamento);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> ExistsAsync(int statoPagamentoId)
        {
            return await _context.StatoPagamento.AnyAsync(s => s.StatoPagamentoId == statoPagamentoId);
        }
    }
}
