using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BBltZen;

namespace Database.Configurations
{
    public class LogAttivitaConfiguration : IEntityTypeConfiguration<LogAttivita>
    {
        public void Configure(EntityTypeBuilder<LogAttivita> builder)
        {
            // ✅ CHIAVE PRIMARIA
            builder.HasKey(l => l.LogId);

            // ✅ PROPRIETÀ OBBLIGATORIE
            builder.Property(l => l.TipoAttivita)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(l => l.Descrizione)
                .IsRequired()
                .HasMaxLength(500);

            // ✅ PROPRIETÀ OPZIONALI CON LUNGHEZZA
            builder.Property(l => l.Dettagli)
                .HasMaxLength(2000);

            // ✅ VALORI DEFAULT
            builder.Property(l => l.DataEsecuzione)
                .HasDefaultValueSql("GETDATE()");

            // ✅ INDICI PER PERFORMANCE
            builder.HasIndex(l => l.DataEsecuzione);
            builder.HasIndex(l => l.TipoAttivita);
            builder.HasIndex(l => l.UtenteId);

            // ✅ RELAZIONE CON UTENTI
            builder.HasOne(l => l.Utente)
                .WithMany()
                .HasForeignKey(l => l.UtenteId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}