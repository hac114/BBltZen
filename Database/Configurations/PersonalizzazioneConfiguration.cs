using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Database.Models;

namespace Database.Configurations
{
    public class PersonalizzazioneConfiguration : IEntityTypeConfiguration<Personalizzazione>
    {
        public void Configure(EntityTypeBuilder<Personalizzazione> builder)
        {
            // ✅ CHIAVE PRIMARIA
            builder.HasKey(p => p.PersonalizzazioneId);

            // ✅ PROPRIETÀ OBBLIGATORIE
            builder.Property(p => p.Nome)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(p => p.Descrizione)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(p => p.DtCreazione)
                .IsRequired();

            // ✅ INDICE UNIVOCO SUL NOME
            builder.HasIndex(p => p.Nome)
                .IsUnique();

            // ✅ RELAZIONI
            builder.HasMany(p => p.BevandaStandard)
                .WithOne(bs => bs.Personalizzazione)
                .HasForeignKey(bs => bs.PersonalizzazioneId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(p => p.PersonalizzazioneIngrediente)
                .WithOne(pi => pi.Personalizzazione)
                .HasForeignKey(pi => pi.PersonalizzazioneId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}