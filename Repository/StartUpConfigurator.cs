using Microsoft.Extensions.DependencyInjection;
using Repository.Interface;
using Repository.Service;
using System.Net.Security;

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
            services.AddScoped<IDimensioneBicchiereRepository, DimensioneBicchiereRepository>();
            services.AddScoped<IUnitaDiMisuraRepository, UnitaDiMisuraRepository>();
            services.AddScoped<ICategoriaIngredienteRepository, CategoriaIngredienteRepository>();
            services.AddScoped<IIngredienteRepository, IngredienteRepository>();
            services.AddScoped<ISessioniQrRepository, SessioniQrRepository>();
            services.AddScoped<IClienteRepository, ClienteRepository>();
            services.AddScoped<IPreferitiClienteRepository, PreferitiClienteRepository>();
            services.AddScoped<IPersonalizzazioneRepository, PersonalizzazioneRepository>();
            services.AddScoped<IPersonalizzazioneIngredienteRepository, PersonalizzazioneIngredienteRepository>();
            services.AddScoped<IBevandaStandardRepository, BevandaStandardRepository>();
            services.AddScoped<IDimensioneQuantitaIngredientiRepository, DimensioneQuantitaIngredientiRepository>();
            services.AddScoped<IPersonalizzazioneCustomRepository, PersonalizzazioneCustomRepository>();
            services.AddScoped<IIngredientiPersonalizzazioneRepository, IngredientiPersonalizzazioneRepository>();
            services.AddScoped<IBevandaCustomRepository, BevandaCustomRepository>();
            services.AddScoped<IArticoloRepository, ArticoloRepository>();
            services.AddScoped<IStatoStoricoOrdineRepository, StatoStoricoOrdineRepository>();
            services.AddScoped<IStatisticheCacheRepository, StatisticheCacheRepository>();
            services.AddScoped<IConfigSoglieTempiRepository, ConfigSoglieTempiRepository>();
            services.AddScoped<ILogAccessiRepository, LogAccessiRepository>();
            services.AddScoped<ILogAttivitaRepository, LogAttivitaRepository>();

            services.AddMemoryCache(); // Per la cache in memoria
            services.AddScoped<IPriceCalculationServiceRepository, PriceCalculationServiceRepository>();
            services.AddScoped<IAdvancedPriceCalculationServiceRepository, AdvancedPriceCalculationServiceRepository>();
            services.AddScoped<ISistemaCacheRepository, SistemaCacheRepository>();

            services.AddScoped<IVwArticoliCompletiRepository, VwArticoliCompletiRepository>();
            services.AddScoped<IVwMenuDinamicoRepository, VwMenuDinamicoRepository>();

            services.AddScoped<IBeverageAvailabilityServiceRepository, BeverageAvailabilityServiceRepository>();
            services.AddScoped<IOrderTotalServiceRepository, OrderTotalServiceRepository>();
            services.AddScoped<IOperationalNotificationServiceRepository, OperationalNotificationServiceRepository>();

            services.AddScoped<IStripeServiceRepository, StripeServiceRepository>();

        }
    }
}
