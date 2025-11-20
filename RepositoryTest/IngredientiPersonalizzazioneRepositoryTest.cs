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
        
        public IngredientiPersonalizzazioneRepositoryTest()
        {            
            _repository = new IngredientiPersonalizzazioneRepository(_context);

            InitializeTestData();
        }

        private void InitializeTestData()
        {
            _context.Database.EnsureDeleted();
            _context.Database.EnsureCreated();

            // Crea Ingredienti - CON TUTTE LE PROPRIETÀ OBBLIGATORIE
            var ingredienti = new List<Ingrediente>
            {
                new Ingrediente
                {
                    IngredienteId = 1,
                    Ingrediente1 = "Tè Verde", // Nome dell'ingrediente
                    CategoriaId = 1,
                    PrezzoAggiunto = 1.00m,
                    Disponibile = true,
                    DataInserimento = DateTime.Now.AddDays(-30),
                    DataAggiornamento = DateTime.Now.AddDays(-10)
                },
                new Ingrediente
                {
                    IngredienteId = 2,
                    Ingrediente1 = "Boba",
                    CategoriaId = 4,
                    PrezzoAggiunto = 0.50m,
                    Disponibile = true,
                    DataInserimento = DateTime.Now.AddDays(-25),
                    DataAggiornamento = DateTime.Now.AddDays(-5)
                },
                new Ingrediente
                {
                    IngredienteId = 3,
                    Ingrediente1 = "Sciroppo di Miele",
                    CategoriaId = 3,
                    PrezzoAggiunto = 0.30m,
                    Disponibile = true,
                    DataInserimento = DateTime.Now.AddDays(-20),
                    DataAggiornamento = DateTime.Now.AddDays(-1)
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
            // Arrange
            var newIngredientePers = new IngredientiPersonalizzazioneDTO
            {
                PersCustomId = 500, // ✅ COMBINAZIONE UNICA
                IngredienteId = 500
            };
            var addedIngredientePers = await _repository.AddAsync(newIngredientePers);

            // Act
            var result = await _repository.GetByIdAsync(addedIngredientePers.IngredientePersId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(addedIngredientePers.IngredientePersId, result.IngredientePersId);
            Assert.Equal(500, result.PersCustomId);
            Assert.Equal(500, result.IngredienteId);
            Assert.NotEqual(default, result.DataCreazione);
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
            // Arrange
            var newIngredientePers = new IngredientiPersonalizzazioneDTO
            {
                PersCustomId = 30,
                IngredienteId = 30
            };
            await _repository.AddAsync(newIngredientePers);

            // Act
            var result = await _repository.GetByCombinazioneAsync(30, 30);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(30, result.PersCustomId);
            Assert.Equal(30, result.IngredienteId);
        }

        [Fact]
        public async Task GetByCombinazioneAsync_WithInvalidCombinazione_ShouldReturnNull()
        {
            // Act
            var result = await _repository.GetByCombinazioneAsync(999, 999);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task AddAsync_ShouldAddNewIngredientePersonalizzazione()
        {
            // Arrange
            var newIngredientePers = new IngredientiPersonalizzazioneDTO
            {
                PersCustomId = 100, // ✅ COMBINAZIONE UNICA
                IngredienteId = 100
            };

            // Act
            var result = await _repository.AddAsync(newIngredientePers);

            // Assert
            Assert.True(result.IngredientePersId > 0);
            Assert.Equal(100, result.PersCustomId);
            Assert.Equal(100, result.IngredienteId);
            Assert.NotEqual(default, result.DataCreazione);
        }

        [Fact]
        public async Task UpdateAsync_ShouldUpdateExistingIngredientePersonalizzazione()
        {
            // Arrange
            var newIngredientePers = new IngredientiPersonalizzazioneDTO
            {
                PersCustomId = 200, // ✅ COMBINAZIONE UNICA
                IngredienteId = 200
            };
            var addedIngredientePers = await _repository.AddAsync(newIngredientePers);

            var updateDto = new IngredientiPersonalizzazioneDTO
            {
                IngredientePersId = addedIngredientePers.IngredientePersId,
                PersCustomId = 201, // ✅ NUOVA COMBINAZIONE UNICA
                IngredienteId = 201
            };

            // Act
            await _repository.UpdateAsync(updateDto);

            // Assert
            var result = await _repository.GetByIdAsync(addedIngredientePers.IngredientePersId);
            Assert.NotNull(result);
            Assert.Equal(201, result.PersCustomId);
            Assert.Equal(201, result.IngredienteId);
        }

        [Fact]
        public async Task DeleteAsync_ShouldRemoveIngredientePersonalizzazione()
        {
            // Arrange
            var newIngredientePers = new IngredientiPersonalizzazioneDTO
            {
                PersCustomId = 300, // ✅ COMBINAZIONE UNICA
                IngredienteId = 300
            };
            var addedIngredientePers = await _repository.AddAsync(newIngredientePers);

            // Act
            await _repository.DeleteAsync(addedIngredientePers.IngredientePersId);

            // Assert
            var result = await _repository.GetByIdAsync(addedIngredientePers.IngredientePersId);
            Assert.Null(result);
        }

        [Fact]
        public async Task ExistsAsync_WithExistingId_ShouldReturnTrue()
        {
            // Arrange
            var newIngredientePers = new IngredientiPersonalizzazioneDTO
            {
                PersCustomId = 400, // ✅ COMBINAZIONE UNICA
                IngredienteId = 400
            };
            var addedIngredientePers = await _repository.AddAsync(newIngredientePers);

            // Act
            var result = await _repository.ExistsAsync(addedIngredientePers.IngredientePersId);

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

        [Fact]
        public async Task UpdateAsync_ShouldNotThrow_ForNonExistingId()
        {
            // Arrange
            var updateDto = new IngredientiPersonalizzazioneDTO
            {
                IngredientePersId = 999,
                PersCustomId = 1,
                IngredienteId = 1
            };

            // Act & Assert - ✅ SILENT FAIL, NO EXCEPTION
            var exception = await Record.ExceptionAsync(() =>
                _repository.UpdateAsync(updateDto)
            );

            Assert.Null(exception);
        }

        [Fact]
        public async Task DeleteAsync_ShouldNotThrow_ForNonExistingId()
        {
            // Act & Assert - ✅ SILENT FAIL, NO EXCEPTION
            var exception = await Record.ExceptionAsync(() =>
                _repository.DeleteAsync(999)
            );

            Assert.Null(exception);
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
        public async Task AddAsync_ShouldThrow_ForDuplicateCombinazione()
        {
            // Arrange
            var ingredientePers1 = new IngredientiPersonalizzazioneDTO
            {
                PersCustomId = 10,
                IngredienteId = 10
            };
            await _repository.AddAsync(ingredientePers1);

            var ingredientePers2 = new IngredientiPersonalizzazioneDTO
            {
                PersCustomId = 10, // ✅ STESSO PersCustomId
                IngredienteId = 10  // ✅ STESSO IngredienteId
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
                _repository.AddAsync(ingredientePers2)
            );

            Assert.Contains("esiste già", exception.Message.ToLower());
        }

        [Fact]
        public async Task UpdateAsync_ShouldThrow_ForDuplicateCombinazione()
        {
            // Arrange - Crea due record con combinazioni DIVERSE
            var ingredientePers1 = new IngredientiPersonalizzazioneDTO
            {
                PersCustomId = 20,
                IngredienteId = 20
            };
            var addedIngredientePers1 = await _repository.AddAsync(ingredientePers1);

            var ingredientePers2 = new IngredientiPersonalizzazioneDTO
            {
                PersCustomId = 21,
                IngredienteId = 21
            };
            var addedIngredientePers2 = await _repository.AddAsync(ingredientePers2);

            // Act & Assert - Prova a fare l'update del secondo record con la STESSA combinazione del primo
            var updateDto = new IngredientiPersonalizzazioneDTO
            {
                IngredientePersId = addedIngredientePers2.IngredientePersId, // Mantiene il suo ID
                PersCustomId = 20,  // ✅ STESSO PersCustomId del primo
                IngredienteId = 20  // ✅ STESSO IngredienteId del primo
            };

            var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
                _repository.UpdateAsync(updateDto)
            );

            Assert.Contains("esiste già", exception.Message.ToLower());
        }
    }
}