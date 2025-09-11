using DTO;
using Microsoft.EntityFrameworkCore;
using Repository.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Database;

namespace Repository.Service
{
    public class DolceRepository : IDolceRepository
    {
        private readonly BubbleTeaContext _context;

        public DolceRepository(BubbleTeaContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<DolceDTO>> GetAllAsync()
        {
            return await _context.Dolce
                .AsNoTracking()
                .Select(d => new DolceDTO
                {
                    ArticoloId = d.ArticoloId,
                    Nome = d.Nome,
                    Prezzo = d.Prezzo,
                    Descrizione = d.Descrizione,
                    ImmagineUrl = d.ImmagineUrl,
                    Disponibile = d.Disponibile,
                    Priorita = d.Priorita,
                    DataCreazione = d.DataCreazione,
                    DataAggiornamento = d.DataAggiornamento
                })
                .ToListAsync();
        }

        public async Task<DolceDTO?> GetByIdAsync(int id)
        {
            var dolce = await _context.Dolce
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.ArticoloId == id);

            if (dolce == null) return null;

            return new DolceDTO
            {
                ArticoloId = dolce.ArticoloId,
                Nome = dolce.Nome,
                Prezzo = dolce.Prezzo,
                Descrizione = dolce.Descrizione,
                ImmagineUrl = dolce.ImmagineUrl,
                Disponibile = dolce.Disponibile,
                Priorita = dolce.Priorita,
                DataCreazione = dolce.DataCreazione,
                DataAggiornamento = dolce.DataAggiornamento
            };
        }

        public async Task<DolceDTO> AddAsync(DolceDTO entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            // Validazioni
            if (string.IsNullOrWhiteSpace(entity.Nome))
                throw new ArgumentException("Nome is required");

            if (entity.Prezzo <= 0)
                throw new ArgumentException("Prezzo must be greater than 0");

            var dolce = new Dolce
            {
                Nome = entity.Nome,
                Prezzo = entity.Prezzo,
                Descrizione = entity.Descrizione,
                ImmagineUrl = entity.ImmagineUrl,
                Disponibile = entity.Disponibile,
                Priorita = entity.Priorita,
                DataCreazione = DateTime.Now,
                DataAggiornamento = DateTime.Now
            };

            await _context.Dolce.AddAsync(dolce);
            await _context.SaveChangesAsync();

            // Aggiorna il DTO con l'ID generato
            entity.ArticoloId = dolce.ArticoloId;
            entity.DataCreazione = dolce.DataCreazione;
            entity.DataAggiornamento = dolce.DataAggiornamento;

            return entity;
        }

        public async Task UpdateAsync(DolceDTO entity)
        {
            if (entity == null || entity.ArticoloId == 0)
                throw new ArgumentException("Invalid entity or entity ID");

            var existingDolce = await _context.Dolce
                .FirstOrDefaultAsync(d => d.ArticoloId == entity.ArticoloId);

            if (existingDolce == null)
                throw new KeyNotFoundException($"Dolce with ID {entity.ArticoloId} not found");

            // Aggiorna le proprietà
            existingDolce.Nome = entity.Nome;
            existingDolce.Prezzo = entity.Prezzo;
            existingDolce.Descrizione = entity.Descrizione;
            existingDolce.ImmagineUrl = entity.ImmagineUrl;
            existingDolce.Disponibile = entity.Disponibile;
            existingDolce.Priorita = entity.Priorita;
            existingDolce.DataAggiornamento = DateTime.Now;

            _context.Dolce.Update(existingDolce);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var dolce = await _context.Dolce
                .FirstOrDefaultAsync(d => d.ArticoloId == id);

            if (dolce == null) return false;

            _context.Dolce.Remove(dolce);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Dolce
                .AnyAsync(d => d.ArticoloId == id);
        }

        public async Task<IEnumerable<DolceDTO>> GetDisponibiliAsync()
        {
            return await _context.Dolce
                .AsNoTracking()
                .Where(d => d.Disponibile)
                .OrderBy(d => d.Priorita)
                .ThenBy(d => d.Nome)
                .Select(d => new DolceDTO
                {
                    ArticoloId = d.ArticoloId,
                    Nome = d.Nome,
                    Prezzo = d.Prezzo,
                    Descrizione = d.Descrizione,
                    ImmagineUrl = d.ImmagineUrl,
                    Disponibile = d.Disponibile,
                    Priorita = d.Priorita,
                    DataCreazione = d.DataCreazione,
                    DataAggiornamento = d.DataAggiornamento
                })
                .ToListAsync();
        }

        public async Task<IEnumerable<DolceDTO>> GetByPrioritaAsync(int priorita)
        {
            return await _context.Dolce
                .AsNoTracking()
                .Where(d => d.Priorita == priorita && d.Disponibile)
                .OrderBy(d => d.Nome)
                .Select(d => new DolceDTO
                {
                    ArticoloId = d.ArticoloId,
                    Nome = d.Nome,
                    Prezzo = d.Prezzo,
                    Descrizione = d.Descrizione,
                    ImmagineUrl = d.ImmagineUrl,
                    Disponibile = d.Disponibile,
                    Priorita = d.Priorita,
                    DataCreazione = d.DataCreazione,
                    DataAggiornamento = d.DataAggiornamento
                })
                .ToListAsync();
        }

        public async Task<bool> ToggleDisponibilitaAsync(int id, bool disponibile)
        {
            var dolce = await _context.Dolce
                .FirstOrDefaultAsync(d => d.ArticoloId == id);

            if (dolce == null) return false;

            dolce.Disponibile = disponibile;
            dolce.DataAggiornamento = DateTime.Now;

            await _context.SaveChangesAsync();
            return true;
        }
    }
}