using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Database;

namespace Database.Configurations
{
    public class ArticoloConfiguration : IEntityTypeConfiguration<Articolo>
    {
        public void Configure(EntityTypeBuilder<Articolo> builder)
        {
            // ✅ CHIAVE PRIMARIA
            builder.HasKey(a => a.ArticoloId);

            // ✅ PROPRIETÀ OBBLIGATORIE
            builder.Property(a => a.Tipo)
                .IsRequired()
                .HasMaxLength(50)
                .HasConversion(
                    v => v.ToUpper(), // ✅ Salva sempre in maiuscolo
                    v => v
                );

            builder.Property(a => a.DataCreazione)
                .IsRequired();

            builder.Property(a => a.DataAggiornamento)
                .IsRequired();

            // ✅ VALORI DEFAULT
            builder.Property(a => a.DataCreazione)
                .HasDefaultValueSql("GETDATE()");

            builder.Property(a => a.DataAggiornamento)
                .HasDefaultValueSql("GETDATE()");

            // ✅ INDICI PER PERFORMANCE
            builder.HasIndex(a => a.Tipo); // ✅ Ricerche per tipo articolo
            builder.HasIndex(a => a.DataCreazione);
            builder.HasIndex(a => a.DataAggiornamento);

            // ✅ RELAZIONE CON BEVANDE CUSTOM
            builder.HasMany(a => a.BevandaCustom)
                .WithOne(bc => bc.Articolo)
                .HasForeignKey(bc => bc.ArticoloId)
                .OnDelete(DeleteBehavior.Restrict); // ✅ Previene eliminazione articolo con bevande custom

            // ✅ RELAZIONE 1:1 CON BEVANDA STANDARD (TPH)
            builder.HasOne(a => a.BevandaStandard)
                .WithOne(bs => bs.Articolo)
                .HasForeignKey<BevandaStandard>(bs => bs.ArticoloId)
                .OnDelete(DeleteBehavior.Cascade); // ✅ Elimina bevanda standard se articolo viene eliminato

            // ✅ RELAZIONE 1:1 CON DOLCE (TPH)
            builder.HasOne(a => a.Dolce)
                .WithOne(d => d.Articolo)
                .HasForeignKey<Dolce>(d => d.ArticoloId)
                .OnDelete(DeleteBehavior.Cascade); // ✅ Elimina dolce se articolo viene eliminato

            // ✅ RELAZIONE CON ORDER ITEM
            builder.HasMany(a => a.OrderItem)
                .WithOne(oi => oi.Articolo)
                .HasForeignKey(oi => oi.ArticoloId)
                .OnDelete(DeleteBehavior.Restrict); // ✅ Previene eliminazione articolo con ordini

            // ✅ CHECK CONSTRAINTS
            builder.ToTable(tb =>
            {
                tb.HasCheckConstraint("CK_Articolo_Tipo",
                    "[Tipo] IN ('BEVANDA_STANDARD', 'DOLCE', 'BEVANDA_CUSTOM')"); // ✅ Tipi articolo validi

                tb.HasCheckConstraint("CK_Articolo_DateConsistency",
                    "[DataAggiornamento] >= [DataCreazione]"); // ✅ Consistenza temporale
            });

            // ✅ CONFIGURAZIONE TPH (TABLE PER HIERARCHY)
            // ✅ Articolo è la tabella base per l'ereditarietà
            // ✅ BevandaStandard e Dolce sono entità derivate che condividono la stessa tabella

            // ✅ CONFIGURAZIONE NOME TABELLA
            builder.ToTable("Articolo");

            // ✅ COMMENTI PER DOCUMENTAZIONE (opzionale)
            // builder.HasComment("Tabella base per la gestione degli articoli (TPH Pattern)");
        }
    }
}