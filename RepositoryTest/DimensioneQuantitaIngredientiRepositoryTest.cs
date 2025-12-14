using BBltZen;
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
        
        public DimensioneQuantitaIngredientiRepositoryTest()
        {            
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
            var result = await _repository.AddAsync(newDimensioneQuantita); // ✅ USA IL RISULTATO

            // Assert
            Assert.Equal(4, result.DimensioneId);
            Assert.Equal(2, result.PersonalizzazioneIngredienteId);
            Assert.Equal(1.5m, result.Moltiplicatore);

            // Verifica anche nel database
            var fromDb = await _repository.GetByIdAsync(4, 2);
            Assert.NotNull(fromDb);
            Assert.Equal(1.5m, fromDb.Moltiplicatore);
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

        [Fact]
        public async Task UpdateAsync_ShouldNotThrow_ForNonExistingId()
        {
            // Arrange
            var updateDto = new DimensioneQuantitaIngredientiDTO
            {
                DimensioneId = 999,
                PersonalizzazioneIngredienteId = 999,
                DimensioneBicchiereId = 1,
                Moltiplicatore = 2.0m
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
                _repository.DeleteAsync(999, 999)
            );

            Assert.Null(exception);
        }

        [Fact]
        public async Task AddAsync_ShouldThrow_ForDuplicateCombinazione()
        {
            // Arrange
            var dimensioneQuantita1 = new DimensioneQuantitaIngredientiDTO
            {
                DimensioneId = 10,
                PersonalizzazioneIngredienteId = 10,
                DimensioneBicchiereId = 1,
                Moltiplicatore = 1.0m
            };
            await _repository.AddAsync(dimensioneQuantita1);

            var dimensioneQuantita2 = new DimensioneQuantitaIngredientiDTO
            {
                DimensioneId = 11, // ✅ DIVERSO DimensioneId
                PersonalizzazioneIngredienteId = 10, // ✅ STESSO PersonalizzazioneIngredienteId
                DimensioneBicchiereId = 1, // ✅ STESSA COMBINAZIONE DimensioneBicchiereId + PersonalizzazioneIngredienteId
                Moltiplicatore = 1.5m
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
                _repository.AddAsync(dimensioneQuantita2)
            );

            Assert.Contains("esiste già", exception.Message.ToLower());
        }

        [Fact]
        public async Task UpdateAsync_ShouldThrow_ForDuplicateCombinazione()
        {
            // Arrange - Crea due record con combinazioni DIVERSE
            var dimensioneQuantita1 = new DimensioneQuantitaIngredientiDTO
            {
                // ✅ NOTA: Non impostare DimensioneId - sarà generato automaticamente
                PersonalizzazioneIngredienteId = 20,
                DimensioneBicchiereId = 1,  // Combinazione: (1, 20)
                Moltiplicatore = 1.0m
            };
            var result1 = await _repository.AddAsync(dimensioneQuantita1);

            var dimensioneQuantita2 = new DimensioneQuantitaIngredientiDTO
            {
                // ✅ NOTA: Non impostare DimensioneId - sarà generato automaticamente
                PersonalizzazioneIngredienteId = 21,
                DimensioneBicchiereId = 2,  // Combinazione: (2, 21)
                Moltiplicatore = 1.2m
            };
            var result2 = await _repository.AddAsync(dimensioneQuantita2);

            // Act & Assert - Prova a fare l'update del secondo record con la STESSA combinazione del primo
            var updateDto = new DimensioneQuantitaIngredientiDTO
            {
                DimensioneId = result2.DimensioneId,         // ✅ Mantiene il suo DimensioneId
                PersonalizzazioneIngredienteId = 21,         // ✅ Mantiene il suo PersonalizzazioneIngredienteId  
                DimensioneBicchiereId = 1,                   // ✅ CAMBIA a STESSO DimensioneBicchiereId del primo
                Moltiplicatore = 1.5m
            };

            // Ora dovrebbe lanciare eccezione perché la combinazione (1, 21) è già usata dal primo record?
            // ATTENZIONE: Il primo record ha combinazione (1, 20), il secondo (1, 21) - NON sono duplicati!
            // Dobbiamo creare un vero duplicato:

            // ✅ CORREZIONE: Crea un vero scenario di duplicato
            var updateDtoDuplicate = new DimensioneQuantitaIngredientiDTO
            {
                DimensioneId = result2.DimensioneId,         // Secondo record
                PersonalizzazioneIngredienteId = 20,         // ✅ STESSO PersonalizzazioneIngredienteId del primo
                DimensioneBicchiereId = 1,                   // ✅ STESSO DimensioneBicchiereId del primo
                Moltiplicatore = 1.5m
            };

            var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
                _repository.UpdateAsync(updateDtoDuplicate)
            );

            Assert.Contains("esiste già", exception.Message.ToLower());
        }
    }
}
