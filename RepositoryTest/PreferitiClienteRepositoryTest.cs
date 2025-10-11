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
    public class PreferitiClienteRepositoryTest : BaseTest
    {
        private readonly IPreferitiClienteRepository _preferitiRepository;

        public PreferitiClienteRepositoryTest()
        {
            _preferitiRepository = new PreferitiClienteRepository(_context);
        }

        [Fact]
        public async Task AddAsync_Should_Add_Preferito()
        {
            // Arrange
            await CleanTablesForPreferitiTest();

            var preferitoDto = new PreferitiClienteDTO
            {
                ClienteId = 1,
                BevandaId = 1
            };

            // Act
            await _preferitiRepository.AddAsync(preferitoDto);

            // Assert
            var result = await _preferitiRepository.GetByIdAsync(preferitoDto.PreferitoId);
            Assert.NotNull(result);
            Assert.Equal(1, result.ClienteId);
            Assert.Equal(1, result.BevandaId);
            Assert.NotNull(result.DataAggiunta);
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Correct_Preferito()
        {
            // Arrange
            await CleanTablesForPreferitiTest();

            var preferitoDto = new PreferitiClienteDTO
            {
                ClienteId = 1,
                BevandaId = 1
            };
            await _preferitiRepository.AddAsync(preferitoDto);

            // Act
            var result = await _preferitiRepository.GetByIdAsync(preferitoDto.PreferitoId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(preferitoDto.PreferitoId, result.PreferitoId);
            Assert.Equal(1, result.ClienteId);
            Assert.Equal(1, result.BevandaId);
        }

        [Fact]
        public async Task GetByClienteIdAsync_Should_Return_All_Preferiti_For_Cliente()
        {
            // Arrange
            await CleanTablesForPreferitiTest();

            var preferiti = new List<PreferitiClienteDTO>
            {
                new PreferitiClienteDTO { ClienteId = 1, BevandaId = 1 },
                new PreferitiClienteDTO { ClienteId = 1, BevandaId = 2 },
                new PreferitiClienteDTO { ClienteId = 2, BevandaId = 1 }
            };

            foreach (var preferito in preferiti)
            {
                await _preferitiRepository.AddAsync(preferito);
            }

            // Act
            var result = await _preferitiRepository.GetByClienteIdAsync(1);

            // Assert
            Assert.Equal(2, result.Count());
            Assert.All(result, p => Assert.Equal(1, p.ClienteId));
        }

        [Fact]
        public async Task GetByBevandaIdAsync_Should_Return_All_Preferiti_For_Bevanda()
        {
            // Arrange
            await CleanTablesForPreferitiTest();

            var preferiti = new List<PreferitiClienteDTO>
            {
                new PreferitiClienteDTO { ClienteId = 1, BevandaId = 1 },
                new PreferitiClienteDTO { ClienteId = 2, BevandaId = 1 },
                new PreferitiClienteDTO { ClienteId = 1, BevandaId = 2 }
            };

            foreach (var preferito in preferiti)
            {
                await _preferitiRepository.AddAsync(preferito);
            }

            // Act
            var result = await _preferitiRepository.GetByBevandaIdAsync(1);

            // Assert
            Assert.Equal(2, result.Count());
            Assert.All(result, p => Assert.Equal(1, p.BevandaId));
        }

        [Fact]
        public async Task GetByClienteAndBevandaAsync_Should_Return_Correct_Preferito()
        {
            // Arrange
            await CleanTablesForPreferitiTest();

            var preferitoDto = new PreferitiClienteDTO
            {
                ClienteId = 1,
                BevandaId = 1
            };
            await _preferitiRepository.AddAsync(preferitoDto);

            // Act
            var result = await _preferitiRepository.GetByClienteAndBevandaAsync(1, 1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.ClienteId);
            Assert.Equal(1, result.BevandaId);
        }

        [Fact]
        public async Task DeleteByClienteAndBevandaAsync_Should_Remove_Preferito()
        {
            // Arrange
            await CleanTablesForPreferitiTest();

            var preferitoDto = new PreferitiClienteDTO
            {
                ClienteId = 1,
                BevandaId = 1
            };
            await _preferitiRepository.AddAsync(preferitoDto);

            // Act
            await _preferitiRepository.DeleteByClienteAndBevandaAsync(1, 1);

            // Assert
            var deleted = await _preferitiRepository.GetByClienteAndBevandaAsync(1, 1);
            Assert.Null(deleted);
        }

        [Fact]
        public async Task ExistsByClienteAndBevandaAsync_Should_Return_True_For_Existing_Preferito()
        {
            // Arrange
            await CleanTablesForPreferitiTest();

            var preferitoDto = new PreferitiClienteDTO
            {
                ClienteId = 1,
                BevandaId = 1
            };
            await _preferitiRepository.AddAsync(preferitoDto);

            // Act
            var exists = await _preferitiRepository.ExistsByClienteAndBevandaAsync(1, 1);

            // Assert
            Assert.True(exists);
        }

        [Fact]
        public async Task GetCountByClienteAsync_Should_Return_Correct_Count()
        {
            // Arrange
            await CleanTablesForPreferitiTest();

            var preferiti = new List<PreferitiClienteDTO>
            {
                new PreferitiClienteDTO { ClienteId = 1, BevandaId = 1 },
                new PreferitiClienteDTO { ClienteId = 1, BevandaId = 2 },
                new PreferitiClienteDTO { ClienteId = 2, BevandaId = 1 }
            };

            foreach (var preferito in preferiti)
            {
                await _preferitiRepository.AddAsync(preferito);
            }

            // Act
            var count = await _preferitiRepository.GetCountByClienteAsync(1);

            // Assert
            Assert.Equal(2, count);
        }

        private async Task CleanTablesForPreferitiTest()
        {
            await CleanTableAsync<Database.PreferitiCliente>();
            await CleanTableAsync<Database.Cliente>();
            await CleanTableAsync<Database.BevandaStandard>();
            await CleanTableAsync<Database.Tavolo>();
            await CleanTableAsync<Database.Articolo>();
            await CleanTableAsync<Database.Personalizzazione>();
            await CleanTableAsync<Database.DimensioneBicchiere>();
            await CleanTableAsync<Database.UnitaDiMisura>();

            // Setup UnitaDiMisura per DimensioneBicchiere
            var unitaMisura = new Database.UnitaDiMisura
            {
                Sigla = "ml",
                Descrizione = "Millilitri"
            };
            _context.UnitaDiMisura.Add(unitaMisura);
            await _context.SaveChangesAsync();

            // Setup DimensioneBicchiere
            var dimensioneBicchiere = new Database.DimensioneBicchiere
            {
                Sigla = "MED",
                Descrizione = "Bicchiere Medio",
                Capienza = 500.0m,
                UnitaMisuraId = unitaMisura.UnitaMisuraId,
                PrezzoBase = 2.0m,
                Moltiplicatore = 1.0m
            };
            _context.DimensioneBicchiere.Add(dimensioneBicchiere);

            // Setup Articolo
            var articolo = new Database.Articolo
            {
                Tipo = "BS", // BEVANDA_STANDARD
                DataCreazione = DateTime.Now,
                DataAggiornamento = DateTime.Now
            };
            _context.Articolo.Add(articolo);

            // Setup Personalizzazione CON TUTTE LE PROPRIETÀ RICHIESTE
            var personalizzazione = new Database.Personalizzazione
            {
                Nome = "Personalizzazione Test",
                Descrizione = "Descrizione test per personalizzazione"
                // Aggiungi altre proprietà obbligatorie se necessario
            };
            _context.Personalizzazione.Add(personalizzazione);

            await _context.SaveChangesAsync();

            // Setup Tavolo
            var tavolo = new Database.Tavolo
            {
                Numero = 1,
                Zona = "Test",
                QrCode = "QR001",
                Disponibile = true
            };
            _context.Tavolo.Add(tavolo);
            await _context.SaveChangesAsync();

            // Setup Cliente
            var cliente = new Database.Cliente
            {
                TavoloId = tavolo.TavoloId
            };
            _context.Cliente.Add(cliente);

            // Setup BevandaStandard
            var bevanda = new Database.BevandaStandard
            {
                ArticoloId = articolo.ArticoloId,
                PersonalizzazioneId = personalizzazione.PersonalizzazioneId,
                DimensioneBicchiereId = dimensioneBicchiere.DimensioneBicchiereId,
                Prezzo = 5.0m,
                Disponibile = true,
                SempreDisponibile = false,
                Priorita = 1,
                DataCreazione = DateTime.Now,
                DataAggiornamento = DateTime.Now
            };
            _context.BevandaStandard.Add(bevanda);

            await _context.SaveChangesAsync();
        }
    }
}