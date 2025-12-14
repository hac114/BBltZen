using Database;
using BBltZen;
using DTO;
using Microsoft.EntityFrameworkCore;
using Repository.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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

        public async Task<DolceDTO?> GetByIdAsync(int articoloId)
        {
            var dolce = await _context.Dolce
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.ArticoloId == articoloId);

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
                .Where(d => d.Priorita == priorita)
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

        public async Task<DolceDTO> AddAsync(DolceDTO dolceDto) // ✅ CORREGGI: ritorna DTO
        {
            if (dolceDto == null)
                throw new ArgumentNullException(nameof(dolceDto));

            // Prima crea l'Articolo
            var articolo = new Articolo
            {
                Tipo = "DOLCE",
                DataCreazione = DateTime.Now,
                DataAggiornamento = DateTime.Now
            };

            _context.Articolo.Add(articolo);
            await _context.SaveChangesAsync(); // Salva per ottenere l'ID

            // Poi crea il Dolce
            var dolce = new Dolce
            {
                ArticoloId = articolo.ArticoloId, // ✅ USA ArticoloId generato automaticamente
                Nome = dolceDto.Nome,
                Prezzo = dolceDto.Prezzo,
                Descrizione = dolceDto.Descrizione,
                ImmagineUrl = dolceDto.ImmagineUrl,
                Disponibile = dolceDto.Disponibile,
                Priorita = dolceDto.Priorita, // ✅ NOT NULL - valore dal DTO
                DataCreazione = DateTime.Now, // ✅ NOT NULL - valore default
                DataAggiornamento = DateTime.Now // ✅ NOT NULL - valore default
            };

            _context.Dolce.Add(dolce);
            await _context.SaveChangesAsync();

            // Aggiorna il DTO con i valori del database
            dolceDto.ArticoloId = dolce.ArticoloId;
            dolceDto.DataCreazione = dolce.DataCreazione;
            dolceDto.DataAggiornamento = dolce.DataAggiornamento;

            return dolceDto; // ✅ AGGIUNGI return
        }

        public async Task UpdateAsync(DolceDTO dolceDto)
        {
            if (dolceDto == null) // ✅ AGGIUNGI validazione
                throw new ArgumentNullException(nameof(dolceDto));

            var dolce = await _context.Dolce
                .FirstOrDefaultAsync(d => d.ArticoloId == dolceDto.ArticoloId);

            if (dolce == null)
                throw new ArgumentException($"Dolce con ArticoloId {dolceDto.ArticoloId} non trovato");

            dolce.Nome = dolceDto.Nome;
            dolce.Prezzo = dolceDto.Prezzo;
            dolce.Descrizione = dolceDto.Descrizione;
            dolce.ImmagineUrl = dolceDto.ImmagineUrl;
            dolce.Disponibile = dolceDto.Disponibile;
            dolce.Priorita = dolceDto.Priorita; // ✅ NOT NULL - valore dal DTO
            dolce.DataAggiornamento = DateTime.Now; // ✅ NOT NULL - aggiornamento automatico

            await _context.SaveChangesAsync();

            dolceDto.DataAggiornamento = dolce.DataAggiornamento;
        }

        public async Task DeleteAsync(int articoloId)
        {
            var dolce = await _context.Dolce
                .FirstOrDefaultAsync(d => d.ArticoloId == articoloId);

            if (dolce != null)
            {
                _context.Dolce.Remove(dolce);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> ExistsAsync(int articoloId)
        {
            return await _context.Dolce
                .AnyAsync(d => d.ArticoloId == articoloId);
        }

        public async Task<bool> ExistsByArticoloIdAsync(int articoloId)
        {
            return await ExistsAsync(articoloId);
        }
    }
}