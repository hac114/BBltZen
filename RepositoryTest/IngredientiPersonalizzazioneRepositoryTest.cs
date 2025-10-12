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
    public class IngredientiPersonalizzazioneRepositoryTest : BaseTest
    {
        private readonly IngredientiPersonalizzazioneRepository _repository;
        private readonly BubbleTeaContext _context;

        public IngredientiPersonalizzazioneRepositoryTest()
        {
            var options = new DbContextOptionsBuilder<BubbleTeaContext>()
                .UseInMemoryDatabase(databaseName: $"IngredientiPersTest_{Guid.NewGuid()}")
                .Options;

            _context = new BubbleTeaContext(options);
            _repository = new IngredientiPersonalizzazioneRepository(_context);

            InitializeTestData();
        }

        private void InitializeTestData()
        {
            _context.Database.EnsureDeleted();
            _context.Database.EnsureCreated();

            // Crea Ingredienti - solo con le proprietà che esistono realmente
            var ingredienti = new List<Ingrediente>
            {
                new Ingrediente { IngredienteId = 1 },
                new Ingrediente { IngredienteId = 2 },
                new Ingrediente { IngredienteId = 3 }
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

            // Crea IngredientiPersonalizzazione
            var ingredientiPersonalizzazione = new List<IngredientiPersonalizzazione>
            {
                new IngredientiPersonalizzazione
                {
                    IngredientePersId = 1,
                    PersCustomId = 1,
                    IngredienteId = 1,
                    DataCreazione = DateTime.Now.AddDays(-10)
                },
                new IngredientiPersonalizzazione
                {
                    IngredientePersId = 2,
                    PersCustomId = 1,
                    IngredienteId = 2,
                    DataCreazione = DateTime.Now.AddDays(-5)
                },
                new IngredientiPersonalizzazione
                {
                    IngredientePersId = 3,
                    PersCustomId = 2,
                    IngredienteId = 1,
                    DataCreazione = DateTime.Now.AddDays(-3)
                },
                new IngredientiPersonalizzazione
                {
                    IngredientePersId = 4,
                    PersCustomId = 2,
                    IngredienteId = 3,
                    DataCreazione = DateTime.Now.AddDays(-1)
                }
            };

            _context.Ingrediente.AddRange(ingredienti);
            _context.PersonalizzazioneCustom.AddRange(personalizzazioniCustom);
            _context.IngredientiPersonalizzazione.AddRange(ingredientiPersonalizzazione);
            _context.SaveChanges();
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnAllIngredientiPersonalizzazione()
        {
            // Act
            var result = await _repository.GetAllAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(4, result.Count());
        }

        [Fact]
        public async Task GetByIdAsync_WithValidId_ShouldReturnIngredientePersonalizzazione()
        {
            // Act
            var result = await _repository.GetByIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.IngredientePersId);
            Assert.Equal(1, result.PersCustomId);
            Assert.Equal(1, result.IngredienteId);
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
        public async Task GetByPersCustomIdAsync_ShouldReturnFilteredResults()
        {
            // Act
            var result = await _repository.GetByPersCustomIdAsync(1);

            // Assert
            var resultList = result.ToList();
            Assert.Equal(2, resultList.Count);
            Assert.All(resultList, ip => Assert.Equal(1, ip.PersCustomId));
        }

        [Fact]
        public async Task GetByIngredienteIdAsync_ShouldReturnFilteredResults()
        {
            // Act
            var result = await _repository.GetByIngredienteIdAsync(1);

            // Assert
            var resultList = result.ToList();
            Assert.Equal(2, resultList.Count);
            Assert.All(resultList, ip => Assert.Equal(1, ip.IngredienteId));
        }

        [Fact]
        public async Task GetByCombinazioneAsync_WithValidCombinazione_ShouldReturnIngredientePersonalizzazione()
        {
            // Act
            var result = await _repository.GetByCombinazioneAsync(1, 1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.PersCustomId);
            Assert.Equal(1, result.IngredienteId);
        }

        [Fact]
        public async Task AddAsync_ShouldAddNewIngredientePersonalizzazione()
        {
            // Arrange
            var newIngredientePers = new IngredientiPersonalizzazioneDTO
            {
                IngredientePersId = 5,
                PersCustomId = 1,
                IngredienteId = 3
            };

            // Act
            await _repository.AddAsync(newIngredientePers);

            // Assert
            var result = await _repository.GetByIdAsync(5);
            Assert.NotNull(result);
            Assert.Equal(1, result.PersCustomId);
            Assert.Equal(3, result.IngredienteId);
            Assert.NotNull(result.DataCreazione);
        }

        [Fact]
        public async Task UpdateAsync_ShouldUpdateExistingIngredientePersonalizzazione()
        {
            // Arrange
            var updateDto = new IngredientiPersonalizzazioneDTO
            {
                IngredientePersId = 1,
                PersCustomId = 2,
                IngredienteId = 3
            };

            // Act
            await _repository.UpdateAsync(updateDto);

            // Assert
            var result = await _repository.GetByIdAsync(1);
            Assert.NotNull(result);
            Assert.Equal(2, result.PersCustomId);
            Assert.Equal(3, result.IngredienteId);
        }

        [Fact]
        public async Task DeleteAsync_ShouldRemoveIngredientePersonalizzazione()
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
        public async Task ExistsByCombinazioneAsync_WithExistingCombinazione_ShouldReturnTrue()
        {
            // Act
            var result = await _repository.ExistsByCombinazioneAsync(1, 1);

            // Assert
            Assert.True(result);
        }
    }
}