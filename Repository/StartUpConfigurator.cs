using Microsoft.Extensions.DependencyInjection;
using Repository.Interface;
using Repository.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository
{
    public static class StartUpConfigurator
    {
        public static void AddServiceDb(this IServiceCollection services)
        {
            services.AddTransient <IOrdineRepository, OrdineRepository>();
            services.AddTransient <IDolceRepository, DolceRepository>();
            //services.AddTransient <IVwIngredientiPopolariRepository, VwIngredientiPopolariRepository>();
            services.AddTransient <IIngredienteRepository, IngredienteRepository>();
            services.AddTransient <INotificheOperativeRepository, NotificheOperativeRepository>();
            services.AddTransient <IOrderItemRepository, OrderItemRepository>();
            services.AddTransient <IOrdineRepository, OrdineRepository>();
            //services.AddTransient <ISessioniQrRepository, SessioniQrRepository>;
            services.AddTransient <IStatoOrdineRepository, StatoOrdineRepository>();
            services.AddTransient <IStatoPagamentoRepository, StatoPagamentoRepository>();
            services.AddTransient <ITavoloRepository, TavoloRepository>();
            services.AddTransient <ITaxRatesRepository, TaxRatesRepository>();
            services.AddTransient <IUtentiRepository, UtentiRepository>();
            services.AddTransient <IVwStatisticheOrdiniAvanzateRepository, VwStatisticheOrdiniAvanzateRepository>();
        }
    }
}
