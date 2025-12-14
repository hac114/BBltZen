using DTO;
using Microsoft.EntityFrameworkCore;
using Repository.Interface;
using QRCoder;
using System.Drawing.Imaging;
using System.Drawing;
using BBltZen;

namespace Repository.Service
{
    public class SessioniQrRepository : ISessioniQrRepository
    {
        private readonly BubbleTeaContext _context;

        public SessioniQrRepository(BubbleTeaContext context)
        {
            _context = context;
        }

        private static SessioniQrDTO MapToDTO(SessioniQr sessioneQr)
        {
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

        public async Task<IEnumerable<SessioniQrDTO>> GetAllAsync()
        {
            return await _context.SessioniQr
                .Select(s => MapToDTO(s)) // ✅ USA MapToDTO
                .ToListAsync();
        }

        public async Task<SessioniQrDTO?> GetByIdAsync(Guid sessioneId)
        {
            var sessioneQr = await _context.SessioniQr.FindAsync(sessioneId);
            return sessioneQr == null ? null : MapToDTO(sessioneQr);
        }

        public async Task<SessioniQrDTO?> GetByQrCodeAsync(string qrCode)
        {
            var sessioneQr = await _context.SessioniQr
                .FirstOrDefaultAsync(s => s.QrCode == qrCode);
            return sessioneQr == null ? null : MapToDTO(sessioneQr);
        }

        public async Task<SessioniQrDTO?> GetByCodiceSessioneAsync(string codiceSessione)
        {
            var sessioneQr = await _context.SessioniQr
                .FirstOrDefaultAsync(s => s.CodiceSessione == codiceSessione);
            return sessioneQr == null ? null : MapToDTO(sessioneQr);
        }

        public async Task<IEnumerable<SessioniQrDTO>> GetByClienteIdAsync(int clienteId)
        {
            return await _context.SessioniQr
                .Where(s => s.ClienteId == clienteId)
                .Select(s => MapToDTO(s)) // ✅ USA MapToDTO
                .ToListAsync();
        }

        public async Task<IEnumerable<SessioniQrDTO>> GetByTavoloIdAsync(int tavoloId)
        {
            return await _context.SessioniQr
                .Where(s => s.TavoloId == tavoloId)
                .Select(s => MapToDTO(s)) // ✅ USA MapToDTO
                .ToListAsync();
        }

        public async Task<IEnumerable<SessioniQrDTO>> GetNonutilizzateAsync()
        {
            return await _context.SessioniQr
                .Where(s => s.Utilizzato == false || s.Utilizzato == null)
                .Select(s => MapToDTO(s)) // ✅ USA MapToDTO
                .ToListAsync();
        }

        public async Task<IEnumerable<SessioniQrDTO>> GetScaduteAsync()
        {
            return await _context.SessioniQr
                .Where(s => s.DataScadenza <= DateTime.Now)
                .Select(s => MapToDTO(s)) // ✅ USA MapToDTO
                .ToListAsync();
        }

        public async Task<SessioniQrDTO> AddAsync(SessioniQrDTO sessioneQrDto)
        {
            if (sessioneQrDto == null)
                throw new ArgumentNullException(nameof(sessioneQrDto));

            var sessione = new SessioniQr
            {
                SessioneId = Guid.NewGuid(),
                TavoloId = sessioneQrDto.TavoloId,
                ClienteId = sessioneQrDto.ClienteId,
                CodiceSessione = sessioneQrDto.CodiceSessione,
                Stato = sessioneQrDto.Stato ?? "Attiva",
                QrCode = sessioneQrDto.QrCode,
                DataCreazione = DateTime.Now,
                DataScadenza = sessioneQrDto.DataScadenza,
                Utilizzato = sessioneQrDto.Utilizzato ?? false,
                DataUtilizzo = sessioneQrDto.DataUtilizzo
            };

            await _context.SessioniQr.AddAsync(sessione);
            await _context.SaveChangesAsync();

            // ✅ Aggiorna DTO con valori dal database
            sessioneQrDto.SessioneId = sessione.SessioneId;
            sessioneQrDto.DataCreazione = sessione.DataCreazione;
            sessioneQrDto.Stato = sessione.Stato;
            sessioneQrDto.Utilizzato = sessione.Utilizzato;

            return sessioneQrDto;
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

        [System.Runtime.Versioning.SupportedOSPlatform("windows")]
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

        [System.Runtime.Versioning.SupportedOSPlatform("windows")]
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