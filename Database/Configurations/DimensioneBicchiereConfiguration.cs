using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BBltZen;

namespace Database.Configurations
{
    public class DimensioneBicchiereConfiguration : IEntityTypeConfiguration<DimensioneBicchiere>
    {
        public void Configure(EntityTypeBuilder<DimensioneBicchiere> builder)
        {
            // ✅ CHIAVE PRIMARIA
            builder.HasKey(d => d.DimensioneBicchiereId);

            // ✅ PROPRIETÀ OBBLIGATORIE
            builder.Property(d => d.Sigla)
                .IsRequired()
                .HasMaxLength(3);

            builder.Property(d => d.Descrizione)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(d => d.Capienza)
                .IsRequired()
                .HasPrecision(8, 2);

            builder.Property(d => d.PrezzoBase)
                .IsRequired()
                .HasPrecision(8, 2);

            builder.Property(d => d.Moltiplicatore)
                .IsRequired()
                .HasPrecision(4, 2);

            // ✅ RELAZIONE CORRETTA CON UnitaDiMisura
            builder.HasOne(d => d.UnitaMisura)
                .WithMany(u => u.DimensioneBicchiere)
                .HasForeignKey(d => d.UnitaMisuraId)
                .OnDelete(DeleteBehavior.Restrict);

            // ❌ RIMOSSO vecchio indice UNIQUE solo su Sigla
            // ❌ RIMOSSO indice semplice su Descrizione

            // ✅ NUOVO INDICE UNIQUE COMBINATO (Sigla + Descrizione)
            builder.HasIndex(d => new { d.Sigla, d.Descrizione })
                .IsUnique()
                .HasDatabaseName("UQ_DimensioneBicchiere_SiglaDescrizione");

            // ✅ INDICE PER CHIAVE ESTERNA (performance)
            builder.HasIndex(d => d.UnitaMisuraId);
        }
    }
}
