using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Database.Models;

namespace Database.Configurations
{
    public class ArticoloConfiguration : IEntityTypeConfiguration<Articolo>
    {
        public void Configure(EntityTypeBuilder<Articolo> builder)
        {
            // ✅ CHIAVE PRIMARIA
            builder.HasKey(a => a.ArticoloId);

            // ✅ PROPRIETÀ OBBLIGATORIE
            builder.Property(a => a.Tipo)
                .IsRequired()
                .HasMaxLength(50)
                .HasConversion(
                    v => v.ToUpper(), // ✅ Salva sempre in maiuscolo
                    v => v
                );

            builder.Property(a => a.DataCreazione)
                .IsRequired();

            builder.Property(a => a.DataAggiornamento)
                .IsRequired();

            // ✅ VALORI DEFAULT
            builder.Property(a => a.DataCreazione)
                .HasDefaultValueSql("GETDATE()");

            builder.Property(a => a.DataAggiornamento)
                .HasDefaultValueSql("GETDATE()");

            // ✅ INDICI PER PERFORMANCE
            builder.HasIndex(a => a.Tipo);
            builder.HasIndex(a => a.DataCreazione);
            builder.HasIndex(a => a.DataAggiornamento);

            // ✅ CORREZIONE: RELAZIONE 1:1 CON BEVANDA CUSTOM
            builder.HasOne(a => a.BevandaCustom)
                .WithOne(bc => bc.Articolo)
                .HasForeignKey<BevandaCustom>(bc => bc.ArticoloId)
                .OnDelete(DeleteBehavior.Cascade);

            // ✅ RELAZIONE 1:1 CON BEVANDA STANDARD
            builder.HasOne(a => a.BevandaStandard)
                .WithOne(bs => bs.Articolo)
                .HasForeignKey<BevandaStandard>(bs => bs.ArticoloId)
                .OnDelete(DeleteBehavior.Cascade);

            // ✅ RELAZIONE 1:1 CON DOLCE
            builder.HasOne(a => a.Dolce)
                .WithOne(d => d.Articolo)
                .HasForeignKey<Dolce>(d => d.ArticoloId)
                .OnDelete(DeleteBehavior.Cascade);

            // ✅ RELAZIONE CON ORDER ITEM
            builder.HasMany(a => a.OrderItem)
                .WithOne(oi => oi.Articolo)
                .HasForeignKey(oi => oi.ArticoloId)
                .OnDelete(DeleteBehavior.Restrict);

            // ✅ CONFIGURAZIONE TABELLA
            builder.ToTable("Articolo");
        }
    }
}