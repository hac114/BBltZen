using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Database.Models;

namespace Database.Configurations
{
    public class StatoOrdineConfiguration : IEntityTypeConfiguration<StatoOrdine>
    {
        public void Configure(EntityTypeBuilder<StatoOrdine> builder)
        {
            // ✅ CHIAVE PRIMARIA
            builder.HasKey(so => so.StatoOrdineId);

            // ✅ PROPRIETÀ OBBLIGATORIE
            builder.Property(so => so.StatoOrdine1)
                .IsRequired()
                .HasMaxLength(50)
                .HasColumnName("StatoOrdine"); // ✅ Nome colonna esplicito

            builder.Property(so => so.Terminale)
                .IsRequired()
                .HasDefaultValue(false); // ✅ Default non terminale

            // ✅ INDICI PER PERFORMANCE
            builder.HasIndex(so => so.StatoOrdine1)
                .IsUnique(); // ✅ Nome stato univoco

            builder.HasIndex(so => so.Terminale); // ✅ Ricerche per stati terminali

            // ✅ RELAZIONE CON CONFIGURAZIONI SOGLIE TEMPI
            builder.HasMany(so => so.ConfigSoglieTempi)
                .WithOne(cst => cst.StatoOrdine)
                .HasForeignKey(cst => cst.StatoOrdineId)
                .OnDelete(DeleteBehavior.Cascade); // ✅ Elimina configurazioni se stato viene eliminato

            // ✅ RELAZIONE CON ORDINI
            builder.HasMany(so => so.Ordine)
                .WithOne(o => o.StatoOrdine)
                .HasForeignKey(o => o.StatoOrdineId)
                .OnDelete(DeleteBehavior.Restrict); // ✅ Previene eliminazione stato con ordini

            // ✅ RELAZIONE CON STATO STORICO ORDINE
            builder.HasMany(so => so.StatoStoricoOrdine)
                .WithOne(sso => sso.StatoOrdine)
                .HasForeignKey(sso => sso.StatoOrdineId)
                .OnDelete(DeleteBehavior.Restrict); // ✅ Previene eliminazione stato con storico

            // ✅ CHECK CONSTRAINTS
            builder.ToTable(tb => tb.HasCheckConstraint(
                "CK_StatoOrdine_StatiValidi",
                "[StatoOrdine] IN ('In_Attesa', 'In_Preparazione', 'Pronto', 'Completato', 'Annullato', 'Consegnato')"));

            // ✅ CONFIGURAZIONE NOME TABELLA
            builder.ToTable("StatoOrdine");

            // ✅ COMMENTI PER DOCUMENTAZIONE (opzionale)
            // builder.HasComment("Tabella per la gestione degli stati del flusso ordini");
        }
    }
}