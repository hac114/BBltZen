using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BBltZen;

namespace Database.Configurations
{
    public class StatoOrdineConfiguration : IEntityTypeConfiguration<StatoOrdine>
    {
        public void Configure(EntityTypeBuilder<StatoOrdine> builder)
        {
            // ✅ DEFINIZIONE TABELLA e CHECK CONSTRAINT (Sintassi aggiornata)
            // Il check constraint viene configurato all'interno della lambda di ToTable
            builder.ToTable(tb =>
            {
                tb.HasCheckConstraint(
                    "CK_terminale_solo_0_1",
                    "([terminale] = 1 OR [terminale] = 0)"
                );
            });

            // Chiave primaria
            builder.HasKey(so => so.StatoOrdineId);

            // Proprietà StatoOrdine1 (nome colonna: "StatoOrdine")
            builder.Property(so => so.StatoOrdine1)
                .IsRequired()
                .HasMaxLength(100)
                .HasColumnName("StatoOrdine");

            // ✅ VINCOLO UNIQUE sul valore dello stato
            builder.HasIndex(so => so.StatoOrdine1)
                .IsUnique()
                .HasDatabaseName("UQ_stato_ordine_valore");

            // ✅ Proprietà Terminale - Mappatura corretta per BIT SQL -> bool C#
            builder.Property(so => so.Terminale)
                .IsRequired()
                .HasDefaultValue(false)
                .HasConversion<int>(); // Conversione opzionale per chiarezza

            // ✅ RELAZIONE UNO-A-ZERO/UNO con ConfigSoglieTempi
            // StatoOrdine può avere 0 o 1 ConfigSoglieTempi
            // ConfigSoglieTempi deve avere esattamente 1 StatoOrdine
            builder.HasOne(so => so.ConfigSoglieTempi)
                .WithOne(cst => cst.StatoOrdine)
                .HasForeignKey<ConfigSoglieTempi>(cst => cst.StatoOrdineId)
                .OnDelete(DeleteBehavior.Cascade);

            // ✅ RELAZIONE UNO-A-MOLTI con Ordine
            builder.HasMany(so => so.Ordine)
                .WithOne(o => o.StatoOrdine)
                .HasForeignKey(o => o.StatoOrdineId)
                .OnDelete(DeleteBehavior.Restrict);

            // ✅ RELAZIONE UNO-A-MOLTI con StatoStoricoOrdine
            builder.HasMany(so => so.StatoStoricoOrdine)
                .WithOne(sso => sso.StatoOrdine)
                .HasForeignKey(sso => sso.StatoOrdineId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}