using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BBltZen;

namespace Database.Configurations
{
    public class DimensioneQuantitaIngredientiConfiguration : IEntityTypeConfiguration<DimensioneQuantitaIngredienti>
    {
        public void Configure(EntityTypeBuilder<DimensioneQuantitaIngredienti> builder)
        {
            // ✅ CHIAVE PRIMARIA
            builder.HasKey(dq => dq.DimensioneId);

            // ✅ PROPRIETÀ OBBLIGATORIE
            builder.Property(dq => dq.PersonalizzazioneIngredienteId)
                .IsRequired();

            builder.Property(dq => dq.DimensioneBicchiereId)
                .IsRequired();

            builder.Property(dq => dq.Moltiplicatore)
                .IsRequired()
                .HasColumnType("decimal(5,2)");

            // ✅ VINCOLO CHECK SU MOLTIPLICATORE
            builder.ToTable("Dimensione_quantita_ingredienti",
               t => t.HasCheckConstraint("CK_Dimensione_quantita_ingredienti",
                   "[moltiplicatore] > 0 AND [moltiplicatore] <= 10"));

            // ✅ INDICE UNIVOCO PER EVITARE DUPLICATI
            builder.HasIndex(dq => new { dq.DimensioneBicchiereId, dq.PersonalizzazioneIngredienteId })
                .IsUnique();

            // ✅ INDICI PER PERFORMANCE (già presenti)
            builder.HasIndex(dq => dq.DimensioneBicchiereId);
            builder.HasIndex(dq => dq.PersonalizzazioneIngredienteId);

            // ✅ RELAZIONI
            builder.HasOne(dq => dq.DimensioneBicchiere)
                .WithMany(db => db.DimensioneQuantitaIngredienti)
                .HasForeignKey(dq => dq.DimensioneBicchiereId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(dq => dq.PersonalizzazioneIngrediente)
                .WithMany(pi => pi.DimensioneQuantitaIngredienti)
                .HasForeignKey(dq => dq.PersonalizzazioneIngredienteId)
                .OnDelete(DeleteBehavior.Cascade);

            // ✅ CONFIGURAZIONE IDENTITY
            builder.Property(dq => dq.DimensioneId)
                .ValueGeneratedOnAdd();

            // ✅ CONFIGURAZIONE NOME TABELLA
            builder.ToTable("Dimensione_quantita_ingredienti");
        }
    }
}