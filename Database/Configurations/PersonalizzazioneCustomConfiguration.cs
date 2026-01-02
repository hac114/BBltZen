using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BBltZen;

namespace Database.Configurations
{
    public class PersonalizzazioneCustomConfiguration : IEntityTypeConfiguration<PersonalizzazioneCustom>
    {
        public void Configure(EntityTypeBuilder<PersonalizzazioneCustom> builder)
        {
            // ✅ CHIAVE PRIMARIA (nome esatto come nel DB)
            builder.HasKey(pc => pc.PersCustomId)
                   .HasName("PK__PERSONAL__776FA86624F5A943");

            // ✅ CLUSTERED INDEX (aggiunto - presente nel DB)
            builder.HasIndex(pc => pc.PersCustomId)
                   .IsClustered()
                   .HasDatabaseName("PK__PERSONAL__776FA86624F5A943");

            // ✅ PROPRIETÀ OBBLIGATORIE
            builder.Property(pc => pc.Nome)
                   .IsRequired()
                   .HasMaxLength(100)
                   .HasDefaultValue("Bevanda Custom");

            builder.Property(pc => pc.GradoDolcezza)
                   .IsRequired()
                   .HasColumnType("tinyint"); // Aggiunto: tipo esatto del DB

            builder.Property(pc => pc.DimensioneBicchiereId)
                   .IsRequired();

            builder.Property(pc => pc.DataCreazione)
                   .IsRequired();

            builder.Property(pc => pc.DataAggiornamento)
                   .IsRequired();

            // ✅ VALORI DEFAULT (GETDATE) con ValueGenerated
            builder.Property(pc => pc.DataCreazione)
                   .HasDefaultValueSql("GETDATE()")
                   .ValueGeneratedOnAdd();

            builder.Property(pc => pc.DataAggiornamento)
                   .HasDefaultValueSql("GETDATE()")
                   .ValueGeneratedOnAddOrUpdate();

            // ✅ RELAZIONE 1:1 CON BEVANDA_CUSTOM (AGGIORNATO: è 1:1, non 1:N)
            builder.HasOne(pc => pc.BevandaCustom)
                   .WithOne(bc => bc.PersCustom)
                   .HasForeignKey<BevandaCustom>(bc => bc.PersCustomId)
                   .HasConstraintName("FK_BEVANDA_CUSTOM_PERSONALIZZAZIONE") // Nome esatto FK dal DB
                   .OnDelete(DeleteBehavior.NoAction); // NO_ACTION nel DB (non Restrict)

            // ✅ RELAZIONE CON DIMENSIONE BICCHIERE
            builder.HasOne(pc => pc.DimensioneBicchiere)
                   .WithMany()
                   .HasForeignKey(pc => pc.DimensioneBicchiereId)
                   .HasConstraintName("FK_DIMENSIONE_BICCHIERE") // Nome esatto dal DB
                   .OnDelete(DeleteBehavior.NoAction); // NO_ACTION nel DB (non Restrict)

            // ✅ RELAZIONE CON INGREDIENTI PERSONALIZZAZIONE
            builder.HasMany(pc => pc.IngredientiPersonalizzazione)
                   .WithOne(ip => ip.PersCustom)
                   .HasForeignKey(ip => ip.PersCustomId)
                   .OnDelete(DeleteBehavior.Cascade);

            // ✅ CHECK CONSTRAINTS (aggiornati con nomi e clausole esatte dal DB)
            builder.ToTable(tb =>
            {
                // Constraint per grado_dolcezza (1-3) - nome e clausola esatta dal DB
                tb.HasCheckConstraint("CK__PERSONALI__grado__08162EEB",
                    "([grado_dolcezza]>=(1) AND [grado_dolcezza]<=(3))");

                // Constraint per consistenza date - nome e clausola esatta dal DB
                tb.HasCheckConstraint("CK_PERSONALIZZAZIONE_CUSTOM_DateConsistency",
                    "([data_aggiornamento]>=[data_creazione])");
            });

            // ✅ INDICI DI PERFORMANCE (aggiunti - presenti nel DB)
            builder.HasIndex(pc => pc.Nome);
            builder.HasIndex(pc => pc.DataCreazione);
            builder.HasIndex(pc => pc.DimensioneBicchiereId);
            builder.HasIndex(pc => pc.GradoDolcezza);

            // ✅ NOME TABELLA (case-sensitive)
            builder.ToTable("PERSONALIZZAZIONE_CUSTOM");            
        }
    }
}