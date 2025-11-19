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
    public class ArticoloRepositoryTest : BaseTest
    {
        private readonly ArticoloRepository _repository;
        private readonly new BubbleTeaContext _context;

        public ArticoloRepositoryTest()
        {
            var options = new DbContextOptionsBuilder<BubbleTeaContext>()
                .UseInMemoryDatabase(databaseName: $"DolceTest_{Guid.NewGuid()}")
                .Options;

            _context = new BubbleTeaContext(options);
            _repository = new ArticoloRepository(_context);

            InitializeTestData();
        }

        private void InitializeTestData()
        {
            _context.Database.EnsureDeleted();
            _context.Database.EnsureCreated();

            // ✅ CREA CATEGORIE SE MANCANTI (per evitare errori foreign key)
            if (!_context.CategoriaIngrediente.Any())
            {
                _context.CategoriaIngrediente.AddRange(
                    new CategoriaIngrediente { CategoriaId = 1, Categoria = "tea" },
                    new CategoriaIngrediente { CategoriaId = 2, Categoria = "latte" },
                    new CategoriaIngrediente { CategoriaId = 3, Categoria = "dolcificante" },
                    new CategoriaIngrediente { CategoriaId = 4, Categoria = "topping" }
                );
                _context.SaveChanges();
            }

            // ✅ CREA UNITÀ DI MISURA SE MANCANTI
            if (!_context.UnitaDiMisura.Any())
            {
                _context.UnitaDiMisura.AddRange(
                    new UnitaDiMisura { UnitaMisuraId = 1, Sigla = "ML", Descrizione = "Millilitri" },
                    new UnitaDiMisura { UnitaMisuraId = 2, Sigla = "GR", Descrizione = "Grammi" }
                );
                _context.SaveChanges();
            }

            // ✅ CREA INGREDIENTI PER TEST DISPONIBILITÀ (COMPLETI DI TUTTI I CAMPI)
            var ingredienti = new List<Ingrediente>
            {
                new Ingrediente {
                    IngredienteId = 1,
                    Ingrediente1 = "Tea Nero",
                    CategoriaId = 1,
                    Disponibile = true,
                    PrezzoAggiunto = 0.50m,
                    DataInserimento = DateTime.Now,
                    DataAggiornamento = DateTime.Now
                },
                new Ingrediente {
                    IngredienteId = 2,
                    Ingrediente1 = "Latte",
                    CategoriaId = 2,
                    Disponibile = true,
                    PrezzoAggiunto = 0.30m,
                    DataInserimento = DateTime.Now,
                    DataAggiornamento = DateTime.Now
                },
                new Ingrediente {
                    IngredienteId = 3,
                    Ingrediente1 = "Perle Esaurite",
                    CategoriaId = 4,
                    Disponibile = false,
                    PrezzoAggiunto = 1.00m,
                    DataInserimento = DateTime.Now,
                    DataAggiornamento = DateTime.Now
                }
            };
            _context.Ingrediente.AddRange(ingredienti);
            _context.SaveChanges();

            // ✅ CREA PERSONALIZZAZIONI CON TUTTI I CAMPI OBBLIGATORI
            var personalizzazioni = new List<Personalizzazione>
            {
                new Personalizzazione {
                    PersonalizzazioneId = 1,
                    Nome = "Classic Tea",
                    Descrizione = "Tè classico con latte", // ✅ CAMPO OBBLIGATORIO
                    DtCreazione = DateTime.Now
                },
                new Personalizzazione {
                    PersonalizzazioneId = 2,
                    Nome = "Special Mix",
                    Descrizione = "Mix speciale di ingredienti", // ✅ CAMPO OBBLIGATORIO
                    DtCreazione = DateTime.Now
                }
            };
            _context.Personalizzazione.AddRange(personalizzazioni);
            _context.SaveChanges();

            // ✅ CREA PERSONALIZZAZIONE INGREDIENTI
            var personalizzazioneIngredienti = new List<PersonalizzazioneIngrediente>
            {
                new PersonalizzazioneIngrediente {
                    PersonalizzazioneIngredienteId = 1,
                    PersonalizzazioneId = 1,
                    IngredienteId = 1,
                    Quantita = 10,
                    UnitaMisuraId = 1
                },
                new PersonalizzazioneIngrediente {
                    PersonalizzazioneIngredienteId = 2,
                    PersonalizzazioneId = 2,
                    IngredienteId = 3,
                    Quantita = 15,
                    UnitaMisuraId = 1
                }
            };
            _context.PersonalizzazioneIngrediente.AddRange(personalizzazioneIngredienti);
            _context.SaveChanges();

            // ✅ CREA ARTICOLI CON SPECIALIZZAZIONI COMPLETE
            var articoli = new List<Articolo>
            {
                new Articolo
                {
                    ArticoloId = 1,
                    Tipo = "BS",
                    DataCreazione = DateTime.Now.AddDays(-10),
                    DataAggiornamento = DateTime.Now.AddDays(-1),
                    BevandaStandard = new BevandaStandard
                    {
                        Disponibile = true,
                        SempreDisponibile = false,
                        PersonalizzazioneId = 1,
                        DimensioneBicchiereId = 1,
                        ImmagineUrl = "www.immagine/BubbleTea.jpg",
                        Prezzo = 4.50m,
                        Priorita = 1,
                        DataCreazione = DateTime.Now,
                        DataAggiornamento = DateTime.Now
                    }
                },
                new Articolo
                {
                    ArticoloId = 2,
                    Tipo = "BS",
                    DataCreazione = DateTime.Now.AddDays(-5),
                    DataAggiornamento = DateTime.Now,
                    BevandaStandard = new BevandaStandard
                    {
                        Disponibile = false,
                        SempreDisponibile = true,
                        PersonalizzazioneId = 2,
                        DimensioneBicchiereId = 1,
                        ImmagineUrl = "www.immagine/BubbleTea.jpg",
                        Prezzo = 5.50m,
                        Priorita = 1,
                        DataCreazione = DateTime.Now,
                        DataAggiornamento = DateTime.Now
                    }
                },
                new Articolo
                {
                    ArticoloId = 3,
                    Tipo = "BS",
                    DataCreazione = DateTime.Now.AddDays(-3),
                    DataAggiornamento = DateTime.Now.AddDays(-1),
                    BevandaStandard = new BevandaStandard
                    {
                        Disponibile = true,
                        SempreDisponibile = true,
                        PersonalizzazioneId = 1,
                        DimensioneBicchiereId = 2,
                        ImmagineUrl = "www.immagine/BubbleTea.jpg",
                        Prezzo = 4.50m,
                        Priorita = 1,
                        DataCreazione = DateTime.Now,
                        DataAggiornamento = DateTime.Now
                    }
                },
                new Articolo
                {
                    ArticoloId = 4,
                    Tipo = "BS",
                    DataCreazione = DateTime.Now.AddDays(-3),
                    DataAggiornamento = DateTime.Now.AddDays(-1),
                    BevandaStandard = new BevandaStandard
                    {
                        Disponibile = false,
                        SempreDisponibile = false,
                        PersonalizzazioneId = 1,
                        DimensioneBicchiereId = 2,
                        ImmagineUrl = "www.immagine/BubbleTea.jpg",
                        Prezzo = 6.50m,
                        Priorita = 1,
                        DataCreazione = DateTime.Now,
                        DataAggiornamento = DateTime.Now
                    }
                },
                // ✅ Bevanda Custom - Sempre ordinabile
                new Articolo
                {
                    ArticoloId = 5,
                    Tipo = "BC",
                    DataCreazione = DateTime.Now.AddDays(-2),
                    DataAggiornamento = DateTime.Now,
                    BevandaCustom = new List<BevandaCustom>
                    {
                        new BevandaCustom {
                            PersCustomId = 1,
                            Prezzo = 5.00m,
                            DataCreazione = DateTime.Now,
                            DataAggiornamento = DateTime.Now
                        }
                    }
                },
                // ✅ Dolce - Disponibile
                new Articolo
                {
                    ArticoloId = 6,
                    Tipo = "D",
                    DataCreazione = DateTime.Now.AddDays(-1),
                    DataAggiornamento = DateTime.Now,
                    Dolce = new Dolce {
                        Disponibile = true,
                        Nome = "Tiramisù",
                        Prezzo = 4.50m,
                        DataCreazione = DateTime.Now,
                        DataAggiornamento = DateTime.Now
                    }
                },
                // ❌ Dolce - Non disponibile
                new Articolo
                {
                    ArticoloId = 7,
                    Tipo = "D",
                    DataCreazione = DateTime.Now.AddDays(-1),
                    DataAggiornamento = DateTime.Now,
                    Dolce = new Dolce {
                        Disponibile = false,
                        Nome = "Cheesecake",
                        Prezzo = 5.00m,
                        DataCreazione = DateTime.Now,
                        DataAggiornamento = DateTime.Now
                    }
                }
            };

            _context.Articolo.AddRange(articoli);
            _context.SaveChanges();
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnAllArticoli()
        {
            var result = await _repository.GetAllAsync();
            Assert.NotNull(result);
            Assert.Equal(7, result.Count()); // ✅ CORRETTO: 7 articoli totali
        }

        [Fact]
        public async Task GetByIdAsync_WithValidId_ShouldReturnArticolo()
        {
            // Act
            var result = await _repository.GetByIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.ArticoloId);
            Assert.Equal("BS", result.Tipo);
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
        public async Task GetByTipoAsync_ShouldReturnFilteredArticoli()
        {
            var result = await _repository.GetByTipoAsync("BS");
            var resultList = result.ToList();

            Assert.Equal(4, resultList.Count); // ✅ CORRETTO: 4 BS (articoli 1,2,3,4)
            Assert.All(resultList, a => Assert.Equal("BS", a.Tipo));
        }

        [Fact]
        public async Task GetByTipoAsync_WithInvalidTipo_ShouldReturnEmpty()
        {
            // Act
            var result = await _repository.GetByTipoAsync("INVALIDO");

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetArticoliOrdinabiliAsync_ShouldReturnOnlyAvailableItems()
        {
            // Act
            var result = await _repository.GetArticoliOrdinabiliAsync();

            // Assert
            var resultList = result.ToList();
            Assert.NotNull(resultList);

            // ✅ ARTICOLI ORDINABILI (4 totali):
            // - Articolo 2: BS con SempreDisponibile=true
            // - Articolo 3: BS con SempreDisponibile=true  
            // - Articolo 5: BC sempre ordinabile
            // - Articolo 6: D con Disponibile=true

            Assert.Equal(4, resultList.Count);

            var articoloIds = resultList.Select(a => a.ArticoloId).ToList();
            Assert.Contains(2, articoloIds);
            Assert.Contains(3, articoloIds);
            Assert.Contains(5, articoloIds);
            Assert.Contains(6, articoloIds);
        }

        [Fact]
        public async Task GetBevandeStandardDisponibiliAsync_ShouldReturnOnlyAvailableBevandeStandard()
        {
            // Act
            var result = await _repository.GetBevandeStandardDisponibiliAsync();

            // Assert
            var resultList = result.ToList();

            Assert.NotNull(resultList);

            // ✅ BEVANDE STANDARD ORDINABILI (SempreDisponibile = true):
            // - Articolo 2: BS con SempreDisponibile=true (Disponibile=false)
            // - Articolo 3: BS con SempreDisponibile=true (Disponibile=true)

            // ❌ BEVANDE STANDARD NON ORDINABILI:
            // - Articolo 1: BS con SempreDisponibile=false
            // - Articolo 4: BS con SempreDisponibile=false

            Assert.Equal(2, resultList.Count);

            var articoloIds = resultList.Select(a => a.ArticoloId).ToList();

            // ✅ Verifica che siano presenti solo le bevande standard con SempreDisponibile=true
            Assert.Contains(2, articoloIds); // BS - SempreDisponibile=true
            Assert.Contains(3, articoloIds); // BS - SempreDisponibile=true

            // ✅ Verifica che siano esclusi quelli con SempreDisponibile=false
            Assert.DoesNotContain(1, articoloIds); // ❌ SempreDisponibile=false
            Assert.DoesNotContain(4, articoloIds); // ❌ SempreDisponibile=false

            // ✅ Verifica che siano solo di tipo BS
            Assert.All(resultList, a => Assert.Equal("BS", a.Tipo));
        }

        [Fact]
        public async Task AddAsync_ShouldAddNewArticolo()
        {
            // Arrange
            var newArticolo = new ArticoloDTO
            {
                Tipo = "BC"
            };

            // Act
            var result = await _repository.AddAsync(newArticolo);

            // Assert
            Assert.True(result.ArticoloId > 0); // ✅ USA result
            var savedArticolo = await _repository.GetByIdAsync(result.ArticoloId); // ✅ USA result
            Assert.NotNull(savedArticolo);
            Assert.Equal("BC", savedArticolo.Tipo);
            Assert.NotNull(savedArticolo.DataCreazione);
            Assert.NotNull(savedArticolo.DataAggiornamento);
        }

        [Fact]
        public async Task UpdateAsync_ShouldUpdateExistingArticolo()
        {
            // Arrange
            var updateDto = new ArticoloDTO
            {
                ArticoloId = 1,
                Tipo = "D"
            };

            // Act
            await _repository.UpdateAsync(updateDto);

            // Assert
            var result = await _repository.GetByIdAsync(1);
            Assert.NotNull(result);
            Assert.Equal("D", result.Tipo);
        }

        [Fact]
        public async Task DeleteAsync_ShouldRemoveArticolo()
        {
            // Arrange - Crea un articolo SENZA specializzazioni (orphan) per test di eliminazione
            var articoloToDelete = new Articolo
            {
                ArticoloId = 100,
                Tipo = "BC", // Bevanda Custom - ma SENZA BevandaCustom associata
                DataCreazione = DateTime.Now,
                DataAggiornamento = DateTime.Now
                // ✅ INTENZIONALMENTE senza BevandaCustom, Dolce, BevandaStandard
            };

            _context.Articolo.Add(articoloToDelete);
            await _context.SaveChangesAsync();

            // Act
            await _repository.DeleteAsync(100);

            // Assert
            var result = await _repository.GetByIdAsync(100);
            Assert.Null(result);
        }

        [Fact]
        public async Task DeleteAsync_ShouldRemoveArticoloWithBevandaStandard()
        {
            // Arrange - Articolo CON BevandaStandard (test cascade manuale)
            var articoloToDelete = new Articolo
            {
                ArticoloId = 101,
                Tipo = "BS",
                DataCreazione = DateTime.Now,
                DataAggiornamento = DateTime.Now,
                BevandaStandard = new BevandaStandard()
                {
                    Disponibile = true,
                    SempreDisponibile = true,
                    Prezzo = 5.00m,
                    DataCreazione = DateTime.Now,
                    DataAggiornamento = DateTime.Now
                }
            };

            _context.Articolo.Add(articoloToDelete);
            await _context.SaveChangesAsync();

            // Act
            await _repository.DeleteAsync(101);

            // Assert
            var articoloResult = await _repository.GetByIdAsync(101);
            Assert.Null(articoloResult);

            var bevandaStandardResult = await _context.BevandaStandard
                .FirstOrDefaultAsync(bs => bs.ArticoloId == 101);
            Assert.Null(bevandaStandardResult); // ✅ Verifica che anche BevandaStandard sia eliminata
        }

        [Fact]
        public async Task DeleteAsync_ShouldRemoveArticoloWithDolce()
        {
            // Arrange - Articolo CON Dolce (test cascade manuale)
            var articoloToDelete = new Articolo
            {
                ArticoloId = 102,
                Tipo = "D",
                DataCreazione = DateTime.Now,
                DataAggiornamento = DateTime.Now,
                Dolce = new Dolce()
                {
                    Nome = "Test Dolce",
                    Prezzo = 4.50m,
                    Disponibile = true,
                    DataCreazione = DateTime.Now,
                    DataAggiornamento = DateTime.Now
                }
            };

            _context.Articolo.Add(articoloToDelete);
            await _context.SaveChangesAsync();

            // Act
            await _repository.DeleteAsync(102);

            // Assert
            var articoloResult = await _repository.GetByIdAsync(102);
            Assert.Null(articoloResult);

            var dolceResult = await _context.Dolce
                .FirstOrDefaultAsync(d => d.ArticoloId == 102);
            Assert.Null(dolceResult); // ✅ Verifica che anche Dolce sia eliminato
        }

        [Fact]
        public async Task DeleteAsync_ShouldRemoveArticoloWithBevandaCustom()
        {
            // Arrange - Articolo CON BevandaCustom (test cascade manuale)
            var articoloToDelete = new Articolo
            {
                ArticoloId = 103,
                Tipo = "BC",
                DataCreazione = DateTime.Now,
                DataAggiornamento = DateTime.Now,
                BevandaCustom = new List<BevandaCustom>()
        {
            new BevandaCustom()
            {
                Prezzo = 6.00m,
                DataCreazione = DateTime.Now,
                DataAggiornamento = DateTime.Now
            }
        }
            };

            _context.Articolo.Add(articoloToDelete);
            await _context.SaveChangesAsync();

            // Act
            await _repository.DeleteAsync(103);

            // Assert
            var articoloResult = await _repository.GetByIdAsync(103);
            Assert.Null(articoloResult);

            var bevandaCustomResult = await _context.BevandaCustom
                .FirstOrDefaultAsync(bc => bc.ArticoloId == 103);
            Assert.Null(bevandaCustomResult); // ✅ Verifica che anche BevandaCustom sia eliminata
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
        public async Task ExistsByTipoAsync_WithExistingTipo_ShouldReturnTrue()
        {
            // Act
            var result = await _repository.ExistsByTipoAsync("BS");

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task ExistsByTipoAsync_WithNonExistingTipo_ShouldReturnFalse()
        {
            // Act
            var result = await _repository.ExistsByTipoAsync("INVALIDO");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task ExistsByTipoAsync_WithExcludeId_ShouldReturnCorrectResult()
        {
            // Act
            var result = await _repository.ExistsByTipoAsync("BS", 1);

            // Assert
            Assert.True(result); // Dovrebbe esistere ancora l'articolo 2 e 3 con tipo "BS"
        }

        [Fact]
        public async Task GetDolciDisponibiliAsync_ShouldReturnOnlyAvailableDolci()
        {
            var result = await _repository.GetDolciDisponibiliAsync();
            var resultList = result.ToList();

            Assert.NotNull(resultList);
            Assert.Single(resultList); // Solo l'articolo 6 (Tiramisù) è disponibile
            Assert.Equal(6, resultList[0].ArticoloId); // ✅ CORRETTO: articolo 6
        }

        [Fact]
        public async Task GetIngredientiDisponibiliPerBevandaCustomAsync_ShouldReturnOnlyAvailableIngredients()
        {
            var result = await _repository.GetIngredientiDisponibiliPerBevandaCustomAsync();
            var resultList = result.ToList();

            Assert.NotNull(resultList);
            Assert.Equal(2, resultList.Count); // Solo 2 ingredienti disponibili (Tea Nero e Latte)
            Assert.All(resultList, i => Assert.True(i.Disponibile));
        }

        [Fact]
        public async Task GetBevandeCustomBaseAsync_ShouldReturnBevandeCustom()
        {
            var result = await _repository.GetBevandeCustomBaseAsync();
            var resultList = result.ToList();

            Assert.NotNull(resultList);
            Assert.Single(resultList); // Solo l'articolo 5
            Assert.Equal(5, resultList[0].ArticoloId); // ✅ CORRETTO: articolo 5
        }

        [Fact]
        public async Task GetAllArticoliCompletoAsync_ShouldReturnAllArticoliWithDetails()
        {
            var result = await _repository.GetAllArticoliCompletoAsync();
            var resultList = result.ToList();

            Assert.NotNull(resultList);
            Assert.Equal(7, resultList.Count); // ✅ CORRETTO: 7 articoli totali
        }
    }
}