using Database.Models;
using DTO;
using Microsoft.EntityFrameworkCore;
using Repository.Helper;
using Repository.Interface;
using System.ComponentModel;

namespace Repository.Service
{
    public class DimensioneBicchiereRepository(BubbleTeaContext context) : IDimensioneBicchiereRepository
    {
        private readonly BubbleTeaContext _context = context;
                        
        private static DimensioneBicchiereDTO MapToDTO(DimensioneBicchiere bicchiereFrontend)
        {
            return new DimensioneBicchiereDTO
            {
                DimensioneBicchiereId = bicchiereFrontend.DimensioneBicchiereId,
                Sigla = bicchiereFrontend.Sigla,
                Descrizione = bicchiereFrontend.Descrizione,
                Capienza = bicchiereFrontend.Capienza,
                PrezzoBase = bicchiereFrontend.PrezzoBase,
                Moltiplicatore = bicchiereFrontend.Moltiplicatore,
                UnitaMisura = new UnitaDiMisuraDTO 
                {
                    UnitaMisuraId = bicchiereFrontend.UnitaMisura.UnitaMisuraId,
                    Sigla = bicchiereFrontend.UnitaMisura.Sigla,
                    Descrizione = bicchiereFrontend.UnitaMisura.Descrizione
                }
            };
        }

        private async Task<bool> SiglaExistsAsync(string sigla)
        {
            return await _context.DimensioneBicchiere
                .AnyAsync(d => d.Sigla == sigla);
        }

        private async Task<bool> DescrizioneExistsAsync(string descrizione)
        {
            return await _context.DimensioneBicchiere
                .AnyAsync(d => d.Descrizione == descrizione);
        }

        public async Task<DimensioneBicchiereDTO?> GetByIdAsync(int bicchiereId)
        {
            try
            {
                // ✅ Validazione input
                if (bicchiereId <= 0)
                    return null;

                // ✅ Query diretta al singolo record
                var dimensione = await _context.DimensioneBicchiere
                    .AsNoTracking()
                    .FirstOrDefaultAsync(d => d.DimensioneBicchiereId == bicchiereId);

                // ✅ Restituisci DTO o null
                return dimensione == null ? null : MapToDTO(dimensione);
            }
            catch (Exception)
            {
                // ✅ Gestione errori minimale
                return null;
            }
        }

        public async Task<PaginatedResponseDTO<DimensioneBicchiereDTO>> GetAllAsync(int page = 1, int pageSize = 10)
        {
            try
            {
                var (safePage, safePageSize) = SecurityHelper.ValidatePagination(page, pageSize);
                var skip = (safePage - 1) * safePageSize;

                var query = _context.DimensioneBicchiere
                    .AsNoTracking()
                    .OrderBy(d => d.Descrizione)
                    .ThenBy(d => d.Sigla);

                var totalCount = await query.CountAsync();
                var items = await query
                    .Skip(skip)
                    .Take(safePageSize)
                    .Select(d => MapToDTO(d))
                    .ToListAsync();

                return new PaginatedResponseDTO<DimensioneBicchiereDTO>
                {
                    Data = items,
                    Page = safePage,
                    PageSize = safePageSize,
                    TotalCount = totalCount,
                    Message = totalCount == 0
                        ? "Nessuna dimensione bicchiere trovata"
                        : $"Trovate {totalCount} dimensioni bicchiere"
                };
            }
            catch (Exception)
            {
                return new PaginatedResponseDTO<DimensioneBicchiereDTO>
                {
                    Data = [],
                    Page = 1,
                    PageSize = pageSize,
                    TotalCount = 0,
                    Message = "Errore nel recupero delle dimensioni bicchiere"
                };
            }
        }

