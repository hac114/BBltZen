using BBltZen;
using DTO;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Repository.Helper;
using Repository.Interface;

namespace Repository.Service
{
    public class BevandaStandardRepository(BubbleTeaContext context, ILogger<BevandaStandardRepository> logger) : IBevandaStandardRepository
    {
        private readonly BubbleTeaContext _context = context;
        private readonly ILogger<BevandaStandardRepository> _logger = logger;

        // METODI PRIVATI DI SUPPORTO        
        private static bool IsUniqueConstraintViolation(DbUpdateException dbEx)
        {
            var sqlException = dbEx.InnerException as Microsoft.Data.SqlClient.SqlException
                                ?? dbEx.InnerException?.InnerException as Microsoft.Data.SqlClient.SqlException;

            if (sqlException != null)
            {
                // 2627: Violazione vincolo UNIQUE KEY
                // 2601: Violazione indice univoco
                return sqlException.Number == 2627 || sqlException.Number == 2601;
            }

            return false;
        }
        private async Task<List<string>> GetIngredientiByPersonalizzazioneAsync(int personalizzazioneId)
        {
            var ingredienti = await _context.PersonalizzazioneIngrediente
                .AsNoTracking()
                .Where(pi => pi.PersonalizzazioneId == personalizzazioneId)
                .Join(_context.Ingrediente,
                    pi => pi.IngredienteId,
                    i => i.IngredienteId,
                    (pi, i) => i.Ingrediente1)
                .Where(nome => !string.IsNullOrEmpty(nome))
                .Distinct()
                .ToListAsync();

            return ingredienti;
        }

        private async Task<List<PrezzoDimensioneDTO>> CalcolaPrezziPerDimensioniAsync(BevandaStandard bevandaStandard)
        {
            // ✅ Recupera TUTTE le dimensioni per questa combinazione Articolo+Personalizzazione
            var tutteBevandeStandard = await _context.BevandaStandard
                .AsNoTracking()
                .Where(bs => bs.ArticoloId == bevandaStandard.ArticoloId &&
                           bs.PersonalizzazioneId == bevandaStandard.PersonalizzazioneId &&
                           bs.SempreDisponibile) // Solo quelle sempre disponibili
                .ToListAsync();

            var dimensioneIds = tutteBevandeStandard.Select(bs => bs.DimensioneBicchiereId).Distinct().ToList();
            var dimensioni = await _context.DimensioneBicchiere
                .Where(d => dimensioneIds.Contains(d.DimensioneBicchiereId))
                .ToDictionaryAsync(d => d.DimensioneBicchiereId);

            var result = new List<PrezzoDimensioneDTO>();

            foreach (var bs in tutteBevandeStandard)
            {
                if (dimensioni.TryGetValue(bs.DimensioneBicchiereId, out var dimensione))
                {
                    var taxRateId = 1; // IVA standard
                    var aliquotaIva = await GetAliquotaIvaAsync(taxRateId);
                    var prezzoBase = bs.Prezzo * dimensione.Moltiplicatore;
                    var prezzoIva = CalcolaIva(prezzoBase, aliquotaIva);
                    var prezzoTotale = prezzoBase + prezzoIva;

                    result.Add(new PrezzoDimensioneDTO
                    {
                        DimensioneBicchiereId = dimensione.DimensioneBicchiereId,
                        Sigla = dimensione.Sigla,
                        Descrizione = $"{dimensione.Descrizione} {dimensione.Capienza}ml",
                        PrezzoNetto = Math.Round(prezzoBase, 2),
                        PrezzoIva = Math.Round(prezzoIva, 2),
                        PrezzoTotale = Math.Round(prezzoTotale, 2),
                        AliquotaIva = aliquotaIva
                    });
                }
            }

            // ✅ Ordina per DimensioneBicchiereId
            return [.. result.OrderBy(p => p.DimensioneBicchiereId)];
        }

        private async Task<decimal> GetAliquotaIvaAsync(int taxRateId)
        {
            var taxRate = await _context.TaxRates
                .AsNoTracking()
                .FirstOrDefaultAsync(tr => tr.TaxRateId == taxRateId);

            return taxRate?.Aliquota ?? 22.00m;
        }

        private static decimal CalcolaIva(decimal prezzoNetto, decimal aliquotaIva)
        {
            return prezzoNetto * (aliquotaIva / 100);
        }

        private async Task<BevandaStandardDTO> MapToDTO(BevandaStandard bevandaStandard)
        {
            var dimensione = await _context.DimensioneBicchiere
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.DimensioneBicchiereId == bevandaStandard.DimensioneBicchiereId);

            UnitaDiMisuraDTO? unitaMisuraDto = null;
            if (dimensione != null)
            {
                var unitaMisura = await _context.UnitaDiMisura
                    .AsNoTracking()
                    .FirstOrDefaultAsync(u => u.UnitaMisuraId == dimensione.UnitaMisuraId);
                if (unitaMisura != null)
                {
                    unitaMisuraDto = new UnitaDiMisuraDTO
                    {
                        UnitaMisuraId = unitaMisura.UnitaMisuraId,
                        Sigla = unitaMisura.Sigla,
                        Descrizione = unitaMisura.Descrizione
                    };
                }
            }

            return new BevandaStandardDTO
            {
                ArticoloId = bevandaStandard.ArticoloId,
                PersonalizzazioneId = bevandaStandard.PersonalizzazioneId,
                DimensioneBicchiereId = bevandaStandard.DimensioneBicchiereId,
                Prezzo = bevandaStandard.Prezzo,
                ImmagineUrl = bevandaStandard.ImmagineUrl,
                Disponibile = bevandaStandard.Disponibile,
                SempreDisponibile = bevandaStandard.SempreDisponibile,
                Priorita = bevandaStandard.Priorita,
                DataCreazione = bevandaStandard.DataCreazione,
                DataAggiornamento = bevandaStandard.DataAggiornamento,
                DimensioneBicchiere = dimensione != null
                    ? new DimensioneBicchiereDTO
                    {
                        DimensioneBicchiereId = dimensione.DimensioneBicchiereId,
                        Sigla = dimensione.Sigla,
                        Descrizione = dimensione.Descrizione,
                        Capienza = dimensione.Capienza,
                        UnitaMisuraId = dimensione.UnitaMisuraId,
                        PrezzoBase = dimensione.PrezzoBase,
                        Moltiplicatore = dimensione.Moltiplicatore,
                        UnitaMisura = unitaMisuraDto   // Aggiunto
                    }
                    : null
            };
        }

