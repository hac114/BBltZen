using BBltZen;
using DTO;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Repository.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Repository.Service
{
    public class VwArticoliCompletiRepository : IVwArticoliCompletiRepository
    {
        private readonly BubbleTeaContext _context;
        private readonly ILogger<VwArticoliCompletiRepository> _logger;

        public VwArticoliCompletiRepository(BubbleTeaContext context, ILogger<VwArticoliCompletiRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        private VwArticoliCompletiDTO MapToDTO(VwArticoliCompleti entity)
        {
            return new VwArticoliCompletiDTO
            {
                ArticoloId = entity.ArticoloId,
                TipoArticolo = entity.TipoArticolo,
                DataCreazione = entity.DataCreazione,
                DataAggiornamento = entity.DataAggiornamento,
                NomeArticolo = entity.NomeArticolo,
                PrezzoBase = entity.PrezzoBase,
                AliquotaIva = entity.AliquotaIva,
                Disponibile = entity.Disponibile,
                Categoria = entity.Categoria
            };
        }

        public async Task<IEnumerable<VwArticoliCompletiDTO>> GetByTipoAsync(string tipoArticolo)
        {
            try
            {
                var articoli = await _context.VwArticoliCompleti
                    .AsNoTracking()
                    .Where(a => a.TipoArticolo == tipoArticolo)
                    .Select(a => MapToDTO(a))
                    .ToListAsync();

                _logger.LogInformation("Recuperati {Count} articoli completi di tipo: {Tipo}", articoli.Count, tipoArticolo);
                return articoli; // ✅ List<T> è assegnabile a IEnumerable<T>
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore nel recupero articoli completi di tipo: {Tipo}", tipoArticolo);
                return Enumerable.Empty<VwArticoliCompletiDTO>();
            }
        }

        public async Task<IEnumerable<VwArticoliCompletiDTO>> GetAllAsync()
        {
            try
            {
                var articoli = await _context.VwArticoliCompleti
                    .AsNoTracking() // ✅ PERFORMANCE - SOLO LETTURA
                    .Select(a => MapToDTO(a))
                    .ToListAsync();

                _logger.LogInformation("Recuperati {Count} articoli completi", articoli.Count);
                return articoli;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore nel recupero di tutti gli articoli completi");
                return Enumerable.Empty<VwArticoliCompletiDTO>();
            }
        }

        // ✅ AGGIUNGI METODO EXISTS
        public async Task<bool> ExistsAsync(int articoloId)
        {
            try
            {
                var exists = await _context.VwArticoliCompleti
                    .AsNoTracking()
                    .AnyAsync(a => a.ArticoloId == articoloId);

                return exists;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore nella verifica esistenza articolo {ArticoloId}", articoloId);
                return false;
            }
        }

        public async Task<VwArticoliCompletiDTO?> GetByIdAsync(int articoloId)
        {
            try
            {
                var articolo = await _context.VwArticoliCompleti
                    .AsNoTracking() // ✅ AGGIUNTO PER PERFORMANCE
                    .Where(a => a.ArticoloId == articoloId)
                    .Select(a => MapToDTO(a)) // ✅ SEMPLIFICATO
                    .FirstOrDefaultAsync();

                if (articolo != null)
                    _logger.LogInformation("Recuperato articolo completo con ID: {ArticoloId}", articoloId);
                else
                    _logger.LogWarning("Articolo completo con ID: {ArticoloId} non trovato", articoloId);

                return articolo;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore nel recupero articolo completo con ID: {ArticoloId}", articoloId);
                return null;
            }
        }

        public async Task<IEnumerable<VwArticoliCompletiDTO>> GetByCategoriaAsync(string categoria)
        {
            try
            {
                var articoli = await _context.VwArticoliCompleti
                    .AsNoTracking()
                    .Where(a => a.Categoria == categoria)
                    .Select(a => MapToDTO(a))
                    .ToListAsync();

                _logger.LogInformation("Recuperati {Count} articoli completi della categoria: {Categoria}", articoli.Count, categoria);
                return articoli;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore nel recupero articoli completi della categoria: {Categoria}", categoria);
                return Enumerable.Empty<VwArticoliCompletiDTO>();
            }
        }

        public async Task<IEnumerable<VwArticoliCompletiDTO>> GetDisponibiliAsync()
        {
            try
            {
                var articoli = await _context.VwArticoliCompleti
                    .AsNoTracking()
                    .Where(a => a.Disponibile == 1)
                    .Select(a => MapToDTO(a))
                    .ToListAsync();

                _logger.LogInformation("Recuperati {Count} articoli completi disponibili", articoli.Count);
                return articoli;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore nel recupero articoli completi disponibili");
                return Enumerable.Empty<VwArticoliCompletiDTO>();
            }
        }

        public async Task<IEnumerable<VwArticoliCompletiDTO>> SearchByNameAsync(string nome)
        {
            try
            {
                var articoli = await _context.VwArticoliCompleti
                    .AsNoTracking()
                    .Where(a => a.NomeArticolo != null && a.NomeArticolo.Contains(nome))
                    .Select(a => MapToDTO(a))
                    .ToListAsync();

                _logger.LogInformation("Trovati {Count} articoli completi con nome contenente: {Nome}", articoli.Count, nome);
                return articoli;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore nella ricerca articoli completi per nome: {Nome}", nome);
                return Enumerable.Empty<VwArticoliCompletiDTO>();
            }
        }

        public async Task<IEnumerable<VwArticoliCompletiDTO>> GetByPriceRangeAsync(decimal prezzoMin, decimal prezzoMax)
        {
            try
            {
                var articoli = await _context.VwArticoliCompleti
                    .AsNoTracking()
                    .Where(a => a.PrezzoBase >= prezzoMin && a.PrezzoBase <= prezzoMax)
                    .Select(a => MapToDTO(a))
                    .ToListAsync();

                _logger.LogInformation("Recuperati {Count} articoli completi nel range prezzi {Min}-{Max}", articoli.Count, prezzoMin, prezzoMax);
                return articoli;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore nel recupero articoli completi nel range prezzi {Min}-{Max}", prezzoMin, prezzoMax);
                return Enumerable.Empty<VwArticoliCompletiDTO>();
            }
        }

        public async Task<IEnumerable<VwArticoliCompletiDTO>> GetArticoliConIvaAsync()
        {
            try
            {
                var articoli = await _context.VwArticoliCompleti
                    .AsNoTracking()
                    .Where(a => a.AliquotaIva > 0)
                    .Select(a => MapToDTO(a))
                    .ToListAsync();

                _logger.LogInformation("Recuperati {Count} articoli completi con IVA", articoli.Count);
                return articoli;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore nel recupero articoli completi con IVA");
                return Enumerable.Empty<VwArticoliCompletiDTO>();
            }
        }

        public async Task<int> GetCountAsync()
        {
            try
            {
                var count = await _context.VwArticoliCompleti.CountAsync();
                _logger.LogInformation($"Totale articoli completi: {count}");
                return count;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore nel conteggio articoli completi");
                return 0;
            }
        }

        // ✅ METODI AGGREGATI CON IEnumerable
        public async Task<IEnumerable<string>> GetCategorieAsync()
        {
            try
            {
                var categorie = await _context.VwArticoliCompleti
                    .AsNoTracking()
                    .Select(a => a.Categoria)
                    .Distinct()
                    .ToListAsync();

                _logger.LogInformation("Recuperate {Count} categorie distinte", categorie.Count);
                return categorie;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore nel recupero categorie");
                return Enumerable.Empty<string>();
            }
        }

        public async Task<IEnumerable<string>> GetTipiArticoloAsync()
        {
            try
            {
                var tipi = await _context.VwArticoliCompleti
                    .AsNoTracking()
                    .Select(a => a.TipoArticolo)
                    .Distinct()
                    .ToListAsync();

                _logger.LogInformation("Recuperati {Count} tipi articolo distinti", tipi.Count);
                return tipi;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore nel recupero tipi articolo");
                return Enumerable.Empty<string>();
            }
        }        
    }
}