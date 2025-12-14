using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BBltZen;

namespace Database.Configurations
{
    public class StatoOrdineConfiguration : IEntityTypeConfiguration<StatoOrdine>
    {
        public void Configure(EntityTypeBuilder<StatoOrdine> builder)
        {
            builder.ToTable("StatoOrdine");

            // Chiave primaria
            builder.HasKey(so => so.StatoOrdineId);

            // Proprietà StatoOrdine1 (nome colonna: "StatoOrdine")
            builder.Property(so => so.StatoOrdine1)
                .IsRequired()
                .HasMaxLength(100)
                .HasColumnName("StatoOrdine");

            // ✅ VINCOLO UNIQUE (ORA ATTIVO - RIMUOVI I COMMENTI)
            builder.HasIndex(so => so.StatoOrdine1)
                .IsUnique()
                .HasDatabaseName("UQ_stato_ordine_valore"); // Nome esatto del DB

            // Proprietà Terminale
            builder.Property(so => so.Terminale)
                .IsRequired()
                .HasDefaultValue(false);

            // Vincolo CHECK presente nel database
            builder.ToTable(tb => tb.HasCheckConstraint(
                "CK_terminale_solo_0_1",
                "([terminale]=(1) OR [terminale]=(0))"));

            // Relazioni
            builder.HasMany(so => so.ConfigSoglieTempi)
                .WithOne(cst => cst.StatoOrdine)
                .HasForeignKey(cst => cst.StatoOrdineId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(so => so.Ordine)
                .WithOne(o => o.StatoOrdine)
                .HasForeignKey(o => o.StatoOrdineId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(so => so.StatoStoricoOrdine)
                .WithOne(sso => sso.StatoOrdine)
                .HasForeignKey(sso => sso.StatoOrdineId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}