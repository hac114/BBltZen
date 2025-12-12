using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Database.Models;

namespace Database.Configurations
{
    public class StatoStoricoOrdineConfiguration : IEntityTypeConfiguration<StatoStoricoOrdine>
    {
        public void Configure(EntityTypeBuilder<StatoStoricoOrdine> builder)
        {
            // ✅ CHIAVE PRIMARIA
            builder.HasKey(s => s.StatoStoricoOrdineId);

            // ✅ PROPRIETÀ OBBLIGATORIE
            builder.Property(s => s.Inizio)
                .IsRequired();

            // ✅ VALORI DEFAULT
            builder.Property(s => s.Inizio)
                .HasDefaultValueSql("GETDATE()");

            // ✅ INDICI PER PERFORMANCE
            builder.HasIndex(s => s.OrdineId);
            builder.HasIndex(s => s.StatoOrdineId);
            builder.HasIndex(s => s.Inizio);
            builder.HasIndex(s => s.Fine);

            // ✅ INDICE UNIVOCO PER STATO ATTIVO
            builder.HasIndex(s => new { s.OrdineId, s.Fine })
                .HasFilter("[Fine] IS NULL")
                .IsUnique();

            // ✅ RELAZIONI
            builder.HasOne(s => s.Ordine)
                .WithMany()
                .HasForeignKey(s => s.OrdineId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(s => s.StatoOrdine)
                .WithMany()
                .HasForeignKey(s => s.StatoOrdineId)
                .OnDelete(DeleteBehavior.Restrict);

            // ✅ CHECK CONSTRAINTS - SINTASSI CORRETTA (NON OBSOLETA)
            builder.ToTable(tb => tb.HasCheckConstraint(
                "CK_StatoStoricoOrdine_Date",
                "[Fine] IS NULL OR [Fine] >= [Inizio]"));
        }
    }
}