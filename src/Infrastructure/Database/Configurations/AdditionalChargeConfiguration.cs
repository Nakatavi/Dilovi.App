using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Database.Configurations;

public class AdditionalChargeConfiguration : IEntityTypeConfiguration<AdditionalCharge>
{
    public void Configure(EntityTypeBuilder<AdditionalCharge> builder)
    {
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Description).HasMaxLength(200).IsRequired();
        builder.Property(c => c.Amount).HasPrecision(12, 2);

        builder.HasOne<Apartment>()
            .WithMany()
            .HasForeignKey(c => c.ApartmentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(c => new { c.ApartmentId, c.Year, c.Month });
    }
}
