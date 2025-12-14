using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BBltZen;

namespace Database.Configurations
{
    public class BevandaStandardConfiguration : IEntityTypeConfiguration<BevandaStandard>
    {
        public void Configure(EntityTypeBuilder<BevandaStandard> builder)
        {
            // ✅ CHIAVE PRIMARIA E FOREIGN KEY
            builder.HasKey(bs => bs.ArticoloId);

            // ✅ PROPRIETÀ OBBLIGATORIE
            builder.Property(bs => bs.PersonalizzazioneId)
                .IsRequired();

            builder.Property(bs => bs.DimensioneBicchiereId)
                .IsRequired();

            builder.Property(bs => bs.Prezzo)
                .IsRequired()
                .HasColumnType("decimal(10,2)"); // ✅ Formato prezzo

            builder.Property(bs => bs.Disponibile)
                .IsRequired()
                .HasDefaultValue(true); // ✅ Default disponibile

            builder.Property(bs => bs.SempreDisponibile)
                .IsRequired()
                .HasDefaultValue(false); // ✅ Default non sempre disponibile

            builder.Property(bs => bs.Priorita)
                .IsRequired()
                .HasDefaultValue(1); // ✅ Priorità default

            builder.Property(bs => bs.DataCreazione)
                .IsRequired();

            builder.Property(bs => bs.DataAggiornamento)
                .IsRequired();

            // ✅ PROPRIETÀ OPZIONALI CON LUNGHEZZA
            builder.Property(bs => bs.ImmagineUrl)
                .HasMaxLength(500); // ✅ URL immagine opzionale

            // ✅ VALORI DEFAULT
            builder.Property(bs => bs.DataCreazione)
                .HasDefaultValueSql("GETDATE()");

            builder.Property(bs => bs.DataAggiornamento)
                .HasDefaultValueSql("GETDATE()");

            // ✅ INDICI PER PERFORMANCE
            builder.HasIndex(bs => bs.PersonalizzazioneId);
            builder.HasIndex(bs => bs.DimensioneBicchiereId);
            builder.HasIndex(bs => bs.Disponibile);
            builder.HasIndex(bs => bs.Priorita);
            builder.HasIndex(bs => bs.Prezzo);
            builder.HasIndex(bs => bs.DataCreazione);

            // ✅ INDICE UNIVOCO PER EVITARE DUPLICATI
            builder.HasIndex(bs => new { bs.ArticoloId, bs.PersonalizzazioneId, bs.DimensioneBicchiereId })
                .IsUnique(); // ✅ Combinazione unica articolo + personalizzazione + dimensione

            // ✅ RELAZIONE CON ARTICOLO (TPH - Table Per Hierarchy)
            builder.HasOne(bs => bs.Articolo)
                .WithOne() // ✅ Relazione 1:1 con Articolo
                .HasForeignKey<BevandaStandard>(bs => bs.ArticoloId)
                .OnDelete(DeleteBehavior.Cascade); // ✅ Elimina bevanda standard se articolo viene eliminato

            // ✅ RELAZIONE CON PERSONALIZZAZIONE
            builder.HasOne(bs => bs.Personalizzazione)
                .WithMany(p => p.BevandaStandard) // ✅ Personalizzazione ha molte BevandaStandard
                .HasForeignKey(bs => bs.PersonalizzazioneId)
                .OnDelete(DeleteBehavior.Restrict); // ✅ Previene eliminazione personalizzazione con bevande standard

            // ✅ RELAZIONE CON DIMENSIONE BICCHIERE
            builder.HasOne(bs => bs.DimensioneBicchiere)
                .WithMany(db => db.BevandaStandard) // ✅ DimensioneBicchiere ha molte BevandaStandard
                .HasForeignKey(bs => bs.DimensioneBicchiereId)
                .OnDelete(DeleteBehavior.Restrict); // ✅ Previene eliminazione dimensione con bevande standard

            // ✅ CHECK CONSTRAINTS
            builder.ToTable(tb =>
            {
                tb.HasCheckConstraint("CK_BevandaStandard_Prezzo",
                    "[Prezzo] >= 0 AND [Prezzo] <= 50"); // ✅ Prezzo tra 0 e 50 euro

                tb.HasCheckConstraint("CK_BevandaStandard_Priorita",
                    "[Priorita] BETWEEN 1 AND 10"); // ✅ Priorità tra 1 e 10

                tb.HasCheckConstraint("CK_BevandaStandard_DateConsistency",
                    "[DataAggiornamento] >= [DataCreazione]"); // ✅ Consistenza temporale
            });

            // ✅ CONFIGURAZIONE NOME TABELLA
            builder.ToTable("BevandaStandard");

            // ✅ COMMENTI PER DOCUMENTAZIONE (opzionale)
            // builder.HasComment("Tabella per la gestione delle bevande standard del menu");
        }
    }
}