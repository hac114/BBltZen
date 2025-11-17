using Database;
using DTO;
using Microsoft.EntityFrameworkCore;
using Repository.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace RepositoryTest
{
    public class BevandaStandardRepositoryTest : BaseTest
    {
        private readonly BevandaStandardRepository _repository;
        private readonly BubbleTeaContext _context;

        public BevandaStandardRepositoryTest()
        {
            var options = new DbContextOptionsBuilder<BubbleTeaContext>()
                .UseInMemoryDatabase(databaseName: $"BevandaStandardTest_{Guid.NewGuid()}")
                .Options;

            _context = new BubbleTeaContext(options);
            _repository = new BevandaStandardRepository(_context);

            InitializeTestData();
        }

        private void InitializeTestData()
        {
            _context.Database.EnsureDeleted();
            _context.Database.EnsureCreated();

            // ✅ CORRETTO: Crea prima gli Articoli SENZA specificare ArticoloId
            var articoli = new List<Articolo>
            {
                new Articolo { Tipo = "BS", DataCreazione = DateTime.Now, DataAggiornamento = DateTime.Now },
                new Articolo { Tipo = "BS", DataCreazione = DateTime.Now, DataAggiornamento = DateTime.Now },
                new Articolo { Tipo = "BS", DataCreazione = DateTime.Now, DataAggiornamento = DateTime.Now },
                new Articolo { Tipo = "BS", DataCreazione = DateTime.Now, DataAggiornamento = DateTime.Now }
            };

            // Salva per generare gli ArticoloId automaticamente
            _context.Articolo.AddRange(articoli);
            _context.SaveChanges();

            // ✅ CORRETTO: Ora abbiamo gli ArticoloId generati
            var articoloId1 = articoli[0].ArticoloId;
            var articoloId2 = articoli[1].ArticoloId;
            var articoloId3 = articoli[2].ArticoloId;
            var articoloId4 = articoli[3].ArticoloId;

            // Crea Personalizzazioni - SOLO campi esistenti
            var personalizzazioni = new List<Personalizzazione>
            {
                new Personalizzazione
                {
                    PersonalizzazioneId = 1,
                    Nome = "Classica",
                    Descrizione = "Personalizzazione classica",
                    DtCreazione = DateTime.Now
                },
                new Personalizzazione
                {
                    PersonalizzazioneId = 2,
                    Nome = "Premium",
                    Descrizione = "Personalizzazione premium",
                    DtCreazione = DateTime.Now
                }
            };

            // Crea UnitaDiMisura
            var unitaDiMisura = new List<UnitaDiMisura>
            {
                new UnitaDiMisura { UnitaMisuraId = 1, Sigla = "GR", Descrizione = "grammi" },
                new UnitaDiMisura { UnitaMisuraId = 2, Sigla = "ML", Descrizione = "millilitri" }
            };

            // Crea Dimensioni Bicchiere
            var dimensioniBicchiere = new List<DimensioneBicchiere>
            {
                new DimensioneBicchiere
                {
                    DimensioneBicchiereId = 1,
                    Sigla = "S",
                    Descrizione = "Piccolo",
                    Capienza = 350.0m,
                    UnitaMisuraId = 1,
                    PrezzoBase = 0.50m,
                    Moltiplicatore = 1.0m
                },
                new DimensioneBicchiere
                {
                    DimensioneBicchiereId = 2,
                    Sigla = "M",
                    Descrizione = "Medio",
                    Capienza = 500.0m,
                    UnitaMisuraId = 1,
                    PrezzoBase = 1.00m,
                    Moltiplicatore = 1.2m
                }
            };

            // ✅ CORRETTO: Crea Bevande Standard con ArticoloId generati automaticamente
            var bevandeStandard = new List<BevandaStandard>
            {
                new BevandaStandard
                {
                    ArticoloId = articoloId1, // ✅ USA ArticoloId generato
                    PersonalizzazioneId = 1,
                    DimensioneBicchiereId = 1,
                    Prezzo = 5.00m,
                    ImmagineUrl = "url1.jpg",
                    Disponibile = true,
                    SempreDisponibile = false,
                    Priorita = 1,
                    DataCreazione = DateTime.Now.AddDays(-10),
                    DataAggiornamento = DateTime.Now.AddDays(-1)
                },
                new BevandaStandard
                {
                    ArticoloId = articoloId2, // ✅ USA ArticoloId generato
                    PersonalizzazioneId = 2,
                    DimensioneBicchiereId = 2,
                    Prezzo = 6.00m,
                    ImmagineUrl = "url2.jpg",
                    Disponibile = true,
                    SempreDisponibile = true,
                    Priorita = 2,
                    DataCreazione = DateTime.Now.AddDays(-5),
                    DataAggiornamento = DateTime.Now
                },
                new BevandaStandard
                {
                    ArticoloId = articoloId3, // ✅ USA ArticoloId generato
                    PersonalizzazioneId = 1,
                    DimensioneBicchiereId = 1,
                    Prezzo = 4.50m,
                    ImmagineUrl = "url3.jpg",
                    Disponibile = false,
                    SempreDisponibile = false,
                    Priorita = 3,
                    DataCreazione = DateTime.Now.AddDays(-3),
                    DataAggiornamento = DateTime.Now.AddDays(-1)
                }
            };

            _context.UnitaDiMisura.AddRange(unitaDiMisura);
            _context.Personalizzazione.AddRange(personalizzazioni);
            _context.DimensioneBicchiere.AddRange(dimensioniBicchiere);
            _context.BevandaStandard.AddRange(bevandeStandard);
            _context.SaveChanges();
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnAllBevandeStandard()
        {
            // Act
            var result = await _repository.GetAllAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, result.Count());
        }

        [Fact]
        public async Task GetDisponibiliAsync_ShouldReturnOnlySempreDisponibiliBevande()
        {
            // Act
            var result = await _repository.GetDisponibiliAsync();

            // Assert
            Assert.NotNull(result);
            var resultList = result.ToList();
            Assert.Single(resultList); // Solo 1 bevanda con SempreDisponibile = true
            Assert.All(resultList, bs => Assert.True(bs.SempreDisponibile));
        }

        [Fact]
        public async Task GetDisponibiliAsync_ShouldOrderByPriorita()
        {
            // Act
            var result = await _repository.GetDisponibiliAsync();

            // Assert
            var resultList = result.ToList();
            Assert.Single(resultList);
            Assert.Equal(2, resultList[0].Priorita); // Bevanda 2 ha Priorita = 2
        }

        [Fact]
        public async Task GetByIdAsync_WithValidId_ShouldReturnBevandaStandard()
        {
            // Act
            var result = await _repository.GetByIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.ArticoloId);
            Assert.Equal(5.00m, result.Prezzo);
            Assert.True(result.Disponibile);
        }

        [Fact]
        public async Task GetByIdAsync_WithInvalidId_ShouldReturnNull()
        {
            // Act
            var result = await _repository.GetByIdAsync(999);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetByArticoloIdAsync_ShouldCallGetByIdAsync()
        {
            // Act
            var result = await _repository.GetByArticoloIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.ArticoloId);
        }

        [Fact]
        public async Task GetByDimensioneBicchiereAsync_ShouldReturnFilteredBevande()
        {
            // Act - Cerca per dimensione 2 (bevanda 2 è SempreDisponibile = true)
            var result = await _repository.GetByDimensioneBicchiereAsync(2);

            // Assert
            var resultList = result.ToList();
            Assert.Single(resultList); // Ora trova la bevanda 2
            Assert.Equal(2, resultList[0].ArticoloId); // Bevanda 2
            Assert.Equal(2, resultList[0].DimensioneBicchiereId);
        }

        [Fact]
        public async Task GetByPersonalizzazioneAsync_ShouldReturnFilteredBevande()
        {
            // Act
            var result = await _repository.GetByPersonalizzazioneAsync(1);

            // Assert
            var resultList = result.ToList();
            Assert.Empty(resultList); // Nessuna bevanda con PersonalizzazioneId = 1 E SempreDisponibile = true
        }

        [Fact]
        public async Task AddAsync_ShouldAddNewBevandaStandard()
        {
            // Arrange
            var newBevanda = new BevandaStandardDTO
            {
                // ❌ RIMUOVI ArticoloId - viene generato automaticamente!
                PersonalizzazioneId = 1,
                DimensioneBicchiereId = 2,
                Prezzo = 7.00m,
                ImmagineUrl = "url4.jpg",
                Disponibile = true,
                SempreDisponibile = false,
                Priorita = 4
            };

            // Act
            var result = await _repository.AddAsync(newBevanda); // ✅ CORREGGI: assegna risultato

            // Assert
            // ✅ Ora usa result.ArticoloId che è stato generato dal repository
            Assert.True(result.ArticoloId > 0);
            var savedBevanda = await _repository.GetByIdAsync(result.ArticoloId);
            Assert.NotNull(savedBevanda);
            Assert.Equal(7.00m, savedBevanda.Prezzo);
            Assert.True(savedBevanda.Disponibile);
            Assert.False(savedBevanda.SempreDisponibile);
            Assert.Equal(4, savedBevanda.Priorita); // ✅ Verifica priorità
            Assert.NotNull(savedBevanda.DataCreazione);
            Assert.NotNull(savedBevanda.DataAggiornamento);
        }

        [Fact]
        public async Task UpdateAsync_ShouldUpdateExistingBevandaStandard()
        {
            // Arrange
            var updateDto = new BevandaStandardDTO
            {
                ArticoloId = 1,
                PersonalizzazioneId = 1,
                DimensioneBicchiereId = 1,
                Prezzo = 6.50m,
                ImmagineUrl = "url_updated.jpg",
                Disponibile = false,
                SempreDisponibile = false,
                Priorita = 5 // ✅ Priorità aggiornata
            };

            // Act
            await _repository.UpdateAsync(updateDto);

            // Assert
            var result = await _repository.GetByIdAsync(1);
            Assert.NotNull(result);
            Assert.Equal(6.50m, result.Prezzo);
            Assert.False(result.Disponibile);
            Assert.Equal(5, result.Priorita); // ✅ Verifica priorità aggiornata
            Assert.Equal("url_updated.jpg", result.ImmagineUrl);
            Assert.NotNull(result.DataAggiornamento); // ✅ Verifica data aggiornamento
        }

        [Fact]
        public async Task UpdateAsync_WithNonExistingId_ShouldThrowException()
        {
            // Arrange
            var updateDto = new BevandaStandardDTO { ArticoloId = 999 };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _repository.UpdateAsync(updateDto));
        }

        [Fact]
        public async Task DeleteAsync_ShouldRemoveBevandaStandard()
        {
            // Act
            await _repository.DeleteAsync(1);

            // Assert
            var result = await _repository.GetByIdAsync(1);
            Assert.Null(result);
        }

        [Fact]
        public async Task DeleteAsync_WithNonExistingId_ShouldNotThrow()
        {
            // Act & Assert
            var exception = await Record.ExceptionAsync(() => _repository.DeleteAsync(999));
            Assert.Null(exception);
        }

        [Fact]
        public async Task ExistsAsync_WithExistingId_ShouldReturnTrue()
        {
            // Act
            var result = await _repository.ExistsAsync(1);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task ExistsAsync_WithNonExistingId_ShouldReturnFalse()
        {
            // Act
            var result = await _repository.ExistsAsync(999);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task ExistsByCombinazioneAsync_WithExistingCombinazione_ShouldReturnTrue()
        {
            // Act
            var result = await _repository.ExistsByCombinazioneAsync(1, 1);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task ExistsByCombinazioneAsync_WithNonExistingCombinazione_ShouldReturnFalse()
        {
            // Act
            var result = await _repository.ExistsByCombinazioneAsync(999, 999);

            // Assert
            Assert.False(result);
        }

        // ✅ NUOVI TEST PER CARD PRODOTTO
        [Fact]
        public async Task GetCardProdottiAsync_ShouldReturnOnlySempreDisponibiliBevande()
        {
            // Act
            var result = await _repository.GetCardProdottiAsync();

            // Assert
            Assert.NotNull(result);
            var resultList = result.ToList();
            Assert.Single(resultList); // Solo 1 bevanda con SempreDisponibile = true
            Assert.All(resultList, card => Assert.True(card.SempreDisponibile));
        }

        [Fact]
        public async Task GetCardProdottiAsync_ShouldReturnCardsWithPrezziAndIngredienti()
        {
            // Act
            var result = await _repository.GetCardProdottiAsync();

            // Assert
            var resultList = result.ToList();
            var card = resultList.First();

            Assert.NotNull(card.PrezziPerDimensioni);
            Assert.NotEmpty(card.PrezziPerDimensioni);
            Assert.NotNull(card.Ingredienti);
            Assert.Equal("Premium", card.Nome);
        }

        [Fact]
        public async Task GetCardProdottoByIdAsync_WithValidId_ShouldReturnCardProdotto()
        {
            // Act
            var result = await _repository.GetCardProdottoByIdAsync(2);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.ArticoloId);
            Assert.Equal("Premium", result.Nome);
            Assert.NotNull(result.PrezziPerDimensioni);
            Assert.NotEmpty(result.PrezziPerDimensioni);
        }

        [Fact]
        public async Task GetCardProdottoByIdAsync_WithInvalidId_ShouldReturnNull()
        {
            // Act
            var result = await _repository.GetCardProdottoByIdAsync(999);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetCardProdottoByIdAsync_WithNonSempreDisponibile_ShouldReturnNull()
        {
            // Act - Bevanda 1 non è SempreDisponibile
            var result = await _repository.GetCardProdottoByIdAsync(1);

            // Assert
            Assert.Null(result);
        }

        // AGGIUNGI QUESTI TEST a BevandaStandardRepositoryTest.cs:

        [Fact]
        public async Task GetPrimoPianoAsync_ShouldReturnOnlyDisponibiliAndSempreDisponibiliBevande()
        {
            // Act
            var result = await _repository.GetPrimoPianoAsync();

            // Assert
            Assert.NotNull(result);
            var resultList = result.ToList();
            Assert.Single(resultList); // Solo bevanda 2: Disponibile=true, SempreDisponibile=true
            Assert.All(resultList, bs => Assert.True(bs.Disponibile));
            Assert.All(resultList, bs => Assert.True(bs.SempreDisponibile));
        }

        [Fact]
        public async Task GetSecondoPianoAsync_ShouldReturnOnlyNonDisponibiliAndSempreDisponibiliBevande()
        {
            // Act
            var result = await _repository.GetSecondoPianoAsync();

            // Assert
            Assert.NotNull(result);
            var resultList = result.ToList();
            Assert.Empty(resultList); // Nessuna bevanda con Disponibile=false E SempreDisponibile=true nei dati di test
        }

        [Fact]
        public async Task GetCardProdottiPrimoPianoAsync_ShouldReturnOnlyDisponibiliAndSempreDisponibiliCards()
        {
            // Act
            var result = await _repository.GetCardProdottiPrimoPianoAsync();

            // Assert
            Assert.NotNull(result);
            var resultList = result.ToList();
            Assert.Single(resultList); // Solo bevanda 2
            var card = resultList.First();
            Assert.True(card.Disponibile);
            Assert.True(card.SempreDisponibile);
            Assert.NotNull(card.PrezziPerDimensioni);
            Assert.NotEmpty(card.PrezziPerDimensioni);
            Assert.Equal("Premium", card.Nome);
        }

        [Fact]
        public async Task GetPrimoPianoAsync_ShouldOrderByPrioritaDescending()
        {
            // ✅ USA UN CONTESTO COMPLETAMENTE ISOLATO per questo test
            var options = new DbContextOptionsBuilder<BubbleTeaContext>()
                .UseInMemoryDatabase(databaseName: $"PrimoPianoOrderTest_{Guid.NewGuid()}")
                .Options;

            using var isolatedContext = new BubbleTeaContext(options);
            var isolatedRepository = new BevandaStandardRepository(isolatedContext);

            // ✅ INIZIALIZZA SOLO I DATI NECESSARI
            isolatedContext.Database.EnsureCreated();

            // Crea personalizzazioni necessarie
            var personalizzazioni = new List<Personalizzazione>
            {
                new Personalizzazione
                {
                    PersonalizzazioneId = 1,
                    Nome = "Classica",
                    Descrizione = "Personalizzazione classica",
                    DtCreazione = DateTime.Now
                },
                new Personalizzazione
                {
                    PersonalizzazioneId = 2,
                    Nome = "Premium",
                    Descrizione = "Personalizzazione premium",
                    DtCreazione = DateTime.Now
                }
            };

            // Crea dimensioni bicchiere necessarie
            var dimensioniBicchiere = new List<DimensioneBicchiere>
            {
                new DimensioneBicchiere
                {
                    DimensioneBicchiereId = 1,
                    Sigla = "S",
                    Descrizione = "Piccolo",
                    Capienza = 350.0m,
                    UnitaMisuraId = 1,
                    PrezzoBase = 0.50m,
                    Moltiplicatore = 1.0m
                },
                new DimensioneBicchiere
                {
                    DimensioneBicchiereId = 2,
                    Sigla = "M",
                    Descrizione = "Medio",
                    Capienza = 500.0m,
                    UnitaMisuraId = 1,
                    PrezzoBase = 1.00m,
                    Moltiplicatore = 1.2m
                }
            };

            // Crea articoli per le bevande
            var articoli = new List<Articolo>
            {
                new Articolo { Tipo = "BS", DataCreazione = DateTime.Now, DataAggiornamento = DateTime.Now },
                new Articolo { Tipo = "BS", DataCreazione = DateTime.Now, DataAggiornamento = DateTime.Now },
                new Articolo { Tipo = "BS", DataCreazione = DateTime.Now, DataAggiornamento = DateTime.Now }
            };

            isolatedContext.Personalizzazione.AddRange(personalizzazioni);
            isolatedContext.DimensioneBicchiere.AddRange(dimensioniBicchiere);
            isolatedContext.Articolo.AddRange(articoli);
            await isolatedContext.SaveChangesAsync();

            var articoloId1 = articoli[0].ArticoloId;
            var articoloId2 = articoli[1].ArticoloId;
            var articoloId3 = articoli[2].ArticoloId;

            // ✅ CREA SOLO LE 3 BEVANDE PER QUESTO TEST
            var bevande = new List<BevandaStandard>
            {
                new BevandaStandard
                {
                    ArticoloId = articoloId1,
                    PersonalizzazioneId = 1,
                    DimensioneBicchiereId = 1,
                    Prezzo = 5.00m,
                    Disponibile = true,
                    SempreDisponibile = true,
                    Priorita = 3, // Priorità media
                    DataCreazione = DateTime.Now,
                    DataAggiornamento = DateTime.Now
                },
                new BevandaStandard
                {
                    ArticoloId = articoloId2,
                    PersonalizzazioneId = 2,
                    DimensioneBicchiereId = 2,
                    Prezzo = 6.00m,
                    Disponibile = true,
                    SempreDisponibile = true,
                    Priorita = 1, // Priorità più bassa
                    DataCreazione = DateTime.Now,
                    DataAggiornamento = DateTime.Now
                },
                new BevandaStandard
                {
                    ArticoloId = articoloId3,
                    PersonalizzazioneId = 1,
                    DimensioneBicchiereId = 1,
                    Prezzo = 4.50m,
                    Disponibile = true,
                    SempreDisponibile = true,
                    Priorita = 5, // Priorità più alta
                    DataCreazione = DateTime.Now,
                    DataAggiornamento = DateTime.Now
                }
            };

            isolatedContext.BevandaStandard.AddRange(bevande);
            await isolatedContext.SaveChangesAsync();

            // ✅ DEBUG: Verifica cosa c'è nel database isolato
            var countBefore = await isolatedContext.BevandaStandard.CountAsync();
            Console.WriteLine($"DEBUG - Bevande nel database isolato: {countBefore}");

            // Act
            var result = await isolatedRepository.GetPrimoPianoAsync();

            // Assert
            var resultList = result.ToList();

            // ✅ VERIFICA CHE CI SIANO ESATTAMENTE 3 BEVANDE
            Assert.Equal(3, resultList.Count);

            // ✅ VERIFICA ORDINAMENTO PER PRIORITÀ DESCENDENTE
            Assert.Equal(5, resultList[0].Priorita); // Priorità più alta prima
            Assert.Equal(3, resultList[1].Priorita); // Priorità media
            Assert.Equal(1, resultList[2].Priorita); // Priorità più bassa ultima

            // ✅ VERIFICA CHE TUTTE LE BEVANDE SIANO DISPONIBILI E SEMPRE DISPONIBILI
            Assert.All(resultList, bs => Assert.True(bs.Disponibile));
            Assert.All(resultList, bs => Assert.True(bs.SempreDisponibile));
        }

        [Fact]
        public async Task GetSecondoPianoAsync_ShouldReturnBevandeWhenAvailable()
        {
            // Arrange - Crea una bevanda in secondo piano
            await CleanTableAsync<BevandaStandard>();
            await CleanTableAsync<Articolo>(); // ✅ PULISCI ANCHE ARTICOLI

            // Crea nuovo articolo
            var articolo = new Articolo
            {
                Tipo = "BS",
                DataCreazione = DateTime.Now,
                DataAggiornamento = DateTime.Now
            };
            _context.Articolo.Add(articolo);
            await _context.SaveChangesAsync();

            var bevandaSecondoPiano = new BevandaStandard
            {
                ArticoloId = articolo.ArticoloId, // ✅ USA ArticoloId generato
                PersonalizzazioneId = 1,
                DimensioneBicchiereId = 1,
                Prezzo = 5.00m,
                Disponibile = false, // Secondo piano
                SempreDisponibile = true, // Ma visibile
                Priorita = 1, // ✅ Priorità NOT NULL
                DataCreazione = DateTime.Now,
                DataAggiornamento = DateTime.Now
            };

            _context.BevandaStandard.Add(bevandaSecondoPiano);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetSecondoPianoAsync();

            // Assert
            var resultList = result.ToList();
            Assert.Single(resultList);
            Assert.False(resultList[0].Disponibile);
            Assert.True(resultList[0].SempreDisponibile);
            Assert.Equal(articolo.ArticoloId, resultList[0].ArticoloId);
            Assert.Equal(1, resultList[0].Priorita); // ✅ Verifica priorità
        }

        [Fact]
        public async Task GetCardProdottiPrimoPianoAsync_ShouldNotReturnNonSempreDisponibili()
        {
            // Usa un repository ISOLATO per questo test specifico
            var options = new DbContextOptionsBuilder<BubbleTeaContext>()
                .UseInMemoryDatabase(databaseName: $"Test_Isolated_{Guid.NewGuid()}")
                .Options;

            using var isolatedContext = new BubbleTeaContext(options);
            var isolatedRepository = new BevandaStandardRepository(isolatedContext);

            // Arrange - Setup dati PULITI
            isolatedContext.Database.EnsureCreated();

            var personalizzazione = new Personalizzazione
            {
                PersonalizzazioneId = 100,
                Nome = "Test Non Visibile",
                Descrizione = "Descrizione test",
                DtCreazione = DateTime.Now
            };

            var bevandaNonVisibile = new BevandaStandard
            {
                ArticoloId = 30,
                PersonalizzazioneId = 100,
                DimensioneBicchiereId = 1,
                Prezzo = 5.00m,
                Disponibile = true,
                SempreDisponibile = false, // ❌ NON visibile
                Priorita = 1,
                DataCreazione = DateTime.Now
            };

            isolatedContext.Personalizzazione.Add(personalizzazione);
            isolatedContext.BevandaStandard.Add(bevandaNonVisibile);
            await isolatedContext.SaveChangesAsync();

            // Act
            var result = await isolatedRepository.GetCardProdottiPrimoPianoAsync();

            // Assert
            var resultList = result.ToList();
            Assert.Empty(resultList); // ✅ ORA funziona!
        }
    }
}