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

            // Crea Articoli - solo con i campi esistenti
            var articoli = new List<Articolo>
            {
                new Articolo { ArticoloId = 1, Tipo = "BS", DataCreazione = DateTime.Now, DataAggiornamento = DateTime.Now },
                new Articolo { ArticoloId = 2, Tipo = "BS", DataCreazione = DateTime.Now, DataAggiornamento = DateTime.Now },
                new Articolo { ArticoloId = 3, Tipo = "BS", DataCreazione = DateTime.Now, DataAggiornamento = DateTime.Now },
                new Articolo { ArticoloId = 4, Tipo = "BS", DataCreazione = DateTime.Now, DataAggiornamento = DateTime.Now }
            };

            // Crea Personalizzazioni - con i nomi esatti delle proprietà
            var personalizzazioni = new List<Personalizzazione>
            {
                new Personalizzazione
                {
                    PersonalizzazioneId = 1,
                    Nome = "Classica",
                    Descrizione = "Personalizzazione classica",
                    DtCreazione = DateTime.Now,
                    DtUpdate = DateTime.Now,
                    IsDeleted = false
                },
                new Personalizzazione
                {
                    PersonalizzazioneId = 2,
                    Nome = "Premium",
                    Descrizione = "Personalizzazione premium",
                    DtCreazione = DateTime.Now,
                    DtUpdate = DateTime.Now,
                    IsDeleted = false
                }
            };

            // Crea UnitaDiMisura necessarie per DimensioneBicchiere
            var unitaDiMisura = new List<UnitaDiMisura>
            {
                new UnitaDiMisura { UnitaMisuraId = 1, Sigla = "GR", Descrizione = "grammi" },
                new UnitaDiMisura { UnitaMisuraId = 2, Sigla = "ML", Descrizione = "millilitri" }
            };

            // Crea Dimensioni Bicchiere - solo con i campi esistenti
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

            // Crea Bevande Standard
            var bevandeStandard = new List<BevandaStandard>
            {
                new BevandaStandard
                {
                    ArticoloId = 1,
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
                    ArticoloId = 2,
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
                    ArticoloId = 3,
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
            _context.Articolo.AddRange(articoli);
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
        public async Task GetDisponibiliAsync_ShouldReturnOnlyAvailableBevande()
        {
            // Act
            var result = await _repository.GetDisponibiliAsync();

            // Assert
            Assert.NotNull(result);
            var resultList = result.ToList();
            Assert.Single(resultList); // ✅ Nuova logica - solo 1 bevanda con SempreDisponibile = true
            Assert.All(resultList, bs => Assert.True(bs.SempreDisponibile)); // Solo SempreDisponibile = true
        }

        [Fact]
        public async Task GetDisponibiliAsync_ShouldOrderByPriorita()
        {
            // Act
            var result = await _repository.GetDisponibiliAsync();

            // Assert
            var resultList = result.ToList();
            Assert.Single(resultList); // ✅ Solo 1 bevanda
            Assert.Equal(2, resultList[0].Priorita); // La bevanda 2 ha Priorita = 2 e SempreDisponibile = true
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
            // Act
            var result = await _repository.GetByDimensioneBicchiereAsync(1);

            // Assert
            var resultList = result.ToList();
            Assert.Single(resultList); // Solo la bevanda disponibile con dimensione 1
            Assert.Equal(1, resultList[0].ArticoloId);
            Assert.Equal(1, resultList[0].DimensioneBicchiereId);
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
                ArticoloId = 4,
                PersonalizzazioneId = 1,
                DimensioneBicchiereId = 2,
                Prezzo = 7.00m,
                ImmagineUrl = "url4.jpg",
                Disponibile = true,
                SempreDisponibile = false,
                Priorita = 4
            };

            // Act
            await _repository.AddAsync(newBevanda);

            // Assert
            var result = await _repository.GetByIdAsync(4);
            Assert.NotNull(result);
            Assert.Equal(7.00m, result.Prezzo);
            Assert.True(result.Disponibile);
            Assert.NotNull(result.DataCreazione);
            Assert.NotNull(result.DataAggiornamento);
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
                Priorita = 5
            };

            // Act
            await _repository.UpdateAsync(updateDto);

            // Assert
            var result = await _repository.GetByIdAsync(1);
            Assert.NotNull(result);
            Assert.Equal(6.50m, result.Prezzo);
            Assert.False(result.Disponibile);
            Assert.Equal(5, result.Priorita);
            Assert.Equal("url_updated.jpg", result.ImmagineUrl);
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

        //[Fact]
        //public async Task Debug_BevandaStandardRepository()
        //{
        //    // Crea context isolato
        //    var options = new DbContextOptionsBuilder<BubbleTeaContext>()
        //        .UseInMemoryDatabase(databaseName: $"Test_Repo_Debug_{Guid.NewGuid()}")
        //        .Options;

        //    using var context = new BubbleTeaContext(options);
        //    context.Database.EnsureCreated();

        //    // Crea dati
        //    var articolo = new Articolo { ArticoloId = 1, Tipo = "BS" };
        //    var bevanda = new BevandaStandard
        //    {
        //        ArticoloId = 1,
        //        Prezzo = 3.50m,
        //        PersonalizzazioneId = 1,
        //        DimensioneBicchiereId = 1,
        //        Disponibile = true
        //    };

        //    context.Articolo.Add(articolo);
        //    context.BevandaStandard.Add(bevanda);
        //    await context.SaveChangesAsync();

        //    Console.WriteLine("=== DEBUG REPOSITORY ===");

        //    // 1. Verifica dati nel context
        //    var bevandaNelContext = context.BevandaStandard.FirstOrDefault(bs => bs.ArticoloId == 1);
        //    Console.WriteLine($"Bevanda nel context: {bevandaNelContext != null}");

        //    // 2. Test repository
        //    var repo = new BevandaStandardRepository(context);
        //    var risultatoRepo = await repo.GetByIdAsync(1);
        //    Console.WriteLine($"Repository.GetByIdAsync(1): {risultatoRepo != null}");

        //    // 3. Se è null, verifica cosa cerca il repository
        //    if (risultatoRepo == null)
        //    {
        //        Console.WriteLine("❌ REPOSITORY NON TROVA I DATI!");

        //        // Verifica la query manualmente
        //        var queryManuale = await context.BevandaStandard
        //            .AsNoTracking()
        //            .Include(bs => bs.Articolo)
        //            .Include(bs => bs.Personalizzazione)
        //            .Include(bs => bs.DimensioneBicchiere)
        //            .FirstOrDefaultAsync(bs => bs.ArticoloId == 1);

        //        Console.WriteLine($"Query manuale: {queryManuale != null}");

        //        if (queryManuale == null)
        //        {
        //            Console.WriteLine("⚠️  ANCHE LA QUERY MANUALE NON TROVA I DATI!");
        //            Console.WriteLine($"Bevande nel DB: {context.BevandaStandard.Count()}");
        //            Console.WriteLine($"Articoli nel DB: {context.Articolo.Count()}");
        //        }
        //    }

        //    Assert.NotNull(risultatoRepo);
        //}
    }
}