using DTO;
using Microsoft.EntityFrameworkCore;
using Moq;
using Repository.Interface;
using Repository.Service;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace RepositoryTest
{
    public class DimensioneBicchiereRepositoryTest : BaseTest
    {
        private readonly IDimensioneBicchiereRepository _repository;
        private readonly List<Database.DimensioneBicchiere> _dimensioniData;
        private readonly Mock<DbSet<Database.DimensioneBicchiere>> _mockSet;

        public DimensioneBicchiereRepositoryTest()
        {
            _repository = GetService<IDimensioneBicchiereRepository>();

            // Dati di test
            _dimensioniData = new List<Database.DimensioneBicchiere>
            {
                new Database.DimensioneBicchiere
                {
                    DimensioneBicchiereId = 1,
                    Sigla = "S",
                    Descrizione = "Small",
                    Capienza = 0.3m,
                    UnitaMisuraId = 1,
                    PrezzoBase = 2.50m,
                    Moltiplicatore = 0.8m
                },
                new Database.DimensioneBicchiere
                {
                    DimensioneBicchiereId = 2,
                    Sigla = "M",
                    Descrizione = "Medium",
                    Capienza = 0.5m,
                    UnitaMisuraId = 1,
                    PrezzoBase = 3.50m,
                    Moltiplicatore = 1.0m
                }
            };

            // Configura il mock del DbSet con supporto async
            _mockSet = new Mock<DbSet<Database.DimensioneBicchiere>>();

            // Configura IQueryable
            var queryable = _dimensioniData.AsQueryable();
            _mockSet.As<IQueryable<Database.DimensioneBicchiere>>()
                .Setup(m => m.Provider)
                .Returns(new TestAsyncQueryProvider<Database.DimensioneBicchiere>(queryable.Provider));

            _mockSet.As<IQueryable<Database.DimensioneBicchiere>>()
                .Setup(m => m.Expression)
                .Returns(queryable.Expression);

            _mockSet.As<IQueryable<Database.DimensioneBicchiere>>()
                .Setup(m => m.ElementType)
                .Returns(queryable.ElementType);

            _mockSet.As<IQueryable<Database.DimensioneBicchiere>>()
                .Setup(m => m.GetEnumerator())
                .Returns(queryable.GetEnumerator());

            // Configura IAsyncEnumerable
            _mockSet.As<IAsyncEnumerable<Database.DimensioneBicchiere>>()
                .Setup(m => m.GetAsyncEnumerator(It.IsAny<CancellationToken>()))
                .Returns(new TestAsyncEnumerator<Database.DimensioneBicchiere>(queryable.GetEnumerator()));

            // Configura il mock del Context
            _mockContext.Setup(c => c.DimensioneBicchiere).Returns(_mockSet.Object);
            _mockContext.Setup(c => c.Set<Database.DimensioneBicchiere>()).Returns(_mockSet.Object);

            // Configura FindAsync per supportare le operazioni di ricerca
            _mockSet.Setup(m => m.FindAsync(It.IsAny<object[]>()))
                .ReturnsAsync((object[] keyValues) =>
                {
                    var id = (int)keyValues[0];
                    return _dimensioniData.FirstOrDefault(d => d.DimensioneBicchiereId == id);
                });
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnDimensione()
        {
            // Act
            var result = await _repository.GetByIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("S", result.Sigla);
            Assert.Equal("Small", result.Descrizione);
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnAllDimensioni()
        {
            // Act
            var result = await _repository.GetAllAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Contains(result, d => d.Sigla == "S");
            Assert.Contains(result, d => d.Sigla == "M");
        }

        [Fact]
        public async Task GetByIdAsync_WithInvalidId_ShouldReturnNull()
        {
            // Act
            var result = await _repository.GetByIdAsync(999);

            // Assert
            Assert.Null(result);
        }
    }
}