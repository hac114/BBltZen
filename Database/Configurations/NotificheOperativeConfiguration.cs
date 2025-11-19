using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Database;

namespace Database.Configurations
{
    public class NotificheOperativeConfiguration : IEntityTypeConfiguration<NotificheOperative>
    {
        public void Configure(EntityTypeBuilder<NotificheOperative> builder)
        {
            // ✅ CHIAVE PRIMARIA
            builder.HasKey(n => n.NotificaId);

            // ✅ PROPRIETÀ OBBLIGATORIE
            builder.Property(n => n.OrdiniCoinvolti)
                .IsRequired()
                .HasMaxLength(1000);

            builder.Property(n => n.Messaggio)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(n => n.Stato)
                .IsRequired()
                .HasMaxLength(20)
                .HasDefaultValue("Pendente");

            // ✅ PROPRIETÀ OPZIONALI CON LUNGHEZZA
            builder.Property(n => n.UtenteGestione)
                .HasMaxLength(100);

            builder.Property(n => n.TipoNotifica)
                .HasMaxLength(50)
                .HasDefaultValue("sistema");

            // ✅ VALORI DEFAULT
            builder.Property(n => n.DataCreazione)
                .HasDefaultValueSql("GETDATE()");

            builder.Property(n => n.Priorita)
                .HasDefaultValue(1);

            // ✅ CHECK CONSTRAINTS - SINTASSI CORRETTA (NON OBSOLETA)
            builder.ToTable(tb =>
            {
                tb.HasCheckConstraint("CK_NotificheOperative_Stato",
                    "[Stato] IN ('Pendente', 'In Lavorazione', 'Gestita', 'Annullata')");

                tb.HasCheckConstraint("CK_NotificheOperative_Priorita",
                    "[Priorita] BETWEEN 1 AND 10");
            });

            // ✅ INDICI PER PERFORMANCE
            builder.HasIndex(n => n.Stato);
            builder.HasIndex(n => n.Priorita);
            builder.HasIndex(n => n.DataCreazione);
            builder.HasIndex(n => n.DataGestione);
        }
    }
}