        public async Task<PaginatedResponseDTO<DimensioneBicchiereDTO?>> GetBySiglaAsync(string? sigla, int page = 1, int pageSize = 10) //OK
        {
            // ✅ Validazione sicurezza input
            if (!SecurityHelper.IsValidInput(sigla))
            {
                throw new ArgumentException("Parametro sigla non valido per motivi di sicurezza");
            }

            // ✅ Paginazione sicura
            var (safePage, safePageSize) = SecurityHelper.ValidatePagination(page, pageSize);
            var skip = (safePage - 1) * safePageSize;

            // ✅ Gestione sicura del parametro nullable
            var normalizedSigla = string.IsNullOrWhiteSpace(sigla)
                ? string.Empty
                : StringHelper.NormalizeSearchTerm(sigla!);

            // ✅ Query base SENZA include (backend usa solo ID)
            var query = _context.DimensioneBicchiere
                .AsNoTracking()
                .AsQueryable();

            // ✅ Applica filtro solo se sigla non è null/empty
            // Usa "StartsWith" per ricerca "inizia con" (come frontend)
            if (!string.IsNullOrWhiteSpace(normalizedSigla))
            {
                query = query.Where(d =>
                    StringHelper.StartsWithCaseInsensitive(d.Sigla, normalizedSigla));
            }

            // ✅ Ordinamento coerente con frontend
            query = query.OrderBy(d => d.Sigla)
                         .ThenBy(d => d.Descrizione)
                         .ThenBy(d => d.Capienza);

            // ✅ Conteggio totale
            var totalCount = await query.CountAsync();

            // ✅ Paginazione e mapping con DTO backend
            var items = await query
                .Skip(skip)
                .Take(safePageSize)
                .Select(d => MapToDTO(d)) // ✅ Usa MapToDTO per DTO backend
                .ToListAsync();

            // ✅ Costruzione messaggio dinamico (uguale a frontend)
            var message = string.IsNullOrWhiteSpace(sigla)
                ? $"Trovate {totalCount} dimensioni bicchiere (tutte)"
                : $"Trovate {totalCount} dimensioni bicchiere con sigla che inizia con '{sigla}'";

            return new PaginatedResponseDTO<DimensioneBicchiereDTO?>
            {
                Data = items,
                Page = safePage,
                PageSize = safePageSize,
                TotalCount = totalCount,
                Message = message
            };
        }

        public async Task<PaginatedResponseDTO<DimensioneBicchiereDTO?>> GetByDescrizioneAsync(string? descrizione, int page = 1, int pageSize = 10)
        {
            // ✅ Validazione sicurezza input
            if (!SecurityHelper.IsValidInput(descrizione))
            {
                throw new ArgumentException("Parametro descrizione non valido per motivi di sicurezza");
            }

            // ✅ Paginazione sicura
            var (safePage, safePageSize) = SecurityHelper.ValidatePagination(page, pageSize);
            var skip = (safePage - 1) * safePageSize;

            // ✅ Gestione sicura del parametro nullable
            var normalizedDescrizione = string.IsNullOrWhiteSpace(descrizione)
                ? string.Empty
                : StringHelper.NormalizeSearchTerm(descrizione!);

            // ✅ Query base SENZA include (backend usa solo ID)
            var query = _context.DimensioneBicchiere
                .AsNoTracking()
                .AsQueryable();

            // ✅ Applica filtro solo se descrizione non è null/empty
            // Usa "Contains" per ricerca parziale nella descrizione
            if (!string.IsNullOrWhiteSpace(normalizedDescrizione))
            {
                query = query.Where(d =>
                    StringHelper.ContainsCaseInsensitive(d.Descrizione, normalizedDescrizione));
            }

            // ✅ Ordinamento coerente con frontend
            query = query.OrderBy(d => d.Descrizione)
                         .ThenBy(d => d.Sigla)
                         .ThenBy(d => d.Capienza);

            // ✅ Conteggio totale
            var totalCount = await query.CountAsync();

            // ✅ Paginazione e mapping con DTO backend
            var items = await query
                .Skip(skip)
                .Take(safePageSize)
                .Select(d => MapToDTO(d)) // ✅ Usa MapToDTO per DTO backend
                .ToListAsync();

            // ✅ Costruzione messaggio dinamico (uguale a frontend)
            var message = string.IsNullOrWhiteSpace(descrizione)
                ? $"Trovate {totalCount} dimensioni bicchiere (tutte)"
                : $"Trovate {totalCount} dimensioni bicchiere con descrizione contenente '{descrizione}'";

            return new PaginatedResponseDTO<DimensioneBicchiereDTO?>
            {
                Data = items,
                Page = safePage,
                PageSize = safePageSize,
                TotalCount = totalCount,
                Message = message
            };
        }

