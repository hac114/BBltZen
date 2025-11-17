using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Database;

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

            // ✅ CORRETTO: CHIAVE ESTERNA (nome corretto della proprietà)
            builder.HasOne(d => d.UnitaMisura) // ✅ "UnitaMisura" invece di "UnitaDiMisura"
                .WithMany(u => u.DimensioneBicchiere) // ✅ SPECIFICA la collection navigation
                .HasForeignKey(d => d.UnitaMisuraId)
                .OnDelete(DeleteBehavior.Restrict);

            // ✅ INDICE UNIVOCO SULLA SIGLA
            builder.HasIndex(d => d.Sigla)
                .IsUnique();

            // ✅ INDICE SULLA DESCRIZIONE
            builder.HasIndex(d => d.Descrizione);

            // ✅ INDICE SULLA CHIAVE ESTERNA (per performance)
            builder.HasIndex(d => d.UnitaMisuraId);
        }
    }
}