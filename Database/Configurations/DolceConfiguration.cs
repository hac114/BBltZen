using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BBltZen;

namespace Database.Configurations
{
    public class DolceConfiguration : IEntityTypeConfiguration<Dolce>
    {
        public void Configure(EntityTypeBuilder<Dolce> builder)
        {
            // ✅ CHIAVE PRIMARIA E FOREIGN KEY
            builder.HasKey(d => d.ArticoloId);

            // ✅ PROPRIETÀ OBBLIGATORIE
            builder.Property(d => d.Nome)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(d => d.Prezzo)
                .IsRequired()
                .HasColumnType("decimal(10,2)"); // ✅ Formato prezzo

            builder.Property(d => d.Disponibile)
                .IsRequired()
                .HasDefaultValue(true); // ✅ Default disponibile

            builder.Property(d => d.Priorita)
                .IsRequired()
                .HasDefaultValue(1); // ✅ Priorità default

            builder.Property(d => d.DataCreazione)
                .IsRequired();

            builder.Property(d => d.DataAggiornamento)
                .IsRequired();

            // ✅ PROPRIETÀ OPZIONALI CON LUNGHEZZA
            builder.Property(d => d.Descrizione)
                .HasMaxLength(500); // ✅ Descrizione opzionale

            builder.Property(d => d.ImmagineUrl)
                .HasMaxLength(500); // ✅ URL immagine opzionale

            // ✅ VALORI DEFAULT
            builder.Property(d => d.DataCreazione)
                .HasDefaultValueSql("GETDATE()");

            builder.Property(d => d.DataAggiornamento)
                .HasDefaultValueSql("GETDATE()");

            // ✅ INDICI PER PERFORMANCE
            builder.HasIndex(d => d.Nome)
                .IsUnique(); // ✅ Nome univoco per dolce

            builder.HasIndex(d => d.Disponibile); // ✅ Ricerche per disponibilità

            builder.HasIndex(d => d.Priorita); // ✅ Ordinamento per priorità

            builder.HasIndex(d => d.Prezzo); // ✅ Ricerche per prezzo

            builder.HasIndex(d => d.DataCreazione); // ✅ Storico

            // ✅ RELAZIONE CON ARTICOLO (TPH - Table Per Hierarchy)
            builder.HasOne(d => d.Articolo)
                .WithOne() // ✅ Relazione 1:1 con Articolo
                .HasForeignKey<Dolce>(d => d.ArticoloId)
                .OnDelete(DeleteBehavior.Cascade); // ✅ Elimina dolce se articolo viene eliminato

            // ✅ CHECK CONSTRAINTS
            builder.ToTable(tb =>
            {
                tb.HasCheckConstraint("CK_Dolce_Prezzo",
                    "[Prezzo] >= 0 AND [Prezzo] <= 100"); // ✅ Prezzo tra 0 e 100

                tb.HasCheckConstraint("CK_Dolce_Priorita",
                    "[Priorita] BETWEEN 1 AND 10"); // ✅ Priorità tra 1 e 10
            });

            // ✅ CONFIGURAZIONE NOME TABELLA
            builder.ToTable("Dolce");

            // ✅ COMMENTI PER DOCUMENTAZIONE (opzionale)
            // builder.HasComment("Tabella per la gestione dei dolci nel menu");
        }
    }
}