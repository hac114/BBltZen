using Database;
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
        private readonly UnitaDiMisuraRepository _unitaDiMisuraRepository;

        public UnitaDiMisuraRepositoryTest()
        {
            _unitaDiMisuraRepository = new UnitaDiMisuraRepository(_context,
                Microsoft.Extensions.Logging.Abstractions.NullLogger<UnitaDiMisuraRepository>.Instance);
            CleanTableAsync<UnitaDiMisura>().Wait();
        }

        [Fact]
        public async Task AddAsync_Should_Add_UnitaDiMisura()
        {
            // Arrange
            var unitaDto = new UnitaDiMisuraDTO { Sigla = "ML", Descrizione = "Millilitri" };

            // Act
            var result = await _unitaDiMisuraRepository.AddAsync(unitaDto);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.UnitaMisuraId > 0);
            Assert.Equal("ML", result.Sigla);
            Assert.Equal("Millilitri", result.Descrizione);
        }

        [Fact]
        public async Task AddAsync_Should_Throw_When_Duplicate_Sigla()
        {
            // Arrange
            await _unitaDiMisuraRepository.AddAsync(new UnitaDiMisuraDTO { Sigla = "GR", Descrizione = "Grammi" });
            var duplicateDto = new UnitaDiMisuraDTO { Sigla = "GR", Descrizione = "Altro" };

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _unitaDiMisuraRepository.AddAsync(duplicateDto));
        }

        [Fact]
        public async Task AddAsync_Should_Throw_When_Duplicate_Descrizione()
        {
            // Arrange
            await _unitaDiMisuraRepository.AddAsync(new UnitaDiMisuraDTO { Sigla = "GR", Descrizione = "Grammi" });
            var duplicateDto = new UnitaDiMisuraDTO { Sigla = "GM", Descrizione = "Grammi" };

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _unitaDiMisuraRepository.AddAsync(duplicateDto));
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Correct_UnitaDiMisura()
        {
            // Arrange
            var addedUnita = await _unitaDiMisuraRepository.AddAsync(
                new UnitaDiMisuraDTO { Sigla = "GR", Descrizione = "Grammi" });

            // Act
            var result = await _unitaDiMisuraRepository.GetByIdAsync(addedUnita.UnitaMisuraId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(addedUnita.UnitaMisuraId, result.UnitaMisuraId);
            Assert.Equal("GR", result.Sigla);
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
        public async Task GetAllAsync_Should_Return_Paginated_Results()
        {
            // Arrange
            var unitaList = new List<UnitaDiMisuraDTO>
            {
                new UnitaDiMisuraDTO { Sigla = "ML", Descrizione = "Millilitri" },
                new UnitaDiMisuraDTO { Sigla = "GR", Descrizione = "Grammi" },
                new UnitaDiMisuraDTO { Sigla = "CL", Descrizione = "Centilitri" }
            };

            foreach (var unita in unitaList)
                await _unitaDiMisuraRepository.AddAsync(unita);

            // Act
            var result = await _unitaDiMisuraRepository.GetAllAsync(page: 1, pageSize: 2);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<PaginatedResponseDTO<UnitaDiMisuraDTO>>(result);
            Assert.Equal(2, result.Data.Count());
            Assert.Equal(3, result.TotalCount);
            Assert.Equal(1, result.Page);
            Assert.Equal(2, result.PageSize);
        }

        [Fact]
        public async Task UpdateAsync_Should_Update_UnitaDiMisura_Correctly()
        {
            // Arrange
            var addedUnita = await _unitaDiMisuraRepository.AddAsync(
                new UnitaDiMisuraDTO { Sigla = "ML", Descrizione = "Millilitri" });

            var updateDto = new UnitaDiMisuraDTO
            {
                UnitaMisuraId = addedUnita.UnitaMisuraId,
                Sigla = "LT",
                Descrizione = "Litri"
            };

            // Act
            await _unitaDiMisuraRepository.UpdateAsync(updateDto);

            // Assert
            var updated = await _unitaDiMisuraRepository.GetByIdAsync(addedUnita.UnitaMisuraId);
            Assert.NotNull(updated);
            Assert.Equal("LT", updated.Sigla);
            Assert.Equal("Litri", updated.Descrizione);
        }

        [Fact]
        public async Task UpdateAsync_Should_Throw_When_Entity_Not_Found()
        {
            // Arrange
            await CleanTableAsync<UnitaDiMisura>(); // ✅ USA IL METODO EREDITATO

            var updateDto = new UnitaDiMisuraDTO
            {
                UnitaMisuraId = 999,
                Sigla = "TE", // ✅ 2 CARATTERI
                Descrizione = "Test"
            };

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                _unitaDiMisuraRepository.UpdateAsync(updateDto));
        }

        [Fact]
        public async Task UpdateAsync_Should_Throw_When_Duplicate_Sigla()
        {
            // Arrange
            var unita1 = await _unitaDiMisuraRepository.AddAsync(
                new UnitaDiMisuraDTO { Sigla = "GR", Descrizione = "Grammi" });
            var unita2 = await _unitaDiMisuraRepository.AddAsync(
                new UnitaDiMisuraDTO { Sigla = "ML", Descrizione = "Millilitri" });

            var updateDto = new UnitaDiMisuraDTO
            {
                UnitaMisuraId = unita2.UnitaMisuraId,
                Sigla = "GR", // Duplicata!
                Descrizione = "Millilitri"
            };

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _unitaDiMisuraRepository.UpdateAsync(updateDto));
        }

        [Fact]
        public async Task DeleteAsync_Should_Remove_UnitaDiMisura()
        {
            // Arrange
            var addedUnita = await _unitaDiMisuraRepository.AddAsync(
                new UnitaDiMisuraDTO { Sigla = "OZ", Descrizione = "Oncie" });

            // Act
            await _unitaDiMisuraRepository.DeleteAsync(addedUnita.UnitaMisuraId);

            // Assert
            var deleted = await _unitaDiMisuraRepository.GetByIdAsync(addedUnita.UnitaMisuraId);
            Assert.Null(deleted);
        }

        [Fact]
        public async Task DeleteAsync_Should_Throw_When_Entity_Not_Found()
        {
            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                _unitaDiMisuraRepository.DeleteAsync(999));
        }

        [Fact]
        public async Task ExistsAsync_Should_Return_Correct_Value()
        {
            // Arrange
            var addedUnita = await _unitaDiMisuraRepository.AddAsync(
                new UnitaDiMisuraDTO { Sigla = "KG", Descrizione = "Chilogrammi" });

            // Act & Assert
            Assert.True(await _unitaDiMisuraRepository.ExistsAsync(addedUnita.UnitaMisuraId));
            Assert.False(await _unitaDiMisuraRepository.ExistsAsync(999));
        }

        [Fact]
        public async Task GetBySiglaAsync_Should_Return_Paginated_Results()
        {
            // Arrange
            await _unitaDiMisuraRepository.AddAsync(new UnitaDiMisuraDTO { Sigla = "ML", Descrizione = "Millilitri" });
            await _unitaDiMisuraRepository.AddAsync(new UnitaDiMisuraDTO { Sigla = "MG", Descrizione = "Milligrammi" });
            await _unitaDiMisuraRepository.AddAsync(new UnitaDiMisuraDTO { Sigla = "GR", Descrizione = "Grammi" });

            // Act
            var result = await _unitaDiMisuraRepository.GetBySiglaAsync("M", page: 1, pageSize: 10);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.TotalCount); // ML e MG
        }

        [Fact]
        public async Task GetBySiglaAsync_Should_Be_Case_Insensitive()
        {
            // Arrange
            await _unitaDiMisuraRepository.AddAsync(
                new UnitaDiMisuraDTO { Sigla = "ML", Descrizione = "Millilitri" });

            // Act
            var result = await _unitaDiMisuraRepository.GetBySiglaAsync("ml", page: 1, pageSize: 10);

            // Assert
            Assert.Equal(1, result.TotalCount);
        }

        [Fact]
        public async Task GetBySiglaPerFrontendAsync_Should_Return_Paginated_FrontendDTOs()
        {
            // Arrange
            await _unitaDiMisuraRepository.AddAsync(new UnitaDiMisuraDTO { Sigla = "ML", Descrizione = "Millilitri" });
            await _unitaDiMisuraRepository.AddAsync(new UnitaDiMisuraDTO { Sigla = "GR", Descrizione = "Grammi" });

            // Act
            var result = await _unitaDiMisuraRepository.GetBySiglaPerFrontendAsync("M", page: 1, pageSize: 10);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<PaginatedResponseDTO<UnitaDiMisuraFrontendDTO>>(result);
            Assert.Equal(1, result.TotalCount); // Solo ML
            Assert.All(result.Data, dto =>
            {
                Assert.StartsWith("M", dto.Sigla, StringComparison.OrdinalIgnoreCase);
                // Verifica che non ci sia ID
                Assert.Null(dto.GetType().GetProperty("UnitaMisuraId")?.GetValue(dto));
            });
        }

        [Fact]
        public async Task GetByDescrizioneAsync_Should_Return_Paginated_Results()
        {
            // Arrange
            await _unitaDiMisuraRepository.AddAsync(new UnitaDiMisuraDTO { Sigla = "ML", Descrizione = "Millilitri" });
            await _unitaDiMisuraRepository.AddAsync(new UnitaDiMisuraDTO { Sigla = "MG", Descrizione = "Milligrammi" });

            // Act
            var result = await _unitaDiMisuraRepository.GetByDescrizioneAsync("Milli", page: 1, pageSize: 10);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.TotalCount);
        }

        [Fact]
        public async Task GetByDescrizionePerFrontendAsync_Should_Return_Paginated_FrontendDTOs()
        {
            // Arrange
            await _unitaDiMisuraRepository.AddAsync(new UnitaDiMisuraDTO { Sigla = "ML", Descrizione = "Millilitri" });
            await _unitaDiMisuraRepository.AddAsync(new UnitaDiMisuraDTO { Sigla = "GR", Descrizione = "Grammi" });

            // Act
            var result = await _unitaDiMisuraRepository.GetByDescrizionePerFrontendAsync("Gram", page: 1, pageSize: 10);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<PaginatedResponseDTO<UnitaDiMisuraFrontendDTO>>(result);
            Assert.Equal(1, result.TotalCount); // Solo Grammi
            Assert.All(result.Data, dto =>
            {
                Assert.StartsWith("Gram", dto.Descrizione, StringComparison.OrdinalIgnoreCase);
                Assert.Null(dto.GetType().GetProperty("UnitaMisuraId")?.GetValue(dto));
            });
        }

        [Fact]
        public async Task SiglaExistsAsync_Should_Return_Correct_Value()
        {
            // Arrange
            await _unitaDiMisuraRepository.AddAsync(
                new UnitaDiMisuraDTO { Sigla = "CM", Descrizione = "Centimetri" });

            // Act & Assert
            Assert.True(await _unitaDiMisuraRepository.SiglaExistsAsync("CM"));
            Assert.False(await _unitaDiMisuraRepository.SiglaExistsAsync("KM"));
        }

        [Fact]
        public async Task SiglaExistsForOtherAsync_Should_Return_Correct_Value()
        {
            // Arrange
            var unita1 = await _unitaDiMisuraRepository.AddAsync(
                new UnitaDiMisuraDTO { Sigla = "MM", Descrizione = "Millimetri" });
            var unita2 = await _unitaDiMisuraRepository.AddAsync(
                new UnitaDiMisuraDTO { Sigla = "DM", Descrizione = "Decimetri" });

            // Act & Assert
            Assert.True(await _unitaDiMisuraRepository.SiglaExistsForOtherAsync(unita1.UnitaMisuraId, "DM"));
            Assert.False(await _unitaDiMisuraRepository.SiglaExistsForOtherAsync(unita1.UnitaMisuraId, "MM"));
            Assert.False(await _unitaDiMisuraRepository.SiglaExistsForOtherAsync(unita1.UnitaMisuraId, "KM"));
        }

        [Fact]
        public async Task DescrizioneExistsAsync_Should_Return_Correct_Value()
        {
            // Arrange
            await _unitaDiMisuraRepository.AddAsync(
                new UnitaDiMisuraDTO { Sigla = "CM", Descrizione = "Centimetri" });

            // Act & Assert
            Assert.True(await _unitaDiMisuraRepository.DescrizioneExistsAsync("Centimetri"));
            Assert.False(await _unitaDiMisuraRepository.DescrizioneExistsAsync("Kilometri"));
        }

        [Fact]
        public async Task DescrizioneExistsForOtherAsync_Should_Return_Correct_Value()
        {
            // Arrange
            var unita1 = await _unitaDiMisuraRepository.AddAsync(
                new UnitaDiMisuraDTO { Sigla = "MM", Descrizione = "Millimetri" });
            var unita2 = await _unitaDiMisuraRepository.AddAsync(
                new UnitaDiMisuraDTO { Sigla = "DM", Descrizione = "Decimetri" });

            // Act & Assert
            Assert.True(await _unitaDiMisuraRepository.DescrizioneExistsForOtherAsync(unita1.UnitaMisuraId, "Decimetri"));
            Assert.False(await _unitaDiMisuraRepository.DescrizioneExistsForOtherAsync(unita1.UnitaMisuraId, "Millimetri"));
            Assert.False(await _unitaDiMisuraRepository.DescrizioneExistsForOtherAsync(unita1.UnitaMisuraId, "Kilometri"));
        }

        [Fact]
        public async Task HasDependenciesAsync_Should_Return_False_InMemory()
        {
            // Arrange
            await CleanTableAsync<UnitaDiMisura>(); // ✅ USA IL METODO EREDITATO

            var unitaDto = new UnitaDiMisuraDTO { Sigla = "TE", Descrizione = "Test" };
            var addedUnita = await _unitaDiMisuraRepository.AddAsync(unitaDto);

            // Act
            var hasDependencies = await _unitaDiMisuraRepository.HasDependenciesAsync(addedUnita.UnitaMisuraId);

            // Assert
            Assert.False(hasDependencies);
        }
    }
}