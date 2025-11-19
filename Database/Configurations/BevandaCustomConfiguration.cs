using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Database;

namespace Database.Configurations
{
    public class BevandaCustomConfiguration : IEntityTypeConfiguration<BevandaCustom>
    {
        public void Configure(EntityTypeBuilder<BevandaCustom> builder)
        {
            // ✅ CHIAVE PRIMARIA
            builder.HasKey(bc => bc.BevandaCustomId);

            // ✅ PROPRIETÀ OBBLIGATORIE
            builder.Property(bc => bc.ArticoloId)
                .IsRequired();

            builder.Property(bc => bc.PersCustomId)
                .IsRequired();

            builder.Property(bc => bc.Prezzo)
                .IsRequired()
                .HasColumnType("decimal(10,2)"); // ✅ Formato prezzo

            builder.Property(bc => bc.DataCreazione)
                .IsRequired();

            builder.Property(bc => bc.DataAggiornamento)
                .IsRequired();

            // ✅ VALORI DEFAULT
            builder.Property(bc => bc.DataCreazione)
                .HasDefaultValueSql("GETDATE()");

            builder.Property(bc => bc.DataAggiornamento)
                .HasDefaultValueSql("GETDATE()");

            // ✅ INDICI PER PERFORMANCE
            builder.HasIndex(bc => bc.ArticoloId);
            builder.HasIndex(bc => bc.PersCustomId);
            builder.HasIndex(bc => bc.DataCreazione);
            builder.HasIndex(bc => bc.Prezzo);

            // ✅ INDICE UNIVOCO PER EVITARE DUPLICATI
            builder.HasIndex(bc => new { bc.ArticoloId, bc.PersCustomId })
                .IsUnique(); // ✅ Una bevanda custom unica per articolo e personalizzazione

            // ✅ RELAZIONE CON ARTICOLO
            builder.HasOne(bc => bc.Articolo)
                .WithMany() // ✅ Assumendo che Articolo non abbia navigation property per BevandaCustom
                .HasForeignKey(bc => bc.ArticoloId)
                .OnDelete(DeleteBehavior.Cascade); // ✅ Elimina bevanda custom se articolo viene eliminato

            // ✅ RELAZIONE CON PERSONALIZZAZIONE CUSTOM
            builder.HasOne(bc => bc.PersCustom)
                .WithMany() // ✅ Assumendo che PersonalizzazioneCustom non abbia navigation property per BevandaCustom
                .HasForeignKey(bc => bc.PersCustomId)
                .OnDelete(DeleteBehavior.Restrict); // ✅ Previene eliminazione personalizzazione con bevande custom

            // ✅ CHECK CONSTRAINTS
            builder.ToTable(tb =>
            {
                tb.HasCheckConstraint("CK_BevandaCustom_Prezzo",
                    "[Prezzo] >= 0 AND [Prezzo] <= 50"); // ✅ Prezzo tra 0 e 50 euro

                tb.HasCheckConstraint("CK_BevandaCustom_DateConsistency",
                    "[DataAggiornamento] >= [DataCreazione]"); // ✅ Consistenza temporale
            });

            // ✅ CONFIGURAZIONE NOME TABELLA
            builder.ToTable("BevandaCustom");

            // ✅ COMMENTI PER DOCUMENTAZIONE (opzionale)
            // builder.HasComment("Tabella per la gestione delle bevande personalizzate create dai clienti");
        }
    }
}