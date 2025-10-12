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
    public class DimensioneQuantitaIngredientiRepositoryTest : BaseTest
    {
        private readonly DimensioneQuantitaIngredientiRepository _repository;
        private readonly BubbleTeaContext _context;

        public DimensioneQuantitaIngredientiRepositoryTest()
        {
            var options = new DbContextOptionsBuilder<BubbleTeaContext>()
                .UseInMemoryDatabase(databaseName: $"DimensioneQuantitaTest_{Guid.NewGuid()}")
                .Options;

            _context = new BubbleTeaContext(options);
            _repository = new DimensioneQuantitaIngredientiRepository(_context);

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

            // Crea DimensioneQuantitaIngredienti (solo i dati necessari)
            var dimensioneQuantitaIngredienti = new List<DimensioneQuantitaIngredienti>
            {
                new DimensioneQuantitaIngredienti
                {
                    DimensioneId = 1,
                    PersonalizzazioneIngredienteId = 1,
                    DimensioneBicchiereId = 1,
                    Moltiplicatore = 1.0m
                },
                new DimensioneQuantitaIngredienti
                {
                    DimensioneId = 2,
                    PersonalizzazioneIngredienteId = 1,
                    DimensioneBicchiereId = 2,
                    Moltiplicatore = 1.2m
                },
                new DimensioneQuantitaIngredienti
                {
                    DimensioneId = 3,
                    PersonalizzazioneIngredienteId = 2,
                    DimensioneBicchiereId = 1,
                    Moltiplicatore = 1.0m
                }
            };

            _context.UnitaDiMisura.AddRange(unitaDiMisura);
            _context.DimensioneBicchiere.AddRange(dimensioniBicchiere);
            _context.DimensioneQuantitaIngredienti.AddRange(dimensioneQuantitaIngredienti);
            _context.SaveChanges();
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnAllDimensioneQuantita()
        {
            // Act
            var result = await _repository.GetAllAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, result.Count());
        }

        [Fact]
        public async Task GetByIdAsync_WithValidId_ShouldReturnDimensioneQuantita()
        {
            // Act
            var result = await _repository.GetByIdAsync(1, 1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.DimensioneId);
            Assert.Equal(1, result.PersonalizzazioneIngredienteId);
            Assert.Equal(1.0m, result.Moltiplicatore);
        }

        [Fact]
        public async Task GetByIdAsync_WithInvalidId_ShouldReturnNull()
        {
            // Act
            var result = await _repository.GetByIdAsync(999, 999);

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
            Assert.All(resultList, dq => Assert.Equal(1, dq.DimensioneBicchiereId));
        }

        [Fact]
        public async Task GetByPersonalizzazioneIngredienteAsync_ShouldReturnFilteredResults()
        {
            // Act
            var result = await _repository.GetByPersonalizzazioneIngredienteAsync(1);

            // Assert
            var resultList = result.ToList();
            Assert.Equal(2, resultList.Count);
            Assert.All(resultList, dq => Assert.Equal(1, dq.PersonalizzazioneIngredienteId));
        }

        [Fact]
        public async Task GetByCombinazioneAsync_WithValidCombinazione_ShouldReturnDimensioneQuantita()
        {
            // Act
            var result = await _repository.GetByCombinazioneAsync(1, 1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.DimensioneBicchiereId);
            Assert.Equal(1, result.PersonalizzazioneIngredienteId);
        }

        [Fact]
        public async Task AddAsync_ShouldAddNewDimensioneQuantita()
        {
            // Arrange
            var newDimensioneQuantita = new DimensioneQuantitaIngredientiDTO
            {
                DimensioneId = 4,
                PersonalizzazioneIngredienteId = 2,
                DimensioneBicchiereId = 2,
                Moltiplicatore = 1.5m
            };

            // Act
            await _repository.AddAsync(newDimensioneQuantita);

            // Assert
            var result = await _repository.GetByIdAsync(4, 2);
            Assert.NotNull(result);
            Assert.Equal(1.5m, result.Moltiplicatore);
        }

        [Fact]
        public async Task UpdateAsync_ShouldUpdateExistingDimensioneQuantita()
        {
            // Arrange
            var updateDto = new DimensioneQuantitaIngredientiDTO
            {
                DimensioneId = 1,
                PersonalizzazioneIngredienteId = 1,
                DimensioneBicchiereId = 1,
                Moltiplicatore = 2.0m
            };

            // Act
            await _repository.UpdateAsync(updateDto);

            // Assert
            var result = await _repository.GetByIdAsync(1, 1);
            Assert.NotNull(result);
            Assert.Equal(2.0m, result.Moltiplicatore);
        }

        [Fact]
        public async Task DeleteAsync_ShouldRemoveDimensioneQuantita()
        {
            // Act
            await _repository.DeleteAsync(1, 1);

            // Assert
            var result = await _repository.GetByIdAsync(1, 1);
            Assert.Null(result);
        }

        [Fact]
        public async Task ExistsAsync_WithExistingId_ShouldReturnTrue()
        {
            // Act
            var result = await _repository.ExistsAsync(1, 1);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task ExistsByCombinazioneAsync_WithExistingCombinazione_ShouldReturnTrue()
        {
            // Act
            var result = await _repository.ExistsByCombinazioneAsync(1, 1);

            // Assert
            Assert.True(result);
        }
    }
}
