using BBltZen;
using DTO;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Repository.Interface;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Repository.Service
{
    public class ScontrinoRepository : IScontrinoRepository
    {
        private readonly BubbleTeaContext _context;
        private readonly ILogger<ScontrinoRepository> _logger;

        public ScontrinoRepository(BubbleTeaContext context, ILogger<ScontrinoRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<ScontrinoDTO> GeneraScontrinoCompletoAsync(int ordineId)
        {
            try
            {
                _logger.LogInformation("Generazione scontrino per ordine {OrdineId}", ordineId);

                var scontrino = new ScontrinoDTO { OrdineId = ordineId, DataGenerazione = DateTime.UtcNow };

                // Chiamata alla stored procedure
                var connection = _context.Database.GetDbConnection();
                await connection.OpenAsync();

                using var command = connection.CreateCommand();
                command.CommandText = "GeneraScontrinoCompleto";
                command.CommandType = System.Data.CommandType.StoredProcedure;
                command.Parameters.Add(new SqlParameter("@OrdineID", ordineId));

                using var reader = await command.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    var riga = reader.GetString(0);
                    scontrino.RigheScontrino.Add(riga);
                }

                // ✅ CORREZIONE: Usa i nomi corretti delle proprietà
                var ordine = await _context.Ordine
                    .Include(o => o.StatoOrdine)
                    .Include(o => o.StatoPagamento)
                    .FirstOrDefaultAsync(o => o.OrdineId == ordineId);

                if (ordine != null)
                {
                    scontrino.TotaleOrdine = ordine.Totale;
                    scontrino.StatoOrdine = ordine.StatoOrdine?.StatoOrdine1 ?? "N/D"; // ✅ StatoOrdine1
                    scontrino.StatoPagamento = ordine.StatoPagamento?.StatoPagamento1 ?? "N/D"; // ✅ StatoPagamento1
                }

                _logger.LogInformation("Scontrino generato con {NumRighe} righe per ordine {OrdineId}",
                    scontrino.RigheScontrino.Count, ordineId);

                return scontrino;
            }
            catch (SqlException sqlEx)
            {
                _logger.LogError(sqlEx, "Errore SQL durante generazione scontrino per ordine {OrdineId}", ordineId);
                throw new ArgumentException($"Errore durante la generazione dello scontrino: {sqlEx.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore generico durante generazione scontrino per ordine {OrdineId}", ordineId);
                throw;
            }
        }

        public async Task<bool> EsisteOrdineAsync(int ordineId)
        {
            return await _context.Ordine.AnyAsync(o => o.OrdineId == ordineId);
        }

        public async Task<bool> VerificaStatoOrdinePerScontrinoAsync(int ordineId)
        {
            var ordine = await _context.Ordine
                .Include(o => o.StatoOrdine)
                .Include(o => o.StatoPagamento)
                .FirstOrDefaultAsync(o => o.OrdineId == ordineId);

            if (ordine == null) return false;

            // ✅ CORREZIONE: Usa i nomi corretti delle proprietà
            var statiValidi = new[] { "consegnato", "pronto consegna", "in preparazione", "in coda" };
            var pagamentoValido = ordine.StatoPagamento?.StatoPagamento1 == "completato"; // ✅ StatoPagamento1

            return statiValidi.Contains(ordine.StatoOrdine?.StatoOrdine1?.ToLower()) && pagamentoValido; // ✅ StatoOrdine1
        }
    }
}