        private async Task<List<BevandaStandardDTO>> MapToDTOList(List<BevandaStandard> bevandeStandard)
        {
            var dimensioneIds = bevandeStandard.Select(bs => bs.DimensioneBicchiereId).Distinct().ToList();

            // Carica tutte le dimensioni necessarie
            var dimensioni = await _context.DimensioneBicchiere
                .Where(d => dimensioneIds.Contains(d.DimensioneBicchiereId))
                .ToListAsync();

            // Carica tutte le unità di misura necessarie
            var unitaMisuraIds = dimensioni.Select(d => d.UnitaMisuraId).Distinct().ToList();
            var unitaMisuraDict = await _context.UnitaDiMisura
                .Where(u => unitaMisuraIds.Contains(u.UnitaMisuraId))
                .ToDictionaryAsync(u => u.UnitaMisuraId);

            var result = new List<BevandaStandardDTO>();

            foreach (var bs in bevandeStandard)
            {
                var dimensione = dimensioni.FirstOrDefault(d => d.DimensioneBicchiereId == bs.DimensioneBicchiereId);

                UnitaDiMisuraDTO? unitaMisuraDto = null;
                if (dimensione != null && unitaMisuraDict.TryGetValue(dimensione.UnitaMisuraId, out var unitaMisura))
                {
                    unitaMisuraDto = new UnitaDiMisuraDTO
                    {
                        UnitaMisuraId = unitaMisura.UnitaMisuraId,
                        Sigla = unitaMisura.Sigla,
                        Descrizione = unitaMisura.Descrizione
                    };
                }

                result.Add(new BevandaStandardDTO
                {
                    ArticoloId = bs.ArticoloId,
                    PersonalizzazioneId = bs.PersonalizzazioneId,
                    DimensioneBicchiereId = bs.DimensioneBicchiereId,
                    Prezzo = bs.Prezzo,
                    ImmagineUrl = bs.ImmagineUrl,
                    Disponibile = bs.Disponibile,
                    SempreDisponibile = bs.SempreDisponibile,
                    Priorita = bs.Priorita,
                    DataCreazione = bs.DataCreazione,
                    DataAggiornamento = bs.DataAggiornamento,
                    DimensioneBicchiere = dimensione != null
                        ? new DimensioneBicchiereDTO
                        {
                            DimensioneBicchiereId = dimensione.DimensioneBicchiereId,
                            Sigla = dimensione.Sigla,
                            Descrizione = dimensione.Descrizione,
                            Capienza = dimensione.Capienza,
                            UnitaMisuraId = dimensione.UnitaMisuraId,
                            PrezzoBase = dimensione.PrezzoBase,
                            Moltiplicatore = dimensione.Moltiplicatore,
                            UnitaMisura = unitaMisuraDto  // ✅ Aggiunto!
                        }
                        : null
                });
            }

            return result;
        }

        private async Task<BevandaStandardCardDTO> MapToCardDTO(BevandaStandard bevandaStandard)
        {
            var personalizzazione = await _context.Personalizzazione
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.PersonalizzazioneId == bevandaStandard.PersonalizzazioneId);

            var ingredienti = await GetIngredientiByPersonalizzazioneAsync(bevandaStandard.PersonalizzazioneId);
            var prezziPerDimensioni = await CalcolaPrezziPerDimensioniAsync(bevandaStandard);

