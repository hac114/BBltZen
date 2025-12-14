using BBltZen;
using DTO;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Repository.Interface;

namespace Repository.Service
{
    public class BeverageAvailabilityServiceRepository : IBeverageAvailabilityServiceRepository
    {
        private readonly BubbleTeaContext _context;
        private readonly ILogger<BeverageAvailabilityServiceRepository> _logger;

        public BeverageAvailabilityServiceRepository(
            BubbleTeaContext context,
            ILogger<BeverageAvailabilityServiceRepository> logger)
        {
            _context = context;
            _logger = logger;
        }        

        private BeverageAvailabilityDTO MapToBeverageAvailabilityDTO(BevandaStandard bevanda, bool disponibile, string? motivo = null, List<IngredienteMancanteDTO>? ingredientiMancanti = null)
        {
            return new BeverageAvailabilityDTO
            {
                ArticoloId = bevanda.ArticoloId,
                TipoArticolo = "BS",
                Nome = bevanda.Personalizzazione?.Nome ?? "Bevanda Standard",
                Disponibile = disponibile,
                MotivoNonDisponibile = motivo,
                IngredientiMancanti = ingredientiMancanti ?? new List<IngredienteMancanteDTO>(),
                DataVerifica = DateTime.UtcNow
            };
        }

        public async Task<BeverageAvailabilityDTO> CheckBeverageAvailabilityAsync(int articoloId)
        {
            try
            {
                _logger.LogInformation("Verifica disponibilità bevanda: {ArticoloId}", articoloId);

                // Trova l'articolo
                var articolo = await _context.Articolo
                    .AsNoTracking()
                    .FirstOrDefaultAsync(a => a.ArticoloId == articoloId);

                if (articolo == null)
                    throw new ArgumentException($"Articolo non trovato: {articoloId}");

                BeverageAvailabilityDTO risultato;

                // Verifica in base al tipo di articolo
                switch (articolo.Tipo)
                {
                    case "BS": // Bevanda Standard
                        risultato = await CheckBevandaStandardAvailabilityAsync(articoloId);
                        break;
                    case "BC": // Bevanda Custom
                        risultato = new BeverageAvailabilityDTO
                        {
                            ArticoloId = articoloId,
                            TipoArticolo = "BC",
                            Nome = "Bevanda Personalizzata",
                            Disponibile = true,
                            DataVerifica = DateTime.UtcNow
                        };
                        break;
                    case "D": // Dolce
                        risultato = await CheckDolceAvailabilityAsync(articoloId);
                        break;
                    default:
                        throw new ArgumentException($"Tipo articolo non supportato: {articolo.Tipo}");
                }

                _logger.LogInformation("Bevanda {ArticoloId}: {Stato}",
                    articoloId, risultato.Disponibile ? "DISPONIBILE" : "NON DISPONIBILE");

                return risultato;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore verifica disponibilità bevanda: {ArticoloId}", articoloId);
                throw;
            }
        }

        private async Task<List<IngredienteMancanteDTO>> GetIngredientiMancantiAsync(int? personalizzazioneId)
        {
            if (personalizzazioneId == null)
                return new List<IngredienteMancanteDTO>();

            var ingredientiMancanti = await _context.PersonalizzazioneIngrediente
                .AsNoTracking()
                .Where(pi => pi.PersonalizzazioneId == personalizzazioneId)
                .Join(_context.Ingrediente,
                    pi => pi.IngredienteId,
                    i => i.IngredienteId,
                    (pi, i) => new { Ingrediente = i, Categoria = i.Categoria })
                .Where(x => !x.Ingrediente.Disponibile)
                .Select(x => new IngredienteMancanteDTO
                {
                    IngredienteId = x.Ingrediente.IngredienteId,
                    NomeIngrediente = x.Ingrediente.Ingrediente1,
                    Categoria = x.Categoria.Categoria,
                    Critico = true
                })
                .ToListAsync();

            return ingredientiMancanti;
        }        

