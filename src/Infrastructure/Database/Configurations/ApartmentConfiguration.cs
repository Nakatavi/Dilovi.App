using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Database.Configurations;

public class ApartmentConfiguration : IEntityTypeConfiguration<Apartment>
{
    public void Configure(EntityTypeBuilder<Apartment> builder)
    {
        builder.HasKey(a => a.Id);

        builder.Property(a => a.Number).HasMaxLength(16).IsRequired();
        builder.HasIndex(a => a.Number).IsUnique();

        builder.Property(a => a.Area).HasPrecision(10, 2);
    }
}
