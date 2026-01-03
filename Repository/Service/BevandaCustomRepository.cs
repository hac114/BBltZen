using BBltZen;
using DTO;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Repository.Helper;
using Repository.Interface;

namespace Repository.Service
{
    public class BevandaCustomRepository(BubbleTeaContext context, ILogger<BevandaCustomRepository> logger) : IBevandaCustomRepository
    {
        private readonly BubbleTeaContext _context = context;
        private readonly ILogger<BevandaCustomRepository> _logger = logger;

        // METODI PRIVATI DI SUPPORTO
        private static bool IsUniqueConstraintViolation(DbUpdateException dbEx)
        {
            var sqlException = dbEx.InnerException as Microsoft.Data.SqlClient.SqlException
                                ?? dbEx.InnerException?.InnerException as Microsoft.Data.SqlClient.SqlException;

            if (sqlException != null)
            {
                return sqlException.Number == 2627 || sqlException.Number == 2601;
            }

            return false;
        }

        private async Task<decimal> GetAliquotaIvaAsync(int taxRateId = 1)
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

        private async Task<List<string>> GetIngredientiByPersCustomIdAsync(int persCustomId)
        {
            var ingredienti = await _context.IngredientiPersonalizzazione
                .AsNoTracking()
                .Where(ip => ip.PersCustomId == persCustomId)
                .Join(_context.Ingrediente,
                    ip => ip.IngredienteId,
                    i => i.IngredienteId,
                    (ip, i) => i.Ingrediente1)
                .Where(nome => !string.IsNullOrEmpty(nome))
                .Distinct()
                .ToListAsync();

            return ingredienti;
        }

        private async Task<PrezzoDimensioneDTO> CalcolaPrezzoPerDimensioneAsync(BevandaCustom bevandaCustom)
        {
            var personalizzazioneCustom = await _context.PersonalizzazioneCustom
                .AsNoTracking()
                .FirstOrDefaultAsync(pc => pc.PersCustomId == bevandaCustom.PersCustomId) ?? throw new InvalidOperationException("Personalizzazione custom non trovata");
            var dimensione = await _context.DimensioneBicchiere
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.DimensioneBicchiereId == personalizzazioneCustom.DimensioneBicchiereId) ?? throw new InvalidOperationException("Dimensione bicchiere non trovata");
            var taxRateId = 1;
            var aliquotaIva = await GetAliquotaIvaAsync(taxRateId);
            var prezzoBase = bevandaCustom.Prezzo * dimensione.Moltiplicatore;
            var prezzoIva = CalcolaIva(prezzoBase, aliquotaIva);
            var prezzoTotale = prezzoBase + prezzoIva;

            return new PrezzoDimensioneDTO
            {
                DimensioneBicchiereId = dimensione.DimensioneBicchiereId,
                Sigla = dimensione.Sigla,
                Descrizione = $"{dimensione.Descrizione} {dimensione.Capienza}ml",
                PrezzoNetto = Math.Round(prezzoBase, 2),
                PrezzoIva = Math.Round(prezzoIva, 2),
                PrezzoTotale = Math.Round(prezzoTotale, 2),
                AliquotaIva = aliquotaIva
            };
        }

        private static BevandaCustomDTO MapToDTO(BevandaCustom bevandaCustom)
        {
            return new BevandaCustomDTO
            {
                ArticoloId = bevandaCustom.ArticoloId,
                PersCustomId = bevandaCustom.PersCustomId,
                Prezzo = bevandaCustom.Prezzo,
                DataCreazione = bevandaCustom.DataCreazione,
                DataAggiornamento = bevandaCustom.DataAggiornamento
            };
        }

        private static List<BevandaCustomDTO> MapToDTOList(List<BevandaCustom> bevandeCustom)
        {
            var result = new List<BevandaCustomDTO>();

            foreach (var bc in bevandeCustom)
            {
                result.Add(new BevandaCustomDTO
                {
                    ArticoloId = bc.ArticoloId,
                    PersCustomId = bc.PersCustomId,
                    Prezzo = bc.Prezzo,
                    DataCreazione = bc.DataCreazione,
                    DataAggiornamento = bc.DataAggiornamento
                });
            }

            return result;
        }

