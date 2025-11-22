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

        private VwMenuDinamicoDTO MapToDTO(VwMenuDinamico entity)
        {
            return new VwMenuDinamicoDTO
            {
                Tipo = entity.Tipo,
                Id = entity.Id,
                NomeBevanda = entity.NomeBevanda,
                Descrizione = entity.Descrizione,
                PrezzoNetto = entity.PrezzoNetto,
                PrezzoLordo = entity.PrezzoLordo,
                TaxRateId = entity.TaxRateId,
                IvaPercentuale = entity.IvaPercentuale,
                ImmagineUrl = entity.ImmagineUrl,
                Priorita = entity.Priorita
            };
        }

        public async Task<IEnumerable<VwMenuDinamicoDTO>> GetMenuCompletoAsync()
        {
            try
            {
                var menu = await _context.VwMenuDinamico
                    .AsNoTracking()
                    .OrderByDescending(m => m.Priorita)
                    .ThenBy(m => m.NomeBevanda)
                    .Select(m => MapToDTO(m))
                    .ToListAsync();

                _logger.LogInformation("Recuperato menu completo con {Count} elementi", menu.Count);
                return menu;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore nel recupero menu completo");
                return Enumerable.Empty<VwMenuDinamicoDTO>();
            }
        }

        public async Task<IEnumerable<VwMenuDinamicoDTO>> GetPrimoPianoAsync(int numeroElementi = 6)
        {
            try
            {
                var primoPiano = await _context.VwMenuDinamico
                    .AsNoTracking()
                    .Where(m => m.Priorita >= 1)
                    .OrderByDescending(m => m.Priorita)
                    .ThenBy(m => m.NomeBevanda)
                    .Take(numeroElementi)
                    .Select(m => MapToDTO(m))
                    .ToListAsync();

                _logger.LogInformation("Recuperati {Count} elementi in primo piano", primoPiano.Count);
                return primoPiano;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore nel recupero elementi primo piano");
                return Enumerable.Empty<VwMenuDinamicoDTO>();
            }
        }

        public async Task<IEnumerable<VwMenuDinamicoDTO>> GetBevandeDisponibiliAsync()
        {
            try
            {
                var bevande = await _context.VwMenuDinamico
                    .AsNoTracking()
                    .Where(m => m.Priorita >= 0)
                    .OrderByDescending(m => m.Priorita)
                    .ThenBy(m => m.NomeBevanda)
                    .Select(m => MapToDTO(m))
                    .ToListAsync();

                _logger.LogInformation("Recuperate {Count} bevande disponibili", bevande.Count);
                return bevande;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore nel recupero bevande disponibili");
                return Enumerable.Empty<VwMenuDinamicoDTO>();
            }
        }

        public async Task<IEnumerable<VwMenuDinamicoDTO>> GetBevandePerCategoriaAsync(string categoria)
        {
            try
            {
                var bevande = await _context.VwMenuDinamico
                    .AsNoTracking()
                    .Where(m => m.Tipo == categoria && m.Priorita >= 0)
                    .OrderByDescending(m => m.Priorita)
                    .ThenBy(m => m.NomeBevanda)
                    .Select(m => MapToDTO(m))
                    .ToListAsync();

                _logger.LogInformation("Recuperate {Count} bevande per categoria: {Categoria}", bevande.Count, categoria);
                return bevande;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore nel recupero bevande per categoria: {Categoria}", categoria);
                return Enumerable.Empty<VwMenuDinamicoDTO>();
            }
        }

        public async Task<IEnumerable<VwMenuDinamicoDTO>> GetBevandePerPrioritaAsync(int prioritaMinima, int prioritaMassima)
        {
            try
            {
                var bevande = await _context.VwMenuDinamico
                    .AsNoTracking()
                    .Where(m => m.Priorita >= prioritaMinima && m.Priorita <= prioritaMassima)
                    .OrderByDescending(m => m.Priorita)
                    .ThenBy(m => m.NomeBevanda)
                    .Select(m => MapToDTO(m))
                    .ToListAsync();

                _logger.LogInformation("Recuperate {Count} bevande con priorità {Min}-{Max}", bevande.Count, prioritaMinima, prioritaMassima);
                return bevande;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore nel recupero bevande per priorità {Min}-{Max}", prioritaMinima, prioritaMassima);
                return Enumerable.Empty<VwMenuDinamicoDTO>();
            }
        }

        public async Task<IEnumerable<VwMenuDinamicoDTO>> GetBevandeConScontoAsync()
        {
            try
            {
                var bevande = await _context.VwMenuDinamico
                    .AsNoTracking()
                    .Where(m => m.PrezzoLordo.HasValue && m.PrezzoLordo < m.PrezzoNetto)
                    .OrderByDescending(m => m.Priorita)
                    .ThenBy(m => m.NomeBevanda)
                    .Select(m => MapToDTO(m))
                    .ToListAsync();

                _logger.LogInformation("Recuperate {Count} bevande con sconto", bevande.Count);
                return bevande;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore nel recupero bevande con sconto");
                return Enumerable.Empty<VwMenuDinamicoDTO>();
            }
        }

        public async Task<VwMenuDinamicoDTO?> GetBevandaByIdAsync(int id, string tipo)
        {
            try
            {
                var bevanda = await _context.VwMenuDinamico
                    .AsNoTracking()
                    .Where(m => m.Id == id && m.Tipo == tipo)
                    .Select(m => MapToDTO(m))
                    .FirstOrDefaultAsync();

                if (bevanda != null)
                    _logger.LogInformation("Recuperata bevanda: {Nome} (ID: {Id}, Tipo: {Tipo})", bevanda.NomeBevanda, id, tipo);
                else
                    _logger.LogWarning("Bevanda non trovata (ID: {Id}, Tipo: {Tipo})", id, tipo);

                return bevanda;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore nel recupero bevanda ID: {Id}, Tipo: {Tipo}", id, tipo);
                return null;
            }
        }

        public async Task<IEnumerable<string>> GetCategorieDisponibiliAsync()
        {
            try
            {
                var categorie = await _context.VwMenuDinamico
                    .AsNoTracking()
                    .Select(m => m.Tipo)
                    .Distinct()
                    .ToListAsync();

                _logger.LogInformation("Recuperate {Count} categorie disponibili", categorie.Count);
                return categorie;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore nel recupero categorie disponibili");
                return Enumerable.Empty<string>();
            }
        }

        public async Task<IEnumerable<VwMenuDinamicoDTO>> SearchBevandeAsync(string searchTerm)
        {
            // ✅ HAI AGGIUNTO la validazione del parametro
            if (string.IsNullOrWhiteSpace(searchTerm))
                return Enumerable.Empty<VwMenuDinamicoDTO>();

            try
            {
                var bevande = await _context.VwMenuDinamico
                    .AsNoTracking()
                    // ✅ HAI USATO EF.Functions.Like come suggerito
                    .Where(m => EF.Functions.Like(m.NomeBevanda, $"%{searchTerm}%") ||
                               (m.Descrizione != null && EF.Functions.Like(m.Descrizione, $"%{searchTerm}%")))
                    .OrderByDescending(m => m.Priorita)
                    .ThenBy(m => m.NomeBevanda)
                    .Select(m => MapToDTO(m))
                    .ToListAsync();

                _logger.LogInformation("Trovate {Count} bevande per ricerca: '{SearchTerm}'", bevande.Count, searchTerm);
                return bevande;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore nella ricerca bevande per: '{SearchTerm}'", searchTerm);
                return Enumerable.Empty<VwMenuDinamicoDTO>();
            }
        }

        public async Task<int> GetCountBevandeDisponibiliAsync()
        {
            try
            {
                var count = await _context.VwMenuDinamico
                    .AsNoTracking()
                    .Where(m => m.Priorita >= 0)
                    .CountAsync();

                _logger.LogInformation("Totale bevande disponibili: {Count}", count);
                return count;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore nel conteggio bevande disponibili");
                return 0;
            }
        }

        public async Task<bool> ExistsAsync(int id, string tipo)
        {
            try
            {
                var exists = await _context.VwMenuDinamico
                    .AsNoTracking()
                    .AnyAsync(m => m.Id == id && m.Tipo == tipo);

                return exists;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore nella verifica esistenza bevanda ID: {Id}, Tipo: {Tipo}", id, tipo);
                return false;
            }
        }
    }
}