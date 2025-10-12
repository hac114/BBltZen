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
    public class PersonalizzazioneCustomRepositoryTest : BaseTest
    {
        private readonly PersonalizzazioneCustomRepository _repository;
        private readonly BubbleTeaContext _context;

        public PersonalizzazioneCustomRepositoryTest()
        {
            var options = new DbContextOptionsBuilder<BubbleTeaContext>()
                .UseInMemoryDatabase(databaseName: $"PersonalizzazioneCustomTest_{Guid.NewGuid()}")
                .Options;

            _context = new BubbleTeaContext(options);
            _repository = new PersonalizzazioneCustomRepository(_context);

            InitializeTestData();
        }

        private void InitializeTestData()
        {
            _context.Database.EnsureDeleted();
            _context.Database.EnsureCreated();

            // Crea UnitaDiMisura
            var unitaDiMisura = new List<UnitaDiMisura>
            {
                new UnitaDiMisura { UnitaMisuraId = 1, Sigla = "ML", Descrizione = "Millilitri" }
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

            // Crea Personalizzazioni Custom
            var personalizzazioniCustom = new List<PersonalizzazioneCustom>
            {
                new PersonalizzazioneCustom
                {
                    PersCustomId = 1,
                    Nome = "Dolce Leggero",
                    GradoDolcezza = 2,
                    DimensioneBicchiereId = 1,
                    DataCreazione = DateTime.Now.AddDays(-10),
                    DataAggiornamento = DateTime.Now.AddDays(-1)
                },
                new PersonalizzazioneCustom
                {
                    PersCustomId = 2,
                    Nome = "Molto Dolce",
                    GradoDolcezza = 5,
                    DimensioneBicchiereId = 2,
                    DataCreazione = DateTime.Now.AddDays(-5),
                    DataAggiornamento = DateTime.Now
                },
                new PersonalizzazioneCustom
                {
                    PersCustomId = 3,
                    Nome = "Medio Dolce",
                    GradoDolcezza = 3,
                    DimensioneBicchiereId = 1,
                    DataCreazione = DateTime.Now.AddDays(-3),
                    DataAggiornamento = DateTime.Now.AddDays(-1)
                }
            };

            _context.UnitaDiMisura.AddRange(unitaDiMisura);
            _context.DimensioneBicchiere.AddRange(dimensioniBicchiere);
            _context.PersonalizzazioneCustom.AddRange(personalizzazioniCustom);
            _context.SaveChanges();
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnAllPersonalizzazioniCustom()
        {
            // Act
            var result = await _repository.GetAllAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, result.Count());
        }

        [Fact]
        public async Task GetByIdAsync_WithValidId_ShouldReturnPersonalizzazioneCustom()
        {
            // Act
            var result = await _repository.GetByIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.PersCustomId);
            Assert.Equal("Dolce Leggero", result.Nome);
            Assert.Equal(2, result.GradoDolcezza);
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
        public async Task GetByDimensioneBicchiereAsync_ShouldReturnFilteredResults()
        {
            // Act
            var result = await _repository.GetByDimensioneBicchiereAsync(1);

            // Assert
            var resultList = result.ToList();
            Assert.Equal(2, resultList.Count);
            Assert.All(resultList, pc => Assert.Equal(1, pc.DimensioneBicchiereId));
        }

        [Fact]
        public async Task GetByGradoDolcezzaAsync_ShouldReturnFilteredResults()
        {
            // Act
            var result = await _repository.GetByGradoDolcezzaAsync(2);

            // Assert
            var resultList = result.ToList();
            Assert.Single(resultList);
            Assert.Equal(2, resultList[0].GradoDolcezza);
        }

        [Fact]
        public async Task AddAsync_ShouldAddNewPersonalizzazioneCustom()
        {
            // Arrange
            var newPersonalizzazione = new PersonalizzazioneCustomDTO
            {
                PersCustomId = 4,
                Nome = "Nuova Personalizzazione",
                GradoDolcezza = 4,
                DimensioneBicchiereId = 2
            };

            // Act
            await _repository.AddAsync(newPersonalizzazione);

            // Assert
            var result = await _repository.GetByIdAsync(4);
            Assert.NotNull(result);
            Assert.Equal("Nuova Personalizzazione", result.Nome);
            Assert.Equal(4, result.GradoDolcezza);
            Assert.NotNull(result.DataCreazione);
            Assert.NotNull(result.DataAggiornamento);
        }

        [Fact]
        public async Task UpdateAsync_ShouldUpdateExistingPersonalizzazioneCustom()
        {
            // Arrange
            var updateDto = new PersonalizzazioneCustomDTO
            {
                PersCustomId = 1,
                Nome = "Dolce Leggero Modificato",
                GradoDolcezza = 1,
                DimensioneBicchiereId = 2
            };

            // Act
            await _repository.UpdateAsync(updateDto);

            // Assert
            var result = await _repository.GetByIdAsync(1);
            Assert.NotNull(result);
            Assert.Equal("Dolce Leggero Modificato", result.Nome);
            Assert.Equal(1, result.GradoDolcezza);
            Assert.Equal(2, result.DimensioneBicchiereId);
        }

        [Fact]
        public async Task DeleteAsync_ShouldRemovePersonalizzazioneCustom()
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