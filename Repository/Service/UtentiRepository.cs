using BBltZen;
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
    public class UtentiRepository : IUtentiRepository
    {
        private readonly BubbleTeaContext _context;
        public UtentiRepository(BubbleTeaContext context)
        {
            _context = context;
        }

        private static UtentiDTO MapToDTO(Utenti utente)
        {
            return new UtentiDTO
            {
                UtenteId = utente.UtenteId,
                ClienteId = utente.ClienteId,
                Email = utente.Email,
                PasswordHash = utente.PasswordHash,
                TipoUtente = utente.TipoUtente,
                DataCreazione = utente.DataCreazione,
                DataAggiornamento = utente.DataAggiornamento,
                UltimoAccesso = utente.UltimoAccesso,
                Attivo = utente.Attivo,
                Nome = utente.Nome,
                Cognome = utente.Cognome,
                Telefono = utente.Telefono,
                SessioneGuest = utente.SessioneGuest
            };
        }
        public async Task<IEnumerable<UtentiDTO>> GetAllAsync()
        {
            return await _context.Utenti
                .Select(u => MapToDTO(u)) // ✅ USA MapToDTO
                .ToListAsync();
        }

        public async Task<UtentiDTO?> GetByIdAsync(int utenteId) // ✅ CAMBIATO: nullable
        {
            var utente = await _context.Utenti.FindAsync(utenteId);
            return utente == null ? null : MapToDTO(utente);
        }

        public async Task<UtentiDTO?> GetByEmailAsync(string email) // ✅ CAMBIATO: nullable
        {
            var utente = await _context.Utenti
                .FirstOrDefaultAsync(u => u.Email == email);
            return utente == null ? null : MapToDTO(utente);
        }

        public async Task<IEnumerable<UtentiDTO>> GetByTipoUtenteAsync(string tipoUtente)
        {
            return await _context.Utenti
                .Where(u => u.TipoUtente == tipoUtente)
                .Select(u => MapToDTO(u)) // ✅ USA MapToDTO
                .ToListAsync();
        }

        public async Task<IEnumerable<UtentiDTO>> GetAttiviAsync()
        {
            return await _context.Utenti
                .Where(u => u.Attivo == true)
                .Select(u => MapToDTO(u)) // ✅ USA MapToDTO
                .ToListAsync();
        }

        public async Task<UtentiDTO> AddAsync(UtentiDTO utenti) // ✅ CAMBIATO: ritorna DTO
        {
            if (utenti == null)
                throw new ArgumentNullException(nameof(utenti));

            var utente = new Utenti
            {
                ClienteId = utenti.ClienteId,
                Email = utenti.Email,
                PasswordHash = utenti.PasswordHash,
                TipoUtente = utenti.TipoUtente ?? "cliente", // ✅ DEFAULT se null
                DataCreazione = DateTime.Now, // ✅ SEMPRE DateTime.Now
                DataAggiornamento = DateTime.Now, // ✅ SEMPRE DateTime.Now
                UltimoAccesso = utenti.UltimoAccesso,
                Attivo = utenti.Attivo ?? true, // ✅ DEFAULT se null
                Nome = utenti.Nome,
                Cognome = utenti.Cognome,
                Telefono = utenti.Telefono,
                SessioneGuest = utenti.SessioneGuest
            };

            await _context.Utenti.AddAsync(utente);
            await _context.SaveChangesAsync();

            // ✅ Aggiorna DTO con valori dal database
            utenti.UtenteId = utente.UtenteId;
            utenti.DataCreazione = utente.DataCreazione;
            utenti.DataAggiornamento = utente.DataAggiornamento;
            utenti.TipoUtente = utente.TipoUtente;
            utenti.Attivo = utente.Attivo;

            return utenti; // ✅ IMPORTANTE: ritorna il DTO
        }

        public async Task UpdateAsync(UtentiDTO utente)
        {
            var utenti = await _context.Utenti.FindAsync(utente.UtenteId);
            if (utenti == null)
                throw new ArgumentException("Utente non trovato");

            utenti.ClienteId = utente.ClienteId;
            utenti.Email = utente.Email;
            utenti.PasswordHash = utente.PasswordHash;
            utenti.TipoUtente = utente.TipoUtente;
            utenti.DataAggiornamento = DateTime.Now; // ✅ SEMPRE aggiorna timestamp
            utenti.UltimoAccesso = utente.UltimoAccesso;
            utenti.Attivo = utente.Attivo;
            utenti.Nome = utente.Nome;
            utenti.Cognome = utente.Cognome;
            utenti.Telefono = utente.Telefono;
            utenti.SessioneGuest = utente.SessioneGuest;

            await _context.SaveChangesAsync();

            // ✅ Aggiorna DTO con DataAggiornamento
            utente.DataAggiornamento = utenti.DataAggiornamento;
        }
        private async Task<Utenti?> GetEntityByIdAsync(int utenteId)
        {
            return await _context.Utenti.FindAsync(utenteId);
        }
        public async Task DeleteAsync(int utenteId)
        {
            var utente = await GetEntityByIdAsync(utenteId);
            if (utente != null)
            {
                _context.Utenti.Remove(utente);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> ExistsAsync(int utenteId)
        {
            return await _context.Utenti.AnyAsync(u => u.UtenteId == utenteId);
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            return await _context.Utenti.AnyAsync(u => u.Email == email);
        }
    }
}