        public async Task<DimensioneBicchiereDTO> AddAsync(DimensioneBicchiereDTO bicchiereDto) //OK
        {
            ArgumentNullException.ThrowIfNull(bicchiereDto);

            if ((await SiglaExistsAsync(bicchiereDto.Sigla)) || (await DescrizioneExistsAsync(bicchiereDto.Descrizione)))
            {
                throw new ArgumentException($"Esiste già un bicchiere con la sigla '{bicchiereDto.Sigla}' e con la descrizione '{bicchiereDto.Descrizione}'");
            }

            var bicchiere = new DimensioneBicchiere
            {
                Sigla = bicchiereDto.Sigla,
                Descrizione = bicchiereDto.Descrizione,
                Capienza = bicchiereDto.Capienza,
                UnitaMisuraId = bicchiereDto.UnitaMisuraId,
                PrezzoBase = bicchiereDto.PrezzoBase,
                Moltiplicatore = bicchiereDto.Moltiplicatore
            };

            _context.DimensioneBicchiere.Add(bicchiere);
            await _context.SaveChangesAsync();

            bicchiereDto.DimensioneBicchiereId = bicchiere.DimensioneBicchiereId;
            return bicchiereDto;
        }

        private async Task<bool> SiglaExistsForOtherAsync(int excludeId, string sigla)
        {
            return await _context.DimensioneBicchiere
                .AnyAsync(d => d.DimensioneBicchiereId != excludeId &&
                              d.Sigla == sigla);
        }

        private async Task<bool> DescrizioneExistsForOtherAsync(int excludeId, string descrizione)
        {
            return await _context.DimensioneBicchiere
                .AnyAsync(d => d.DimensioneBicchiereId != excludeId &&
                              d.Descrizione == descrizione);
        }

        public async Task UpdateAsync(DimensioneBicchiereDTO bicchiereDto)
        {
            // ✅ Usa FindAsync per chiave primaria (più efficiente)
            var bicchiere = await _context.DimensioneBicchiere
                .FindAsync(bicchiereDto.DimensioneBicchiereId);

            ArgumentNullException.ThrowIfNull(bicchiere,
                $"Dimensione bicchiere con ID {bicchiereDto.DimensioneBicchiereId} non trovata");

            // ✅ Correzione: Controlla se la sigla ESISTE IN UN ALTRO RECORD (non in sé stesso)
            if (await SiglaExistsForOtherAsync(bicchiereDto.DimensioneBicchiereId, bicchiereDto.Sigla))
            {
                throw new ArgumentException($"Esiste già un'altra dimensione con la sigla '{bicchiereDto.Sigla}'");
            }

            // ✅ Correzione: Controlla se la descrizione ESISTE IN UN ALTRO RECORD
            if (await DescrizioneExistsForOtherAsync(bicchiereDto.DimensioneBicchiereId, bicchiereDto.Descrizione))
            {
                throw new ArgumentException($"Esiste già un'altra dimensione con la descrizione '{bicchiereDto.Descrizione}'");
            }

            // ✅ Aggiorna solo i campi che possono cambiare
            bicchiere.Sigla = bicchiereDto.Sigla;
            bicchiere.Descrizione = bicchiereDto.Descrizione;
            bicchiere.Capienza = bicchiereDto.Capienza;
            bicchiere.UnitaMisuraId = bicchiereDto.UnitaMisuraId;
            bicchiere.PrezzoBase = bicchiereDto.PrezzoBase;
            bicchiere.Moltiplicatore = bicchiereDto.Moltiplicatore;

            await _context.SaveChangesAsync();
        }

