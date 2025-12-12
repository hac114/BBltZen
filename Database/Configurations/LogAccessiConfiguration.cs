using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Database.Models;

namespace Database.Configurations
{
    public class LogAccessiConfiguration : IEntityTypeConfiguration<LogAccessi>
    {
        public void Configure(EntityTypeBuilder<LogAccessi> builder)
        {
            // ✅ CHIAVE PRIMARIA
            builder.HasKey(l => l.LogId);

            // ✅ PROPRIETÀ OBBLIGATORIE
            builder.Property(l => l.TipoAccesso)
                .IsRequired()
                .HasMaxLength(20);

            builder.Property(l => l.Esito)
                .IsRequired()
                .HasMaxLength(20);

            // ✅ PROPRIETÀ OPZIONALI CON LUNGHEZZA
            builder.Property(l => l.IpAddress)
                .HasMaxLength(45);

            builder.Property(l => l.UserAgent)
                .HasMaxLength(500);

            builder.Property(l => l.Dettagli)
                .HasMaxLength(1000);

            // ✅ VALORI DEFAULT
            builder.Property(l => l.DataCreazione)
                .HasDefaultValueSql("GETDATE()");

            // ✅ INDICI PER PERFORMANCE
            builder.HasIndex(l => l.DataCreazione);
            builder.HasIndex(l => l.UtenteId);
            builder.HasIndex(l => l.ClienteId);
            builder.HasIndex(l => l.TipoAccesso);
            builder.HasIndex(l => l.Esito);

            // ✅ RELAZIONI
            builder.HasOne(l => l.Utente)
                .WithMany()
                .HasForeignKey(l => l.UtenteId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasOne(l => l.Cliente)
                .WithMany()
                .HasForeignKey(l => l.ClienteId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}