            return new BevandaStandardCardDTO
            {
                ArticoloId = bevandaStandard.ArticoloId,
                Nome = personalizzazione?.Nome ?? "Bevanda Standard",
                Descrizione = personalizzazione?.Descrizione,
                ImmagineUrl = bevandaStandard.ImmagineUrl,
                Disponibile = bevandaStandard.Disponibile,
                SempreDisponibile = bevandaStandard.SempreDisponibile,
                Priorita = bevandaStandard.Priorita,
                PrezziPerDimensioni = prezziPerDimensioni,
                Ingredienti = ingredienti
            };
        }

        private async Task<List<BevandaStandardCardDTO>> MapToCardDTOList(List<BevandaStandard> bevandeStandard)
        {
            var personalizzazioneIds = bevandeStandard.Select(bs => bs.PersonalizzazioneId).Distinct().ToList();

            var personalizzazioni = await _context.Personalizzazione
                .Where(p => personalizzazioneIds.Contains(p.PersonalizzazioneId))
                .ToDictionaryAsync(p => p.PersonalizzazioneId);

            var result = new List<BevandaStandardCardDTO>();

            foreach (var bs in bevandeStandard)
            {
                personalizzazioni.TryGetValue(bs.PersonalizzazioneId, out var personalizzazione);

                var ingredienti = await GetIngredientiByPersonalizzazioneAsync(bs.PersonalizzazioneId);
                var prezziPerDimensioni = await CalcolaPrezziPerDimensioniAsync(bs);

                result.Add(new BevandaStandardCardDTO
                {
                    ArticoloId = bs.ArticoloId,
                    Nome = personalizzazione?.Nome ?? "Bevanda Standard",
                    Descrizione = personalizzazione?.Descrizione,
                    ImmagineUrl = bs.ImmagineUrl,
                    Disponibile = bs.Disponibile,
                    SempreDisponibile = bs.SempreDisponibile,
                    Priorita = bs.Priorita,
                    PrezziPerDimensioni = prezziPerDimensioni,
                    Ingredienti = ingredienti
                });
            }

            return result;
        }

        public async Task<PaginatedResponseDTO<BevandaStandardDTO>> GetAllAsync(int page = 1, int pageSize = 10)
        {
            try
            {
                var (safePage, safePageSize) = SecurityHelper.ValidatePagination(page, pageSize);
                var skip = (safePage - 1) * safePageSize;

                var query = _context.BevandaStandard
                    .AsNoTracking()
                    .OrderByDescending(bs => bs.DataCreazione);

                var totalCount = await query.CountAsync();

                var bevandeStandard = await query
                    .Skip(skip)
                    .Take(safePageSize)
                    .ToListAsync();

                var result = await MapToDTOList(bevandeStandard);

                return new PaginatedResponseDTO<BevandaStandardDTO>
                {
                    Data = result,
                    Page = safePage,
                    PageSize = safePageSize,
                    TotalCount = totalCount,
                    Message = totalCount switch
                    {
                        0 => "Nessuna bevanda standard trovata",
                        1 => "Trovata 1 bevanda standard",
                        _ => $"Trovate {totalCount} bevande standard"
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore in GetAllAsync");
                return new PaginatedResponseDTO<BevandaStandardDTO>
                {
                    Data = [],
                    Page = 1,
                    PageSize = pageSize,
                    TotalCount = 0,
                    Message = "Errore nel recupero delle bevande standard"
                };
            }
        }

        public async Task<SingleResponseDTO<BevandaStandardDTO>> GetByIdAsync(int articoloId)
        {
            try
            {
                if (articoloId <= 0)
                    return SingleResponseDTO<BevandaStandardDTO>.ErrorResponse("ID articolo non valido");

                var bevandaStandard = await _context.BevandaStandard
                    .AsNoTracking()
                    .FirstOrDefaultAsync(bs => bs.ArticoloId == articoloId);

                if (bevandaStandard == null)
                    return SingleResponseDTO<BevandaStandardDTO>.NotFoundResponse($"Bevanda standard con ArticoloId {articoloId} non trovata");

                var dto = await MapToDTO(bevandaStandard);

                return SingleResponseDTO<BevandaStandardDTO>.SuccessResponse(
                    dto,
                    $"Bevanda standard con ArticoloId {articoloId} trovata");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore in GetByIdAsync per ArticoloId: {ArticoloId}", articoloId);
                return SingleResponseDTO<BevandaStandardDTO>.ErrorResponse("Errore interno nel recupero della bevanda standard");
            }
        }

        public async Task<PaginatedResponseDTO<BevandaStandardDTO>> GetDisponibiliAsync(int page = 1, int pageSize = 10)
        {
            try
            {
                var (safePage, safePageSize) = SecurityHelper.ValidatePagination(page, pageSize);
                var skip = (safePage - 1) * safePageSize;

                var query = _context.BevandaStandard
                    .AsNoTracking()
                    .Where(bs => bs.SempreDisponibile)
                    .OrderByDescending(bs => bs.Priorita)
                    .ThenBy(bs => bs.ArticoloId);

                var totalCount = await query.CountAsync();

                var bevandeStandard = await query
                    .Skip(skip)
                    .Take(safePageSize)
                    .ToListAsync();

                var result = await MapToDTOList(bevandeStandard);

                string message = totalCount switch
                {
                    0 => "Nessuna bevanda standard disponibile trovata",
                    1 => "Trovata 1 bevanda standard disponibile",
                    _ => $"Trovate {totalCount} bevande standard disponibili"
                };

                return new PaginatedResponseDTO<BevandaStandardDTO>
                {
                    Data = result,
                    Page = safePage,
                    PageSize = safePageSize,
                    TotalCount = totalCount,
                    Message = message
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore in GetDisponibiliAsync");
                return new PaginatedResponseDTO<BevandaStandardDTO>
                {
                    Data = [],
                    Page = 1,
                    PageSize = pageSize,
                    TotalCount = 0,
                    Message = "Errore nel recupero delle bevande standard disponibili"
                };
            }
        }

        public async Task<PaginatedResponseDTO<BevandaStandardDTO>> GetAllOrderedByDimensioneAsync(int page = 1, int pageSize = 10)
        {
            try
            {
                var (safePage, safePageSize) = SecurityHelper.ValidatePagination(page, pageSize);
                var skip = (safePage - 1) * safePageSize;

                var query = _context.BevandaStandard
                    .AsNoTracking()
                    .Where(bs => bs.SempreDisponibile)
                    .OrderBy(bs => bs.DimensioneBicchiereId)
                    .ThenByDescending(bs => bs.Priorita);

                var totalCount = await query.CountAsync();

                var bevandeStandard = await query
                    .Skip(skip)
                    .Take(safePageSize)
                    .ToListAsync();

                var result = await MapToDTOList(bevandeStandard);

                string message = totalCount switch
                {
                    0 => "Nessuna bevanda standard trovata per dimensione bicchiere",
                    1 => "Trovata 1 bevanda standard per dimensione bicchiere",
                    _ => $"Trovate {totalCount} bevande standard per dimensione bicchiere"
                };

                return new PaginatedResponseDTO<BevandaStandardDTO>
                {
                    Data = result,
                    Page = safePage,
                    PageSize = safePageSize,
                    TotalCount = totalCount,
                    Message = message
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore in GetByDimensioneBicchiereAsync");
                return new PaginatedResponseDTO<BevandaStandardDTO>
                {
                    Data = [],
                    Page = 1,
                    PageSize = pageSize,
                    TotalCount = 0,
                    Message = "Errore nel recupero delle bevande standard per dimensione bicchiere"
                };
            }
        }

        public async Task<PaginatedResponseDTO<BevandaStandardDTO>> GetAllOrderedByPersonalizzazioneAsync(int page = 1, int pageSize = 10)
        {
            try
            {
                var (safePage, safePageSize) = SecurityHelper.ValidatePagination(page, pageSize);
                var skip = (safePage - 1) * safePageSize;

                var query = _context.BevandaStandard
                    .AsNoTracking()
                    .Where(bs => bs.SempreDisponibile)
                    .OrderBy(bs => bs.PersonalizzazioneId)
                    .ThenByDescending(bs => bs.Priorita);

                var totalCount = await query.CountAsync();

                var bevandeStandard = await query
                    .Skip(skip)
                    .Take(safePageSize)
                    .ToListAsync();

                var result = await MapToDTOList(bevandeStandard);

                string message = totalCount switch
                {
                    0 => "Nessuna bevanda standard trovata per personalizzazione",
                    1 => "Trovata 1 bevanda standard per personalizzazione",
                    _ => $"Trovate {totalCount} bevande standard per personalizzazione"
                };

                return new PaginatedResponseDTO<BevandaStandardDTO>
                {
                    Data = result,
                    Page = safePage,
                    PageSize = safePageSize,
                    TotalCount = totalCount,
                    Message = message
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore in GetByPersonalizzazioneAsync");
                return new PaginatedResponseDTO<BevandaStandardDTO>
                {
                    Data = [],
                    Page = 1,
                    PageSize = pageSize,
                    TotalCount = 0,
                    Message = "Errore nel recupero delle bevande standard per personalizzazione"
                };
            }
        }

        public async Task<PaginatedResponseDTO<BevandaStandardDTO>> GetByDimensioneBicchiereAsync(int dimensioneBicchiereId, int page = 1, int pageSize = 10)
        {
            try
            {
                if (dimensioneBicchiereId <= 0)
                    return new PaginatedResponseDTO<BevandaStandardDTO>
                    {
                        Data = [],
                        Page = 1,
                        PageSize = pageSize,
                        TotalCount = 0,
                        Message = "ID dimensione bicchiere non valido"
                    };

                var (safePage, safePageSize) = SecurityHelper.ValidatePagination(page, pageSize);
                var skip = (safePage - 1) * safePageSize;

                var query = _context.BevandaStandard
                    .AsNoTracking()
                    .Where(bs => bs.DimensioneBicchiereId == dimensioneBicchiereId && bs.SempreDisponibile)
                    .OrderByDescending(bs => bs.Priorita);

                var totalCount = await query.CountAsync();

                var bevandeStandard = await query
                    .Skip(skip)
                    .Take(safePageSize)
                    .ToListAsync();

                var result = await MapToDTOList(bevandeStandard);

                string message = totalCount switch
                {
                    0 => $"Nessuna bevanda standard trovata per dimensione bicchiere ID: {dimensioneBicchiereId}",
                    1 => $"Trovata 1 bevanda standard per dimensione bicchiere ID: {dimensioneBicchiereId}",
                    _ => $"Trovate {totalCount} bevande standard per dimensione bicchiere ID: {dimensioneBicchiereId}"
                };

                return new PaginatedResponseDTO<BevandaStandardDTO>
                {
                    Data = result,
                    Page = safePage,
                    PageSize = safePageSize,
                    TotalCount = totalCount,
                    Message = message
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore in GetByDimensioneBicchiereAsync per DimensioneBicchiereId: {DimensioneBicchiereId}",
                    dimensioneBicchiereId);
                return new PaginatedResponseDTO<BevandaStandardDTO>
                {
                    Data = [],
                    Page = 1,
                    PageSize = pageSize,
                    TotalCount = 0,
                    Message = "Errore nel recupero delle bevande standard per dimensione bicchiere"
                };
            }
        }

        public async Task<PaginatedResponseDTO<BevandaStandardDTO>> GetByPersonalizzazioneAsync(int personalizzazioneId, int page = 1, int pageSize = 10)
        {
            try
            {
                if (personalizzazioneId <= 0)
                    return new PaginatedResponseDTO<BevandaStandardDTO>
                    {
                        Data = [],
                        Page = 1,
                        PageSize = pageSize,
                        TotalCount = 0,
                        Message = "ID personalizzazione non valido"
                    };

                var (safePage, safePageSize) = SecurityHelper.ValidatePagination(page, pageSize);
                var skip = (safePage - 1) * safePageSize;

                var query = _context.BevandaStandard
                    .AsNoTracking()
                    .Where(bs => bs.PersonalizzazioneId == personalizzazioneId && bs.SempreDisponibile)
                    .OrderByDescending(bs => bs.Priorita);

                var totalCount = await query.CountAsync();

                var bevandeStandard = await query
                    .Skip(skip)
                    .Take(safePageSize)
                    .ToListAsync();

                var result = await MapToDTOList(bevandeStandard);

                string message = totalCount switch
                {
                    0 => $"Nessuna bevanda standard trovata per personalizzazione ID: {personalizzazioneId}",
                    1 => $"Trovata 1 bevanda standard per personalizzazione ID: {personalizzazioneId}",
                    _ => $"Trovate {totalCount} bevande standard per personalizzazione ID: {personalizzazioneId}"
                };

                return new PaginatedResponseDTO<BevandaStandardDTO>
                {
                    Data = result,
                    Page = safePage,
                    PageSize = safePageSize,
                    TotalCount = totalCount,
                    Message = message
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore in GetByPersonalizzazioneAsync per PersonalizzazioneId: {PersonalizzazioneId}", personalizzazioneId);
                return new PaginatedResponseDTO<BevandaStandardDTO>
                {
                    Data = [],
                    Page = 1,
                    PageSize = pageSize,
                    TotalCount = 0,
                    Message = "Errore nel recupero delle bevande standard per personalizzazione"
                };
            }
        }

        private async Task<bool> ExistsByCombinazioneInternalAsync(int personalizzazioneId, int dimensioneBicchiereId)
        {
            return await _context.BevandaStandard
                .AsNoTracking()
                .AnyAsync(bs => bs.PersonalizzazioneId == personalizzazioneId &&
                                bs.DimensioneBicchiereId == dimensioneBicchiereId);
        }

        public async Task<SingleResponseDTO<BevandaStandardDTO>> AddAsync(BevandaStandardDTO bevandaStandardDto)
        {
            try
            {
                ArgumentNullException.ThrowIfNull(bevandaStandardDto);

                // ✅ Validazione vincoli configurazione
                if (bevandaStandardDto.PersonalizzazioneId <= 0)
                    return SingleResponseDTO<BevandaStandardDTO>.ErrorResponse("Il parametro PersonalizzazioneId è obbligatorio");

                if (bevandaStandardDto.DimensioneBicchiereId <= 0)
                    return SingleResponseDTO<BevandaStandardDTO>.ErrorResponse("Il parametro DimensioneBicchiereId è obbligatorio");

                // ✅ Validazione prezzo decimal(4,2)
                if (bevandaStandardDto.Prezzo < 0 || bevandaStandardDto.Prezzo > 99.99m)
                    return SingleResponseDTO<BevandaStandardDTO>.ErrorResponse("Il prezzo deve essere tra 0.00 e 99.99");

                // ✅ Validazione priorità 1-10
                if (bevandaStandardDto.Priorita < 1 || bevandaStandardDto.Priorita > 10)
                    return SingleResponseDTO<BevandaStandardDTO>.ErrorResponse("La priorità deve essere tra 1 e 10");

                // ✅ Vincolo di coerenza: (!SempreDisponibile && !Disponibile) || (SempreDisponibile)
                if (!bevandaStandardDto.SempreDisponibile && bevandaStandardDto.Disponibile)
                {
                    // Se SempreDisponibile=false, allora Disponibile DEVE essere false
                    bevandaStandardDto.Disponibile = false;
                    _logger.LogWarning("Vincolo di coerenza: Disponibile forzato a false perché SempreDisponibile è false");
                }

                // ✅ Controllo esistenza combinazione (PersonalizzazioneId + DimensioneBicchiereId)
                if (await ExistsByCombinazioneInternalAsync(bevandaStandardDto.PersonalizzazioneId, bevandaStandardDto.DimensioneBicchiereId))
                    return SingleResponseDTO<BevandaStandardDTO>.ErrorResponse(
                        $"Esiste già una bevanda standard con PersonalizzazioneId: {bevandaStandardDto.PersonalizzazioneId} e DimensioneBicchiereId: {bevandaStandardDto.DimensioneBicchiereId}");

                // ✅ Creazione Articolo (obbligatorio per FK)
                var articolo = new Articolo
                {
                    Tipo = "BEVANDA_STANDARD",
                    DataCreazione = DateTime.Now,
                    DataAggiornamento = DateTime.Now
                };

                _context.Articolo.Add(articolo);
                await _context.SaveChangesAsync();

                // ✅ Creazione BevandaStandard con ArticoloId generato
                var bevandaStandard = new BevandaStandard
                {
                    ArticoloId = articolo.ArticoloId,
                    PersonalizzazioneId = bevandaStandardDto.PersonalizzazioneId,
                    DimensioneBicchiereId = bevandaStandardDto.DimensioneBicchiereId,
                    Prezzo = Math.Round(bevandaStandardDto.Prezzo, 2), // ✅ Arrotondamento a 2 decimali
                    ImmagineUrl = StringHelper.IsValidUrlInput(bevandaStandardDto.ImmagineUrl)
                        ? bevandaStandardDto.ImmagineUrl
                        : null,
                    Disponibile = bevandaStandardDto.Disponibile,
                    SempreDisponibile = bevandaStandardDto.SempreDisponibile,
                    Priorita = bevandaStandardDto.Priorita,
                    DataCreazione = DateTime.Now,
                    DataAggiornamento = DateTime.Now
                };

                _context.BevandaStandard.Add(bevandaStandard);
                await _context.SaveChangesAsync();

                // ✅ Aggiorna DTO con valori DB
                bevandaStandardDto.ArticoloId = bevandaStandard.ArticoloId;
                bevandaStandardDto.DataCreazione = bevandaStandard.DataCreazione;
                bevandaStandardDto.DataAggiornamento = bevandaStandard.DataAggiornamento;

                return SingleResponseDTO<BevandaStandardDTO>.SuccessResponse(
                    bevandaStandardDto,
                    $"Bevanda standard creata con successo (ArticoloId: {bevandaStandardDto.ArticoloId})");
            }
            catch (DbUpdateException dbEx) when (IsUniqueConstraintViolation(dbEx))
            {
                // ✅ NUOVO: Gestione specifica per violazione vincolo UNIQUE
                _logger.LogError(dbEx, "Violazione vincolo UNIQUE in AddAsync");
                return SingleResponseDTO<BevandaStandardDTO>.ErrorResponse(
                    $"Esiste già una bevanda standard con PersonalizzazioneId: {bevandaStandardDto.PersonalizzazioneId} e DimensioneBicchiereId: {bevandaStandardDto.DimensioneBicchiereId}");
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "Errore DB in AddAsync - Vincoli violati");
                return SingleResponseDTO<BevandaStandardDTO>.ErrorResponse(
                    "Errore di validazione del database. Verifica i vincoli (prezzo, priorità, coerenza disponibilità)");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore in AddAsync");
                return SingleResponseDTO<BevandaStandardDTO>.ErrorResponse(
                    "Errore interno durante la creazione della bevanda standard");
            }
        }

        private async Task<bool> ExistsByCombinazioneInternalAsync(int personalizzazioneId, int dimensioneBicchiereId, int excludeArticoloId)
        {
            return await _context.BevandaStandard
                .AsNoTracking()
                .AnyAsync(bs => bs.PersonalizzazioneId == personalizzazioneId &&
                               bs.DimensioneBicchiereId == dimensioneBicchiereId &&
                               bs.ArticoloId != excludeArticoloId);
        }

        public async Task<SingleResponseDTO<bool>> UpdateAsync(BevandaStandardDTO bevandaStandardDto)
        {
            try
            {
                ArgumentNullException.ThrowIfNull(bevandaStandardDto);

                if (bevandaStandardDto.ArticoloId <= 0)
                    return SingleResponseDTO<bool>.ErrorResponse("ID bevanda standard obbligatorio");

                // ✅ Validazione vincoli configurazione
                if (bevandaStandardDto.Prezzo < 0 || bevandaStandardDto.Prezzo > 99.99m)
                    return SingleResponseDTO<bool>.ErrorResponse("Il prezzo deve essere tra 0.00 e 99.99");

                if (bevandaStandardDto.Priorita < 1 || bevandaStandardDto.Priorita > 10)
                    return SingleResponseDTO<bool>.ErrorResponse("La priorità deve essere tra 1 e 10");

                // ✅ Vincolo di coerenza: (!SempreDisponibile && !Disponibile) || (SempreDisponibile)
                if (!bevandaStandardDto.SempreDisponibile && bevandaStandardDto.Disponibile)
                {
                    bevandaStandardDto.Disponibile = false;
                    _logger.LogWarning("Vincolo di coerenza in aggiornamento: Disponibile forzato a false");
                }

                var bevandaStandard = await _context.BevandaStandard
                    .FirstOrDefaultAsync(bs => bs.ArticoloId == bevandaStandardDto.ArticoloId);

                if (bevandaStandard == null)
                    return SingleResponseDTO<bool>.NotFoundResponse(
                        $"Bevanda standard con ArticoloId {bevandaStandardDto.ArticoloId} non trovata");

                // ✅ CRITICO: Controllo duplicati per combinazione se stiamo cambiando PersonalizzazioneId o DimensioneBicchiereId
                bool cambiaPersonalizzazione = bevandaStandard.PersonalizzazioneId != bevandaStandardDto.PersonalizzazioneId;
                bool cambiaDimensione = bevandaStandard.DimensioneBicchiereId != bevandaStandardDto.DimensioneBicchiereId;

                if ((cambiaPersonalizzazione || cambiaDimensione) &&
                    await ExistsByCombinazioneInternalAsync(bevandaStandardDto.PersonalizzazioneId, bevandaStandardDto.DimensioneBicchiereId, bevandaStandardDto.ArticoloId))
                {
                    return SingleResponseDTO<bool>.ErrorResponse(
                        $"Esiste già un'altra bevanda standard con PersonalizzazioneId: {bevandaStandardDto.PersonalizzazioneId} e DimensioneBicchiereId: {bevandaStandardDto.DimensioneBicchiereId}");
                }

                bool hasChanges = false;

                // ✅ Controllo cambiamenti con validazione
                if (cambiaPersonalizzazione)
                {
                    bevandaStandard.PersonalizzazioneId = bevandaStandardDto.PersonalizzazioneId;
                    hasChanges = true;
                }

                if (cambiaDimensione)
                {
                    bevandaStandard.DimensioneBicchiereId = bevandaStandardDto.DimensioneBicchiereId;
                    hasChanges = true;
                }

                var prezzoArrotondato = Math.Round(bevandaStandardDto.Prezzo, 2);
                if (bevandaStandard.Prezzo != prezzoArrotondato)
                {
                    bevandaStandard.Prezzo = prezzoArrotondato;
                    hasChanges = true;
                }

                var immagineUrlValidata = StringHelper.IsValidUrlInput(bevandaStandardDto.ImmagineUrl)
                    ? bevandaStandardDto.ImmagineUrl
                    : null;
                if (bevandaStandard.ImmagineUrl != immagineUrlValidata)
                {
                    bevandaStandard.ImmagineUrl = immagineUrlValidata;
                    hasChanges = true;
                }

                if (bevandaStandard.Disponibile != bevandaStandardDto.Disponibile)
                {
                    bevandaStandard.Disponibile = bevandaStandardDto.Disponibile;
                    hasChanges = true;
                }

                if (bevandaStandard.SempreDisponibile != bevandaStandardDto.SempreDisponibile)
                {
                    bevandaStandard.SempreDisponibile = bevandaStandardDto.SempreDisponibile;
                    hasChanges = true;
                }

                if (bevandaStandard.Priorita != bevandaStandardDto.Priorita)
                {
                    bevandaStandard.Priorita = bevandaStandardDto.Priorita;
                    hasChanges = true;
                }

                if (hasChanges)
                {
                    bevandaStandard.DataAggiornamento = DateTime.Now;
                    await _context.SaveChangesAsync();
                    return SingleResponseDTO<bool>.SuccessResponse(true,
                        $"Bevanda standard con ArticoloId: {bevandaStandard.ArticoloId} aggiornata con successo");
                }
                else
                {
                    return SingleResponseDTO<bool>.SuccessResponse(false,
                        $"Nessuna modifica necessaria per bevanda standard con ArticoloId: {bevandaStandard.ArticoloId}");
                }
            }
            catch (DbUpdateException dbEx) when (IsUniqueConstraintViolation(dbEx))
            {
                // ✅ NUOVO: Gestione specifica per violazione vincolo UNIQUE
                _logger.LogError(dbEx, "Violazione vincolo UNIQUE in UpdateAsync per ArticoloId: {ArticoloId}",
                    bevandaStandardDto?.ArticoloId);
                return SingleResponseDTO<bool>.ErrorResponse(
                    $"Esiste già un'altra bevanda standard con PersonalizzazioneId: {bevandaStandardDto?.PersonalizzazioneId} e DimensioneBicchiereId: {bevandaStandardDto?.DimensioneBicchiereId}");
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "Errore DB in UpdateAsync per ArticoloId: {ArticoloId}", bevandaStandardDto?.ArticoloId);
                return SingleResponseDTO<bool>.ErrorResponse("Errore di validazione del database durante l'aggiornamento");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore in UpdateAsync per ArticoloId: {ArticoloId}", bevandaStandardDto?.ArticoloId);
                return SingleResponseDTO<bool>.ErrorResponse("Errore interno durante l'aggiornamento della bevanda standard");
            }
        }

        public async Task<SingleResponseDTO<bool>> DeleteAsync(int articoloId)
        {
            try
            {
                if (articoloId <= 0)
                    return SingleResponseDTO<bool>.ErrorResponse("ID articolo non valido");

                var bevandaStandard = await _context.BevandaStandard
                    .FirstOrDefaultAsync(bs => bs.ArticoloId == articoloId);

                if (bevandaStandard == null)
                    return SingleResponseDTO<bool>.NotFoundResponse(
                        $"Bevanda standard con ArticoloId: {articoloId} non trovata");

                // ✅ DeleteBehavior.NoAction → dobbiamo eliminare manualmente in ordine inverso
                _context.BevandaStandard.Remove(bevandaStandard);

                // ✅ Elimina anche l'Articolo associato (orphan removal)
                var articolo = await _context.Articolo
                    .FirstOrDefaultAsync(a => a.ArticoloId == articoloId);

                if (articolo != null)
                    _context.Articolo.Remove(articolo);

                await _context.SaveChangesAsync();

                return SingleResponseDTO<bool>.SuccessResponse(true,
                    $"Bevanda standard con ArticoloId: {articoloId} eliminata con successo");
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "Errore DB in DeleteAsync per ArticoloId: {ArticoloId}", articoloId);
                return SingleResponseDTO<bool>.ErrorResponse(
                    "Impossibile eliminare a causa di vincoli referenziali. Assicurati che non ci siano dipendenze");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore in DeleteAsync per ArticoloId: {ArticoloId}", articoloId);
                return SingleResponseDTO<bool>.ErrorResponse("Errore interno durante l'eliminazione della bevanda standard");
            }
        }

        public async Task<SingleResponseDTO<bool>> ExistsAsync(int articoloId)
        {
            try
            {
                if (articoloId <= 0)
                    return SingleResponseDTO<bool>.ErrorResponse("ID non valido");

                var exists = await _context.BevandaStandard
                    .AsNoTracking()
                    .AnyAsync(bs => bs.ArticoloId == articoloId);

                string message = exists
                    ? $"Bevanda standard con ArticoloId {articoloId} esiste"
                    : $"Bevanda standard con ArticoloId {articoloId} non trovata";

                return SingleResponseDTO<bool>.SuccessResponse(exists, message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore in ExistsAsync per ArticoloId: {ArticoloId}", articoloId);
                return SingleResponseDTO<bool>.ErrorResponse("Errore nella verifica dell'esistenza della bevanda standard");
            }
        }

        public async Task<SingleResponseDTO<bool>> ExistsByCombinazioneAsync(int personalizzazioneId, int dimensioneBicchiereId)
        {
            try
            {
                if (personalizzazioneId <= 0 || dimensioneBicchiereId <= 0)
                    return SingleResponseDTO<bool>.ErrorResponse("ID non validi");

                var exists = await _context.BevandaStandard
                    .AsNoTracking()
                    .AnyAsync(bs => bs.PersonalizzazioneId == personalizzazioneId &&
                                   bs.DimensioneBicchiereId == dimensioneBicchiereId);

                string message = exists
                    ? $"Bevanda standard con PersonalizzazioneId: {personalizzazioneId} e DimensioneBicchiereId: {dimensioneBicchiereId} esiste"
                    : $"Bevanda standard con PersonalizzazioneId: {personalizzazioneId} e DimensioneBicchiereId: {dimensioneBicchiereId} non trovata";

                return SingleResponseDTO<bool>.SuccessResponse(exists, message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore in ExistsByCombinazioneAsync per PersonalizzazioneId: {PersonalizzazioneId} e DimensioneBicchiereId: {DimensioneBicchiereId}",
                    personalizzazioneId, dimensioneBicchiereId);
                return SingleResponseDTO<bool>.ErrorResponse("Errore nella verifica dell'esistenza della bevanda standard per combinazione");
            }
        }

        public async Task<SingleResponseDTO<bool>> ExistsByCombinazioneAsync(string personalizzazione, string descrizioneBicchiere)
        {
            try
            {
                if (!SecurityHelper.IsValidInput(personalizzazione) || !SecurityHelper.IsValidInput(descrizioneBicchiere))
                    return SingleResponseDTO<bool>.ErrorResponse("Parametri di ricerca non validi");

                var personalizzazioneNormalizzata = StringHelper.NormalizeSearchTerm(personalizzazione);
                var descrizioneNormalizzata = StringHelper.NormalizeSearchTerm(descrizioneBicchiere);

                var exists = await _context.BevandaStandard
                    .AsNoTracking()
                    .Join(_context.Personalizzazione,
                        bs => bs.PersonalizzazioneId,
                        p => p.PersonalizzazioneId,
                        (bs, p) => new { BevandaStandard = bs, Personalizzazione = p })
                    .Join(_context.DimensioneBicchiere,
                        bp => bp.BevandaStandard.DimensioneBicchiereId,
                        d => d.DimensioneBicchiereId,
                        (bp, d) => new { bp.BevandaStandard, bp.Personalizzazione, Dimensione = d })
                    .AnyAsync(x =>
                        (x.Personalizzazione.Nome != null &&
                         StringHelper.ContainsCaseInsensitive(x.Personalizzazione.Nome, personalizzazioneNormalizzata)) &&
                        (x.Dimensione.Descrizione != null &&
                         StringHelper.ContainsCaseInsensitive(x.Dimensione.Descrizione, descrizioneNormalizzata)));

                string message = exists
                    ? $"Bevanda standard con personalizzazione: '{personalizzazione}' e descrizione bicchiere: '{descrizioneBicchiere}' esiste"
                    : $"Bevanda standard con personalizzazione: '{personalizzazione}' e descrizione bicchiere: '{descrizioneBicchiere}' non trovata";

                return SingleResponseDTO<bool>.SuccessResponse(exists, message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore in ExistsByCombinazioneAsync per personalizzazione: {Personalizzazione} e descrizioneBicchiere: {DescrizioneBicchiere}",
                    personalizzazione, descrizioneBicchiere);
                return SingleResponseDTO<bool>.ErrorResponse("Errore nella verifica dell'esistenza della bevanda standard per combinazione stringa");
            }
        }        

        public async Task<PaginatedResponseDTO<BevandaStandardCardDTO>> GetCardProdottiAsync(int page = 1, int pageSize = 10)
        {
            try
            {
                var (safePage, safePageSize) = SecurityHelper.ValidatePagination(page, pageSize);
                var skip = (safePage - 1) * safePageSize;

                // ✅ Rimosso GroupBy non necessario perché ArticoloId è univoco
                var query = _context.BevandaStandard
                    .AsNoTracking()
                    .Where(bs => bs.SempreDisponibile)
                    .OrderByDescending(bs => bs.Priorita)
                    .ThenBy(bs => bs.ArticoloId);

                var totalCount = await query.CountAsync();

                var bevandeStandard = await query
                    .Skip(skip)
                    .Take(safePageSize)
                    .ToListAsync();

                var result = await MapToCardDTOList(bevandeStandard);

                string message = totalCount switch
                {
                    0 => "Nessuna card prodotto trovata",
                    1 => "Trovata 1 card prodotto",
                    _ => $"Trovate {totalCount} card prodotti"
                };

                return new PaginatedResponseDTO<BevandaStandardCardDTO>
                {
                    Data = result,
                    Page = safePage,
                    PageSize = safePageSize,
                    TotalCount = totalCount,
                    Message = message
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore in GetCardProdottiAsync");
                return new PaginatedResponseDTO<BevandaStandardCardDTO>
                {
                    Data = [],
                    Page = 1,
                    PageSize = pageSize,
                    TotalCount = 0,
                    Message = "Errore nel recupero delle card prodotti"
                };
            }
        }

        public async Task<SingleResponseDTO<BevandaStandardCardDTO>> GetCardProdottoByIdAsync(int articoloId)
        {
            try
            {
                if (articoloId <= 0)
                    return SingleResponseDTO<BevandaStandardCardDTO>.ErrorResponse("ID articolo non valido");

                var bevanda = await _context.BevandaStandard
                    .AsNoTracking()
                    .Where(bs => bs.ArticoloId == articoloId && bs.SempreDisponibile)
                    .FirstOrDefaultAsync();

                if (bevanda == null)
                    return SingleResponseDTO<BevandaStandardCardDTO>.NotFoundResponse(
                        $"Card prodotto con ArticoloId {articoloId} non trovata o non disponibile");

                var cardDto = await MapToCardDTO(bevanda);

                return SingleResponseDTO<BevandaStandardCardDTO>.SuccessResponse(
                    cardDto,
                    $"Card prodotto con ArticoloId {articoloId} trovata");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore in GetCardProdottoByIdAsync per ArticoloId: {ArticoloId}", articoloId);
                return SingleResponseDTO<BevandaStandardCardDTO>.ErrorResponse(
                    "Errore interno nel recupero della card prodotto");
            }
        }

        public async Task<PaginatedResponseDTO<BevandaStandardDTO>> GetPrimoPianoAsync(int page = 1, int pageSize = 10)
        {
            try
            {
                var (safePage, safePageSize) = SecurityHelper.ValidatePagination(page, pageSize);
                var skip = (safePage - 1) * safePageSize;

                // ✅ Vincolo: (!SempreDisponibile && !Disponibile) || (SempreDisponibile)
                // Primo piano: SempreDisponibile=true E Disponibile=true
                var query = _context.BevandaStandard
                    .AsNoTracking()
                    .Where(bs => bs.SempreDisponibile && bs.Disponibile)
                    .OrderByDescending(bs => bs.Priorita)
                    .ThenBy(bs => bs.ArticoloId);

                var totalCount = await query.CountAsync();

                var bevandeStandard = await query
                    .Skip(skip)
                    .Take(safePageSize)
                    .ToListAsync();

                var result = await MapToDTOList(bevandeStandard);

                string message = totalCount switch
                {
                    0 => "Nessuna bevanda in primo piano trovata",
                    1 => "Trovata 1 bevanda in primo piano",
                    _ => $"Trovate {totalCount} bevande in primo piano"
                };

                return new PaginatedResponseDTO<BevandaStandardDTO>
                {
                    Data = result,
                    Page = safePage,
                    PageSize = safePageSize,
                    TotalCount = totalCount,
                    Message = message
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore in GetPrimoPianoAsync");
                return new PaginatedResponseDTO<BevandaStandardDTO>
                {
                    Data = [],
                    Page = 1,
                    PageSize = pageSize,
                    TotalCount = 0,
                    Message = "Errore nel recupero delle bevande in primo piano"
                };
            }
        }

        public async Task<PaginatedResponseDTO<BevandaStandardDTO>> GetSecondoPianoAsync(int page = 1, int pageSize = 10)
        {
            try
            {
                var (safePage, safePageSize) = SecurityHelper.ValidatePagination(page, pageSize);
                var skip = (safePage - 1) * safePageSize;

                // ✅ Secondo piano: SempreDisponibile=true E Disponibile=false
                var query = _context.BevandaStandard
                    .AsNoTracking()
                    .Where(bs => bs.SempreDisponibile && !bs.Disponibile)
                    .OrderByDescending(bs => bs.Priorita)
                    .ThenBy(bs => bs.ArticoloId);

                var totalCount = await query.CountAsync();

                var bevandeStandard = await query
                    .Skip(skip)
                    .Take(safePageSize)
                    .ToListAsync();

                var result = await MapToDTOList(bevandeStandard);

                string message = totalCount switch
                {
                    0 => "Nessuna bevanda in secondo piano trovata",
                    1 => "Trovata 1 bevanda in secondo piano",
                    _ => $"Trovate {totalCount} bevande in secondo piano"
                };

                return new PaginatedResponseDTO<BevandaStandardDTO>
                {
                    Data = result,
                    Page = safePage,
                    PageSize = safePageSize,
                    TotalCount = totalCount,
                    Message = message
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore in GetSecondoPianoAsync");
                return new PaginatedResponseDTO<BevandaStandardDTO>
                {
                    Data = [],
                    Page = 1,
                    PageSize = pageSize,
                    TotalCount = 0,
                    Message = "Errore nel recupero delle bevande in secondo piano"
                };
            }
        }        

        public async Task<PaginatedResponseDTO<BevandaStandardCardDTO>> GetCardProdottiPrimoPianoAsync(int page = 1, int pageSize = 10)
        {
            try
            {
                var (safePage, safePageSize) = SecurityHelper.ValidatePagination(page, pageSize);
                var skip = (safePage - 1) * safePageSize;

                // ✅ Rimosso GroupBy non necessario
                var query = _context.BevandaStandard
                    .AsNoTracking()
                    .Where(bs => bs.SempreDisponibile && bs.Disponibile)
                    .OrderByDescending(bs => bs.Priorita)
                    .ThenBy(bs => bs.ArticoloId);

                var totalCount = await query.CountAsync();

                var bevandeStandard = await query
                    .Skip(skip)
                    .Take(safePageSize)
                    .ToListAsync();

                var result = await MapToCardDTOList(bevandeStandard);

                string message = totalCount switch
                {
                    0 => "Nessuna card prodotto in primo piano trovata",
                    1 => "Trovata 1 card prodotto in primo piano",
                    _ => $"Trovate {totalCount} card prodotti in primo piano"
                };

                return new PaginatedResponseDTO<BevandaStandardCardDTO>
                {
                    Data = result,
                    Page = safePage,
                    PageSize = safePageSize,
                    TotalCount = totalCount,
                    Message = message
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore in GetCardProdottiPrimoPianoAsync");
                return new PaginatedResponseDTO<BevandaStandardCardDTO>
                {
                    Data = [],
                    Page = 1,
                    PageSize = pageSize,
                    TotalCount = 0,
                    Message = "Errore nel recupero delle card prodotti in primo piano"
                };
            }
        }

        public async Task<SingleResponseDTO<int>> CountAsync()
        {
            try
            {
                var totalCount = await _context.BevandaStandard
                    .AsNoTracking()
                    .CountAsync();

                string message = totalCount switch
                {
                    0 => "Nessuna bevanda standard presente",
                    1 => "C'è 1 bevanda standard in totale",
                    _ => $"Ci sono {totalCount} bevande standard in totale"
                };                    

                return SingleResponseDTO<int>.SuccessResponse(totalCount, message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore in CountAsync");
                return SingleResponseDTO<int>.ErrorResponse("Errore nel conteggio delle bevande standard");
            }
        }

        public async Task<SingleResponseDTO<int>> CountPrimoPianoAsync()
        {
            try
            {
                var totalCount = await _context.BevandaStandard
                    .AsNoTracking()
                    .Where(bs => bs.SempreDisponibile && bs.Disponibile)
                    .CountAsync();

                string message = totalCount switch
                {
                    0 => "Nessuna bevanda standard presente in primo piano",
                    1 => "C'è 1 bevanda standard presente in primo piano",
                    _ => $"Ci sono {totalCount} bevande standard presenti in primo piano"
                };

                return SingleResponseDTO<int>.SuccessResponse(totalCount, message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore in CountPrimoPianoAsync");
                return SingleResponseDTO<int>.ErrorResponse("Errore nel conteggio delle bevande standard in primo piano");
            }
        }

        public async Task<SingleResponseDTO<int>> CountSecondoPianoAsync()
        {
            try
            {
                var totalCount = await _context.BevandaStandard
                    .AsNoTracking()
                    .Where(bs => bs.SempreDisponibile && !bs.Disponibile)
                    .CountAsync();

                string message = totalCount switch
                {
                    0 => "Nessuna bevanda standard presente in secondo piano",
                    1 => "C'è 1 bevanda standard presente in secondo piano",
                    _ => $"Ci sono {totalCount} bevande standard presenti in secondo piano"
                };

                return SingleResponseDTO<int>.SuccessResponse(totalCount, message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore in CountSecondoPianoAsync");
                return SingleResponseDTO<int>.ErrorResponse("Errore nel conteggio delle bevande standard in secondo piano");
            }
        }

        public async Task<SingleResponseDTO<int>> CountDisponibiliAsync()
        {
            try
            {
                var totalCount = await _context.BevandaStandard
                    .AsNoTracking()
                    .Where(bs => bs.SempreDisponibile)
                    .CountAsync();

                string message = totalCount switch
                {
                    0 => "Nessuna bevanda standard è sempre disponibile",
                    1 => "C'è 1 bevanda standard sempre disponibile",
                    _ => $"Ci sono {totalCount} bevande standard sempre disponibili"
                };

                return SingleResponseDTO<int>.SuccessResponse(totalCount, message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore in CountDisponibiliAsync");
                return SingleResponseDTO<int>.ErrorResponse("Errore nel conteggio delle bevande standard sempre disponibili");
            }
        }

        public async Task<SingleResponseDTO<int>> CountNonDisponibiliAsync()
        {
            try
            {
                var totalCount = await _context.BevandaStandard
                    .AsNoTracking()
                    .Where(bs => !bs.SempreDisponibile)
                    .CountAsync();

                string message = totalCount switch
                {
                    0 => "Tutte le bevande standard sono sempre disponibili",
                    1 => "C'è 1 bevanda standard non sempre disponibile",
                    _ => $"Ci sono {totalCount} bevande standard non sempre disponibili"
                };

                return SingleResponseDTO<int>.SuccessResponse(totalCount, message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore in CountNonDisponibiliAsync");
                return SingleResponseDTO<int>.ErrorResponse("Errore nel conteggio delle bevande standard non sempre disponibili");
            }
        }
    }
}