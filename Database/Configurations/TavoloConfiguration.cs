using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BBltZen;

namespace Database.Configurations
{
    public class TavoloConfiguration : IEntityTypeConfiguration<Tavolo>
    {
        public void Configure(EntityTypeBuilder<Tavolo> builder)
        {
            // ✅ CHIAVE PRIMARIA
            builder.HasKey(t => t.TavoloId);

            // ✅ PROPRIETÀ OBBLIGATORIE
            builder.Property(t => t.Numero)
                .IsRequired();

            builder.Property(t => t.Disponibile)
                .IsRequired()
                .HasDefaultValue(true);

            // ✅ PROPRIETÀ OPZIONALI
            builder.Property(t => t.Zona)
                .HasMaxLength(50)
                .IsRequired(false);

            // ✅ INDICE UNIVOCO PER NUMERO TAVOLO
            builder.HasIndex(t => t.Numero)
                .IsUnique();

            // ✅ INDICI PER PERFORMANCE
            builder.HasIndex(t => t.Disponibile);
            builder.HasIndex(t => t.Zona);

            // ✅ RELAZIONI (se necessario configurare)
            builder.HasMany(t => t.Cliente)
                .WithOne(c => c.Tavolo)
                .HasForeignKey(c => c.TavoloId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(t => t.SessioniQr)
                .WithOne(sq => sq.Tavolo)
                .HasForeignKey(sq => sq.TavoloId)
                .OnDelete(DeleteBehavior.Cascade);

            // ✅ CONFIGURAZIONE NOME TABELLA
            builder.ToTable("Tavolo");
        }
    }
}