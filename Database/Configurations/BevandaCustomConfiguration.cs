using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BBltZen;

namespace Database.Configurations
{
    public class BevandaCustomConfiguration : IEntityTypeConfiguration<BevandaCustom>
    {
        public void Configure(EntityTypeBuilder<BevandaCustom> builder)
        {            
            // ✅ CHIAVE PRIMARIA & INDICE CLUSTERED            
            builder.HasKey(bc => bc.ArticoloId)
                   .HasName("PK_BevandaCustom_ARTICOLO");

            builder.HasIndex(bc => bc.ArticoloId)
                   .IsClustered()
                   .HasDatabaseName("PK_BevandaCustom_ARTICOLO");
            
            // ✅ PROPRIETÀ CONFIGURATION           
            builder.Property(bc => bc.ArticoloId)
                   .IsRequired()
                   .ValueGeneratedNever(); // Valore assegnato dalla tabella ARTICOLO

            builder.Property(bc => bc.PersCustomId)
                   .IsRequired();

            // Prezzo: decimal(4,2) con precisione/escala esatta
            builder.Property(bc => bc.Prezzo)
                   .IsRequired()
                   .HasPrecision(4, 2)
                   .HasColumnType("decimal(4,2)");

            builder.Property(bc => bc.DataCreazione)
                   .IsRequired();

            builder.Property(bc => bc.DataAggiornamento)
                   .IsRequired();

            // ✅ VALORI DEFAULT (GETDATE)           
            builder.Property(bc => bc.DataCreazione)
                   .HasDefaultValueSql("GETDATE()")
                   .ValueGeneratedOnAdd();

            builder.Property(bc => bc.DataAggiornamento)
                   .HasDefaultValueSql("GETDATE()")
                   .ValueGeneratedOnAddOrUpdate();
            
            // ✅ INDICI DI PERFORMANCE (ALLINEATI AL DB)            
            // 1. Indice su DataCreazione
            builder.HasIndex(bc => bc.DataCreazione)
                   .HasDatabaseName("IX_BEVANDA_CUSTOM_DATA_CREAZIONE")
                   .IsClustered(false);

            // 2. Indice su Prezzo
            builder.HasIndex(bc => bc.Prezzo)
                   .HasDatabaseName("IX_BEVANDA_CUSTOM_PREZZO")
                   .IsClustered(false);

            // 3. UNIQUE CONSTRAINT su PersCustomId (già creato nel DB)
            // NOTA: Questo crea un indice UNIQUE in EF Core, ma nel DB abbiamo già il constraint
            builder.HasIndex(bc => bc.PersCustomId)
                   .IsUnique()
                   .HasDatabaseName("UQ_BEVANDA_CUSTOM_PERS_CUSTOM_ID");

            
            // ✅ RELAZIONI E VINCOLI (AGGIORNATO)            
            // Relazione 1:1 con ARTICOLO (NO_ACTION)
            builder.HasOne(bc => bc.Articolo)
                   .WithOne(a => a.BevandaCustom)
                   .HasForeignKey<BevandaCustom>(bc => bc.ArticoloId)
                   .HasConstraintName("FK_BEVANDA_CUSTOM_ARTICOLO")
                   .OnDelete(DeleteBehavior.NoAction);

            // ✅ CORREZIONE: Relazione 1:1 con PERSONALIZZAZIONE_CUSTOM (NO_ACTION)
            // Con WithOne perché PersonalizzazioneCustom ha una singola BevandaCustom
            builder.HasOne(bc => bc.PersCustom)
                   .WithOne(pc => pc.BevandaCustom) // Relazione 1:1 bidirezionale
                   .HasForeignKey<BevandaCustom>(bc => bc.PersCustomId)
                   .HasConstraintName("FK_BEVANDA_CUSTOM_PERSONALIZZAZIONE")
                   .OnDelete(DeleteBehavior.NoAction);

            
            // ✅ CHECK CONSTRAINTS (ESATTAMENTE come nel DB)           
            builder.ToTable(tb =>
            {
                tb.HasCheckConstraint("CK_BC_PREZZO", "[Prezzo] >= (0)");
                tb.HasCheckConstraint("CK_BC_DATA", "[DataAggiornamento] >= [DataCreazione]");
            });
            
            // ✅ NOME TABELLA (CASE-SENSITIVE)            
            builder.ToTable("BEVANDA_CUSTOM");            
        }
    }
}
