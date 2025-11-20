using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Database;

namespace Database.Configurations
{
    public class StatisticheCacheConfiguration : IEntityTypeConfiguration<StatisticheCache>
    {
        public void Configure(EntityTypeBuilder<StatisticheCache> builder)
        {
            // ✅ CHIAVE PRIMARIA
            builder.HasKey(s => s.Id);

            // ✅ PROPRIETÀ OBBLIGATORIE
            builder.Property(s => s.TipoStatistica)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(s => s.Periodo)
                .IsRequired()
                .HasMaxLength(20);

            builder.Property(s => s.Metriche)
                .IsRequired()
                .HasMaxLength(4000);

            builder.Property(s => s.DataAggiornamento)
                .IsRequired();

            // ✅ INDICE UNIVOCO PER EVITARE DUPLICATI
            builder.HasIndex(s => new { s.TipoStatistica, s.Periodo })
                .IsUnique();

            // ✅ INDICI PER PERFORMANCE
            builder.HasIndex(s => s.TipoStatistica);
            builder.HasIndex(s => s.Periodo);
            builder.HasIndex(s => s.DataAggiornamento);

            // ✅ CONFIGURAZIONE IDENTITY
            builder.Property(s => s.Id)
                .ValueGeneratedOnAdd();

            // ✅ CONFIGURAZIONE NOME TABELLA
            builder.ToTable("StatisticheCache");
        }
    }
}