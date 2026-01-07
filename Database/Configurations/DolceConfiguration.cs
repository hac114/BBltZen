using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BBltZen;

namespace Database.Configurations
{
    public class DolceConfiguration : IEntityTypeConfiguration<Dolce>
    {
        public void Configure(EntityTypeBuilder<Dolce> builder)
        {
            builder.HasKey(d => d.ArticoloId);

            builder.HasOne(d => d.Articolo)
                .WithOne()
                .HasForeignKey<Dolce>(d => d.ArticoloId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Property(d => d.Nome).IsRequired().HasMaxLength(100);
            builder.Property(d => d.Prezzo).IsRequired().HasColumnType("decimal(4,2)").HasPrecision(4, 2);
            builder.Property(d => d.Disponibile).IsRequired().HasDefaultValue(true);
            builder.Property(d => d.Priorita).IsRequired().HasDefaultValue(1);
            builder.Property(d => d.DataCreazione).IsRequired().HasDefaultValueSql("GETDATE()");
            builder.Property(d => d.DataAggiornamento).IsRequired().HasDefaultValueSql("GETDATE()");
            builder.Property(d => d.Descrizione).HasMaxLength(255);
            builder.Property(d => d.ImmagineUrl).HasMaxLength(500);

            // Indici
            builder.HasIndex(d => d.Nome).IsUnique();
            builder.HasIndex(d => d.Disponibile);
            builder.HasIndex(d => d.Priorita);
            builder.HasIndex(d => d.Prezzo);
            builder.HasIndex(d => d.DataCreazione);

            // Vincoli CHECK (solo quelli presenti nel DB)
            builder.ToTable(tb =>
            {
                tb.HasCheckConstraint("CHK_Dolce_Prezzo", "[Prezzo] >= 0");
                tb.HasCheckConstraint("CHK_Dolce_Priorita_Range", "[Priorita] >= 1 AND [Priorita] <= 10");
                tb.HasCheckConstraint("CHK_Dolce_Date", "[DataAggiornamento] >= [DataCreazione]");
            });

            builder.ToTable("DOLCE");

            // ✅ COMMENTI PER DOCUMENTAZIONE (opzionale)
            // builder.HasComment("Tabella per la gestione dei dolci nel menu");
        }
    }
}