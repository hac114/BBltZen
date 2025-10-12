﻿using Database;
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
        private readonly BubbleTeaContext _context;

        public DolceRepositoryTest()
        {
            var options = new DbContextOptionsBuilder<BubbleTeaContext>()
                .UseInMemoryDatabase(databaseName: $"DolceTest_{Guid.NewGuid()}")
                .Options;

            _context = new BubbleTeaContext(options);
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
            await _repository.AddAsync(newDolce);

            // Assert
            Assert.True(newDolce.ArticoloId > 0);
            var result = await _repository.GetByIdAsync(newDolce.ArticoloId);
            Assert.NotNull(result);
            Assert.Equal("Torta al Cioccolato", result.Nome);
            Assert.Equal(4.00m, result.Prezzo);
            Assert.NotNull(result.DataCreazione);
            Assert.NotNull(result.DataAggiornamento);
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
    }
}
