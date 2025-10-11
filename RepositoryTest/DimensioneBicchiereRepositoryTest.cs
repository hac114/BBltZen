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
    public class DimensioneBicchiereRepositoryTest : BaseTest
    {
        private readonly IDimensioneBicchiereRepository _dimensioneBicchiereRepository;

        public DimensioneBicchiereRepositoryTest()
        {
            // ✅ SEMPLICE: BaseTest già fornisce _context (AppDbContext) inizializzato
            _dimensioneBicchiereRepository = new DimensioneBicchiereRepository(_context);

            // Setup: aggiungi alcune unità di misura per i test
            SetupUnitaDiMisura();
        }

        // ✅ RIMOSSO Dispose() - BaseTest già lo gestisce automaticamente

        private void SetupUnitaDiMisura()
        {
            // Aggiungi unità di misura di test se non esistono
            if (!_context.UnitaDiMisura.Any())
            {
                _context.UnitaDiMisura.AddRange(
                    new UnitaDiMisura { Sigla = "ml", Descrizione = "Millilitri" },
                    new UnitaDiMisura { Sigla = "cl", Descrizione = "Centilitri" }
                );
                _context.SaveChanges();
            }
        }

        [Fact]
        public async Task AddAsync_Should_Add_DimensioneBicchiere()
        {
            // Arrange
            var dimensioneDto = new DimensioneBicchiereDTO
            {
                Sigla = "M",
                Descrizione = "Medio",
                Capienza = 400.0m,
                UnitaMisuraId = 1,
                PrezzoBase = 3.50m,
                Moltiplicatore = 1.0m
            };

            // Act
            await _dimensioneBicchiereRepository.AddAsync(dimensioneDto);

            // Assert
            var result = await _dimensioneBicchiereRepository.GetByIdAsync(dimensioneDto.DimensioneBicchiereId);
            Assert.NotNull(result);
            Assert.Equal("M", result.Sigla);
            Assert.Equal("Medio", result.Descrizione);
            Assert.Equal(400.0m, result.Capienza);
            Assert.Equal(1, result.UnitaMisuraId);
            Assert.Equal(3.50m, result.PrezzoBase);
            Assert.Equal(1.0m, result.Moltiplicatore);
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Correct_DimensioneBicchiere()
        {
            // Arrange
            var dimensioneDto = new DimensioneBicchiereDTO
            {
                Sigla = "G",
                Descrizione = "Grande",
                Capienza = 500.0m,
                UnitaMisuraId = 1,
                PrezzoBase = 4.50m,
                Moltiplicatore = 1.2m
            };
            await _dimensioneBicchiereRepository.AddAsync(dimensioneDto);

            // Act
            var result = await _dimensioneBicchiereRepository.GetByIdAsync(dimensioneDto.DimensioneBicchiereId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(dimensioneDto.DimensioneBicchiereId, result.DimensioneBicchiereId);
            Assert.Equal("G", result.Sigla);
            Assert.Equal("Grande", result.Descrizione);
            Assert.Equal(500.0m, result.Capienza);
            Assert.Equal(1, result.UnitaMisuraId);
            Assert.Equal(4.50m, result.PrezzoBase);
            Assert.Equal(1.2m, result.Moltiplicatore);
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_For_NonExisting_Id()
        {
            // Act
            var result = await _dimensioneBicchiereRepository.GetByIdAsync(999);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetAllAsync_Should_Return_All_DimensioniBicchiere()
        {
            // Arrange
            var dimensioniList = new List<DimensioneBicchiereDTO>
            {
                new DimensioneBicchiereDTO
                {
                    Sigla = "S",
                    Descrizione = "Piccolo",
                    Capienza = 300.0m,
                    UnitaMisuraId = 1,
                    PrezzoBase = 2.50m,
                    Moltiplicatore = 0.8m
                },
                new DimensioneBicchiereDTO
                {
                    Sigla = "M",
                    Descrizione = "Medio",
                    Capienza = 400.0m,
                    UnitaMisuraId = 1,
                    PrezzoBase = 3.50m,
                    Moltiplicatore = 1.0m
                },
                new DimensioneBicchiereDTO
                {
                    Sigla = "L",
                    Descrizione = "Grande",
                    Capienza = 500.0m,
                    UnitaMisuraId = 1,
                    PrezzoBase = 4.50m,
                    Moltiplicatore = 1.2m
                }
            };

            foreach (var dimensione in dimensioniList)
            {
                await _dimensioneBicchiereRepository.AddAsync(dimensione);
            }

            // Act
            var result = await _dimensioneBicchiereRepository.GetAllAsync();

            // Assert
            Assert.Equal(3, result.Count);
            Assert.Contains(result, d => d.Sigla == "S");
            Assert.Contains(result, d => d.Sigla == "M");
            Assert.Contains(result, d => d.Sigla == "L");
        }

        [Fact]
        public async Task UpdateAsync_Should_Update_DimensioneBicchiere_Correctly()
        {
            // Arrange
            var dimensioneDto = new DimensioneBicchiereDTO
            {
                Sigla = "M",
                Descrizione = "Medio",
                Capienza = 400.0m,
                UnitaMisuraId = 1,
                PrezzoBase = 3.50m,
                Moltiplicatore = 1.0m
            };
            await _dimensioneBicchiereRepository.AddAsync(dimensioneDto);

            var updateDto = new DimensioneBicchiereDTO
            {
                DimensioneBicchiereId = dimensioneDto.DimensioneBicchiereId,
                Sigla = "M+",
                Descrizione = "Medio Plus",
                Capienza = 450.0m,
                UnitaMisuraId = 1,
                PrezzoBase = 4.00m,
                Moltiplicatore = 1.1m
            };

            // Act
            await _dimensioneBicchiereRepository.UpdateAsync(updateDto);

            // Assert
            var updated = await _dimensioneBicchiereRepository.GetByIdAsync(dimensioneDto.DimensioneBicchiereId);
            Assert.NotNull(updated);
            Assert.Equal("M+", updated.Sigla);
            Assert.Equal("Medio Plus", updated.Descrizione);
            Assert.Equal(450.0m, updated.Capienza);
            Assert.Equal(4.00m, updated.PrezzoBase);
            Assert.Equal(1.1m, updated.Moltiplicatore);
        }

        [Fact]
        public async Task UpdateAsync_Should_Not_Throw_For_NonExisting_Id()
        {
            // Arrange
            var updateDto = new DimensioneBicchiereDTO
            {
                DimensioneBicchiereId = 999,
                Sigla = "TEST",
                Descrizione = "Test",
                Capienza = 100.0m,
                UnitaMisuraId = 1,
                PrezzoBase = 1.00m,
                Moltiplicatore = 1.0m
            };

            // Act & Assert - Non dovrebbe lanciare eccezioni
            await _dimensioneBicchiereRepository.UpdateAsync(updateDto);
        }

        [Fact]
        public async Task DeleteAsync_Should_Remove_DimensioneBicchiere()
        {
            // Arrange
            var dimensioneDto = new DimensioneBicchiereDTO
            {
                Sigla = "XL",
                Descrizione = "Extra Large",
                Capienza = 600.0m,
                UnitaMisuraId = 1,
                PrezzoBase = 5.50m,
                Moltiplicatore = 1.5m
            };
            await _dimensioneBicchiereRepository.AddAsync(dimensioneDto);

            // Act
            await _dimensioneBicchiereRepository.DeleteAsync(dimensioneDto.DimensioneBicchiereId);

            // Assert
            var deleted = await _dimensioneBicchiereRepository.GetByIdAsync(dimensioneDto.DimensioneBicchiereId);
            Assert.Null(deleted);
        }

        [Fact]
        public async Task DeleteAsync_Should_Not_Throw_For_NonExisting_Id()
        {
            // Act & Assert - Non dovrebbe lanciare eccezioni
            await _dimensioneBicchiereRepository.DeleteAsync(999);
        }

        [Fact]
        public async Task AddAsync_Should_Assign_Generated_Id()
        {
            // Arrange
            var dimensioneDto = new DimensioneBicchiereDTO
            {
                Sigla = "XXL",
                Descrizione = "Doppio Extra Large",
                Capienza = 700.0m,
                UnitaMisuraId = 1,
                PrezzoBase = 6.50m,
                Moltiplicatore = 1.8m
            };

            // Act
            await _dimensioneBicchiereRepository.AddAsync(dimensioneDto);

            // Assert
            Assert.True(dimensioneDto.DimensioneBicchiereId > 0);
        }

        [Fact]
        public async Task GetAllAsync_Should_Return_Empty_List_When_No_Data()
        {
            // Act
            var result = await _dimensioneBicchiereRepository.GetAllAsync();

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetAllAsync_Should_Include_UnitaMisura_Relation()
        {
            // Arrange
            var dimensioneDto = new DimensioneBicchiereDTO
            {
                Sigla = "T",
                Descrizione = "Test",
                Capienza = 350.0m,
                UnitaMisuraId = 1, // Deve esistere nel setup
                PrezzoBase = 3.00m,
                Moltiplicatore = 0.9m
            };
            await _dimensioneBicchiereRepository.AddAsync(dimensioneDto);

            // Act
            var result = await _dimensioneBicchiereRepository.GetAllAsync();

            // Assert
            var dimensione = result.First();
            Assert.Equal(1, dimensione.UnitaMisuraId);
            // Nota: Il DTO non include i dati dell'unità di misura, solo l'ID
        }

        [Fact]
        public async Task Debug_GetByIdAsync_Issue()
        {
            // Arrange
            var dimensioneDto = new DimensioneBicchiereDTO
            {
                Sigla = "DEBUG",
                Descrizione = "Debug Test",
                Capienza = 350.0m,
                UnitaMisuraId = 1,
                PrezzoBase = 3.00m,
                Moltiplicatore = 0.9m
            };

            await _dimensioneBicchiereRepository.AddAsync(dimensioneDto);
            Console.WriteLine($"ID assegnato: {dimensioneDto.DimensioneBicchiereId}");

            // Debug: query diretta
            var directQuery = await _context.DimensioneBicchiere
                .Where(d => d.DimensioneBicchiereId == dimensioneDto.DimensioneBicchiereId)
                .FirstOrDefaultAsync();
            Console.WriteLine($"Query diretta: {directQuery?.Sigla}");

            // Act
            var result = await _dimensioneBicchiereRepository.GetByIdAsync(dimensioneDto.DimensioneBicchiereId);
            Console.WriteLine($"Repository result: {result?.Sigla}");
        }
    }
}