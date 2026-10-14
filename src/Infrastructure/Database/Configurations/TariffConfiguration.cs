using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Database.Configurations;

public class TariffConfiguration : IEntityTypeConfiguration<Tariff>
{
    public void Configure(EntityTypeBuilder<Tariff> builder)
    {
        builder.HasKey(t => t.Id);

        builder.Property(t => t.ServiceType).HasConversion<string>().HasMaxLength(32);
        builder.Property(t => t.ChargeBasis).HasConversion<string>().HasMaxLength(32);
        builder.Property(t => t.PricePerUnit).HasPrecision(12, 4);

        builder.HasIndex(t => new { t.ServiceType, t.ValidFrom }).IsUnique();
    }
}
