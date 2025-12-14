using BBltZen;
using DTO;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Repository.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace RepositoryTest
{
    public class BeverageAvailabilityServiceRepositoryTest : BaseTest
    {
        private readonly BeverageAvailabilityServiceRepository _service;
        private readonly Mock<ILogger<BeverageAvailabilityServiceRepository>> _loggerMock;

        public BeverageAvailabilityServiceRepositoryTest()
        {
            _loggerMock = new Mock<ILogger<BeverageAvailabilityServiceRepository>>();
            _service = new BeverageAvailabilityServiceRepository(_context, _loggerMock.Object);
        }

        [Fact]
        public async Task CheckBeverageAvailabilityAsync_BevandaStandardDisponibile_ReturnsTrue()
        {
            // Arrange
            var articolo = CreateTestArticolo("BS");
            var personalizzazione = CreateTestPersonalizzazione();
            var dimensioneBicchiere = CreateTestDimensioneBicchiere();
            var bevanda = CreateTestBevandaStandard(articolo.ArticoloId, personalizzazione.PersonalizzazioneId, dimensioneBicchiere.DimensioneBicchiereId, true, true);

            var ingrediente = CreateTestIngrediente(true);
            CreateTestPersonalizzazioneIngrediente(personalizzazione.PersonalizzazioneId, ingrediente.IngredienteId, 1);

            // Act
            var result = await _service.CheckBeverageAvailabilityAsync(articolo.ArticoloId);

            // Assert
            Assert.True(result.Disponibile);
            Assert.Equal("BS", result.TipoArticolo);
            Assert.Null(result.MotivoNonDisponibile);
        }

        [Fact]
        public async Task CheckBeverageAvailabilityAsync_BevandaStandardConIngredienteNonDisponibile_ReturnsFalse()
        {
            // Arrange
            var articolo = CreateTestArticolo("BS");
            var personalizzazione = CreateTestPersonalizzazione();
            var dimensioneBicchiere = CreateTestDimensioneBicchiere();
            var bevanda = CreateTestBevandaStandard(articolo.ArticoloId, personalizzazione.PersonalizzazioneId, dimensioneBicchiere.DimensioneBicchiereId, true, false);

            var ingrediente = CreateTestIngrediente(false); // Ingrediente non disponibile
            CreateTestPersonalizzazioneIngrediente(personalizzazione.PersonalizzazioneId, ingrediente.IngredienteId, 1);

            // Act
            var result = await _service.CheckBeverageAvailabilityAsync(articolo.ArticoloId);

            // Assert
            Assert.False(result.Disponibile);
            Assert.Equal("Ingredienti non disponibili", result.MotivoNonDisponibile);
            Assert.NotNull(result.IngredientiMancanti);
            Assert.Single(result.IngredientiMancanti);
        }

        [Fact]
        public async Task CheckBeverageAvailabilityAsync_BevandaCustom_ReturnsAlwaysTrue()
        {
            // Arrange
            var articolo = CreateTestArticolo("BC");

            // Act
            var result = await _service.CheckBeverageAvailabilityAsync(articolo.ArticoloId);

            // Assert
            Assert.True(result.Disponibile);
            Assert.Equal("BC", result.TipoArticolo);
            Assert.Equal("Bevanda Personalizzata", result.Nome);
        }

        [Fact]
        public async Task CheckBeverageAvailabilityAsync_DolceDisponibile_ReturnsTrue()
        {
            // Arrange
            var articolo = CreateTestArticolo("D");
            var dolce = CreateTestDolce(articolo.ArticoloId, true);

            // Act
            var result = await _service.CheckBeverageAvailabilityAsync(articolo.ArticoloId);

            // Assert
            Assert.True(result.Disponibile);
            Assert.Equal("D", result.TipoArticolo);
        }

        [Fact]
        public async Task CheckBeverageAvailabilityAsync_DolceNonDisponibile_ReturnsFalse()
        {
            // Arrange
            var articolo = CreateTestArticolo("D");
            var dolce = CreateTestDolce(articolo.ArticoloId, false);

            // Act
            var result = await _service.CheckBeverageAvailabilityAsync(articolo.ArticoloId);

            // Assert
            Assert.False(result.Disponibile);
            Assert.Equal("Dolce non disponibile", result.MotivoNonDisponibile);
        }

        [Fact]
        public async Task CheckMultipleBeveragesAvailabilityAsync_MultipleBeverages_ReturnsAllResults()
        {
            // Arrange
            var articolo1 = CreateTestArticolo("BS");
            var articolo2 = CreateTestArticolo("D");

            var personalizzazione = CreateTestPersonalizzazione();
            var dimensioneBicchiere = CreateTestDimensioneBicchiere();
            var bevanda = CreateTestBevandaStandard(articolo1.ArticoloId, personalizzazione.PersonalizzazioneId, dimensioneBicchiere.DimensioneBicchiereId, true, true);
            var dolce = CreateTestDolce(articolo2.ArticoloId, true);

            var ingrediente = CreateTestIngrediente(true);
            CreateTestPersonalizzazioneIngrediente(personalizzazione.PersonalizzazioneId, ingrediente.IngredienteId, 1);

            var articoliIds = new List<int> { articolo1.ArticoloId, articolo2.ArticoloId };

            // Act
            var results = await _service.CheckMultipleBeveragesAvailabilityAsync(articoliIds);

            // Assert
            Assert.Equal(2, results.Count());
            Assert.All(results, r => Assert.True(r.Disponibile));
        }

        [Fact]
        public async Task IsBeverageAvailableAsync_AvailableBeverage_ReturnsTrue()
        {
            // Arrange
            var articolo = CreateTestArticolo("BS");
            var personalizzazione = CreateTestPersonalizzazione();
            var dimensioneBicchiere = CreateTestDimensioneBicchiere();
            var bevanda = CreateTestBevandaStandard(articolo.ArticoloId, personalizzazione.PersonalizzazioneId, dimensioneBicchiere.DimensioneBicchiereId, true, true);
            var ingrediente = CreateTestIngrediente(true);
            CreateTestPersonalizzazioneIngrediente(personalizzazione.PersonalizzazioneId, ingrediente.IngredienteId, 1);

            // Act
            var result = await _service.IsBeverageAvailableAsync(articolo.ArticoloId);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task UpdateBeverageAvailabilityAsync_WithUnavailableIngredients_ReturnsFalse()
        {
            // Arrange
            var articolo = CreateTestArticolo("BS");
            var personalizzazione = CreateTestPersonalizzazione();
            var dimensioneBicchiere = CreateTestDimensioneBicchiere();

            // CREA BEVANDA CON INGREDIENTE NON DISPONIBILE
            var bevanda = CreateTestBevandaStandard(
                articolo.ArticoloId,
                personalizzazione.PersonalizzazioneId,
                dimensioneBicchiere.DimensioneBicchiereId,
                true,   // Disponibile (non importa)
                true    // SempreDisponibile (non importa)
            );

            var ingrediente = CreateTestIngrediente(false); // INGREDIENTE NON DISPONIBILE
            CreateTestPersonalizzazioneIngrediente(personalizzazione.PersonalizzazioneId, ingrediente.IngredienteId, 1);

            // Act
            var result = await _service.UpdateBeverageAvailabilityAsync(articolo.ArticoloId);

            // Assert - DEVE ESSERE FALSE PERCHÉ L'INGREDIENTE NON È DISPONIBILE
            Assert.False(result.NuovoStatoDisponibilita);
            Assert.Equal("Ingredienti non disponibili", result.Motivo);

            // La bevanda dovrebbe essere NON disponibile nel sito
            var checkFinale = await _service.CheckBeverageAvailabilityAsync(articolo.ArticoloId);
            Assert.False(checkFinale.Disponibile, "La bevanda dovrebbe essere NON disponibile nel sito");
        }

        [Fact]
        public async Task UpdateBeverageAvailabilityAsync_UpdatesAvailabilityStatus()
        {
            // Arrange
            var articolo = CreateTestArticolo("BS");
            var personalizzazione = CreateTestPersonalizzazione();
            var dimensioneBicchiere = CreateTestDimensioneBicchiere();

            // CREA BEVANDA CON INGREDIENTI DISPONIBILI
            var bevanda = CreateTestBevandaStandard(
                articolo.ArticoloId,
                personalizzazione.PersonalizzazioneId,
                dimensioneBicchiere.DimensioneBicchiereId,
                true,   // Disponibile
                false   // SempreDisponibile
            );

            var ingrediente = CreateTestIngrediente(true); // INGREDIENTE DISPONIBILE
            CreateTestPersonalizzazioneIngrediente(personalizzazione.PersonalizzazioneId, ingrediente.IngredienteId, 1);

            // Act
            var result = await _service.UpdateBeverageAvailabilityAsync(articolo.ArticoloId);

            // Assert - DEVE ESSERE TRUE perché l'ingrediente è disponibile
            Assert.True(result.NuovoStatoDisponibilita);
            Assert.Equal("BS", result.TipoArticolo);
            Assert.Null(result.Motivo); // Nessun motivo = disponibile
        }

        [Fact]
        public async Task ForceBeverageAvailabilityAsync_ForcesAvailability()
        {
            // Arrange
            var articolo = CreateTestArticolo("BS");
            var personalizzazione = CreateTestPersonalizzazione();
            var dimensioneBicchiere = CreateTestDimensioneBicchiere();
            var bevanda = CreateTestBevandaStandard(articolo.ArticoloId, personalizzazione.PersonalizzazioneId, dimensioneBicchiere.DimensioneBicchiereId, false, false);

            // Act
            await _service.ForceBeverageAvailabilityAsync(articolo.ArticoloId, true, "Test forzatura");

            // Assert
            var updatedBevanda = _context.BevandaStandard.First(bs => bs.ArticoloId == articolo.ArticoloId);
            Assert.True(updatedBevanda.Disponibile);
        }

        [Fact]
        public async Task GetMenuAvailabilityStatusAsync_ReturnsCorrectCounts()
        {
            // Arrange
            var articolo1 = CreateTestArticolo("BS");
            var articolo2 = CreateTestArticolo("BS");
            var articolo3 = CreateTestArticolo("D");

            var personalizzazione = CreateTestPersonalizzazione();
            var dimensioneBicchiere = CreateTestDimensioneBicchiere();
            var bevanda1 = CreateTestBevandaStandard(articolo1.ArticoloId, personalizzazione.PersonalizzazioneId, dimensioneBicchiere.DimensioneBicchiereId, true, true);
            var bevanda2 = CreateTestBevandaStandard(articolo2.ArticoloId, personalizzazione.PersonalizzazioneId, dimensioneBicchiere.DimensioneBicchiereId, false, false);
            var dolce = CreateTestDolce(articolo3.ArticoloId, true);

            var ingrediente = CreateTestIngrediente(true);
            CreateTestPersonalizzazioneIngrediente(personalizzazione.PersonalizzazioneId, ingrediente.IngredienteId, 1);

            // Act
            var result = await _service.GetMenuAvailabilityStatusAsync();

            // Assert
            Assert.Equal(3, result.TotalBevande);
            Assert.Equal(2, result.BevandeDisponibili); // bevanda1 + dolce
            Assert.Equal(1, result.BevandeNonDisponibili); // bevanda2
        }

        [Fact]
        public async Task GetIngredientiCriticiAsync_ReturnsNonAvailableIngredients()
        {
            // Arrange
            var ingrediente1 = CreateTestIngrediente(false); // Non disponibile
            var ingrediente2 = CreateTestIngrediente(true);  // Disponibile

            // Act
            var result = await _service.GetIngredientiCriticiAsync();

            // Assert
            Assert.Single(result);
            Assert.Equal(ingrediente1.IngredienteId, result.First().IngredienteId);
            Assert.True(result.First().Critico);
        }

        // Helper methods for test data creation
        private Articolo CreateTestArticolo(string tipo)
        {
            var articolo = new Articolo
            {
                Tipo = tipo,
                DataCreazione = DateTime.Now,
                DataAggiornamento = DateTime.Now
            };
            _context.Articolo.Add(articolo);
            _context.SaveChanges();
            return articolo;
        }

        private Personalizzazione CreateTestPersonalizzazione()
        {
            var personalizzazione = new Personalizzazione
            {
                Nome = "Test Personalizzazione",
                Descrizione = "Descrizione test personalizzazione",
                DtCreazione = DateTime.Now,                
            };
            _context.Personalizzazione.Add(personalizzazione);
            _context.SaveChanges();
            return personalizzazione;
        }

        private DimensioneBicchiere CreateTestDimensioneBicchiere()
        {
            // Prima crea un'unità di misura se non esiste
            if (!_context.UnitaDiMisura.Any())
            {
                var unitaMisura = new UnitaDiMisura
                {
                    Sigla = "ml",
                    Descrizione = "millilitri"
                };
                _context.UnitaDiMisura.Add(unitaMisura);
                _context.SaveChanges();
            }

            var unitaMisuraId = _context.UnitaDiMisura.First().UnitaMisuraId;

            var dimensione = new DimensioneBicchiere
            {
                Sigla = "M",
                Descrizione = "Medium",
                Capienza = 500.00m,
                UnitaMisuraId = unitaMisuraId,
                PrezzoBase = 3.50m,
                Moltiplicatore = 1.00m
            };
            _context.DimensioneBicchiere.Add(dimensione);
            _context.SaveChanges();
            return dimensione;
        }

        private BevandaStandard CreateTestBevandaStandard(int articoloId, int personalizzazioneId, int dimensioneBicchiereId, bool disponibile, bool sempreDisponibile)
        {
            var bevanda = new BevandaStandard
            {
                ArticoloId = articoloId,
                PersonalizzazioneId = personalizzazioneId,
                DimensioneBicchiereId = dimensioneBicchiereId,
                Prezzo = 3.50m, // Prezzo obbligatorio
                Disponibile = disponibile,
                SempreDisponibile = sempreDisponibile,
                Priorita = 1,
                DataCreazione = DateTime.Now,
                DataAggiornamento = DateTime.Now
            };
            _context.BevandaStandard.Add(bevanda);
            _context.SaveChanges();
            return bevanda;
        }

        private Dolce CreateTestDolce(int articoloId, bool disponibile)
        {
            var dolce = new Dolce
            {
                ArticoloId = articoloId,
                Nome = "Test Dolce",
                Prezzo = 4.50m, // Prezzo obbligatorio
                Descrizione = "Descrizione test dolce",
                Disponibile = disponibile,
                Priorita = 1,
                DataCreazione = DateTime.Now,
                DataAggiornamento = DateTime.Now
            };
            _context.Dolce.Add(dolce);
            _context.SaveChanges();
            return dolce;
        }

        private Ingrediente CreateTestIngrediente(bool disponibile)
        {
            var categoria = _context.CategoriaIngrediente.First();

            var ingrediente = new Ingrediente
            {
                Ingrediente1 = $"Test Ingrediente {Guid.NewGuid()}",
                CategoriaId = categoria.CategoriaId,
                PrezzoAggiunto = 1.00m, // Prezzo obbligatorio
                Disponibile = disponibile,
                DataInserimento = DateTime.Now,
                DataAggiornamento = DateTime.Now
            };
            _context.Ingrediente.Add(ingrediente);
            _context.SaveChanges();
            return ingrediente;
        }

        private PersonalizzazioneIngrediente CreateTestPersonalizzazioneIngrediente(int personalizzazioneId, int ingredienteId, decimal quantita)
        {
            // Prima crea un'unità di misura se non esiste per PersonalizzazioneIngrediente
            if (!_context.UnitaDiMisura.Any(um => um.Sigla == "g"))
            {
                var unitaMisura = new UnitaDiMisura
                {
                    Sigla = "g",
                    Descrizione = "grammi"
                };
                _context.UnitaDiMisura.Add(unitaMisura);
                _context.SaveChanges();
            }

            var unitaMisuraId = _context.UnitaDiMisura.First(um => um.Sigla == "g").UnitaMisuraId;

            var pi = new PersonalizzazioneIngrediente
            {
                PersonalizzazioneId = personalizzazioneId,
                IngredienteId = ingredienteId,
                Quantita = quantita,
                UnitaMisuraId = unitaMisuraId // Campo obbligatorio
            };
            _context.PersonalizzazioneIngrediente.Add(pi);
            _context.SaveChanges();
            return pi;
        }
    }
}