using DTO;
using Repository.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace RepositoryTest
{
    public class IngredienteRepositoryTest : BaseTest
    {
        [Fact]
        public async Task AddAsync_Should_Add_Ingrediente_As_Available()
        {
            // Arrange
            await CleanTableAsync<Database.Ingrediente>();

            var ingredienteDto = new IngredienteDTO
            {
                Nome = "Tapioca",
                CategoriaId = 1,
                PrezzoAggiunto = 0.50m,
                Disponibile = true
            };

            // Act
            var result = await _ingredienteRepository.AddAsync(ingredienteDto); // ✅ USA IL RISULTATO

            // Assert
            Assert.True(result.IngredienteId > 0); // ✅ VERIFICA ID GENERATO
            Assert.Equal("Tapioca", result.Nome);
            Assert.True(result.Disponibile);

            // Verifica anche nel database
            var inDb = await _context.Ingrediente.FindAsync(result.IngredienteId);
            Assert.NotNull(inDb);
            Assert.True(inDb.Disponibile);
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Ingrediente_Regardless_Of_Availability()
        {
            // Arrange
            await CleanTableAsync<Database.Ingrediente>();

            var ingredienteDto = new IngredienteDTO
            {
                Nome = "Sciroppo di vaniglia",
                CategoriaId = 2,
                PrezzoAggiunto = 0.30m,
                Disponibile = true
            };
            var added = await _ingredienteRepository.AddAsync(ingredienteDto); // ✅ USA IL RISULTATO

            // Act
            var result = await _ingredienteRepository.GetByIdAsync(added.IngredienteId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(added.IngredienteId, result.IngredienteId);
            Assert.Equal("Sciroppo di vaniglia", result.Nome);
            Assert.True(result.Disponibile);
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Unavailable_Ingrediente()
        {
            // Arrange
            await CleanTableAsync<Database.Ingrediente>();

            var ingredienteDto = new IngredienteDTO
            {
                Nome = "Non Disponibile",
                CategoriaId = 1,
                PrezzoAggiunto = 0.50m,
                Disponibile = true
            };
            var added = await _ingredienteRepository.AddAsync(ingredienteDto); // ✅ USA IL RISULTATO

            // Imposta come non disponibile
            await _ingredienteRepository.SetDisponibilitaAsync(added.IngredienteId, false);

            // Act
            var result = await _ingredienteRepository.GetByIdAsync(added.IngredienteId);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Disponibile);
        }

        [Fact]
        public async Task GetAllAsync_Should_Return_All_Ingredienti_Regardless_Of_Availability()
        {
            // Arrange
            await CleanTableAsync<Database.Ingrediente>();

            var ingredienti = new List<IngredienteDTO>
            {
                new IngredienteDTO { Nome = "Tè verde", CategoriaId = 1, PrezzoAggiunto = 0.50m, Disponibile = true },
                new IngredienteDTO { Nome = "Tè nero", CategoriaId = 1, PrezzoAggiunto = 0.50m, Disponibile = false },
                new IngredienteDTO { Nome = "Latte", CategoriaId = 2, PrezzoAggiunto = 0.30m, Disponibile = true }
            };

            foreach (var ingrediente in ingredienti)
            {
                await _ingredienteRepository.AddAsync(ingrediente);
            }

            // Act
            var result = await _ingredienteRepository.GetAllAsync();

            // Assert
            Assert.Equal(3, result.Count());

            // ✅ VERIFICA I VALORI REALI
            var teNero = result.First(i => i.Nome == "Tè nero");
            Assert.False(teNero.Disponibile); // ✅ Ora dovrebbe essere false

            var teVerde = result.First(i => i.Nome == "Tè verde");
            Assert.True(teVerde.Disponibile);

            var latte = result.First(i => i.Nome == "Latte");
            Assert.True(latte.Disponibile);
        }

        [Fact]
        public async Task GetByCategoriaAsync_Should_Return_All_Ingredienti_Of_Category_Regardless_Of_Availability()
        {
            // Arrange
            await CleanTableAsync<Database.Ingrediente>();

            var categoriaId = 1;
            var ingredienti = new List<IngredienteDTO>
            {
                new IngredienteDTO { Nome = "Tè verde", CategoriaId = categoriaId, PrezzoAggiunto = 0.50m, Disponibile = true },
                new IngredienteDTO { Nome = "Tè nero", CategoriaId = categoriaId, PrezzoAggiunto = 0.50m, Disponibile = false },
                new IngredienteDTO { Nome = "Latte", CategoriaId = 2, PrezzoAggiunto = 0.30m, Disponibile = true }
            };

            foreach (var ingrediente in ingredienti)
            {
                await _ingredienteRepository.AddAsync(ingrediente);
            }

            // Act
            var result = await _ingredienteRepository.GetByCategoriaAsync(categoriaId);

            // Assert
            Assert.Equal(2, result.Count());
            Assert.All(result, i => Assert.Equal(categoriaId, i.CategoriaId));

            // ✅ VERIFICA I VALORI REALI
            var teNero = result.First(i => i.Nome == "Tè nero");
            Assert.False(teNero.Disponibile); // ✅ Ora dovrebbe essere false
        }

        [Fact]
        public async Task GetDisponibiliAsync_Should_Return_Only_Available_Ingredienti()
        {
            // Arrange
            await CleanTableAsync<Database.Ingrediente>();

            var ingredienti = new List<IngredienteDTO>
            {
                new IngredienteDTO { Nome = "Tè verde", CategoriaId = 1, PrezzoAggiunto = 0.50m, Disponibile = true },
                new IngredienteDTO { Nome = "Tè nero", CategoriaId = 1, PrezzoAggiunto = 0.50m, Disponibile = false },
                new IngredienteDTO { Nome = "Latte", CategoriaId = 2, PrezzoAggiunto = 0.30m, Disponibile = true }
            };

            foreach (var ingrediente in ingredienti)
            {
                await _ingredienteRepository.AddAsync(ingrediente);
            }

            // Act
            var result = await _ingredienteRepository.GetDisponibiliAsync();

            // Assert
            Assert.Equal(2, result.Count()); // ✅ Solo quelli disponibili
            Assert.All(result, i => Assert.True(i.Disponibile));
            Assert.DoesNotContain(result, i => i.Nome == "Tè nero");
        }

        [Fact]
        public async Task UpdateAsync_Should_Update_Ingrediente_And_Availability()
        {
            // Arrange
            await CleanTableAsync<Database.Ingrediente>();

            var ingredienteDto = new IngredienteDTO
            {
                Nome = "Tè verde",
                CategoriaId = 1,
                PrezzoAggiunto = 0.50m,
                Disponibile = true
            };
            var added = await _ingredienteRepository.AddAsync(ingredienteDto); // ✅ USA IL RISULTATO

            var updateDto = new IngredienteDTO
            {
                IngredienteId = added.IngredienteId,
                Nome = "Tè verde premium",
                CategoriaId = 2,
                PrezzoAggiunto = 0.80m,
                Disponibile = false
            };

            // Act
            await _ingredienteRepository.UpdateAsync(updateDto);

            // Assert
            var updated = await _ingredienteRepository.GetByIdAsync(added.IngredienteId);
            Assert.NotNull(updated);
            Assert.Equal("Tè verde premium", updated.Nome);
            Assert.Equal(2, updated.CategoriaId);
            Assert.Equal(0.80m, updated.PrezzoAggiunto);
            Assert.False(updated.Disponibile);
        }

        [Fact]
        public async Task DeleteAsync_Should_Permanently_Delete_Ingrediente()
        {
            // Arrange
            await CleanTableAsync<Database.Ingrediente>();

            var ingredienteDto = new IngredienteDTO
            {
                Nome = "Tè matcha",
                CategoriaId = 1,
                PrezzoAggiunto = 1.00m,
                Disponibile = true
            };
            var added = await _ingredienteRepository.AddAsync(ingredienteDto); // ✅ USA IL RISULTATO

            // Act
            await _ingredienteRepository.DeleteAsync(added.IngredienteId);

            // Assert
            var afterDelete = await _ingredienteRepository.GetByIdAsync(added.IngredienteId);
            Assert.Null(afterDelete);
        }

        [Fact]
        public async Task DeleteAsync_Should_Not_Throw_For_NonExisting_Id()
        {
            // Act & Assert - Non dovrebbe lanciare eccezioni
            await _ingredienteRepository.DeleteAsync(999);
        }

        [Fact]
        public async Task DeleteAsync_Should_Throw_When_Ingrediente_Has_Dependencies()
        {
            // Arrange
            await CleanTableAsync<Database.Ingrediente>();
            await CleanTableAsync<Database.PersonalizzazioneIngrediente>();

            var ingredienteDto = new IngredienteDTO
            {
                Nome = "Ingrediente Con Dipendenze",
                CategoriaId = 1,
                PrezzoAggiunto = 0.50m,
                Disponibile = true
            };
            var added = await _ingredienteRepository.AddAsync(ingredienteDto);

            // Crea una dipendenza (PersonalizzazioneIngrediente)
            var personalizzazioneIngrediente = new Database.PersonalizzazioneIngrediente
            {
                IngredienteId = added.IngredienteId,
                PersonalizzazioneId = 1,
                Quantita = 1,
                UnitaMisuraId = 1
            };
            _context.PersonalizzazioneIngrediente.Add(personalizzazioneIngrediente);
            await _context.SaveChangesAsync();

            // Act & Assert - Dovrebbe lanciare eccezione per dipendenze
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _ingredienteRepository.DeleteAsync(added.IngredienteId)
            );

            // ✅ VERIFICA LE PAROLE CHIAVE ITALIANE CORRETTE (basate sul messaggio effettivo)
            Assert.Contains("impossibile", exception.Message.ToLower());
            Assert.Contains("eliminare", exception.Message.ToLower());
            Assert.Contains("ingrediente", exception.Message.ToLower());

            // ✅ VERIFICA ALMENO UNA DI QUESTE PAROLE (in base al messaggio effettivo)
            bool hasPersonalizzazioni = exception.Message.ToLower().Contains("personalizzazioni");
            bool hasUtilizzato = exception.Message.ToLower().Contains("utilizzato");
            bool hasEsistenti = exception.Message.ToLower().Contains("esistenti");

            Assert.True(hasPersonalizzazioni || hasUtilizzato || hasEsistenti,
                $"Il messaggio dovrebbe contenere una di queste parole: 'personalizzazioni', 'utilizzato', 'esistenti'. Messaggio: {exception.Message}");
        }

        [Fact]
        public async Task ToggleDisponibilitaAsync_Should_Invert_Availability()
        {
            // Arrange
            await CleanTableAsync<Database.Ingrediente>();

            var ingredienteDto = new IngredienteDTO
            {
                Nome = "Test Toggle",
                CategoriaId = 1,
                PrezzoAggiunto = 0.50m,
                Disponibile = true
            };
            await _ingredienteRepository.AddAsync(ingredienteDto);

            // Partiamo come disponibile (Disponibile = true)
            var initial = await _ingredienteRepository.GetByIdAsync(ingredienteDto.IngredienteId);
            Assert.True(initial.Disponibile);

            // Act - Toggle per disabilitare
            await _ingredienteRepository.ToggleDisponibilitaAsync(ingredienteDto.IngredienteId);

            // Assert - Ora non dovrebbe essere disponibile
            var afterToggle = await _ingredienteRepository.GetByIdAsync(ingredienteDto.IngredienteId);
            Assert.NotNull(afterToggle);
            Assert.False(afterToggle.Disponibile); // ✅ Ora non disponibile

            // Act - Toggle per riabilitare
            await _ingredienteRepository.ToggleDisponibilitaAsync(ingredienteDto.IngredienteId);

            // Assert - Ora dovrebbe essere di nuovo disponibile
            var final = await _ingredienteRepository.GetByIdAsync(ingredienteDto.IngredienteId);
            Assert.NotNull(final);
            Assert.True(final.Disponibile); // ✅ Di nuovo disponibile
        }

        [Fact]
        public async Task SetDisponibilitaAsync_Should_Set_Specific_Availability()
        {
            // Arrange
            await CleanTableAsync<Database.Ingrediente>();

            var ingredienteDto = new IngredienteDTO
            {
                Nome = "Test SetDisponibilita",
                CategoriaId = 1,
                PrezzoAggiunto = 0.50m,
                Disponibile = true
            };
            await _ingredienteRepository.AddAsync(ingredienteDto);

            // Act - Imposta come non disponibile
            await _ingredienteRepository.SetDisponibilitaAsync(ingredienteDto.IngredienteId, false);

            // Assert
            var result = await _ingredienteRepository.GetByIdAsync(ingredienteDto.IngredienteId);
            Assert.NotNull(result);
            Assert.False(result.Disponibile); // ✅ Conferma non disponibile

            // Act - Re-imposta come disponibile
            await _ingredienteRepository.SetDisponibilitaAsync(ingredienteDto.IngredienteId, true);

            // Assert
            var final = await _ingredienteRepository.GetByIdAsync(ingredienteDto.IngredienteId);
            Assert.NotNull(final);
            Assert.True(final.Disponibile); // ✅ Conferma disponibile
        }

        [Fact]
        public async Task ExistsAsync_Should_Return_True_For_Existing_Ingrediente_Regardless_Of_Availability()
        {
            // Arrange
            await CleanTableAsync<Database.Ingrediente>();

            var ingredienteDto = new IngredienteDTO
            {
                Nome = "Test Esistenza",
                CategoriaId = 1,
                PrezzoAggiunto = 0.50m,
                Disponibile = true
            };
            await _ingredienteRepository.AddAsync(ingredienteDto);

            // Imposta come non disponibile
            await _ingredienteRepository.SetDisponibilitaAsync(ingredienteDto.IngredienteId, false);

            // Act - Dovrebbe esistere COMUNQUE
            var exists = await _ingredienteRepository.ExistsAsync(ingredienteDto.IngredienteId);

            // Assert
            Assert.True(exists); // ✅ Esiste anche se non disponibile
        }

        [Fact]
        public async Task ExistsAsync_Should_Return_False_For_NonExisting_Ingrediente()
        {
            // Act
            var exists = await _ingredienteRepository.ExistsAsync(999);

            // Assert
            Assert.False(exists);
        }

        [Fact]
        public async Task AddAsync_Should_Throw_For_Duplicate_Nome()
        {
            // Arrange
            await CleanTableAsync<Database.Ingrediente>();

            var ingrediente1 = new IngredienteDTO
            {
                Nome = "Tè matcha",
                CategoriaId = 1,
                PrezzoAggiunto = 1.00m,
                Disponibile = true
            };
            await _ingredienteRepository.AddAsync(ingrediente1);

            var ingrediente2 = new IngredienteDTO
            {
                Nome = "Tè matcha", // ✅ STESSO NOME
                CategoriaId = 1,
                PrezzoAggiunto = 1.20m,
                Disponibile = true
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
                _ingredienteRepository.AddAsync(ingrediente2)
            );

            Assert.Contains("esiste già", exception.Message.ToLower());
        }

        [Fact]
        public async Task UpdateAsync_Should_Throw_For_Duplicate_Nome()
        {
            // Arrange
            await CleanTableAsync<Database.Ingrediente>();

            var ingrediente1 = new IngredienteDTO
            {
                Nome = "Tè matcha",
                CategoriaId = 1,
                PrezzoAggiunto = 1.00m,
                Disponibile = true
            };
            var added1 = await _ingredienteRepository.AddAsync(ingrediente1);

            var ingrediente2 = new IngredienteDTO
            {
                Nome = "Tè verde",
                CategoriaId = 1,
                PrezzoAggiunto = 0.80m,
                Disponibile = true
            };
            var added2 = await _ingredienteRepository.AddAsync(ingrediente2);

            // Prova ad aggiornare il secondo con il nome del primo
            var updateDto = new IngredienteDTO
            {
                IngredienteId = added2.IngredienteId,
                Nome = "Tè matcha", // ✅ NOME DUPLICATO
                CategoriaId = 1,
                PrezzoAggiunto = 0.80m,
                Disponibile = true
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
                _ingredienteRepository.UpdateAsync(updateDto)
            );

            Assert.Contains("esiste già", exception.Message.ToLower());
        }

        [Fact]
        public async Task UpdateAsync_Should_Not_Throw_For_NonExisting_Id()
        {
            // Arrange
            var nonExistingDto = new IngredienteDTO
            {
                IngredienteId = 999,
                Nome = "Non Esiste",
                CategoriaId = 1,
                PrezzoAggiunto = 1.00m,
                Disponibile = true
            };

            // Act & Assert - ✅ SILENT FAIL, NO EXCEPTION
            var exception = await Record.ExceptionAsync(() =>
                _ingredienteRepository.UpdateAsync(nonExistingDto)
            );

            Assert.Null(exception);
        }
    }
}