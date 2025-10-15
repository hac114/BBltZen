using Database;
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

        public async Task<List<VwArticoliCompletiDTO>> GetAllAsync()
        {
            try
            {
                var articoli = await _context.VwArticoliCompleti
                    .Select(a => new VwArticoliCompletiDTO
                    {
                        ArticoloId = a.ArticoloId,
                        TipoArticolo = a.TipoArticolo,
                        DataCreazione = a.DataCreazione,
                        DataAggiornamento = a.DataAggiornamento,
                        NomeArticolo = a.NomeArticolo,
                        PrezzoBase = a.PrezzoBase,
                        AliquotaIva = a.AliquotaIva,
                        Disponibile = a.Disponibile,
                        Categoria = a.Categoria
                    })
                    .ToListAsync();

                _logger.LogInformation($"Recuperati {articoli.Count} articoli completi");
                return articoli;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore nel recupero di tutti gli articoli completi");
                return new List<VwArticoliCompletiDTO>();
            }
        }

        public async Task<VwArticoliCompletiDTO?> GetByIdAsync(int articoloId)
        {
            try
            {
                var articolo = await _context.VwArticoliCompleti
                    .Where(a => a.ArticoloId == articoloId)
                    .Select(a => new VwArticoliCompletiDTO
                    {
                        ArticoloId = a.ArticoloId,
                        TipoArticolo = a.TipoArticolo,
                        DataCreazione = a.DataCreazione,
                        DataAggiornamento = a.DataAggiornamento,
                        NomeArticolo = a.NomeArticolo,
                        PrezzoBase = a.PrezzoBase,
                        AliquotaIva = a.AliquotaIva,
                        Disponibile = a.Disponibile,
                        Categoria = a.Categoria
                    })
                    .FirstOrDefaultAsync();

                if (articolo != null)
                    _logger.LogInformation($"Recuperato articolo completo con ID: {articoloId}");
                else
                    _logger.LogWarning($"Articolo completo con ID: {articoloId} non trovato");

                return articolo;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Errore nel recupero articolo completo con ID: {articoloId}");
                return null;
            }
        }

        public async Task<List<VwArticoliCompletiDTO>> GetByTipoAsync(string tipoArticolo)
        {
            try
            {
                var articoli = await _context.VwArticoliCompleti
                    .Where(a => a.TipoArticolo == tipoArticolo)
                    .Select(a => new VwArticoliCompletiDTO
                    {
                        ArticoloId = a.ArticoloId,
                        TipoArticolo = a.TipoArticolo,
                        DataCreazione = a.DataCreazione,
                        DataAggiornamento = a.DataAggiornamento,
                        NomeArticolo = a.NomeArticolo,
                        PrezzoBase = a.PrezzoBase,
                        AliquotaIva = a.AliquotaIva,
                        Disponibile = a.Disponibile,
                        Categoria = a.Categoria
                    })
                    .ToListAsync();

                _logger.LogInformation($"Recuperati {articoli.Count} articoli completi di tipo: {tipoArticolo}");
                return articoli;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Errore nel recupero articoli completi di tipo: {tipoArticolo}");
                return new List<VwArticoliCompletiDTO>();
            }
        }

        public async Task<List<VwArticoliCompletiDTO>> GetByCategoriaAsync(string categoria)
        {
            try
            {
                var articoli = await _context.VwArticoliCompleti
                    .Where(a => a.Categoria == categoria)
                    .Select(a => new VwArticoliCompletiDTO
                    {
                        ArticoloId = a.ArticoloId,
                        TipoArticolo = a.TipoArticolo,
                        DataCreazione = a.DataCreazione,
                        DataAggiornamento = a.DataAggiornamento,
                        NomeArticolo = a.NomeArticolo,
                        PrezzoBase = a.PrezzoBase,
                        AliquotaIva = a.AliquotaIva,
                        Disponibile = a.Disponibile,
                        Categoria = a.Categoria
                    })
                    .ToListAsync();

                _logger.LogInformation($"Recuperati {articoli.Count} articoli completi della categoria: {categoria}");
                return articoli;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Errore nel recupero articoli completi della categoria: {categoria}");
                return new List<VwArticoliCompletiDTO>();
            }
        }

        public async Task<List<VwArticoliCompletiDTO>> GetDisponibiliAsync()
        {
            try
            {
                var articoli = await _context.VwArticoliCompleti
                    .Where(a => a.Disponibile == 1)
                    .Select(a => new VwArticoliCompletiDTO
                    {
                        ArticoloId = a.ArticoloId,
                        TipoArticolo = a.TipoArticolo,
                        DataCreazione = a.DataCreazione,
                        DataAggiornamento = a.DataAggiornamento,
                        NomeArticolo = a.NomeArticolo,
                        PrezzoBase = a.PrezzoBase,
                        AliquotaIva = a.AliquotaIva,
                        Disponibile = a.Disponibile,
                        Categoria = a.Categoria
                    })
                    .ToListAsync();

                _logger.LogInformation($"Recuperati {articoli.Count} articoli completi disponibili");
                return articoli;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore nel recupero articoli completi disponibili");
                return new List<VwArticoliCompletiDTO>();
            }
        }

        public async Task<List<VwArticoliCompletiDTO>> SearchByNameAsync(string nome)
        {
            try
            {
                var articoli = await _context.VwArticoliCompleti
                    .Where(a => a.NomeArticolo != null && a.NomeArticolo.Contains(nome))
                    .Select(a => new VwArticoliCompletiDTO
                    {
                        ArticoloId = a.ArticoloId,
                        TipoArticolo = a.TipoArticolo,
                        DataCreazione = a.DataCreazione,
                        DataAggiornamento = a.DataAggiornamento,
                        NomeArticolo = a.NomeArticolo,
                        PrezzoBase = a.PrezzoBase,
                        AliquotaIva = a.AliquotaIva,
                        Disponibile = a.Disponibile,
                        Categoria = a.Categoria
                    })
                    .ToListAsync();

                _logger.LogInformation($"Trovati {articoli.Count} articoli completi con nome contenente: {nome}");
                return articoli;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Errore nella ricerca articoli completi per nome: {nome}");
                return new List<VwArticoliCompletiDTO>();
            }
        }

        public async Task<List<VwArticoliCompletiDTO>> GetByPriceRangeAsync(decimal prezzoMin, decimal prezzoMax)
        {
            try
            {
                var articoli = await _context.VwArticoliCompleti
                    .Where(a => a.PrezzoBase >= prezzoMin && a.PrezzoBase <= prezzoMax)
                    .Select(a => new VwArticoliCompletiDTO
                    {
                        ArticoloId = a.ArticoloId,
                        TipoArticolo = a.TipoArticolo,
                        DataCreazione = a.DataCreazione,
                        DataAggiornamento = a.DataAggiornamento,
                        NomeArticolo = a.NomeArticolo,
                        PrezzoBase = a.PrezzoBase,
                        AliquotaIva = a.AliquotaIva,
                        Disponibile = a.Disponibile,
                        Categoria = a.Categoria
                    })
                    .ToListAsync();

                _logger.LogInformation($"Recuperati {articoli.Count} articoli completi nel range prezzi {prezzoMin}-{prezzoMax}");
                return articoli;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Errore nel recupero articoli completi nel range prezzi {prezzoMin}-{prezzoMax}");
                return new List<VwArticoliCompletiDTO>();
            }
        }

        public async Task<List<VwArticoliCompletiDTO>> GetArticoliConIvaAsync()
        {
            try
            {
                var articoli = await _context.VwArticoliCompleti
                    .Where(a => a.AliquotaIva > 0)
                    .Select(a => new VwArticoliCompletiDTO
                    {
                        ArticoloId = a.ArticoloId,
                        TipoArticolo = a.TipoArticolo,
                        DataCreazione = a.DataCreazione,
                        DataAggiornamento = a.DataAggiornamento,
                        NomeArticolo = a.NomeArticolo,
                        PrezzoBase = a.PrezzoBase,
                        AliquotaIva = a.AliquotaIva,
                        Disponibile = a.Disponibile,
                        Categoria = a.Categoria
                    })
                    .ToListAsync();

                _logger.LogInformation($"Recuperati {articoli.Count} articoli completi con IVA");
                return articoli;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore nel recupero articoli completi con IVA");
                return new List<VwArticoliCompletiDTO>();
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

        public async Task<List<string>> GetCategorieAsync()
        {
            try
            {
                var categorie = await _context.VwArticoliCompleti
                    .Select(a => a.Categoria)
                    .Distinct()
                    .ToListAsync();

                _logger.LogInformation($"Recuperate {categorie.Count} categorie distinte");
                return categorie;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore nel recupero categorie");
                return new List<string>();
            }
        }

        public async Task<List<string>> GetTipiArticoloAsync()
        {
            try
            {
                var tipi = await _context.VwArticoliCompleti
                    .Select(a => a.TipoArticolo)
                    .Distinct()
                    .ToListAsync();

                _logger.LogInformation($"Recuperati {tipi.Count} tipi articolo distinti");
                return tipi;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore nel recupero tipi articolo");
                return new List<string>();
            }
        }
    }
}