using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Database;

namespace Database.Configurations
{
    public class SessioniQrConfiguration : IEntityTypeConfiguration<SessioniQr>
    {
        public void Configure(EntityTypeBuilder<SessioniQr> builder)
        {
            // ✅ CHIAVE PRIMARIA
            builder.HasKey(s => s.SessioneId);

            // ✅ PROPRIETÀ OBBLIGATORIE
            builder.Property(s => s.QrCode)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(s => s.CodiceSessione)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(s => s.Stato)
                .IsRequired()
                .HasMaxLength(20)
                .HasConversion(
                    v => v.ToUpper(),
                    v => v
                )
                .HasDefaultValue("ATTIVO");

            builder.Property(s => s.TavoloId)
                .IsRequired();

            builder.Property(s => s.DataScadenza)
                .IsRequired();

            // ✅ PROPRIETÀ OPZIONALI
            builder.Property(s => s.ClienteId)
                .IsRequired(false);

            builder.Property(s => s.DataCreazione)
                .IsRequired(false)
                .HasDefaultValueSql("GETUTCDATE()");

            builder.Property(s => s.Utilizzato)
                .IsRequired(false)
                .HasDefaultValue(false);

            builder.Property(s => s.DataUtilizzo)
                .IsRequired(false);

            // ✅ INDICI PER PERFORMANCE
            builder.HasIndex(s => s.CodiceSessione)
                .IsUnique();

            builder.HasIndex(s => s.QrCode)
                .IsUnique();

            builder.HasIndex(s => s.Stato);

            builder.HasIndex(s => s.TavoloId);

            builder.HasIndex(s => s.DataScadenza);

            builder.HasIndex(s => new { s.Stato, s.DataScadenza });

            // ✅ RELAZIONE CON TAVOLO
            builder.HasOne(s => s.Tavolo)
                .WithMany()
                .HasForeignKey(s => s.TavoloId)
                .OnDelete(DeleteBehavior.Restrict);

            // ✅ RELAZIONE CON ORDINI - CORRETTA CON NOME ESATTO
            builder.HasMany(s => s.Ordine)
                .WithOne(o => o.Sessione) // ✅ "Sessione" non "SessioneQr"
                .HasForeignKey(o => o.SessioneId)
                .OnDelete(DeleteBehavior.Restrict);

            // ✅ CONFIGURAZIONE NOME TABELLA
            builder.ToTable("SessioniQr");
        }
    }
}