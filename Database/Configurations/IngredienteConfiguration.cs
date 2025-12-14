using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BBltZen;

namespace Database.Configurations
{
    public class IngredienteConfiguration : IEntityTypeConfiguration<Ingrediente>
    {
        public void Configure(EntityTypeBuilder<Ingrediente> builder)
        {
            // ✅ CHIAVE PRIMARIA
            builder.HasKey(i => i.IngredienteId);

            // ✅ PROPRIETÀ OBBLIGATORIE
            builder.Property(i => i.Ingrediente1)
                .IsRequired()
                .HasMaxLength(50)
                .HasColumnName("Ingrediente"); // ✅ Nome colonna esplicito

            builder.Property(i => i.CategoriaId)
                .IsRequired();

            builder.Property(i => i.PrezzoAggiunto)
                .IsRequired()
                .HasColumnType("decimal(5,2)"); // ✅ Formato prezzo

            builder.Property(i => i.Disponibile)
                .IsRequired()
                .HasDefaultValue(true); // ✅ Default true

            builder.Property(i => i.DataInserimento)
                .IsRequired();

            builder.Property(i => i.DataAggiornamento)
                .IsRequired();

            // ✅ INDICE UNIVOCO SUL NOME
            builder.HasIndex(i => i.Ingrediente1)
                .IsUnique();

            // ✅ INDICE SU CATEGORIA
            builder.HasIndex(i => i.CategoriaId);

            // ✅ INDICE SU DISPONIBILITÀ
            builder.HasIndex(i => i.Disponibile);

            // ✅ RELAZIONE CON CATEGORIA - CORRETTA CON NOME ESATTO
            builder.HasOne(i => i.Categoria)
                .WithMany(c => c.Ingrediente) // ✅ "Ingrediente" (singolare) non "Ingredienti"
                .HasForeignKey(i => i.CategoriaId)
                .OnDelete(DeleteBehavior.Restrict); // ✅ Previene eliminazione cascata

            // ✅ RELAZIONI CON PERSONALIZZAZIONI
            builder.HasMany(i => i.IngredientiPersonalizzazione)
                .WithOne(ip => ip.Ingrediente)
                .HasForeignKey(ip => ip.IngredienteId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(i => i.PersonalizzazioneIngrediente)
                .WithOne(pi => pi.Ingrediente)
                .HasForeignKey(pi => pi.IngredienteId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}