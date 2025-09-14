using Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Repository;

namespace RepositoryTest
{
    public class BaseTest
    {
        protected readonly IServiceProvider _serviceProvider;
        protected readonly Mock<BubbleTeaContext> _mockContext;

        public BaseTest()
        {
            var services = new ServiceCollection();

            // Crea un mock del DbContext
            _mockContext = new Mock<BubbleTeaContext>();

            // Registra il mock nel container DI
            services.AddScoped<BubbleTeaContext>(_ => _mockContext.Object);

            // Registra TUTTI i repository
            services.AddServiceDb();

            _serviceProvider = services.BuildServiceProvider();
        }

        protected T GetService<T>() => _serviceProvider.GetRequiredService<T>();
    }
}