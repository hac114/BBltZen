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
    public class UnitaDiMisuraRepositoryTest : BaseTest
    {
        private readonly IUnitaDiMisuraRepository _unitaDiMisuraRepository;

        public UnitaDiMisuraRepositoryTest()
        {
            // ✅ CREA IL REPOSITORY SPECIFICO USANDO IL CONTEXT EREDITATO
            _unitaDiMisuraRepository = new UnitaDiMisuraRepository(_context);
        }

        [Fact]
        public async Task AddAsync_Should_Add_UnitaDiMisura()
        {
            // Arrange
            var unitaDto = new UnitaDiMisuraDTO
            {
                Sigla = "ml",
                Descrizione = "Millilitri"
            };

            // Act
            var result = await _unitaDiMisuraRepository.AddAsync(unitaDto); // ✅ CORREGGI: assegna risultato

            // Assert
            Assert.NotNull(result);
            Assert.True(result.UnitaMisuraId > 0); // ✅ VERIFICA ID generato
            Assert.Equal("ml", result.Sigla);
            Assert.Equal("Millilitri", result.Descrizione);
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Correct_UnitaDiMisura()
        {
            // Arrange
            var unitaDto = new UnitaDiMisuraDTO
            {
                Sigla = "g",
                Descrizione = "Grammi"
            };
            var addedUnita = await _unitaDiMisuraRepository.AddAsync(unitaDto); // ✅ CORREGGI: assegna risultato

            // Act
            var result = await _unitaDiMisuraRepository.GetByIdAsync(addedUnita.UnitaMisuraId); // ✅ USA ID dal risultato

            // Assert
            Assert.NotNull(result);
            Assert.Equal(addedUnita.UnitaMisuraId, result.UnitaMisuraId);
            Assert.Equal("g", result.Sigla);
            Assert.Equal("Grammi", result.Descrizione);
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_For_NonExisting_Id()
        {
            // Act
            var result = await _unitaDiMisuraRepository.GetByIdAsync(999);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetAllAsync_Should_Return_All_UnitaDiMisura()
        {
            // Arrange
            var unitaList = new List<UnitaDiMisuraDTO>
    {
        new UnitaDiMisuraDTO { Sigla = "ml", Descrizione = "Millilitri" },
        new UnitaDiMisuraDTO { Sigla = "g", Descrizione = "Grammi" },
        new UnitaDiMisuraDTO { Sigla = "cl", Descrizione = "Centilitri" }
    };

            // ✅ CORREGGI: Assegna i risultati per ottenere gli ID
            var addedUnita = new List<UnitaDiMisuraDTO>();
            foreach (var unita in unitaList)
            {
                var result1 = await _unitaDiMisuraRepository.AddAsync(unita);
                addedUnita.Add(result1);
            }

            // Act
            var result = await _unitaDiMisuraRepository.GetAllAsync();

            // Assert
            Assert.Equal(3, result.Count()); // ✅ CORREGGI: Count() per IEnumerable
            Assert.Contains(result, u => u.Sigla == "ml");
            Assert.Contains(result, u => u.Sigla == "g");
            Assert.Contains(result, u => u.Sigla == "cl");

            // ✅ VERIFICA CHE TUTTI GLI ID SIANO STATI GENERATI
            Assert.All(addedUnita, u => Assert.True(u.UnitaMisuraId > 0));
        }

        [Fact]
        public async Task UpdateAsync_Should_Update_UnitaDiMisura_Correctly()
        {
            // Arrange
            var unitaDto = new UnitaDiMisuraDTO
            {
                Sigla = "ml",
                Descrizione = "Millilitri"
            };
            var addedUnita = await _unitaDiMisuraRepository.AddAsync(unitaDto); // ✅ CORREGGI: assegna risultato

            var updateDto = new UnitaDiMisuraDTO
            {
                UnitaMisuraId = addedUnita.UnitaMisuraId, // ✅ USA ID dal risultato
                Sigla = "ML",
                Descrizione = "MILLILITRI"
            };

            // Act
            await _unitaDiMisuraRepository.UpdateAsync(updateDto);

            // Assert
            var updated = await _unitaDiMisuraRepository.GetByIdAsync(addedUnita.UnitaMisuraId);
            Assert.NotNull(updated);
            Assert.Equal("ML", updated.Sigla);
            Assert.Equal("MILLILITRI", updated.Descrizione);
        }

        [Fact]
        public async Task UpdateAsync_Should_Not_Throw_For_NonExisting_Id()
        {
            // Arrange
            var updateDto = new UnitaDiMisuraDTO
            {
                UnitaMisuraId = 999,
                Sigla = "test",
                Descrizione = "test"
            };

            // Act & Assert - Non dovrebbe lanciare eccezioni
            await _unitaDiMisuraRepository.UpdateAsync(updateDto);
        }

        [Fact]
        public async Task DeleteAsync_Should_Remove_UnitaDiMisura()
        {
            // Arrange
            var unitaDto = new UnitaDiMisuraDTO
            {
                Sigla = "oz",
                Descrizione = "Oncie"
            };
            var addedUnita = await _unitaDiMisuraRepository.AddAsync(unitaDto); // ✅ CORREGGI: assegna risultato

            // Act
            await _unitaDiMisuraRepository.DeleteAsync(addedUnita.UnitaMisuraId); // ✅ USA ID dal risultato

            // Assert
            var deleted = await _unitaDiMisuraRepository.GetByIdAsync(addedUnita.UnitaMisuraId);
            Assert.Null(deleted);
        }

        [Fact]
        public async Task DeleteAsync_Should_Not_Throw_For_NonExisting_Id()
        {
            // Act & Assert - Non dovrebbe lanciare eccezioni
            await _unitaDiMisuraRepository.DeleteAsync(999);
        }

        [Fact]
        public async Task AddAsync_Should_Assign_Generated_Id()
        {
            // Arrange
            var unitaDto = new UnitaDiMisuraDTO
            {
                Sigla = "lt",
                Descrizione = "Litri"
            };

            // Act
            var result = await _unitaDiMisuraRepository.AddAsync(unitaDto); // ✅ CORREGGI: assegna risultato

            // Assert
            Assert.True(result.UnitaMisuraId > 0); // ✅ VERIFICA sul risultato
        }

        [Fact]
        public async Task GetAllAsync_Should_Return_Empty_List_When_No_Data()
        {
            // Act
            var result = await _unitaDiMisuraRepository.GetAllAsync();

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task ExistsAsync_Should_Return_Correct_Value()
        {
            // Arrange
            var unitaDto = new UnitaDiMisuraDTO
            {
                Sigla = "kg",
                Descrizione = "Chilogrammi"
            };
            var addedUnita = await _unitaDiMisuraRepository.AddAsync(unitaDto);

            // Act & Assert
            Assert.True(await _unitaDiMisuraRepository.ExistsAsync(addedUnita.UnitaMisuraId));
            Assert.False(await _unitaDiMisuraRepository.ExistsAsync(999));
        }

        [Fact]
        public async Task SiglaExistsAsync_Should_Return_Correct_Value()
        {
            // Arrange
            var unitaDto = new UnitaDiMisuraDTO
            {
                Sigla = "cm",
                Descrizione = "Centimetri"
            };
            await _unitaDiMisuraRepository.AddAsync(unitaDto);

            // Act & Assert
            Assert.True(await _unitaDiMisuraRepository.SiglaExistsAsync("cm"));
            Assert.False(await _unitaDiMisuraRepository.SiglaExistsAsync("km"));
        }

        [Fact]
        public async Task SiglaExistsForOtherAsync_Should_Return_Correct_Value()
        {
            // Arrange
            var unita1 = await _unitaDiMisuraRepository.AddAsync(new UnitaDiMisuraDTO
            {
                Sigla = "mm",
                Descrizione = "Millimetri"
            });

            var unita2 = await _unitaDiMisuraRepository.AddAsync(new UnitaDiMisuraDTO
            {
                Sigla = "dm",
                Descrizione = "Decimetri"
            });

            // Act & Assert
            Assert.True(await _unitaDiMisuraRepository.SiglaExistsForOtherAsync(unita1.UnitaMisuraId, "dm"));
            Assert.False(await _unitaDiMisuraRepository.SiglaExistsForOtherAsync(unita1.UnitaMisuraId, "mm"));
            Assert.False(await _unitaDiMisuraRepository.SiglaExistsForOtherAsync(unita1.UnitaMisuraId, "km"));
        }

        [Fact]
        public async Task GetBySiglaAsync_Should_Return_Correct_Unita()
        {
            // Arrange
            var unitaDto = new UnitaDiMisuraDTO
            {
                Sigla = "lb",
                Descrizione = "Libbre"
            };
            await _unitaDiMisuraRepository.AddAsync(unitaDto);

            // Act
            var result = await _unitaDiMisuraRepository.GetBySiglaAsync("lb");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("lb", result.Sigla);
            Assert.Equal("Libbre", result.Descrizione);
        }
    }
}