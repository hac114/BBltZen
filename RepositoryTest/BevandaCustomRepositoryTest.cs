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
    public class BevandaCustomRepositoryTest : BaseTest
    {
        private readonly BevandaCustomRepository _repository;
        private readonly BubbleTeaContext _context;

        public BevandaCustomRepositoryTest()
        {
            var options = new DbContextOptionsBuilder<BubbleTeaContext>()
                .UseInMemoryDatabase(databaseName: $"BevandaCustomTest_{Guid.NewGuid()}")
                .Options;

            _context = new BubbleTeaContext(options);
            _repository = new BevandaCustomRepository(_context);

            InitializeTestData();
        }

        private void InitializeTestData()
        {
            _context.Database.EnsureDeleted();
            _context.Database.EnsureCreated();

            // Crea Articoli
            var articoli = new List<Articolo>
            {
                new Articolo { ArticoloId = 1, Tipo = "BC", DataCreazione = DateTime.Now, DataAggiornamento = DateTime.Now },
                new Articolo { ArticoloId = 2, Tipo = "BC", DataCreazione = DateTime.Now, DataAggiornamento = DateTime.Now },
                new Articolo { ArticoloId = 3, Tipo = "BC", DataCreazione = DateTime.Now, DataAggiornamento = DateTime.Now }
            };

            // Crea Personalizzazioni Custom
            var personalizzazioniCustom = new List<PersonalizzazioneCustom>
            {
                new PersonalizzazioneCustom
                {
                    PersCustomId = 1,
                    Nome = "Dolce Leggero",
                    GradoDolcezza = 2,
                    DimensioneBicchiereId = 1,
                    DataCreazione = DateTime.Now,
                    DataAggiornamento = DateTime.Now
                },
                new PersonalizzazioneCustom
                {
                    PersCustomId = 2,
                    Nome = "Molto Dolce",
                    GradoDolcezza = 5,
                    DimensioneBicchiereId = 2,
                    DataCreazione = DateTime.Now,
                    DataAggiornamento = DateTime.Now
                }
            };

            // Crea Bevande Custom
            var bevandeCustom = new List<BevandaCustom>
            {
                new BevandaCustom
                {
                    BevandaCustomId = 1,
                    ArticoloId = 1,
                    PersCustomId = 1,
                    Prezzo = 5.50m,
                    DataCreazione = DateTime.Now.AddDays(-10),
                    DataAggiornamento = DateTime.Now.AddDays(-1)
                },
                new BevandaCustom
                {
                    BevandaCustomId = 2,
                    ArticoloId = 2,
                    PersCustomId = 2,
                    Prezzo = 6.50m,
                    DataCreazione = DateTime.Now.AddDays(-5),
                    DataAggiornamento = DateTime.Now
                },
                new BevandaCustom
                {
                    BevandaCustomId = 3,
                    ArticoloId = 3,
                    PersCustomId = 1,
                    Prezzo = 4.50m,
                    DataCreazione = DateTime.Now.AddDays(-3),
                    DataAggiornamento = DateTime.Now.AddDays(-1)
                }
            };

            _context.Articolo.AddRange(articoli);
            _context.PersonalizzazioneCustom.AddRange(personalizzazioniCustom);
            _context.BevandaCustom.AddRange(bevandeCustom);
            _context.SaveChanges();
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnAllBevandeCustom()
        {
            // Act
            var result = await _repository.GetAllAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, result.Count());
        }

        [Fact]
        public async Task GetByIdAsync_WithValidId_ShouldReturnBevandaCustom()
        {
            // Act
            var result = await _repository.GetByIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.BevandaCustomId);
            Assert.Equal(1, result.ArticoloId);
            Assert.Equal(5.50m, result.Prezzo);
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
        public async Task GetByArticoloIdAsync_WithValidId_ShouldReturnBevandaCustom()
        {
            // Act
            var result = await _repository.GetByArticoloIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.ArticoloId);
            Assert.Equal(1, result.BevandaCustomId);
        }

        [Fact]
        public async Task GetByPersCustomIdAsync_ShouldReturnFilteredResults()
        {
            // Act
            var result = await _repository.GetByPersCustomIdAsync(1);

            // Assert
            var resultList = result.ToList();
            Assert.Equal(2, resultList.Count);
            Assert.All(resultList, bc => Assert.Equal(1, bc.PersCustomId));
        }

        [Fact]
        public async Task AddAsync_ShouldAddNewBevandaCustom()
        {
            // Arrange
            var newBevandaCustom = new BevandaCustomDTO
            {
                BevandaCustomId = 4,
                ArticoloId = 4,
                PersCustomId = 2,
                Prezzo = 7.00m
            };

            // Act
            await _repository.AddAsync(newBevandaCustom);

            // Assert
            var result = await _repository.GetByIdAsync(4);
            Assert.NotNull(result);
            Assert.Equal(7.00m, result.Prezzo);
            Assert.Equal(2, result.PersCustomId);
            Assert.NotNull(result.DataCreazione);
            Assert.NotNull(result.DataAggiornamento);
        }

        [Fact]
        public async Task UpdateAsync_ShouldUpdateExistingBevandaCustom()
        {
            // Arrange
            var updateDto = new BevandaCustomDTO
            {
                BevandaCustomId = 1,
                ArticoloId = 1,
                PersCustomId = 2,
                Prezzo = 6.00m
            };

            // Act
            await _repository.UpdateAsync(updateDto);

            // Assert
            var result = await _repository.GetByIdAsync(1);
            Assert.NotNull(result);
            Assert.Equal(6.00m, result.Prezzo);
            Assert.Equal(2, result.PersCustomId);
        }

        [Fact]
        public async Task DeleteAsync_ShouldRemoveBevandaCustom()
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
        public async Task ExistsByArticoloIdAsync_WithExistingArticoloId_ShouldReturnTrue()
        {
            // Act
            var result = await _repository.ExistsByArticoloIdAsync(1);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task ExistsByPersCustomIdAsync_WithExistingPersCustomId_ShouldReturnTrue()
        {
            // Act
            var result = await _repository.ExistsByPersCustomIdAsync(1);

            // Assert
            Assert.True(result);
        }
    }
}