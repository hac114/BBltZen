using Database;
using DTO;
using Microsoft.EntityFrameworkCore;
using Repository.Interface;
using QRCoder;
using System.Drawing.Imaging;
using System.Drawing;

namespace Repository.Service
{
    public class SessioniQrRepository : ISessioniQrRepository
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
                    TavoloId = s.TavoloId,
                    ClienteId = s.ClienteId,
                    CodiceSessione = s.CodiceSessione,
                    Stato = s.Stato,
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
                TavoloId = sessioneQr.TavoloId,
                ClienteId = sessioneQr.ClienteId,
                CodiceSessione = sessioneQr.CodiceSessione,
                Stato = sessioneQr.Stato,
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
                TavoloId = sessioneQr.TavoloId,
                ClienteId = sessioneQr.ClienteId,
                CodiceSessione = sessioneQr.CodiceSessione,
                Stato = sessioneQr.Stato,
                QrCode = sessioneQr.QrCode,
                DataCreazione = sessioneQr.DataCreazione,
                DataScadenza = sessioneQr.DataScadenza,
                Utilizzato = sessioneQr.Utilizzato,
                DataUtilizzo = sessioneQr.DataUtilizzo
            };
        }

        public async Task<SessioniQrDTO> GetByCodiceSessioneAsync(string codiceSessione)
        {
            var sessioneQr = await _context.SessioniQr
                .FirstOrDefaultAsync(s => s.CodiceSessione == codiceSessione);
            if (sessioneQr == null) return null;

            return new SessioniQrDTO
            {
                SessioneId = sessioneQr.SessioneId,
                TavoloId = sessioneQr.TavoloId,
                ClienteId = sessioneQr.ClienteId,
                CodiceSessione = sessioneQr.CodiceSessione,
                Stato = sessioneQr.Stato,
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
                    TavoloId = s.TavoloId,
                    ClienteId = s.ClienteId,
                    CodiceSessione = s.CodiceSessione,
                    Stato = s.Stato,
                    QrCode = s.QrCode,
                    DataCreazione = s.DataCreazione,
                    DataScadenza = s.DataScadenza,
                    Utilizzato = s.Utilizzato,
                    DataUtilizzo = s.DataUtilizzo
                })
                .ToListAsync();
        }

        public async Task<IEnumerable<SessioniQrDTO>> GetByTavoloIdAsync(int tavoloId)
        {
            return await _context.SessioniQr
                .Where(s => s.TavoloId == tavoloId)
                .Select(s => new SessioniQrDTO
                {
                    SessioneId = s.SessioneId,
                    TavoloId = s.TavoloId,
                    ClienteId = s.ClienteId,
                    CodiceSessione = s.CodiceSessione,
                    Stato = s.Stato,
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
                    TavoloId = s.TavoloId,
                    ClienteId = s.ClienteId,
                    CodiceSessione = s.CodiceSessione,
                    Stato = s.Stato,
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
                    TavoloId = s.TavoloId,
                    ClienteId = s.ClienteId,
                    CodiceSessione = s.CodiceSessione,
                    Stato = s.Stato,
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
                SessioneId = Guid.NewGuid(),
                TavoloId = sessioneQrDto.TavoloId,
                ClienteId = sessioneQrDto.ClienteId,
                CodiceSessione = sessioneQrDto.CodiceSessione,
                Stato = sessioneQrDto.Stato,
                QrCode = sessioneQrDto.QrCode,
                DataCreazione = DateTime.Now,
                DataScadenza = sessioneQrDto.DataScadenza,
                Utilizzato = sessioneQrDto.Utilizzato,
                DataUtilizzo = sessioneQrDto.DataUtilizzo
            };

            await _context.SessioniQr.AddAsync(sessione);
            await _context.SaveChangesAsync();

            sessioneQrDto.SessioneId = sessione.SessioneId;
            sessioneQrDto.DataCreazione = sessione.DataCreazione;
        }

        public async Task UpdateAsync(SessioniQrDTO sessioneQrDto)
        {
            var sessione = await _context.SessioniQr.FindAsync(sessioneQrDto.SessioneId);
            if (sessione == null)
                throw new ArgumentException("Sessione QR not found");

            sessione.TavoloId = sessioneQrDto.TavoloId;
            sessione.ClienteId = sessioneQrDto.ClienteId;
            sessione.CodiceSessione = sessioneQrDto.CodiceSessione;
            sessione.Stato = sessioneQrDto.Stato;
            sessione.QrCode = sessioneQrDto.QrCode;
            sessione.DataScadenza = sessioneQrDto.DataScadenza;
            sessione.Utilizzato = sessioneQrDto.Utilizzato;
            sessione.DataUtilizzo = sessioneQrDto.DataUtilizzo;

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

        public async Task<SessioniQrDTO> GeneraSessioneQrAsync(int tavoloId, string frontendUrl)
        {
            // Verifica che il tavolo esista
            var tavolo = await _context.Tavolo.FindAsync(tavoloId);
            if (tavolo == null)
                throw new ArgumentException($"Tavolo {tavoloId} non trovato");

            // Genera codice sessione univoco
            var codiceSessione = $"T{tavoloId}_{DateTime.UtcNow:yyyyMMdd_HHmmss}_{Guid.NewGuid().ToString("N").Substring(0, 8)}";

            // Crea URL per il frontend
            var sessionUrl = $"{frontendUrl}/session?tavolo={tavoloId}&sessione={codiceSessione}";

            // Genera QR Code
            var qrCodeBase64 = GenerateQRCode(sessionUrl);

            // Crea la sessione
            var sessioneDto = new SessioniQrDTO
            {
                TavoloId = tavoloId,
                CodiceSessione = codiceSessione,
                Stato = "Attiva",
                QrCode = qrCodeBase64,
                DataCreazione = DateTime.Now,
                DataScadenza = DateTime.Now.AddHours(2), // Scade dopo 2 ore
                Utilizzato = false
            };

            await AddAsync(sessioneDto);
            return sessioneDto;
        }

        private string GenerateQRCode(string data)
        {
            using var qrGenerator = new QRCodeGenerator();
            using var qrCodeData = qrGenerator.CreateQrCode(data, QRCodeGenerator.ECCLevel.Q);
            using var qrCode = new QRCode(qrCodeData);
            using var bitmap = qrCode.GetGraphic(20);
            using var ms = new MemoryStream();
            bitmap.Save(ms, ImageFormat.Png);
            return Convert.ToBase64String(ms.ToArray());
        }
    }
}