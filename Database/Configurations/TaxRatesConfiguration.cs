using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Database.Configurations
{
    public class TaxRatesConfiguration : IEntityTypeConfiguration<TaxRates>
    {
        public void Configure(EntityTypeBuilder<TaxRates> builder)
        {
            builder.HasKey(t => t.TaxRateId);

            builder.Property(t => t.Aliquota)
                .IsRequired()
                .HasColumnType("decimal(5,2)"); // ✅ CORRETTO: 22.00, 10.00, 4.00

            builder.Property(t => t.Descrizione)
                .IsRequired()
                .HasMaxLength(100);

            // ✅ RIMUOVI: builder.HasIndex(t => t.Descrizione).IsUnique();

            builder.HasIndex(t => new { t.Aliquota, t.Descrizione })
                .IsUnique(); // ✅ VINCOLO UNIQUE COMPOSTO

            builder.Property(t => t.DataCreazione)
                .HasDefaultValueSql("GETDATE()");

            builder.Property(t => t.DataAggiornamento)
                .HasDefaultValueSql("GETDATE()");

            builder.HasMany(t => t.OrderItem)
                .WithOne(oi => oi.TaxRate)
                .HasForeignKey(oi => oi.TaxRateId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.ToTable("TaxRates");
        }
    }
}