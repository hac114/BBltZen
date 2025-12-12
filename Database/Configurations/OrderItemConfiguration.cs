using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Database.Models;

namespace Database.Configurations
{
    public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
    {
        public void Configure(EntityTypeBuilder<OrderItem> builder)
        {
            // ✅ VALORI DEFAULT
            builder.Property(oi => oi.DataCreazione)
                .HasDefaultValueSql("GETDATE()");

            builder.Property(oi => oi.DataAggiornamento)
                .HasDefaultValueSql("GETDATE()");

            builder.Property(oi => oi.TipoArticolo)
                .HasDefaultValue("BS")
                .IsRequired();

            builder.Property(oi => oi.ScontoApplicato)
                .HasDefaultValue(0);

            // ✅ VALIDAZIONI
            builder.Property(oi => oi.Quantita)
                .IsRequired();

            builder.Property(oi => oi.PrezzoUnitario)
                .HasPrecision(18, 2)
                .IsRequired();

            builder.Property(oi => oi.Imponibile)
                .HasPrecision(18, 2)
                .IsRequired();

            // ✅ CHIAVI ESTERNE
            builder.HasOne(oi => oi.Ordine)
                .WithMany(o => o.OrderItem)
                .HasForeignKey(oi => oi.OrdineId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(oi => oi.TaxRate)
                .WithMany()
                .HasForeignKey(oi => oi.TaxRateId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}