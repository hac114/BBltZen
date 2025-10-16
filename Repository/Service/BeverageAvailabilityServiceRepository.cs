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

        public async Task<BeverageAvailabilityDTO> CheckBeverageAvailabilityAsync(int articoloId)
        {
            try
            {
                _logger.LogInformation($"Verifica disponibilità bevanda: {articoloId}");

                // Trova l'articolo
                var articolo = await _context.Articolo
                    .FirstOrDefaultAsync(a => a.ArticoloId == articoloId);

                if (articolo == null)
                    throw new ArgumentException($"Articolo non trovato: {articoloId}");

                var risultato = new BeverageAvailabilityDTO
                {
                    ArticoloId = articoloId,
                    TipoArticolo = articolo.Tipo,
                    DataVerifica = DateTime.Now
                };

                // Verifica in base al tipo di articolo
                switch (articolo.Tipo)
                {
                    case "BS": // Bevanda Standard
                        await CheckBevandaStandardAvailabilityAsync(articoloId, risultato);
                        break;
                    case "BC": // Bevanda Custom
                        risultato.Disponibile = true; // Le custom sono sempre disponibili
                        risultato.Nome = "Bevanda Personalizzata";
                        break;
                    case "D": // Dolce
                        await CheckDolceAvailabilityAsync(articoloId, risultato);
                        break;
                    default:
                        throw new ArgumentException($"Tipo articolo non supportato: {articolo.Tipo}");
                }

                _logger.LogInformation($"Bevanda {articoloId}: {(risultato.Disponibile ? "DISPONIBILE" : "NON DISPONIBILE")}");
                return risultato;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Errore verifica disponibilità bevanda: {articoloId}");
                throw;
            }
        }

        private async Task CheckBevandaStandardAvailabilityAsync(int articoloId, BeverageAvailabilityDTO risultato)
        {
            var bevanda = await _context.BevandaStandard
                .FirstOrDefaultAsync(bs => bs.ArticoloId == articoloId);

            if (bevanda == null)
                throw new ArgumentException($"Bevanda Standard non trovata: {articoloId}");

            risultato.Nome = bevanda.Personalizzazione?.Nome ?? "Bevanda Standard";

            // VERIFICA SOLO GLI INGREDIENTI - NIENTE ALTRO CONTA!
            await CheckIngredientiBevandaStandardAsync(bevanda, risultato);

            // I campi SempreDisponibile e Disponibile sono solo per gestione interna
            // ma non influenzano la disponibilità effettiva della bevanda
        }

        private async Task CheckIngredientiBevandaStandardAsync(BevandaStandard bevanda, BeverageAvailabilityDTO risultato)
        {
            if (bevanda.PersonalizzazioneId == null)
            {
                risultato.Disponibile = true;
                return;
            }

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

        private async Task CheckDolceAvailabilityAsync(int articoloId, BeverageAvailabilityDTO risultato)
        {
            var dolce = await _context.Dolce
                .FirstOrDefaultAsync(d => d.ArticoloId == articoloId);

            if (dolce == null)
                throw new ArgumentException($"Dolce non trovato: {articoloId}");

            risultato.Nome = dolce.Nome;
            risultato.Disponibile = dolce.Disponibile;

            if (!risultato.Disponibile)
            {
                risultato.MotivoNonDisponibile = "Dolce non disponibile";
            }
        }

        public async Task<List<BeverageAvailabilityDTO>> CheckMultipleBeveragesAvailabilityAsync(List<int> articoliIds)
        {
            try
            {
                _logger.LogInformation($"Verifica disponibilità multipla per {articoliIds.Count} bevande");

                var risultati = new List<BeverageAvailabilityDTO>();
                var tasks = articoliIds.Select(id => CheckBeverageAvailabilityAsync(id));
                risultati.AddRange(await Task.WhenAll(tasks));

                return risultati;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Errore verifica disponibilità multipla per {articoliIds.Count} bevande");
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
                _logger.LogError(ex, $"Errore verifica rapida disponibilità bevanda: {articoloId}");
                return false;
            }
        }

        public async Task<AvailabilityUpdateDTO> UpdateBeverageAvailabilityAsync(int articoloId)
        {
            try
            {
                _logger.LogInformation($"Aggiornamento disponibilità bevanda: {articoloId}");

                var disponibilita = await CheckBeverageAvailabilityAsync(articoloId);
                var articolo = await _context.Articolo.FindAsync(articoloId);

                if (articolo == null)
                    throw new ArgumentException($"Articolo non trovato: {articoloId}");

                // Aggiorna SOLO in base agli ingredienti - NIENTE ALTRO!
                switch (articolo.Tipo)
                {
                    case "BS":
                        var bevanda = await _context.BevandaStandard
                            .FirstOrDefaultAsync(bs => bs.ArticoloId == articoloId);
                        if (bevanda != null)
                        {
                            // AGGIORNA SOLO IL CAMPO DI DISPONIBILITÀ REALE
                            // I campi SempreDisponibile/Disponibile sono per uso interno
                            bevanda.DataAggiornamento = DateTime.Now;
                            // Non aggiorniamo altri campi - la disponibilità è determinata solo dagli ingredienti
                        }
                        break;
                    case "D":
                        var dolce = await _context.Dolce
                            .FirstOrDefaultAsync(d => d.ArticoloId == articoloId);
                        if (dolce != null)
                        {
                            dolce.DataAggiornamento = DateTime.Now;
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
                    DataAggiornamento = DateTime.Now
                };

                _logger.LogInformation($"Bevanda {articoloId} aggiornata a: {(disponibilita.Disponibile ? "DISPONIBILE" : "NON DISPONIBILE")}");
                return risultato;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Errore aggiornamento disponibilità bevanda: {articoloId}");
                throw;
            }
        }

        public async Task<List<AvailabilityUpdateDTO>> UpdateAllBeveragesAvailabilityAsync()
        {
            try
            {
                _logger.LogInformation("Aggiornamento disponibilità TUTTE le bevande");

                // Trova tutte le bevande standard e dolci
                var bevandeStandardIds = await _context.BevandaStandard
                    .Select(bs => bs.ArticoloId)
                    .ToListAsync();

                var dolciIds = await _context.Dolce
                    .Select(d => d.ArticoloId)
                    .ToListAsync();

                var tuttiArticoliIds = bevandeStandardIds.Concat(dolciIds).ToList();

                var risultati = new List<AvailabilityUpdateDTO>();
                foreach (var articoloId in tuttiArticoliIds)
                {
                    try
                    {
                        var risultato = await UpdateBeverageAvailabilityAsync(articoloId);
                        risultati.Add(risultato);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Errore aggiornamento bevanda {articoloId}");
                    }
                }

                _logger.LogInformation($"Aggiornate {risultati.Count} bevande su {tuttiArticoliIds.Count}");
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
                _logger.LogInformation($"Forzatura disponibilità bevanda {articoloId} a: {disponibile}");

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
                            bevanda.DataAggiornamento = DateTime.Now;
                            if (!disponibile)
                            {
                                bevanda.SempreDisponibile = false; // Non può essere sempre disponibile se forzata a false
                            }
                        }
                        break;
                    case "D":
                        var dolce = await _context.Dolce
                            .FirstOrDefaultAsync(d => d.ArticoloId == articoloId);
                        if (dolce != null)
                        {
                            dolce.Disponibile = disponibile;
                            dolce.DataAggiornamento = DateTime.Now;
                        }
                        break;
                }

                await _context.SaveChangesAsync();
                _logger.LogInformation($"Bevanda {articoloId} forzata a: {disponibile}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Errore forzatura disponibilità bevanda: {articoloId}");
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
                    PrimoPianoDisponibile = primoPianoDisponibile,
                    SostitutiPrimoPiano = sostituti,
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

        public async Task<List<BeverageAvailabilityDTO>> GetAvailableBeveragesForPrimoPianoAsync(int numeroElementi = 6)
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

        public async Task<List<BeverageAvailabilityDTO>> FindSostitutiPrimoPianoAsync(int numeroRichieste = 3)
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

        public async Task<List<IngredienteMancanteDTO>> GetIngredientiCriticiAsync()
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

        public async Task<List<int>> GetBeveragesAffectedByIngredientAsync(int ingredienteId)
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
    }
}