        private async Task<BeverageAvailabilityDTO> CheckBevandaStandardAvailabilityAsync(int articoloId)
        {
            var bevanda = await _context.BevandaStandard
                .AsNoTracking()
                .Include(bs => bs.Personalizzazione)
                .FirstOrDefaultAsync(bs => bs.ArticoloId == articoloId);

            if (bevanda == null)
                throw new ArgumentException($"Bevanda Standard non trovata: {articoloId}");

            // VERIFICA SOLO GLI INGREDIENTI
            var ingredientiMancanti = await GetIngredientiMancantiAsync(bevanda.PersonalizzazioneId);
            var disponibile = !ingredientiMancanti.Any();

            return MapToBeverageAvailabilityDTO(
                bevanda,
                disponibile,
                disponibile ? null : "Ingredienti non disponibili",
                ingredientiMancanti
            );
        }

        private async Task CheckIngredientiBevandaStandardAsync(BevandaStandard bevanda, BeverageAvailabilityDTO risultato)
        {
            var ingredientiNecessari = await _context.PersonalizzazioneIngrediente
                .Where(pi => pi.PersonalizzazioneId == bevanda.PersonalizzazioneId)
                .Join(_context.Ingrediente,
                    pi => pi.IngredienteId,
                    i => i.IngredienteId,
                    (pi, i) => new { Ingrediente = i, Quantita = pi.Quantita })
                .ToListAsync();

            var ingredientiMancanti = new List<IngredienteMancanteDTO>();

            foreach (var ingrediente in ingredientiNecessari)
            {
                if (!ingrediente.Ingrediente.Disponibile)
                {
                    ingredientiMancanti.Add(new IngredienteMancanteDTO
                    {
                        IngredienteId = ingrediente.Ingrediente.IngredienteId,
                        NomeIngrediente = ingrediente.Ingrediente.Ingrediente1,
                        Categoria = ingrediente.Ingrediente.Categoria?.Categoria ?? "Sconosciuta",
                        Critico = true // Se manca un ingrediente, la bevanda non è disponibile
                    });
                }
            }

            if (ingredientiMancanti.Any())
            {
                risultato.Disponibile = false;
                risultato.MotivoNonDisponibile = "Ingredienti non disponibili";
                risultato.IngredientiMancanti = ingredientiMancanti;
            }
            else
            {
                risultato.Disponibile = true;
            }
        }

        private async Task<BeverageAvailabilityDTO> CheckDolceAvailabilityAsync(int articoloId)
        {
            var dolce = await _context.Dolce
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.ArticoloId == articoloId);

            if (dolce == null)
                throw new ArgumentException($"Dolce non trovato: {articoloId}");

            return new BeverageAvailabilityDTO
            {
                ArticoloId = articoloId,
                TipoArticolo = "D",
                Nome = dolce.Nome,
                Disponibile = dolce.Disponibile,
                MotivoNonDisponibile = dolce.Disponibile ? null : "Dolce non disponibile",
                DataVerifica = DateTime.UtcNow
            };
        }

        public async Task<IEnumerable<BeverageAvailabilityDTO>> CheckMultipleBeveragesAvailabilityAsync(List<int> articoliIds)
        {
            try
            {
                _logger.LogInformation("Verifica disponibilità multipla per {Count} bevande", articoliIds.Count);

                // ✅ OTTIMIZZATO: Task.WhenAll invece di sequenziale
                var tasks = articoliIds.Select(id => CheckBeverageAvailabilityAsync(id));
                var risultati = await Task.WhenAll(tasks);

                return risultati;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore verifica disponibilità multipla per {Count} bevande", articoliIds.Count);
                throw;
            }
        }

        public async Task<bool> IsBeverageAvailableAsync(int articoloId)
        {
            try
            {
                var disponibilita = await CheckBeverageAvailabilityAsync(articoloId);
                return disponibilita.Disponibile;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore verifica rapida disponibilità bevanda: {ArticoloId}", articoloId);
                return false;
            }
        }

