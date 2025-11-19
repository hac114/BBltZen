using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Database;

namespace Database.Configurations
{
    public class TaxRatesConfiguration : IEntityTypeConfiguration<TaxRates>
    {
        public void Configure(EntityTypeBuilder<TaxRates> builder)
        {
            // ✅ CHIAVE PRIMARIA
            builder.HasKey(t => t.TaxRateId);

            // ✅ PROPRIETÀ OBBLIGATORIE
            builder.Property(t => t.Aliquota)
                .IsRequired()
                .HasColumnType("decimal(5,4)"); // ✅ Formato aliquota (es. 0.2200 per 22%)

            builder.Property(t => t.Descrizione)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(t => t.DataCreazione)
                .IsRequired();

            builder.Property(t => t.DataAggiornamento)
                .IsRequired();

            // ✅ VALORI DEFAULT
            builder.Property(t => t.DataCreazione)
                .HasDefaultValueSql("GETDATE()");

            builder.Property(t => t.DataAggiornamento)
                .HasDefaultValueSql("GETDATE()");

            // ✅ INDICI PER PERFORMANCE
            builder.HasIndex(t => t.Aliquota);
            builder.HasIndex(t => t.Descrizione)
                .IsUnique(); // ✅ Descrizione univoca per aliquota
            builder.HasIndex(t => t.DataCreazione);

            // ✅ RELAZIONE CON ORDER ITEM - CORRETTA CON NOME ESATTO
            builder.HasMany(t => t.OrderItem)
                .WithOne(oi => oi.TaxRate) // ✅ "TaxRate" non "TaxRates"
                .HasForeignKey(oi => oi.TaxRateId)
                .OnDelete(DeleteBehavior.Restrict); // ✅ Previene eliminazione aliquota con ordini

            // ✅ CHECK CONSTRAINTS
            builder.ToTable(tb =>
            {
                tb.HasCheckConstraint("CK_TaxRates_Aliquota",
                    "[Aliquota] >= 0 AND [Aliquota] <= 1"); // ✅ Aliquota tra 0% e 100%

                tb.HasCheckConstraint("CK_TaxRates_DateConsistency",
                    "[DataAggiornamento] >= [DataCreazione]"); // ✅ Consistenza temporale
            });

            // ✅ CONFIGURAZIONE NOME TABELLA
            builder.ToTable("TaxRates");

            // ✅ COMMENTI PER DOCUMENTAZIONE (opzionale)
            // builder.HasComment("Tabella per la gestione delle aliquote IVA");
        }
    }
}