        private async Task<bool> HasDependenciesAsync(int dimensioneBicchiereId)
        {
            // ✅ Controlla in sequenza (più semplice da leggere)
            return await _context.BevandaStandard.AnyAsync(b => b.DimensioneBicchiereId == dimensioneBicchiereId) ||
                   await _context.DimensioneQuantitaIngredienti.AnyAsync(d => d.DimensioneBicchiereId == dimensioneBicchiereId) ||
                   await _context.PersonalizzazioneCustom.AnyAsync(p => p.DimensioneBicchiereId == dimensioneBicchiereId) ||
                   await _context.PreferitiCliente.AnyAsync(p => p.DimensioneBicchiereId == dimensioneBicchiereId);
        }

        public async Task<bool> DeleteAsync(int bicchiereid) //OK 
        {
            var bicchiere = await _context.DimensioneBicchiere.FindAsync(bicchiereid);
            if (bicchiere == null) return false;

            // ✅ Controlla dipendenze prima di eliminare
            if (await HasDependenciesAsync(bicchiereid))
                throw new InvalidOperationException($"Impossibile eliminare il bicchiere Id '{bicchiereid}' perché ci sono dipendenze attive");

            _context.DimensioneBicchiere.Remove(bicchiere);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<PaginatedResponseDTO<DimensioneBicchiereDTO?>> GetFrontendByIdAsync(int? bicchiereId = null, int page = 1, int pageSize = 10) //OK
        {
            // ✅ Validazione paginazione
            var (safePage, safePageSize) = SecurityHelper.ValidatePagination(page, pageSize);
            var skip = (safePage - 1) * safePageSize;

            // ✅ Query base con include per UnitaMisura
            var query = _context.DimensioneBicchiere
                .Include(d => d.UnitaMisura)
                .AsNoTracking()
                .AsQueryable();

            // ✅ Se bicchiereId ha valore, filtra per quello specifico
            if (bicchiereId.HasValue && bicchiereId > 0)
            {
                query = query.Where(d => d.DimensioneBicchiereId == bicchiereId.Value);
            }

            // ✅ Ordinamento per coerenza
            query = query.OrderBy(d => d.Descrizione)
                         .ThenBy(d => d.Sigla);

            // ✅ Conteggio totale
            var totalCount = await query.CountAsync();

            // ✅ Se c'è un ID specifico e non troviamo nulla
            if (bicchiereId.HasValue && totalCount == 0)
            {
                return new PaginatedResponseDTO<DimensioneBicchiereDTO?>
                {
                    Data = [null], // Record non trovato
                    Page = safePage,
                    PageSize = safePageSize,
                    TotalCount = 0,
                    Message = $"Dimensione bicchiere con ID {bicchiereId} non trovata"
                };
            }

            // ✅ Paginazione e mapping
            var items = await query
                .Skip(skip)
                .Take(safePageSize)
                .Select(d => MapToDTO(d))
                .ToListAsync();

            // ✅ Costruzione messaggio appropriato
            var message = bicchiereId.HasValue
                ? $"Trovata dimensione bicchiere ID {bicchiereId}"
                : $"Trovate {totalCount} dimensioni bicchiere (pagina {safePage} di {Math.Ceiling(totalCount / (double)safePageSize)})";

            return new PaginatedResponseDTO<DimensioneBicchiereDTO?>
            {
                Data = items,
                Page = safePage,
                PageSize = safePageSize,
                TotalCount = totalCount,
                Message = message
            };
        }

        public async Task<PaginatedResponseDTO<DimensioneBicchiereDTO?>> GetFrontendBySiglaAsync(string? sigla, int page = 1, int pageSize = 10) //OK
        {
            // ✅ Validazione sicurezza input
            if (!SecurityHelper.IsValidInput(sigla))
            {
                throw new ArgumentException("Parametro sigla non valido per motivi di sicurezza");
            }

            // ✅ Paginazione sicura
            var (safePage, safePageSize) = SecurityHelper.ValidatePagination(page, pageSize);
            var skip = (safePage - 1) * safePageSize;

            // ✅ Gestione sicura del parametro nullable
            var normalizedSigla = string.IsNullOrWhiteSpace(sigla)
                ? string.Empty
                : StringHelper.NormalizeSearchTerm(sigla!);

            // ✅ Query base con include
            var query = _context.DimensioneBicchiere
                .Include(d => d.UnitaMisura)
                .AsNoTracking()
                .AsQueryable();

            // ✅ Applica filtro solo se sigla non è null/empty
            // Usa "StartsWith" per ricerca "inizia con"
            if (!string.IsNullOrWhiteSpace(normalizedSigla))
            {
                query = query.Where(d =>
                    StringHelper.StartsWithCaseInsensitive(d.Sigla, normalizedSigla));
            }

            // ✅ Ordinamento coerente
            query = query.OrderBy(d => d.Sigla)
                         .ThenBy(d => d.Descrizione)
                         .ThenBy(d => d.Capienza);

            // ✅ Conteggio totale
            var totalCount = await query.CountAsync();

            // ✅ Paginazione e mapping
            var items = await query
                .Skip(skip)
                .Take(safePageSize)
                .Select(d => MapToDTO(d))
                .ToListAsync();

            // ✅ Costruzione messaggio dinamico
            var message = string.IsNullOrWhiteSpace(sigla)
                ? $"Trovate {totalCount} dimensioni bicchiere (tutte)"
                : $"Trovate {totalCount} dimensioni bicchiere con sigla che inizia con '{sigla}'";

            return new PaginatedResponseDTO<DimensioneBicchiereDTO?>
            {
                Data = items,
                Page = safePage,
                PageSize = safePageSize,
                TotalCount = totalCount,
                Message = message
            };
        }

        public async Task<PaginatedResponseDTO<DimensioneBicchiereDTO>> GetFrontendByDescrizioneAsync(string? descrizione, int page = 1, int pageSize = 10) //OK
        {
            // ✅ Validazione sicurezza input
            if (!SecurityHelper.IsValidInput(descrizione))
            {
                throw new ArgumentException("Parametro descrizione non valido per motivi di sicurezza");
            }

            // ✅ Paginazione sicura
            var (safePage, safePageSize) = SecurityHelper.ValidatePagination(page, pageSize);
            var skip = (safePage - 1) * safePageSize;

            // ✅ Gestione sicura del parametro nullable
            var normalizedDescrizione = string.IsNullOrWhiteSpace(descrizione)
                ? string.Empty
                : StringHelper.NormalizeSearchTerm(descrizione!);

            // ✅ Query base con include
            var query = _context.DimensioneBicchiere
                .Include(d => d.UnitaMisura)
                .AsNoTracking()
                .AsQueryable();

            // ✅ Applica filtro solo se descrizione non è null/empty
            // Usa "Contains" per ricerca parziale nella descrizione
            if (!string.IsNullOrWhiteSpace(normalizedDescrizione))
            {
                query = query.Where(d =>
                    StringHelper.ContainsCaseInsensitive(d.Descrizione, normalizedDescrizione));
            }

            // ✅ Ordinamento coerente (prima per descrizione, poi per sigla)
            query = query.OrderBy(d => d.Descrizione)
                         .ThenBy(d => d.Sigla)
                         .ThenBy(d => d.Capienza);

            // ✅ Conteggio totale
            var totalCount = await query.CountAsync();

            // ✅ Paginazione e mapping
            var items = await query
                .Skip(skip)
                .Take(safePageSize)
                .Select(d => MapToDTO(d))
                .ToListAsync();

            // ✅ Costruzione messaggio dinamico
            var message = string.IsNullOrWhiteSpace(descrizione)
                ? $"Trovate {totalCount} dimensioni bicchiere (tutte)"
                : $"Trovate {totalCount} dimensioni bicchiere con descrizione contenente '{descrizione}'";

            return new PaginatedResponseDTO<DimensioneBicchiereDTO>
            {
                Data = items,
                Page = safePage,
                PageSize = safePageSize,
                TotalCount = totalCount,
                Message = message
            };
        }

        public async Task<PaginatedResponseDTO<DimensioneBicchiereDTO>> GetFrontendAsync(string? sigla, string? descrizione, decimal? capienza, decimal? prezzoBase, decimal? moltiplicatore, int page = 1, int pageSize = 10)
        {
            // ✅ Validazione sicurezza per input stringhe
            if (!SecurityHelper.IsValidInput(sigla) || !SecurityHelper.IsValidInput(descrizione))
            {
                throw new ArgumentException("Parametri di ricerca non validi per motivi di sicurezza");
            }

            // ✅ Paginazione sicura (usa parametri di default se non specificati)
            var (safePage, safePageSize) = SecurityHelper.ValidatePagination(page, pageSize);
            var skip = (safePage - 1) * safePageSize;

            // ✅ Normalizza filtri con gestione null-safe
            var normalizedSigla = string.IsNullOrWhiteSpace(sigla)
                ? string.Empty
                : StringHelper.NormalizeSearchTerm(sigla!);

            var normalizedDescrizione = string.IsNullOrWhiteSpace(descrizione)
                ? string.Empty
                : StringHelper.NormalizeSearchTerm(descrizione!);

            // ✅ Query base CON INCLUDE per UnitaDiMisura
            var query = _context.DimensioneBicchiere
                .Include(d => d.UnitaMisura)
                .AsNoTracking()  // ✅ Aggiunto per performance
                .AsQueryable();

            // ✅ Filtro per sigla (case-insensitive, STARTS WITH - come specificato)
            if (!string.IsNullOrWhiteSpace(normalizedSigla))
                query = query.Where(d => StringHelper.StartsWithCaseInsensitive(d.Sigla, normalizedSigla));

            // ✅ Filtro per descrizione (case-insensitive, CONTAINS - ricerca parziale)
            if (!string.IsNullOrWhiteSpace(normalizedDescrizione))
                query = query.Where(d => StringHelper.ContainsCaseInsensitive(d.Descrizione, normalizedDescrizione));

            // ✅ Filtro per capienza (range ±10% se specificato)
            if (capienza.HasValue)
                query = query.Where(d => d.Capienza >= capienza.Value * 0.9m &&
                                         d.Capienza <= capienza.Value * 1.1m);

            // ✅ Filtro per prezzoBase (range ±10% se specificato)
            if (prezzoBase.HasValue)
                query = query.Where(d => d.PrezzoBase >= prezzoBase.Value * 0.9m &&
                                         d.PrezzoBase <= prezzoBase.Value * 1.1m);

            // ✅ Filtro per moltiplicatore (range ±10% se specificato)
            if (moltiplicatore.HasValue)
                query = query.Where(d => d.Moltiplicatore >= moltiplicatore.Value * 0.9m &&
                                         d.Moltiplicatore <= moltiplicatore.Value * 1.1m);

            // ✅ Ordinamento coerente: prima per descrizione, poi per sigla
            query = query.OrderBy(d => d.Descrizione)
                         .ThenBy(d => d.Sigla)
                         .ThenBy(d => d.Capienza);

            // ✅ Conteggio totale
            var totalCount = await query.CountAsync();

            // ✅ Paginazione e mapping usando MapToFrontendDTO
            var items = await query
                .Skip(skip)
                .Take(safePageSize)
                .Select(d => MapToDTO(d))  // ✅ Usa il metodo di mapping esistente
                .ToListAsync();

            // ✅ Costruzione messaggio dettagliato con filtri applicati
            var filterInfo = new System.Text.StringBuilder();
            if (!string.IsNullOrWhiteSpace(sigla)) filterInfo.Append($"Sigla inizia con: '{sigla}', ");
            if (!string.IsNullOrWhiteSpace(descrizione)) filterInfo.Append($"Descrizione contiene: '{descrizione}', ");
            if (capienza.HasValue) filterInfo.Append($"Capienza: {capienza}±10%, ");
            if (prezzoBase.HasValue) filterInfo.Append($"Prezzo base: {prezzoBase}±10%, ");
            if (moltiplicatore.HasValue) filterInfo.Append($"Moltiplicatore: {moltiplicatore}±10%, ");

            var filterStr = filterInfo.Length > 0
                ? $" con filtri: {filterInfo.ToString().TrimEnd(',', ' ')}"
                : "";

            var message = $"Trovate {totalCount} dimensioni bicchiere{filterStr}";

            return new PaginatedResponseDTO<DimensioneBicchiereDTO>
            {
                Data = items,
                Page = safePage,
                PageSize = safePageSize,
                TotalCount = totalCount,
                Message = message
            };
        }
    }
}