        public async Task<AvailabilityUpdateDTO> UpdateBeverageAvailabilityAsync(int articoloId)
        {
            try
            {
                _logger.LogInformation("Aggiornamento disponibilità bevanda: {ArticoloId}", articoloId);

                var disponibilita = await CheckBeverageAvailabilityAsync(articoloId);
                var articolo = await _context.Articolo.FindAsync(articoloId);

                if (articolo == null)
                    throw new ArgumentException($"Articolo non trovato: {articoloId}");

                // ✅ OTTIMIZZATO: Query specifica per tipo articolo
                switch (articolo.Tipo)
                {
                    case "BS":
                        var bevanda = await _context.BevandaStandard
                            .FirstOrDefaultAsync(bs => bs.ArticoloId == articoloId);
                        if (bevanda != null)
                        {
                            bevanda.DataAggiornamento = DateTime.UtcNow;
                        }
                        break;
                    case "D":
                        var dolce = await _context.Dolce
                            .FirstOrDefaultAsync(d => d.ArticoloId == articoloId);
                        if (dolce != null)
                        {
                            dolce.DataAggiornamento = DateTime.UtcNow;
                        }
                        break;
                }

                await _context.SaveChangesAsync();

                var risultato = new AvailabilityUpdateDTO
                {
                    ArticoloId = articoloId,
                    TipoArticolo = articolo.Tipo,
                    NuovoStatoDisponibilita = disponibilita.Disponibile,
                    Motivo = disponibilita.MotivoNonDisponibile,
                    DataAggiornamento = DateTime.UtcNow
                };

                _logger.LogInformation("Bevanda {ArticoloId} aggiornata a: {Stato}",
                    articoloId, disponibilita.Disponibile ? "DISPONIBILE" : "NON DISPONIBILE");

                return risultato;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore aggiornamento disponibilità bevanda: {ArticoloId}", articoloId);
                throw;
            }
        }

        public async Task<IEnumerable<AvailabilityUpdateDTO>> UpdateAllBeveragesAvailabilityAsync()
        {
            try
            {
                _logger.LogInformation("Aggiornamento disponibilità TUTTE le bevande");

                // ✅ OTTIMIZZATO: Single query per tutti gli ID
                var tuttiArticoliIds = await _context.BevandaStandard
                    .Select(bs => bs.ArticoloId)
                    .Concat(_context.Dolce.Select(d => d.ArticoloId))
                    .ToListAsync();

                // ✅ OTTIMIZZATO: Task.WhenAll per aggiornamenti paralleli
                var tasks = tuttiArticoliIds.Select(id => UpdateBeverageAvailabilityAsync(id));
                var risultati = await Task.WhenAll(tasks);

                _logger.LogInformation("Aggiornate {Count} bevande su {Total}", risultati.Length, tuttiArticoliIds.Count);
                return risultati;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore aggiornamento massa disponibilità bevande");
                throw;
            }
        }

        public async Task ForceBeverageAvailabilityAsync(int articoloId, bool disponibile, string? motivo = null)
        {
            try
            {
                _logger.LogInformation("Forzatura disponibilità bevanda {ArticoloId} a: {Disponibile}",
                    articoloId, disponibile);

                var articolo = await _context.Articolo.FindAsync(articoloId);
                if (articolo == null)
                    throw new ArgumentException($"Articolo non trovato: {articoloId}");

                switch (articolo.Tipo)
                {
                    case "BS":
                        var bevanda = await _context.BevandaStandard
                            .FirstOrDefaultAsync(bs => bs.ArticoloId == articoloId);
                        if (bevanda != null)
                        {
                            bevanda.Disponibile = disponibile;
                            bevanda.DataAggiornamento = DateTime.UtcNow;
                            if (!disponibile)
                            {
                                bevanda.SempreDisponibile = false;
                            }
                        }
                        break;
                    case "D":
                        var dolce = await _context.Dolce
                            .FirstOrDefaultAsync(d => d.ArticoloId == articoloId);
                        if (dolce != null)
                        {
                            dolce.Disponibile = disponibile;
                            dolce.DataAggiornamento = DateTime.UtcNow;
                        }
                        break;
                }

                await _context.SaveChangesAsync();
                _logger.LogInformation("Bevanda {ArticoloId} forzata a: {Disponibile}", articoloId, disponibile);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore forzatura disponibilità bevanda: {ArticoloId}", articoloId);
                throw;
            }
        }

