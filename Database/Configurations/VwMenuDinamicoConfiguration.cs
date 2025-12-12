using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Database.Models;

namespace Database.Configurations
{
    public class VwMenuDinamicoConfiguration : IEntityTypeConfiguration<VwMenuDinamico>
    {
        public void Configure(EntityTypeBuilder<VwMenuDinamico> builder)
        {
            // ✅ VISTA - NO CHIAVE PRIMARIA
            builder.ToView("VwMenuDinamico");
            builder.HasNoKey(); // ✅ AGGIUNGI QUESTO!

            // ✅ PROPRIETÀ OBBLIGATORIE
            builder.Property(v => v.Tipo)
                .IsRequired()
                .HasMaxLength(10);

            builder.Property(v => v.NomeBevanda)
                .IsRequired()
                .HasMaxLength(200);

            // ✅ PROPRIETÀ OPZIONALI
            builder.Property(v => v.Descrizione)
                .HasMaxLength(500);

            builder.Property(v => v.PrezzoNetto)
                .HasPrecision(10, 2);

            builder.Property(v => v.PrezzoLordo)
                .HasPrecision(10, 2);

            builder.Property(v => v.IvaPercentuale)
                .HasPrecision(5, 2);

            builder.Property(v => v.ImmagineUrl)
                .HasMaxLength(500);

            // ✅ INDICI PER PERFORMANCE
            builder.HasIndex(v => v.Tipo);
            builder.HasIndex(v => v.Priorita);
            builder.HasIndex(v => new { v.Id, v.Tipo });
        }
    }
}