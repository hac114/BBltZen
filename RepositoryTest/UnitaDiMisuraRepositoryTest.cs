using DTO;
using Repository.Interface;
using Repository.Service;
using Database;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace RepositoryTest
{
    public class UnitaDiMisuraRepositoryTest : IDisposable
    {
        private readonly IUnitaDiMisuraRepository _unitaDiMisuraRepository;
        private readonly BubbleTeaContext _context;

        public UnitaDiMisuraRepositoryTest()
        {
            var options = new DbContextOptionsBuilder<BubbleTeaContext>()
                .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
                .Options;

            _context = new BubbleTeaContext(options);
            _context.Database.EnsureCreated();
            _unitaDiMisuraRepository = new UnitaDiMisuraRepository(_context);
        }

        public void Dispose()
        {
            _context?.Database?.EnsureDeleted();
            _context?.Dispose();
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
            await _unitaDiMisuraRepository.AddAsync(unitaDto);

            // Assert
            var result = await _unitaDiMisuraRepository.GetByIdAsync(unitaDto.UnitaMisuraId);
            Assert.NotNull(result);
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
            await _unitaDiMisuraRepository.AddAsync(unitaDto);

            // Act
            var result = await _unitaDiMisuraRepository.GetByIdAsync(unitaDto.UnitaMisuraId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(unitaDto.UnitaMisuraId, result.UnitaMisuraId);
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

            foreach (var unita in unitaList)
            {
                await _unitaDiMisuraRepository.AddAsync(unita);
            }

            // Act
            var result = await _unitaDiMisuraRepository.GetAllAsync();

            // Assert
            Assert.Equal(3, result.Count);
            Assert.Contains(result, u => u.Sigla == "ml");
            Assert.Contains(result, u => u.Sigla == "g");
            Assert.Contains(result, u => u.Sigla == "cl");
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
            await _unitaDiMisuraRepository.AddAsync(unitaDto);

            var updateDto = new UnitaDiMisuraDTO
            {
                UnitaMisuraId = unitaDto.UnitaMisuraId,
                Sigla = "ML",
                Descrizione = "MILLILITRI"
            };

            // Act
            await _unitaDiMisuraRepository.UpdateAsync(updateDto);

            // Assert
            var updated = await _unitaDiMisuraRepository.GetByIdAsync(unitaDto.UnitaMisuraId);
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
            await _unitaDiMisuraRepository.AddAsync(unitaDto);

            // Act
            await _unitaDiMisuraRepository.DeleteAsync(unitaDto.UnitaMisuraId);

            // Assert
            var deleted = await _unitaDiMisuraRepository.GetByIdAsync(unitaDto.UnitaMisuraId);
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
            await _unitaDiMisuraRepository.AddAsync(unitaDto);

            // Assert
            Assert.True(unitaDto.UnitaMisuraId > 0);
        }

        [Fact]
        public async Task GetAllAsync_Should_Return_Empty_List_When_No_Data()
        {
            // Act
            var result = await _unitaDiMisuraRepository.GetAllAsync();

            // Assert
            Assert.Empty(result);
        }
    }
}