        public async Task<MenuAvailabilityDTO> GetMenuAvailabilityStatusAsync()
        {
            try
            {
                _logger.LogInformation("Recupero stato disponibilità menu completo");

                var bevandeStandard = await _context.BevandaStandard.ToListAsync();
                var dolci = await _context.Dolce.ToListAsync();

                var totalBevande = bevandeStandard.Count + dolci.Count;
                var bevandeDisponibili = bevandeStandard.Count(bs => bs.Disponibile) + dolci.Count(d => d.Disponibile);

                // Trova bevande per primo piano
                var primoPianoDisponibile = await GetAvailableBeveragesForPrimoPianoAsync(6);
                var sostituti = await FindSostitutiPrimoPianoAsync(3);

                var risultato = new MenuAvailabilityDTO
                {
                    TotalBevande = totalBevande,
                    BevandeDisponibili = bevandeDisponibili,
                    BevandeNonDisponibili = totalBevande - bevandeDisponibili,
                    PrimoPianoDisponibile = primoPianoDisponibile.ToList(),
                    SostitutiPrimoPiano = sostituti.ToList(),
                    DataAggiornamento = DateTime.Now
                };

                _logger.LogInformation($"Menu: {bevandeDisponibili}/{totalBevande} bevande disponibili");
                return risultato;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore recupero stato disponibilità menu");
                throw;
            }
        }

        public async Task<IEnumerable<BeverageAvailabilityDTO>> GetAvailableBeveragesForPrimoPianoAsync(int numeroElementi = 6)
        {
            try
            {
                // Bevande con priorità alta e disponibili
                var bevandePrimoPiano = await _context.BevandaStandard
                    .Where(bs => bs.Disponibile && bs.Priorita >= 1)
                    .OrderByDescending(bs => bs.Priorita)
                    .Take(numeroElementi)
                    .Select(bs => bs.ArticoloId)
                    .ToListAsync();

                var risultati = new List<BeverageAvailabilityDTO>();
                foreach (var articoloId in bevandePrimoPiano)
                {
                    var disponibilita = await CheckBeverageAvailabilityAsync(articoloId);
                    if (disponibilita.Disponibile)
                    {
                        risultati.Add(disponibilita);
                    }
                }

                // Se non abbiamo abbastanza bevande, aggiungiamo altre disponibili
                if (risultati.Count < numeroElementi)
                {
                    var altreBevande = await _context.BevandaStandard
                        .Where(bs => bs.Disponibile && bs.Priorita == 0)
                        .OrderBy(bs => bs.ArticoloId)
                        .Take(numeroElementi - risultati.Count)
                        .Select(bs => bs.ArticoloId)
                        .ToListAsync();

                    foreach (var articoloId in altreBevande)
                    {
                        var disponibilita = await CheckBeverageAvailabilityAsync(articoloId);
                        if (disponibilita.Disponibile)
                        {
                            risultati.Add(disponibilita);
                        }
                    }
                }

                return risultati.Take(numeroElementi).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore recupero bevande per primo piano");
                return new List<BeverageAvailabilityDTO>();
            }
        }

        public async Task<IEnumerable<BeverageAvailabilityDTO>> FindSostitutiPrimoPianoAsync(int numeroRichieste = 3)
        {
            try
            {
                // Trova bevande disponibili non in primo piano che possono sostituire
                var sostituti = await _context.BevandaStandard
                    .Where(bs => bs.Disponibile && bs.Priorita == 0) // Non in primo piano
                    .OrderByDescending(bs => bs.Priorita)
                    .ThenBy(bs => bs.ArticoloId)
                    .Take(numeroRichieste * 2) // Prendine il doppio per sicurezza
                    .Select(bs => bs.ArticoloId)
                    .ToListAsync();

                var risultati = new List<BeverageAvailabilityDTO>();
                foreach (var articoloId in sostituti)
                {
                    var disponibilita = await CheckBeverageAvailabilityAsync(articoloId);
                    if (disponibilita.Disponibile && await CanBeverageBeInPrimoPianoAsync(articoloId))
                    {
                        risultati.Add(disponibilita);
                        if (risultati.Count >= numeroRichieste)
                            break;
                    }
                }

                return risultati;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore ricerca sostituti primo piano");
                return new List<BeverageAvailabilityDTO>();
            }
        }

