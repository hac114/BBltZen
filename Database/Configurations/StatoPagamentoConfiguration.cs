using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Database;

namespace Database.Configurations
{
    public class StatoPagamentoConfiguration : IEntityTypeConfiguration<StatoPagamento>
    {
        public void Configure(EntityTypeBuilder<StatoPagamento> builder)
        {
            // ✅ CHIAVE PRIMARIA
            builder.HasKey(sp => sp.StatoPagamentoId);

            // ✅ PROPRIETÀ OBBLIGATORIE
            builder.Property(sp => sp.StatoPagamento1)
                .IsRequired()
                .HasMaxLength(50)
                .HasColumnName("StatoPagamento"); // ✅ Nome colonna esplicito

            // ✅ INDICI PER PERFORMANCE
            builder.HasIndex(sp => sp.StatoPagamento1)
                .IsUnique(); // ✅ Nome stato univoco

            // ✅ RELAZIONE CON ORDINI
            builder.HasMany(sp => sp.Ordine)
                .WithOne(o => o.StatoPagamento)
                .HasForeignKey(o => o.StatoPagamentoId)
                .OnDelete(DeleteBehavior.Restrict); // ✅ Previene eliminazione stato con ordini

            // ✅ CHECK CONSTRAINTS
            builder.ToTable(tb => tb.HasCheckConstraint(
                "CK_StatoPagamento_StatiValidi",
                "[StatoPagamento] IN ('In_Attesa', 'Pagato', 'Fallito', 'Rimborsato', 'Annullato')"));

            // ✅ CONFIGURAZIONE NOME TABELLA
            builder.ToTable("StatoPagamento");

            // ✅ COMMENTI PER DOCUMENTAZIONE (opzionale)
            // builder.HasComment("Tabella per la gestione degli stati di pagamento degli ordini");
        }
    }
}