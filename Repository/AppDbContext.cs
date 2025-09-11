using DTO;
using Microsoft.EntityFrameworkCore;

namespace Database
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options) { }

        // Tabelle
        public DbSet<OrdineDTO> Ordini { get; set; }
        public DbSet<OrderItemDTO> OrderItems { get; set; }
        public DbSet<DolceDTO> Dolci { get; set; }
        public DbSet<IngredienteDTO> Ingredienti { get; set; }
        public DbSet<UtentiDTO> Utenti { get; set; }
        public DbSet<TavoloDTO> Tavoli { get; set; }
        public DbSet<SessioniQrDTO> SessioniQr { get; set; }
        public DbSet<LogAccessiDTO> LogAccessi { get; set; }
        public DbSet<LogAttivitaDTO> LogAttivita { get; set; }
        public DbSet<NotificheOperativeDTO> NotificheOperative { get; set; }

        // Views
        public DbSet<VwStatisticheOrdiniAvanzateDTO> VwStatisticheOrdiniAvanzate { get; set; }
        public DbSet<VwDashboardAmministrativaDTO> VwDashboardAmministrativa { get; set; }
        public DbSet<VwDashboardSintesiDTO> VwDashboardSintesi { get; set; }
        public DbSet<VwDashboardStatisticheDTO> VwDashboardStatistiche { get; set; }
        public DbSet<VwStatisticheRecentiDTO> VwStatisticheRecenti { get; set; }
        public DbSet<VwStatisticheMensiliDTO> VwStatisticheMensili { get; set; }
        public DbSet<VwStatisticheGiornaliereDTO> VwStatisticheGiornaliere { get; set; }
    }
}