        private async Task<BevandaCustomCardDTO> MapToCardDTO(BevandaCustom bevandaCustom)
        {
            var personalizzazioneCustom = await _context.PersonalizzazioneCustom
                .AsNoTracking()
                .FirstOrDefaultAsync(pc => pc.PersCustomId == bevandaCustom.PersCustomId);

            var ingredienti = await GetIngredientiByPersCustomIdAsync(bevandaCustom.PersCustomId);
            var prezzoDimensione = await CalcolaPrezzoPerDimensioneAsync(bevandaCustom);

            return new BevandaCustomCardDTO
            {
                ArticoloId = bevandaCustom.ArticoloId,
                PersCustomId = bevandaCustom.PersCustomId,
                NomePersonalizzazione = personalizzazioneCustom?.Nome ?? "Bevanda Custom",
                PrezzoDimensione = prezzoDimensione,
                Ingredienti = ingredienti
            };
        }

        private async Task<List<BevandaCustomCardDTO>> MapToCardDTOList(List<BevandaCustom> bevandeCustom)
        {
            var persCustomIds = bevandeCustom.Select(bc => bc.PersCustomId).Distinct().ToList();

            var personalizzazioni = await _context.PersonalizzazioneCustom
                .Where(pc => persCustomIds.Contains(pc.PersCustomId))
                .ToDictionaryAsync(pc => pc.PersCustomId);

            var result = new List<BevandaCustomCardDTO>();

            foreach (var bc in bevandeCustom)
            {
                personalizzazioni.TryGetValue(bc.PersCustomId, out var personalizzazioneCustom);

                var ingredienti = await GetIngredientiByPersCustomIdAsync(bc.PersCustomId);
                var prezzoDimensione = await CalcolaPrezzoPerDimensioneAsync(bc);

                result.Add(new BevandaCustomCardDTO
                {
                    ArticoloId = bc.ArticoloId,
                    PersCustomId = bc.PersCustomId,
                    NomePersonalizzazione = personalizzazioneCustom?.Nome ?? "Bevanda Custom",
                    PrezzoDimensione = prezzoDimensione,
                    Ingredienti = ingredienti
                });
            }

            return result;
        }

