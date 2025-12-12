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
                .AsNoTracking()
                .Select(s => new StatoPagamentoDTO
                {
                    StatoPagamentoId = s.StatoPagamentoId,
                    StatoPagamento1 = s.StatoPagamento1
                })
                .ToListAsync();
        }

        public async Task<StatoPagamentoDTO?> GetByIdAsync(int statoPagamentoId)
        {
            var statoPagamento = await _context.StatoPagamento
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.StatoPagamentoId == statoPagamentoId);

            if (statoPagamento == null) return null;

            return new StatoPagamentoDTO
            {
                StatoPagamentoId = statoPagamento.StatoPagamentoId,
                StatoPagamento1 = statoPagamento.StatoPagamento1
            };
        }

        public async Task<StatoPagamentoDTO?> GetByNomeAsync(string nomeStatoPagamento)
        {
            var statoPagamento = await _context.StatoPagamento
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.StatoPagamento1 == nomeStatoPagamento);

            if (statoPagamento == null) return null;

            return new StatoPagamentoDTO
            {
                StatoPagamentoId = statoPagamento.StatoPagamentoId,
                StatoPagamento1 = statoPagamento.StatoPagamento1
            };
        }

        public async Task<StatoPagamentoDTO> AddAsync(StatoPagamentoDTO statoPagamentoDto) // ✅ CORREGGI: ritorna DTO
        {
            if (statoPagamentoDto == null)
                throw new ArgumentNullException(nameof(statoPagamentoDto));

            var statoPagamento = new StatoPagamento
            {
                StatoPagamento1 = statoPagamentoDto.StatoPagamento1
            };

            await _context.StatoPagamento.AddAsync(statoPagamento);
            await _context.SaveChangesAsync();

            statoPagamentoDto.StatoPagamentoId = statoPagamento.StatoPagamentoId;
            return statoPagamentoDto; // ✅ AGGIUNGI return
        }

        public async Task UpdateAsync(StatoPagamentoDTO statoPagamentoDto)
        {
            if (statoPagamentoDto == null) // ✅ AGGIUNGI validazione
                throw new ArgumentNullException(nameof(statoPagamentoDto));

            var statoPagamento = await _context.StatoPagamento
                .FirstOrDefaultAsync(s => s.StatoPagamentoId == statoPagamentoDto.StatoPagamentoId);

            if (statoPagamento == null)
                throw new ArgumentException($"Stato pagamento con ID {statoPagamentoDto.StatoPagamentoId} non trovato");

            statoPagamento.StatoPagamento1 = statoPagamentoDto.StatoPagamento1;
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int statoPagamentoId)
        {
            var statoPagamento = await _context.StatoPagamento
                .FirstOrDefaultAsync(s => s.StatoPagamentoId == statoPagamentoId);

            if (statoPagamento != null)
            {
                _context.StatoPagamento.Remove(statoPagamento);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> ExistsAsync(int statoPagamentoId)
        {
            return await _context.StatoPagamento
                .AnyAsync(s => s.StatoPagamentoId == statoPagamentoId);
        }
    }
}