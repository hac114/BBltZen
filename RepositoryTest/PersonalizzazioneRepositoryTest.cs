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
            Assert.False(result.IsDeleted);
            Assert.NotNull(result.DtCreazione);
            Assert.NotNull(result.DtUpdate);
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Correct_Personalizzazione()
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
            Assert.Equal("Descrizione di test", result.Descrizione);
            Assert.False(result.IsDeleted);
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
        public async Task GetByIdAsync_Should_Return_Null_For_Deleted_Personalizzazione()
        {
            // Arrange
            await CleanTableAsync<Database.Personalizzazione>();

            var personalizzazioneDto = new PersonalizzazioneDTO
            {
                Nome = "Da Eliminare",
                Descrizione = "Descrizione"
            };
            await _personalizzazioneRepository.AddAsync(personalizzazioneDto);
            await _personalizzazioneRepository.DeleteAsync(personalizzazioneDto.PersonalizzazioneId);

            // Act
            var result = await _personalizzazioneRepository.GetByIdAsync(personalizzazioneDto.PersonalizzazioneId);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetAllAsync_Should_Return_All_Active_Personalizzazioni()
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

            // Elimina una personalizzazione
            await _personalizzazioneRepository.DeleteAsync(personalizzazioni[0].PersonalizzazioneId);

            // Act
            var result = await _personalizzazioneRepository.GetAllAsync();

            // Assert
            Assert.Equal(2, result.Count()); // Solo quelle attive
            Assert.All(result, p => Assert.False(p.IsDeleted));
        }

        [Fact]
        public async Task GetAttiveAsync_Should_Return_Only_Active_Personalizzazioni()
        {
            // Arrange
            await CleanTableAsync<Database.Personalizzazione>();

            var personalizzazioni = new List<PersonalizzazioneDTO>
            {
                new PersonalizzazioneDTO { Nome = "Attiva 1", Descrizione = "Descrizione" },
                new PersonalizzazioneDTO { Nome = "Attiva 2", Descrizione = "Descrizione" },
                new PersonalizzazioneDTO { Nome = "Da Eliminare", Descrizione = "Descrizione" }
            };

            foreach (var personalizzazione in personalizzazioni)
            {
                await _personalizzazioneRepository.AddAsync(personalizzazione);
            }

            // Elimina una personalizzazione
            await _personalizzazioneRepository.DeleteAsync(personalizzazioni[2].PersonalizzazioneId);

            // Act
            var result = await _personalizzazioneRepository.GetAttiveAsync();

            // Assert
            Assert.Equal(2, result.Count());
            Assert.All(result, p => Assert.False(p.IsDeleted));
        }

        [Fact]
        public async Task UpdateAsync_Should_Update_Personalizzazione_Correctly()
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
        public async Task DeleteAsync_Should_SoftDelete_Personalizzazione()
        {
            // Arrange
            await CleanTableAsync<Database.Personalizzazione>();

            var personalizzazioneDto = new PersonalizzazioneDTO
            {
                Nome = "Da Eliminare",
                Descrizione = "Descrizione"
            };
            await _personalizzazioneRepository.AddAsync(personalizzazioneDto);

            // Act
            await _personalizzazioneRepository.DeleteAsync(personalizzazioneDto.PersonalizzazioneId);

            // Assert - Non dovrebbe essere più visibile
            var deleted = await _personalizzazioneRepository.GetByIdAsync(personalizzazioneDto.PersonalizzazioneId);
            Assert.Null(deleted);

            // Ma dovrebbe essere ancora nel database (soft delete)
            var inDb = await _context.Personalizzazione.FindAsync(personalizzazioneDto.PersonalizzazioneId);
            Assert.NotNull(inDb);
            Assert.True(inDb.IsDeleted);
        }

        [Fact]
        public async Task DeleteAsync_Should_Not_Throw_For_NonExisting_Id()
        {
            // Act & Assert - Non dovrebbe lanciare eccezioni
            await _personalizzazioneRepository.DeleteAsync(999);
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
        public async Task AddAsync_Should_Assign_Generated_Id_And_Timestamps()
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
            Assert.NotNull(personalizzazioneDto.DtUpdate);
        }
    }
}