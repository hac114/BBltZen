using DTO;
using Repository.Interface;
using Repository.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace RepositoryTest
{
    public class PersonalizzazioneRepositoryTest : BaseTest
    {
        private readonly IPersonalizzazioneRepository _personalizzazioneRepository;

        public PersonalizzazioneRepositoryTest()
        {
            _personalizzazioneRepository = new PersonalizzazioneRepository(_context);
        }

        [Fact]
        public async Task AddAsync_Should_Add_Personalizzazione()
        {
            // Arrange
            await CleanTableAsync<Database.Personalizzazione>();

            var personalizzazioneDto = new PersonalizzazioneDTO
            {
                Nome = "Test Personalizzazione",
                Descrizione = "Descrizione test"
            };

            // Act
            await _personalizzazioneRepository.AddAsync(personalizzazioneDto);

            // Assert
            var result = await _personalizzazioneRepository.GetByIdAsync(personalizzazioneDto.PersonalizzazioneId);
            Assert.NotNull(result);
            Assert.Equal("Test Personalizzazione", result.Nome);
            Assert.Equal("Descrizione test", result.Descrizione);
            Assert.NotNull(result.DtCreazione);
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Personalizzazione()
        {
            // Arrange
            await CleanTableAsync<Database.Personalizzazione>();

            var personalizzazioneDto = new PersonalizzazioneDTO
            {
                Nome = "Personalizzazione Test",
                Descrizione = "Descrizione di test"
            };
            await _personalizzazioneRepository.AddAsync(personalizzazioneDto);

            // Act
            var result = await _personalizzazioneRepository.GetByIdAsync(personalizzazioneDto.PersonalizzazioneId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(personalizzazioneDto.PersonalizzazioneId, result.PersonalizzazioneId);
            Assert.Equal("Personalizzazione Test", result.Nome);
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_For_NonExisting_Id()
        {
            // Act
            var result = await _personalizzazioneRepository.GetByIdAsync(999);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetAllAsync_Should_Return_All_Personalizzazioni()
        {
            // Arrange
            await CleanTableAsync<Database.Personalizzazione>();

            var personalizzazioni = new List<PersonalizzazioneDTO>
            {
                new PersonalizzazioneDTO { Nome = "Personalizzazione 1", Descrizione = "Descrizione 1" },
                new PersonalizzazioneDTO { Nome = "Personalizzazione 2", Descrizione = "Descrizione 2" },
                new PersonalizzazioneDTO { Nome = "Personalizzazione 3", Descrizione = "Descrizione 3" }
            };

            foreach (var personalizzazione in personalizzazioni)
            {
                await _personalizzazioneRepository.AddAsync(personalizzazione);
            }

            // Act
            var result = await _personalizzazioneRepository.GetAllAsync();

            // Assert
            Assert.Equal(3, result.Count());
        }

        [Fact]
        public async Task UpdateAsync_Should_Update_Personalizzazione()
        {
            // Arrange
            await CleanTableAsync<Database.Personalizzazione>();

            var personalizzazioneDto = new PersonalizzazioneDTO
            {
                Nome = "Originale",
                Descrizione = "Descrizione originale"
            };
            await _personalizzazioneRepository.AddAsync(personalizzazioneDto);

            var updateDto = new PersonalizzazioneDTO
            {
                PersonalizzazioneId = personalizzazioneDto.PersonalizzazioneId,
                Nome = "Aggiornata",
                Descrizione = "Descrizione aggiornata"
            };

            // Act
            await _personalizzazioneRepository.UpdateAsync(updateDto);

            // Assert
            var updated = await _personalizzazioneRepository.GetByIdAsync(personalizzazioneDto.PersonalizzazioneId);
            Assert.NotNull(updated);
            Assert.Equal("Aggiornata", updated.Nome);
            Assert.Equal("Descrizione aggiornata", updated.Descrizione);
        }

        [Fact]
        public async Task UpdateAsync_Should_Throw_For_NonExisting_Id()
        {
            // Arrange
            var updateDto = new PersonalizzazioneDTO
            {
                PersonalizzazioneId = 999,
                Nome = "Inesistente",
                Descrizione = "Descrizione"
            };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _personalizzazioneRepository.UpdateAsync(updateDto));
        }

        [Fact]
        public async Task ExistsAsync_Should_Return_True_For_Existing_Personalizzazione()
        {
            // Arrange
            await CleanTableAsync<Database.Personalizzazione>();

            var personalizzazioneDto = new PersonalizzazioneDTO
            {
                Nome = "Test Esistenza",
                Descrizione = "Descrizione"
            };
            await _personalizzazioneRepository.AddAsync(personalizzazioneDto);

            // Act
            var exists = await _personalizzazioneRepository.ExistsAsync(personalizzazioneDto.PersonalizzazioneId);

            // Assert
            Assert.True(exists);
        }

        [Fact]
        public async Task ExistsAsync_Should_Return_False_For_NonExisting_Personalizzazione()
        {
            // Act
            var exists = await _personalizzazioneRepository.ExistsAsync(999);

            // Assert
            Assert.False(exists);
        }

        [Fact]
        public async Task ExistsByNameAsync_Should_Return_True_For_Existing_Name()
        {
            // Arrange
            await CleanTableAsync<Database.Personalizzazione>();

            var personalizzazioneDto = new PersonalizzazioneDTO
            {
                Nome = "Nome Unico",
                Descrizione = "Descrizione"
            };
            await _personalizzazioneRepository.AddAsync(personalizzazioneDto);

            // Act
            var exists = await _personalizzazioneRepository.ExistsByNameAsync("Nome Unico");

            // Assert
            Assert.True(exists);
        }

        [Fact]
        public async Task ExistsByNameAsync_Should_Return_False_For_NonExisting_Name()
        {
            // Act
            var exists = await _personalizzazioneRepository.ExistsByNameAsync("Nome Inesistente");

            // Assert
            Assert.False(exists);
        }

        [Fact]
        public async Task AddAsync_Should_Assign_Generated_Id_And_Timestamp()
        {
            // Arrange
            await CleanTableAsync<Database.Personalizzazione>();

            var personalizzazioneDto = new PersonalizzazioneDTO
            {
                Nome = "Test Timestamps",
                Descrizione = "Descrizione"
            };

            // Act
            await _personalizzazioneRepository.AddAsync(personalizzazioneDto);

            // Assert
            Assert.True(personalizzazioneDto.PersonalizzazioneId > 0);
            Assert.NotNull(personalizzazioneDto.DtCreazione);
        }

        [Fact]
        public async Task DeleteAsync_Should_Permanently_Delete_Personalizzazione()
        {
            // Arrange
            await CleanTableAsync<Database.Personalizzazione>();

            var personalizzazioneDto = new PersonalizzazioneDTO
            {
                Nome = "Da Eliminare Definitivamente",
                Descrizione = "Descrizione"
            };
            await _personalizzazioneRepository.AddAsync(personalizzazioneDto);

            // Verifica che esista prima del delete
            var beforeDelete = await _personalizzazioneRepository.GetByIdAsync(personalizzazioneDto.PersonalizzazioneId);
            Assert.NotNull(beforeDelete);

            // Act
            await _personalizzazioneRepository.DeleteAsync(personalizzazioneDto.PersonalizzazioneId);

            // Assert - Non dovrebbe esistere più nel database
            var afterDelete = await _personalizzazioneRepository.GetByIdAsync(personalizzazioneDto.PersonalizzazioneId);
            Assert.Null(afterDelete);

            // Verifica anche a livello di context
            var inDb = await _context.Personalizzazione.FindAsync(personalizzazioneDto.PersonalizzazioneId);
            Assert.Null(inDb);
        }

        [Fact]
        public async Task DeleteAsync_Should_Not_Throw_For_NonExisting_Id()
        {
            // Act & Assert - Non dovrebbe lanciare eccezioni
            await _personalizzazioneRepository.DeleteAsync(999);
        }

        [Fact]
        public async Task DeleteAsync_Should_Throw_When_Personalizzazione_Has_Dependencies()
        {
            // Arrange
            await CleanTableAsync<Database.Personalizzazione>();
            await CleanTableAsync<Database.PersonalizzazioneIngrediente>();

            var personalizzazioneDto = new PersonalizzazioneDTO
            {
                Nome = "Personalizzazione Con Dipendenze",
                Descrizione = "Descrizione"
            };
            await _personalizzazioneRepository.AddAsync(personalizzazioneDto);

            var ingredientePersonalizzazione = new Database.PersonalizzazioneIngrediente
            {
                PersonalizzazioneId = personalizzazioneDto.PersonalizzazioneId,
                IngredienteId = 1,
                Quantita = 1,
                UnitaMisuraId = 1
            };
            _context.PersonalizzazioneIngrediente.Add(ingredientePersonalizzazione);
            await _context.SaveChangesAsync();

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _personalizzazioneRepository.DeleteAsync(personalizzazioneDto.PersonalizzazioneId)
            );

            // ✅ VERIFICA CHE L'ECCEZIONE SIA DEL TIPO GIUSTO E ABBIA IL MESSAGGIO
            Assert.NotNull(exception);
            Assert.IsType<InvalidOperationException>(exception);

            // ✅ VERIFICA CHE IL MESSAGGIO CONTENGA LE PAROLE CHIAVE ITALIANE
            var message = exception.Message.ToLower();
            Assert.Contains("impossibile", message);
            Assert.Contains("eliminare", message);
            Assert.Contains("personalizzazione", message);
            Assert.Contains("collegata", message);
        }

        [Fact]
        public async Task DeleteAsync_Should_Work_When_No_Dependencies()
        {
            // Arrange
            await CleanTableAsync<Database.Personalizzazione>();

            var personalizzazioneDto = new PersonalizzazioneDTO
            {
                Nome = "Senza Dipendenze",
                Descrizione = "Descrizione"
            };
            await _personalizzazioneRepository.AddAsync(personalizzazioneDto);

            // Act - Dovrebbe funzionare senza eccezioni
            await _personalizzazioneRepository.DeleteAsync(personalizzazioneDto.PersonalizzazioneId);

            // Assert
            var exists = await _personalizzazioneRepository.ExistsAsync(personalizzazioneDto.PersonalizzazioneId);
            Assert.False(exists);
        }
    }
}