using Microsoft.Extensions.DependencyInjection;
using Repository.Interface;
using Repository.Service;

namespace Repository
{
    public static class StartUpConfigurator
    {
        public static void AddServiceDb(this IServiceCollection services)
        {
            services.AddTransient<IOrdineRepository, OrdineRepository>();
            services.AddTransient<IDolceRepository, DolceRepository>();
            services.AddTransient<IIngredienteRepository, IngredienteRepository>();
            services.AddTransient<INotificheOperativeRepository, NotificheOperativeRepository>();
            services.AddTransient<IOrderItemRepository, OrderItemRepository>();
            services.AddTransient<IStatoOrdineRepository, StatoOrdineRepository>();
            services.AddTransient<IStatoPagamentoRepository, StatoPagamentoRepository>();
            services.AddTransient<ITavoloRepository, TavoloRepository>();
            services.AddTransient<ITaxRatesRepository, TaxRatesRepository>();
            services.AddTransient<IUtentiRepository, UtentiRepository>();
            services.AddTransient<IVwStatisticheOrdiniAvanzateRepository, VwStatisticheOrdiniAvanzateRepository>();

            // COMMENTATI (ma tenuti per riferimento):
            // services.AddTransient<IVwIngredientiPopolariRepository, VwIngredientiPopolariRepository>();
            // services.AddTransient<ISessioniQrRepository, SessioniQrRepository>();
        }
    }
}
