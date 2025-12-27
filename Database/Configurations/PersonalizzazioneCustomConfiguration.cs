using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BBltZen;

namespace Database.Configurations
{
    public class PersonalizzazioneCustomConfiguration : IEntityTypeConfiguration<PersonalizzazioneCustom>
    {
        public void Configure(EntityTypeBuilder<PersonalizzazioneCustom> builder)
        {
            // ✅ CHIAVE PRIMARIA
            builder.HasKey(pc => pc.PersCustomId)
                .HasName("PK__PERSONAL__776FA86624F5A943");

            // ✅ PROPRIETÀ OBBLIGATORIE
            builder.Property(pc => pc.Nome)
                .IsRequired()
                .HasMaxLength(100)
                .HasDefaultValue("Bevanda Custom");

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
                // Constraint per grado_dolcezza (1-3) - esistente nel DB
                tb.HasCheckConstraint("CK__PERSONALI__grado__08162EEB",
                    "[grado_dolcezza] >= 1 AND [grado_dolcezza] <= 3");

                // Constraint per consistenza date - ora anche nel DB
                tb.HasCheckConstraint("CK_PERSONALIZZAZIONE_CUSTOM_DateConsistency",
                    "[data_aggiornamento] >= [data_creazione]");
            });

            // ✅ CONFIGURAZIONE NOME TABELLA
            builder.ToTable("PERSONALIZZAZIONE_CUSTOM");
        }        
    }
}