        // IMPLEMENTAZIONE METODI INTERFACCIA (nell'ordine specificato)
        public async Task<PaginatedResponseDTO<BevandaCustomDTO>> GetAllAsync(int page = 1, int pageSize = 10)
        {
            try
            {
                var (safePage, safePageSize) = SecurityHelper.ValidatePagination(page, pageSize);
                var skip = (safePage - 1) * safePageSize;

                var query = _context.BevandaCustom
                    .AsNoTracking()
                    .OrderByDescending(bc => bc.DataCreazione);

                var totalCount = await query.CountAsync();

                var bevandeCustom = await query
                    .Skip(skip)
                    .Take(safePageSize)
                    .ToListAsync();

                var result = MapToDTOList(bevandeCustom);

                return new PaginatedResponseDTO<BevandaCustomDTO>
                {
                    Data = result,
                    Page = safePage,
                    PageSize = safePageSize,
                    TotalCount = totalCount,
                    Message = totalCount switch
                    {
                        0 => "Nessuna bevanda custom trovata",
                        1 => "Trovata 1 bevanda custom",
                        _ => $"Trovate {totalCount} bevande custom"
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore in GetAllAsync");
                return new PaginatedResponseDTO<BevandaCustomDTO>
                {
                    Data = [],
                    Page = 1,
                    PageSize = pageSize,
                    TotalCount = 0,
                    Message = "Errore nel recupero delle bevande custom"
                };
            }
        }

        public async Task<BevandaCustomDTO> GetByIdAsync(int articoloId)
        {
            try
            {
                if (articoloId <= 0)
                    throw new ArgumentException("ID articolo non valido");

                var bevandaCustom = await _context.BevandaCustom
                    .AsNoTracking()
                    .FirstOrDefaultAsync(bc => bc.ArticoloId == articoloId);

                if (bevandaCustom == null)
                    throw new KeyNotFoundException($"Bevanda custom con ArticoloId {articoloId} non trovata");

                return MapToDTO(bevandaCustom);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore in GetByIdAsync per ArticoloId: {ArticoloId}", articoloId);
                throw;
            }
        }

        public async Task<SingleResponseDTO<BevandaCustomDTO>> GetByPersCustomIdAsync(int persCustomId)
        {
            try
            {
                if (persCustomId <= 0)
                    return SingleResponseDTO<BevandaCustomDTO>.ErrorResponse("ID personalizzazione custom non valido");

                var bevandaCustom = await _context.BevandaCustom
                    .AsNoTracking()
                    .FirstOrDefaultAsync(bc => bc.PersCustomId == persCustomId);

                if (bevandaCustom == null)
                    return SingleResponseDTO<BevandaCustomDTO>.NotFoundResponse(
                        $"Bevanda custom con PersCustomId {persCustomId} non trovata");

                var dto = MapToDTO(bevandaCustom);

                return SingleResponseDTO<BevandaCustomDTO>.SuccessResponse(
                    dto,
                    $"Bevanda custom con PersCustomId {persCustomId} trovata");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore in GetByPersCustomIdAsync per PersCustomId: {PersCustomId}", persCustomId);
                return SingleResponseDTO<BevandaCustomDTO>.ErrorResponse("Errore interno nel recupero della bevanda custom");
            }
        }

        public async Task<PaginatedResponseDTO<BevandaCustomDTO>> GetAllOrderedByDimensioneAsync(int page = 1, int pageSize = 10)
        {
            try
            {
                var (safePage, safePageSize) = SecurityHelper.ValidatePagination(page, pageSize);
                var skip = (safePage - 1) * safePageSize;

                var query = _context.BevandaCustom
                    .AsNoTracking()
                    .Join(_context.PersonalizzazioneCustom,
                        bc => bc.PersCustomId,
                        pc => pc.PersCustomId,
                        (bc, pc) => new { BevandaCustom = bc, DimensioneId = pc.DimensioneBicchiereId })
                    .OrderBy(x => x.DimensioneId)
                    .ThenByDescending(x => x.BevandaCustom.DataCreazione)
                    .Select(x => x.BevandaCustom);

                var totalCount = await query.CountAsync();

                var bevandeCustom = await query
                    .Skip(skip)
                    .Take(safePageSize)
                    .ToListAsync();

                var result = MapToDTOList(bevandeCustom);

                string message = totalCount switch
                {
                    0 => "Nessuna bevanda custom trovata per dimensione bicchiere",
                    1 => "Trovata 1 bevanda custom per dimensione bicchiere",
                    _ => $"Trovate {totalCount} bevande custom per dimensione bicchiere"
                };

                return new PaginatedResponseDTO<BevandaCustomDTO>
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
                _logger.LogError(ex, "Errore in GetAllOrderedByDimensioneAsync");
                return new PaginatedResponseDTO<BevandaCustomDTO>
                {
                    Data = [],
                    Page = 1,
                    PageSize = pageSize,
                    TotalCount = 0,
                    Message = "Errore nel recupero delle bevande custom per dimensione bicchiere"
                };
            }
        }

        public async Task<PaginatedResponseDTO<BevandaCustomDTO>> GetAllOrderedByPersonalizzazioneAsync(int page = 1, int pageSize = 10)
        {
            try
            {
                var (safePage, safePageSize) = SecurityHelper.ValidatePagination(page, pageSize);
                var skip = (safePage - 1) * safePageSize;

                var query = _context.BevandaCustom
                    .AsNoTracking()
                    .OrderBy(bc => bc.PersCustomId)
                    .ThenByDescending(bc => bc.DataCreazione);

                var totalCount = await query.CountAsync();

                var bevandeCustom = await query
                    .Skip(skip)
                    .Take(safePageSize)
                    .ToListAsync();

                var result = MapToDTOList(bevandeCustom);

                string message = totalCount switch
                {
                    0 => "Nessuna bevanda custom trovata per personalizzazione",
                    1 => "Trovata 1 bevanda custom per personalizzazione",
                    _ => $"Trovate {totalCount} bevande custom per personalizzazione"
                };

                return new PaginatedResponseDTO<BevandaCustomDTO>
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
                _logger.LogError(ex, "Errore in GetAllOrderedByPersonalizzazioneAsync");
                return new PaginatedResponseDTO<BevandaCustomDTO>
                {
                    Data = [],
                    Page = 1,
                    PageSize = pageSize,
                    TotalCount = 0,
                    Message = "Errore nel recupero delle bevande custom per personalizzazione"
                };
            }
        }

        public async Task<SingleResponseDTO<BevandaCustomDTO>> AddAsync(BevandaCustomDTO bevandaCustomDto)
        {
            try
            {
                ArgumentNullException.ThrowIfNull(bevandaCustomDto);

                if (bevandaCustomDto.PersCustomId <= 0)
                    return SingleResponseDTO<BevandaCustomDTO>.ErrorResponse("Il parametro PersCustomId è obbligatorio");

                if (bevandaCustomDto.Prezzo < 0.01m || bevandaCustomDto.Prezzo > 99.99m)
                    return SingleResponseDTO<BevandaCustomDTO>.ErrorResponse("Il prezzo deve essere tra 0.01 e 99.99");

                if (await _context.PersonalizzazioneCustom
                    .AsNoTracking()
                    .AnyAsync(pc => pc.PersCustomId == bevandaCustomDto.PersCustomId) == false)
                    return SingleResponseDTO<BevandaCustomDTO>.ErrorResponse("Personalizzazione custom non trovata");

                if (await ExistsByPersCustomIdInternalAsync(bevandaCustomDto.PersCustomId))
                    return SingleResponseDTO<BevandaCustomDTO>.ErrorResponse(
                        $"Esiste già una bevanda custom per PersCustomId: {bevandaCustomDto.PersCustomId}");

                var articolo = new Articolo
                {
                    Tipo = "BEVANDA_CUSTOM",
                    DataCreazione = DateTime.Now,
                    DataAggiornamento = DateTime.Now
                };

                _context.Articolo.Add(articolo);
                await _context.SaveChangesAsync();

                var bevandaCustom = new BevandaCustom
                {
                    ArticoloId = articolo.ArticoloId,
                    PersCustomId = bevandaCustomDto.PersCustomId,
                    Prezzo = Math.Round(bevandaCustomDto.Prezzo, 2),
                    DataCreazione = DateTime.Now,
                    DataAggiornamento = DateTime.Now
                };

                _context.BevandaCustom.Add(bevandaCustom);
                await _context.SaveChangesAsync();

                bevandaCustomDto.ArticoloId = bevandaCustom.ArticoloId;
                bevandaCustomDto.DataCreazione = bevandaCustom.DataCreazione;
                bevandaCustomDto.DataAggiornamento = bevandaCustom.DataAggiornamento;

                return SingleResponseDTO<BevandaCustomDTO>.SuccessResponse(
                    bevandaCustomDto,
                    $"Bevanda custom creata con successo (ArticoloId: {bevandaCustomDto.ArticoloId})");
            }
            catch (DbUpdateException dbEx) when (IsUniqueConstraintViolation(dbEx))
            {
                _logger.LogError(dbEx, "Violazione vincolo UNIQUE in AddAsync");
                return SingleResponseDTO<BevandaCustomDTO>.ErrorResponse(
                    $"Esiste già una bevanda custom per PersCustomId: {bevandaCustomDto.PersCustomId}");
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "Errore DB in AddAsync - Vincoli violati");
                return SingleResponseDTO<BevandaCustomDTO>.ErrorResponse(
                    "Errore di validazione del database. Verifica i vincoli (prezzo, FK)");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore in AddAsync");
                return SingleResponseDTO<BevandaCustomDTO>.ErrorResponse(
                    "Errore interno durante la creazione della bevanda custom");
            }
        }

        private async Task<bool> ExistsByPersCustomIdInternalAsync(int persCustomId, int excludeArticoloId = 0)
        {
            var query = _context.BevandaCustom
                .AsNoTracking()
                .Where(bc => bc.PersCustomId == persCustomId);

            if (excludeArticoloId > 0)
                query = query.Where(bc => bc.ArticoloId != excludeArticoloId);

            return await query.AnyAsync();
        }

        public async Task<SingleResponseDTO<bool>> UpdateAsync(BevandaCustomDTO bevandaCustomDto)
        {
            try
            {
                ArgumentNullException.ThrowIfNull(bevandaCustomDto);

                if (bevandaCustomDto.ArticoloId <= 0)
                    return SingleResponseDTO<bool>.ErrorResponse("ID bevanda custom obbligatorio");

                if (bevandaCustomDto.Prezzo < 0.01m || bevandaCustomDto.Prezzo > 99.99m)
                    return SingleResponseDTO<bool>.ErrorResponse("Il prezzo deve essere tra 0.01 e 99.99");

                var bevandaCustom = await _context.BevandaCustom
                    .FirstOrDefaultAsync(bc => bc.ArticoloId == bevandaCustomDto.ArticoloId);

                if (bevandaCustom == null)
                    return SingleResponseDTO<bool>.NotFoundResponse(
                        $"Bevanda custom con ArticoloId {bevandaCustomDto.ArticoloId} non trovata");

                bool cambiaPersCustomId = bevandaCustom.PersCustomId != bevandaCustomDto.PersCustomId;

                if (cambiaPersCustomId)
                {
                    if (bevandaCustomDto.PersCustomId <= 0)
                        return SingleResponseDTO<bool>.ErrorResponse("Il parametro PersCustomId è obbligatorio");

                    if (await _context.PersonalizzazioneCustom
                        .AsNoTracking()
                        .AnyAsync(pc => pc.PersCustomId == bevandaCustomDto.PersCustomId) == false)
                        return SingleResponseDTO<bool>.ErrorResponse("Personalizzazione custom non trovata");

                    if (await ExistsByPersCustomIdInternalAsync(bevandaCustomDto.PersCustomId, bevandaCustomDto.ArticoloId))
                        return SingleResponseDTO<bool>.ErrorResponse(
                            $"Esiste già un'altra bevanda custom per PersCustomId: {bevandaCustomDto.PersCustomId}");
                }

                bool hasChanges = false;

                if (cambiaPersCustomId)
                {
                    bevandaCustom.PersCustomId = bevandaCustomDto.PersCustomId;
                    hasChanges = true;
                }

                var prezzoArrotondato = Math.Round(bevandaCustomDto.Prezzo, 2);
                if (bevandaCustom.Prezzo != prezzoArrotondato)
                {
                    bevandaCustom.Prezzo = prezzoArrotondato;
                    hasChanges = true;
                }

                if (hasChanges)
                {
                    bevandaCustom.DataAggiornamento = DateTime.Now;
                    await _context.SaveChangesAsync();
                    return SingleResponseDTO<bool>.SuccessResponse(true,
                        $"Bevanda custom con ArticoloId: {bevandaCustom.ArticoloId} aggiornata con successo");
                }
                else
                {
                    return SingleResponseDTO<bool>.SuccessResponse(false,
                        $"Nessuna modifica necessaria per bevanda custom con ArticoloId: {bevandaCustom.ArticoloId}");
                }
            }
            catch (DbUpdateException dbEx) when (IsUniqueConstraintViolation(dbEx))
            {
                _logger.LogError(dbEx, "Violazione vincolo UNIQUE in UpdateAsync per ArticoloId: {ArticoloId}",
                    bevandaCustomDto?.ArticoloId);
                return SingleResponseDTO<bool>.ErrorResponse(
                    $"Esiste già un'altra bevanda custom per PersCustomId: {bevandaCustomDto?.PersCustomId}");
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "Errore DB in UpdateAsync per ArticoloId: {ArticoloId}", bevandaCustomDto?.ArticoloId);
                return SingleResponseDTO<bool>.ErrorResponse("Errore di validazione del database durante l'aggiornamento");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore in UpdateAsync per ArticoloId: {ArticoloId}", bevandaCustomDto?.ArticoloId);
                return SingleResponseDTO<bool>.ErrorResponse("Errore interno durante l'aggiornamento della bevanda custom");
            }
        }

