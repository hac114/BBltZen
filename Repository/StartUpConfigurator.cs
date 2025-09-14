using Microsoft.Extensions.DependencyInjection;
using Repository.Interface;
using Repository.Service;

namespace Repository
{
    public static class StartUpConfigurator
    {
        public static void AddServiceDb(this IServiceCollection services)
        {
            services.AddScoped<IOrdineRepository, OrdineRepository>();
            services.AddScoped<IDolceRepository, DolceRepository>();
            services.AddScoped<IIngredienteRepository, IngredienteRepository>();
            services.AddScoped<INotificheOperativeRepository, NotificheOperativeRepository>();
            services.AddScoped<IOrderItemRepository, OrderItemRepository>();
            services.AddScoped<IStatoOrdineRepository, StatoOrdineRepository>();
            services.AddScoped<IStatoPagamentoRepository, StatoPagamentoRepository>();
            services.AddScoped<ITavoloRepository, TavoloRepository>();
            services.AddScoped<ITaxRatesRepository, TaxRatesRepository>();
            services.AddScoped<IUtentiRepository, UtentiRepository>();
            services.AddScoped<IVwStatisticheOrdiniAvanzateRepository, VwStatisticheOrdiniAvanzateRepository>();

            // Aggiungi anche questi quando saranno implementati:
            // services.AddScoped<IArticoloRepository, ArticoloRepository>();
            // services.AddScoped<IVwIngredientiPopolariRepository, VwIngredientiPopolariRepository>();
            // services.AddScoped<ISessioniQrRepository, SessioniQrRepository>();
        }
    }
}
