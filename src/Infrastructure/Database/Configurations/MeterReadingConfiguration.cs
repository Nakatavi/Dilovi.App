using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Database.Configurations;

public class MeterReadingConfiguration : IEntityTypeConfiguration<MeterReading>
{
    public void Configure(EntityTypeBuilder<MeterReading> builder)
    {
        builder.HasKey(r => r.Id);

        builder.Property(r => r.MeterType).HasConversion<string>().HasMaxLength(32);
        builder.Property(r => r.Value).HasPrecision(12, 3);

        builder.HasOne<Apartment>()
            .WithMany()
            .HasForeignKey(r => r.ApartmentId)
            .OnDelete(DeleteBehavior.Cascade);

        // One regular reading and one new-meter reading per meter and day
        builder.HasIndex(r => new { r.ApartmentId, r.MeterType, r.ReadingDate, r.IsNewMeter }).IsUnique();
    }
}
