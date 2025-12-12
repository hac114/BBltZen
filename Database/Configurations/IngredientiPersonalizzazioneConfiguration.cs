using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Database.Models;

namespace Database.Configurations
{
    public class IngredientiPersonalizzazioneConfiguration : IEntityTypeConfiguration<IngredientiPersonalizzazione>
    {
        public void Configure(EntityTypeBuilder<IngredientiPersonalizzazione> builder)
        {
            // ✅ CHIAVE PRIMARIA
            builder.HasKey(ip => ip.IngredientePersId);

            // ✅ PROPRIETÀ OBBLIGATORIE
            builder.Property(ip => ip.PersCustomId)
                .IsRequired();

            builder.Property(ip => ip.IngredienteId)
                .IsRequired();

            builder.Property(ip => ip.DataCreazione)
                .IsRequired()
                .HasDefaultValueSql("GETDATE()");

            // ✅ INDICE UNIVOCO PER EVITARE DUPLICATI
            builder.HasIndex(ip => new { ip.PersCustomId, ip.IngredienteId })
                .IsUnique();

            // ✅ INDICI PER PERFORMANCE
            builder.HasIndex(ip => ip.PersCustomId);
            builder.HasIndex(ip => ip.IngredienteId);
            builder.HasIndex(ip => ip.DataCreazione);

            // ✅ RELAZIONI
            builder.HasOne(ip => ip.PersCustom)
                .WithMany(pc => pc.IngredientiPersonalizzazione)
                .HasForeignKey(ip => ip.PersCustomId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(ip => ip.Ingrediente)
                .WithMany(i => i.IngredientiPersonalizzazione)
                .HasForeignKey(ip => ip.IngredienteId)
                .OnDelete(DeleteBehavior.Restrict);

            // ✅ CONFIGURAZIONE IDENTITY
            builder.Property(ip => ip.IngredientePersId)
                .ValueGeneratedOnAdd();

            // ✅ CONFIGURAZIONE NOME TABELLA
            builder.ToTable("IngredientiPersonalizzazione");
        }
    }
}