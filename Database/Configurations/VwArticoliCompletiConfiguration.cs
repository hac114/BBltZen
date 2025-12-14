using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BBltZen;

namespace Database.Configurations
{
    public class VwArticoliCompletiConfiguration : IEntityTypeConfiguration<VwArticoliCompleti>
    {
        public void Configure(EntityTypeBuilder<VwArticoliCompleti> builder)
        {
            // ✅ VISTA - NO CHIAVE PRIMARIA (è una vista)
            builder.ToView("VwArticoliCompleti"); // Specifica il nome della vista

            // ✅ PROPRIETÀ OBBLIGATORIE
            builder.Property(v => v.TipoArticolo)
                .IsRequired()
                .HasMaxLength(10);

            builder.Property(v => v.Categoria)
                .IsRequired()
                .HasMaxLength(100);

            // ✅ PROPRIETÀ OPZIONALI
            builder.Property(v => v.NomeArticolo)
                .HasMaxLength(200);

            builder.Property(v => v.PrezzoBase)
                .HasPrecision(10, 2);

            builder.Property(v => v.AliquotaIva)
                .HasPrecision(5, 2);

            // ✅ INDICI PER PERFORMANCE (se supportati dalla vista)
            builder.HasIndex(v => v.TipoArticolo);
            builder.HasIndex(v => v.Categoria);
            builder.HasIndex(v => v.Disponibile);
            builder.HasIndex(v => v.PrezzoBase);
        }
    }
}