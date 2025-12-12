using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Database.Models;

namespace Database.Configurations
{
    public class ClienteConfiguration : IEntityTypeConfiguration<Cliente>
    {
        public void Configure(EntityTypeBuilder<Cliente> builder)
        {
            // ✅ CHIAVE PRIMARIA
            builder.HasKey(c => c.ClienteId);

            // ✅ PROPRIETÀ OBBLIGATORIE
            builder.Property(c => c.TavoloId)
                .IsRequired();

            builder.Property(c => c.DataCreazione)
                .IsRequired();

            builder.Property(c => c.DataAggiornamento)
                .IsRequired();

            // ✅ VALORI DEFAULT
            builder.Property(c => c.DataCreazione)
                .HasDefaultValueSql("GETDATE()");

            builder.Property(c => c.DataAggiornamento)
                .HasDefaultValueSql("GETDATE()");

            // ✅ INDICI PER PERFORMANCE
            builder.HasIndex(c => c.TavoloId);
            builder.HasIndex(c => c.DataCreazione);
            builder.HasIndex(c => c.DataAggiornamento);

            // ✅ RELAZIONE CON TAVOLO
            builder.HasOne(c => c.Tavolo)
                .WithMany() // ✅ Assumendo che Tavolo non abbia navigation property per Cliente
                .HasForeignKey(c => c.TavoloId)
                .OnDelete(DeleteBehavior.Restrict); // ✅ Previene eliminazione tavolo con clienti attivi

            // ✅ RELAZIONE CON LOG ACCESSI
            builder.HasMany(c => c.LogAccessi)
                .WithOne(la => la.Cliente)
                .HasForeignKey(la => la.ClienteId)
                .OnDelete(DeleteBehavior.Cascade); // ✅ Elimina log quando cliente viene eliminato

            // ✅ RELAZIONE CON ORDINI
            builder.HasMany(c => c.Ordine)
                .WithOne(o => o.Cliente)
                .HasForeignKey(o => o.ClienteId)
                .OnDelete(DeleteBehavior.Restrict); // ✅ Previene eliminazione cliente con ordini

            // ✅ RELAZIONE CON PREFERITI CLIENTE
            builder.HasMany(c => c.PreferitiCliente)
                .WithOne(pc => pc.Cliente)
                .HasForeignKey(pc => pc.ClienteId)
                .OnDelete(DeleteBehavior.Cascade); // ✅ Elimina preferiti quando cliente viene eliminato

            // ✅ RELAZIONE CON UTENTI
            builder.HasMany(c => c.Utenti)
                .WithOne(u => u.Cliente)
                .HasForeignKey(u => u.ClienteId)
                .OnDelete(DeleteBehavior.Restrict); // ✅ Previene eliminazione cliente con utenti associati

            // ✅ CHECK CONSTRAINTS (se supportati)
            builder.ToTable(tb => tb.HasCheckConstraint(
                "CK_Cliente_DateConsistency",
                "[DataAggiornamento] >= [DataCreazione]"));

            // ✅ CONFIGURAZIONE NOME TABELLA
            builder.ToTable("Cliente");

            // ✅ COMMENTI PER DOCUMENTAZIONE (opzionale)
            // builder.HasComment("Tabella per la gestione dei clienti associati ai tavoli");
        }
    }
}