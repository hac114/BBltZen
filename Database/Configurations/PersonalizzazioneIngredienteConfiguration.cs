using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Database.Models;

namespace Database.Configurations
{
    public class PersonalizzazioneIngredienteConfiguration : IEntityTypeConfiguration<PersonalizzazioneIngrediente>
    {
        public void Configure(EntityTypeBuilder<PersonalizzazioneIngrediente> builder)
        {
            // ✅ CHIAVE PRIMARIA
            builder.HasKey(pi => pi.PersonalizzazioneIngredienteId);

            // ✅ PROPRIETÀ OBBLIGATORIE
            builder.Property(pi => pi.PersonalizzazioneId)
                .IsRequired();

            builder.Property(pi => pi.IngredienteId)
                .IsRequired();

            builder.Property(pi => pi.Quantita)
                .IsRequired()
                .HasColumnType("decimal(10,3)"); // ✅ Precisione quantità

            builder.Property(pi => pi.UnitaMisuraId)
                .IsRequired();

            // ✅ INDICI PER PERFORMANCE
            builder.HasIndex(pi => pi.PersonalizzazioneId);
            builder.HasIndex(pi => pi.IngredienteId);
            builder.HasIndex(pi => pi.UnitaMisuraId);

            // ✅ INDICE UNIVOCO PER EVITARE DUPLICATI
            builder.HasIndex(pi => new { pi.PersonalizzazioneId, pi.IngredienteId })
                .IsUnique();

            // ✅ RELAZIONI
            builder.HasOne(pi => pi.Personalizzazione)
                .WithMany(p => p.PersonalizzazioneIngrediente)
                .HasForeignKey(pi => pi.PersonalizzazioneId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(pi => pi.Ingrediente)
                .WithMany(i => i.PersonalizzazioneIngrediente)
                .HasForeignKey(pi => pi.IngredienteId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(pi => pi.UnitaMisura)
                .WithMany()
                .HasForeignKey(pi => pi.UnitaMisuraId)
                .OnDelete(DeleteBehavior.Restrict);

            // ✅ RELAZIONE CON DIMENSIONE QUANTITA INGREDIENTI
            builder.HasMany(pi => pi.DimensioneQuantitaIngredienti)
                .WithOne(dqi => dqi.PersonalizzazioneIngrediente)
                .HasForeignKey(dqi => dqi.PersonalizzazioneIngredienteId)
                .OnDelete(DeleteBehavior.Cascade);

            // ✅ CHECK CONSTRAINTS
            builder.ToTable(tb => tb.HasCheckConstraint(
                "CK_PersonalizzazioneIngrediente_Quantita",
                "[Quantita] > 0 AND [Quantita] <= 1000"));

            builder.ToTable("PersonalizzazioneIngrediente");
        }
    }
}