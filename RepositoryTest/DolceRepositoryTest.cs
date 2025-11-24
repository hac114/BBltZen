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
    public class DolceRepositoryTest : BaseTest
    {
        private readonly DolceRepository _repository;        

        public DolceRepositoryTest()
        {            
            _repository = new DolceRepository(_context);

            InitializeTestData();
        }

        private void InitializeTestData()
        {
            _context.Database.EnsureDeleted();
            _context.Database.EnsureCreated();

            // Crea Articoli
            var articoli = new List<Articolo>
            {
                new Articolo { ArticoloId = 1, Tipo = "DOLCE", DataCreazione = DateTime.Now, DataAggiornamento = DateTime.Now },
                new Articolo { ArticoloId = 2, Tipo = "DOLCE", DataCreazione = DateTime.Now, DataAggiornamento = DateTime.Now },
                new Articolo { ArticoloId = 3, Tipo = "DOLCE", DataCreazione = DateTime.Now, DataAggiornamento = DateTime.Now },
                new Articolo { ArticoloId = 4, Tipo = "DOLCE", DataCreazione = DateTime.Now, DataAggiornamento = DateTime.Now }
            };

            // Crea Dolci
            var dolci = new List<Dolce>
            {
                new Dolce
                {
                    ArticoloId = 1,
                    Nome = "Tiramisù",
                    Prezzo = 4.50m,
                    Descrizione = "Classico tiramisù italiano",
                    ImmagineUrl = "tiramisu.jpg",
                    Disponibile = true,
                    Priorita = 1,
                    DataCreazione = DateTime.Now.AddDays(-10),
                    DataAggiornamento = DateTime.Now.AddDays(-1)
                },
                new Dolce
                {
                    ArticoloId = 2,
                    Nome = "Cheesecake",
                    Prezzo = 5.00m,
                    Descrizione = "Cheesecake ai frutti di bosco",
                    ImmagineUrl = "cheesecake.jpg",
                    Disponibile = true,
                    Priorita = 2,
                    DataCreazione = DateTime.Now.AddDays(-5),
                    DataAggiornamento = DateTime.Now
                },
                new Dolce
                {
                    ArticoloId = 3,
                    Nome = "Panna Cotta",
                    Prezzo = 3.50m,
                    Descrizione = "Panna cotta al caramello",
                    ImmagineUrl = "panna_cotta.jpg",
                    Disponibile = false,
                    Priorita = 3,
                    DataCreazione = DateTime.Now.AddDays(-3),
                    DataAggiornamento = DateTime.Now.AddDays(-1)
                }
            };

            _context.Articolo.AddRange(articoli);
            _context.Dolce.AddRange(dolci);
            _context.SaveChanges();
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnAllDolci()
        {
            // Act
            var result = await _repository.GetAllAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, result.Count());
        }

        [Fact]
        public async Task GetByIdAsync_WithValidId_ShouldReturnDolce()
        {
            // Act
            var result = await _repository.GetByIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.ArticoloId);
            Assert.Equal("Tiramisù", result.Nome);
            Assert.Equal(4.50m, result.Prezzo);
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
        public async Task GetDisponibiliAsync_ShouldReturnOnlyAvailableDolci()
        {
            // Act
            var result = await _repository.GetDisponibiliAsync();

            // Assert
            var resultList = result.ToList();
            Assert.Equal(2, resultList.Count);
            Assert.All(resultList, d => Assert.True(d.Disponibile));
        }

        [Fact]
        public async Task GetByPrioritaAsync_ShouldReturnFilteredDolci()
        {
            // Act
            var result = await _repository.GetByPrioritaAsync(1);

            // Assert
            var resultList = result.ToList();
            Assert.Single(resultList);
            Assert.Equal(1, resultList[0].Priorita);
            Assert.Equal("Tiramisù", resultList[0].Nome);
        }

        [Fact]
        public async Task AddAsync_ShouldAddNewDolce()
        {
            // Arrange
            var newDolce = new DolceDTO
            {
                Nome = "Torta al Cioccolato",
                Prezzo = 4.00m,
                Descrizione = "Torta al cioccolato fondente",
                ImmagineUrl = "torta_cioccolato.jpg",
                Disponibile = true,
                Priorita = 4
            };

            // Act
            var result = await _repository.AddAsync(newDolce); // ✅ CORREGGI: assegna risultato

            // Assert
            // ✅ Ora usa result.ArticoloId che è stato generato dal repository
            Assert.True(result.ArticoloId > 0);
            var savedDolce = await _repository.GetByIdAsync(result.ArticoloId);
            Assert.NotNull(savedDolce);
            Assert.Equal("Torta al Cioccolato", savedDolce.Nome);
            Assert.Equal(4.00m, savedDolce.Prezzo);
            Assert.True(savedDolce.Disponibile);
            Assert.Equal(4, savedDolce.Priorita);

            // ✅ OPZIONALE: Verifica che le date siano state impostate
            Assert.NotEqual(default(DateTime), savedDolce.DataCreazione);
            Assert.NotEqual(default(DateTime), savedDolce.DataAggiornamento);
        }

        [Fact]
        public async Task UpdateAsync_ShouldUpdateExistingDolce()
        {
            // Arrange
            var updateDto = new DolceDTO
            {
                ArticoloId = 1,
                Nome = "Tiramisù Speciale",
                Prezzo = 5.00m,
                Descrizione = "Tiramisù rivisitato",
                ImmagineUrl = "tiramisu_speciale.jpg",
                Disponibile = false,
                Priorita = 5
            };

            // Act
            await _repository.UpdateAsync(updateDto);

            // Assert
            var result = await _repository.GetByIdAsync(1);
            Assert.NotNull(result);
            Assert.Equal("Tiramisù Speciale", result.Nome);
            Assert.Equal(5.00m, result.Prezzo);
            Assert.False(result.Disponibile);
            Assert.Equal(5, result.Priorita);
            
            // ✅ VERIFICA CHE LA DATA SIA STATA AGGIORNATA
            Assert.NotEqual(default(DateTime), result.DataAggiornamento);
        }

        [Fact]
        public async Task DeleteAsync_ShouldRemoveDolce()
        {
            // Act
            await _repository.DeleteAsync(1);

            // Assert
            var result = await _repository.GetByIdAsync(1);
            Assert.Null(result);
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
        public async Task AddAsync_WithMinimumPriorita_ShouldWorkCorrectly()
        {
            // Arrange
            var newDolce = new DolceDTO
            {
                Nome = "Biscotti",
                Prezzo = 2.50m,
                Disponibile = true,
                Priorita = 0 // ✅ Priorità minima consentita
            };

            // Act
            var result = await _repository.AddAsync(newDolce);

            // Assert
            var savedDolce = await _repository.GetByIdAsync(result.ArticoloId);
            Assert.NotNull(savedDolce);
            Assert.Equal(0, savedDolce.Priorita); // ✅ Verifica priorità zero
            Assert.Equal("Biscotti", savedDolce.Nome);
        }

        [Fact]
        public async Task AddAsync_WithMaximumPriorita_ShouldWorkCorrectly()
        {
            // Arrange
            var newDolce = new DolceDTO
            {
                Nome = "Torta Speciale",
                Prezzo = 6.00m,
                Disponibile = true,
                Priorita = 10 // ✅ Priorità massima consentita
            };

            // Act
            var result = await _repository.AddAsync(newDolce);

            // Assert
            var savedDolce = await _repository.GetByIdAsync(result.ArticoloId);
            Assert.NotNull(savedDolce);
            Assert.Equal(10, savedDolce.Priorita); // ✅ Verifica priorità massima
        }

        [Fact]
        public async Task GetDisponibiliAsync_ShouldOrderByPrioritaThenByName()
        {
            // Act
            var result = await _repository.GetDisponibiliAsync();

            // Assert
            var resultList = result.ToList();
            Assert.Equal(2, resultList.Count);

            // ✅ Verifica ordinamento: prima per priorità, poi per nome
            Assert.Equal(1, resultList[0].Priorita); // Tiramisù - Priorità 1
            Assert.Equal(2, resultList[1].Priorita); // Cheesecake - Priorità 2
            Assert.Equal("Tiramisù", resultList[0].Nome);
            Assert.Equal("Cheesecake", resultList[1].Nome);
        }

        [Fact]
        public async Task UpdateAsync_ShouldUpdateDataAggiornamento()
        {
            // Arrange
            var existing = await _repository.GetByIdAsync(1);
            Assert.NotNull(existing);
            var originalUpdateTime = existing.DataAggiornamento;

            var updateDto = new DolceDTO
            {
                ArticoloId = 1,
                Nome = existing.Nome,
                Prezzo = existing.Prezzo + 1.00m, // Modifica solo il prezzo
                Descrizione = existing.Descrizione,
                ImmagineUrl = existing.ImmagineUrl,
                Disponibile = existing.Disponibile,
                Priorita = existing.Priorita
            };

            // Act
            await _repository.UpdateAsync(updateDto);

            // Assert
            var result = await _repository.GetByIdAsync(1);
            Assert.NotNull(result);
            Assert.True(result.DataAggiornamento > originalUpdateTime); // ✅ Verifica che DataAggiornamento sia aggiornata
            Assert.Equal(existing.Prezzo + 1.00m, result.Prezzo);
        }
    }
}
