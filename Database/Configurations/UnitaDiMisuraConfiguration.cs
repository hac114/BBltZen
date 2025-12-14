using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BBltZen;

namespace Database.Configurations
{
    public class UnitaDiMisuraConfiguration : IEntityTypeConfiguration<UnitaDiMisura>
    {
        public void Configure(EntityTypeBuilder<UnitaDiMisura> builder)
        {
            // ✅ TABELLA
            builder.ToTable("Unita_di_misura");

            // ✅ CHIAVE PRIMARIA
            builder.HasKey(u => u.UnitaMisuraId)
                .HasName("PK_Unita_di_misura_1");

            builder.Property(u => u.UnitaMisuraId)
                .HasColumnName("unita_misura_id");

            // ✅ SIGLA: CHAR(2) NOT NULL UNIQUE
            builder.Property(u => u.Sigla)
                .IsRequired()
                .HasMaxLength(2)
                .IsFixedLength()
                .HasColumnName("sigla")
                .HasComment("Sigla univoca dell'unità di misura (es: GR, ML, PZ)");

            // ✅ DESCRIZIONE: NVARCHAR(10) NOT NULL UNIQUE  
            builder.Property(u => u.Descrizione)
                .IsRequired()
                .HasMaxLength(10)
                .HasColumnName("descrizione")
                .HasComment("Descrizione estesa dell'unità di misura");

            // ✅ VINCOLI UNIQUE (ESATTI COME NEL DATABASE)
            builder.HasIndex(u => u.Sigla)
                .IsUnique()
                .HasDatabaseName("UQ_Unita_di_misura_sigla");

            builder.HasIndex(u => u.Descrizione)
                .IsUnique()
                .HasDatabaseName("UQ_Unita_di_misura_descrizione");

            // ✅ RELAZIONI (se esistono)
            builder.HasMany(u => u.DimensioneBicchiere)
                .WithOne(d => d.UnitaMisura)
                .HasForeignKey(d => d.UnitaMisuraId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(u => u.PersonalizzazioneIngrediente)
                .WithOne(pi => pi.UnitaMisura)
                .HasForeignKey(pi => pi.UnitaMisuraId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}