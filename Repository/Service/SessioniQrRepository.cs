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
    public class SessioniQrRepository
    {
        private readonly BubbleTeaContext _context;
        public SessioniQrRepository(BubbleTeaContext context)
        {
            _context = context;
        }
        public async Task<IEnumerable<SessioniQrDTO>> GetAllAsync()
        {
            return await _context.SessioniQr
                .Select(s => new SessioniQrDTO
                {
                    SessioneId = s.SessioneId,
                    ClienteId = s.ClienteId,
                    QrCode = s.QrCode,
                    DataCreazione = s.DataCreazione,
                    DataScadenza = s.DataScadenza,
                    Utilizzato = s.Utilizzato,
                    DataUtilizzo = s.DataUtilizzo
                })
                .ToListAsync();
        }
        public async Task<SessioniQrDTO> GetByIdAsync(Guid sessioneId)
        {
            var sessioneQr = await _context.SessioniQr.FindAsync(sessioneId);
            if (sessioneQr == null) return null;
            return new SessioniQrDTO
            {
                SessioneId = sessioneQr.SessioneId,
                ClienteId = sessioneQr.ClienteId,
                QrCode = sessioneQr.QrCode,
                DataCreazione = sessioneQr.DataCreazione,
                DataScadenza = sessioneQr.DataScadenza,
                Utilizzato = sessioneQr.Utilizzato,
                DataUtilizzo = sessioneQr.DataUtilizzo
            };
        }
        public async Task<SessioniQrDTO> GetByQrCodeAsync(string qrCode)
        {
            var sessioneQr = await _context.SessioniQr
                .FirstOrDefaultAsync(s => s.QrCode == qrCode);
            if (sessioneQr == null) return null;
            return new SessioniQrDTO
            {
                SessioneId = sessioneQr.SessioneId,
                ClienteId = sessioneQr.ClienteId,
                QrCode = sessioneQr.QrCode,
                DataCreazione = sessioneQr.DataCreazione,
                DataScadenza = sessioneQr.DataScadenza,
                Utilizzato = sessioneQr.Utilizzato,
                DataUtilizzo = sessioneQr.DataUtilizzo
            };
        }
        public async Task<IEnumerable<SessioniQrDTO>> GetByClienteIdAsync(int clienteId)
        {
            return await _context.SessioniQr
                .Where(s => s.ClienteId == clienteId)
                .Select(s => new SessioniQrDTO
                {
                    SessioneId = s.SessioneId,
                    ClienteId = s.ClienteId,
                    QrCode = s.QrCode,
                    DataCreazione = s.DataCreazione,
                    DataScadenza = s.DataScadenza,
                    Utilizzato = s.Utilizzato,
                    DataUtilizzo = s.DataUtilizzo
                })
                .ToListAsync();
        }
        public async Task<IEnumerable<SessioniQrDTO>> GetNonutilizzateAsync()
        {
            return await _context.SessioniQr
                .Where(s => s.Utilizzato == false || s.Utilizzato == null)
                .Select(s => new SessioniQrDTO
                {
                    SessioneId = s.SessioneId,
                    ClienteId = s.ClienteId,
                    QrCode = s.QrCode,
                    DataCreazione = s.DataCreazione,
                    DataScadenza = s.DataScadenza,
                    Utilizzato = s.Utilizzato,
                    DataUtilizzo = s.DataUtilizzo
                })
                .ToListAsync();
        }
        public async Task<IEnumerable<SessioniQrDTO>> GetScaduteAsync()
        {
            return await _context.SessioniQr
                .Where(s => s.DataScadenza <= DateTime.Now)
                .Select(s => new SessioniQrDTO
                {
                    SessioneId = s.SessioneId,
                    ClienteId = s.ClienteId,
                    QrCode = s.QrCode,
                    DataCreazione = s.DataCreazione,
                    DataScadenza = s.DataScadenza,
                    Utilizzato = s.Utilizzato,
                    DataUtilizzo = s.DataUtilizzo
                })
                .ToListAsync();
        }
        public async Task AddAsync(SessioniQrDTO sessioneQrDto)
        {
            var sessione = new SessioniQr
            {
                SessioneId = Guid.NewGuid(), // Or use the one from DTO if provided
                QrCode = sessioneQrDto.QrCode,
                ClienteId = sessioneQrDto.ClienteId,
                DataCreazione = DateTime.Now, // Or use from DTO
                DataScadenza = sessioneQrDto.DataScadenza,
                Utilizzato = sessioneQrDto.Utilizzato,
                DataUtilizzo = sessioneQrDto.DataUtilizzo
                // Map other properties as needed
            };

            await _context.SessioniQr.AddAsync(sessione);
            await _context.SaveChangesAsync();

            // Return the generated data to DTO
            sessioneQrDto.SessioneId = sessione.SessioneId;
            sessioneQrDto.DataCreazione = sessione.DataCreazione;
        }

        public async Task UpdateAsync(SessioniQrDTO sessioneQrDto)
        {
            var sessione = await _context.SessioniQr.FindAsync(sessioneQrDto.SessioneId);
            if (sessione == null)
                throw new ArgumentException("Sessione QR not found");

            sessione.QrCode = sessioneQrDto.QrCode;
            sessione.ClienteId = sessioneQrDto.ClienteId;
            sessione.DataScadenza = sessioneQrDto.DataScadenza;
            sessione.Utilizzato = sessioneQrDto.Utilizzato;
            sessione.DataUtilizzo = sessioneQrDto.DataUtilizzo;
            // Update other properties as needed

            _context.SessioniQr.Update(sessione);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid sessioneId)
        {
            var sessione = await _context.SessioniQr.FindAsync(sessioneId);
            if (sessione != null)
            {
                _context.SessioniQr.Remove(sessione);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> ExistsAsync(Guid sessioneId)
        {
            return await _context.SessioniQr.AnyAsync(s => s.SessioneId == sessioneId);
        }
    }
}
