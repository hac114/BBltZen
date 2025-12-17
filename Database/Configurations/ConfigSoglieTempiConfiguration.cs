using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BBltZen;

namespace Database.Configurations
{
    public class ConfigSoglieTempiConfiguration : IEntityTypeConfiguration<ConfigSoglieTempi>
    {
        public void Configure(EntityTypeBuilder<ConfigSoglieTempi> builder)
        {
            // ✅ CHIAVE PRIMARIA
            builder.HasKey(c => c.SogliaId);

            // ✅ PROPRIETÀ OBBLIGATORIE
            builder.Property(c => c.StatoOrdineId)
                .IsRequired();

            builder.Property(c => c.SogliaAttenzione)
                .IsRequired();

            builder.Property(c => c.SogliaCritico)
                .IsRequired();

            // ✅ PROPRIETÀ OPZIONALI CON LUNGHEZZA
            builder.Property(c => c.UtenteAggiornamento)
                .HasMaxLength(100);

            // ✅ VALORI DEFAULT
            builder.Property(c => c.DataAggiornamento)
                .HasDefaultValueSql("GETDATE()");

            // ✅ CHECK CONSTRAINTS - CORRISPONDONO AL DATABASE
            builder.ToTable(tb =>
            {
                tb.HasCheckConstraint(
                    "CK_CONFIG_SOGLIE_TEMPI",
                    "[soglia_attenzione] >= 0 AND [soglia_attenzione] <= 1000");

                tb.HasCheckConstraint(
                    "CK_CONFIG_SOGLIE_TEMPI_1",
                    "[soglia_critico] >= 0 AND [soglia_critico] <= 1000");

                tb.HasCheckConstraint(
                    "CK_CONFIG_SOGLIE_TEMPI_critico_gt_attenzione",
                    "[soglia_critico] > [soglia_attenzione]");

                // ✅ NUOVO VINCOLO AGGIUNTO: Impedisce configurazioni per stati terminali
                // Stati terminali: 4 = consegnato, 6 = annullato
                // ✅ CORREZIONE: Usa il nome della colonna nel database (stato_ordine_id)
                tb.HasCheckConstraint(
                    "CK_CONFIG_SOGLIE_TEMPI_no_stati_terminali",
                    "[stato_ordine_id] NOT IN (4, 6)");
            });

            // ✅ UNIQUE CONSTRAINT (NEL DB: UQ_CONFIG_SOGLIE_TEMPI_stato_ordine_id)
            builder.HasIndex(c => c.StatoOrdineId)
                .IsUnique()
                .HasDatabaseName("UQ_CONFIG_SOGLIE_TEMPI_stato_ordine_id");

            // ✅ INDICE SU DATA_AGGIORNAMENTO (NEL DB: IX_CONFIG_SOGLIE_TEMPI_data_aggiornamento)
            builder.HasIndex(c => c.DataAggiornamento)
                .HasDatabaseName("IX_CONFIG_SOGLIE_TEMPI_data_aggiornamento");

            // ✅ RELAZIONE CON STATO ORDINE
            builder.HasOne(c => c.StatoOrdine)
                .WithMany()
                .HasForeignKey(c => c.StatoOrdineId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}