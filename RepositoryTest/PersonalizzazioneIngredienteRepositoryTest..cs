using Database.Models;
using DTO;
using Microsoft.EntityFrameworkCore;
using Repository.Interface;
using Repository.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace RepositoryTest
{
    public class PersonalizzazioneIngredienteRepositoryTest : BaseTest
    {
        private readonly IPersonalizzazioneIngredienteRepository _personalizzazioneIngredienteRepository;

        public PersonalizzazioneIngredienteRepositoryTest()
        {
            _personalizzazioneIngredienteRepository = new PersonalizzazioneIngredienteRepository(_context);
        }

        //[Fact]
        //public async Task UpdateAsync_Should_Update_PersonalizzazioneIngrediente_Correctly()
        //{
        //    // Arrange
        //    await CleanTablesForPersonalizzazioneIngredienteTest();

        //    var personalizzazioneIngredienteDto = new PersonalizzazioneIngredienteDTO
        //    {
        //        PersonalizzazioneId = 1,
        //        IngredienteId = 1,
        //        Quantita = 100.0m,
        //        UnitaMisuraId = 1
        //    };

        //    // FASE 1: Add
        //    var added = await _personalizzazioneIngredienteRepository.AddAsync(personalizzazioneIngredienteDto); // ✅ USA IL RISULTATO

        //    // FASE 2: Update
        //    var updateDto = new PersonalizzazioneIngredienteDTO
        //    {
        //        PersonalizzazioneIngredienteId = added.PersonalizzazioneIngredienteId,
        //        PersonalizzazioneId = 1,
        //        IngredienteId = 1,
        //        Quantita = 200.0m,
        //        UnitaMisuraId = 2
        //    };

        //    await _personalizzazioneIngredienteRepository.UpdateAsync(updateDto);

        //    // FASE 3: Verifica
        //    var afterUpdate = await _personalizzazioneIngredienteRepository.GetByIdAsync(added.PersonalizzazioneIngredienteId);
        //    Assert.NotNull(afterUpdate);
        //    Assert.Equal(200.0m, afterUpdate.Quantita);
        //    Assert.Equal(2, afterUpdate.UnitaMisuraId);
        //}
                
        //[Fact]
        //public async Task AddAsync_Should_Add_PersonalizzazioneIngrediente()
        //{
        //    // Arrange
        //    await CleanTablesForPersonalizzazioneIngredienteTest();

        //    var personalizzazioneIngredienteDto = new PersonalizzazioneIngredienteDTO
        //    {
        //        PersonalizzazioneId = 1,
        //        IngredienteId = 1,
        //        Quantita = 100.0m,
        //        UnitaMisuraId = 1
        //    };

        //    // Act
        //    var result = await _personalizzazioneIngredienteRepository.AddAsync(personalizzazioneIngredienteDto); // ✅ USA IL RISULTATO

        //    // Assert
        //    Assert.True(result.PersonalizzazioneIngredienteId > 0); // ✅ VERIFICA ID GENERATO
        //    Assert.Equal(1, result.PersonalizzazioneId);
        //    Assert.Equal(1, result.IngredienteId);
        //    Assert.Equal(100.0m, result.Quantita);
        //    Assert.Equal(1, result.UnitaMisuraId);
        //}

        //[Fact]
        //public async Task GetByIdAsync_Should_Return_Correct_PersonalizzazioneIngrediente()
        //{
        //    // Arrange
        //    await CleanTablesForPersonalizzazioneIngredienteTest();

        //    var personalizzazioneIngredienteDto = new PersonalizzazioneIngredienteDTO
        //    {
        //        PersonalizzazioneId = 1,
        //        IngredienteId = 1,
        //        Quantita = 50.0m,
        //        UnitaMisuraId = 1
        //    };
        //    var added = await _personalizzazioneIngredienteRepository.AddAsync(personalizzazioneIngredienteDto); // ✅ USA IL RISULTATO

        //    // Act
        //    var result = await _personalizzazioneIngredienteRepository.GetByIdAsync(added.PersonalizzazioneIngredienteId);

        //    // Assert
        //    Assert.NotNull(result);
        //    Assert.Equal(added.PersonalizzazioneIngredienteId, result.PersonalizzazioneIngredienteId);
        //    Assert.Equal(1, result.PersonalizzazioneId);
        //    Assert.Equal(1, result.IngredienteId);
        //    Assert.Equal(50.0m, result.Quantita);
        //}

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_For_NonExisting_Id()
        {
            // Act
            var result = await _personalizzazioneIngredienteRepository.GetByIdAsync(999);

            // Assert
            Assert.Null(result);
        }

        //[Fact]
        //public async Task GetByPersonalizzazioneIdAsync_Should_Return_All_PersonalizzazioneIngredienti_For_Personalizzazione()
        //{
        //    // Arrange
        //    await CleanTablesForPersonalizzazioneIngredienteTest();

        //    var personalizzazioneIngredienti = new List<PersonalizzazioneIngredienteDTO>
        //    {
        //        new PersonalizzazioneIngredienteDTO { PersonalizzazioneId = 1, IngredienteId = 1, Quantita = 100.0m, UnitaMisuraId = 1 },
        //        new PersonalizzazioneIngredienteDTO { PersonalizzazioneId = 1, IngredienteId = 2, Quantita = 50.0m, UnitaMisuraId = 1 },
        //        new PersonalizzazioneIngredienteDTO { PersonalizzazioneId = 2, IngredienteId = 1, Quantita = 75.0m, UnitaMisuraId = 1 }
        //    };

        //    foreach (var personalizzazioneIngrediente in personalizzazioneIngredienti)
        //    {
        //        await _personalizzazioneIngredienteRepository.AddAsync(personalizzazioneIngrediente);
        //    }

        //    // Act
        //    var result = await _personalizzazioneIngredienteRepository.GetByPersonalizzazioneIdAsync(1);

        //    // Assert
        //    Assert.Equal(2, result.Count());
        //    Assert.All(result, pi => Assert.Equal(1, pi.PersonalizzazioneId));
        //}

        //[Fact]
        //public async Task GetByIngredienteIdAsync_Should_Return_All_PersonalizzazioneIngredienti_For_Ingrediente()
        //{
        //    // Arrange
        //    await CleanTablesForPersonalizzazioneIngredienteTest();

        //    var personalizzazioneIngredienti = new List<PersonalizzazioneIngredienteDTO>
        //    {
        //        new PersonalizzazioneIngredienteDTO { PersonalizzazioneId = 1, IngredienteId = 1, Quantita = 100.0m, UnitaMisuraId = 1 },
        //        new PersonalizzazioneIngredienteDTO { PersonalizzazioneId = 2, IngredienteId = 1, Quantita = 50.0m, UnitaMisuraId = 1 },
        //        new PersonalizzazioneIngredienteDTO { PersonalizzazioneId = 1, IngredienteId = 2, Quantita = 75.0m, UnitaMisuraId = 1 }
        //    };

        //    foreach (var personalizzazioneIngrediente in personalizzazioneIngredienti)
        //    {
        //        await _personalizzazioneIngredienteRepository.AddAsync(personalizzazioneIngrediente);
        //    }

        //    // Act
        //    var result = await _personalizzazioneIngredienteRepository.GetByIngredienteIdAsync(1);

        //    // Assert
        //    Assert.Equal(2, result.Count());
        //    Assert.All(result, pi => Assert.Equal(1, pi.IngredienteId));
        //}

        //[Fact]
        //public async Task GetByPersonalizzazioneAndIngredienteAsync_Should_Return_Correct_PersonalizzazioneIngrediente()
        //{
        //    // Arrange
        //    await CleanTablesForPersonalizzazioneIngredienteTest();

        //    var personalizzazioneIngredienteDto = new PersonalizzazioneIngredienteDTO
        //    {
        //        PersonalizzazioneId = 1,
        //        IngredienteId = 1,
        //        Quantita = 100.0m,
        //        UnitaMisuraId = 1
        //    };
        //    var added = await _personalizzazioneIngredienteRepository.AddAsync(personalizzazioneIngredienteDto); // ✅ USA IL RISULTATO

        //    // Act
        //    var result = await _personalizzazioneIngredienteRepository.GetByPersonalizzazioneAndIngredienteAsync(1, 1);

        //    // Assert
        //    Assert.NotNull(result);
        //    Assert.Equal(1, result.PersonalizzazioneId);
        //    Assert.Equal(1, result.IngredienteId);
        //    Assert.Equal(100.0m, result.Quantita);
        //}

        //[Fact]
        //public async Task DeleteAsync_Should_Remove_PersonalizzazioneIngrediente()
        //{
        //    // Arrange
        //    await CleanTablesForPersonalizzazioneIngredienteTest();

        //    var personalizzazioneIngredienteDto = new PersonalizzazioneIngredienteDTO
        //    {
        //        PersonalizzazioneId = 1,
        //        IngredienteId = 1,
        //        Quantita = 100.0m,
        //        UnitaMisuraId = 1
        //    };
        //    var added = await _personalizzazioneIngredienteRepository.AddAsync(personalizzazioneIngredienteDto); // ✅ USA IL RISULTATO

        //    // Act
        //    await _personalizzazioneIngredienteRepository.DeleteAsync(added.PersonalizzazioneIngredienteId);

        //    // Assert
        //    var deleted = await _personalizzazioneIngredienteRepository.GetByIdAsync(added.PersonalizzazioneIngredienteId);
        //    Assert.Null(deleted);
        //}

        [Fact]
        public async Task DeleteAsync_Should_Not_Throw_For_NonExisting_Id()
        {
            // Act & Assert - Non dovrebbe lanciare eccezioni
            await _personalizzazioneIngredienteRepository.DeleteAsync(999);
        }

        //[Fact]
        //public async Task ExistsByPersonalizzazioneAndIngredienteAsync_Should_Return_True_For_Existing_PersonalizzazioneIngrediente()
        //{
        //    // Arrange
        //    await CleanTablesForPersonalizzazioneIngredienteTest();

        //    var personalizzazioneIngredienteDto = new PersonalizzazioneIngredienteDTO
        //    {
        //        PersonalizzazioneId = 1,
        //        IngredienteId = 1,
        //        Quantita = 100.0m,
        //        UnitaMisuraId = 1
        //    };
        //    await _personalizzazioneIngredienteRepository.AddAsync(personalizzazioneIngredienteDto);

        //    // Act
        //    var exists = await _personalizzazioneIngredienteRepository.ExistsByPersonalizzazioneAndIngredienteAsync(1, 1);

        //    // Assert
        //    Assert.True(exists);
        //}

        [Fact]
        public async Task ExistsByPersonalizzazioneAndIngredienteAsync_Should_Return_False_For_NonExisting_PersonalizzazioneIngrediente()
        {
            // Act
            var exists = await _personalizzazioneIngredienteRepository.ExistsByPersonalizzazioneAndIngredienteAsync(1, 999);

            // Assert
            Assert.False(exists);
        }

        //[Fact]
        //public async Task ExistsAsync_Should_Return_True_For_Existing_PersonalizzazioneIngrediente()
        //{
        //    // Arrange
        //    await CleanTablesForPersonalizzazioneIngredienteTest();

        //    var personalizzazioneIngredienteDto = new PersonalizzazioneIngredienteDTO
        //    {
        //        PersonalizzazioneId = 1,
        //        IngredienteId = 1,
        //        Quantita = 100.0m,
        //        UnitaMisuraId = 1
        //    };
        //    var added = await _personalizzazioneIngredienteRepository.AddAsync(personalizzazioneIngredienteDto); // ✅ USA IL RISULTATO

        //    // Act
        //    var exists = await _personalizzazioneIngredienteRepository.ExistsAsync(added.PersonalizzazioneIngredienteId);

        //    // Assert
        //    Assert.True(exists);
        //}

        [Fact]
        public async Task ExistsAsync_Should_Return_False_For_NonExisting_PersonalizzazioneIngrediente()
        {
            // Act
            var exists = await _personalizzazioneIngredienteRepository.ExistsAsync(999);

            // Assert
            Assert.False(exists);
        }

        //[Fact]
        //public async Task GetCountByPersonalizzazioneAsync_Should_Return_Correct_Count()
        //{
        //    // Arrange
        //    await CleanTablesForPersonalizzazioneIngredienteTest();

        //    var personalizzazioneIngredienti = new List<PersonalizzazioneIngredienteDTO>
        //    {
        //        new PersonalizzazioneIngredienteDTO { PersonalizzazioneId = 1, IngredienteId = 1, Quantita = 100.0m, UnitaMisuraId = 1 },
        //        new PersonalizzazioneIngredienteDTO { PersonalizzazioneId = 1, IngredienteId = 2, Quantita = 50.0m, UnitaMisuraId = 1 },
        //        new PersonalizzazioneIngredienteDTO { PersonalizzazioneId = 2, IngredienteId = 1, Quantita = 75.0m, UnitaMisuraId = 1 }
        //    };

        //    foreach (var personalizzazioneIngrediente in personalizzazioneIngredienti)
        //    {
        //        await _personalizzazioneIngredienteRepository.AddAsync(personalizzazioneIngrediente);
        //    }

        //    // Act
        //    var count = await _personalizzazioneIngredienteRepository.GetCountByPersonalizzazioneAsync(1);

        //    // Assert
        //    Assert.Equal(2, count);
        //}
        
        //[Fact]
        //public async Task GetAllAsync_Should_Return_All_PersonalizzazioneIngredienti()
        //{
        //    // Arrange
        //    await CleanTablesForPersonalizzazioneIngredienteTest();

        //    var personalizzazioneIngredienti = new List<PersonalizzazioneIngredienteDTO>
        //    {
        //        new PersonalizzazioneIngredienteDTO { PersonalizzazioneId = 1, IngredienteId = 1, Quantita = 100.0m, UnitaMisuraId = 1 },
        //        new PersonalizzazioneIngredienteDTO { PersonalizzazioneId = 1, IngredienteId = 2, Quantita = 50.0m, UnitaMisuraId = 1 },
        //        new PersonalizzazioneIngredienteDTO { PersonalizzazioneId = 2, IngredienteId = 1, Quantita = 75.0m, UnitaMisuraId = 1 }
        //    };

        //    foreach (var personalizzazioneIngrediente in personalizzazioneIngredienti)
        //    {
        //        await _personalizzazioneIngredienteRepository.AddAsync(personalizzazioneIngrediente);
        //    }

        //    // Act
        //    var result = await _personalizzazioneIngredienteRepository.GetAllAsync();

        //    // Assert
        //    Assert.Equal(3, result.Count());
        //}

        //private async Task CleanTablesForPersonalizzazioneIngredienteTest()
        //{
        //    await CleanTableAsync<PersonalizzazioneIngrediente>();
        //    await CleanTableAsync<Personalizzazione>();
        //    await CleanTableAsync<Ingrediente>();
        //    await CleanTableAsync<UnitaDiMisura>();
        //    await CleanTableAsync<CategoriaIngrediente>();

        //    // Setup CategoriaIngrediente
        //    var categoria = new Database.CategoriaIngrediente
        //    {
        //        Categoria = "Test Categoria"
        //    };
        //    _context.CategoriaIngrediente.Add(categoria);

        //    // Setup UnitaDiMisura
        //    var unitaMisura = new Database.UnitaDiMisura
        //    {
        //        Sigla = "ml",
        //        Descrizione = "Millilitri"
        //    };
        //    _context.UnitaDiMisura.Add(unitaMisura);

        //    await _context.SaveChangesAsync();

        //    // Setup Ingrediente
        //    var ingrediente1 = new Database.Ingrediente
        //    {
        //        Ingrediente1 = "Test Ingrediente 1",
        //        CategoriaId = categoria.CategoriaId,
        //        PrezzoAggiunto = 1.0m,
        //        Disponibile = true
        //    };
        //    _context.Ingrediente.Add(ingrediente1);

        //    var ingrediente2 = new Database.Ingrediente
        //    {
        //        Ingrediente1 = "Test Ingrediente 2",
        //        CategoriaId = categoria.CategoriaId,
        //        PrezzoAggiunto = 2.0m,
        //        Disponibile = true
        //    };
        //    _context.Ingrediente.Add(ingrediente2);

        //    // Setup Personalizzazione
        //    var personalizzazione1 = new Database.Personalizzazione
        //    {
        //        Nome = "Test Personalizzazione 1",
        //        Descrizione = "Descrizione test 1",
        //        DtCreazione = DateTime.Now                
        //    };
        //    _context.Personalizzazione.Add(personalizzazione1);

        //    var personalizzazione2 = new Database.Personalizzazione
        //    {
        //        Nome = "Test Personalizzazione 2",
        //        Descrizione = "Descrizione test 2",
        //        DtCreazione = DateTime.Now                
        //    };
        //    _context.Personalizzazione.Add(personalizzazione2);

        //    await _context.SaveChangesAsync();
        //}

        [Fact]
        public async Task UpdateAsync_Should_Not_Throw_For_NonExisting_Id()
        {
            // Arrange
            var updateDto = new PersonalizzazioneIngredienteDTO
            {
                PersonalizzazioneIngredienteId = 999,
                PersonalizzazioneId = 1,
                IngredienteId = 1,
                Quantita = 100.0m,
                UnitaMisuraId = 1
            };

            // Act & Assert - ✅ SILENT FAIL, NO EXCEPTION
            var exception = await Record.ExceptionAsync(() =>
                _personalizzazioneIngredienteRepository.UpdateAsync(updateDto)
            );

            Assert.Null(exception); // ✅ Conferma che non viene lanciata alcuna eccezione
        }

        //[Fact]
        //public async Task AddAsync_Should_Throw_For_Duplicate_Personalizzazione_Ingrediente()
        //{
        //    // Arrange
        //    await CleanTablesForPersonalizzazioneIngredienteTest();

        //    var personalizzazioneIngrediente1 = new PersonalizzazioneIngredienteDTO
        //    {
        //        PersonalizzazioneId = 1,
        //        IngredienteId = 1,
        //        Quantita = 100.0m,
        //        UnitaMisuraId = 1
        //    };
        //    await _personalizzazioneIngredienteRepository.AddAsync(personalizzazioneIngrediente1);

        //    var personalizzazioneIngrediente2 = new PersonalizzazioneIngredienteDTO
        //    {
        //        PersonalizzazioneId = 1, // ✅ STESSA COMBINAZIONE
        //        IngredienteId = 1,       // ✅ STESSA COMBINAZIONE
        //        Quantita = 200.0m,
        //        UnitaMisuraId = 2
        //    };

        //    // Act & Assert
        //    var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
        //        _personalizzazioneIngredienteRepository.AddAsync(personalizzazioneIngrediente2)
        //    );

        //    Assert.Contains("esiste già", exception.Message.ToLower());
        //}
    }
}