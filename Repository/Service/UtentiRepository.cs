using Database;
using DTO;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Service
{
    public class UtentiRepository
    {
        private readonly BubbleTeaContext _context;
        public UtentiRepository(BubbleTeaContext context)
        {
            _context = context;
        }
        public async Task<IEnumerable<UtentiDTO>> GetAllAsync()
        {
            return await _context.Utenti
                .Select(u => new UtentiDTO
                {
                    UtenteId = u.UtenteId,
                    ClienteId = u.ClienteId,
                    Email = u.Email,
                    PasswordHash = u.PasswordHash,
                    TipoUtente = u.TipoUtente,
                    DataCreazione = u.DataCreazione,
                    DataAggiornamento = u.DataAggiornamento,
                    UltimoAccesso = u.UltimoAccesso,
                    Attivo = u.Attivo
                })
                .ToListAsync();
        }

        public async Task<UtentiDTO> GetByIdAsync(int utenteId)
        {
            var utente= await _context.Utenti
                .FindAsync(utenteId);
            if(utente == null) return null;
            return new UtentiDTO
            {
                UtenteId = utenteId,
                ClienteId = utente.ClienteId,
                Email = utente.Email,
                PasswordHash = utente.PasswordHash,
                TipoUtente = utente.TipoUtente,
                DataCreazione = utente.DataCreazione,
                DataAggiornamento = utente.DataAggiornamento,
                UltimoAccesso = utente.UltimoAccesso,
                Attivo = utente.Attivo
            };
        }

        public async Task<UtentiDTO> GetByEmailAsync(string email)
        {
            var utente = await _context.Utenti
                .FirstOrDefaultAsync(u => u.Email == email);
            if (utente == null) return null;
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
                Attivo = utente.Attivo
            };
        }

        public async Task<IEnumerable<UtentiDTO>> GetByTipoUtenteAsync(string tipoUtente)
        {
            return await _context.Utenti
                .Where(u => u.TipoUtente == tipoUtente)
                .Select(u => new UtentiDTO
                {
                    UtenteId = u.UtenteId,
                    ClienteId = u.ClienteId,
                    Email = u.Email,
                    PasswordHash = u.PasswordHash,
                    TipoUtente = u.TipoUtente,
                    DataCreazione = u.DataCreazione,
                    DataAggiornamento = u.DataAggiornamento,
                    UltimoAccesso = u.UltimoAccesso,
                    Attivo = u.Attivo
                })
                .ToListAsync();
        }

        public async Task<IEnumerable<UtentiDTO>> GetAttiviAsync()
        {
            return await _context.Utenti
                .Where(u => u.Attivo == true)
                .Select(u => new UtentiDTO
                {
                    UtenteId = u.UtenteId,
                    ClienteId = u.ClienteId,
                    Email = u.Email,
                    PasswordHash = u.PasswordHash,
                    TipoUtente = u.TipoUtente,
                    DataCreazione = u.DataCreazione,
                    DataAggiornamento = u.DataAggiornamento,
                    UltimoAccesso = u.UltimoAccesso,
                    Attivo = u.Attivo
                })
                .ToListAsync();
        }

        public async Task AddAsync(UtentiDTO utenti)
        {
            var utente = new Utenti
            {
                ClienteId = utenti.ClienteId,
                Email = utenti.Email,
                PasswordHash = utenti.PasswordHash,
                TipoUtente = utenti.TipoUtente,
                DataCreazione = utenti.DataCreazione,
                DataAggiornamento = utenti.DataAggiornamento,
                UltimoAccesso = utenti.UltimoAccesso,
                Attivo = utenti.Attivo
            };
            await _context.Utenti.AddAsync(utente);
            await _context.SaveChangesAsync();
            utenti.UtenteId = utente.UtenteId;
        }

        public async Task UpdateAsync(UtentiDTO utente)
        {
            var utenti = await _context.Utenti.FindAsync(utente.UtenteId);
            if (utenti == null)
                throw new ArgumentException("Invalid entity or entity ID.");
            utenti.ClienteId = utente.ClienteId;
            utenti.Email = utente.Email;
            utenti.PasswordHash = utente.PasswordHash;
            utenti.TipoUtente = utente.TipoUtente;
            utenti.DataCreazione = utente.DataCreazione;
            utenti.DataAggiornamento = utente.DataAggiornamento;
            utenti.UltimoAccesso = utente.UltimoAccesso;
            utenti.Attivo = utente.Attivo;

            _context.Utenti.Update(utenti);
            await _context.SaveChangesAsync();
        }
        private async Task<Utenti> GetEntityByIdAsync(int utenteId)
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
