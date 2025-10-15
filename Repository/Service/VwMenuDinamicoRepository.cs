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
    public class VwMenuDinamicoRepository : IVwMenuDinamicoRepository
    {
        private readonly BubbleTeaContext _context;
        private readonly ILogger<VwMenuDinamicoRepository> _logger;

        public VwMenuDinamicoRepository(BubbleTeaContext context, ILogger<VwMenuDinamicoRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<VwMenuDinamicoDTO>> GetMenuCompletoAsync()
        {
            try
            {
                var menu = await _context.VwMenuDinamico
                    .Select(m => new VwMenuDinamicoDTO
                    {
                        Tipo = m.Tipo,
                        Id = m.Id,
                        NomeBevanda = m.NomeBevanda,
                        Descrizione = m.Descrizione,
                        PrezzoNetto = m.PrezzoNetto,
                        PrezzoLordo = m.PrezzoLordo,
                        TaxRateId = m.TaxRateId,
                        IvaPercentuale = m.IvaPercentuale,
                        ImmagineUrl = m.ImmagineUrl,
                        Priorita = m.Priorita
                    })
                    .OrderByDescending(m => m.Priorita)
                    .ThenBy(m => m.NomeBevanda)
                    .ToListAsync();

                _logger.LogInformation($"Recuperato menu completo con {menu.Count} elementi");
                return menu;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore nel recupero menu completo");
                return new List<VwMenuDinamicoDTO>();
            }
        }

        public async Task<List<VwMenuDinamicoDTO>> GetPrimoPianoAsync(int numeroElementi = 6)
        {
            try
            {
                var primoPiano = await _context.VwMenuDinamico
                    .Where(m => m.Priorita >= 1) // Priorità >= 1 significa "in primo piano"
                    .OrderByDescending(m => m.Priorita)
                    .ThenBy(m => m.NomeBevanda)
                    .Take(numeroElementi)
                    .Select(m => new VwMenuDinamicoDTO
                    {
                        Tipo = m.Tipo,
                        Id = m.Id,
                        NomeBevanda = m.NomeBevanda,
                        Descrizione = m.Descrizione,
                        PrezzoNetto = m.PrezzoNetto,
                        PrezzoLordo = m.PrezzoLordo,
                        TaxRateId = m.TaxRateId,
                        IvaPercentuale = m.IvaPercentuale,
                        ImmagineUrl = m.ImmagineUrl,
                        Priorita = m.Priorita
                    })
                    .ToListAsync();

                _logger.LogInformation($"Recuperati {primoPiano.Count} elementi in primo piano");
                return primoPiano;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore nel recupero elementi primo piano");
                return new List<VwMenuDinamicoDTO>();
            }
        }

        public async Task<List<VwMenuDinamicoDTO>> GetBevandeDisponibiliAsync()
        {
            try
            {
                var bevande = await _context.VwMenuDinamico
                    .Where(m => m.Priorita >= 0) // Priorità >= 0 significa "disponibile"
                    .Select(m => new VwMenuDinamicoDTO
                    {
                        Tipo = m.Tipo,
                        Id = m.Id,
                        NomeBevanda = m.NomeBevanda,
                        Descrizione = m.Descrizione,
                        PrezzoNetto = m.PrezzoNetto,
                        PrezzoLordo = m.PrezzoLordo,
                        TaxRateId = m.TaxRateId,
                        IvaPercentuale = m.IvaPercentuale,
                        ImmagineUrl = m.ImmagineUrl,
                        Priorita = m.Priorita
                    })
                    .OrderByDescending(m => m.Priorita)
                    .ThenBy(m => m.NomeBevanda)
                    .ToListAsync();

                _logger.LogInformation($"Recuperate {bevande.Count} bevande disponibili");
                return bevande;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore nel recupero bevande disponibili");
                return new List<VwMenuDinamicoDTO>();
            }
        }

        public async Task<List<VwMenuDinamicoDTO>> GetBevandePerCategoriaAsync(string categoria)
        {
            try
            {
                var bevande = await _context.VwMenuDinamico
                    .Where(m => m.Tipo == categoria && m.Priorita >= 0)
                    .Select(m => new VwMenuDinamicoDTO
                    {
                        Tipo = m.Tipo,
                        Id = m.Id,
                        NomeBevanda = m.NomeBevanda,
                        Descrizione = m.Descrizione,
                        PrezzoNetto = m.PrezzoNetto,
                        PrezzoLordo = m.PrezzoLordo,
                        TaxRateId = m.TaxRateId,
                        IvaPercentuale = m.IvaPercentuale,
                        ImmagineUrl = m.ImmagineUrl,
                        Priorita = m.Priorita
                    })
                    .OrderByDescending(m => m.Priorita)
                    .ThenBy(m => m.NomeBevanda)
                    .ToListAsync();

                _logger.LogInformation($"Recuperate {bevande.Count} bevande per categoria: {categoria}");
                return bevande;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Errore nel recupero bevande per categoria: {categoria}");
                return new List<VwMenuDinamicoDTO>();
            }
        }

        public async Task<List<VwMenuDinamicoDTO>> GetBevandePerPrioritaAsync(int prioritaMinima, int prioritaMassima)
        {
            try
            {
                var bevande = await _context.VwMenuDinamico
                    .Where(m => m.Priorita >= prioritaMinima && m.Priorita <= prioritaMassima)
                    .Select(m => new VwMenuDinamicoDTO
                    {
                        Tipo = m.Tipo,
                        Id = m.Id,
                        NomeBevanda = m.NomeBevanda,
                        Descrizione = m.Descrizione,
                        PrezzoNetto = m.PrezzoNetto,
                        PrezzoLordo = m.PrezzoLordo,
                        TaxRateId = m.TaxRateId,
                        IvaPercentuale = m.IvaPercentuale,
                        ImmagineUrl = m.ImmagineUrl,
                        Priorita = m.Priorita
                    })
                    .OrderByDescending(m => m.Priorita)
                    .ThenBy(m => m.NomeBevanda)
                    .ToListAsync();

                _logger.LogInformation($"Recuperate {bevande.Count} bevande con priorità {prioritaMinima}-{prioritaMassima}");
                return bevande;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Errore nel recupero bevande per priorità {prioritaMinima}-{prioritaMassima}");
                return new List<VwMenuDinamicoDTO>();
            }
        }

        public async Task<List<VwMenuDinamicoDTO>> GetBevandeConScontoAsync()
        {
            try
            {
                var bevande = await _context.VwMenuDinamico
                    .Where(m => m.PrezzoLordo.HasValue && m.PrezzoLordo < m.PrezzoNetto)
                    .Select(m => new VwMenuDinamicoDTO
                    {
                        Tipo = m.Tipo,
                        Id = m.Id,
                        NomeBevanda = m.NomeBevanda,
                        Descrizione = m.Descrizione,
                        PrezzoNetto = m.PrezzoNetto,
                        PrezzoLordo = m.PrezzoLordo,
                        TaxRateId = m.TaxRateId,
                        IvaPercentuale = m.IvaPercentuale,
                        ImmagineUrl = m.ImmagineUrl,
                        Priorita = m.Priorita
                    })
                    .OrderByDescending(m => m.Priorita)
                    .ThenBy(m => m.NomeBevanda)
                    .ToListAsync();

                _logger.LogInformation($"Recuperate {bevande.Count} bevande con sconto");
                return bevande;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore nel recupero bevande con sconto");
                return new List<VwMenuDinamicoDTO>();
            }
        }

        public async Task<VwMenuDinamicoDTO?> GetBevandaByIdAsync(int id, string tipo)
        {
            try
            {
                var bevanda = await _context.VwMenuDinamico
                    .Where(m => m.Id == id && m.Tipo == tipo)
                    .Select(m => new VwMenuDinamicoDTO
                    {
                        Tipo = m.Tipo,
                        Id = m.Id,
                        NomeBevanda = m.NomeBevanda,
                        Descrizione = m.Descrizione,
                        PrezzoNetto = m.PrezzoNetto,
                        PrezzoLordo = m.PrezzoLordo,
                        TaxRateId = m.TaxRateId,
                        IvaPercentuale = m.IvaPercentuale,
                        ImmagineUrl = m.ImmagineUrl,
                        Priorita = m.Priorita
                    })
                    .FirstOrDefaultAsync();

                if (bevanda != null)
                    _logger.LogInformation($"Recuperata bevanda: {bevanda.NomeBevanda} (ID: {id}, Tipo: {tipo})");
                else
                    _logger.LogWarning($"Bevanda non trovata (ID: {id}, Tipo: {tipo})");

                return bevanda;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Errore nel recupero bevanda ID: {id}, Tipo: {tipo}");
                return null;
            }
        }

        public async Task<List<string>> GetCategorieDisponibiliAsync()
        {
            try
            {
                var categorie = await _context.VwMenuDinamico
                    .Select(m => m.Tipo)
                    .Distinct()
                    .ToListAsync();

                _logger.LogInformation($"Recuperate {categorie.Count} categorie disponibili");
                return categorie;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore nel recupero categorie disponibili");
                return new List<string>();
            }
        }

        public async Task<List<VwMenuDinamicoDTO>> SearchBevandeAsync(string searchTerm)
        {
            try
            {
                var bevande = await _context.VwMenuDinamico
                    .Where(m => m.NomeBevanda.Contains(searchTerm) ||
                               (m.Descrizione != null && m.Descrizione.Contains(searchTerm)))
                    .Select(m => new VwMenuDinamicoDTO
                    {
                        Tipo = m.Tipo,
                        Id = m.Id,
                        NomeBevanda = m.NomeBevanda,
                        Descrizione = m.Descrizione,
                        PrezzoNetto = m.PrezzoNetto,
                        PrezzoLordo = m.PrezzoLordo,
                        TaxRateId = m.TaxRateId,
                        IvaPercentuale = m.IvaPercentuale,
                        ImmagineUrl = m.ImmagineUrl,
                        Priorita = m.Priorita
                    })
                    .OrderByDescending(m => m.Priorita)
                    .ThenBy(m => m.NomeBevanda)
                    .ToListAsync();

                _logger.LogInformation($"Trovate {bevande.Count} bevande per ricerca: '{searchTerm}'");
                return bevande;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Errore nella ricerca bevande per: '{searchTerm}'");
                return new List<VwMenuDinamicoDTO>();
            }
        }

        public async Task<int> GetCountBevandeDisponibiliAsync()
        {
            try
            {
                var count = await _context.VwMenuDinamico
                    .Where(m => m.Priorita >= 0)
                    .CountAsync();

                _logger.LogInformation($"Totale bevande disponibili: {count}");
                return count;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore nel conteggio bevande disponibili");
                return 0;
            }
        }
    }
}