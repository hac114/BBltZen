using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Database.Models;

namespace Database.Configurations
{
    public class OrdineConfiguration : IEntityTypeConfiguration<Ordine>
    {
        public void Configure(EntityTypeBuilder<Ordine> builder)
        {
            // ✅ CHIAVE PRIMARIA
            builder.HasKey(o => o.OrdineId);

            // ✅ PROPRIETÀ OBBLIGATORIE
            builder.Property(o => o.ClienteId)
                .IsRequired();

            builder.Property(o => o.DataCreazione)
                .IsRequired();

            builder.Property(o => o.DataAggiornamento)
                .IsRequired();

            builder.Property(o => o.StatoOrdineId)
                .IsRequired();

            builder.Property(o => o.StatoPagamentoId)
                .IsRequired();

            builder.Property(o => o.Totale)
                .IsRequired()
                .HasColumnType("decimal(10,2)"); // ✅ Formato totale

            builder.Property(o => o.Priorita)
                .IsRequired();

            // ✅ PROPRIETÀ OPZIONALI
            builder.Property(o => o.SessioneId)
                .IsRequired(false);

            // ✅ VALORI DEFAULT
            builder.Property(o => o.DataCreazione)
                .HasDefaultValueSql("GETDATE()");

            builder.Property(o => o.DataAggiornamento)
                .HasDefaultValueSql("GETDATE()");

            builder.Property(o => o.Priorita)
                .HasDefaultValue(1); // ✅ Priorità default

            // ✅ INDICI PER PERFORMANCE
            builder.HasIndex(o => o.ClienteId);
            builder.HasIndex(o => o.StatoOrdineId);
            builder.HasIndex(o => o.StatoPagamentoId);
            builder.HasIndex(o => o.SessioneId);
            builder.HasIndex(o => o.DataCreazione);
            builder.HasIndex(o => o.DataAggiornamento);
            builder.HasIndex(o => o.Priorita);
            builder.HasIndex(o => o.Totale);

            // ✅ INDICE COMPOSTO PER RICERCHE FREQUENTI
            builder.HasIndex(o => new { o.StatoOrdineId, o.DataCreazione });
            builder.HasIndex(o => new { o.StatoPagamentoId, o.DataCreazione });

            // ✅ RELAZIONE CON CLIENTE
            builder.HasOne(o => o.Cliente)
                .WithMany(c => c.Ordine)
                .HasForeignKey(o => o.ClienteId)
                .OnDelete(DeleteBehavior.Restrict); // ✅ Previene eliminazione cliente con ordini

            // ✅ RELAZIONE CON ORDER ITEM
            builder.HasMany(o => o.OrderItem)
                .WithOne(oi => oi.Ordine)
                .HasForeignKey(oi => oi.OrdineId)
                .OnDelete(DeleteBehavior.Cascade); // ✅ Elimina items quando ordine viene eliminato

            // ✅ RELAZIONE CON SESSIONE QR
            builder.HasOne(o => o.Sessione)
                .WithMany(s => s.Ordine)
                .HasForeignKey(o => o.SessioneId)
                .OnDelete(DeleteBehavior.SetNull); // ✅ Mantiene ordine se sessione viene eliminata

            // ✅ RELAZIONE CON STATO ORDINE
            builder.HasOne(o => o.StatoOrdine)
                .WithMany(so => so.Ordine)
                .HasForeignKey(o => o.StatoOrdineId)
                .OnDelete(DeleteBehavior.Restrict); // ✅ Previene eliminazione stato con ordini

            // ✅ RELAZIONE CON STATO PAGAMENTO
            builder.HasOne(o => o.StatoPagamento)
                .WithMany(sp => sp.Ordine)
                .HasForeignKey(o => o.StatoPagamentoId)
                .OnDelete(DeleteBehavior.Restrict); // ✅ Previene eliminazione stato pagamento con ordini

            // ✅ RELAZIONE CON STATO STORICO ORDINE
            builder.HasMany(o => o.StatoStoricoOrdine)
                .WithOne(sso => sso.Ordine)
                .HasForeignKey(sso => sso.OrdineId)
                .OnDelete(DeleteBehavior.Cascade); // ✅ Elimina storico quando ordine viene eliminato

            // ✅ CHECK CONSTRAINTS
            builder.ToTable(tb =>
            {
                tb.HasCheckConstraint("CK_Ordine_Totale",
                    "[Totale] >= 0 AND [Totale] <= 1000"); // ✅ Totale tra 0 e 1000 euro

                tb.HasCheckConstraint("CK_Ordine_Priorita",
                    "[Priorita] BETWEEN 1 AND 10"); // ✅ Priorità tra 1 e 10

                tb.HasCheckConstraint("CK_Ordine_DateConsistency",
                    "[DataAggiornamento] >= [DataCreazione]"); // ✅ Consistenza temporale
            });

            // ✅ CONFIGURAZIONE NOME TABELLA
            builder.ToTable("Ordine");

            // ✅ COMMENTI PER DOCUMENTAZIONE (opzionale)
            // builder.HasComment("Tabella per la gestione degli ordini del sistema");
        }
    }
}