using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Database;

namespace Database.Configurations
{
    public class UtentiConfiguration : IEntityTypeConfiguration<Utenti>
    {
        public void Configure(EntityTypeBuilder<Utenti> builder)
        {
            // ✅ VALORI DEFAULT A LIVELLO DATABASE
            builder.Property(u => u.TipoUtente)
                .HasDefaultValue("cliente")
                .IsRequired();

            builder.Property(u => u.DataCreazione)
                .HasDefaultValueSql("GETDATE()"); // Per SQL Server

            builder.Property(u => u.DataAggiornamento)
                .HasDefaultValueSql("GETDATE()");

            builder.Property(u => u.Attivo)
                .HasDefaultValue(true);

            // ✅ VALIDAZIONI E CONFIGURAZIONI
            builder.Property(u => u.Email)
                .HasMaxLength(255);

            builder.Property(u => u.PasswordHash)
                .HasMaxLength(512);

            builder.Property(u => u.TipoUtente)
                .HasMaxLength(50);

            builder.Property(u => u.Nome)
                .HasMaxLength(100);

            builder.Property(u => u.Cognome)
                .HasMaxLength(100);

            builder.Property(u => u.Telefono)
                .HasMaxLength(20);
        }
    }
}