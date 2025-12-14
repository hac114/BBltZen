using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BBltZen;

namespace Database.Configurations
{
    public class PreferitiClienteConfiguration : IEntityTypeConfiguration<PreferitiCliente>
    {
        public void Configure(EntityTypeBuilder<PreferitiCliente> builder)
        {
            // ✅ CHIAVE PRIMARIA
            builder.HasKey(pc => pc.PreferitoId);

            // ✅ PROPRIETÀ OBBLIGATORIE
            builder.Property(pc => pc.ClienteId)
                .IsRequired();

            builder.Property(pc => pc.BevandaId)
                .IsRequired();

            builder.Property(pc => pc.TipoArticolo)
                .IsRequired()
                .HasMaxLength(50)
                .HasConversion(
                    v => v.ToUpper(), // ✅ Salva sempre in maiuscolo
                    v => v
                );

            // ✅ PROPRIETÀ OPZIONALI CON LUNGHEZZA
            builder.Property(pc => pc.NomePersonalizzato)
                .HasMaxLength(100); // ✅ Nome personalizzato opzionale

            builder.Property(pc => pc.NotePersonali)
                .HasMaxLength(500); // ✅ Note personali opzionali

            builder.Property(pc => pc.IngredientiJson)
                .HasMaxLength(2000); // ✅ JSON ingredienti opzionale

            // ✅ VALORI DEFAULT
            builder.Property(pc => pc.DataAggiunta)
                .HasDefaultValueSql("GETDATE()");

            // ✅ INDICI PER PERFORMANCE
            builder.HasIndex(pc => pc.ClienteId);
            builder.HasIndex(pc => pc.BevandaId);
            builder.HasIndex(pc => pc.TipoArticolo);
            builder.HasIndex(pc => pc.DimensioneBicchiereId);
            builder.HasIndex(pc => pc.DataAggiunta);

            // ✅ INDICE UNIVOCO PER EVITARE DUPLICATI
            builder.HasIndex(pc => new { pc.ClienteId, pc.BevandaId, pc.TipoArticolo, pc.DimensioneBicchiereId })
                .IsUnique(); // ✅ Combinazione unica cliente-bevanda-tipo-dimensione

            // ✅ RELAZIONE CON CLIENTE
            builder.HasOne(pc => pc.Cliente)
                .WithMany(c => c.PreferitiCliente)
                .HasForeignKey(pc => pc.ClienteId)
                .OnDelete(DeleteBehavior.Cascade); // ✅ Elimina preferiti quando cliente viene eliminato

            // ✅ RELAZIONE CON DIMENSIONE BICCHIERE
            builder.HasOne(pc => pc.DimensioneBicchiere)
                .WithMany(db => db.PreferitiCliente)
                .HasForeignKey(pc => pc.DimensioneBicchiereId)
                .OnDelete(DeleteBehavior.SetNull); // ✅ Mantiene preferito se dimensione viene eliminata

            // ✅ CHECK CONSTRAINTS
            builder.ToTable(tb =>
            {
                tb.HasCheckConstraint("CK_PreferitiCliente_TipoArticolo",
                    "[TipoArticolo] IN ('BEVANDA_STANDARD', 'BEVANDA_CUSTOM')"); // ✅ Tipi articolo validi

                tb.HasCheckConstraint("CK_PreferitiCliente_GradoDolcezza",
                    "[GradoDolcezza] IS NULL OR [GradoDolcezza] BETWEEN 0 AND 10"); // ✅ Grado dolcezza opzionale tra 0-10

                tb.HasCheckConstraint("CK_PreferitiCliente_DataAggiunta",
                    "[DataAggiunta] IS NULL OR [DataAggiunta] <= GETDATE()"); // ✅ Data aggiunta non futura
            });

            // ✅ CONFIGURAZIONE NOME TABELLA
            builder.ToTable("PreferitiCliente");

            // ✅ COMMENTI PER DOCUMENTAZIONE (opzionale)
            // builder.HasComment("Tabella per la gestione delle bevande preferite dei clienti");
        }
    }
}