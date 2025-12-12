using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Database.Models;

namespace Database.Configurations
{
    public class BevandaCustomConfiguration : IEntityTypeConfiguration<BevandaCustom>
    {
        public void Configure(EntityTypeBuilder<BevandaCustom> builder)
        {
            // ✅ CHIAVE PRIMARIA (ArticoloId come PK)
            builder.HasKey(bc => bc.ArticoloId);

            // ✅ PROPRIETÀ OBBLIGATORIE
            builder.Property(bc => bc.ArticoloId)
                .IsRequired();

            builder.Property(bc => bc.PersCustomId)
                .IsRequired();

            builder.Property(bc => bc.Prezzo)
                .IsRequired()
                .HasColumnType("decimal(10,2)");

            builder.Property(bc => bc.DataCreazione)
                .IsRequired();

            builder.Property(bc => bc.DataAggiornamento)
                .IsRequired();

            // ✅ VALORI DEFAULT
            builder.Property(bc => bc.DataCreazione)
                .HasDefaultValueSql("GETDATE()");

            builder.Property(bc => bc.DataAggiornamento)
                .HasDefaultValueSql("GETDATE()");

            // ✅ INDICI PER PERFORMANCE
            builder.HasIndex(bc => bc.PersCustomId);
            builder.HasIndex(bc => bc.DataCreazione);
            builder.HasIndex(bc => bc.Prezzo);

            // ✅ INDICE UNIVOCO
            builder.HasIndex(bc => new { bc.ArticoloId, bc.PersCustomId })
                .IsUnique();

            // ✅ RELAZIONE 1:1 CON ARTICOLO
            builder.HasOne(bc => bc.Articolo)
                .WithOne()
                .HasForeignKey<BevandaCustom>(bc => bc.ArticoloId)
                .OnDelete(DeleteBehavior.Cascade);

            // ✅ RELAZIONE CON PERSONALIZZAZIONE CUSTOM
            builder.HasOne(bc => bc.PersCustom)
                .WithMany()
                .HasForeignKey(bc => bc.PersCustomId)
                .OnDelete(DeleteBehavior.Restrict);

            // ✅ CHECK CONSTRAINTS
            builder.ToTable(tb =>
            {
                tb.HasCheckConstraint("CK_BevandaCustom_Prezzo",
                    "[Prezzo] >= 0 AND [Prezzo] <= 50");
                tb.HasCheckConstraint("CK_BevandaCustom_DateConsistency",
                    "[DataAggiornamento] >= [DataCreazione]");
            });

            builder.ToTable("BevandaCustom");
        }
    }
}