// DependencyInjection.cs
using Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Repository.Interface;
using Repository.Service;

namespace BBltZen
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services, string connectionString)
        {
            // Database Context
            services.AddDbContext<BubbleTeaContext>(options =>
                options.UseSqlServer(connectionString));

            // Repository
            services.AddScoped<ITavoloRepository, TavoloRepository>();

            // Altri repository li aggiungeremo man mano...
            // services.AddScoped<IProductRepository, ProductRepository>();
            // services.AddScoped<IOrderRepository, OrderRepository>();

            return services;
        }
    }
}