using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Database;

namespace Database.Configurations
{
    public class PersonalizzazioneCustomConfiguration : IEntityTypeConfiguration<PersonalizzazioneCustom>
    {
        public void Configure(EntityTypeBuilder<PersonalizzazioneCustom> builder)
        {
            // ✅ CHIAVE PRIMARIA
            builder.HasKey(pc => pc.PersCustomId);

            // ✅ PROPRIETÀ OBBLIGATORIE
            builder.Property(pc => pc.Nome)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(pc => pc.GradoDolcezza)
                .IsRequired();

            builder.Property(pc => pc.DimensioneBicchiereId)
                .IsRequired();

            builder.Property(pc => pc.DataCreazione)
                .IsRequired();

            builder.Property(pc => pc.DataAggiornamento)
                .IsRequired();

            // ✅ VALORI DEFAULT
            builder.Property(pc => pc.DataCreazione)
                .HasDefaultValueSql("GETDATE()");

            builder.Property(pc => pc.DataAggiornamento)
                .HasDefaultValueSql("GETDATE()");

            // ✅ INDICI PER PERFORMANCE
            builder.HasIndex(pc => pc.Nome)
                .IsUnique();

            builder.HasIndex(pc => pc.DimensioneBicchiereId);
            builder.HasIndex(pc => pc.GradoDolcezza);
            builder.HasIndex(pc => pc.DataCreazione);

            // ✅ RELAZIONE CON DIMENSIONE BICCHIERE
            builder.HasOne(pc => pc.DimensioneBicchiere)
                .WithMany()
                .HasForeignKey(pc => pc.DimensioneBicchiereId)
                .OnDelete(DeleteBehavior.Restrict);

            // ✅ RELAZIONE CON BEVANDE CUSTOM
            builder.HasMany(pc => pc.BevandaCustom)
                .WithOne(bc => bc.PersCustom)
                .HasForeignKey(bc => bc.PersCustomId)
                .OnDelete(DeleteBehavior.Restrict);

            // ✅ RELAZIONE CON INGREDIENTI PERSONALIZZAZIONE - CORRETTA CON NOME ESATTO
            builder.HasMany(pc => pc.IngredientiPersonalizzazione)
                .WithOne(ip => ip.PersCustom) // ✅ "PersCustom" non "PersonalizzazioneCustom"
                .HasForeignKey(ip => ip.PersCustomId)
                .OnDelete(DeleteBehavior.Cascade);

            // ✅ CHECK CONSTRAINTS
            builder.ToTable(tb =>
            {
                tb.HasCheckConstraint("CK_PersonalizzazioneCustom_GradoDolcezza",
                    "[GradoDolcezza] BETWEEN 0 AND 10");

                tb.HasCheckConstraint("CK_PersonalizzazioneCustom_DateConsistency",
                    "[DataAggiornamento] >= [DataCreazione]");
            });

            // ✅ CONFIGURAZIONE NOME TABELLA
            builder.ToTable("PersonalizzazioneCustom");
        }
    }
}