        public async Task<IEnumerable<IngredienteMancanteDTO>> GetIngredientiCriticiAsync()
        {
            try
            {
                var ingredientiNonDisponibili = await _context.Ingrediente
                    .Where(i => !i.Disponibile)
                    .Select(i => new IngredienteMancanteDTO
                    {
                        IngredienteId = i.IngredienteId,
                        NomeIngrediente = i.Ingrediente1,
                        Categoria = i.Categoria.Categoria,
                        Critico = true
                    })
                    .ToListAsync();

                return ingredientiNonDisponibili;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore recupero ingredienti critici");
                return new List<IngredienteMancanteDTO>();
            }
        }

        public async Task<int> GetCountBeveragesWithLowStockAsync()
        {
            try
            {
                // Conta bevande non disponibili a causa di ingredienti mancanti
                var bevandeNonDisponibili = await _context.BevandaStandard
                    .Where(bs => !bs.Disponibile && !bs.SempreDisponibile)
                    .CountAsync();

                return bevandeNonDisponibili;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore conteggio bevande con stock basso");
                return 0;
            }
        }

        public async Task<bool> CanBeverageBeInPrimoPianoAsync(int articoloId)
        {
            try
            {
                var disponibilita = await CheckBeverageAvailabilityAsync(articoloId);
                if (!disponibilita.Disponibile)
                    return false;

                // Verifica ulteriori criteri per il primo piano
                var bevanda = await _context.BevandaStandard
                    .FirstOrDefaultAsync(bs => bs.ArticoloId == articoloId);

                return bevanda != null && bevanda.Disponibile;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Errore verifica bevanda per primo piano: {articoloId}");
                return false;
            }
        }

        public async Task<IEnumerable<int>> GetBeveragesAffectedByIngredientAsync(int ingredienteId)
        {
            try
            {
                // Trova tutte le bevande che usano questo ingrediente
                var bevandeAffected = await _context.PersonalizzazioneIngrediente
                    .Where(pi => pi.IngredienteId == ingredienteId)
                    .Join(_context.Personalizzazione,
                        pi => pi.PersonalizzazioneId,
                        p => p.PersonalizzazioneId,
                        (pi, p) => p.PersonalizzazioneId)
                    .Join(_context.BevandaStandard,
                        pid => pid,
                        bs => bs.PersonalizzazioneId,
                        (pid, bs) => bs.ArticoloId)
                    .Distinct()
                    .ToListAsync();

                return bevandeAffected;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Errore recupero bevande affette da ingrediente: {ingredienteId}");
                return new List<int>();
            }
        }

        private async Task<bool> CheckIngredientiDisponibiliAsync(int? personalizzazioneId)
        {
            if (personalizzazioneId == null) return true;

            var ingredientiNonDisponibili = await _context.PersonalizzazioneIngrediente
                .Where(pi => pi.PersonalizzazioneId == personalizzazioneId)
                .Join(_context.Ingrediente,
                    pi => pi.IngredienteId,
                    i => i.IngredienteId,
                    (pi, i) => i)
                .AnyAsync(i => !i.Disponibile);

            return !ingredientiNonDisponibili;
        }

        public async Task<bool> ExistsAsync(int articoloId)
        {
            try
            {
                var exists = await _context.Articolo
                    .AsNoTracking()
                    .AnyAsync(a => a.ArticoloId == articoloId);

                return exists;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore nella verifica esistenza articolo: {ArticoloId}", articoloId);
                return false;
            }
        }
    }
}