using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BBltZen;

namespace Database.Configurations
{
    public class CategoriaIngredienteConfiguration : IEntityTypeConfiguration<CategoriaIngrediente>
    {
        public void Configure(EntityTypeBuilder<CategoriaIngrediente> builder)
        {
            // ✅ CHIAVE PRIMARIA
            builder.HasKey(c => c.CategoriaId);

            // ✅ PROPRIETÀ OBBLIGATORIE
            builder.Property(c => c.Categoria)
                .IsRequired()
                .HasMaxLength(50);

            // ✅ INDICE UNIVOCO SULLA CATEGORIA
            builder.HasIndex(c => c.Categoria)
                .IsUnique();

            // ✅ RELAZIONE CON Ingrediente (lato CategoriaIngrediente)
            builder.HasMany(c => c.Ingrediente)
                .WithOne(i => i.Categoria) // ✅ CORRETTO: usa la navigation property da Ingrediente
                .HasForeignKey(i => i.CategoriaId)
                .OnDelete(DeleteBehavior.Restrict);

            // ✅ NOME TABELLA ESPLICITO (opzionale)
            builder.ToTable("CategoriaIngrediente");
        }
    }
}