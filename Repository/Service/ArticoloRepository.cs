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
    public class ArticoloRepository : IArticoloRepository
    {
        private readonly BubbleTeaContext _context;

        public ArticoloRepository(BubbleTeaContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ArticoloDTO>> GetAllAsync()
        {
            return await _context.Articolo
                .AsNoTracking()
                .Select(a => new ArticoloDTO
                {
                    ArticoloId = a.ArticoloId,
                    Tipo = a.Tipo,
                    DataCreazione = a.DataCreazione,
                    DataAggiornamento = a.DataAggiornamento
                })
                .ToListAsync();
        }

        public async Task<ArticoloDTO?> GetByIdAsync(int articoloId)
        {
            var articolo = await _context.Articolo
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.ArticoloId == articoloId);

            if (articolo == null) return null;

            return new ArticoloDTO
            {
                ArticoloId = articolo.ArticoloId,
                Tipo = articolo.Tipo,
                DataCreazione = articolo.DataCreazione,
                DataAggiornamento = articolo.DataAggiornamento
            };
        }

        public async Task<IEnumerable<ArticoloDTO>> GetByTipoAsync(string tipo)
        {
            return await _context.Articolo
                .AsNoTracking()
                .Where(a => a.Tipo == tipo)
                .Select(a => new ArticoloDTO
                {
                    ArticoloId = a.ArticoloId,
                    Tipo = a.Tipo,
                    DataCreazione = a.DataCreazione,
                    DataAggiornamento = a.DataAggiornamento
                })
                .ToListAsync();
        }

        public async Task<IEnumerable<ArticoloDTO>> GetArticoliOrdinabiliAsync()
        {
            // Articoli che hanno almeno una specializzazione (BevandaStandard, BevandaCustom, Dolce) disponibile
            var articoliConBevandeStandard = _context.Articolo
                .Where(a => a.Tipo == "BS" && a.BevandaStandard != null &&
                           (a.BevandaStandard.Disponibile || a.BevandaStandard.SempreDisponibile));

            var articoliConBevandeCustom = _context.Articolo
                .Where(a => a.Tipo == "BC" && a.BevandaCustom != null);

            var articoliConDolci = _context.Articolo
                .Where(a => a.Tipo == "DOLCE" && a.Dolce != null && a.Dolce.Disponibile);

            var articoliOrdinabili = articoliConBevandeStandard
                .Union(articoliConBevandeCustom)
                .Union(articoliConDolci);

            return await articoliOrdinabili
                .AsNoTracking()
                .Select(a => new ArticoloDTO
                {
                    ArticoloId = a.ArticoloId,
                    Tipo = a.Tipo,
                    DataCreazione = a.DataCreazione,
                    DataAggiornamento = a.DataAggiornamento
                })
                .ToListAsync();
        }

        public async Task AddAsync(ArticoloDTO articoloDto)
        {
            var articolo = new Articolo
            {
                Tipo = articoloDto.Tipo,
                DataCreazione = DateTime.Now,
                DataAggiornamento = DateTime.Now
            };

            _context.Articolo.Add(articolo);
            await _context.SaveChangesAsync();

            // Aggiorna il DTO con i valori del database
            articoloDto.ArticoloId = articolo.ArticoloId;
            articoloDto.DataCreazione = articolo.DataCreazione;
            articoloDto.DataAggiornamento = articolo.DataAggiornamento;
        }

        public async Task UpdateAsync(ArticoloDTO articoloDto)
        {
            var articolo = await _context.Articolo
                .FirstOrDefaultAsync(a => a.ArticoloId == articoloDto.ArticoloId);

            if (articolo == null)
                throw new ArgumentException($"Articolo con ArticoloId {articoloDto.ArticoloId} non trovato");

            articolo.Tipo = articoloDto.Tipo;
            articolo.DataAggiornamento = DateTime.Now;

            await _context.SaveChangesAsync();

            articoloDto.DataAggiornamento = articolo.DataAggiornamento;
        }

        public async Task DeleteAsync(int articoloId)
        {
            var articolo = await _context.Articolo
                .FirstOrDefaultAsync(a => a.ArticoloId == articoloId);

            if (articolo != null)
            {
                _context.Articolo.Remove(articolo);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> ExistsAsync(int articoloId)
        {
            return await _context.Articolo
                .AnyAsync(a => a.ArticoloId == articoloId);
        }

        public async Task<bool> ExistsByTipoAsync(string tipo, int? excludeArticoloId = null)
        {
            var query = _context.Articolo.Where(a => a.Tipo == tipo);

            if (excludeArticoloId.HasValue)
            {
                query = query.Where(a => a.ArticoloId != excludeArticoloId.Value);
            }

            return await query.AnyAsync();
        }
    }
}