        public async Task<SingleResponseDTO<bool>> DeleteAsync(int articoloId)
        {
            try
            {
                if (articoloId <= 0)
                    return SingleResponseDTO<bool>.ErrorResponse("ID articolo non valido");

                var bevandaCustom = await _context.BevandaCustom
                    .FirstOrDefaultAsync(bc => bc.ArticoloId == articoloId);

                if (bevandaCustom == null)
                    return SingleResponseDTO<bool>.NotFoundResponse(
                        $"Bevanda custom con ArticoloId: {articoloId} non trovata");

                _context.BevandaCustom.Remove(bevandaCustom);

                var articolo = await _context.Articolo
                    .FirstOrDefaultAsync(a => a.ArticoloId == articoloId);

                if (articolo != null)
                    _context.Articolo.Remove(articolo);

                await _context.SaveChangesAsync();

                return SingleResponseDTO<bool>.SuccessResponse(true,
                    $"Bevanda custom con ArticoloId: {articoloId} eliminata con successo");
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
                return SingleResponseDTO<bool>.ErrorResponse("Errore interno durante l'eliminazione della bevanda custom");
            }
        }

        public async Task<SingleResponseDTO<bool>> ExistsAsync(int articoloId)
        {
            try
            {
                if (articoloId <= 0)
                    return SingleResponseDTO<bool>.ErrorResponse("ID non valido");

                var exists = await _context.BevandaCustom
                    .AsNoTracking()
                    .AnyAsync(bc => bc.ArticoloId == articoloId);

                string message = exists
                    ? $"Bevanda custom con ArticoloId {articoloId} esiste"
                    : $"Bevanda custom con ArticoloId {articoloId} non trovata";

                return SingleResponseDTO<bool>.SuccessResponse(exists, message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore in ExistsAsync per ArticoloId: {ArticoloId}", articoloId);
                return SingleResponseDTO<bool>.ErrorResponse("Errore nella verifica dell'esistenza della bevanda custom");
            }
        }

        public async Task<SingleResponseDTO<bool>> ExistsByPersCustomIdAsync(int persCustomId)
        {
            try
            {
                if (persCustomId <= 0)
                    return SingleResponseDTO<bool>.ErrorResponse("ID personalizzazione custom non valido");

                var exists = await ExistsByPersCustomIdInternalAsync(persCustomId);

                string message = exists
                    ? $"Bevanda custom per PersCustomId {persCustomId} esiste"
                    : $"Bevanda custom per PersCustomId {persCustomId} non trovata";

                return SingleResponseDTO<bool>.SuccessResponse(exists, message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore in ExistsByPersCustomIdAsync per PersCustomId: {PersCustomId}", persCustomId);
                return SingleResponseDTO<bool>.ErrorResponse("Errore nella verifica dell'esistenza della bevanda custom");
            }
        }        

        public async Task<PaginatedResponseDTO<BevandaCustomCardDTO>> GetCardProdottiAsync(int page = 1, int pageSize = 10)
        {
            try
            {
                var (safePage, safePageSize) = SecurityHelper.ValidatePagination(page, pageSize);
                var skip = (safePage - 1) * safePageSize;

                var query = _context.BevandaCustom
                    .AsNoTracking()
                    .OrderByDescending(bc => bc.DataCreazione);

                var totalCount = await query.CountAsync();

                var bevandeCustom = await query
                    .Skip(skip)
                    .Take(safePageSize)
                    .ToListAsync();

                var result = await MapToCardDTOList(bevandeCustom);

                string message = totalCount switch
                {
                    0 => "Nessuna card prodotto custom trovata",
                    1 => "Trovata 1 card prodotto custom",
                    _ => $"Trovate {totalCount} card prodotti custom"
                };

                return new PaginatedResponseDTO<BevandaCustomCardDTO>
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
                return new PaginatedResponseDTO<BevandaCustomCardDTO>
                {
                    Data = [],
                    Page = 1,
                    PageSize = pageSize,
                    TotalCount = 0,
                    Message = "Errore nel recupero delle card prodotti custom"
                };
            }
        }

        public async Task<SingleResponseDTO<BevandaCustomCardDTO>> GetCardProdottoByIdAsync(int articoloId)
        {
            try
            {
                if (articoloId <= 0)
                    return SingleResponseDTO<BevandaCustomCardDTO>.ErrorResponse("ID articolo non valido");

                var bevanda = await _context.BevandaCustom
                    .AsNoTracking()
                    .FirstOrDefaultAsync(bc => bc.ArticoloId == articoloId);

                if (bevanda == null)
                    return SingleResponseDTO<BevandaCustomCardDTO>.NotFoundResponse(
                        $"Card prodotto custom con ArticoloId {articoloId} non trovata");

                var cardDto = await MapToCardDTO(bevanda);

                return SingleResponseDTO<BevandaCustomCardDTO>.SuccessResponse(
                    cardDto,
                    $"Card prodotto custom con ArticoloId {articoloId} trovata");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore in GetCardProdottoByIdAsync per ArticoloId: {ArticoloId}", articoloId);
                return SingleResponseDTO<BevandaCustomCardDTO>.ErrorResponse(
                    "Errore interno nel recupero della card prodotto custom");
            }
        }

        public async Task<PaginatedResponseDTO<BevandaCustomCardDTO>> GetCardPersonalizzazioneAsync(string nomePersonalizzazione, int page = 1, int pageSize = 10)
        {
            try
            {
                if (!SecurityHelper.IsValidInput(nomePersonalizzazione, 100))
                    return new PaginatedResponseDTO<BevandaCustomCardDTO>
                    {
                        Data = [],
                        Page = 1,
                        PageSize = pageSize,
                        TotalCount = 0,
                        Message = "Nome personalizzazione non valido"
                    };

                var (safePage, safePageSize) = SecurityHelper.ValidatePagination(page, pageSize);
                var skip = (safePage - 1) * safePageSize;

                var normalizedSearch = StringHelper.NormalizeSearchTerm(nomePersonalizzazione);

                var persCustomIds = await _context.PersonalizzazioneCustom
                    .AsNoTracking()
                    .Where(pc => StringHelper.ContainsCaseInsensitive(pc.Nome, normalizedSearch))
                    .Select(pc => pc.PersCustomId)
                    .ToListAsync();

                if (persCustomIds.Count == 0)
                    return new PaginatedResponseDTO<BevandaCustomCardDTO>
                    {
                        Data = [],
                        Page = safePage,
                        PageSize = safePageSize,
                        TotalCount = 0,
                        Message = $"Nessuna personalizzazione trovata con nome: {nomePersonalizzazione}"
                    };

                var query = _context.BevandaCustom
                    .AsNoTracking()
                    .Where(bc => persCustomIds.Contains(bc.PersCustomId))
                    .OrderByDescending(bc => bc.DataCreazione);

                var totalCount = await query.CountAsync();

                var bevandeCustom = await query
                    .Skip(skip)
                    .Take(safePageSize)
                    .ToListAsync();

                var result = await MapToCardDTOList(bevandeCustom);

                string message = totalCount switch
                {
                    0 => $"Nessuna card prodotto custom trovata per personalizzazione: {nomePersonalizzazione}",
                    1 => $"Trovata 1 card prodotto custom per personalizzazione: {nomePersonalizzazione}",
                    _ => $"Trovate {totalCount} card prodotti custom per personalizzazione: {nomePersonalizzazione}"
                };

                return new PaginatedResponseDTO<BevandaCustomCardDTO>
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
                _logger.LogError(ex, "Errore in GetCardPersonalizzazioneAsync per nome: {Nome}", nomePersonalizzazione);
                return new PaginatedResponseDTO<BevandaCustomCardDTO>
                {
                    Data = [],
                    Page = 1,
                    PageSize = pageSize,
                    TotalCount = 0,
                    Message = "Errore nel recupero delle card per personalizzazione"
                };
            }
        }

        public async Task<PaginatedResponseDTO<BevandaCustomCardDTO>> GetCardDimensioneBicchiereAsync(string descrizionBicchiere, int page = 1, int pageSize = 10)
        {
            try
            {
                if (!SecurityHelper.IsValidInput(descrizionBicchiere, 100))
                    return new PaginatedResponseDTO<BevandaCustomCardDTO>
                    {
                        Data = [],
                        Page = 1,
                        PageSize = pageSize,
                        TotalCount = 0,
                        Message = "Descrizione bicchiere non valida"
                    };

                var (safePage, safePageSize) = SecurityHelper.ValidatePagination(page, pageSize);
                var skip = (safePage - 1) * safePageSize;

                var normalizedSearch = StringHelper.NormalizeSearchTerm(descrizionBicchiere);

                var dimensioneIds = await _context.DimensioneBicchiere
                    .AsNoTracking()
                    .Where(d => StringHelper.ContainsCaseInsensitive(d.Descrizione, normalizedSearch))
                    .Select(d => d.DimensioneBicchiereId)
                    .ToListAsync();

                if (dimensioneIds.Count == 0)
                    return new PaginatedResponseDTO<BevandaCustomCardDTO>
                    {
                        Data = [],
                        Page = safePage,
                        PageSize = safePageSize,
                        TotalCount = 0,
                        Message = $"Nessuna dimensione trovata con descrizione: {descrizionBicchiere}"
                    };

                var persCustomIds = await _context.PersonalizzazioneCustom
                    .AsNoTracking()
                    .Where(pc => dimensioneIds.Contains(pc.DimensioneBicchiereId))
                    .Select(pc => pc.PersCustomId)
                    .ToListAsync();

                var query = _context.BevandaCustom
                    .AsNoTracking()
                    .Where(bc => persCustomIds.Contains(bc.PersCustomId))
                    .OrderByDescending(bc => bc.DataCreazione);

                var totalCount = await query.CountAsync();

                var bevandeCustom = await query
                    .Skip(skip)
                    .Take(safePageSize)
                    .ToListAsync();

                var result = await MapToCardDTOList(bevandeCustom);

                string message = totalCount switch
                {
                    0 => $"Nessuna card prodotto custom trovata per dimensione: {descrizionBicchiere}",
                    1 => $"Trovata 1 card prodotto custom per dimensione: {descrizionBicchiere}",
                    _ => $"Trovate {totalCount} card prodotti custom per dimensione: {descrizionBicchiere}"
                };

                return new PaginatedResponseDTO<BevandaCustomCardDTO>
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
                _logger.LogError(ex, "Errore in GetCardDimensioneBicchiereAsync per descrizione: {Descrizione}", descrizionBicchiere);
                return new PaginatedResponseDTO<BevandaCustomCardDTO>
                {
                    Data = [],
                    Page = 1,
                    PageSize = pageSize,
                    TotalCount = 0,
                    Message = "Errore nel recupero delle card per dimensione bicchiere"
                };
            }
        }

        public async Task<SingleResponseDTO<int>> CountAsync()
        {
            try
            {
                var count = await _context.BevandaCustom
                    .AsNoTracking()
                    .CountAsync();

                return SingleResponseDTO<int>.SuccessResponse(count, 
                    count switch
                    {
                        0 => "Nessuna bevanda custom presente",
                        1 => "1 bevanda custom presente",
                        _ => $"{count} bevande custom presenti"
                    });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore in CountAsync");
                return SingleResponseDTO<int>.ErrorResponse("Errore nel conteggio delle bevande custom");
            }
        }

        public async Task<SingleResponseDTO<int>> CountDimensioneBicchiereAsync(string descrizionBicchiere)
        {
            try
            {
                if (!SecurityHelper.IsValidInput(descrizionBicchiere, 100))
                    return SingleResponseDTO<int>.ErrorResponse("Descrizione bicchiere non valida");

                var normalizedSearch = StringHelper.NormalizeSearchTerm(descrizionBicchiere);

                var dimensioneIds = await _context.DimensioneBicchiere
                    .AsNoTracking()
                    .Where(d => StringHelper.ContainsCaseInsensitive(d.Descrizione, normalizedSearch))
                    .Select(d => d.DimensioneBicchiereId)
                    .ToListAsync();

                if (dimensioneIds.Count == 0)
                    return SingleResponseDTO<int>.SuccessResponse(0, 
                        $"Nessuna dimensione trovata con descrizione: {descrizionBicchiere}");

                var persCustomIds = await _context.PersonalizzazioneCustom
                    .AsNoTracking()
                    .Where(pc => dimensioneIds.Contains(pc.DimensioneBicchiereId))
                    .Select(pc => pc.PersCustomId)
                    .ToListAsync();

                var count = await _context.BevandaCustom
                    .AsNoTracking()
                    .CountAsync(bc => persCustomIds.Contains(bc.PersCustomId));

                return SingleResponseDTO<int>.SuccessResponse(count,
                    count switch
                    {
                        0 => $"Nessuna bevanda custom trovata per dimensione: {descrizionBicchiere}",
                        1 => $"1 bevanda custom trovata per dimensione: {descrizionBicchiere}",
                        _ => $"{count} bevande custom trovate per dimensione: {descrizionBicchiere}"
                    });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore in CountDimensioneBicchiereAsync per descrizione: {Descrizione}", descrizionBicchiere);
                return SingleResponseDTO<int>.ErrorResponse("Errore nel conteggio per dimensione bicchiere");
            }
        }

        public async Task<SingleResponseDTO<int>> CountPersonalizzazioneAsync(string nomePersonalizzazione)
        {
            try
            {
                if (!SecurityHelper.IsValidInput(nomePersonalizzazione, 100))
                    return SingleResponseDTO<int>.ErrorResponse("Nome personalizzazione non valido");

                var normalizedSearch = StringHelper.NormalizeSearchTerm(nomePersonalizzazione);

                var persCustomIds = await _context.PersonalizzazioneCustom
                    .AsNoTracking()
                    .Where(pc => StringHelper.ContainsCaseInsensitive(pc.Nome, normalizedSearch))
                    .Select(pc => pc.PersCustomId)
                    .ToListAsync();

                if (persCustomIds.Count == 0)
                    return SingleResponseDTO<int>.SuccessResponse(0,
                        $"Nessuna personalizzazione trovata con nome: {nomePersonalizzazione}");

                var count = await _context.BevandaCustom
                    .AsNoTracking()
                    .CountAsync(bc => persCustomIds.Contains(bc.PersCustomId));

                return SingleResponseDTO<int>.SuccessResponse(count,
                    count switch
                    {
                        0 => $"Nessuna bevanda custom trovata per personalizzazione: {nomePersonalizzazione}",
                        1 => $"1 bevanda custom trovata per personalizzazione: {nomePersonalizzazione}",
                        _ => $"{count} bevande custom trovate per personalizzazione: {nomePersonalizzazione}"
                    });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore in CountPersonalizzazioneAsync per nome: {Nome}", nomePersonalizzazione);
                return SingleResponseDTO<int>.ErrorResponse("Errore nel conteggio per personalizzazione");
            }
        }
    }
}