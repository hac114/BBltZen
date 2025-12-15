using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BBltZen;

namespace Database.Configurations
{
    public class StatoPagamentoConfiguration : IEntityTypeConfiguration<StatoPagamento>
    {
        public void Configure(EntityTypeBuilder<StatoPagamento> builder)
        {
            // ✅ CHIAVE PRIMARIA
            builder.HasKey(sp => sp.StatoPagamentoId);

            // ✅ PROPRIETÀ OBBLIGATORIE - ALLINEATE AL DB
            builder.Property(sp => sp.StatoPagamento1)
                .IsRequired()
                .HasMaxLength(100) // ✅ Aggiornato a 100 (come nel DB: nvarchar(100))
                .HasColumnName("stato_pagamento"); // ✅ Nome colonna corretto (minuscolo)

            // ✅ INDICE UNIVOCO (vincolo UNIQUE già aggiunto al DB)
            builder.HasIndex(sp => sp.StatoPagamento1)
                .IsUnique()
                .HasDatabaseName("UQ_STATO_PAGAMENTO_stato_pagamento"); // ✅ Nome esplicito

            // ✅ RELAZIONE CON ORDINI
            builder.HasMany(sp => sp.Ordine)
                .WithOne(o => o.StatoPagamento)
                .HasForeignKey(o => o.StatoPagamentoId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK__ORDINE__stato_pa__6E2152BE"); // ✅ Nome FK esistente

            // ✅ CHECK CONSTRAINT - ALLINEATO AL DB
            // NOTA: Il check constraint esiste già nel DB con nome "CHK_STATO_PAGAMENTO_valori_ammessi"
            // Non serve ricrearlo, ma possiamo documentarlo
            // builder.ToTable(tb => tb.HasCheckConstraint(
            //     "CHK_STATO_PAGAMENTO_valori_ammessi",
            //     "[stato_pagamento] IN ('pendente', 'completato', 'fallito', 'rimborsato', 'non richiesto')"));

            builder.ToTable("STATO_PAGAMENTO", t => t.HasComment("Tabella degli stati di pagamento. Valori ammessi: pendente, completato, fallito, rimborsato, non richiesto"));
        }
    }
}