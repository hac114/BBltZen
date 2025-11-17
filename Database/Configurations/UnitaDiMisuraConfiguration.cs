using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Database;

namespace Database.Configurations
{
    public class UnitaDiMisuraConfiguration : IEntityTypeConfiguration<UnitaDiMisura>
    {
        public void Configure(EntityTypeBuilder<UnitaDiMisura> builder)
        {
            // ✅ CHIAVE PRIMARIA
            builder.HasKey(u => u.UnitaMisuraId);

            // ✅ PROPRIETÀ OBBLIGATORIE
            builder.Property(u => u.Sigla)
                .IsRequired()
                .HasMaxLength(10);

            builder.Property(u => u.Descrizione)
                .IsRequired()
                .HasMaxLength(50);

            // ✅ INDICE UNIVOCO SULLA SIGLA
            builder.HasIndex(u => u.Sigla)
                .IsUnique();

            // ✅ RELAZIONE CON DimensioneBicchiere (lato UnitaDiMisura)
            builder.HasMany(u => u.DimensioneBicchiere)
                .WithOne(d => d.UnitaMisura)
                .HasForeignKey(d => d.UnitaMisuraId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}