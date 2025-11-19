using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Database;

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

            // ✅ CHECK CONSTRAINTS - SINTASSI CORRETTA (NON OBSOLETA)
            builder.ToTable(tb => tb.HasCheckConstraint(
                "CK_ConfigSoglieTempi_Soglie",
                "[SogliaAttenzione] >= 0 AND [SogliaCritico] >= 0 AND [SogliaCritico] > [SogliaAttenzione]"));

            // ✅ INDICI PER PERFORMANCE
            builder.HasIndex(c => c.StatoOrdineId)
                .IsUnique(); // ✅ UNIQUE: Una configurazione per stato ordine

            builder.HasIndex(c => c.DataAggiornamento);

            // ✅ RELAZIONE CON STATO ORDINE
            builder.HasOne(c => c.StatoOrdine)
                .WithMany()
                .HasForeignKey(c => c.StatoOrdineId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}