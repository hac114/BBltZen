using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BBltZen;

namespace Database.Configurations
{
    public class BevandaStandardConfiguration : IEntityTypeConfiguration<BevandaStandard>
    {
        public void Configure(EntityTypeBuilder<BevandaStandard> builder)
        {
            // ✅ CHIAVE PRIMARIA
            builder.HasKey(bs => bs.ArticoloId);

            // ✅ VINCOLO UNIQUE AGGIUNTO (colonne: PersonalizzazioneId, DimensioneBicchiereId)
            builder.HasIndex(bs => new { bs.PersonalizzazioneId, bs.DimensioneBicchiereId })
                   .IsUnique()
                   .HasDatabaseName("UQ_BevandaStandard_Personalizzazione_Dimensione");

            // ✅ TIPO DECIMAL CORRETTO (4,2)
            builder.Property(bs => bs.Prezzo)
                .IsRequired()
                .HasColumnType("decimal(4,2)");

            // ✅ VALORI DEFAULT
            builder.Property(bs => bs.Disponibile)
                .IsRequired()
                .HasDefaultValue(true);

            builder.Property(bs => bs.SempreDisponibile)
                .IsRequired()
                .HasDefaultValue(false);

            builder.Property(bs => bs.Priorita)
                .IsRequired()
                .HasDefaultValue(1);

            builder.Property(bs => bs.DataCreazione)
                .IsRequired()
                .HasDefaultValueSql("GETDATE()");

            builder.Property(bs => bs.DataAggiornamento)
                .IsRequired()
                .HasDefaultValueSql("GETDATE()");

            // ✅ IMMAGINE URL (opzionale, max 500)
            builder.Property(bs => bs.ImmagineUrl)
                .HasMaxLength(500)
                .IsRequired(false);

            // ✅ INDICE COMPOSITO (quello che abbiamo creato)
            builder.HasIndex(bs => new { bs.SempreDisponibile, bs.Priorita, bs.Disponibile })
                   .HasDatabaseName("IX_BevandaStandard_Completo");

            // ✅ RELAZIONI con comportamento NO_ACTION (come nel DB)
            builder.HasOne(bs => bs.Articolo)
                .WithOne()
                .HasForeignKey<BevandaStandard>(bs => bs.ArticoloId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(bs => bs.Personalizzazione)
                .WithMany(p => p.BevandaStandard)
                .HasForeignKey(bs => bs.PersonalizzazioneId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(bs => bs.DimensioneBicchiere)
                .WithMany(db => db.BevandaStandard)
                .HasForeignKey(bs => bs.DimensioneBicchiereId)
                .OnDelete(DeleteBehavior.NoAction);

            // ✅ CHECK CONSTRAINTS (allineati al DB)
            builder.ToTable(tb =>
            {
                // Prezzo >= 0
                tb.HasCheckConstraint("CK_prezzo_positivo", "[Prezzo] >= 0");

                // Priorità 1-10
                tb.HasCheckConstraint("CHK_BevandaStandard_Priorita_Range",
                    "[Priorita] >= 1 AND [Priorita] <= 10");

                // ✅ NUOVO: Vincolo di coerenza
                tb.HasCheckConstraint("CHK_Disponibilita_Coerente",
                    "([SempreDisponibile] = 0 AND [Disponibile] = 0) OR ([SempreDisponibile] = 1)");
            });

            // ✅ NOME TABELLA IN MAIUSCOLO (come nel DB)
            builder.ToTable("BEVANDA_STANDARD");
        }
    }
}